﻿/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          PosTaggerErrorRpt\Program.cs
 *  Version:       1.0
 *  Desc:		   POS tagger error report generator
 *  Author:		   Miha Grcar
 *  Created on:    Nov-2009
 *  Last modified: Nov-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;
using Latino;
using Latino.Model;
using MetaTagger;

namespace PosTagger
{
    class Program
    {
        static void Train(int cut_off, int num_iter, int num_threads, Corpus corpus, ref SuffixTrie suffix_trie, ref Dictionary<string, int> feature_space,
            ref MaximumEntropyClassifier<string> model, string model_file_name)
        {
            try
            {
                suffix_trie = new SuffixTrie();
                foreach (TaggedWord word in corpus.TaggedWords)
                {
                    suffix_trie.AddWordTagPair(word.WordLower, word.Tag);
                }
                suffix_trie.PropagateTags();
                model = new MaximumEntropyClassifier<string>();
                Dataset<string, BinaryVector<int>.ReadOnly> dataset = new Dataset<string, BinaryVector<int>.ReadOnly>();
                feature_space = new Dictionary<string, int>();
                Utils.Verbose("Generating feature vectors ...\r\n");
                for (int i = 0; i < corpus.TaggedWords.Count; i++)
                {
                    Utils.Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                    BinaryVector<int> feature_vector = corpus.GenerateFeatureVector(i, feature_space, /*extend_feature_space=*/true, suffix_trie);
                    dataset.Add(corpus.TaggedWords[i].Tag, feature_vector);
                }
                Utils.Verbose("\r\n");
                Utils.Verbose("Training ...\r\n");
                DateTime start_time = DateTime.Now;
                model.CutOff = cut_off;
                model.NumThreads = num_threads;
                model.NumIter = num_iter;
                model.Train(dataset);
                TimeSpan span = DateTime.Now - start_time;
                Utils.Verbose("Training time: {0:00}:{1:00}:{2:00}.{3:000}.\r\n", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                Utils.Verbose("Writing model ...\r\n");
                BinarySerializer writer = new BinarySerializer(model_file_name, FileMode.Create);
                suffix_trie.Save(writer);
                Utils.SaveDictionary(feature_space, writer);
                model.Save(writer);
                writer.Close();
                Utils.Verbose("Done.\r\n");
            }
            catch (Exception exception)
            {
                Console.WriteLine("*** Training: Unexpected error. Details: {0}\r\n{1}", exception, exception.StackTrace);   
            }
        }

        static ClassifierResult<string> TrimAndNormalize(ClassifierResult<string> result, int max_size, Set<string> filter)
        {
            ClassifierResult<string> filtered_result = result;
            if (filter != null)
            {
                ArrayList<KeyDat<double, string>> tmp = new ArrayList<KeyDat<double, string>>();
                foreach (KeyDat<double, string> item in result)
                {
                    if (filter.Contains(item.Dat)) { tmp.Add(item); }
                }
                if (tmp.Count > 0) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ce se to zgodi, bi moral uporabiti najbolj pogost tag iz leksikona 
                { 
                    filtered_result = new ClassifierResult<string>(tmp); 
                }
            }
            ArrayList<KeyDat<double, string>> aux = new ArrayList<KeyDat<double, string>>();
            int size = Math.Min(max_size, filtered_result.Count);
            double sum = 0;
            for (int i = 0; i < size; i++)
            {
                sum += filtered_result[i].Key;
            }           
            for (int i = 0; i < size; i++)
            {
                if (filtered_result[i].Key > 0)
                {
                    if (!m_nonword_regex.Match(filtered_result[i].Dat).Success)
                    {
                        aux.Add(new KeyDat<double, string>(filtered_result[i].Key / sum, filtered_result[i].Dat));
                    }
                    else
                    {
                        size = Math.Min(size + 1, filtered_result.Count);
                    }
                }
            }
            return new ClassifierResult<string>(aux);
        }

        static bool Contains(ClassifierResult<string> cfy_result, string tag, int n)
        {
            int m = Math.Min(n, cfy_result.Count);
            for (int i = 0; i < m; i++)
            {
                if (cfy_result[i].Dat == tag) { return true; }
            }
            return false;
        }

        static string GetErrorClass(string correct_tag, string predicted_tag, ref string sub_class)
        {
            if (correct_tag[0] != predicted_tag[0])
            {
                sub_class = string.Format("{0} instead of {1}", predicted_tag[0], correct_tag[0]);
                return sub_class;
            }
            sub_class = string.Format("{0} instead of {1}", predicted_tag, correct_tag);
            if (correct_tag.Length > predicted_tag.Length)
            {
                predicted_tag += new string('_', correct_tag.Length - predicted_tag.Length);
            }
            else if (predicted_tag.Length > correct_tag.Length)
            {
                correct_tag += new string('_', predicted_tag.Length - correct_tag.Length);
            }
            string mask = "";
            for (int i = 0; i < predicted_tag.Length; i++)
            {
                if (predicted_tag[i] != correct_tag[i])
                {
                    mask += "*";
                }
                else
                {
                    mask += predicted_tag[i];
                }
            }
            return mask;
        }

        class SentenceInfo
        {
            public int StartIdx;
            public int EndIdx
                = -1;
            public int ErrIdx;
            public Corpus Corpus;
            public SuffixTrie SuffixTrie;
        }

        static Dictionary<string, Dictionary<string, ArrayList<SentenceInfo>>> m_error_rpt
            = new Dictionary<string, Dictionary<string, ArrayList<SentenceInfo>>>();

        static int m_num_errors
            = 0;

        static Regex m_nonword_regex 
            = new Regex("^\\W+(\\<eos\\>)?$", RegexOptions.Compiled);

        static Regex m_slo_regex 
            = new Regex("[šŠđĐžŽćĆčČ]", RegexOptions.Compiled);    

        static string FixUnicode(string str)
        {
            return m_slo_regex.Replace(str, "_");
        }

        static void Tag(SuffixTrie suffix_trie, Dictionary<string, int> feature_space, MaximumEntropyClassifier<string> model, Corpus corpus, StreamWriter writer, Dictionary<string, ArrayList<string>> lex)
        {
            try
            {
                const int MAX_TOP = 10;
                DateTime start_time = DateTime.Now;
                Utils.Verbose("Tagging ...\r\n");
                int known_words_correct = 0;
                int known_words_pos_correct = 0;
                int[] known_words_top_n_correct = new int[MAX_TOP - 1];
                int[] unknown_words_top_n_correct = new int[MAX_TOP - 1];
                int known_words = 0;
                int unknown_words_correct = 0;
                int unknown_words_pos_correct = 0;
                int unknown_words = 0;
                int eos_count = 0;
                int eos_correct = 0;
                string[] gold_std = new string[corpus.TaggedWords.Count];                
                for (int i = 0; i < corpus.TaggedWords.Count; i++)
                {                    
                    Utils.Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                    gold_std[i] = corpus.TaggedWords[i].Tag;
                    BinaryVector<int> feature_vector = corpus.GenerateFeatureVector(i, feature_space, /*extend_feature_space=*/false, suffix_trie);
                    ClassifierResult<string> result = model.Classify(feature_vector);
                    if (m_nonword_regex.Match(corpus.TaggedWords[i].WordLower).Success)
                    {
                        foreach (KeyDat<double, string> item in result)
                        {
                            if (corpus.TaggedWords[i].Word == item.Dat || corpus.TaggedWords[i].Word + "<eos>" == item.Dat)
                            {
                                corpus.TaggedWords[i].ClassifierResult = new ClassifierResult<string>(new KeyDat<double, string>[] { new KeyDat<double, string>(1, item.Dat) });
                                corpus.TaggedWords[i].Tag = item.Dat;
                                break;
                            }
                        }
                        if (corpus.TaggedWords[i].ClassifierResult == null)
                        {
                            corpus.TaggedWords[i].ClassifierResult = new ClassifierResult<string>(new KeyDat<double, string>[] { new KeyDat<double, string>(1, corpus.TaggedWords[i].Word) });
                            corpus.TaggedWords[i].Tag = corpus.TaggedWords[i].Word;
                        }
                    }
                    else
                    {
                        Set<string> filter = lex.ContainsKey(corpus.TaggedWords[i].WordLower) ? new Set<string>(lex[corpus.TaggedWords[i].WordLower]) : null;
                        if (suffix_trie.Contains(corpus.TaggedWords[i].WordLower)) { filter = null; }
                        corpus.TaggedWords[i].ClassifierResult = TrimAndNormalize(result, MAX_TOP, filter);
                        corpus.TaggedWords[i].Tag = corpus.TaggedWords[i].ClassifierResult.BestClassLabel;
                    }
                }
                Utils.Verbose("\r\n");
                TimeSpan span = DateTime.Now - start_time;
                //Utils.Verbose("Tagging time: {0:00}:{1:00}:{2:00}.{3:000}.\r\n", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                double ms_per_tag = span.TotalMilliseconds / (double)corpus.TaggedWords.Count;
                Utils.Verbose("Tagging speed: {0:0.0000} ms per tag.\r\n", ms_per_tag);
                if (writer != null) { writer.WriteLine("Tagging speed: {0:0.0000} ms per tag.", ms_per_tag); }
                // restore tags
                for (int i = 0; i < corpus.TaggedWords.Count; i++)  
                {
                    corpus.TaggedWords[i].Tag = gold_std[i];
                }
                // collect error report data
                int eos_idx = 0;
                ArrayList<SentenceInfo> need_end_idx = new ArrayList<SentenceInfo>();
                for (int i = 0; i < corpus.TaggedWords.Count; i++)
                {
                    string word_lower = corpus.TaggedWords[i].WordLower;
                    string predicted_tag = corpus.TaggedWords[i].ClassifierResult.BestClassLabel;
                    bool is_known = suffix_trie.Contains(word_lower);
                    if (predicted_tag == corpus.TaggedWords[i].Tag)
                    {
                        if (is_known) { known_words_correct++; }
                        else { unknown_words_correct++; }
                    }
                    else // wrong prediction
                    {
                        string sub_class = null;
                        string error_class = GetErrorClass(corpus.TaggedWords[i].Tag, predicted_tag, ref sub_class);
                        if (!m_error_rpt.ContainsKey(error_class)) { m_error_rpt.Add(error_class, new Dictionary<string, ArrayList<SentenceInfo>>()); }
                        if (!m_error_rpt[error_class].ContainsKey(sub_class)) { m_error_rpt[error_class].Add(sub_class, new ArrayList<SentenceInfo>()); }
                        ArrayList<SentenceInfo> error_sentence_info = m_error_rpt[error_class][sub_class];
                        error_sentence_info.Add(new SentenceInfo());                        
                        error_sentence_info.Last.StartIdx = eos_idx;
                        error_sentence_info.Last.ErrIdx = i;
                        error_sentence_info.Last.Corpus = corpus;
                        error_sentence_info.Last.SuffixTrie = suffix_trie;
                        need_end_idx.Add(error_sentence_info.Last);
                    }
                    if (predicted_tag[0] == corpus.TaggedWords[i].Tag[0])
                    {
                        if (is_known) { known_words_pos_correct++; }
                        else { unknown_words_pos_correct++; }
                    }
                    if (is_known) { known_words++; }
                    else { unknown_words++; }
                    if (corpus.TaggedWords[i].MoreInfo.EndOfSentence)
                    {
                        foreach (SentenceInfo sentence_info in need_end_idx)
                        {
                            sentence_info.EndIdx = i + 1;
                        }
                        need_end_idx.Clear();
                        eos_idx = i + 1;
                        eos_count++;
                        if (predicted_tag.EndsWith("<eos>")) { eos_correct++; }
                    }
                    for (int j = 2; j <= MAX_TOP; j++)
                    {
                        if (Contains(corpus.TaggedWords[i].ClassifierResult, corpus.TaggedWords[i].Tag, j))
                        {
                            if (is_known) { known_words_top_n_correct[j - 2]++; }
                            else { unknown_words_top_n_correct[j - 2]++; }
                        }
                    }
                }
                #region Do I need to do this?
                foreach (SentenceInfo sentence_info in need_end_idx)
                {
                    sentence_info.EndIdx = corpus.TaggedWords.Count;
                }
                #endregion                
                int all_words = known_words + unknown_words;
                int all_words_correct = known_words_correct + unknown_words_correct;
                int all_words_pos_correct = known_words_pos_correct + unknown_words_pos_correct;
                Utils.Verbose("Accuracy on known words: ......... {2:0.00}% ({0} / {1})\r\n", known_words_correct, known_words,
                    (double)known_words_correct / (double)known_words * 100.0);
                Utils.Verbose("Accuracy on unknown words: ....... {2:0.00}% ({0} / {1})\r\n", unknown_words_correct, unknown_words,
                    (double)unknown_words_correct / (double)unknown_words * 100.0);
                Utils.Verbose("Overall accuracy: ................ {2:0.00}% ({0} / {1})\r\n", all_words_correct, all_words,
                    (double)all_words_correct / (double)all_words * 100.0);
                for (int j = 0; j <= MAX_TOP - 2; j++)
                {
                    Utils.Verbose("Accuracy on known words (top {0}):   {3:0.00}% ({1} / {2})\r\n", j + 2, known_words_top_n_correct[j], known_words,
                        (double)known_words_top_n_correct[j] / (double)known_words * 100.0);
                    Utils.Verbose("Accuracy on unknown words (top {0}): {3:0.00}% ({1} / {2})\r\n", j + 2, unknown_words_top_n_correct[j], unknown_words,
                        (double)unknown_words_top_n_correct[j] / (double)unknown_words * 100.0);
                    Utils.Verbose("Overall accuracy (top {0}): ........ {3:0.00}% ({1} / {2})\r\n", j + 2, known_words_top_n_correct[j] + unknown_words_top_n_correct[j],
                        all_words, (double)(known_words_top_n_correct[j] + unknown_words_top_n_correct[j]) / (double)all_words * 100.0);
                }
                Utils.Verbose("Accuracy on known words (POS): ... {2:0.00}% ({0} / {1})\r\n", known_words_pos_correct, known_words,
                    (double)known_words_pos_correct / (double)known_words * 100.0);
                Utils.Verbose("Accuracy on unknown words (POS):   {2:0.00}% ({0} / {1})\r\n", unknown_words_pos_correct, unknown_words,
                    (double)unknown_words_pos_correct / (double)unknown_words * 100.0);
                Utils.Verbose("Overall accuracy (POS): .......... {2:0.00}% ({0} / {1})\r\n", all_words_pos_correct, all_words,
                    (double)all_words_pos_correct / (double)all_words * 100.0);
                Utils.Verbose("End-of-sentence accuracy: ........ {2:0.00}% ({0} / {1})\r\n", eos_correct, eos_count,
                    (double)eos_correct / (double)eos_count * 100.0);
                #region Write eval results
                if (writer != null)
                {
                    writer.Write("Accuracy on known words: ......... {2:0.00}% ({0} / {1})\r\n", known_words_correct, known_words,
                        (double)known_words_correct / (double)known_words * 100.0);
                    writer.Write("Accuracy on unknown words: ....... {2:0.00}% ({0} / {1})\r\n", unknown_words_correct, unknown_words,
                        (double)unknown_words_correct / (double)unknown_words * 100.0);
                    writer.Write("Overall accuracy: ................ {2:0.00}% ({0} / {1})\r\n", all_words_correct, all_words,
                        (double)all_words_correct / (double)all_words * 100.0);
                    for (int j = 0; j <= MAX_TOP - 2; j++)
                    {
                        writer.Write("Accuracy on known words (top {0}):   {3:0.00}% ({1} / {2})\r\n", j + 2, known_words_top_n_correct[j], known_words,
                            (double)known_words_top_n_correct[j] / (double)known_words * 100.0);
                        writer.Write("Accuracy on unknown words (top {0}): {3:0.00}% ({1} / {2})\r\n", j + 2, unknown_words_top_n_correct[j], unknown_words,
                            (double)unknown_words_top_n_correct[j] / (double)unknown_words * 100.0);
                        writer.Write("Overall accuracy (top {0}): ........ {3:0.00}% ({1} / {2})\r\n", j + 2, known_words_top_n_correct[j] + unknown_words_top_n_correct[j],
                            all_words, (double)(known_words_top_n_correct[j] + unknown_words_top_n_correct[j]) / (double)all_words * 100.0);
                    }
                    writer.Write("Accuracy on known words (POS): ... {2:0.00}% ({0} / {1})\r\n", known_words_pos_correct, known_words,
                        (double)known_words_pos_correct / (double)known_words * 100.0);
                    writer.Write("Accuracy on unknown words (POS):   {2:0.00}% ({0} / {1})\r\n", unknown_words_pos_correct, unknown_words,
                        (double)unknown_words_pos_correct / (double)unknown_words * 100.0);
                    writer.Write("Overall accuracy (POS): .......... {2:0.00}% ({0} / {1})\r\n", all_words_pos_correct, all_words,
                        (double)all_words_pos_correct / (double)all_words * 100.0);
                    writer.Write("End-of-sentence accuracy: ........ {2:0.00}% ({0} / {1})\r\n", eos_correct, eos_count,
                        (double)eos_correct / (double)eos_count * 100.0);
                }
                #endregion
                m_num_errors += all_words - all_words_correct; 
            }
            catch (Exception exception)
            {
                Console.WriteLine("*** Tagging: Unexpected error. Details: {0}\r\n{1}", exception, exception.StackTrace);
            }
        }

        static void Main()
        {
            const int CUT_OFF = 2;
            const int NUM_ITER = 100;
            const int NUM_THREADS = 3;
            const int NUM_FOLDS = 5;
            string data_path = "..\\..\\..\\data\\".TrimEnd('\\');
            // load lexicon
            //string[] lex_lines = File.ReadAllLines(string.Format("{0}\\krekMSD_lexicon_word_lemma_tag.tsv", data_path));
            //Dictionary<string, ArrayList<string>> lex = new Dictionary<string, ArrayList<string>>();
            //foreach (string lex_line in lex_lines)
            //{
            //    string[] lex_row = lex_line.Split('\t');
            //    string word_form = lex_row[0].ToLower();
            //    if (!lex.ContainsKey(word_form))
            //    {
            //        lex.Add(word_form, new ArrayList<string>(new string[] { lex_row[2] }));
            //    }
            //    else
            //    {
            //        lex[word_form].Add(lex_row[2]);
            //    }
            //}
            // load corpus
            Corpus corpus = new Corpus();
            SuffixTrie suffix_trie = null;            
            StreamWriter writer = new StreamWriter(string.Format("{0}\\eval_report.txt", data_path));
#if DEBUG
            corpus.LoadFromXml(string.Format("{0}\\jos100k-test.xml", data_path), /*tag_len=*/int.MaxValue);
#else
            corpus.LoadFromXml(string.Format("{0}\\jos100k-original.xml", data_path), /*tag_len=*/int.MaxValue);
#endif
            Corpus[] folds = corpus.Split(NUM_FOLDS, /*rnd=*/null);
            ArrayList<Pair<Corpus, SuffixTrie>> boost_data = new ArrayList<Pair<Corpus, SuffixTrie>>();
            for (int i = 0; i < NUM_FOLDS; i++)
            {
                Utils.Verbose("*** Fold {0} / {1} ***\r\n", i + 1, NUM_FOLDS);
                if (writer != null) { writer.WriteLine("*** Fold {0} / {1} ***", i + 1, NUM_FOLDS); }
                Corpus test_set = folds[i];
                Corpus train_set = new Corpus();
                foreach (Corpus fold in folds)
                {
                    if (fold != test_set) { train_set.AddCorpus(fold); }
                }
                Dictionary<string, int> feature_space = null;
                MaximumEntropyClassifier<string> model = null;
                string model_file_name = string.Format("{0}\\model_fold{1}.bin", data_path, i + 1);
                if (Utils.VerifyFileNameOpen(model_file_name))
                {
                    // load model
                    BinarySerializer reader = new BinarySerializer(model_file_name, FileMode.Open);
                    suffix_trie = new SuffixTrie(reader);
                    feature_space = Utils.LoadDictionary<string, int>(reader);
                    model = new MaximumEntropyClassifier<string>(reader);
                    reader.Close();
                }
                else
                {
                    // training
                    Train(CUT_OFF, NUM_ITER, NUM_THREADS, train_set, ref suffix_trie, ref feature_space, ref model, model_file_name);
                }
                // tagging
                Tag(suffix_trie, feature_space, model, test_set, writer, new Dictionary<string, ArrayList<string>>());
                boost_data.Add(new Pair<Corpus, SuffixTrie>(test_set, suffix_trie));
#if DEBUG
                break;
#endif
            }
            writer.Close();
            // output error report
            Utils.VerboseLine("Outputting error report ...");
            StreamWriter _writer = new StreamWriter(string.Format("{0}\\error_report.html", data_path));
            _writer.WriteLine("<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head><body>");
            ArrayList<KeyDat<int, string>> tmp = new ArrayList<KeyDat<int, string>>();
            foreach (KeyValuePair<string, Dictionary<string, ArrayList<SentenceInfo>>> item in m_error_rpt)
            {
                int count = 0;
                foreach (KeyValuePair<string, ArrayList<SentenceInfo>> item_inner in item.Value)
                {
                    count += item_inner.Value.Count;
                }
                tmp.Add(new KeyDat<int, string>(count, item.Key));
            }
            tmp.Sort(new DescSort<KeyDat<int, string>>());
            int file_id = 1;
            foreach (KeyDat<int, string> item in tmp)
            {
                double perc = (double)item.Key / (double)m_num_errors * 100.0;
                _writer.WriteLine("<h1><a href='error_rpt_{0}.html'>{1} ({2:0.00}% of errors)</a></h1>", file_id, item.Dat, perc);
                /*StreamWriter*/ writer = new StreamWriter(string.Format("{0}\\error_rpt_{1}.html", data_path, file_id));
                writer.WriteLine("<html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'></head><body>");
                writer.WriteLine(string.Format("<h1>{0} ({1:0.00}% of errors)</h1>", HttpUtility.HtmlEncode(item.Dat), perc));
                ArrayList<KeyDat<int, string>> aux = new ArrayList<KeyDat<int, string>>();
                foreach (KeyValuePair<string, ArrayList<SentenceInfo>> item_inner in m_error_rpt[item.Dat])
                {
                    aux.Add(new KeyDat<int, string>(item_inner.Value.Count, item_inner.Key));
                }
                aux.Sort(new DescSort<KeyDat<int, string>>());
                int part_id = 1;
                foreach (KeyDat<int, string> item_inner in aux)
                {
                    perc = (double)item_inner.Key / (double)m_num_errors * 100.0;
                    _writer.WriteLine("<a href='error_rpt_{0}.html#part_{1}'>{2} ({3:0.00}%)</a>{4}", file_id, part_id, HttpUtility.HtmlEncode(item_inner.Dat), perc, part_id == aux.Count ? "" : " | ");
                    writer.WriteLine("<h2 id='part_{0}'>{1} ({2:0.00}% of errors)</h2>", part_id, HttpUtility.HtmlEncode(item_inner.Dat), perc);
                    ArrayList<SentenceInfo> sentence_info_list = m_error_rpt[item.Dat][item_inner.Dat];
                    writer.WriteLine("<ul>");
                    foreach (SentenceInfo sentence_info in sentence_info_list)
                    {
                        writer.WriteLine("<li style='margin-bottom: 5px'>");
                        for (int i = sentence_info.StartIdx; i < sentence_info.EndIdx; i++)
                        {
                            if (sentence_info.SuffixTrie.Contains(sentence_info.Corpus.TaggedWords[i].WordLower))
                            {
                                writer.Write("{0}", sentence_info.Corpus.TaggedWords[i].Word);
                            }
                            else
                            {
                                writer.Write("<font style='background-color: #FFE8E8'>{0}</font>", sentence_info.Corpus.TaggedWords[i].Word);
                            }
                            if (i != sentence_info.ErrIdx)
                            {
                                if (sentence_info.Corpus.TaggedWords[i].ClassifierResult.BestClassLabel == sentence_info.Corpus.TaggedWords[i].Tag)
                                {
                                    writer.Write("<font style='color: gray'>/{0}</font> ", HttpUtility.HtmlEncode(sentence_info.Corpus.TaggedWords[i].Tag));
                                }
                                else
                                {
                                    writer.Write("<font style='color: orange'><font style='color: gray'>/</font><span title='{0} instead of {1}'>{0}</span></font> ",
                                        HttpUtility.HtmlEncode(sentence_info.Corpus.TaggedWords[i].ClassifierResult.BestClassLabel), HttpUtility.HtmlEncode(sentence_info.Corpus.TaggedWords[i].Tag));
                                }
                            }
                            else
                            {
                                writer.Write("<font style='color: gray'>/</font><font style='color: red'><span title='{0} instead of {1}'>{0}</span></font> ",
                                    HttpUtility.HtmlEncode(sentence_info.Corpus.TaggedWords[i].ClassifierResult.BestClassLabel), HttpUtility.HtmlEncode(sentence_info.Corpus.TaggedWords[i].Tag));
                            }
                        }
                        writer.WriteLine("</li>");
                        //writer.WriteLine("<br/>");
                    }
                    writer.WriteLine("</ul>");
                    part_id++;
                }
                writer.WriteLine("</body></html>");
                writer.Close();
                file_id++;
            }
            _writer.WriteLine("</body></html>");
            _writer.Close();

            StreamWriter cross_tagged = new StreamWriter("c:\\cross_tagged.tbl");
            SparseVector<string> yyy = new SparseVector<string>();
            int fold_num = 1;
            foreach (Pair<Corpus, SuffixTrie> item in boost_data)
            {
                int idx = fold_num;
                foreach (TaggedWord word in item.First.TaggedWords)
                {
                    if (yyy.ContainsAt(idx))
                    {
                        yyy[idx] += "\t" + string.Format("{0} {1}", word.Word, word.ClassifierResult.BestClassLabel); 
                    }
                    else
                    {
                        yyy[idx] = string.Format("{0} {1}", word.Word, word.ClassifierResult.BestClassLabel); 
                    }
                    if (word.MoreInfo.EndOfSentence)
                    {
                        idx += NUM_FOLDS;
                    }
                }
                fold_num++;
            }
            foreach (IdxDat<string> stc_tags in yyy)
            {
                string[] words = stc_tags.Dat.Split('\t');
                foreach (string word in words)
                {
                    cross_tagged.WriteLine(word.Replace(" ", "\t"));
                }
            }
            cross_tagged.Close();
            
            return;

            // create dataset for boosting
            Utils.VerboseLine("Creating training set for boosting ...");
            _writer = new StreamWriter(string.Format("{0}\\boost_ds.tsv", data_path));
            MetaTaggerData.LoadAttributes(string.Format("{0}\\krekMSD-canon-sl-for-error-correction.tbl", data_path));
            string[] templates = new string[] { "Prev_POS_1", "POS_1", "Next_POS_1", "Prev_POS_2", "POS_2", "Next_POS_2", "Prev_{0}_1", "Prev_{0}_2", "{0}_1", "{0}_2", "Next_{0}_1", "Next_{0}_2" };
            string missing_val = "0";
            ArrayList<string> attr_list = new ArrayList<string>(MetaTaggerData.AttrSet);
            _writer.Write("Class\t");
            //_writer.Write("IsLastWord\tEndsWithO\tSwapProbability\tIsKnownWord\tInListOfAdverbs\tIsTrainingSetAdjective\t");
            _writer.Write("SwapProbability\tIsKnownWord\tSwapType\ttSwapType1\t");
            foreach (string _attr in attr_list)
            {
                foreach (string template in templates)
                {
                    string attr = string.Format(template, _attr);
                    _writer.Write("{0}\t", FixUnicode(attr));
                }
            }
            _writer.WriteLine();
            _writer.Write("discrete\t");
            //_writer.Write("discrete\tdiscrete\tcontinuous\tdiscrete\tdiscrete\tdiscrete\t");
            _writer.Write("continuous\tdiscrete\tdiscrete\tdiscrete\t");
            foreach (string _attr in attr_list)
            {
                foreach (string template in templates)
                {
                    string attr = string.Format(template, _attr);
                    _writer.Write("discrete\t", attr);
                }
            }
            _writer.WriteLine();
            _writer.WriteLine("class");
            Random rnd = new Random(1);
            foreach (Pair<Corpus, SuffixTrie> item in boost_data)
            {
                MetaTaggerData.Items.Clear();
                for (int idx = 0; idx < item.First.TaggedWords.Count; idx++)
                {
                    TaggedWord word = item.First.TaggedWords[idx];
                    if (m_nonword_regex.Match(word.Tag).Success)                    
                    {
                        // *** accessing private member in the following lines
                        if (!MetaTaggerData.m_attr.ContainsKey(word.Tag))
                        {
                            MetaTaggerData.m_attr.Add(word.Tag, new Dictionary<string, string>()); 
                        }
                        if (!MetaTaggerData.m_attr.ContainsKey(word.ClassifierResult.BestClassLabel))
                        {
                            MetaTaggerData.m_attr.Add(word.ClassifierResult.BestClassLabel, new Dictionary<string, string>());
                        }
                        if (word.ClassifierResult.Count >= 2 && !MetaTaggerData.m_attr.ContainsKey(word.ClassifierResult[1].Dat))
                        {
                            MetaTaggerData.m_attr.Add(word.ClassifierResult[1].Dat, new Dictionary<string, string>());
                        }
                    }
                    MetaTaggerData.Items.Add(new MetaTaggerData.DataEntry(word.Word, word.Tag,
                        word.ClassifierResult.BestClassLabel, word.ClassifierResult.Count >= 2 ? word.ClassifierResult[1].Dat : word.ClassifierResult.BestClassLabel));
                }
                for (int idx = 0; idx < MetaTaggerData.Items.Count; idx++)
                {
                    if ((MetaTaggerData.Items[idx].GoldTag == MetaTaggerData.Items[idx].Tag1 || MetaTaggerData.Items[idx].GoldTag == MetaTaggerData.Items[idx].Tag2) &&
                        !m_nonword_regex.Match(MetaTaggerData.Items[idx].Tag1).Success && !m_nonword_regex.Match(MetaTaggerData.Items[idx].Tag2).Success &&
                        item.First.TaggedWords[idx].ClassifierResult.Count > 1 && rnd.NextDouble() > 0.5)
                    //if (MetaTaggerData.Items[idx].Tag1[0] == 'R') // R instead of P?
                    {
                        TaggedWord word = item.First.TaggedWords[idx];
                        //_writer.Write("{0}\t", MetaTaggerData.Items[idx].GoldTag[0] == 'P' ? "P" : "R");   
                        _writer.Write("{0}\t", MetaTaggerData.Items[idx].GoldTag == MetaTaggerData.Items[idx].Tag1 ? "KEEP" : "SWAP");                        
                        //_writer.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t", 
                        //    IsLastWord(item.First.TaggedWords, idx), 
                        //    EndsWithO(word),
                        //    SwapProbability(word).ToString(CultureInfo.InvariantCulture),
                        //    IsKnownWord(word, item.Second),
                        //    InListOfAdverbs(word, data_path),
                        //    IsTrainingSetAdjective(word, item.Second));
                        _writer.Write("{0}\t{1}\t{2}\t{3}\t",
                            SwapProbability(word).ToString(CultureInfo.InvariantCulture),
                            IsKnownWord(word, item.Second),
                            SwapType(word),
                            SwapType1(word));
                        Dictionary<string, string> vec = new Dictionary<string, string>();
                        foreach (KeyDat<string, string> vec_item in MetaTaggerData.CreateExample(idx))
                        {
                            //Console.WriteLine("adding {0}", vec_item.Key);
                            vec.Add(vec_item.Key, vec_item.Dat);
                        }
                        foreach (string _attr in attr_list)
                        {
                            foreach (string template in templates)
                            {
                                string attr = string.Format(template, _attr);
                                if (vec.ContainsKey(attr))
                                {
                                    _writer.Write("{0}\t", FixUnicode(vec[attr]));
                                }
                                else
                                {
                                    _writer.Write("{0}\t", missing_val);
                                }
                            }
                        }
                        _writer.WriteLine();
                    }
                }
            }
            _writer.Close();
            Utils.VerboseLine("Done.");
        }

        // *** Linguistic features for boosting ***

        static bool IsLastWord(ArrayList<TaggedWord>.ReadOnly words, int idx)
        {
            return idx < words.Count - 1 && words[idx + 1].ClassifierResult.BestClassLabel.EndsWith("<eos>");
        }

        static bool EndsWithO(TaggedWord word)
        {
            return word.WordLower.EndsWith("o");
        }

        static double SwapProbability(TaggedWord word)
        {
            if (word.ClassifierResult.Count < 2) { return 0; }
            double sum = word.ClassifierResult[0].Key + word.ClassifierResult[1].Key;
            return 1.0 - word.ClassifierResult[0].Key / sum;
        }

        static string SwapType(TaggedWord word)
        {
            return string.Format("{0}->{1}", word.ClassifierResult[0].Dat, word.ClassifierResult[1].Dat);
        }

        static string SwapType1(TaggedWord word)
        {
            return string.Format("{0}->{1}", word.ClassifierResult[0].Dat[0], word.ClassifierResult[1].Dat[0]);
        }

        static bool IsKnownWord(TaggedWord word, SuffixTrie suffix_trie)
        {
            return suffix_trie.Contains(word.WordLower);
        }

        static Set<string> m_adverb_list = null;
        static bool InListOfAdverbs(TaggedWord word, string data_path)
        {             
            if (m_adverb_list == null)
            {
                m_adverb_list = new Set<string>(File.ReadAllLines(string.Format("{0}\\prisl.txt", data_path)));
            }
            return m_adverb_list.Contains(word.WordLower);
        }

        static bool IsTrainingSetAdjective(TaggedWord word, SuffixTrie suffix_trie)
        {
            return suffix_trie.Contains(word.WordLower) && suffix_trie.GetAmbiguityClass(word.WordLower).Contains("P");
        }
    }
}

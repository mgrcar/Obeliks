/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          PosTaggerTag\Program.cs
 *  Version:       1.0
 *  Desc:		   POS tagger tagging utility
 *  Author:		   Miha Grcar
 *  Created on:    Sep-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using Latino;
using Latino.Model;
using LemmaSharp;

namespace PosTagger
{
    class Program
    {
        static bool m_verbose
            = false;

        static void OutputHelp()
        {
            Console.WriteLine("*** Oblikoslovni označevalnik 1.0 - Modul za označevanje ***");
            Console.WriteLine();
            Console.WriteLine("Uporaba:");
            Console.WriteLine("PosTaggerTag [<nastavitve>] <besedilo> <model_bin> <označeni_korpus_xml>"); 
            Console.WriteLine();
            Console.WriteLine("<nastavitve>:     Glej spodaj.");
            Console.WriteLine("<besedilo>:       Besedilo za označevanje (vhod).");
            Console.WriteLine("<model_bin>:      Model za označevanje (vhod).");
            Console.WriteLine("<označeni_korpus_xml>:"); 
            Console.WriteLine("                  Označeni korpus v formatu XML-TEI (izhod).");
            Console.WriteLine();
            Console.WriteLine("Nastavitve:");
            Console.WriteLine("-v                Izpisovanje na zaslon (verbose).");
            Console.WriteLine("                  (privzeto: ni izpisovanja)");
            Console.WriteLine("-xml              Vhodno besedilo je v formatu XML-TEI (evalvacija).");
            Console.WriteLine("                  (privzeto: vhodno besedilo je v običajni tekstovni obliki)");
            Console.WriteLine("-lem:ime_datoteke Model za lematizacijo.");
            Console.WriteLine("                  (privzeto: lematizacija se ne izvede)");            
        }

        static bool ParseParams(string[] args, ref bool verbose, ref bool eval_mode, ref string corpus_file, ref string tagging_model_file, ref string lemmatizer_model_file, ref string tagged_corpus_file)
        {
            // parse
            for (int i = 0; i < args.Length - 3; i++)
            {
                string arg_lwr = args[i].ToLower();
                if (arg_lwr == "-v")
                {
                    verbose = true;
                }
                else if (arg_lwr == "-xml")
                {
                    eval_mode = true;
                }
                else if (arg_lwr.StartsWith("-lem:"))
                {
                    lemmatizer_model_file = args[i].Substring(5, args[i].Length - 5);
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            corpus_file = args[args.Length - 3];
            tagging_model_file = args[args.Length - 2];
            tagged_corpus_file = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpus_file))
            {
                Console.WriteLine("*** Napačno ime datoteke učnega korpusa ali datoteka ne obstaja ({0}).\r\n", corpus_file);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(tagging_model_file))
            {
                Console.WriteLine("*** Napačno ime datoteke modela za označevanje ali datoteka ne obstaja ({0}).\r\n", tagging_model_file);
                OutputHelp();
                return false;
            }
            if (lemmatizer_model_file != null && !Utils.VerifyFileNameOpen(lemmatizer_model_file))
            {
                Console.WriteLine("*** Napačno ime datoteke modela za lematizacijo ali datoteka ne obstaja ({0}).\r\n", lemmatizer_model_file);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(tagged_corpus_file))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", tagged_corpus_file);
                OutputHelp();
                return false;
            }
            return true;
        }

        static string FixWordCase(string lemma, ArrayList<TaggedWord>.ReadOnly words, int i)
        {
            if (TaggerUtils.IsCapitalLetterWord(words[i].Word) && i > 0 && !words[i - 1].Tag.EndsWith("<eos>") &&
                (words[i - 1].Word != "\"") && (words[i - 1].Word != "»") && (words[i - 1].Word != ">>") &&
                (words[i - 1].Word != "«") && (words[i - 1].Word != "<<"))
            {
                return TaggerUtils.SetWordCaseType(lemma, TaggerUtils.WordCaseType.CapitalLetter);
            }
            else
            {
                return lemma;
            }
        }

        static ClassifierResult<string> TrimAndNormalize(ClassifierResult<string> result, int max_size)
        {
            ArrayList<KeyDat<double, string>> aux = new ArrayList<KeyDat<double, string>>();
            int size = Math.Min(max_size, result.Count);
            double sum = 0;
            for (int i = 0; i < size; i++)
            {                
                sum += result[i].Key;
            }
            if (sum == 0) { sum = 1; }
            for (int i = 0; i < size; i++)
            {
                if (result[i].Key > 0)
                {
                    aux.Add(new KeyDat<double, string>(result[i].Key / sum, result[i].Dat));
                }
            }
            return new ClassifierResult<string>(aux);
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 4)
                {
                    OutputHelp();
                }
                else
                {
                    bool eval_mode = false;
                    string corpus_file = null, tagger_model_file = null, lemmatizer_model_file = null, tagged_corpus_file = null;
                    if (ParseParams(args, ref m_verbose, ref eval_mode, ref corpus_file, ref tagger_model_file, ref lemmatizer_model_file, ref tagged_corpus_file))
                    {
                        Utils.Verbose("Nalagam model za označevanje ...\r\n");
                        BinarySerializer reader = new BinarySerializer(tagger_model_file, FileMode.Open);
                        SuffixTrie suffix_trie = new SuffixTrie(reader);
                        Dictionary<string, int> feature_space = Utils.LoadDictionary<string, int>(reader);
                        MaximumEntropyClassifier<string> model = new MaximumEntropyClassifier<string>(reader);
                        reader.Close();
                        Lemmatizer lemmatizer = null;
                        bool attach_tags = false;
                        if (lemmatizer_model_file != null)
                        {
                            Utils.Verbose("Nalagam model za lematizacijo ...\r\n");
                            reader = new BinarySerializer(lemmatizer_model_file, FileMode.Open);
                            attach_tags = reader.ReadBool();
                            lemmatizer = new Lemmatizer(reader);
                            reader.Close();
                        }
                        Utils.Verbose("Nalagam besedilo ...\r\n");
                        Corpus corpus = new Corpus();
                        if (eval_mode)
                        {
                            corpus.LoadFromXml(corpus_file, /*tag_len=*/-1);
                        }
                        else
                        {
                            corpus.LoadFromText(File.ReadAllText(corpus_file));
                        }
                        //StreamWriter _writer = new StreamWriter("c:\\jos100k_test.txt");
                        //_writer.WriteLine(corpus.ToString());
                        //_writer.Close();
                        DateTime start_time = DateTime.Now;
                        Utils.Verbose("Označujem besedilo ...\r\n");
                        int known_words_correct = 0;
                        int known_words_pos_correct = 0;
                        int known_words = 0;
                        int unknown_words_correct = 0;
                        int unknown_words_pos_correct = 0;
                        int unknown_words = 0;
                        int lemma_correct = 0;
                        int lemma_correct_lowercase = 0;
                        int eos_count = 0;
                        int eos_correct = 0;
                        int lemma_words = 0;
                        string[] gold_tags = new string[corpus.TaggedWords.Count];
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            gold_tags[i] = corpus.TaggedWords[i].Tag;
                        }
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            Utils.Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                            const int SEARCH_BEAM_SIZE = 1;
                            ArrayList<string> best_seq = Viterbi.TagSentence(i, delegate(int word_idx, ArrayList<string>.ReadOnly tag_seq) 
                                {
                                    if (word_idx >= corpus.TaggedWords.Count) { return null; }
                                    string word_lower = corpus.TaggedWords[word_idx].WordLower;         
                                    for (int j = tag_seq.Count - 1, k = 1; j >= 0; j--, k++)
                                    { 
                                        corpus.TaggedWords[word_idx - k].Tag = tag_seq[j];
                                    }
                                    BinaryVector<int> feature_vector = corpus.GenerateFeatureVector(word_idx, feature_space, /*extend_feature_space=*/false, suffix_trie);
                                    return TrimAndNormalize(model.Classify(feature_vector), SEARCH_BEAM_SIZE); // TODO: filter according to suffix trie!
                                });       
                            // set best tags and lemmatize
                            foreach (string tag in best_seq)
                            {
                                string word_lower = corpus.TaggedWords[i].WordLower;
                                corpus.TaggedWords[i].Tag = tag;
                                // lemmatize                                
                                if (tag != word_lower)
                                {
                                    string lemma = "";
                                    if (lemmatizer != null)
                                    {                                        
                                        lemma = lemmatizer.Lemmatize(attach_tags ? string.Format("{0}-{1}", word_lower, tag[0]) : word_lower);
                                        lemma = FixWordCase(lemma, corpus.TaggedWords, i); // case-fixing heuristics                                    
                                    }
                                    if (eval_mode && !string.IsNullOrEmpty(lemma))
                                    {
                                        lemma_words++;
                                        if (lemma == corpus.TaggedWords[i].Lemma)
                                        {
                                            lemma_correct++;
                                        }
                                        if (corpus.TaggedWords[i].Lemma != null && lemma.ToLower() == corpus.TaggedWords[i].Lemma.ToLower())
                                        {
                                            lemma_correct_lowercase++;
                                        }
                                    }
                                    corpus.TaggedWords[i].Lemma = lemma;
                                }
                                i++;
                            }                            
                            i--;                            
                        }
                        Utils.Verbose("\r\n");
                        TimeSpan span = DateTime.Now - start_time;
                        Utils.Verbose("Trajanje označevanja: {0:00}:{1:00}:{2:00}.{3:000}.\r\n", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                        Utils.Verbose("Zapisujem označeno besedilo ...\r\n");
                        StreamWriter writer = new StreamWriter(tagged_corpus_file);
                        writer.Write(corpus.ToString("XML"));
                        writer.Close();
                        Utils.Verbose("Končano.\r\n");
                        if (eval_mode)
                        {
                            for (int i = 0; i < corpus.TaggedWords.Count; i++)
                            {
                                string word_lower = corpus.TaggedWords[i].WordLower;
                                string tag = corpus.TaggedWords[i].Tag;
                                bool is_known = suffix_trie.Contains(word_lower);                             
                                if (tag == gold_tags[i])
                                {
                                    if (is_known) { known_words_correct++; }
                                    else { unknown_words_correct++; }
                                }
                                if (tag[0] == gold_tags[i][0])
                                {
                                    if (is_known) { known_words_pos_correct++; }
                                    else { unknown_words_pos_correct++; }
                                }
                                if (is_known) { known_words++; }
                                else { unknown_words++; }
                                if (corpus.TaggedWords[i].MoreInfo.EndOfSentence)
                                {
                                    eos_count++;
                                    if (tag.EndsWith("<eos>")) { eos_correct++; }
                                }
                            }
                            int all_words = known_words + unknown_words;
                            int all_words_correct = known_words_correct + unknown_words_correct;
                            int all_words_pos_correct = known_words_pos_correct + unknown_words_pos_correct;
                            Utils.Verbose("Točnost na znanih besedah: ....... {2:0.00}% ({0} / {1})\r\n", known_words_correct, known_words, 
                                (double)known_words_correct / (double)known_words * 100.0);
                            Utils.Verbose("Točnost na neznanih besedah: ..... {2:0.00}% ({0} / {1})\r\n", unknown_words_correct, unknown_words, 
                                (double)unknown_words_correct / (double)unknown_words * 100.0);
                            Utils.Verbose("Skupna točnost: .................. {2:0.00}% ({0} / {1})\r\n", all_words_correct, all_words, 
                                (double)all_words_correct / (double)all_words * 100.0);
                            Utils.Verbose("Točnost na znanih besedah (POS):   {2:0.00}% ({0} / {1})\r\n", known_words_pos_correct, known_words,
                                (double)known_words_pos_correct / (double)known_words * 100.0);
                            Utils.Verbose("Točnost na neznanih besedah (POS): {2:0.00}% ({0} / {1})\r\n", unknown_words_pos_correct, unknown_words,
                                (double)unknown_words_pos_correct / (double)unknown_words * 100.0);
                            Utils.Verbose("Skupna točnost (POS): ............ {2:0.00}% ({0} / {1})\r\n", all_words_pos_correct, all_words, 
                                (double)all_words_pos_correct / (double)all_words * 100.0);
                            if (lemmatizer != null)
                            {
                            Utils.Verbose("Točnost lematizacije: ............ {2:0.00}% ({0} / {1})\r\n", lemma_correct, lemma_words,
                                    (double)lemma_correct / (double)lemma_words * 100.0);
                            Utils.Verbose("Točnost lematizacije (male črke):  {2:0.00}% ({0} / {1})\r\n", lemma_correct_lowercase, lemma_words,
                                    (double)lemma_correct_lowercase / (double)lemma_words * 100.0);
                            }
                            Utils.Verbose("Točnost detekcije konca stavka: .. {2:0.00}% ({0} / {1})\r\n", eos_correct, eos_count,
                                (double)eos_correct / (double)eos_count * 100.0);
                        }
                    }                    
                }                
            }
            catch (Exception exception)
            {
                Console.WriteLine("*** Nepričakovana napaka. Podrobnosti: {0}\r\n{1}", exception, exception.StackTrace);
            }
        }
    }
}

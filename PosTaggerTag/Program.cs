/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          PosTaggerTag\Program.cs
 *  Version:       1.0
 *  Desc:		   POS tagger tagging utility
 *  Author:		   Miha Grcar
 *  Created on:    Sep-2009
 *  Last modified: Feb-2010
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Latino;
using Latino.Model;
using LemmaSharp;

namespace PosTagger
{
    class Program
    {
        static Regex m_nonword_regex
            = new Regex("^\\W+(\\<eos\\>)?$", RegexOptions.Compiled);

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
            Console.WriteLine("-k                Uporaba razčlenjevalnika SSJ.");
            Console.WriteLine("                  (privzeto: ne uporabi razčlenjevalnika SSJ)");
        }

        static bool ParseParams(string[] args, ref bool verbose, ref bool eval_mode, ref string corpus_file, ref string tagging_model_file, ref string lemmatizer_model_file, ref string tagged_corpus_file, ref bool ssj_tokenizer)
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
                else if (arg_lwr == "-k")
                {
                    ssj_tokenizer = true;
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
                Console.WriteLine("*** Napačno ime datoteke korpusa ali datoteka ne obstaja ({0}).\r\n", corpus_file);
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

        static ClassifierResult<string> ProcessResult(ClassifierResult<string> result, Set<string>.ReadOnly filter)
        {
            ClassifierResult<string> new_result = new ClassifierResult<string>();
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Key > 0)
                {
                    if (!m_nonword_regex.Match(result[i].Dat).Success && (filter == null || filter.Contains(result[i].Dat)))
                    {
                        new_result.Items.Add(result[i]);
                    }
                }
            }
            if (new_result.Count == 0) { new_result.Items.Add(new KeyDat<double, string>(1, "*")); } // just to make sure something is there
            return new_result;
        }

        static void Main(string[] args)
        {
            Queue<string> context = new Queue<string>();
            try
            {
                if (args.Length < 4)
                {
                    OutputHelp();
                }
                else
                {
                    bool eval_mode = false;
                    bool verbose = false;
                    string corpus_file = null, tagger_model_file = null, lemmatizer_model_file = null, tagged_corpus_file = null;
                    bool ssj_tokenizer = false;
                    if (ParseParams(args, ref verbose, ref eval_mode, ref corpus_file, ref tagger_model_file, ref lemmatizer_model_file, ref tagged_corpus_file, ref ssj_tokenizer))
                    {
                        Utils.VerboseEnabled = verbose;
                        Utils.VerboseLine("Nalagam model za označevanje ...");
                        BinarySerializer reader = new BinarySerializer(tagger_model_file, FileMode.Open);
                        SuffixTrie suffix_trie = new SuffixTrie(reader);
                        Dictionary<string, int> feature_space = Utils.LoadDictionary<string, int>(reader);
                        MaximumEntropyClassifier<string> model = new MaximumEntropyClassifier<string>(reader);
                        reader.Close();
                        Lemmatizer lemmatizer = null;
                        bool attach_tags = false;
                        if (lemmatizer_model_file != null)
                        {
                            Utils.VerboseLine("Nalagam model za lematizacijo ...");
                            reader = new BinarySerializer(lemmatizer_model_file, FileMode.Open);
                            attach_tags = reader.ReadBool();
                            lemmatizer = new Lemmatizer(reader);
                            reader.Close();
                        }
                        Utils.VerboseLine("Nalagam besedilo ...");
                        Corpus corpus = new Corpus();
                        if (eval_mode)
                        {
                            corpus.LoadFromXmlFile(corpus_file, /*tag_len=*/-1);
                        }
                        else if (ssj_tokenizer)
                        {
                            corpus.LoadFromTextSsjTokenizer(File.ReadAllText(corpus_file));
                        }
                        else
                        {
                            corpus.LoadFromText(File.ReadAllText(corpus_file));
                        }
                        DateTime start_time = DateTime.Now;
                        Utils.VerboseLine("Označujem besedilo ...");
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
                            context.Enqueue(corpus.TaggedWords[i].Word);
                            if (context.Count > 10) { context.Dequeue(); }
                            Utils.Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);              
                            BinaryVector<int> feature_vector = corpus.GenerateFeatureVector(i, feature_space, /*extend_feature_space=*/false, suffix_trie);
                            ClassifierResult<string> result = model.Classify(feature_vector);
                            if (m_nonword_regex.Match(corpus.TaggedWords[i].WordLower).Success) // non-word
                            {
                                bool flag = false;
                                foreach (KeyDat<double, string> item in result)
                                {
                                    if (corpus.TaggedWords[i].Word == item.Dat || corpus.TaggedWords[i].Word + "<eos>" == item.Dat)
                                    {
                                        corpus.TaggedWords[i].Tag = item.Dat;
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    corpus.TaggedWords[i].Tag = corpus.TaggedWords[i].Word;
                                }
                            }
                            else // word
                            {
                                Set<string>.ReadOnly filter = suffix_trie.Contains(corpus.TaggedWords[i].WordLower) ? suffix_trie.GetTags(corpus.TaggedWords[i].WordLower) : null;
                                corpus.TaggedWords[i].Tag = ProcessResult(result, filter).BestClassLabel;    
                                if (lemmatizer != null)
                                {
                                    string tag = corpus.TaggedWords[i].Tag;
                                    string word_lower = corpus.TaggedWords[i].WordLower;
                                    string lemma_src = attach_tags ? string.Format("{0}-{1}", word_lower, tag[0]) : word_lower;
                                    string lemma = lemmatizer.Lemmatize(lemma_src);
                                    if (string.IsNullOrEmpty(lemma) || (attach_tags && lemma == lemma_src)) { lemma = word_lower; }
                                    lemma = FixWordCase(lemma, corpus.TaggedWords, i); // case-fixing heuristics                                    
                                    if (eval_mode)
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
                            }
                        }
                        Utils.VerboseLine("");
                        TimeSpan span = DateTime.Now - start_time;
                        Utils.VerboseLine("Trajanje označevanja: {0:00}:{1:00}:{2:00}.{3:000}.", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                        Utils.VerboseLine("Zapisujem označeno besedilo ...");
                        StreamWriter writer = new StreamWriter(tagged_corpus_file);
                        writer.Write(corpus.ToString(eval_mode ? "XML-MI" : "XML"));
                        writer.Close();
                        Utils.VerboseLine("Končano.");
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
                                if (gold_tags[i] != null && tag[0] == gold_tags[i][0])
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
                            Utils.VerboseLine("Točnost na znanih besedah: ....... {2:0.00}% ({0} / {1})", known_words_correct, known_words, 
                                (double)known_words_correct / (double)known_words * 100.0);
                            Utils.VerboseLine("Točnost na neznanih besedah: ..... {2:0.00}% ({0} / {1})", unknown_words_correct, unknown_words, 
                                (double)unknown_words_correct / (double)unknown_words * 100.0);
                            Utils.VerboseLine("Skupna točnost: .................. {2:0.00}% ({0} / {1})", all_words_correct, all_words, 
                                (double)all_words_correct / (double)all_words * 100.0);
                            Utils.VerboseLine("Točnost na znanih besedah (POS):   {2:0.00}% ({0} / {1})", known_words_pos_correct, known_words,
                                (double)known_words_pos_correct / (double)known_words * 100.0);
                            Utils.VerboseLine("Točnost na neznanih besedah (POS): {2:0.00}% ({0} / {1})", unknown_words_pos_correct, unknown_words,
                                (double)unknown_words_pos_correct / (double)unknown_words * 100.0);
                            Utils.VerboseLine("Skupna točnost (POS): ............ {2:0.00}% ({0} / {1})", all_words_pos_correct, all_words, 
                                (double)all_words_pos_correct / (double)all_words * 100.0);
                            if (lemmatizer != null)
                            {
                                Utils.VerboseLine("Točnost lematizacije: ............ {2:0.00}% ({0} / {1})", lemma_correct, lemma_words,
                                    (double)lemma_correct / (double)lemma_words * 100.0);
                                Utils.VerboseLine("Točnost lematizacije (male črke):  {2:0.00}% ({0} / {1})", lemma_correct_lowercase, lemma_words,
                                    (double)lemma_correct_lowercase / (double)lemma_words * 100.0);
                            }
                            Utils.VerboseLine("Točnost detekcije konca stavka: .. {2:0.00}% ({0} / {1})", eos_correct, eos_count,
                                (double)eos_correct / (double)eos_count * 100.0);
                        }
                    }                    
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("*** Nepričakovana napaka. Podrobnosti: {0}\r\n{1}", exception, exception.StackTrace);
                if (context.Count > 0)
                {
                    Console.WriteLine("*** Kontekst napake:");
                    int count = context.Count;
                    for (int i = 0; i < count - 1; i++)
                    {
                        Console.Write("\"{0}\" ", context.Dequeue());
                    }
                    Console.WriteLine("-->\"{0}\"<--", context.Dequeue());
                }
            }
        }
    }
}

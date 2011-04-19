/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          PosTaggerTrain\Program.cs
 *  Version:       1.0
 *  Desc:		   POS tagger training utility
 *  Author:		   Miha Grcar
 *  Created on:    Sep-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Latino;
using Latino.Model;

namespace PosTagger
{
    class Program
    {
        static bool m_verbose
            = false;

        static void OutputHelp()
        {
            Console.WriteLine("*** Oblikoslovni označevalnik 1.0 - Modul za učenje ***");
            Console.WriteLine();
            Console.WriteLine("Uporaba:");
            Console.WriteLine("PosTaggerTrain [<nastavitve>] <korpus_xml> <model_bin>");
            Console.WriteLine();
            Console.WriteLine("<nastavitve>: Glej spodaj.");
            Console.WriteLine("<korpus_xml>: Učni korpus v formatu XML-TEI (vhod).");
            Console.WriteLine("<model_bin>:  Model za označevanje (izhod).");
            Console.WriteLine();
            Console.WriteLine("Nastavitve:");
            Console.WriteLine("-v              Izpisovanje na zaslon (verbose).");
            Console.WriteLine("                (privzeto: ni izpisovanja)");
            Console.WriteLine("-c:<int>=0>     Parameter za izgradnjo modela (cut-off).");  
            Console.WriteLine("                (privzeto: 2)");
            Console.WriteLine("-i:<int>0>      Število iteracij za izgradnjo modela."); 
            Console.WriteLine("                (privzeto: 50)");
            Console.WriteLine("-t:<int>0>      Število niti za paralelizacijo algoritma.");
            Console.WriteLine("                (privzeto: 1)");
            Console.WriteLine("-l:ime_datoteke Zunanji leksikon.");
            Console.WriteLine("                (privzeto: označevanje brez leksikona)");
        } 

        static bool ParseParams(string[] args, ref bool verbose, ref int cut_off, ref int num_iter, ref int num_threads, ref string corpus_file_name, ref string model_file_name, ref string lexicon_file_name)
        {
            // parse
            for (int i = 0; i < args.Length - 2; i++)
            {
                string arg_lwr = args[i].ToLower();
                if (arg_lwr == "-v")
                {
                    verbose = true;
                }
                else if (arg_lwr.StartsWith("-c:"))
                {
                    try { cut_off = Convert.ToInt32(arg_lwr.Split(':')[1]); }
                    catch { cut_off = -1; }
                }
                else if (arg_lwr.StartsWith("-i:"))
                {
                    try { num_iter = Convert.ToInt32(arg_lwr.Split(':')[1]); }
                    catch { num_iter = 0; }
                }
                else if (arg_lwr.StartsWith("-t:"))
                {
                    try { num_threads = Convert.ToInt32(arg_lwr.Split(':')[1]); }
                    catch { num_threads = 0; }
                }
                else if (arg_lwr.StartsWith("-l:"))
                {
                    lexicon_file_name = args[i].Substring(3, args[i].Length - 3);
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // verify settings
            if (cut_off < 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -c. Vrednost mora biti celo število, večje ali enako 0.\r\n");
                OutputHelp();
                return false;
            }
            if (num_iter <= 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -i. Vrednost mora biti celo število, večje od 0.\r\n");
                OutputHelp();
                return false;
            }
            if (num_threads <= 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -t. Vrednost mora biti celo število, večje od 0.\r\n");
                OutputHelp();
                return false;
            }
            // check file names 
            if (lexicon_file_name != null && !Utils.VerifyFileNameOpen(lexicon_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke leksikona ali datoteka ne obstaja ({0}).\r\n", lexicon_file_name);
                OutputHelp();
                return false;
            }
            corpus_file_name = args[args.Length - 2];
            model_file_name = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpus_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke učnega korpusa ali datoteka ne obstaja ({0}).\r\n", corpus_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(model_file_name))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", model_file_name);
                OutputHelp();
                return false;
            }
            return true;
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    OutputHelp();
                }
                else
                {
                    int cut_off = 2;
                    int num_iter = 50;
                    int num_threads = 1;
                    string corpus_file_name = null, model_file_name = null, lexicon_file_name = null;
                    if (ParseParams(args, ref m_verbose, ref cut_off, ref num_iter, ref num_threads, ref corpus_file_name, ref model_file_name, ref lexicon_file_name))
                    {
                        Corpus corpus = new Corpus();
                        Utils.Verbose("Nalagam učni korpus ...\r\n");
                        corpus.LoadFromXml(corpus_file_name, /*tag_len=*/-1);
                        SuffixTrie suffix_trie = new SuffixTrie();
                        foreach (TaggedWord word in corpus.TaggedWords)
                        {
                            suffix_trie.AddWordTagPair(word.WordLower, word.Tag);
                        }
                        if (lexicon_file_name != null)
                        {
                            Utils.Verbose("Nalagam leksikon ...\r\n");
                            StreamReader lexReader = new StreamReader(lexicon_file_name);
                            string lexLine;
                            while ((lexLine = lexReader.ReadLine()) != null)
                            {
                                string[] lexData = lexLine.Split('\t');
                                suffix_trie.AddWordTagPair(lexData[0].ToLower(), lexData[2]);
                                //Console.WriteLine("{0} {1}", lexData[0], lexData[2]);
                            }
                            lexReader.Close();
                        }
                        suffix_trie.PropagateTags();
                        MaximumEntropyClassifier<string> model = new MaximumEntropyClassifier<string>();
                        Dataset<string, BinaryVector<int>.ReadOnly> dataset = new Dataset<string, BinaryVector<int>.ReadOnly>();
                        Dictionary<string, int> feature_space = new Dictionary<string, int>();
                        Utils.Verbose("Pripravljam vektorje značilk ...\r\n");
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            Utils.Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                            BinaryVector<int> feature_vector = corpus.GenerateFeatureVector(i, feature_space, /*extend_feature_space=*/true, suffix_trie);
                            dataset.Add(corpus.TaggedWords[i].Tag, feature_vector);
                        }
                        Utils.Verbose("\r\n");
                        Utils.Verbose("Gradim model ...\r\n");
                        DateTime start_time = DateTime.Now;
                        model.CutOff = cut_off;
                        model.NumThreads = num_threads;
                        model.NumIter = num_iter;
                        model.Train(dataset);
                        TimeSpan span = DateTime.Now - start_time;
                        Utils.Verbose("Trajanje gradnje modela: {0:00}:{1:00}:{2:00}.{3:000}.\r\n", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                        Utils.Verbose("Zapisujem model ...\r\n");
                        BinarySerializer writer = new BinarySerializer(model_file_name, FileMode.Create);
                        suffix_trie.Save(writer);
                        Utils.SaveDictionary(feature_space, writer);
                        model.Save(writer);
                        writer.Close();                        
                        Utils.Verbose("Končano.\r\n");
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

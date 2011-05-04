/*==========================================================================;
 *
 *  This file is part of SSJ software. See http://www.slovenscina.eu
 *
 *  File:    PosTaggerTrain\Program.cs
 *  Desc:    POS tagger training utility
 *  Created: Sep-2009
 *
 *  Author:  Miha Grcar
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
        static bool mVerbose
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

        static bool ParseParams(string[] args, ref bool verbose, ref int cutOff, ref int numIter, ref int numThreads, ref string corpusFileName, ref string modelFileName, ref string lexiconFileName)
        {
            // parse
            for (int i = 0; i < args.Length - 2; i++)
            {
                string argLwr = args[i].ToLower();
                if (argLwr == "-v")
                {
                    verbose = true;
                }
                else if (argLwr.StartsWith("-c:"))
                {
                    try { cutOff = Convert.ToInt32(argLwr.Split(':')[1]); }
                    catch { cutOff = -1; }
                }
                else if (argLwr.StartsWith("-i:"))
                {
                    try { numIter = Convert.ToInt32(argLwr.Split(':')[1]); }
                    catch { numIter = 0; }
                }
                else if (argLwr.StartsWith("-t:"))
                {
                    try { numThreads = Convert.ToInt32(argLwr.Split(':')[1]); }
                    catch { numThreads = 0; }
                }
                else if (argLwr.StartsWith("-l:"))
                {
                    lexiconFileName = args[i].Substring(3, args[i].Length - 3);
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // verify settings
            if (cutOff < 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -c. Vrednost mora biti celo število, večje ali enako 0.\r\n");
                OutputHelp();
                return false;
            }
            if (numIter <= 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -i. Vrednost mora biti celo število, večje od 0.\r\n");
                OutputHelp();
                return false;
            }
            if (numThreads <= 0)
            {
                Console.WriteLine("*** Napačna vrednost parametra -t. Vrednost mora biti celo število, večje od 0.\r\n");
                OutputHelp();
                return false;
            }
            // check file names 
            if (lexiconFileName != null && !Utils.VerifyFileNameOpen(lexiconFileName))
            {
                Console.WriteLine("*** Napačno ime datoteke leksikona ali datoteka ne obstaja ({0}).\r\n", lexiconFileName);
                OutputHelp();
                return false;
            }
            corpusFileName = args[args.Length - 2];
            modelFileName = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpusFileName))
            {
                Console.WriteLine("*** Napačno ime datoteke učnega korpusa ali datoteka ne obstaja ({0}).\r\n", corpusFileName);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(modelFileName))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", modelFileName);
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
                    int cutOff = 2;
                    int numIter = 50;
                    int numThreads = 1;
                    string corpusFileName = null, modelFileName = null, lexiconFileName = null;
                    if (ParseParams(args, ref mVerbose, ref cutOff, ref numIter, ref numThreads, ref corpusFileName, ref modelFileName, ref lexiconFileName))
                    {
                        Corpus corpus = new Corpus();
                        Console.Write("Nalagam učni korpus ...\r\n");
                        corpus.LoadFromXmlFile(corpusFileName, /*tagLen=*/-1);
                        SuffixTrie suffixTrie = new SuffixTrie();
                        foreach (TaggedWord word in corpus.TaggedWords)
                        {
                            suffixTrie.AddWordTagPair(word.WordLower, word.Tag);
                        }
                        if (lexiconFileName != null)
                        {
                            Console.Write("Nalagam leksikon ...\r\n");
                            StreamReader lexReader = new StreamReader(lexiconFileName);
                            string lexLine;
                            while ((lexLine = lexReader.ReadLine()) != null)
                            {
                                string[] lexData = lexLine.Split('\t');
                                suffixTrie.AddWordTagPair(lexData[0].ToLower(), lexData[2]);
                            }
                            lexReader.Close();
                        }
                        suffixTrie.PropagateTags();
                        MaximumEntropyClassifierFast<string> model = new MaximumEntropyClassifierFast<string>();
                        LabeledDataset<string, BinaryVector<int>.ReadOnly> dataset = new LabeledDataset<string, BinaryVector<int>.ReadOnly>();
                        Dictionary<string, int> featureSpace = new Dictionary<string, int>();
                        Console.Write("Pripravljam vektorje značilk ...\r\n");
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            Console.Write("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                            BinaryVector<int> featureVector = corpus.GenerateFeatureVector(i, featureSpace, /*extendFeatureSpace=*/true, suffixTrie);
                            dataset.Add(corpus.TaggedWords[i].Tag, featureVector);
                        }
                        Console.Write("\r\n");
                        Console.Write("Gradim model ...\r\n");
                        DateTime startTime = DateTime.Now;
                        model.CutOff = cutOff;
                        model.NumThreads = numThreads;
                        model.NumIter = numIter;
                        model.Train(dataset);
                        TimeSpan span = DateTime.Now - startTime;
                        Console.Write("Trajanje gradnje modela: {0:00}:{1:00}:{2:00}.{3:000}.\r\n", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                        Console.Write("Zapisujem model ...\r\n");
                        BinarySerializer writer = new BinarySerializer(modelFileName, FileMode.Create);
                        suffixTrie.Save(writer);
                        Utils.SaveDictionary(featureSpace, writer);
                        model.Save(writer);
                        writer.Close();                        
                        Console.Write("Končano.\r\n");
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
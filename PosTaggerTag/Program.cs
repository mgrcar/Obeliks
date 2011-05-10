/*==========================================================================;
 *
 *  Avtorske pravice za to izdajo programske opreme ureja licenca 
 *    Priznanje avtorstva-Nekomercialno-Brez predelav 2.5
 *  This work is licenced under the Creative Commons 
 *    Attribution-NonCommercial-NoDerivs 2.5 licence
 *
 *  Projekt Sporazumevanje v slovenskem jeziku: 
 *    http://www.slovenscina.eu/Vsebine/Sl/Domov/Domov.aspx
 *  Project Communication in Slovene: 
 *    http://www.slovenscina.eu/Vsebine/En/Domov/Domov.aspx
 *
 *  File:    PosTaggerTag\Program.cs
 *  Desc:    POS tagger tagging utility
 *  Created: Sep-2009
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Latino;
using Latino.Model;
using LemmaSharp;

namespace PosTagger
{
    class Program
    {
        static Regex mNonWordRegex
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

        static bool ParseParams(string[] args, ref bool verbose, ref bool evalMode, ref string corpusFile, ref string taggingModelFile, ref string lemmatizerModelFile, ref string taggedCorpusFile, ref bool ssjTokenizer)
        {
            // parse
            for (int i = 0; i < args.Length - 3; i++)
            {
                string argLwr = args[i].ToLower();
                if (argLwr == "-v")
                {
                    verbose = true;
                }
                else if (argLwr == "-xml")
                {
                    evalMode = true;
                }
                else if (argLwr.StartsWith("-lem:"))
                {
                    lemmatizerModelFile = args[i].Substring(5, args[i].Length - 5);
                }
                else if (argLwr == "-k")
                {
                    ssjTokenizer = true;
                } 
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            corpusFile = args[args.Length - 3];
            taggingModelFile = args[args.Length - 2];
            taggedCorpusFile = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpusFile))
            {
                Console.WriteLine("*** Napačno ime datoteke korpusa ali datoteka ne obstaja ({0}).\r\n", corpusFile);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(taggingModelFile))
            {
                Console.WriteLine("*** Napačno ime datoteke modela za označevanje ali datoteka ne obstaja ({0}).\r\n", taggingModelFile);
                OutputHelp();
                return false;
            }
            if (lemmatizerModelFile != null && !Utils.VerifyFileNameOpen(lemmatizerModelFile))
            {
                Console.WriteLine("*** Napačno ime datoteke modela za lematizacijo ali datoteka ne obstaja ({0}).\r\n", lemmatizerModelFile);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(taggedCorpusFile))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", taggedCorpusFile);
                OutputHelp();
                return false;
            }
            return true;
        }

        static Prediction<string> ProcessResult(Prediction<string> result, Set<string>.ReadOnly filter)
        {
            ArrayList<KeyDat<double, string>> newResult = new ArrayList<KeyDat<double, string>>();
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Key > 0)
                {
                    if (!mNonWordRegex.Match(result[i].Dat).Success && (filter == null || filter.Contains(result[i].Dat)))
                    {
                        newResult.Add(result[i]);
                    }
                }
            }
            return new Prediction<string>(newResult);
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
                    bool evalMode = false;
                    bool verbose = false;
                    string corpusFile = null, taggerModelFile = null, lemmatizerModelFile = null, taggedCorpusFile = null;
                    bool ssjTokenizer = false;
                    if (ParseParams(args, ref verbose, ref evalMode, ref corpusFile, ref taggerModelFile, ref lemmatizerModelFile, ref taggedCorpusFile, ref ssjTokenizer))
                    {
                        Logger logger = Logger.GetRootLogger();
                        if (!verbose)
                        {
                            logger.LocalLevel = Logger.Level.Off;
                            logger.LocalProgressOutputType = Logger.ProgressOutputType.Off;
                        }
                        else
                        {
                            logger.LocalOutputType = Logger.OutputType.Custom;
                            logger.CustomOutput = new Logger.CustomOutputDelegate(delegate(string loggerName, Logger.Level level, string funcName, Exception e,
                                string message, object[] msgArgs) { Console.WriteLine(message, msgArgs); });
                        }
                        logger.Info(/*funcName=*/null, "Nalagam model za označevanje ...");
                        GC.Collect();
                        long inUseStart = Process.GetCurrentProcess().PrivateMemorySize64;
                        BinarySerializer reader = new BinarySerializer(taggerModelFile, FileMode.Open);
                        PatriciaTree suffixTrie = new PatriciaTree(reader);
                        GC.Collect();
                        long inUse = Process.GetCurrentProcess().PrivateMemorySize64;
                        logger.Info(/*funcName=*/null, "Poraba pomnilnika (drevo končnic): {0:0.00} MB", (double)(inUse - inUseStart) / 1048576.0);
                        inUseStart = inUse;
                        Dictionary<string, int> featureSpace = Utils.LoadDictionary<string, int>(reader);
                        MaximumEntropyClassifierFast<string> model = new MaximumEntropyClassifierFast<string>(reader);
                        reader.Close();
                        Lemmatizer lemmatizer = null;
                        bool considerTags = false;
                        GC.Collect();
                        inUse = Process.GetCurrentProcess().PrivateMemorySize64;
                        logger.Info(/*funcName=*/null, "Poraba pomnilnika (klasifikacijski model): {0:0.00} MB", (double)(inUse - inUseStart) / 1048576.0);
                        inUseStart = inUse;
                        if (lemmatizerModelFile != null)
                        {
                            logger.Info(/*funcName=*/null, "Nalagam model za lematizacijo ...");
                            reader = new BinarySerializer(lemmatizerModelFile, FileMode.Open);
                            considerTags = reader.ReadBool();
                            lemmatizer = new Lemmatizer(reader);
                            reader.Close();
                            GC.Collect();
                            inUse = Process.GetCurrentProcess().PrivateMemorySize64;
                            logger.Info(/*funcName=*/null, "Poraba pomnilnika (lematizacijsko drevo): {0:0.00} MB", (double)(inUse - inUseStart) / 1048576.0);
                        }
                        logger.Info(/*funcName=*/null, "Nalagam besedilo ...");
                        Corpus corpus = new Corpus();
                        if (evalMode)
                        {
                            corpus.LoadFromXmlFile(corpusFile, /*tagLen=*/-1);
                        }
                        else if (ssjTokenizer)
                        {
                            corpus.LoadFromTextSsjTokenizer(File.ReadAllText(corpusFile));
                        }
                        else
                        {
                            corpus.LoadFromText(File.ReadAllText(corpusFile));
                        }
                        DateTime startTime = DateTime.Now;
                        logger.Info(/*funcName=*/null, "Označujem besedilo ...");
                        int knownWordsCorrect = 0;
                        int knownWordsPosCorrect = 0;
                        int knownWords = 0;
                        int unknownWordsCorrect = 0;
                        int unknownWordsPosCorrect = 0;
                        int unknownWords = 0;
                        int lemmaCorrect = 0;
                        int lemmaCorrectLowercase = 0;
                        int eosCount = 0;
                        int eosCorrect = 0;
                        int lemmaWords = 0;                        
                        string[] goldTags = new string[corpus.TaggedWords.Count];
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            goldTags[i] = corpus.TaggedWords[i].Tag;
                        }
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            context.Enqueue(corpus.TaggedWords[i].Word);
                            if (context.Count > 10) { context.Dequeue(); }
                            logger.ProgressFast(/*sender=*/null, /*funcName=*/null, "{0} / {1}", i + 1, corpus.TaggedWords.Count);              
                            BinaryVector<int> featureVector = corpus.GenerateFeatureVector(i, featureSpace, /*extendFeatureSpace=*/false, suffixTrie);
                            Prediction<string> result = model.Predict(featureVector);
                            if (mNonWordRegex.Match(corpus.TaggedWords[i].WordLower).Success) // non-word
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
                                Set<string>.ReadOnly filter = suffixTrie.Contains(corpus.TaggedWords[i].WordLower) ? suffixTrie.GetTags(corpus.TaggedWords[i].WordLower) : null;
                                result = ProcessResult(result, filter);
                                corpus.TaggedWords[i].Tag = result.Count == 0 ? "*"/*unable to classify*/ : result.BestClassLabel;    
                                if (lemmatizer != null)
                                {
                                    string tag = corpus.TaggedWords[i].Tag;
                                    string wordLower = corpus.TaggedWords[i].WordLower;
                                    //if (tag == "*")
                                    //{
                                    //    // *** TODO: take the most frequent tag from the filter (currently, frequency info not available)
                                    //    Console.WriteLine(tag);
                                    //    Console.WriteLine(filter);
                                    //}
                                    string lemma = (considerTags && tag != "*") ? lemmatizer.Lemmatize(wordLower, tag) : lemmatizer.Lemmatize(wordLower);
                                    if (string.IsNullOrEmpty(lemma) || (considerTags && lemma == wordLower)) { lemma = wordLower; }
                                    if (evalMode)
                                    {
                                        lemmaWords++;
                                        if (lemma == corpus.TaggedWords[i].Lemma)
                                        {
                                            lemmaCorrect++;
                                        }
                                        if (corpus.TaggedWords[i].Lemma != null && lemma.ToLower() == corpus.TaggedWords[i].Lemma.ToLower())
                                        {
                                            lemmaCorrectLowercase++;
                                        }
                                    }
                                    corpus.TaggedWords[i].Lemma = lemma;
                                }
                            }
                        }
                        TimeSpan span = DateTime.Now - startTime;
                        logger.Info(/*funcName=*/null, "Trajanje označevanja: {0:00}:{1:00}:{2:00}.{3:000}.", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
                        logger.Info(/*funcName=*/null, "Zapisujem označeno besedilo ...");
                        StreamWriter writer = new StreamWriter(taggedCorpusFile);
                        writer.Write(corpus.ToString(evalMode ? "XML-MI" : "XML"));
                        writer.Close();
                        logger.Info(/*funcName=*/null, "Končano.");
                        if (evalMode)
                        {
                            for (int i = 0; i < corpus.TaggedWords.Count; i++)
                            {
                                string wordLower = corpus.TaggedWords[i].WordLower;
                                string tag = corpus.TaggedWords[i].Tag;
                                bool isKnown = suffixTrie.Contains(wordLower);                             
                                if (tag == goldTags[i])
                                {
                                    if (isKnown) { knownWordsCorrect++; }
                                    else { unknownWordsCorrect++; }
                                }
                                if (goldTags[i] != null && tag[0] == goldTags[i][0])
                                {
                                    if (isKnown) { knownWordsPosCorrect++; }
                                    else { unknownWordsPosCorrect++; }
                                }
                                if (isKnown) { knownWords++; }
                                else { unknownWords++; }
                                if (corpus.TaggedWords[i].MoreInfo.EndOfSentence)
                                {
                                    eosCount++;
                                    if (tag.EndsWith("<eos>")) { eosCorrect++; }
                                }
                            }
                            int allWords = knownWords + unknownWords;
                            int allWordsCorrect = knownWordsCorrect + unknownWordsCorrect;
                            int allWordsPosCorrect = knownWordsPosCorrect + unknownWordsPosCorrect;
                            Console.WriteLine("Točnost na znanih besedah: ....... {2:0.00}% ({0} / {1})", knownWordsCorrect, knownWords, 
                                (double)knownWordsCorrect / (double)knownWords * 100.0);
                            Console.WriteLine("Točnost na neznanih besedah: ..... {2:0.00}% ({0} / {1})", unknownWordsCorrect, unknownWords, 
                                (double)unknownWordsCorrect / (double)unknownWords * 100.0);
                            Console.WriteLine("Skupna točnost: .................. {2:0.00}% ({0} / {1})", allWordsCorrect, allWords, 
                                (double)allWordsCorrect / (double)allWords * 100.0);
                            Console.WriteLine("Točnost na znanih besedah (POS):   {2:0.00}% ({0} / {1})", knownWordsPosCorrect, knownWords,
                                (double)knownWordsPosCorrect / (double)knownWords * 100.0);
                            Console.WriteLine("Točnost na neznanih besedah (POS): {2:0.00}% ({0} / {1})", unknownWordsPosCorrect, unknownWords,
                                (double)unknownWordsPosCorrect / (double)unknownWords * 100.0);
                            Console.WriteLine("Skupna točnost (POS): ............ {2:0.00}% ({0} / {1})", allWordsPosCorrect, allWords, 
                                (double)allWordsPosCorrect / (double)allWords * 100.0);
                            if (lemmatizer != null)
                            {
                                Console.WriteLine("Točnost lematizacije: ............ {2:0.00}% ({0} / {1})", lemmaCorrect, lemmaWords,
                                    (double)lemmaCorrect / (double)lemmaWords * 100.0);
                                Console.WriteLine("Točnost lematizacije (male črke):  {2:0.00}% ({0} / {1})", lemmaCorrectLowercase, lemmaWords,
                                    (double)lemmaCorrectLowercase / (double)lemmaWords * 100.0);
                            }
                            Console.WriteLine("Točnost detekcije konca stavka: .. {2:0.00}% ({0} / {1})", eosCorrect, eosCount,
                                (double)eosCorrect / (double)eosCount * 100.0);
                        }
                    }                    
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine();
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

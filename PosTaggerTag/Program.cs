/*==========================================================================;
 *
 *  Projekt Sporazumevanje v slovenskem jeziku: 
 *    http://www.slovenscina.eu/Vsebine/Sl/Domov/Domov.aspx
 *  Project Communication in Slovene: 
 *    http://www.slovenscina.eu/Vsebine/En/Domov/Domov.aspx
 *    
 *  Avtorske pravice za to izdajo programske opreme ureja licenca 
 *    Creative Commons Priznanje avtorstva-Nekomercialno-Brez predelav 2.5
 *  This work is licenced under the Creative Commons 
 *    Attribution-NonCommercial-NoDerivs 2.5 licence
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
    public static class PosTaggerTag
    {
        private static Regex mNonWordRegex
            = new Regex(@"^\W+(\<eos\>)?$", RegexOptions.Compiled);
        private static Logger mLogger
            = Logger.GetRootLogger();

        private static void OutputHelp()
        {
#if !LIB
            mLogger.Info(null, "*** Oblikoslovni označevalnik 1.0 - Modul za označevanje ***");
            mLogger.Info(null, "");
            mLogger.Info(null, "Uporaba:");
            mLogger.Info(null, "PosTaggerTag [<nastavitve>] <vhodne_datoteke> <model_bin> <označeni_korpus_xml>"); 
            mLogger.Info(null, "");
            mLogger.Info(null, "<nastavitve>:     Glej spodaj.");
            mLogger.Info(null, "<besedilo>:       Besedilo za označevanje (vhod).");
            mLogger.Info(null, "<model_bin>:      Model za označevanje (vhod).");
            mLogger.Info(null, "<označeni_korpus_xml>:"); 
            mLogger.Info(null, "                  Označeni korpus v formatu XML-TEI (izhod).");
            mLogger.Info(null, "");
            mLogger.Info(null, "Nastavitve:");
            mLogger.Info(null, "-v                Izpisovanje na zaslon (verbose).");
            mLogger.Info(null, "                  (privzeto: ni izpisovanja)");
            mLogger.Info(null, "-lem:ime_datoteke Model za lematizacijo.");
            mLogger.Info(null, "                  (privzeto: lematizacija se ne izvede)");
            mLogger.Info(null, "-s                Vključi podmape pri iskanju vhodnih besedil.");
            mLogger.Info(null, "                  (privzeto: išči samo v podani mapi)");
            mLogger.Info(null, "-t                Uporaba razčlenjevalnika SSJ.");
            mLogger.Info(null, "                  (privzeto: ne uporabi razčlenjevalnika SSJ)");           
#endif
        }

        private static string Locate(string path, out string searchPattern)
        {
            path = path.Trim();
            searchPattern = null;
            if (path == "") { return null; }
            if (!path.Contains("\\")) { path = ".\\" + path; }
            if (Utils.VerifyFolderName(path, /*mustExist=*/true)) { searchPattern = "*.*"; return path; }
            int splitAt = path.LastIndexOf('\\');
            searchPattern = path.Substring(splitAt + 1);
            path = path.Substring(0, splitAt);
            if (Utils.VerifyFolderName(path, /*mustExist=*/true)) { return path; }
            return null;
        }

        private static bool ParseParams(string[] args, ref bool verbose, ref string inputFolder, ref string searchPattern, 
            ref string taggerModelFile, ref string lemmatizerModelFile, ref string outputFileOrFolder, ref bool ssjTokenizer,
            ref bool searchSubfolders)
        {
            // parse
            for (int i = 0; i < args.Length - 3; i++)
            {
                string argLwr = args[i].ToLower();
                if (argLwr == "-v")
                {
                    verbose = true;
                }
                else if (argLwr.StartsWith("-lem:"))
                {
                    lemmatizerModelFile = args[i].Substring(5, args[i].Length - 5);
                }
                else if (argLwr == "-t")
                {
                    ssjTokenizer = true;
                }
                else if (argLwr == "-s")
                {
                    searchSubfolders = true;
                }
                else
                {
                    mLogger.Info(null, "*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            inputFolder = args[args.Length - 3];
            taggerModelFile = args[args.Length - 2];
            outputFileOrFolder = args[args.Length - 1];
            if ((inputFolder = Locate(inputFolder, out searchPattern)) == null)
            {
                mLogger.Info(null, "*** Napačen vnos vhodnih datotek ({0}).\r\n", inputFolder);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(taggerModelFile))
            {
                mLogger.Info(null, "*** Napačno ime datoteke modela za označevanje ali datoteka ne obstaja ({0}).\r\n", taggerModelFile);
                OutputHelp();
                return false;
            }
            if (lemmatizerModelFile != null && !Utils.VerifyFileNameOpen(lemmatizerModelFile))
            {
                mLogger.Info(null, "*** Napačno ime datoteke modela za lematizacijo ali datoteka ne obstaja ({0}).\r\n", lemmatizerModelFile);
                OutputHelp();
                return false;
            }
            bool isFolder = searchPattern.Contains("*") || searchPattern.Contains("?");
            if ((isFolder && !(Utils.VerifyFolderName(outputFileOrFolder, /*mustExist=*/false))) || (!isFolder && !Utils.VerifyFileNameCreate(outputFileOrFolder))) 
            {
                mLogger.Info(null, "*** Napačno ime izhodne {1} ({0}).\r\n", outputFileOrFolder, isFolder ? "mape" : "datoteke");
                OutputHelp();
                return false;
            }
            if (new DirectoryInfo(inputFolder).GetFiles(searchPattern).Length == 0)
            {
                mLogger.Info(null, "*** S podanim iskalnim vzorcem ni moč najti nobene vhodne datoteke.");
                mLogger.Info(null, "Mapa z vhodnimi datotekami: {0}", inputFolder);
                mLogger.Info(null, "Iskalni vzorec: {0}", searchPattern);
                OutputHelp();                
                return false;            
            }
            return true;
        }

        private static Prediction<string> ProcessResult(Prediction<string> result, Set<string> filter)
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

        public static void Tag(Corpus corpus, MaximumEntropyClassifierFast<string> model, Dictionary<string, int> featureSpace,
            PatriciaTree suffixTree, Lemmatizer lemmatizer, bool considerTags)
        {
            int foo, bar, foobar;
            Tag(corpus, model, featureSpace, suffixTree, lemmatizer, considerTags, out foo, out bar, out foobar, /*xmlMode=*/false);
        }

        public static void Tag(Corpus corpus, MaximumEntropyClassifierFast<string> model, Dictionary<string, int> featureSpace,
            PatriciaTree suffixTree, Lemmatizer lemmatizer, bool considerTags, out int lemmaCorrect, out int lemmaCorrectLowercase,
            out int lemmaWords, bool xmlMode)
        {
            DateTime startTime = DateTime.Now;
            mLogger.Debug(null, "Označujem besedilo ...");
            lemmaCorrect = 0;
            lemmaCorrectLowercase = 0;
            lemmaWords = 0;
            for (int i = 0; i < corpus.TaggedWords.Count; i++)
            {
                mLogger.ProgressFast(/*sender=*/null, null, "{0} / {1}", i + 1, corpus.TaggedWords.Count);
                BinaryVector<int> featureVector = corpus.GenerateFeatureVector(i, featureSpace, /*extendFeatureSpace=*/false, suffixTree);
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
                    Set<string> filter = suffixTree.Contains(corpus.TaggedWords[i].WordLower) ? suffixTree.GetTags(corpus.TaggedWords[i].WordLower) : null;
                    result = ProcessResult(result, filter);
                    corpus.TaggedWords[i].Tag = result.Count == 0 ? "*"/*unable to classify*/ : result.BestClassLabel;
                    if (lemmatizer != null)
                    {
                        string tag = corpus.TaggedWords[i].Tag;
                        string wordLower = corpus.TaggedWords[i].WordLower;
                        //if (tag == "*")
                        //{
                        //    // *** TODO: take the most frequent tag from the filter (currently, frequency info not available)
                        //    logger.Info(null, tag);
                        //    logger.Info(null, filter);
                        //}
                        string lemma = (considerTags && tag != "*") ? lemmatizer.Lemmatize(wordLower, tag) : lemmatizer.Lemmatize(wordLower);
                        if (string.IsNullOrEmpty(lemma) || (considerTags && lemma == wordLower)) { lemma = wordLower; }
                        if (xmlMode)
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
            mLogger.Debug(null, "Trajanje označevanja: {0:00}:{1:00}:{2:00}.{3:000}.", span.Hours, span.Minutes, span.Seconds, span.Milliseconds);
        }

        private static bool IsXmlTei(string content)
        {
            return content.Contains("<TEI") && content.Contains("<text") && content.Contains("<w");
        }

#if LIB
        public static void Tag(string[] args)
        {
#else
        private static void Main(string[] args)
        {
            // initialize logger
            mLogger.LocalLevel = Logger.Level.Debug;
            mLogger.LocalOutputType = Logger.OutputType.Custom;
            mLogger.CustomOutput = new Logger.CustomOutputDelegate(delegate(string loggerName, Logger.Level level, string funcName, Exception e,
                string message, object[] msgArgs) { Console.WriteLine(message, msgArgs); });
#endif
            try
            {
                if (args.Length < 4)
                {
                    OutputHelp();
                }
                else
                {
                    string inputFolder = null, searchPattern = null, taggerModelFile = null, lemmatizerModelFile = null, outputFileOrFolder = null;
                    bool ssjTokenizer = false, searchSubfolders = false, verbose = false;
                    if (ParseParams(args, ref verbose, ref inputFolder, ref searchPattern, ref taggerModelFile, ref lemmatizerModelFile, 
                        ref outputFileOrFolder, ref ssjTokenizer, ref searchSubfolders))
                    {
                        if (!verbose)
                        {
                            mLogger.LocalLevel = Logger.Level.Info;
                            mLogger.LocalProgressOutputType = Logger.ProgressOutputType.Off;
                        }
                        mLogger.Debug(null, "Nalagam model za označevanje ...");
                        BinarySerializer reader = new BinarySerializer(taggerModelFile, FileMode.Open);
                        PatriciaTree suffixTree = new PatriciaTree(reader);
                        Dictionary<string, int> featureSpace = Utils.LoadDictionary<string, int>(reader);
                        MaximumEntropyClassifierFast<string> model = new MaximumEntropyClassifierFast<string>(reader);
                        reader.Close();
                        Lemmatizer lemmatizer = null;
                        bool considerTags = false;
                        if (lemmatizerModelFile != null)
                        {
                            mLogger.Debug(null, "Nalagam model za lematizacijo ...");
                            reader = new BinarySerializer(lemmatizerModelFile, FileMode.Open);
                            considerTags = reader.ReadBool();
                            lemmatizer = new Lemmatizer(reader);
                            reader.Close();
                        }
                        mLogger.Debug(null, "Mapa z vhodnimi datotekami: {0}", inputFolder);
                        mLogger.Debug(null, "Iskalni vzorec: {0}", searchPattern);
                        foreach (FileInfo file in new DirectoryInfo(inputFolder).GetFiles(searchPattern, 
                            searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                        {
                            mLogger.Debug(null, "Nalagam {0} ...", file.FullName);
                            Corpus corpus;
                            bool xmlMode = false;
                            try
                            {
                                corpus = new Corpus();
                                corpus.LoadFromXmlFile(file.FullName, /*tagLen=*/-1);
                                xmlMode = true;
                            }
                            catch (Exception e)
                            {
                                corpus = new Corpus();
                                string content = File.ReadAllText(file.FullName);
                                if (IsXmlTei(content))
                                {
                                    mLogger.Debug(null, "*** Opozorilo: Datoteka z besedilom vsebuje značke XML-TEI, vendar nima pravilne oblike. Podrobnosti: {0}", e.Message);
                                }
                                if (ssjTokenizer)
                                {
                                    corpus.LoadFromTextSsjTokenizer(content);
                                }
                                else
                                {
                                    corpus.LoadFromText(content);
                                }
                            }
                            int knownWordsCorrect = 0;
                            int knownWordsPosCorrect = 0;
                            int knownWords = 0;
                            int unknownWordsCorrect = 0;
                            int unknownWordsPosCorrect = 0;
                            int unknownWords = 0;
                            int eosCount = 0;
                            int eosCorrect = 0;
                            int lemmaCorrect = 0;
                            int lemmaCorrectLowercase = 0;
                            int lemmaWords = 0;
                            string[] goldTags = new string[corpus.TaggedWords.Count];
                            for (int i = 0; i < corpus.TaggedWords.Count; i++)
                            {
                                goldTags[i] = corpus.TaggedWords[i].Tag;
                            }
                            Tag(corpus, model, featureSpace, suffixTree, lemmatizer, considerTags, out lemmaCorrect, out lemmaCorrectLowercase, out lemmaWords, xmlMode);                            
                            string outputFileName = outputFileOrFolder;
                            if (searchPattern.Contains("*") || searchPattern.Contains("?")) // search pattern contains wildcards thus output is a folder
                            {
                                outputFileName = outputFileOrFolder.TrimEnd('\\') + "\\" + 
                                    file.Name.Substring(0, file.Name.Length - file.Extension.Length) + ".out" + file.Extension;
                                DirectoryInfo dirInfo = new FileInfo(outputFileName).Directory;
                                if (!dirInfo.Exists) { dirInfo.Create(); }
                            }
                            mLogger.Debug(null, "Zapisujem označeno besedilo v datoteko {0} ...", outputFileName);
                            StreamWriter writer = new StreamWriter(outputFileName);
                            writer.Write(corpus.ToString(xmlMode || ssjTokenizer ? "XML-MI" : "XML"));
                            writer.Close();
                            mLogger.Debug(null, "Končano.");
                            if (xmlMode)
                            {
                                for (int i = 0; i < corpus.TaggedWords.Count; i++)
                                {
                                    string wordLower = corpus.TaggedWords[i].WordLower;
                                    string tag = corpus.TaggedWords[i].Tag;
                                    bool isKnown = suffixTree.Contains(wordLower);
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
                                mLogger.Info(null, "Točnost na znanih besedah: ....... {2:0.00}% ({0} / {1})", knownWordsCorrect, knownWords,
                                    (double)knownWordsCorrect / (double)knownWords * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah: ..... {2:0.00}% ({0} / {1})", unknownWordsCorrect, unknownWords,
                                    (double)unknownWordsCorrect / (double)unknownWords * 100.0);
                                mLogger.Info(null, "Skupna točnost: .................. {2:0.00}% ({0} / {1})", allWordsCorrect, allWords,
                                    (double)allWordsCorrect / (double)allWords * 100.0);
                                mLogger.Info(null, "Točnost na znanih besedah (POS):   {2:0.00}% ({0} / {1})", knownWordsPosCorrect, knownWords,
                                    (double)knownWordsPosCorrect / (double)knownWords * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah (POS): {2:0.00}% ({0} / {1})", unknownWordsPosCorrect, unknownWords,
                                    (double)unknownWordsPosCorrect / (double)unknownWords * 100.0);
                                mLogger.Info(null, "Skupna točnost (POS): ............ {2:0.00}% ({0} / {1})", allWordsPosCorrect, allWords,
                                    (double)allWordsPosCorrect / (double)allWords * 100.0);
                                if (lemmatizer != null)
                                {
                                    mLogger.Info(null, "Točnost lematizacije: ............ {2:0.00}% ({0} / {1})", lemmaCorrect, lemmaWords,
                                        (double)lemmaCorrect / (double)lemmaWords * 100.0);
                                    mLogger.Info(null, "Točnost lematizacije (male črke):  {2:0.00}% ({0} / {1})", lemmaCorrectLowercase, lemmaWords,
                                        (double)lemmaCorrectLowercase / (double)lemmaWords * 100.0);
                                }
                                mLogger.Info(null, "Točnost detekcije konca stavka: .. {2:0.00}% ({0} / {1})", eosCorrect, eosCount,
                                    (double)eosCorrect / (double)eosCount * 100.0);
                            }
                        }
                    }                    
                }
            }
            catch (Exception e)
            {
                mLogger.Info(null, "");
                mLogger.Info(null, "*** Nepričakovana napaka. Podrobnosti: {0}", e);
            }
        }
    }
}

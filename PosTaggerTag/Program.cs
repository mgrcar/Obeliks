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
            mLogger.Info(null, "-o                Prepiši obstoječe izhodne datoteke.");
            mLogger.Info(null, "                  (privzeto: ne prepiši obstoječih datotek)");           
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
            ref bool searchSubfolders, ref bool overwrite)
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
                else if (argLwr == "-o")
                {
                    overwrite = true;
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

        private static bool IsXmlTei(string content)
        {
            return content.Contains("<TEI") && content.Contains("<text");
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
            Logger.CustomOutput = new Logger.CustomOutputDelegate(delegate(string loggerName, Logger.Level level, string funcName, Exception e,
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
                    bool ssjTokenizer = false, searchSubfolders = false, verbose = false, overwrite = false;
                    if (ParseParams(args, ref verbose, ref inputFolder, ref searchPattern, ref taggerModelFile, ref lemmatizerModelFile,
                        ref outputFileOrFolder, ref ssjTokenizer, ref searchSubfolders, ref overwrite))
                    {
                        if (!verbose)
                        {
                            mLogger.LocalLevel = Logger.Level.Info;
                            mLogger.LocalProgressOutputType = Logger.ProgressOutputType.Off;
                        }
                        PartOfSpeechTagger tagger = new PartOfSpeechTagger();
                        tagger.LoadModels(taggerModelFile, lemmatizerModelFile);
                        mLogger.Debug(null, "Mapa z vhodnimi datotekami: {0}", inputFolder);
                        mLogger.Debug(null, "Iskalni vzorec: {0}", searchPattern);
                        foreach (FileInfo file in new DirectoryInfo(inputFolder).GetFiles(searchPattern,
                            searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                        {
                            string outputFileName = outputFileOrFolder;
                            if (searchPattern.Contains("*") || searchPattern.Contains("?")) // search pattern contains wildcards thus output is a folder
                            {
                                outputFileName = outputFileOrFolder.TrimEnd('\\') + "\\" +
                                    file.Name.Substring(0, file.Name.Length - file.Extension.Length) + ".out" + file.Extension;
                                DirectoryInfo dirInfo = new FileInfo(outputFileName).Directory;
                                if (!dirInfo.Exists) { dirInfo.Create(); }
                            }
                            if (File.Exists(outputFileName) && !overwrite)
                            {
                                mLogger.Debug(null, "Datoteka {0} že obstaja. Pripadajoča vhodna datoteka ni bila ponovno označena.", outputFileName);
                                continue;
                            }
                            mLogger.Debug(null, "Nalagam {0} ...", file.FullName);
                            Corpus corpus;
                            bool xmlMode = false;
                            try
                            {
                                corpus = new Corpus();
                                corpus.LoadFromXmlFile(file.FullName, /*tagLen=*/-1);
                                if (corpus.TaggedWords.Count > 0)
                                {
                                    xmlMode = true;
                                }
                                else
                                {
                                    corpus.LoadFromGigaFidaFile(file.FullName);
                                }
                            }
                            catch (ThreadHandler.AbortedByUserException)
                            {
                                throw;
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
                            int knownWordsCorrectNoPunct = 0;
                            int knownWordsPosCorrectNoPunct = 0;
                            int knownWordsNoPunct = 0;
                            int unknownWordsCorrectNoPunct = 0;
                            int unknownWordsPosCorrectNoPunct = 0;
                            int unknownWordsNoPunct = 0;
                            string[] goldTags = new string[corpus.TaggedWords.Count];
                            for (int i = 0; i < corpus.TaggedWords.Count; i++)
                            {
                                goldTags[i] = corpus.TaggedWords[i].Tag;
                            }
                            tagger.Tag(corpus, out lemmaCorrect, out lemmaCorrectLowercase, out lemmaWords, xmlMode);
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
                                    bool isKnown = tagger.IsKnownWord(wordLower);
                                    if (!PartOfSpeechTagger.mNonWordRegex.Match(corpus.TaggedWords[i].Word).Success)
                                    {
                                        if (tag == goldTags[i])
                                        {
                                            if (isKnown) { knownWordsCorrectNoPunct++; }
                                            else { unknownWordsCorrectNoPunct++; }
                                        }
                                        if (goldTags[i] != null && tag[0] == goldTags[i][0])
                                        {
                                            if (isKnown) { knownWordsPosCorrectNoPunct++; }
                                            else { unknownWordsPosCorrectNoPunct++; }
                                        }
                                        if (isKnown) { knownWordsNoPunct++; }
                                        else { unknownWordsNoPunct++; }
                                    }
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
                                int allWordsNoPunct = knownWordsNoPunct + unknownWordsNoPunct;
                                int allWordsCorrectNoPunct = knownWordsCorrectNoPunct + unknownWordsCorrectNoPunct;
                                int allWordsPosCorrectNoPunct = knownWordsPosCorrectNoPunct + unknownWordsPosCorrectNoPunct;
                                mLogger.Info(null, "Točnost na znanih besedah: ................... {2:0.00}% ({0} / {1})", knownWordsCorrect, knownWords,
                                    (double)knownWordsCorrect / (double)knownWords * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah: ................. {2:0.00}% ({0} / {1})", unknownWordsCorrect, unknownWords,
                                    (double)unknownWordsCorrect / (double)unknownWords * 100.0);
                                mLogger.Info(null, "Skupna točnost: .............................. {2:0.00}% ({0} / {1})", allWordsCorrect, allWords,
                                    (double)allWordsCorrect / (double)allWords * 100.0);
                                mLogger.Info(null, "Točnost na znanih besedah (POS): ............. {2:0.00}% ({0} / {1})", knownWordsPosCorrect, knownWords,
                                    (double)knownWordsPosCorrect / (double)knownWords * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah (POS): ........... {2:0.00}% ({0} / {1})", unknownWordsPosCorrect, unknownWords,
                                    (double)unknownWordsPosCorrect / (double)unknownWords * 100.0);
                                mLogger.Info(null, "Skupna točnost (POS): ........................ {2:0.00}% ({0} / {1})", allWordsPosCorrect, allWords,
                                    (double)allWordsPosCorrect / (double)allWords * 100.0);
                                mLogger.Info(null, "Točnost na znanih besedah (brez ločil): ...... {2:0.00}% ({0} / {1})", knownWordsCorrectNoPunct, knownWordsNoPunct,
                                    (double)knownWordsCorrectNoPunct / (double)knownWordsNoPunct * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah (brez ločil): .... {2:0.00}% ({0} / {1})", unknownWordsCorrectNoPunct, unknownWordsNoPunct,
                                    (double)unknownWordsCorrectNoPunct / (double)unknownWordsNoPunct * 100.0);
                                mLogger.Info(null, "Skupna točnost (brez ločil): ................. {2:0.00}% ({0} / {1})", allWordsCorrectNoPunct, allWordsNoPunct,
                                    (double)allWordsCorrectNoPunct / (double)allWordsNoPunct * 100.0);
                                mLogger.Info(null, "Točnost na znanih besedah (POS, brez ločil):   {2:0.00}% ({0} / {1})", knownWordsPosCorrectNoPunct, knownWordsNoPunct,
                                    (double)knownWordsPosCorrectNoPunct / (double)knownWordsNoPunct * 100.0);
                                mLogger.Info(null, "Točnost na neznanih besedah (POS, brez ločil): {2:0.00}% ({0} / {1})", unknownWordsPosCorrectNoPunct, unknownWordsNoPunct,
                                    (double)unknownWordsPosCorrectNoPunct / (double)unknownWordsNoPunct * 100.0);
                                mLogger.Info(null, "Skupna točnost (POS, brez ločil): ............ {2:0.00}% ({0} / {1})", allWordsPosCorrectNoPunct, allWordsNoPunct,
                                    (double)allWordsPosCorrectNoPunct / (double)allWordsNoPunct * 100.0);
                                if (lemmatizerModelFile != null)
                                {
                                    mLogger.Info(null, "Točnost lematizacije (brez ločil): ........... {2:0.00}% ({0} / {1})", lemmaCorrect, lemmaWords,
                                        (double)lemmaCorrect / (double)lemmaWords * 100.0);
                                    mLogger.Info(null, "Točnost lematizacije (male črke, brez ločil):  {2:0.00}% ({0} / {1})", lemmaCorrectLowercase, lemmaWords,
                                        (double)lemmaCorrectLowercase / (double)lemmaWords * 100.0);
                                }
                                mLogger.Info(null, "Točnost detekcije konca stavka: .............. {2:0.00}% ({0} / {1})", eosCorrect, eosCount,
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

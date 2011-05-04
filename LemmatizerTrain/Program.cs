using System;
using System.IO;
using Latino;
using LemmaSharp;
using PosTagger;

namespace PosTagger
{
    class Program
    {
        static bool mVerbose
            = false;

        static void OutputHelp()
        {
            Console.WriteLine("*** Lematizator 1.0 - Modul za učenje ***");
            Console.WriteLine();
            Console.WriteLine("Uporaba:");
            Console.WriteLine("LemmatizerTrain [<nastavitve>] <korpus_xml> <model_bin>");
            Console.WriteLine();
            Console.WriteLine("<nastavitve>:  Glej spodaj.");
            Console.WriteLine("<korpus_xml>:  Učni korpus v formatu XML-TEI (vhod).");
            Console.WriteLine("<model_bin>:   Model za lematizacijo (izhod).");
            Console.WriteLine();
            Console.WriteLine("Nastavitve:");
            Console.WriteLine("-v             Izpisovanje na zaslon (verbose).");
            Console.WriteLine("               (privzeto: ni izpisovanja)");
            Console.WriteLine("-t             Upoštevanje oblikoslovnih oznak.");
            Console.WriteLine("               (privzeto: oblikoslovne oznake niso upoštevane)");
            Console.WriteLine("-o             Optimizacija lematizacijskega drevesa (oznake SSJ).");
            Console.WriteLine("               (privzeto: optimizacija se ne izvede)");
        }

        static bool ParseParams(string[] args, ref bool verbose, ref bool considerTag, ref bool treeOpt, ref string corpusFileName, ref string modelFileName)
        {
            // parse
            for (int i = 0; i < args.Length - 2; i++)
            {
                string argLwr = args[i].ToLower();
                if (argLwr == "-v")
                {
                    verbose = true;
                }
                else if (argLwr == "-t")
                {
                    considerTag = true;
                }
                else if (argLwr == "-o")
                {
                    treeOpt = true;
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            corpusFileName = args[args.Length - 2];
            modelFileName = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpusFileName))
            {
                Console.WriteLine("*** Napačno ime datoteke ali datoteka ne obstaja ({0}).\r\n", corpusFileName);
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

        static void Verbose(string text, params object[] args)
        {
            if (mVerbose)
            {
                Console.Write(text, args);
            }
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
                    string corpusFileName = null, modelFileName = null;
                    bool considerTag = false;
                    bool treeOpt = false;
                    if (ParseParams(args, ref mVerbose, ref considerTag, ref treeOpt, ref corpusFileName, ref modelFileName))
                    {
                        Corpus corpus = new Corpus();
                        Verbose("Nalagam učni korpus ...\r\n");
                        corpus.LoadFromXml(corpusFileName, /*tagLen=*/int.MaxValue);
                        Verbose("Gradim model za lematizacijo ...\r\n");
                        LemmatizerSettings lemmatizerSettings = new LemmatizerSettings();
                        lemmatizerSettings.eMsdConsider = considerTag ? LemmatizerSettings.MsdConsideration.Distinct : LemmatizerSettings.MsdConsideration.Ignore;
                        lemmatizerSettings.bUseFromInRules = true;
                        lemmatizerSettings.iMaxRulesPerNode = 0;
                        lemmatizerSettings.bBuildFrontLemmatizer = false;
                        lemmatizerSettings.bStoreAllFullKnownWords = false;
                        lemmatizerSettings.bUseMsdSplitTreeOptimization = treeOpt;
                        Lemmatizer lemmatizer = new Lemmatizer(lemmatizerSettings);     
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                            TaggedWord word = corpus.TaggedWords[i];
                            if (!word.MoreInfo.Punctuation)
                            {
                                lemmatizer.AddExample(word.WordLower, word.Lemma, 1, word.Tag);
                            }
                        }
                        Verbose("\r\n");
                        if (treeOpt)
                        {
                            string msdSpec = Utils.GetManifestResourceString(typeof(Program), "MsdSpecsSloEngCodes.txt");
                            MsdSplitTree.BeamSearchParams beamSearchParams = new MsdSplitTree.BeamSearchParams();
                            beamSearchParams.beamsPerLevel[0] = 2;
                            lemmatizer.BuildModel(msdSpec, beamSearchParams);
                        }
                        else
                        {
                            lemmatizer.BuildModel();
                        }
                        Verbose("Zapisujem model ...\r\n");
                        BinarySerializer writer = new BinarySerializer(modelFileName, FileMode.Create);
                        writer.WriteBool(considerTag);
                        lemmatizer.Save(writer);
                        writer.Close();
                        Verbose("Končano.\r\n");
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

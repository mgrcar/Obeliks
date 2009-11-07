using System;
using System.IO;
using Latino;
using LemmaSharp;
using PosTagger;

namespace PosTagger
{
    class Program
    {
        static bool m_verbose
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
        }

        static bool ParseParams(string[] args, ref bool verbose, ref bool attach_tag, ref string corpus_file_name, ref string model_file_name)
        {
            // parse
            for (int i = 0; i < args.Length - 2; i++)
            {
                string arg_lwr = args[i].ToLower();
                if (arg_lwr == "-v")
                {
                    verbose = true;
                }
                else if (arg_lwr == "-t")
                {
                    attach_tag = true;
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            corpus_file_name = args[args.Length - 2];
            model_file_name = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(corpus_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke ali datoteka ne obstaja ({0}).\r\n", corpus_file_name);
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

        static void Verbose(string text, params object[] args)
        {
            if (m_verbose)
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
                    string corpus_file_name = null, model_file_name = null;
                    bool attach_tag = false;
                    if (ParseParams(args, ref m_verbose, ref attach_tag, ref corpus_file_name, ref model_file_name))
                    {
                        Corpus corpus = new Corpus();
                        Verbose("Nalagam učni korpus ...\r\n");
                        corpus.LoadFromXml(corpus_file_name, /*tag_len=*/1);
                        Verbose("Gradim model za lematizacijo ...\r\n");
                        LemmatizerSettings lemmatizer_settings = new LemmatizerSettings();
                        lemmatizer_settings.eMsdConsider = LemmatizerSettings.MsdConsideration.Ignore;
                        lemmatizer_settings.bUseFromInRules = true; 
                        lemmatizer_settings.iMaxRulesPerNode = 0;
                        Lemmatizer lemmatizer = new Lemmatizer(lemmatizer_settings);                      
                        for (int i = 0; i < corpus.TaggedWords.Count; i++)
                        {
                            Verbose("{0} / {1}\r", i + 1, corpus.TaggedWords.Count);
                            TaggedWord word = corpus.TaggedWords[i];
                            if (!word.MoreInfo.Punctuation)
                            {
                                lemmatizer.AddExample(attach_tag ? string.Format("{0}-{1}", word.WordLower, word.Tag) : word.WordLower, word.Lemma);
                            }
                        }
                        Verbose("\r\n");
                        lemmatizer.BuildModel();                        
                        Verbose("Zapisujem model ...\r\n");
                        BinarySerializer writer = new BinarySerializer(model_file_name, FileMode.Create);
                        writer.WriteBool(attach_tag);
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

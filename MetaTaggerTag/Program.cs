/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          MetaTaggerTag\Program.cs
 *  Version:       1.0
 *  Desc:		   Meta-tagger tagging utility
 *  Author:		   Miha Grcar
 *  Created on:    Jun-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.IO;
using Latino;
using PosTagger;

namespace MetaTagger
{
    static class Program
    {
        static bool m_verbose
            = false;
        static bool m_consistency_check
            = true;

        static void OutputHelp()
        {
            Console.WriteLine("*** Metaoznačevalnik 1.0 - Modul za označevanje ***");
            Console.WriteLine();
            Console.WriteLine("Uporaba:");
            Console.WriteLine("MetaTaggerTag [<nastavitve>] <oznake1_xml> <oznake2_xml> <oznake_tbl> <model> <izhod_xml>");
            Console.WriteLine();
            Console.WriteLine("<nastavitve>:  Glej spodaj.");
            Console.WriteLine("<oznake1_xml>: Izhod prvega označevalnika v formatu XML-TEI (vhod).");
            Console.WriteLine("<oznake2_xml>: Izhod drugega označevalnika v formatu XML-TEI (vhod).");
            Console.WriteLine("<oznake_tbl>:  Oznake v tabelaričnem formatu (vhod).");
            Console.WriteLine("<model>:       Naučeno odločitveno drevo (vhod).");
            Console.WriteLine("<izhod_xml>:   Izhod meta-označevalnika v formatu XML-TEI (izhod).");
            Console.WriteLine();
            Console.WriteLine("Nastavitve:");
            Console.WriteLine("-v             Izpisovanje na zaslon (verbose).");
            Console.WriteLine("               (privzeto: ni izpisovanja)");
            Console.WriteLine("-!c            Brez preverjanja ujemanja besed v vhodnih korpusih.");
            Console.WriteLine("               (privzeto: preverjanje ujemanja besed)");
            Console.WriteLine();
        }

        static bool ParseParams(string[] args, ref bool verbose, ref bool consistency_check, ref string tags_1_file_name, 
            ref string tags_2_file_name, ref string tbl_file_name, ref string tree_file_name, ref string output_file_name)
        {
            // parse
            for (int i = 0; i < args.Length - 5; i++)
            {
                string arg_lwr = args[i].ToLower();
                if (arg_lwr == "-v")
                {
                    verbose = true;
                }
                else if (arg_lwr == "-!c")
                {
                    consistency_check = false;
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            tags_1_file_name = args[args.Length - 5];
            tags_2_file_name = args[args.Length - 4];
            tbl_file_name = args[args.Length - 3];
            tree_file_name = args[args.Length - 2];
            output_file_name = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(tags_1_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke oznak ali datoteka ne obstaja ({0}).\r\n", tags_1_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(tags_2_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke oznak ali datoteka ne obstaja ({0}).\r\n", tags_2_file_name);
                OutputHelp();
                return false;            
            }
            if (!Utils.VerifyFileNameOpen(tbl_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke oznak ali datoteka ne obstaja ({0}).\r\n", tbl_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(tree_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke odločitvenega drevesa ali datoteka ne obstaja ({0}).\r\n", tree_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(output_file_name))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", output_file_name);
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
                if (args.Length < 5)
                {
                    OutputHelp();
                }
                else
                {
                    string tags_1_file_name = null, tags_2_file_name = null, tbl_file_name = null, tree_file_name = null, 
                        output_file_name = null;
                    if (ParseParams(args, ref m_verbose, ref m_consistency_check, ref tags_1_file_name, ref tags_2_file_name, 
                        ref tbl_file_name, ref tree_file_name, ref output_file_name))
                    {
                        Verbose("Nalagam izhod prvega označevalnika ...\r\n");
                        Corpus corpus_1 = new Corpus();
                        corpus_1.LoadFromXml(tags_1_file_name, /*tag_len=*/-1);
                        Verbose("Nalagam izhod drugega označevalnika ...\r\n");
                        Corpus corpus_2 = new Corpus();
                        corpus_2.LoadFromXml(tags_2_file_name, /*tag_len=*/-1);
                        if (!m_consistency_check)
                        {
                            if (corpus_1.TaggedWords.Count != corpus_2.TaggedWords.Count)
                            {
                                Console.WriteLine("*** Napaka! Dolžini vhodnih korpusov se ne ujemata.");
                                return;
                            }
                        }
                        else
                        {
                            Verbose("Preverjam ujemanje besed v vhodnih korpusih ...\r\n");
                            for (int i = 0; i < corpus_1.TaggedWords.Count; i++)
                            {
                                if (corpus_1.TaggedWords[i].Word.ToLower() != corpus_2.TaggedWords[i].Word.ToLower())
                                {
                                    Console.WriteLine("*** Napaka! Besede v vhodnih korpusih se ne ujemajo.");
                                    return;
                                }
                            }
                        }
                        Verbose("Nalagam tabelo oznak ...\r\n");
                        MetaTaggerData.LoadAttributes(tbl_file_name);
                        Verbose("Nalagam odločitveno drevo ...\r\n");
                        Tree tree = new Tree(tree_file_name);
                        Verbose("Označujem besedilo ...\r\n");
                        MetaTaggerData.LoadTestData(corpus_1, corpus_2);
                        for (int i = 0; i < MetaTaggerData.Items.Count; i++)
                        {
                            if (MetaTaggerData.Items[i].Tag1 != MetaTaggerData.Items[i].Tag2)
                            {
                                ArrayList<KeyDat<string, string>> test_example = MetaTaggerData.CreateExample(i);
                                if (tree.Classify(test_example) != 1)
                                {                                 
                                    corpus_1.TaggedWords[i].Lemma = MetaTaggerData.Items[i].Lemma2;
                                    corpus_1.TaggedWords[i].Tag = MetaTaggerData.Items[i].Tag2;
                                }
                            }
                        }
                        Verbose("Pišem izhodno datoteko ...\r\n");
                        StreamWriter writer = new StreamWriter(output_file_name);                        
                        writer.Write(corpus_1.ToString("XML-MI"));
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

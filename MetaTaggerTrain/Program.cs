/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          MetaTaggerTrain\Program.cs
 *  Version:       1.0
 *  Desc:		   Meta-tagger training utility
 *  Author:		   Miha Grcar
 *  Created on:    Jun-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using Latino;

namespace MetaTagger
{
    static class Program
    {
        static bool m_verbose
            = false;

        static void OutputHelp()
        {
            Console.WriteLine("*** Metaoznačevalnik 1.0 - Modul za učenje ***");
            Console.WriteLine();
            Console.WriteLine("Uporaba:");
            Console.WriteLine("MetaTaggerTrain [<nastavitve>] <oznake_tbl> <korpus_tg3> <izhod_orange>");
            Console.WriteLine();
            Console.WriteLine("<nastavitve>:   Glej spodaj.");
            Console.WriteLine("<oznake_tbl>:   Oznake v tabelaričnem formatu (vhod).");
            Console.WriteLine("<korpus_tg3>:   Učni korpus v formatu TG3 (vhod).");
            Console.WriteLine("<izhod_orange>: Izhodna datoteka učnih primerov za Orange (izhod).");
            Console.WriteLine();
            Console.WriteLine("Nastavitve:");
            Console.WriteLine("-v              Izpisovanje na zaslon (verbose).");
            Console.WriteLine("                (privzeto: ni izpisovanja)");
            Console.WriteLine();
        }

        static bool ParseParams(string[] args, ref bool verbose, ref string tbl_file_name, ref string tg3_file_name, ref string orange_file_name)
        {
            // parse
            for (int i = 0; i < args.Length - 3; i++)
            {
                string arg_lwr = args[i].ToLower();
                if (arg_lwr == "-v")
                {
                    verbose = true;
                }
                else
                {
                    Console.WriteLine("*** Napačna nastavitev {0}.\r\n", args[i]);
                    OutputHelp();
                    return false;
                }
            }
            // check file names
            tbl_file_name = args[args.Length - 3];
            tg3_file_name = args[args.Length - 2];
            orange_file_name = args[args.Length - 1];
            if (!Utils.VerifyFileNameOpen(tbl_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke oznak ali datoteka ne obstaja ({0}).\r\n", tbl_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameOpen(tg3_file_name))
            {
                Console.WriteLine("*** Napačno ime datoteke učnega korpusa ali datoteka ne obstaja ({0}).\r\n", tg3_file_name);
                OutputHelp();
                return false;
            }
            if (!Utils.VerifyFileNameCreate(orange_file_name))
            {
                Console.WriteLine("*** Napačno ime izhodne datoteke ({0}).\r\n", orange_file_name);
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
                if (args.Length < 3)
                {
                    OutputHelp();
                }
                else
                {
                    string tbl_file_name = null, tg3_file_name = null, orange_file_name = null;
                    if (ParseParams(args, ref m_verbose, ref tbl_file_name, ref tg3_file_name, ref orange_file_name))
                    {
                        Verbose("Nalagam tabelo oznak ...\r\n");
                        MetaTaggerData.LoadAttributes(tbl_file_name);
                        Verbose("Nalagam učni korpus ...\r\n");
                        MetaTaggerData.LoadData(tg3_file_name);
                        Verbose("Pišem datoteko učnih primerov za Orange ...\r\n");
                        MetaTaggerData.WriteDatasetOrange(orange_file_name, /*null_val=*/"0");
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

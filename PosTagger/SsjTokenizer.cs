/*==========================================================================;
 *
 *  This file is part of SSJ software. See http://www.slovenscina.eu
 *
 *  File:    SsjTokenizer.cs
 *  Desc:    Rule-based tokenizer for Slovene
 *  Created: Dec-2010
 *
 *  Authors: Simon Krek, Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Text.RegularExpressions;
using System.IO;
using Latino;

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class TokenizerRegex
       |
       '-----------------------------------------------------------------------
    */
    internal class TokenizerRegex
    {
        public Regex mRegex;
        public bool mVal;
        public bool mTxt;
        public string mRhs;
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class SsjTokenizer
       |
       '-----------------------------------------------------------------------
    */
    public static class SsjTokenizer
    {
        private static ArrayList<TokenizerRegex> mRules
            = null;
        private static Regex mTagRegex 
            = new Regex(@"\</?[^>]+\>", RegexOptions.Compiled); 

        private static ArrayList<TokenizerRegex> LoadRules()
        {
            Regex splitRegex = new Regex(@"^(?<regex>.*)((--)|(==))\>(?<rhs>.*)$", RegexOptions.Compiled);
            Regex tagRegex = new Regex(@"\</?[^>]+\>", RegexOptions.Compiled);
            ArrayList<TokenizerRegex> rules = new ArrayList<TokenizerRegex>();
            StreamReader rulesReader = new StreamReader(Utils.GetManifestResourceStream(typeof(SsjTokenizer), "SsjTokenizerRules.txt"));
            string line;
            while ((line = rulesReader.ReadLine()) != null)
            {
                if (line.Trim() == "stop") { break; }
                if (!line.StartsWith("#") && line.Trim() != "")
                {
                    RegexOptions opt = RegexOptions.Compiled | RegexOptions.Multiline;
                    if (line.Contains("-->")) { opt |= RegexOptions.IgnoreCase; }
                    TokenizerRegex tknRegex = new TokenizerRegex();
                    tknRegex.mVal = line.Contains("$val");
                    tknRegex.mTxt = line.Contains("$txt");
                    Match match = splitRegex.Match(line);
                    if (match.Success)
                    {
                        try
                        {
                            tknRegex.mRegex = new Regex(match.Result("${regex}").Trim(), opt);
                            tknRegex.mRhs = match.Result("${rhs}").Trim();
                            rules.Add(tknRegex);
                        }
                        catch
                        {
                            Console.WriteLine("*** Warning: Cannot parse line \"{0}\".", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("*** Warning: Cannot parse line \"{0}\".", line);
                    }
                }
            }
            return rules;
        }

        private static string ExecRules(string text)
        {            
            foreach (TokenizerRegex tknRegex in mRules)
            {
                if (!tknRegex.mVal && !tknRegex.mTxt)
                {
                    text = tknRegex.mRegex.Replace(text, tknRegex.mRhs);
                }
                else
                {
                    text = tknRegex.mRegex.Replace(text, delegate(Match m)
                    {
                        string rhs = m.Result(tknRegex.mRhs);
                        if (tknRegex.mVal)
                        {
                            rhs = rhs.Replace("$val", m.Value);
                        }
                        if (tknRegex.mTxt)
                        {
                            rhs = rhs.Replace("$txt", mTagRegex.Replace(m.Value, ""));
                        }
                        return rhs;
                    });
                }
            }
            return text;
        }

        public static string Tokenize(string text)
        {
            Utils.ThrowException(text == null ? new ArgumentNullException("text") : null);
            if (mRules == null) { mRules = LoadRules(); }
            string xml = ExecRules(text);
            return "<text>" + xml + "</text>";
        }
    }
}

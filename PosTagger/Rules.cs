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
 *  File:    Rules.cs
 *  Desc:    Tokenization, tagging, and lemmatization rules for Slovene
 *  Created: Dec-2010
 *
 *  Authors: Miha Grcar, Simon Krek, Kaja Dobrovoljc
 *
 ***************************************************************************/

using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Latino;

/* .-----------------------------------------------------------------------
   |
   |  Class Rules
   |
   '-----------------------------------------------------------------------
*/
public static class Rules
{
    internal class TokenizerRegex
    {
        public Regex mRegex;
        public bool mVal;
        public bool mTxt;
        public string mRhs;
    }

    private static Set<string> mListDmLetter
        = new Set<string>("v,u,o".Split(','));
    private static Set<string> mListDtLetter
        = new Set<string>("v,u".Split(','));
    private static Set<string> mListDoDrLetter
        = new Set<string>("z,s,ž".Split(','));
    private static Set<string> mListDdLetter
        = new Set<string>("k,h".Split(','));
    private static Set<string> mListKrgLetter
        = new Set<string>("i,v,x,l,m,d,c".Split(','));
    private static Set<string> mListNLetter
        = new Set<string>("a,e,i,u".Split(','));

    private static Set<string> mListDi
        = new Set<string>(LoadList("Tagger.ListDi.txt"));
    private static Set<string> mListDr
        = new Set<string>(LoadList("Tagger.ListDr.txt"));
    private static Set<string> mListDd
        = new Set<string>(LoadList("Tagger.ListDd.txt"));
    private static Set<string> mListDt
        = new Set<string>(LoadList("Tagger.ListDt.txt"));
    private static Set<string> mListDm
        = new Set<string>(LoadList("Tagger.ListDm.txt"));
    private static Set<string> mListDo
        = new Set<string>(LoadList("Tagger.ListDo.txt"));
    private static Set<string> mListVp
        = new Set<string>(LoadList("Tagger.ListVp.txt"));
    private static Set<string> mListVd
        = new Set<string>(LoadList("Tagger.ListVd.txt"));
    private static Set<string> mListL
        = new Set<string>(LoadList("Tagger.ListL.txt"));
    private static Set<string> mListZ
        = new Set<string>(LoadList("Tagger.ListZ.txt"));
    private static Set<string> mListO
        = new Set<string>(LoadList("Tagger.ListO.txt"));
    private static ArrayList<string> mListKbPrefix
        = new ArrayList<string>(LoadList("Tagger.ListKbPrefix.txt"));

    private static Set<string> mLemListPpLemma
        = new Set<string>(LoadList("Lemmatizer.ListPpLemma.txt"));
    private static Set<string> mLemListPsLemma
        = new Set<string>(LoadList("Lemmatizer.ListPsLemma.txt"));
    private static Dictionary<string, string> mLemListSoLemma
        = new Dictionary<string, string>();
    private static Set<string> mLemListSSuffix
        = new Set<string>(LoadList("Lemmatizer.ListSSuffix.txt"));
    private static Set<string> mLemListRSuffix
        = new Set<string>(LoadList("Lemmatizer.ListRSuffix.txt"));
    private static Set<string> mLemListPSuffix
        = new Set<string>(LoadList("Lemmatizer.ListPSuffix.txt"));

    //private static ArrayList<string> mLemListTagPrefix
    //    = new ArrayList<string>("N,O,M,L,Kag,Kav,Krg,Krv,Rsn,Rd".Split(','));
    //private static Regex mLemmaTagRegex
    //    = new Regex(@"^((V.)|(D.)|(G..n.*)|(S..ei)|(P.nmein)|(Z..mei.*))$", RegexOptions.Compiled);

    static Set<string> mAbbrvAll
        = new Set<string>(LoadList("Tokenizer.ListOAll.txt"));
    static Set<string> mAbbrvAllCS
        = new Set<string>(LoadList("Tokenizer.ListOAllCS.txt"));
    static Set<string> mAbbrvExcl
        = new Set<string>(LoadList("Tokenizer.ListOExcl.txt"));
    static Set<string> mAbbrvExclCS
        = new Set<string>(LoadList("Tokenizer.ListOExclCS.txt"));
    static Set<string> mAbbrvSeq
        = new Set<string>(LoadList("Tokenizer.ListOSeq.txt"));
    static Set<string> mAbbrvSeg
        = new Set<string>(LoadList("Tokenizer.ListOSeg.txt"));
    static Set<string> mAbbrvSegSeq
        = new Set<string>(LoadList("Tokenizer.ListOSegSeq.txt"));
    static Set<string> mAbbrvNoSegSeq
        = new Set<string>(LoadList("Tokenizer.ListONoSegSeq.txt"));

    static Regex mAbbrvExclRegex
        = new Regex(@"(?<step><w>(?<word>\p{L}+)</w><c>\.</c>(?<tail><S/>)?)(?<ctx>(</[ps]>)|(<[wc]>.))", RegexOptions.Compiled);
    static Regex mAbbrvOtherRegex
        = new Regex(@"(?<step><w>(?<word>\p{L}+)</w><c>\.</c>(?<tail><S/>)?)<[wc]>[:,;0-9\p{Ll}]", RegexOptions.Compiled);
    static Regex mAbbrvRegex
        = new Regex(@"<w>(\p{L}+)</w><c>\.</c>", RegexOptions.Compiled);
    static Regex mEndOfSentenceRegex
        = new Regex(@"^<[wc]>[\p{Lu}""»“‘'0-9]$", RegexOptions.Compiled);    
    
    private static Regex mKrRegex
        = new Regex(@"^[ivxlmdc]+\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mPRRegex
        = new Regex(@"^[\p{L}\-–—']+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mSRegex
        = new Regex(@"^[\p{L}0-9\-–—']+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mAcronymRegex
        = new Regex(@"^(?<acronym>\p{Lu}+)[-–—](?<suffix>\p{Ll}+)$", RegexOptions.Compiled);

    private static Regex mTagRegex
        = new Regex(@"\</?[^>]+\>", RegexOptions.Compiled); 
    private static ArrayList<TokenizerRegex> mTokRulesPart1
        = LoadRules("Tokenizer.TokRulesPart1.txt");
    private static ArrayList<TokenizerRegex> mTokRulesPart2
        = LoadRules("Tokenizer.TokRulesPart2.txt");

    static ArrayList<int> mAbbrvSeqLen;

    private static MultiSet<string> mTagStats
        = new MultiSet<string>();

    static Rules()
    {
        string[] tagStats = LoadList("Tagger.TagStats.txt");
        foreach (string item in tagStats)
        {
            string[] tmp = item.Split(':');
            mTagStats.Add(tmp[0], Convert.ToInt32(tmp[1]));
        }
        string[] lemmaList = LoadList("Lemmatizer.ListSoLemma.txt");
        foreach (string lemma in lemmaList)
        {
            mLemListSoLemma.Add(lemma.ToLower(), lemma);
        }
        Set<int> lengths = new Set<int>();
        foreach (string abbrv in mAbbrvSeq)
        {
            int len = 0;
            foreach (char ch in abbrv)
            {
                if (ch == '.') { len++; }
            }
            lengths.Add(len);
        }
        mAbbrvSeqLen = new ArrayList<int>(lengths);
        mAbbrvSeqLen.Sort(DescSort<int>.Instance);
    }

    private static ArrayList<TokenizerRegex> LoadRules(string resName)
    {
        Regex splitRegex = new Regex(@"^(?<regex>.*)((--)|(==))\>(?<rhs>.*)$", RegexOptions.Compiled);
        ArrayList<TokenizerRegex> rules = new ArrayList<TokenizerRegex>();
        StreamReader rulesReader = new StreamReader(Utils.GetManifestResourceStream(typeof(Rules), resName));
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

    private static string ExecRules(string text, ArrayList<TokenizerRegex> rules)
    {
        foreach (TokenizerRegex tknRegex in rules)
        {
            if (!tknRegex.mVal && !tknRegex.mTxt)
            {
                text = tknRegex.mRegex.Replace(text, tknRegex.mRhs);
            }
            else
            {
                text = tknRegex.mRegex.Replace(text, delegate(Match m) {
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

    static string ProcessAbbrvSeq(string txt, int seqLen)
    {
        int idx = 0;
        StringBuilder s = new StringBuilder();
        Regex regex = new Regex(@"(?<jump>(?<step><w>\p{L}+</w><c>\.</c>(<S/>)?)(<w>\p{L}+</w><c>\.</c>(<S/>)?){" + (seqLen - 1) + @"})(?<ctx>(</[ps]>)|(<[wc]>.))", RegexOptions.Compiled);
        Match m = regex.Match(txt);
        while (m.Success)
        {
            s.Append(txt.Substring(idx, m.Index - idx));
            string xml = m.Result("${jump}");
            string abbrvLower = mTagRegex.Replace(xml, "").Replace(" ", "").ToLower();
            if (mAbbrvSeq.Contains(abbrvLower))
            {
                idx = m.Index + xml.Length;
                xml = mAbbrvRegex.Replace(xml, "<w>$1.</w>");
                if (mEndOfSentenceRegex.Match(m.Result("${ctx}")).Success)
                {
                    if (mAbbrvSegSeq.Contains(abbrvLower))
                    {
                        xml = xml + "</s><s>";
                    }
                    else if (mAbbrvNoSegSeq.Contains(abbrvLower))
                    {
                        xml += "<!s/>";
                    }
                }
            }
            else
            {
                xml = m.Result("${step}");
                idx = m.Index + xml.Length;
            }
            s.Append(xml);
            m = regex.Match(txt, idx);
        }
        s.Append(txt.Substring(idx, txt.Length - idx));
        return s.ToString();
    }

    static string ProcessAbbrvExcl(string txt)
    {
        int idx = 0;
        StringBuilder s = new StringBuilder();
        Match m = mAbbrvExclRegex.Match(txt);
        while (m.Success)
        {
            s.Append(txt.Substring(idx, m.Index - idx));
            string xml;
            string word = m.Result("${word}");
            string wordLower = word.ToLower();
            if (word.Length == 1 || mAbbrvExcl.Contains(wordLower) || mAbbrvExclCS.Contains(word))
            {
                xml = m.Result("<w>${word}.</w>${tail}");
                idx = m.Index + m.Result("${step}").Length;
                if (mAbbrvSeg.Contains(wordLower) && mEndOfSentenceRegex.Match(m.Result("${ctx}")).Success)
                {
                    xml += "</s><s>";
                }
            }
            else
            {
                xml = m.Result("${step}");
                idx = m.Index + xml.Length;
            }
            s.Append(xml);
            m = mAbbrvExclRegex.Match(txt, idx);
        }
        s.Append(txt.Substring(idx, txt.Length - idx));
        return s.ToString();
    }

    static string ProcessAbbrvOther(string txt)
    {
        int idx = 0;
        StringBuilder s = new StringBuilder();
        Match m = mAbbrvOtherRegex.Match(txt);
        while (m.Success)
        {
            s.Append(txt.Substring(idx, m.Index - idx));
            string xml;
            string word = m.Result("${word}");
            string wordLower = word.ToLower();
            if (mAbbrvAll.Contains(wordLower) || mAbbrvAllCS.Contains(word))
            {
                xml = m.Result("<w>${word}.</w>${tail}");
                idx = m.Index + m.Result("${step}").Length;
            }
            else
            {
                xml = m.Result("${step}");
                idx = m.Index + xml.Length;
            }
            s.Append(xml);
            m = mAbbrvOtherRegex.Match(txt, idx);
        }
        s.Append(txt.Substring(idx, txt.Length - idx));
        return s.ToString();
    }

    public static string Tokenize(string text)
    {
        Utils.ThrowException(text == null ? new ArgumentNullException("text") : null);
        string xml = ExecRules(text, mTokRulesPart1);
        foreach (int len in mAbbrvSeqLen)
        {
            xml = ProcessAbbrvSeq(xml, len);
        }
        xml = ProcessAbbrvExcl(xml);
        xml = ProcessAbbrvOther(xml);
        xml = ExecRules(xml, mTokRulesPart2);
        xml = xml.Replace("<!s/>", "");
        return "<text>" + xml + "</text>";
    }

    private static string[] LoadList(string name)
    {
        ArrayList<string> list = new ArrayList<string>();
        string listStr = Utils.GetManifestResourceString(typeof(Rules), name);
        foreach (string line in listStr.Split('\n'))
        {
            string token = line.Trim();
            if (token != "" && !token.StartsWith("#"))
            {
                list.Add(token);
            }
        }
        return list.ToArray();
    }

    public static string GetMostFrequentTag(IEnumerable<string> tagList)
    {
        string maxFreqTag = null;
        int maxFreq = 0;
        foreach (string tag in tagList)
        {
            int freq = mTagStats.GetCount(tag);
            if (freq > maxFreq) { freq = maxFreq; maxFreqTag = tag; }
        }
        return maxFreqTag;
    }

    private static bool StartsWith(string str, IEnumerable<string> list)
    {
        foreach (string prefix in list)
        {
            if (str.StartsWith(prefix)) { return true; }
        }
        return false;
    }

    private static void CopyTags(Set<string> to, params string[] patterns)
    {
        foreach (string pattern in patterns)
        {
            foreach (KeyValuePair<string, int> item in mTagStats)
            {
                if (item.Key.StartsWith(pattern)) { to.Add(item.Key); }
            }
        }
    }

    private static void RemoveTags(Set<string> tags, params string[] patterns)
    {
        foreach (string pattern in patterns)
        {
            foreach (string tag in tags.ToArray())
            {
                if (tag.StartsWith(pattern)) { tags.Remove(tag); }
            }
        }
    }

    private static void CountChars(string word, out int numLetters, out int numDigits)
    {
        numLetters = 0;
        numDigits = 0;
        foreach (char ch in word)
        {
            if (char.IsLetter(ch)) { numLetters++; }
            else if (char.IsDigit(ch)) { numDigits++; }
        }
    }

    public static Set<string> ApplyTaggerRules(Set<string> filter, string word, out string rule)
    {
        string wordLower = word.ToLower();
        Set<string> newFilter = new Set<string>();
        int numLetters, numDigits;
        CountChars(word, out numLetters, out numDigits);
        rule = "";
        // *** Part 1 ***
        if (word.Length == 1 && numLetters == 1)
        {
            rule = "p1_r1";
            CopyTags(newFilter, "Some");
            if (mListDmLetter.Contains(wordLower)) { newFilter.Add("Dm"); rule += " p1_r1a-1"; }
            if (mListDtLetter.Contains(wordLower)) { newFilter.Add("Dt"); rule += " p1_r1a-2"; }
            if (mListDoDrLetter.Contains(wordLower)) { newFilter.Add("Do"); newFilter.Add("Dr"); rule += " p1_r1a-3"; }
            if (mListDdLetter.Contains(wordLower)) { newFilter.Add("Dd"); rule += " p1_r1a-4"; }
            if (wordLower == "a") { newFilter.Add("Vp"); newFilter.Add("Rsn"); rule += " p1_r1b"; }
            if (wordLower == "à") { newFilter.Add("Rsn"); rule += " p1_r1c"; }
            if (mListKrgLetter.Contains(wordLower)) { newFilter.Add("Krg"); rule += " p1_r1d"; }
            if (mListNLetter.Contains(wordLower)) { newFilter.Add("N"); rule += " p1_r1e"; }
        }
        else if (numLetters > 0 && numDigits > 0 && numLetters + numDigits == word.Length)
        {
            rule = "p1_r2";
            CopyTags(newFilter, "S");
            newFilter.Add("N");
            newFilter.Add("Kag");
        }
        else if (numDigits > 0 && numLetters == 0)
        {
            rule = "p1_r3";
            if (word.EndsWith(".")) { newFilter.Add("Kav"); }
            else { newFilter.Add("Kag"); }
        }
        else if (word.Contains("."))
        {
            rule = "p1_r4";
            if (mKrRegex.Match(word).Success) { newFilter.Add("Krv"); }
            if (numLetters == word.Length - 1 && word.EndsWith(".")) { newFilter.Add("O"); }
            if (!word.EndsWith(".")) { newFilter.Add("N"); }
        }
        else
        {
            rule = "p1_r5";
            newFilter = filter;
        }
        // *** Part 2 ***
        if (!mListDi.Contains(wordLower)) { newFilter.Remove("Di"); rule += " p2_r1-1"; }
        if (!mListDr.Contains(wordLower)) { newFilter.Remove("Dr"); rule += " p2_r1-2"; }
        if (!mListDd.Contains(wordLower)) { newFilter.Remove("Dd"); rule += " p2_r1-3"; }
        if (!mListDt.Contains(wordLower)) { newFilter.Remove("Dt"); rule += " p2_r1-4"; }
        if (!mListDm.Contains(wordLower)) { newFilter.Remove("Dm"); rule += " p2_r1-5"; }
        if (!mListDo.Contains(wordLower)) { newFilter.Remove("Do"); rule += " p2_r1-6"; }
        if (!mListVp.Contains(wordLower)) { newFilter.Remove("Vp"); rule += " p2_r2-1"; }
        if (!mListVd.Contains(wordLower)) { newFilter.Remove("Vd"); rule += " p2_r2-2"; }
        if (!mListL.Contains(wordLower)) { newFilter.Remove("L"); rule += " p2_r3"; }
        if (!mListZ.Contains(wordLower)) { RemoveTags(newFilter, "Z"); rule += " p2_r4"; }
        if (numLetters != word.Length) { newFilter.Remove("M"); RemoveTags(newFilter, "G"); rule += " p2_r5"; }
        if (!mPRRegex.Match(word).Success) { RemoveTags(newFilter, "P", "R"); rule += " p2_r6"; }
        if (!mSRegex.Match(word).Success) { RemoveTags(newFilter, "S"); rule += " p2_r7"; }
        if (!mListO.Contains(wordLower) && !(numLetters == 1 && word.Length == 2 && word.EndsWith("."))) { newFilter.Remove("O"); rule += " p2_r8"; }
        if (!mKrRegex.Match(word).Success) { RemoveTags(newFilter, "Kr"); rule += " p2_r9"; }
        if (!StartsWith(wordLower, mListKbPrefix)) { RemoveTags(newFilter, "Kb"); rule += " p2_r10"; }
        if (!char.IsDigit(word[0])) { RemoveTags(newFilter, "Ka"); rule += " p2_r11"; }
        return newFilter;
    }

    public static string ApplyLemmaRules(string lemma, string word, string tag)
    {
        //if (mLemListTagPrefix.Contains(tag) || mLemmaTagRegex.Match(tag).Success)
        //{
        //    if (lemma != word.ToLower()) { Console.WriteLine(word + " " + lemma + " " + tag); }
        //    lemma = word.ToLower();
        //}
        if (word.Length >= 1)
        {            
            Utils.CaseType caseType = Utils.GetCaseType(word);
            bool isFirstCap = caseType == Utils.CaseType.Abc || caseType == Utils.CaseType.AbC || caseType == Utils.CaseType.ABC;
            bool isAllCaps = caseType == Utils.CaseType.ABC;
            if (tag.StartsWith("R"))
            {
                Match m = mAcronymRegex.Match(word);
                if (m.Success && mLemListRSuffix.Contains(m.Result("${suffix}"))) 
                {
                    //Console.WriteLine(word + " " + m.Result("${acronym}"));
                    return m.Result("${acronym}");
                }
            }
            else if (tag.StartsWith("Kr"))
            {
                if (isAllCaps) { return lemma.ToUpper(); }
            }
            else if (tag.StartsWith("Pp") && mLemListPpLemma.Contains(lemma))
            {
                return char.ToUpper(lemma[0]) + lemma.Substring(1);
            }
            else if (tag.StartsWith("Ps"))
            {
                Match m = mAcronymRegex.Match(word);
                if (m.Success && mLemListPSuffix.Contains(m.Result("${suffix}")))
                {
                    //Console.WriteLine(word + " " + m.Result("${acronym}"));
                    return m.Result("${acronym}");
                }
                else if (mLemListPsLemma.Contains(lemma))
                {
                    return lemma;
                }
                else if (isFirstCap && lemma.Length >= 1)
                {
                    return char.ToUpper(lemma[0]) + lemma.Substring(1);
                }
            }
            else if (tag.StartsWith("So"))
            {
                if (word.Length == 1) 
                { 
                    return word; 
                }
                else if (mLemListSoLemma.ContainsKey(lemma))
                {
                    //Console.WriteLine(mLemListSoLemma[lemma]);
                    return mLemListSoLemma[lemma];
                }
                else // *** check with Simon/Kaja if this is OK
                {
                    Match m = mAcronymRegex.Match(word);
                    if (m.Success && mLemListSSuffix.Contains(m.Result("${suffix}")))
                    {
                        //Console.WriteLine(word + " " + m.Result("${acronym}"));
                        return m.Result("${acronym}");
                    }
                }
            }
            else if (tag.StartsWith("Sl"))
            {
                Match m = mAcronymRegex.Match(word);
                if (m.Success && mLemListSSuffix.Contains(m.Result("${suffix}")))
                {
                    //Console.WriteLine(word + " " + m.Result("${acronym}"));
                    return m.Result("${acronym}");
                }
                else if (isAllCaps)
                {
                    return lemma.ToUpper();
                }
                else if (isFirstCap && lemma.Length >= 1)
                {
                    return char.ToUpper(lemma[0]) + lemma.Substring(1);
                }
            }
        }
        return lemma;
    }
}
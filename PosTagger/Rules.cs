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
 *  Desc:    Language-specific rules for tagging (Slovene language)
 *  Created: Dec-2011
 *
 *  Author:  Miha Grcar
 *
 ***************************************************************************/

using System;
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

    private static MultiSet<string> mTagStats
        = new MultiSet<string>();

    private static Regex mRegexKr
        = new Regex(@"^[ivxlmdc]+\.?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mRegexPR
        = new Regex(@"^[\p{L}\-–—']+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mRegexS
        = new Regex(@"^[\p{L}0-9\-–—']+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex mAcronymRegex
        = new Regex(@"^(?<acronym>\p{Lu}+)[-–—](?<suffix>\p{Ll}+)$", RegexOptions.Compiled);

    static Rules()
    {
        string[] tagStats = LoadList("TagStats.txt");
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
            if (mRegexKr.Match(word).Success) { newFilter.Add("Krv"); }
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
        if (!mRegexPR.Match(word).Success) { RemoveTags(newFilter, "P", "R"); rule += " p2_r6"; }
        if (!mRegexS.Match(word).Success) { RemoveTags(newFilter, "S"); rule += " p2_r7"; }
        if (!mListO.Contains(wordLower) && !(numLetters == 1 && word.Length == 2 && word.EndsWith("."))) { newFilter.Remove("O"); rule += " p2_r8"; }
        if (!mRegexKr.Match(word).Success) { RemoveTags(newFilter, "Kr"); rule += " p2_r9"; }
        if (!StartsWith(wordLower, mListKbPrefix)) { RemoveTags(newFilter, "Kb"); rule += " p2_r10"; }
        if (!char.IsDigit(word[0])) { RemoveTags(newFilter, "Ka"); rule += " p2_r11"; }
        return newFilter;
    }

    public static string FixLemmaCase(string lemma, string word, string tag)
    {
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
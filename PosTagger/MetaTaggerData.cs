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
 *  File:    MetaTaggerData.cs
 *  Desc:    Meta-tagger dataset 
 *  Created: Jun-2009
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using Latino;
using PosTagger;

namespace MetaTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class MetaTaggerDataEntry
       |
       '-----------------------------------------------------------------------
    */
    public class MetaTaggerDataEntry
    {
        private string mWord;
        private string mGoldTag;
        private string mTag1;
        private string mTag2;
        private string mLemma1;
        private string mLemma2;

        internal MetaTaggerDataEntry(string word, string goldTag, string tag1, string tag2, string lemma1, string lemma2)
        {
            mWord = word;
            mGoldTag = goldTag;
            mTag1 = tag1;
            mTag2 = tag2;
            mLemma1 = lemma1;
            mLemma2 = lemma2;
        }

        internal MetaTaggerDataEntry(string word, string goldTag, string tag1, string tag2) : this(word, goldTag, tag1, tag2, /*lemma1=*/null, /*lemma2=*/null)
        {
        }

        public string Word
        {
            get { return mWord; }
        }

        public string GoldTag
        {
            get { return mGoldTag; }
        }

        public string Tag1
        {
            get { return mTag1; }
        }

        public string Tag2
        {
            get { return mTag2; }
        }

        public string Lemma1
        {
            get { return mLemma1; }
        }

        public string Lemma2
        {
            get { return mLemma2; }
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class MetaTaggerData
       |
       '-----------------------------------------------------------------------
    */
    public static class MetaTaggerData
    {
        private static ArrayList<MetaTaggerDataEntry> mItems
            = new ArrayList<MetaTaggerDataEntry>();
        private static Dictionary<string, Dictionary<string, string>> mAttr
            = new Dictionary<string, Dictionary<string, string>>();
        private static Set<string> mAttrSet
            = new Set<string>();

        public static Set<string>.ReadOnly AttrSet
        {
            get { return mAttrSet; }
        }

        public static ArrayList<MetaTaggerDataEntry>.ReadOnly Items
        {
            get { return mItems; }
        }

        public static void LoadAttributes(string fileName)
        {
            StreamReader reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(' ', '\t');
                Dictionary<string, string> attrValDict = new Dictionary<string, string>();
                if (fields.Length >= 1)
                {
                    mAttr.Add(fields[0], attrValDict);
                }
                for (int i = 2; i < fields.Length; i++)
                {
                    string[] attrVal = fields[i].Split('=');
                    if (attrVal.Length != 2) { throw new InvalidDataException(); }
                    attrValDict.Add(attrVal[0], attrVal[1]);
                    mAttrSet.Add(attrVal[0]);
                }
            }
            reader.Close();
        }

        public static void LoadData(string fileName)
        { 
            StreamReader reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(' ', '\t');
                if (fields.Length == 4)
                { 
                    string word = fields[0];
                    string goldTag = fields[1];
                    string tag1 = fields[2];
                    string tag2 = fields[3];
                    if (mAttr.ContainsKey(tag1) && mAttr.ContainsKey(tag2)) // *** make sure to load attributes first
                    {
                        mItems.Add(new MetaTaggerDataEntry(word, goldTag, tag1, tag2));
                    }
                    else if (word == goldTag && goldTag == tag1 && tag1 == tag2) // punctuation
                    {
                        mItems.Add(new MetaTaggerDataEntry(word, goldTag, tag1, tag2));
                        mAttr.Add(goldTag, new Dictionary<string, string>());
                    }
                }
            }
        }

        public static void LoadTestData(Corpus corpus1, Corpus corpus2)
        {
            for (int i = 0; i < corpus1.TaggedWords.Count; i++)
            {
                string word = corpus1.TaggedWords[i].Word;
                string tag1 = corpus1.TaggedWords[i].Tag;
                string tag2 = corpus2.TaggedWords[i].Tag;
                string lemma1 = corpus1.TaggedWords[i].Lemma;
                string lemma2 = corpus2.TaggedWords[i].Lemma;
                if (tag1 == null) { tag1 = tag2; }
                if (tag2 == null) { tag2 = tag1; }
                if (mAttr.ContainsKey(tag1) && mAttr.ContainsKey(tag2)) // *** make sure to load attributes first
                {
                    mItems.Add(new MetaTaggerDataEntry(word, /*goldTag=*/null, tag1, tag2, lemma1, lemma2));
                }
                else
                {
                    mItems.Add(new MetaTaggerDataEntry(word, /*goldTag=*/null, tag1, tag1, lemma1, lemma1));
                    if (!mAttr.ContainsKey(tag1))
                    {
                        mAttr.Add(tag1, new Dictionary<string, string>());
                    }
                }
            }
        }

        public static ArrayList<KeyDat<string, string>> CreateExample(int idx)
        {
            MetaTaggerDataEntry prevEntry = idx > 0 ? mItems[idx - 1] : null;
            MetaTaggerDataEntry entry = mItems[idx];
            MetaTaggerDataEntry nextEntry = idx < mItems.Count - 1 ? mItems[idx + 1] : null;
            ArrayList<KeyDat<string, string>> example = new ArrayList<KeyDat<string, string>>();
            Dictionary<string, string> attrVal1;
            Dictionary<string, string> attrVal2;
            // previous word
            if (prevEntry != null)
            {
                attrVal1 = mAttr[prevEntry.Tag1];
                attrVal2 = mAttr[prevEntry.Tag2];
                if (prevEntry.Tag1.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Prev_POS_1", prevEntry.Tag1[0].ToString()));
                }
                if (prevEntry.Tag2.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Prev_POS_2", prevEntry.Tag2[0].ToString()));
                }
                foreach (string attr in mAttrSet)
                {
                    if (attrVal1.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Prev_{0}_1", attr), attrVal1[attr]));
                    }
                    if (attrVal2.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Prev_{0}_2", attr), attrVal2[attr]));
                    }
                }
            }
            // current word
            attrVal1 = mAttr[entry.Tag1];
            attrVal2 = mAttr[entry.Tag2];
            string pos1 = entry.Tag1.Length > 0 ? entry.Tag1[0].ToString() : null;
            string pos2 = entry.Tag2.Length > 0 ? entry.Tag2[0].ToString() : null;
            if (pos1 != null)
            {
                example.Add(new KeyDat<string, string>("POS_1", pos1));
            }
            if (pos2 != null)
            {
                example.Add(new KeyDat<string, string>("POS_2", pos2));
            }
            example.Add(new KeyDat<string, string>("Agree_POS", pos1 == pos2 ? "yes" : "no"));
            foreach (string attr in mAttrSet)
            {
                string attr1 = attrVal1.ContainsKey(attr) ? attrVal1[attr] : null;
                string attr2 = attrVal2.ContainsKey(attr) ? attrVal2[attr] : null;
                if (attr1 != null)
                {
                    example.Add(new KeyDat<string, string>(string.Format("{0}_1", attr), attr1));
                }
                if (attr2 != null)
                {
                    example.Add(new KeyDat<string, string>(string.Format("{0}_2", attr), attr2));
                }
                example.Add(new KeyDat<string, string>(string.Format("Agree_{0}", attr), attr1 == attr2 ? "yes" : "no"));
            }
            // next word
            if (nextEntry != null)
            {
                attrVal1 = mAttr[nextEntry.Tag1];
                attrVal2 = mAttr[nextEntry.Tag2];
                if (nextEntry.Tag1.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Next_POS_1", nextEntry.Tag1[0].ToString()));
                }
                if (nextEntry.Tag2.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Next_POS_2", nextEntry.Tag2[0].ToString()));
                }
                foreach (string attr in mAttrSet)
                {
                    if (attrVal1.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Next_{0}_1", attr), attrVal1[attr]));
                    }
                    if (attrVal2.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Next_{0}_2", attr), attrVal2[attr]));
                    }
                }
            }
            return example;
        }

        public static void WriteDatasetOrange(string fileName, string nullVal)
        {
            StreamWriter writer = new StreamWriter(fileName);
            StringBuilder line = new StringBuilder();            
            line.Append("Tagger\t");
            line.Append("Prev_POS_1\tPrev_POS_2\t");
            int c = 3;
            foreach (string attr in mAttrSet)
            {
                line.Append(string.Format("Prev_{0}_1\t", attr));
                line.Append(string.Format("Prev_{0}_2\t", attr));
                c += 2;
            }
            line.Append("POS_1\tPOS_2\tAgree_POS\t");
            c += 3;
            foreach (string attr in mAttrSet)
            {
                line.Append(string.Format("{0}_1\t", attr));                
                line.Append(string.Format("{0}_2\t", attr));
                line.Append(string.Format("Agree_{0}\t", attr));
                c += 3;
            }
            line.Append("Next_POS_1\tNext_POS_2\t");
            c += 2;
            foreach (string attr in mAttrSet)
            {
                line.Append(string.Format("Next_{0}_1\t", attr));
                line.Append(string.Format("Next_{0}_2\t", attr));
                c += 2;
            }
            writer.WriteLine(line.ToString().TrimEnd('\t'));
            line = new StringBuilder();
            for (int i = 0; i < c; i++)
            {
                line.Append("discrete\t");
            }
            writer.WriteLine(line.ToString().TrimEnd('\t'));
            writer.WriteLine("class");
            for (int i = 0; i < mItems.Count; i++)
            {
                Dictionary<string, string> attrVal1;
                Dictionary<string, string> attrVal2;
                MetaTaggerDataEntry entry = mItems[i];
                if (entry.Tag1 != entry.Tag2 && (entry.GoldTag == entry.Tag1 || entry.GoldTag == entry.Tag2)) // the two taggers disagree, one of them is correct
                {
                    line = new StringBuilder();
                    line.Append(entry.GoldTag == entry.Tag1 ? "Tagger1" : "Tagger2");
                    line.Append("\t");
                    // previous word
                    if (i == 0)
                    {
                        c = 2 + mAttrSet.Count * 2;
                        for (int j = 0; j < c; j++)
                        {
                            line.Append(nullVal);
                            line.Append("\t");
                        }
                    }
                    else
                    {
                        MetaTaggerDataEntry prevEntry = mItems[i - 1];
                        string prevPos1 = prevEntry.Tag1.Length > 0 ? prevEntry.Tag1[0].ToString() : nullVal;
                        string prevPos2 = prevEntry.Tag2.Length > 0 ? prevEntry.Tag2[0].ToString() : nullVal;
                        line.Append(prevPos1);
                        line.Append("\t");
                        line.Append(prevPos2);
                        line.Append("\t");
                        attrVal1 = mAttr[prevEntry.Tag1];
                        attrVal2 = mAttr[prevEntry.Tag2];
                        foreach (string attr in mAttrSet)
                        {
                            line.Append(attrVal1.ContainsKey(attr) ? attrVal1[attr] : nullVal);
                            line.Append("\t");
                            line.Append(attrVal2.ContainsKey(attr) ? attrVal2[attr] : nullVal);
                            line.Append("\t");
                        }
                    }
                    // current word
                    string pos1 = entry.Tag1.Length > 0 ? entry.Tag1[0].ToString() : nullVal;
                    string pos2 = entry.Tag2.Length > 0 ? entry.Tag2[0].ToString() : nullVal;
                    line.Append(pos1);
                    line.Append("\t");
                    line.Append(pos2);
                    line.Append("\t");
                    line.Append(pos1 == pos2 ? "yes" : "no");
                    line.Append("\t");
                    attrVal1 = mAttr[entry.Tag1];
                    attrVal2 = mAttr[entry.Tag2];
                    foreach (string attr in mAttrSet)
                    {
                        string attr1 = attrVal1.ContainsKey(attr) ? attrVal1[attr] : nullVal;
                        string attr2 = attrVal2.ContainsKey(attr) ? attrVal2[attr] : nullVal;
                        line.Append(attr1);
                        line.Append("\t");
                        line.Append(attr2);
                        line.Append("\t");
                        line.Append(attr1 == attr2 ? "yes" : "no");
                        line.Append("\t");
                    }
                    // next word
                    if (i == mItems.Count - 1)
                    {
                        c = 2 + mAttrSet.Count * 2;
                        for (int j = 0; j < c; j++)
                        {
                            line.Append(nullVal);
                            line.Append("\t");
                        }
                    }
                    else
                    {
                        MetaTaggerDataEntry nextEntry = mItems[i + 1];
                        string nextPos1 = nextEntry.Tag1.Length > 0 ? nextEntry.Tag1[0].ToString() : nullVal;
                        string nextPos2 = nextEntry.Tag2.Length > 0 ? nextEntry.Tag2[0].ToString() : nullVal;
                        line.Append(nextPos1);
                        line.Append("\t");
                        line.Append(nextPos2);
                        line.Append("\t");
                        attrVal1 = mAttr[nextEntry.Tag1];
                        attrVal2 = mAttr[nextEntry.Tag2];
                        foreach (string attr in mAttrSet)
                        {
                            line.Append(attrVal1.ContainsKey(attr) ? attrVal1[attr] : nullVal);
                            line.Append("\t");
                            line.Append(attrVal2.ContainsKey(attr) ? attrVal2[attr] : nullVal);
                            line.Append("\t");
                        }
                    }
                    writer.WriteLine(line.ToString().TrimEnd('\t'));
                }
            }
            writer.Close();
        }
    }
}

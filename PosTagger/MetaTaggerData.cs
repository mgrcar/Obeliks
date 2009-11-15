/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          MetaTaggerData.cs
 *  Version:       1.0
 *  Desc:		   Meta-tagger dataset utilities
 *  Author:		   Miha Grcar
 *  Created on:    Jun-2009
 *  Last modified: Sep-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using Latino;
using PosTagger;

namespace MetaTagger
{
    public static class MetaTaggerData
    {
        public class DataEntry
        {
            private string m_word;
            private string m_gold_tag;
            private string m_tag_1;
            private string m_tag_2;
            private string m_lemma_1;
            private string m_lemma_2;
            public DataEntry(string word, string gold_tag, string tag_1, string tag_2, string lemma_1, string lemma_2)
            {
                m_word = word;
                m_gold_tag = gold_tag;
                m_tag_1 = tag_1;
                m_tag_2 = tag_2;
                m_lemma_1 = lemma_1;
                m_lemma_2 = lemma_2;
            }
            public DataEntry(string word, string gold_tag, string tag_1, string tag_2) : this(word, gold_tag, tag_1, tag_2, /*lemma_1=*/null, /*lemma_2=*/null)
            {
            }
            public string Word
            {
                get { return m_word; }
            }
            public string GoldTag
            {
                get { return m_gold_tag; }
            }
            public string Tag1
            {
                get { return m_tag_1; }
            }
            public string Tag2
            {
                get { return m_tag_2; }
            }
            public string Lemma1
            {
                get { return m_lemma_1; }
            }
            public string Lemma2
            {
                get { return m_lemma_2; }
            }
        }

        private static ArrayList<DataEntry> m_items
            = new ArrayList<DataEntry>();
        /*private*/public static Dictionary<string, Dictionary<string, string>> m_attr
            = new Dictionary<string, Dictionary<string, string>>();
        private static Set<string> m_attr_set
            = new Set<string>();

        public static Set<string>.ReadOnly AttrSet
        {
            get { return m_attr_set; }
        }

        public static ArrayList<DataEntry>/*.ReadOnly*/ Items
        {
            get { return m_items; }
        }

        public static void LoadAttributes(string file_name)
        {
            StreamReader reader = new StreamReader(file_name);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(' ', '\t');
                Dictionary<string, string> attr_val_dict = new Dictionary<string, string>();
                if (fields.Length >= 1)
                {
                    m_attr.Add(fields[0], attr_val_dict);
                }
                for (int i = 2; i < fields.Length; i++)
                {
                    string[] attr_val = fields[i].Split('=');
                    if (attr_val.Length != 2) { throw new InvalidDataException(); }
                    attr_val_dict.Add(attr_val[0], attr_val[1]);
                    m_attr_set.Add(attr_val[0]);
                }
            }
            reader.Close();
        }

        public static void LoadData(string file_name)
        { 
            StreamReader reader = new StreamReader(file_name);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split(' ', '\t');
                if (fields.Length == 4)
                { 
                    string word = fields[0];
                    string gold_tag = fields[1];
                    string tag_1 = fields[2];
                    string tag_2 = fields[3];
                    if (m_attr.ContainsKey(tag_1) && m_attr.ContainsKey(tag_2)) // *** make sure to load attributes first
                    {
                        m_items.Add(new DataEntry(word, gold_tag, tag_1, tag_2));
                    }
                    else if (word == gold_tag && gold_tag == tag_1 && tag_1 == tag_2) // punctuation
                    {
                        m_items.Add(new DataEntry(word, gold_tag, tag_1, tag_2));
                        m_attr.Add(gold_tag, new Dictionary<string, string>());
                    }
                }
            }
        }

        public static void LoadTestData(Corpus corpus_1, Corpus corpus_2)
        {
            for (int i = 0; i < corpus_1.TaggedWords.Count; i++)
            {
                string word = corpus_1.TaggedWords[i].Word;
                string tag_1 = corpus_1.TaggedWords[i].Tag;
                string tag_2 = corpus_2.TaggedWords[i].Tag;
                string lemma_1 = corpus_1.TaggedWords[i].Lemma;
                string lemma_2 = corpus_2.TaggedWords[i].Lemma;
                if (m_attr.ContainsKey(tag_1) && m_attr.ContainsKey(tag_2)) // *** make sure to load attributes first
                {
                    m_items.Add(new DataEntry(word, /*gold_tag=*/null, tag_1, tag_2, lemma_1, lemma_2));
                }
                else 
                {
                    m_items.Add(new DataEntry(word, /*gold_tag=*/null, tag_1, tag_1, lemma_1, lemma_1));
                    if (!m_attr.ContainsKey(tag_1))
                    {
                        m_attr.Add(tag_1, new Dictionary<string, string>());
                    }
                }
            }
        }

        public static ArrayList<KeyDat<string, string>> CreateExample(int idx)
        {
            DataEntry prev_entry = idx > 0 ? m_items[idx - 1] : null;
            DataEntry entry = m_items[idx];
            DataEntry next_entry = idx < m_items.Count - 1 ? m_items[idx + 1] : null;
            ArrayList<KeyDat<string, string>> example = new ArrayList<KeyDat<string, string>>();
            Dictionary<string, string> attr_val_1;
            Dictionary<string, string> attr_val_2;
            // previous word
            if (prev_entry != null)
            {
                attr_val_1 = m_attr[prev_entry.Tag1];
                attr_val_2 = m_attr[prev_entry.Tag2];
                if (prev_entry.Tag1.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Prev_POS_1", prev_entry.Tag1[0].ToString()));    
                }
                if (prev_entry.Tag2.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Prev_POS_2", prev_entry.Tag2[0].ToString()));    
                }
                foreach (string attr in m_attr_set)
                {
                    if (attr_val_1.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Prev_{0}_1", attr), attr_val_1[attr]));                        
                    }
                    if (attr_val_2.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Prev_{0}_2", attr), attr_val_2[attr]));
                    }
                }
            }
            // current word
            attr_val_1 = m_attr[entry.Tag1];
            attr_val_2 = m_attr[entry.Tag2];
            string pos_1 = entry.Tag1.Length > 0 ? entry.Tag1[0].ToString() : null;
            string pos_2 = entry.Tag2.Length > 0 ? entry.Tag2[0].ToString() : null;
            if (pos_1 != null)
            {
                example.Add(new KeyDat<string, string>("POS_1", pos_1));
            }
            if (pos_2 != null)
            {
                example.Add(new KeyDat<string, string>("POS_2", pos_2));
            }
            example.Add(new KeyDat<string, string>("Agree_POS", pos_1 == pos_2 ? "yes" : "no"));
            foreach (string attr in m_attr_set)
            {
                string attr_1 = attr_val_1.ContainsKey(attr) ? attr_val_1[attr] : null;
                string attr_2 = attr_val_2.ContainsKey(attr) ? attr_val_2[attr] : null;
                if (attr_1 != null)
                {
                    example.Add(new KeyDat<string, string>(string.Format("{0}_1", attr), attr_1));
                }
                if (attr_2 != null)
                {
                    example.Add(new KeyDat<string, string>(string.Format("{0}_2", attr), attr_2));
                }
                example.Add(new KeyDat<string, string>(string.Format("Agree_{0}", attr), attr_1 == attr_2 ? "yes" : "no"));
            }
            // next word
            if (next_entry != null)
            {
                attr_val_1 = m_attr[next_entry.Tag1];
                attr_val_2 = m_attr[next_entry.Tag2];
                if (next_entry.Tag1.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Next_POS_1", next_entry.Tag1[0].ToString()));
                }
                if (next_entry.Tag2.Length > 0)
                {
                    example.Add(new KeyDat<string, string>("Next_POS_2", next_entry.Tag2[0].ToString()));
                }
                foreach (string attr in m_attr_set)
                {
                    if (attr_val_1.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Next_{0}_1", attr), attr_val_1[attr]));
                    }
                    if (attr_val_2.ContainsKey(attr))
                    {
                        example.Add(new KeyDat<string, string>(string.Format("Next_{0}_2", attr), attr_val_2[attr]));
                    }
                }
            }
            return example;
        }

        public static void WriteDatasetOrange(string file_name, string null_val)
        {
            StreamWriter writer = new StreamWriter(file_name);
            StringBuilder line = new StringBuilder();            
            line.Append("Tagger\t");
            line.Append("Prev_POS_1\tPrev_POS_2\t");
            int c = 3;
            foreach (string attr in m_attr_set)
            {
                line.Append(string.Format("Prev_{0}_1\t", attr));
                line.Append(string.Format("Prev_{0}_2\t", attr));
                c += 2;
            }
            line.Append("POS_1\tPOS_2\tAgree_POS\t");
            c += 3;
            foreach (string attr in m_attr_set)
            {
                line.Append(string.Format("{0}_1\t", attr));                
                line.Append(string.Format("{0}_2\t", attr));
                line.Append(string.Format("Agree_{0}\t", attr));
                c += 3;
            }
            line.Append("Next_POS_1\tNext_POS_2\t");
            c += 2;
            foreach (string attr in m_attr_set)
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
            for (int i = 0; i < m_items.Count; i++)
            {
                Dictionary<string, string> attr_val_1;
                Dictionary<string, string> attr_val_2;
                DataEntry entry = m_items[i];
                if (entry.Tag1 != entry.Tag2 && (entry.GoldTag == entry.Tag1 || entry.GoldTag == entry.Tag2)) // the two taggers disagree, one of them is correct
                {
                    line = new StringBuilder();
                    line.Append(entry.GoldTag == entry.Tag1 ? "Tagger1" : "Tagger2");
                    line.Append("\t");
                    // previous word
                    if (i == 0)
                    {
                        c = 2 + m_attr_set.Count * 2;
                        for (int j = 0; j < c; j++)
                        {
                            line.Append(null_val);
                            line.Append("\t");
                        }
                    }
                    else
                    {
                        DataEntry prev_entry = m_items[i - 1];
                        string prev_pos_1 = prev_entry.Tag1.Length > 0 ? prev_entry.Tag1[0].ToString() : null_val;
                        string prev_pos_2 = prev_entry.Tag2.Length > 0 ? prev_entry.Tag2[0].ToString() : null_val;
                        line.Append(prev_pos_1);
                        line.Append("\t");
                        line.Append(prev_pos_2);
                        line.Append("\t");
                        attr_val_1 = m_attr[prev_entry.Tag1];
                        attr_val_2 = m_attr[prev_entry.Tag2];
                        foreach (string attr in m_attr_set)
                        {
                            line.Append(attr_val_1.ContainsKey(attr) ? attr_val_1[attr] : null_val);
                            line.Append("\t");
                            line.Append(attr_val_2.ContainsKey(attr) ? attr_val_2[attr] : null_val);
                            line.Append("\t");
                        }
                    }
                    // current word
                    string pos_1 = entry.Tag1.Length > 0 ? entry.Tag1[0].ToString() : null_val;
                    string pos_2 = entry.Tag2.Length > 0 ? entry.Tag2[0].ToString() : null_val;
                    line.Append(pos_1);
                    line.Append("\t");
                    line.Append(pos_2);
                    line.Append("\t");
                    line.Append(pos_1 == pos_2 ? "yes" : "no");
                    line.Append("\t");
                    attr_val_1 = m_attr[entry.Tag1];
                    attr_val_2 = m_attr[entry.Tag2];
                    foreach (string attr in m_attr_set)
                    {
                        string attr_1 = attr_val_1.ContainsKey(attr) ? attr_val_1[attr] : null_val;
                        string attr_2 = attr_val_2.ContainsKey(attr) ? attr_val_2[attr] : null_val;
                        line.Append(attr_1);
                        line.Append("\t");
                        line.Append(attr_2);
                        line.Append("\t");
                        line.Append(attr_1 == attr_2 ? "yes" : "no");
                        line.Append("\t");
                    }
                    // next word
                    if (i == m_items.Count - 1)
                    {
                        c = 2 + m_attr_set.Count * 2;
                        for (int j = 0; j < c; j++)
                        {
                            line.Append(null_val);
                            line.Append("\t");
                        }
                    }
                    else
                    {
                        DataEntry next_entry = m_items[i + 1];
                        string next_pos_1 = next_entry.Tag1.Length > 0 ? next_entry.Tag1[0].ToString() : null_val;
                        string next_pos_2 = next_entry.Tag2.Length > 0 ? next_entry.Tag2[0].ToString() : null_val;
                        line.Append(next_pos_1);
                        line.Append("\t");
                        line.Append(next_pos_2);
                        line.Append("\t");
                        attr_val_1 = m_attr[next_entry.Tag1];
                        attr_val_2 = m_attr[next_entry.Tag2];
                        foreach (string attr in m_attr_set)
                        {
                            line.Append(attr_val_1.ContainsKey(attr) ? attr_val_1[attr] : null_val);
                            line.Append("\t");
                            line.Append(attr_val_2.ContainsKey(attr) ? attr_val_2[attr] : null_val);
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

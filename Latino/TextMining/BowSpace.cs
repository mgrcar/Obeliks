/*==========================================================================;
 *
 *  (c) 2008-09 JSI.  All rights reserved.
 *
 *  File:          BowSpace.cs
 *  Version:       1.0
 *  Desc:		   Bag-of-words space
 *  Author:        Miha Grcar
 *  Created on:    Dec-2008
 *  Last modified: Apr-2009
 *  Revision:      Apr-2009
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Latino.Model;

namespace Latino.TextMining
{
    /* .-----------------------------------------------------------------------
       |
       |  Enum WordWeightType
       |
       '-----------------------------------------------------------------------
    */
    public enum WordWeightType
    { 
        TermFreq,
        TfIdf,
        LogDfTfIdf
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class Word
       |
       '-----------------------------------------------------------------------
    */
    public class Word : ISerializable, IEnumerable<KeyValuePair<string, int>>
    {
        internal int m_idx
            = -1;
        internal Dictionary<string, int> m_forms
            = new Dictionary<string, int>();
        internal string m_most_frequent_form;
        internal int m_doc_freq
            = 1;
        internal int m_freq
            = 1;
        internal double m_idf
            = -1;
        internal Word(BinarySerializer reader)
        {
            Load(reader); 
        }
        internal Word(string word)
        {
            m_most_frequent_form = word;
            m_forms.Add(word, 1);
        }
        public string MostFrequentForm
        {
            get { return m_most_frequent_form; }
        }
        public int DocFreq
        {
            get { return m_doc_freq; }
        }
        public int Freq
        {
            get { return m_freq; }
        }
        public double Idf
        {
            get { return m_idf; }
        }
        // *** IEnumerable<KeyValuePair<string, int>> interface implementation ***
        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return m_forms.GetEnumerator(); 
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_forms.GetEnumerator(); 
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt(m_idx);
            writer.WriteString(m_most_frequent_form);
            writer.WriteInt(m_doc_freq);
            writer.WriteInt(m_freq);
            writer.WriteDouble(m_idf);
        }
        internal void Load(BinarySerializer reader)
        {
            m_idx = reader.ReadInt();
            m_most_frequent_form = reader.ReadString();
            m_doc_freq = reader.ReadInt();
            m_freq = reader.ReadInt();
            m_idf = reader.ReadDouble();
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class BowSpace
       |
       '-----------------------------------------------------------------------
    */
    public class BowSpace : ISerializable
    {
        private class WordStem
        {
            public string Word
                = null;
            public string Stem
                = null;
        }
        private ITokenizer m_tokenizer
            = new UnicodeTokenizer();
        private Set<string>.ReadOnly m_stop_words
            = null;
        private IStemmer m_stemmer
            = null;
        private Dictionary<string, Word> m_word_info
            = new Dictionary<string, Word>();
        private ArrayList<Word> m_idx_info
            = new ArrayList<Word>();
        private ArrayList<SparseVector<double>.ReadOnly> m_bow_vectors
            = new ArrayList<SparseVector<double>.ReadOnly>();
        private int m_max_n_gram_len
            = 2;
        private int m_min_word_freq
            = 5;
        private WordWeightType m_word_weight_type
            = WordWeightType.TermFreq;
        private double m_cut_low_weights_perc
            = 0.2;
        private bool m_normalize_vectors
            = true;
        private bool m_keep_word_forms
            = false;
        public BowSpace()
        {
            // configure tokenizer
            UnicodeTokenizer tokenizer = (UnicodeTokenizer)m_tokenizer;
            tokenizer.Filter = TokenizerFilter.AlphanumLoose;
            tokenizer.MinTokenLen = 2;
        }
        public ITokenizer Tokenizer
        {
            get { return m_tokenizer; }
            set 
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Tokenizer") : null);
                m_tokenizer = value; 
            }
        }
        public Set<string>.ReadOnly StopWords
        {
            get { return m_stop_words; }
            set { m_stop_words = value; }
        }
        public IStemmer Stemmer
        {
            get { return m_stemmer; }
            set { m_stemmer = value; }
        }
        public int MaxNGramLen
        {
            get { return m_max_n_gram_len; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("MaxNGramLen") : null);
                m_max_n_gram_len = value;
            }
        }
        public int MinWordFreq
        {
            get { return m_min_word_freq; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("MinWordFreq") : null);
                m_min_word_freq = value;
            }
        }
        public WordWeightType WordWeightType
        {
            get { return m_word_weight_type; }
            set { m_word_weight_type = value; }
        }
        public double CutLowWeightsPerc
        {
            get { return m_cut_low_weights_perc; }
            set
            {
                Utils.ThrowException(value < 0 || value >= 1 ? new ArgumentOutOfRangeException("CutLowWeightsPerc") : null);
                m_cut_low_weights_perc = value;
            }
        }
        public bool NormalizeVectors
        {
            get { return m_normalize_vectors; }
            set { m_normalize_vectors = value; }
        }
        public ArrayList<SparseVector<double>.ReadOnly>.ReadOnly BowVectors
        {
            get { return m_bow_vectors; }
        }
        public ArrayList<Word>.ReadOnly Words
        {
            get { return m_idx_info; }
        }
        public bool KeepWordForms
        {
            get { return m_keep_word_forms; }
            set { m_keep_word_forms = value; }
        }
        public void OutputStats(StreamWriter writer)
        {
            writer.WriteLine("Word\tStem\tF\tDF");
            foreach (KeyValuePair<string, Word> word_info in m_word_info)
            {
                writer.WriteLine("{0}\t{1}\t{2}\t{3}", word_info.Value.m_most_frequent_form, word_info.Key, word_info.Value.m_freq, word_info.Value.m_doc_freq);
            }
        }
        private void CutLowWeights(ref SparseVector<double> vec)
        {
            if (m_cut_low_weights_perc > 0)
            {
                double wgt_sum = 0;
                ArrayList<KeyDat<double, int>> tmp = new ArrayList<KeyDat<double, int>>(vec.Count);
                foreach (IdxDat<double> item in vec)
                {
                    wgt_sum += item.Dat;
                    tmp.Add(new KeyDat<double, int>(item.Dat, item.Idx));
                }
                tmp.Sort();
                double cut_sum = m_cut_low_weights_perc * wgt_sum;
                double cut_wgt = -1;
                foreach (KeyDat<double, int> item in tmp)
                {
                    cut_sum -= item.Key;
                    if (cut_sum <= 0)
                    {
                        cut_wgt = item.Key;
                        break;
                    }
                }                
                SparseVector<double> new_vec = new SparseVector<double>();
                if (cut_wgt != -1)
                {
                    foreach (IdxDat<double> item in vec)
                    {
                        if (item.Dat >= cut_wgt)
                        {
                            new_vec.InnerIdx.Add(item.Idx);
                            new_vec.InnerDat.Add(item.Dat);
                        }
                    }
                }
                vec = new_vec;
            }
        }
        private void ProcessNGramsPass1(ArrayList<WordStem> n_grams, int start_idx, Set<string> doc_words)
        {
            string n_gram_stem = "";
            string n_gram = "";
            for (int i = start_idx; i < n_grams.Count; i++)
            {
                n_gram += n_grams[i].Word;
                n_gram_stem += n_grams[i].Stem;
                if (!m_word_info.ContainsKey(n_gram_stem))
                {
                    Word n_gram_info = new Word(n_gram);
                    m_word_info.Add(n_gram_stem, n_gram_info);
                    doc_words.Add(n_gram_stem);
                }
                else
                {
                    Word n_gram_info = m_word_info[n_gram_stem];
                    if (!doc_words.Contains(n_gram_stem))
                    {
                        doc_words.Add(n_gram_stem);
                        n_gram_info.m_doc_freq++;
                    }
                    n_gram_info.m_freq++;
                    if (!n_gram_info.m_forms.ContainsKey(n_gram))
                    {
                        n_gram_info.m_forms.Add(n_gram, 1);
                    }
                    else
                    {
                        n_gram_info.m_forms[n_gram]++;
                    }
                }
                n_gram += " ";
                n_gram_stem += " ";
            }
        }
        private void ProcessNGramsPass2(ArrayList<WordStem> n_grams, int start_idx, Dictionary<int, int> tf_vec)
        {
            string n_gram_stem = "";
            for (int i = start_idx; i < n_grams.Count; i++)
            {
                n_gram_stem += n_grams[i].Stem;
                if (m_word_info.ContainsKey(n_gram_stem))
                {
                    Word word_info = m_word_info[n_gram_stem];
                    if (word_info.m_idx == -1)
                    {
                        word_info.m_idx = m_idx_info.Count;
                        tf_vec.Add(word_info.m_idx, 1);
                        m_idx_info.Add(word_info);
                    }
                    else if (!tf_vec.ContainsKey(word_info.m_idx))
                    {
                        tf_vec.Add(word_info.m_idx, 1);
                    }
                    else
                    {
                        tf_vec[word_info.m_idx]++;
                    }
                }
                else
                {
                    break;
                }
                n_gram_stem += " ";
            }
        }
        public void Initialize(IEnumerable<string> documents)
        {
            Initialize(documents, /*large_scale=*/false);
        }
        public void Initialize(IEnumerable<string> documents, bool large_scale)
        {
            Utils.ThrowException(documents == null ? new ArgumentNullException("documents") : null);            
            m_word_info.Clear();
            m_idx_info.Clear();
            m_bow_vectors.Clear();
            // build vocabulary
            Utils.VerboseLine("Building vocabulary ...");
            int doc_count = 0;
            if (!large_scale)
            {                
                foreach (string document in documents)
                {
                    doc_count++;
                    Utils.Verbose("Document {0} ...\r", doc_count);
                    Set<string> doc_words = new Set<string>();
                    ArrayList<WordStem> n_grams = new ArrayList<WordStem>(m_max_n_gram_len);
                    m_tokenizer.Text = document;
                    foreach (string token in m_tokenizer)
                    {
                        string word = token.Trim().ToLower();
                        if (m_stop_words == null || !m_stop_words.Contains(word))
                        {
                            string stem = m_stemmer == null ? word : m_stemmer.GetStem(word).Trim().ToLower();
                            if (n_grams.Count < m_max_n_gram_len)
                            {
                                WordStem word_stem = new WordStem();
                                word_stem.Word = word;
                                word_stem.Stem = stem;
                                n_grams.Add(word_stem);
                                if (n_grams.Count < m_max_n_gram_len) { continue; }
                            }
                            else
                            {
                                WordStem word_stem = n_grams[0];
                                word_stem.Word = word;
                                word_stem.Stem = stem;
                                for (int i = 0; i < m_max_n_gram_len - 1; i++) { n_grams[i] = n_grams[i + 1]; }
                                n_grams[m_max_n_gram_len - 1] = word_stem;
                            }
                            ProcessNGramsPass1(n_grams, 0, doc_words);
                        }
                    }
                    int start_idx = n_grams.Count == m_max_n_gram_len ? 1 : 0;
                    for (int i = start_idx; i < n_grams.Count; i++)
                    {
                        ProcessNGramsPass1(n_grams, i, doc_words);
                    }
                }
                Utils.VerboseLine("");
            }
            else // large-scale mode (needs less memory, slower)
            {
                for (int n = 1; n <= m_max_n_gram_len; n++)
                {
                    doc_count = 0;
                    Utils.VerboseLine("Pass {0} of {1} ...", n, m_max_n_gram_len);
                    foreach (string document in documents)
                    {
                        doc_count++;
                        Utils.Verbose("Document {0} ...\r", doc_count);
                        ArrayList<WordStem> n_grams = new ArrayList<WordStem>(n);
                        Set<string> doc_words = new Set<string>();
                        m_tokenizer.Text = document;
                        foreach (string token in m_tokenizer)
                        {
                            string word = token.Trim().ToLower();
                            if (m_stop_words == null || !m_stop_words.Contains(word))
                            {
                                string stem = m_stemmer == null ? word : m_stemmer.GetStem(word).Trim().ToLower();
                                if (n_grams.Count < n)
                                {
                                    WordStem word_stem = new WordStem();
                                    word_stem.Word = word;
                                    word_stem.Stem = stem;
                                    n_grams.Add(word_stem);
                                    if (n_grams.Count < n) { continue; }
                                }
                                else
                                {
                                    WordStem word_stem = n_grams[0];
                                    word_stem.Word = word;
                                    word_stem.Stem = stem;
                                    for (int i = 0; i < n - 1; i++) { n_grams[i] = n_grams[i + 1]; }
                                    n_grams[n - 1] = word_stem;
                                }
                                string n_gram = n_grams[0].Word;
                                string n_gram_stem = n_grams[0].Stem;
                                if (n > 1)
                                {
                                    for (int i = 1; i < n - 1; i++)
                                    {
                                        n_gram += " " + n_grams[i].Word;
                                        n_gram_stem += " " + n_grams[i].Stem;
                                    }
                                    if (!m_word_info.ContainsKey(n_gram_stem)) { continue; }
                                    if (m_word_info[n_gram_stem].m_freq < m_min_word_freq) { continue; }
                                    string n_gram_stem_2 = "";
                                    for (int i = 1; i < n - 1; i++)
                                    {
                                        n_gram_stem_2 += n_grams[i].Stem + " ";
                                    }
                                    n_gram_stem_2 += n_grams[n - 1].Stem;
                                    if (!m_word_info.ContainsKey(n_gram_stem_2)) { continue; }
                                    if (m_word_info[n_gram_stem_2].m_freq < m_min_word_freq) { continue; }
                                    n_gram += " " + n_grams[n - 1].Word;
                                    n_gram_stem += " " + n_grams[n - 1].Stem;
                                }
                                if (!m_word_info.ContainsKey(n_gram_stem))
                                {
                                    Word n_gram_info = new Word(n_gram);
                                    m_word_info.Add(n_gram_stem, n_gram_info);
                                    doc_words.Add(n_gram_stem);
                                }
                                else
                                {
                                    Word n_gram_info = m_word_info[n_gram_stem];
                                    if (!doc_words.Contains(n_gram_stem))
                                    {
                                        n_gram_info.m_doc_freq++;
                                        doc_words.Add(n_gram_stem);                                        
                                    }
                                    n_gram_info.m_freq++;
                                    if (!n_gram_info.m_forms.ContainsKey(n_gram))
                                    {
                                        n_gram_info.m_forms.Add(n_gram, 1);
                                    }
                                    else
                                    {
                                        n_gram_info.m_forms[n_gram]++;
                                    }
                                }
                            }
                        }
                    }
                    Utils.VerboseLine("");
                }
            }            
            // remove unfrequent words and n-grams, precompute IDF      
            ArrayList<string> remove_list = new ArrayList<string>();
            foreach (KeyValuePair<string, Word> word_info in m_word_info)
            {
                if (word_info.Value.m_freq < m_min_word_freq)
                {
                    remove_list.Add(word_info.Key);
                }
                else
                {
                    word_info.Value.m_idf = Math.Log((double)doc_count / (double)word_info.Value.m_doc_freq);
                }
            }
            foreach (string key in remove_list) { m_word_info.Remove(key); }
            // determine most frequent word and n-gram forms
            foreach (Word word_info in m_word_info.Values)
            { 
                int max = 0;
                foreach (KeyValuePair<string, int> word_form in word_info.m_forms)
                { 
                    if (word_form.Value > max) 
                    { 
                        max = word_form.Value;
                        word_info.m_most_frequent_form = word_form.Key;
                    }
                }
                if (!m_keep_word_forms) { word_info.m_forms.Clear(); } 
            }
            // compute bag-of-words vectors
            Utils.VerboseLine("Computing bag-of-words vectors ...");           
            int doc_num = 1;
            foreach (string document in documents)
            {                
                Utils.Verbose("Document {0} of {1} ...\r", doc_num++, doc_count);
                Dictionary<int, int> tf_vec = new Dictionary<int, int>();
                ArrayList<WordStem> n_grams = new ArrayList<WordStem>(m_max_n_gram_len);
                m_tokenizer.Text = document;
                foreach (string token in m_tokenizer)
                {
                    string word = token.Trim().ToLower();                    
                    if (m_stop_words == null || !m_stop_words.Contains(word))
                    {
                        string stem = m_stemmer == null ? word : m_stemmer.GetStem(word).Trim().ToLower();
                        if (n_grams.Count < m_max_n_gram_len)
                        {
                            WordStem word_stem = new WordStem();
                            word_stem.Word = word;
                            word_stem.Stem = stem;
                            n_grams.Add(word_stem);
                            if (n_grams.Count < m_max_n_gram_len) { continue; }
                        }
                        else
                        {
                            WordStem word_stem = n_grams[0];
                            word_stem.Word = word;
                            word_stem.Stem = stem;
                            for (int i = 0; i < m_max_n_gram_len - 1; i++) { n_grams[i] = n_grams[i + 1]; }
                            n_grams[m_max_n_gram_len - 1] = word_stem;
                        }
                        ProcessNGramsPass2(n_grams, 0, tf_vec);
                    }
                }
                int start_idx = n_grams.Count == m_max_n_gram_len ? 1 : 0;
                for (int i = start_idx; i < n_grams.Count; i++)
                {
                    ProcessNGramsPass2(n_grams, i, tf_vec);
                }
                SparseVector<double> doc_vec = new SparseVector<double>();
                if (m_word_weight_type == WordWeightType.TermFreq)
                {
                    foreach (KeyValuePair<int, int> tf_item in tf_vec)
                    {
                        doc_vec.InnerIdx.Add(tf_item.Key);
                        doc_vec.InnerDat.Add(tf_item.Value);
                    }
                }
                else if (m_word_weight_type == WordWeightType.TfIdf)
                {
                    foreach (KeyValuePair<int, int> tf_item in tf_vec)
                    {
                        double tf_idf = (double)tf_item.Value * m_idx_info[tf_item.Key].m_idf;
                        if (tf_idf > 0)
                        {
                            doc_vec.InnerIdx.Add(tf_item.Key);
                            doc_vec.InnerDat.Add(tf_idf);
                        }
                    }
                }
                else if (m_word_weight_type == WordWeightType.LogDfTfIdf)
                {
                    foreach (KeyValuePair<int, int> tf_item in tf_vec)
                    {
                        double tf_idf = (double)tf_item.Value * m_idx_info[tf_item.Key].m_idf;
                        if (tf_idf > 0)
                        {
                            doc_vec.InnerIdx.Add(tf_item.Key);
                            doc_vec.InnerDat.Add(Math.Log(1 + m_idx_info[tf_item.Key].m_doc_freq) * tf_idf);
                        }
                    }
                }
                doc_vec.Sort();
                CutLowWeights(ref doc_vec);
                if (m_normalize_vectors) { ModelUtils.TryNrmVecL2(doc_vec); }
                m_bow_vectors.Add(doc_vec);
            }
            Utils.VerboseLine("");
        }
        private void ProcessDocumentNGrams(ArrayList<WordStem> n_grams, int start_idx, Dictionary<int, int> tf_vec)
        {
            string n_gram_stem = "";
            string n_gram = "";
            for (int i = start_idx; i < n_grams.Count; i++)
            {
                n_gram += n_grams[i].Word;
                n_gram_stem += n_grams[i].Stem;
                if (m_word_info.ContainsKey(n_gram_stem))
                {
                    int stem_idx = m_word_info[n_gram_stem].m_idx;
                    if (tf_vec.ContainsKey(stem_idx))
                    {
                        tf_vec[stem_idx]++;
                    }
                    else
                    {
                        tf_vec.Add(stem_idx, 1);
                    }
                }
                n_gram += " ";
                n_gram_stem += " ";
            }
        }
        public SparseVector<double> ProcessDocument(string document)
        {
            Dictionary<int, int> tf_vec = new Dictionary<int, int>();
            ArrayList<WordStem> n_grams = new ArrayList<WordStem>(m_max_n_gram_len);
            m_tokenizer.Text = document;
            foreach (string token in m_tokenizer)
            {
                string word = token.Trim().ToLower();
                if (m_stop_words == null || !m_stop_words.Contains(word))
                {
                    string stem = m_stemmer == null ? word : m_stemmer.GetStem(word).Trim().ToLower();
                    if (n_grams.Count < m_max_n_gram_len)
                    {
                        WordStem word_stem = new WordStem();
                        word_stem.Word = word;
                        word_stem.Stem = stem;
                        n_grams.Add(word_stem);
                        if (n_grams.Count < m_max_n_gram_len) { continue; }
                    }
                    else
                    {
                        WordStem word_stem = n_grams[0];
                        word_stem.Word = word;
                        word_stem.Stem = stem;
                        for (int i = 0; i < m_max_n_gram_len - 1; i++) { n_grams[i] = n_grams[i + 1]; }
                        n_grams[m_max_n_gram_len - 1] = word_stem;
                    }
                    ProcessDocumentNGrams(n_grams, 0, tf_vec);
                }
            }
            int start_idx = n_grams.Count == m_max_n_gram_len ? 1 : 0;
            for (int i = start_idx; i < n_grams.Count; i++)
            {
                ProcessDocumentNGrams(n_grams, i, tf_vec);
            }
            SparseVector<double> doc_vec = new SparseVector<double>();
            if (m_word_weight_type == WordWeightType.TermFreq)
            {
                foreach (KeyValuePair<int, int> tf_item in tf_vec)
                {
                    doc_vec.InnerIdx.Add(tf_item.Key);
                    doc_vec.InnerDat.Add(tf_item.Value);
                }
            }
            else if (m_word_weight_type == WordWeightType.TfIdf)
            {
                foreach (KeyValuePair<int, int> tf_item in tf_vec)
                {
                    double tf_idf = (double)tf_item.Value * m_idx_info[tf_item.Key].m_idf;
                    if (tf_idf > 0)
                    {
                        doc_vec.InnerIdx.Add(tf_item.Key);
                        doc_vec.InnerDat.Add(tf_idf);
                    }
                }
            }
            else if (m_word_weight_type == WordWeightType.LogDfTfIdf)
            {
                foreach (KeyValuePair<int, int> tf_item in tf_vec)
                {
                    double tf_idf = (double)tf_item.Value * m_idx_info[tf_item.Key].m_idf;
                    if (tf_idf > 0)
                    {
                        doc_vec.InnerIdx.Add(tf_item.Key);
                        doc_vec.InnerDat.Add(Math.Log(1 + m_idx_info[tf_item.Key].m_doc_freq) * tf_idf);
                    }
                }
            }
            doc_vec.Sort();
            CutLowWeights(ref doc_vec);
            if (m_normalize_vectors) { ModelUtils.TryNrmVecL2(doc_vec); }
            return doc_vec;
        }
        public ArrayList<KeyDat<double, string>> GetKeywords(SparseVector<double>.ReadOnly bow_vec)
        {
            Utils.ThrowException(bow_vec == null ? new ArgumentNullException("bow_vec") : null);            
            ArrayList<KeyDat<double, string>> keywords = new ArrayList<KeyDat<double, string>>(bow_vec.Count);
            foreach (IdxDat<double> item in bow_vec)
            {
                keywords.Add(new KeyDat<double, string>(item.Dat, m_idx_info[item.Idx].m_most_frequent_form)); // throws ArgumentOutOfRangeException
            }
            keywords.Sort(new DescSort<KeyDat<double, string>>());
            return keywords;
        }
        public ArrayList<string> GetKeywords(SparseVector<double>.ReadOnly bow_vec, int n)
        {
            Utils.ThrowException(n <= 0 ? new ArgumentOutOfRangeException("n") : null);
            ArrayList<KeyDat<double, string>> keywords = GetKeywords(bow_vec); // throws ArgumentNullException, ArgumentOutOfRangeException            
            int keyword_count = Math.Min(n, keywords.Count);
            ArrayList<string> keyword_list = new ArrayList<string>(keyword_count);
            for (int i = 0; i < keyword_count; i++)
            {
                keyword_list.Add(keywords[i].Dat);
            }
            return keyword_list;
        }
        public ArrayList<KeyDat<double, string>> GetKeywordsAllForms(SparseVector<double>.ReadOnly bow_vec, bool distribute_weight)
        {            
            Utils.ThrowException(bow_vec == null ? new ArgumentNullException("bow_vec") : null);
            if (!distribute_weight) // inherit weight from the most frequent word form
            {
                ArrayList<KeyDat<Pair<double, int>, string>> keywords = new ArrayList<KeyDat<Pair<double, int>, string>>(bow_vec.Count);
                foreach (IdxDat<double> item in bow_vec)
                {
                    Word word = m_idx_info[item.Idx]; // throws ArgumentOutOfRangeException
                    foreach (KeyValuePair<string, int> word_form_info in word.m_forms)
                    {
                        keywords.Add(new KeyDat<Pair<double, int>, string>(new Pair<double, int>(item.Dat, word_form_info.Value), word_form_info.Key));
                    }
                }
                keywords.Sort(new DescSort<KeyDat<Pair<double, int>, string>>());
                ArrayList<KeyDat<double, string>> keyword_list = new ArrayList<KeyDat<double, string>>(keywords.Count);
                foreach (KeyDat<Pair<double, int>, string> item in keywords)
                {
                    keyword_list.Add(new KeyDat<double, string>(item.Key.First, item.Dat));
                }
                return keyword_list;
            }
            else
            {
                return null;
                // !!!!!!!! TBD !!!!!!!!!!!!!
            }
        }
        public ArrayList<string> GetKeywordsAllForms(SparseVector<double>.ReadOnly bow_vec, int n, bool distribute_weight)
        {
            Utils.ThrowException(n <= 0 ? new ArgumentOutOfRangeException("n") : null);
            ArrayList<KeyDat<double, string>> keywords = GetKeywordsAllForms(bow_vec, distribute_weight); // throws ArgumentNullException, ArgumentOutOfRangeException            
            int keyword_count = Math.Min(n, keywords.Count);
            ArrayList<string> keyword_list = new ArrayList<string>(keyword_count);
            for (int i = 0; i < keyword_count; i++)
            {
                keyword_list.Add(keywords[i].Dat);
            }
            return keyword_list;
        }
        // *** ISerializable interface implementation ***
        public void SaveVocabulary(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt(m_word_info.Count);
            foreach (KeyValuePair<string, Word> item in m_word_info)
            {
                writer.WriteString(item.Key);
                item.Value.Save(writer);
            }
        }
        public void LoadVocabulary(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            ArrayList<IdxDat<Word>> tmp = new ArrayList<IdxDat<Word>>();
            // the following statements throw serialization-related exceptions
            m_word_info.Clear();
            m_idx_info.Clear();
            m_bow_vectors.Clear(); // *** bags-of-words are removed 
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                Word dat = new Word(reader);
                m_word_info.Add(key, dat);
                tmp.Add(new IdxDat<Word>(dat.m_idx, dat));
            }
            tmp.Sort();
            foreach (IdxDat<Word> item in tmp)
            {
                m_idx_info.Add(item.Dat);
            }
        }
        public void Save(BinarySerializer writer)
        {
            // the following statements throw serialization-related exceptions
            SaveVocabulary(writer); // throws ArgumentNullException
            writer.WriteObject<ITokenizer>(m_tokenizer);
            writer.WriteObject<Set<string>.ReadOnly>(m_stop_words);
            writer.WriteObject<IStemmer>(m_stemmer);
            m_bow_vectors.Save(writer);
            writer.WriteInt(m_max_n_gram_len);
            writer.WriteInt(m_min_word_freq);
            writer.WriteInt((int)m_word_weight_type);
            writer.WriteDouble(m_cut_low_weights_perc);
            writer.WriteBool(m_normalize_vectors);
        }
        public void Load(BinarySerializer reader)
        {
            // the following statements throw serialization-related exceptions
            LoadVocabulary(reader); // throws ArgumentNullException
            m_tokenizer = reader.ReadObject<ITokenizer>();
            m_stop_words = reader.ReadObject<Set<string>.ReadOnly>();
            m_stemmer = reader.ReadObject<IStemmer>();
            m_bow_vectors.Load(reader);
            m_max_n_gram_len = reader.ReadInt();
            m_min_word_freq = reader.ReadInt();
            m_word_weight_type = (WordWeightType)reader.ReadInt();
            m_cut_low_weights_perc = reader.ReadDouble();
            m_normalize_vectors = reader.ReadBool();
        }
    }
}

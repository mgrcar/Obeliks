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
 *  File:    Corpus.cs
 *  Desc:    Textual corpus, XML-TEI support 
 *  Created: Jun-2009
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using Latino;
using Latino.TextMining;

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class MoreInfo
       |
       '-----------------------------------------------------------------------
    */
    public class MoreInfo
    {
        private bool mPunct
            = false;
        private bool mEos
            = false;
        private bool mEop
            = false;
        private bool mSpc
            = false;

        public bool Punctuation
        {
            get { return mPunct; }
        }

        public bool EndOfSentence
        {
            get { return mEos; }
        }

        public bool EndOfParagraph
        {
            get { return mEop; }
        }

        public bool FollowedBySpace
        {
            get { return mSpc; }
        }

        internal void SetPunctuationFlag()
        {
            mPunct = true;
        }

        internal void SetEndOfSentenceFlag()
        {
            mEos = true;
        }

        internal void SetEndOfParagraphFlag()
        { 
            mEop = true;
        }

        internal void RemoveEndOfParagraphFlag() 
        {
            mEop = false;
        }

        internal void SetFollowedBySpaceFlag()
        {
            mSpc = true;
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class TaggedWord
       |
       '-----------------------------------------------------------------------
    */
    public class TaggedWord
    {
        private string mWord;
        private string mTag;
        private string mLemma;
        private MoreInfo mMoreInfo
            = null;

        public TaggedWord(string word, string tag, string lemma)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            mWord = word;
            mTag = tag;
            mLemma = lemma;
        }

        public TaggedWord(string word) : this(word, /*tag=*/null, /*lemma=*/null) // throws ArgumentNullException
        {
        }

        public string WordLower
        {
            get { return mWord.ToLower(); }
        }

        public string Word
        {
            get { return mWord; }
        }

        public string Tag
        {
            get { return mTag; }
            set { mTag = value; }
        }

        public string Lemma
        {
            get { return mLemma; }
            set { mLemma = value; }
        }

        public MoreInfo MoreInfo
        {
            get { return mMoreInfo; }
        }

        internal void EnableMoreInfo()
        {
            mMoreInfo = new MoreInfo();
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class Corpus
       |
       '-----------------------------------------------------------------------
    */
    public class Corpus
    {
        private ArrayList<TaggedWord> mTaggedWords 
            = new ArrayList<TaggedWord>();
        private string mTeiHeader
            = null;

        public ArrayList<TaggedWord>.ReadOnly TaggedWords
        {
            get { return mTaggedWords; }
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (TaggedWord word in mTaggedWords)
            {
                str.Append(word.Word);
                if (word.MoreInfo == null)
                {
                    str.Append(" ");
                }
                else
                {
                    if (word.MoreInfo.EndOfParagraph)
                    {
                        str.AppendLine();
                        str.AppendLine();
                    }
                    else if (word.MoreInfo.EndOfSentence)
                    {
                        str.AppendLine();
                    }
                    else if (word.MoreInfo.FollowedBySpace)
                    {
                        str.Append(" ");
                    }
                }
            }
            return str.ToString().TrimEnd(' ', '\n', '\r');
        }

        private string XmlEncodeText(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string XmlEncodeValue(string value)
        {
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        public string ToString(string format)
        {
            if (format == "T")
            {
                return ToString();
            }
            else if (format == "TT")
            {
                StringBuilder str = new StringBuilder();
                foreach (TaggedWord taggedWord in mTaggedWords)
                {
                    str.AppendLine(string.Format("{0}\t{1}", taggedWord.Word, taggedWord.Tag));
                }
                return str.ToString();
            }
            else if (format == "XML")
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("<TEI xmlns=\"http://www.tei-c.org/ns/1.0\">");
                if (mTeiHeader != null) 
                { 
                    str.Append(mTeiHeader);
                }
                str.AppendLine("\t<text>");
                str.AppendLine("\t\t<body>");
                str.AppendLine("\t\t\t<p>");
                bool newSentence = true;
                foreach (TaggedWord taggedWord in mTaggedWords)
                {
                    if (newSentence)
                    {
                        str.AppendLine("\t\t\t\t<s>");
                        newSentence = false;
                    }
                    string tag = taggedWord.Tag;
                    bool eos = false;
                    if (tag != null && tag.EndsWith("<eos>"))
                    {
                        tag = tag.Substring(0, tag.Length - 5);
                        eos = true;
                    }
                    if (tag == taggedWord.Word)
                    {
                        str.AppendLine(string.Format("\t\t\t\t\t<c>{0}</c>", XmlEncodeText(taggedWord.Word)));
                    }
                    else
                    {
                        str.AppendLine(string.Format("\t\t\t\t\t<w msd=\"{1}\" lemma=\"{0}\">{2}</w>", XmlEncodeValue(taggedWord.Lemma), tag, XmlEncodeText(taggedWord.Word)));
                    }
                    if (eos)
                    {
                        str.AppendLine("\t\t\t\t</s>");
                        newSentence = true;
                    }
                }
                if (!newSentence) { str.AppendLine("\t\t\t\t</s>"); }
                str.AppendLine("\t\t\t</p>");
                str.AppendLine("\t\t</body>");
                str.AppendLine("\t</text>");
                str.AppendLine("</TEI>");
                return str.ToString();
            }
			else if (format == "XML-MI")
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("<TEI xmlns=\"http://www.tei-c.org/ns/1.0\">");
                if (mTeiHeader != null)
                {
                    str.Append(mTeiHeader);
                }
                str.AppendLine("\t<text>");
                str.AppendLine("\t\t<body>");               
                bool newSentence = true;
                bool newParagraph = true;
                foreach (TaggedWord taggedWord in mTaggedWords)
                {
                    if (newParagraph)
                    {
                        str.AppendLine("\t\t\t<p>");
                        newParagraph = false;
                    }
                    if (newSentence)
                    {
                        str.AppendLine("\t\t\t\t<s>");
                        newSentence = false;
                    }
                    string tag = taggedWord.Tag;
                    if (tag != null && tag.EndsWith("<eos>"))
                    {
                        tag = tag.Substring(0, tag.Length - 5);
                    }
                    if (taggedWord.MoreInfo.Punctuation)
                    {
                        str.AppendLine(string.Format("\t\t\t\t\t<c>{0}</c>", XmlEncodeText(taggedWord.Word)));
                    }
                    else if (taggedWord.Lemma != null)
                    {
                        str.AppendLine(string.Format("\t\t\t\t\t<w msd=\"{0}\" lemma=\"{1}\">{2}</w>", tag, XmlEncodeValue(taggedWord.Lemma), XmlEncodeText(taggedWord.Word)));
                    }
                    else
                    {
                        str.AppendLine(string.Format("\t\t\t\t\t<w msd=\"{0}\">{1}</w>", tag, XmlEncodeText(taggedWord.Word)));
                    }
                    if (taggedWord.MoreInfo.FollowedBySpace)
                    {
                        str.AppendLine("\t\t\t\t\t<S/>");
                    }
                    if (taggedWord.MoreInfo.EndOfSentence)
                    {
                        str.AppendLine("\t\t\t\t</s>");
                        newSentence = true;
                    }
                    if (taggedWord.MoreInfo.EndOfParagraph)
                    {
                        str.AppendLine("\t\t\t</p>");
                        newParagraph = true;
                    }
                }
                if (!newSentence) { str.AppendLine("\t\t\t\t</s>"); }
                if (!newParagraph) { str.AppendLine("\t\t\t</p>"); }
                str.AppendLine("\t\t</body>");
                str.AppendLine("\t</text>");
                str.AppendLine("</TEI>");
                return str.ToString();
            }
            else if (format == "TBL")
            {
                StringBuilder str = new StringBuilder();
                foreach (TaggedWord taggedWord in mTaggedWords)
                {
                    str.AppendLine(string.Format("{0}\t{1}\t{2}", taggedWord.Word, taggedWord.Lemma, taggedWord.Tag));
                }
                return str.ToString();
            }
            else
            {
                throw new ArgumentNotSupportedException("format");
            }
        }

        public void LoadFromTextSsjTokenizer(string text) 
        {
            Utils.ThrowException(text == null ? new ArgumentNullException("text") : null);
            mTaggedWords.Clear();
            mTeiHeader = null;
            string xml = Rules.Tokenize(text);
            LoadFromXml(xml, /*tagLen=*/-1);
        }

        public void LoadFromText(string text)
        {
            Utils.ThrowException(text == null ? new ArgumentNullException("text") : null);
            mTaggedWords.Clear();
            mTeiHeader = null;
            RegexTokenizer tokenizer = new RegexTokenizer();
            tokenizer.TokenRegex = @"\p{L}+(-\p{L}+)*";
            tokenizer.IgnoreUnknownTokens = false;
            foreach (string word in tokenizer.GetTokens(text))
            {
                mTaggedWords.Add(new TaggedWord(word, /*tag=*/null, /*lemma=*/null));
            }
        }

        public void LoadFromTsvFile(string fileName)
        {
            Utils.ThrowException(fileName == null ? new ArgumentNullException("fileName") : null);
            Utils.ThrowException(!Utils.VerifyFileNameOpen(fileName) ? new ArgumentValueException("fileName") : null);
            mTaggedWords.Clear();
            mTeiHeader = null;
            StreamReader reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] taggedWord = line.Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (taggedWord.Length >= 1)
                {
                    string word = taggedWord[0];
                    string tag = taggedWord.Length > 1 ? taggedWord[1] : null;
                    mTaggedWords.Add(new TaggedWord(word, tag, /*lemma=*/null));
                }
            }
        }

        public void LoadFromXml(string xml, int tagLen)
        {
            Utils.ThrowException(xml == null ? new ArgumentNullException("xml") : null);
            mTaggedWords.Clear();
            mTeiHeader = null;
            XmlTextReader xmlReader = new XmlTextReader(new StringReader(xml));
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "p") // paragraph
                {                    
                    while (xmlReader.Read() && !(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "p"))
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "s") // sentence
                        {
                            while (xmlReader.Read() && !(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "s"))
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "w") // word
                                {
                                    string lemma = xmlReader.GetAttribute("lemma");
                                    string msd = xmlReader.GetAttribute("msd");
                                    if (tagLen > 0) { msd = msd.Substring(0, Math.Min(msd.Length, tagLen)).TrimEnd('-'); }
                                    xmlReader.Read();
                                    mTaggedWords.Add(new TaggedWord(xmlReader.Value, msd, lemma));
                                    mTaggedWords.Last.EnableMoreInfo();
                                }
                                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "c") // punctuation
                                {
                                    xmlReader.Read();
                                    mTaggedWords.Add(new TaggedWord(xmlReader.Value, xmlReader.Value, /*lemma=*/null));
                                    mTaggedWords.Last.EnableMoreInfo();
                                    mTaggedWords.Last.MoreInfo.SetPunctuationFlag();
                                }
                                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "S") // space
                                {
                                    if (mTaggedWords.Count > 0) { mTaggedWords.Last.MoreInfo.SetFollowedBySpaceFlag(); }
                                }
                            }
                            if (mTaggedWords.Count > 0)
                            {
                                mTaggedWords.Last.MoreInfo.SetEndOfSentenceFlag();
                                if (mTaggedWords.Last.MoreInfo.Punctuation)
                                {
                                    mTaggedWords.Last.Tag += "<eos>"; // end-of-statement punctuation                               
                                }
                            }
                        }
                    }
                    if (mTaggedWords.Count > 0) { mTaggedWords.Last.MoreInfo.SetEndOfParagraphFlag(); }
                }
            }
            xmlReader.Close();
        }

        private static Regex mTeiHeaderStart 
            = new Regex(@"\<teiHeader[^>]*\>.*$", RegexOptions.Compiled);
        private static Regex mTeiHeaderEnd 
            = new Regex(@"^.*\</teiHeader\>", RegexOptions.Compiled);
        private static Regex mTeiHeaderFull
            = new Regex(@"\<teiHeader[^>]*\>.*\</teiHeader\>", RegexOptions.Compiled);

        private void ReadTeiHeader(string fileName)
        {
            StringBuilder sb = null;
            StreamReader reader = new StreamReader(fileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {                
                Match m;
                if ((m = mTeiHeaderFull.Match(line)).Success)
                {
                    mTeiHeader = m.Value;
                    break;
                }
                else if ((m = mTeiHeaderStart.Match(line)).Success)
                {
                    sb = new StringBuilder();
                    sb.AppendLine(m.Value);
                }
                else if ((m = mTeiHeaderEnd.Match(line)).Success)
                {
                    sb.AppendLine(m.Value);
                    mTeiHeader = sb.ToString();
                    break;
                }
                else if (sb != null) 
                { 
                    sb.AppendLine(line); 
                }
            }
            reader.Close();
        }

        public void LoadFromGigaFidaFile(string fileName)
        { 
            Utils.ThrowException(fileName == null ? new ArgumentNullException("fileName") : null);
            Utils.ThrowException(!Utils.VerifyFileNameOpen(fileName) ? new ArgumentValueException("fileName") : null);
            XmlTextReader xmlReader = null;
            try
            {
                bool hasHeader = false;
                mTaggedWords.Clear();
                mTeiHeader = null;
                xmlReader = new XmlTextReader(new FileStream(fileName, FileMode.Open));
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "teiHeader") // header
                    {
                        hasHeader = true;
                        Utils.XmlSkip(xmlReader, "teiHeader");
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "p") // paragraph
                    {
                        ThreadHandler.AbortCheckpoint(); // TODO: do this at various appropriate places
                        xmlReader.Read();
                        Corpus aux = new Corpus();
                        aux.LoadFromTextSsjTokenizer(xmlReader.Value);
                        if (aux.TaggedWords.Count > 0)
                        {
                            foreach (TaggedWord word in aux.TaggedWords)
                            {
                                word.MoreInfo.RemoveEndOfParagraphFlag();
                                mTaggedWords.Add(word);
                            }
                            aux.TaggedWords.Last.MoreInfo.SetEndOfParagraphFlag();
                        }
                    }
                }
                xmlReader.Close();
                if (hasHeader) { ReadTeiHeader(fileName); }
            }
            catch
            {
                try { xmlReader.Close(); } catch { }
                throw;
            }
        }

        public void LoadFromXmlFile(string fileName, int tagLen)
        {
            Utils.ThrowException(fileName == null ? new ArgumentNullException("fileName") : null);
            Utils.ThrowException(!Utils.VerifyFileNameOpen(fileName) ? new ArgumentValueException("fileName") : null);
            XmlTextReader xmlReader = null;
            try
            {
                bool hasHeader = false;
                mTaggedWords.Clear();
                mTeiHeader = null;
                xmlReader = new XmlTextReader(new FileStream(fileName, FileMode.Open));
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "teiHeader") // header
                    {
                        hasHeader = true;
                        Utils.XmlSkip(xmlReader, "teiHeader");
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "p") // paragraph
                    {
                        while (xmlReader.Read() && !(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "p"))
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "s") // sentence
                            {
                                while (xmlReader.Read() && !(xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "s"))
                                {
                                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "w") // word
                                    {
                                        string lemma = xmlReader.GetAttribute("lemma");
                                        string msd = xmlReader.GetAttribute("msd");
                                        if (tagLen > 0) { msd = msd.Substring(0, Math.Min(msd.Length, tagLen)).TrimEnd('-'); }
                                        xmlReader.Read();
                                        mTaggedWords.Add(new TaggedWord(xmlReader.Value, msd, lemma));
                                        mTaggedWords.Last.EnableMoreInfo();
                                    }
                                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "c") // punctuation
                                    {
                                        xmlReader.Read();
                                        mTaggedWords.Add(new TaggedWord(xmlReader.Value, xmlReader.Value, /*lemma=*/null));
                                        mTaggedWords.Last.EnableMoreInfo();
                                        mTaggedWords.Last.MoreInfo.SetPunctuationFlag();
                                    }
                                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "S") // space
                                    {
                                        if (mTaggedWords.Count > 0) { mTaggedWords.Last.MoreInfo.SetFollowedBySpaceFlag(); }
                                    }
                                }
                                if (mTaggedWords.Count > 0)
                                {
                                    mTaggedWords.Last.MoreInfo.SetEndOfSentenceFlag();
                                    if (mTaggedWords.Last.MoreInfo.Punctuation)
                                    {
                                        mTaggedWords.Last.Tag += "<eos>"; // end-of-statement punctuation                               
                                    }
                                }
                            }
                        }
                        if (mTaggedWords.Count > 0) { mTaggedWords.Last.MoreInfo.SetEndOfParagraphFlag(); }
                    }
                }
                xmlReader.Close();
                if (hasHeader) { ReadTeiHeader(fileName); }
            }
            catch
            {
                try { xmlReader.Close(); } catch { }
                throw;
            }
        }

        public void SaveToTsvFile(string fileName)
        {
            Utils.ThrowException(fileName == null ? new ArgumentNullException("fileName") : null);
            Utils.ThrowException(!Utils.VerifyFileNameCreate(fileName) ? new ArgumentValueException("fileName") : null);
            StreamWriter writer = new StreamWriter(fileName);
            foreach (TaggedWord taggedWord in mTaggedWords)
            {
                writer.WriteLine("{0}\t{1}", taggedWord.Word, taggedWord.Tag);
            }
            writer.Close();
        }

        private static void AddFeature(string featureName, Dictionary<string, int> featureSpace, bool extendFeatureSpace, ArrayList<int> featureVector)
        {
            if (featureSpace.ContainsKey(featureName))
            {
                featureVector.Add(featureSpace[featureName]);
            }
            else if (extendFeatureSpace)
            {
                featureVector.Add(featureSpace.Count);
                featureSpace.Add(featureName, featureSpace.Count);
            }
        }

        private static string GetSuffix(string word, int n)
        {
            if (word.Length <= n) { return word; }
            return word.Substring(word.Length - n);
        }

        private static string GetPrefix(string word, int n)
        {
            if (word.Length <= n) { return word; }
            return word.Substring(0, n);
        }

        public BinaryVector GenerateFeatureVector(int wordIdx, Dictionary<string, int> featureSpace, bool extendFeatureSpace, PatriciaTree suffixTree)
        {
            Utils.ThrowException((wordIdx < 0 || wordIdx >= mTaggedWords.Count) ? new ArgumentOutOfRangeException("wordIdx") : null);
            Utils.ThrowException(suffixTree == null ? new ArgumentNullException("suffixTree") : null);
            ArrayList<int> featureVector = new ArrayList<int>();
            for (int offset = -3; offset <= 3; offset++) // consider context of 3 + 1 + 3 words
            {
                int idx = wordIdx + offset;
                // *** unigrams ***
                if (idx >= 0 && idx < mTaggedWords.Count)
                {
                    AddFeature(string.Format("w({0}) {1}", offset, mTaggedWords[idx].WordLower), featureSpace, extendFeatureSpace, featureVector);
                    for (int i = 1; i <= 4; i++) // consider prefixes and suffixes of up to 4 letters
                    {
                        string prefix = GetPrefix(mTaggedWords[idx].WordLower, i);
                        AddFeature(string.Format("p{0}({1}) {2}", i, offset, prefix), featureSpace, extendFeatureSpace, featureVector);
                        string suffix = GetSuffix(mTaggedWords[idx].WordLower, i);
                        AddFeature(string.Format("s{0}({1}) {2}", i, offset, suffix), featureSpace, extendFeatureSpace, featureVector);
                    }
                    if (offset < 0) // tag is available iff offset < 0
                    {
                        AddFeature(string.Format("t({0}) {1}", offset, mTaggedWords[idx].Tag), featureSpace, extendFeatureSpace, featureVector);
                        if (mTaggedWords[idx].Tag.Length > 0)
                        {
                            AddFeature(string.Format("t1({0}) {1}", offset, mTaggedWords[idx].Tag[0]), featureSpace, extendFeatureSpace, featureVector);
                        }
                    }
                    else // tag not available; use "maybe" features and ambiguity class instead
                    {
                        string word = mTaggedWords[idx].WordLower;
                        Set<string>.ReadOnly tags = suffixTree.GetTags(word);
                        foreach (string tag in tags)
                        {
                            AddFeature(string.Format("m({0}) {1}", offset, tag), featureSpace, extendFeatureSpace, featureVector);
                            if (tag.Length > 0)
                            {
                                AddFeature(string.Format("m1({0}) {1}", offset, tag[0]), featureSpace, extendFeatureSpace, featureVector);
                            }
                        }
                        string ambiguityClass = suffixTree.GetAmbiguityClass(word);
                        AddFeature(string.Format("t({0}) {1}", offset, ambiguityClass), featureSpace, extendFeatureSpace, featureVector);
                    }
                }
            }
#if NGRAM_FEATURES
            // *** bigrams and trigrams ***
            for (int n = 2; n <= 3; n++)
            {
                for (int offset = -2; offset <= 3 - n; offset++) // consider 4 bigrams and 3 trigrams
                {
                    string wordFeature = string.Format("w({0},{1})", n, offset);
                    string tagFeature = string.Format("t({0},{1})", n, offset);
                    string[] prefixFeature = new string[4];
                    string[] suffixFeature = new string[4];
                    for (int i = 0; i < 4; i++) // consider prefixes and suffixes of up to 4 letters
                    {
                        prefixFeature[i] = string.Format("p{0}({1},{2})", i, n, offset);
                        suffixFeature[i] = string.Format("s{0}({1},{2})", i, n, offset);
                    }
                    if (wordIdx + offset >= 0 && wordIdx + offset + (n - 1) < mTaggedWords.Count)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            int idx = wordIdx + offset + i;
                            string word = mTaggedWords[idx].WordLower;
                            wordFeature += " " + word;
                            for (int j = 0; j < 4; j++) // prefixes and suffixes
                            {
                                prefixFeature[j] += " " + GetPrefix(word, j);
                                suffixFeature[j] += " " + GetSuffix(word, j);
                            }
                            if (offset + i < 0) // tag is available iff offset + i < 0
                            {
                                tagFeature += " " + mTaggedWords[idx].Tag;
                            }
                            else // tag not available; use ambiguity class instead
                            {
                                string ambiguityClass = suffixTree.GetAmbiguityClass(word);
                                tagFeature += " " + ambiguityClass;
                            }
                        }
                        AddFeature(wordFeature, featureSpace, extendFeatureSpace, featureVector);
                        AddFeature(tagFeature, featureSpace, extendFeatureSpace, featureVector);
                        for (int i = 0; i < 4; i++) // add prefix and suffix features
                        {
                            AddFeature(prefixFeature[i], featureSpace, extendFeatureSpace, featureVector);
                            AddFeature(suffixFeature[i], featureSpace, extendFeatureSpace, featureVector);
                        }
                    }
                }
            }
#endif
            // character features
            foreach (char ch in mTaggedWords[wordIdx].Word)
            {
                // contains non-alphanum char?
                if (!char.IsLetterOrDigit(ch))
                {
                    AddFeature(string.Format("c{0}", ch), featureSpace, extendFeatureSpace, featureVector);
                }
                // contains number?
                if (char.IsDigit(ch))
                {
                    AddFeature("cd", featureSpace, extendFeatureSpace, featureVector);
                }
                // contains uppercase char?
                if (char.IsUpper(ch))
                {
                    AddFeature("cu", featureSpace, extendFeatureSpace, featureVector);
                }
            } 
            // starts with capital letter?
            if (mTaggedWords[wordIdx].Word.Length > 0 && char.IsUpper(mTaggedWords[wordIdx].Word[0]))
            {
                AddFeature("cl", featureSpace, extendFeatureSpace, featureVector);
            }
            // starts with capital letter and not first word?
            if (wordIdx > 0 && !mTaggedWords[wordIdx - 1].Tag.EndsWith("<eos>") && mTaggedWords[wordIdx].Word.Length > 0 && char.IsUpper(mTaggedWords[wordIdx].Word[0]))
            {
                AddFeature("cl+", featureSpace, extendFeatureSpace, featureVector);
            }
            return new BinaryVector(featureVector);
        }
    }
}

using System;
using System.Text;
using System.Collections.Generic;
using Latino;

// TODO: throw exceptions

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SuffixTrie
       |
       '-----------------------------------------------------------------------
    */
    public class SuffixTrie : ISerializable
    {
        private SuffixTrieNode m_root
            = new SuffixTrieNode('*');
        public SuffixTrie()
        { 
        }
        public SuffixTrie(BinarySerializer reader)
        {
            Load(reader);
        }
        public void AddWordTagPair(string word, string tag)
        {
            word = "#" + word;
            SuffixTrieNode current = m_root;
            int i = word.Length - 1;
            while (i >= 0 && current.m_children.ContainsKey(word[i]))
            {
                current = current.m_children[word[i]];
                i--;
            }
            while (i >= 0)
            {
                current.m_children.Add(word[i], current = new SuffixTrieNode(word[i]));
                i--;
            }
            if (current.m_tags == null)
            {
                current.m_tags = new Set<string>(new string[] { tag });
            }
            else
            {
                current.m_tags.Add(tag);
            }
        }
        public bool Contains(string word)
        {
            word = "#" + word;
            SuffixTrieNode current = m_root;
            int i = word.Length - 1;
            while (i >= 0 && current.m_children.ContainsKey(word[i]))
            {
                current = current.m_children[word[i]];
                i--;
            }
            return i == -1;
        }
        private void PropagateTags(SuffixTrieNode node)
        {
            if (node.m_children.Count == 1)
            {
                foreach (SuffixTrieNode child in node.m_children.Values)
                {
                    PropagateTags(child);
                    node.m_tags = child.m_tags;
                }                
            }
            else if (node.m_children.Count > 1)
            {
                Set<string> tags = new Set<string>();
                foreach (SuffixTrieNode child in node.m_children.Values)
                {
                    PropagateTags(child);
                    tags.AddRange(child.m_tags);
                }
                node.m_tags = tags;
            }
        }
        public void PropagateTags()
        {
            PropagateTags(m_root);
        }
        public Set<string>.ReadOnly GetTags(string word)
        {
            word = "#" + word;
            SuffixTrieNode current = m_root;
            int i = word.Length - 1;
            while (i >= 0 && current.m_children.ContainsKey(word[i]))
            {
                current = current.m_children[word[i]];
                i--;
            }
            return current.m_tags;
        }
        private string GetAmbiguityClass(Set<string>.ReadOnly tag_class)
        {
            if (tag_class.Count == 0) { return ""; }
            ArrayList<string> tags_sorted = new ArrayList<string>(tag_class);
            tags_sorted.Sort();
            string ambiguity_class = tags_sorted[0];
            for (int i = 1; i < tags_sorted.Count; i++)
            {
                ambiguity_class += string.Format("_{0}", tags_sorted[i]);
            }
            return ambiguity_class;
        }
        public string GetAmbiguityClass(string word)
        {
            Set<string>.ReadOnly tags = GetTags(word);
            return GetAmbiguityClass(tags);
        }
        private static void ToString(SuffixTrieNode node, StringBuilder str_builder, string offset)
        {
            str_builder.Append(offset);
            str_builder.Append(node.m_letter.ToString());
            if (node.m_tags != null) { str_builder.Append(string.Format(" {0}", node.m_tags)); }
            str_builder.AppendLine();
            foreach (SuffixTrieNode child_node in node.m_children.Values)
            {
                ToString(child_node, str_builder, offset + "  ");
            }
        }
        public override string ToString()
        {
            StringBuilder str_builder = new StringBuilder();
            ToString(m_root, str_builder, "");
            return str_builder.ToString();
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
            m_root.Save(writer);
        }
        public void Load(BinarySerializer reader)
        {
            m_root = new SuffixTrieNode(reader);
        }
        /* .-----------------------------------------------------------------------
           |
           |  Class SuffixTrieNode
           |
           '-----------------------------------------------------------------------
        */
        private class SuffixTrieNode : ISerializable
        {
            public char m_letter;
            public Set<string> m_tags
                = null;
            public Dictionary<char, SuffixTrieNode> m_children
                = new Dictionary<char, SuffixTrieNode>();
            public SuffixTrieNode(char letter)
            {
                m_letter = letter;
            }
            public SuffixTrieNode(BinarySerializer reader)
            {
                Load(reader);
            }
            // *** ISerializable interface implementation ***
            public void Save(BinarySerializer writer)
            {
                writer.WriteChar(m_letter);
                writer.WriteObject(m_tags);
                Utils.SaveDictionary(m_children, writer);                
            }
            public void Load(BinarySerializer reader)
            {
                m_letter = reader.ReadChar();
                m_tags = reader.ReadObject<Set<string>>();
                m_children = Utils.LoadDictionary<char, SuffixTrieNode>(reader);
            }
        }
    }
}

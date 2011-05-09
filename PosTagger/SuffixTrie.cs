/*==========================================================================;
 *
 *  Avtorske pravice za to izdajo programske opreme ureja licenca 
 *    Priznanje avtorstva-Nekomercialno-Brez predelav 2.5
 *  This work is licenced under the Creative Commons 
 *    Attribution-NonCommercial-NoDerivs 2.5 licence
 *
 *  Projekt Sporazumevanje v slovenskem jeziku: 
 *    http://www.slovenscina.eu/Vsebine/Sl/Domov/Domov.aspx
 *  Project Communication in Slovene: 
 *    http://www.slovenscina.eu/Vsebine/En/Domov/Domov.aspx
 *
 *  File:    SuffixTrie.cs
 *  Desc:    Suffix trie data struct
 *  Created: Sep-2009
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using Latino;

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SuffixTrieNode
       |
       '-----------------------------------------------------------------------
    */
    internal class SuffixTrieNode : ISerializable
    {
        public char mLetter;
        public Set<string> mTags
            = null;
        public Dictionary<char, SuffixTrieNode> mChildren
            = new Dictionary<char, SuffixTrieNode>();

        public SuffixTrieNode(char letter)
        {
            mLetter = letter;
        }

        public SuffixTrieNode(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions 
            writer.WriteChar(mLetter);
            writer.WriteObject(mTags);
            Utils.SaveDictionary(mChildren, writer);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions 
            mLetter = reader.ReadChar();
            mTags = reader.ReadObject<Set<string>>();
            mChildren = Utils.LoadDictionary<char, SuffixTrieNode>(reader);
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class SuffixTrie
       |
       '-----------------------------------------------------------------------
    */
    public class SuffixTrie : ISerializable
    {
        private SuffixTrieNode mRoot
            = new SuffixTrieNode('*');

        public SuffixTrie()
        { 
        }

        public SuffixTrie(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public void AddWordTagPair(string word, string tag)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            Utils.ThrowException(tag == null ? new ArgumentNullException("tag") : null);
            word = "#" + word;
            SuffixTrieNode current = mRoot;
            int i = word.Length - 1;
            while (i >= 0 && current.mChildren.ContainsKey(word[i]))
            {
                current = current.mChildren[word[i]];
                i--;
            }
            while (i >= 0)
            {
                current.mChildren.Add(word[i], current = new SuffixTrieNode(word[i]));
                i--;
            }
            if (current.mTags == null)
            {
                current.mTags = new Set<string>(new string[] { tag });
            }
            else
            {
                current.mTags.Add(tag);
            }
        }

        public bool Contains(string word)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            word = "#" + word;
            SuffixTrieNode current = mRoot;
            int i = word.Length - 1;
            while (i >= 0 && current.mChildren.ContainsKey(word[i]))
            {
                current = current.mChildren[word[i]];
                i--;
            }
            return i == -1;
        }

        private void PropagateTags(SuffixTrieNode node)
        {
            if (node.mChildren.Count == 1)
            {
                foreach (SuffixTrieNode child in node.mChildren.Values)
                {
                    PropagateTags(child);
                    node.mTags = child.mTags;
                }                
            }
            else if (node.mChildren.Count > 1)
            {
                Set<string> tags = new Set<string>();
                foreach (SuffixTrieNode child in node.mChildren.Values)
                {
                    PropagateTags(child);
                    tags.AddRange(child.mTags);
                }
                node.mTags = tags;
            }
        }

        public void PropagateTags()
        {
            PropagateTags(mRoot);
        }

        public Set<string>.ReadOnly GetTags(string word)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            word = "#" + word;
            SuffixTrieNode current = mRoot;
            int i = word.Length - 1;
            while (i >= 0 && current.mChildren.ContainsKey(word[i]))
            {
                current = current.mChildren[word[i]];
                i--;
            }
            return current.mTags;
        }

        private string GetAmbiguityClass(Set<string>.ReadOnly tagClass)
        {
            if (tagClass.Count == 0) { return ""; }
            ArrayList<string> tagsSorted = new ArrayList<string>(tagClass);
            tagsSorted.Sort();
            string ambiguityClass = tagsSorted[0];
            for (int i = 1; i < tagsSorted.Count; i++)
            {
                ambiguityClass += string.Format("_{0}", tagsSorted[i]);
            }
            return ambiguityClass;
        }

        public string GetAmbiguityClass(string word)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            Set<string>.ReadOnly tags = GetTags(word);
            return GetAmbiguityClass(tags);
        }

        private static void ToString(SuffixTrieNode node, StringBuilder strBuilder, string offset)
        {
            strBuilder.Append(offset);
            strBuilder.Append(node.mLetter.ToString());
            if (node.mTags != null) { strBuilder.Append(string.Format(" {0}", node.mTags)); }
            strBuilder.AppendLine();
            foreach (SuffixTrieNode childNode in node.mChildren.Values)
            {
                ToString(childNode, strBuilder, offset + "  ");
            }
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();
            ToString(mRoot, strBuilder, "");
            return strBuilder.ToString();
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            mRoot.Save(writer); // throws ArgumentNullException, serialization-related exceptions
        }

        public void Load(BinarySerializer reader)
        {
            mRoot = new SuffixTrieNode(reader); // throws ArgumentNullException, serialization-related exceptions
        }
    }
}

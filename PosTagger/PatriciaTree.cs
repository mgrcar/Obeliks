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
 *  File:    PatriciaTree.cs
 *  Desc:    Memory-efficient suffix tree implementation
 *  Created: May-2011
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using Latino;

namespace PosTagger
{
    /* .-----------------------------------------------------------------------
       |
       |  Class PatriciaTree
       |
       '-----------------------------------------------------------------------
    */
    public class PatriciaTree : ISerializable
    {
        /* .-----------------------------------------------------------------------
           |
           |  Class Node
           |
           '-----------------------------------------------------------------------
        */
        private class Node : ISerializable
        {
            public string mSubstr
                = "*";
            public ArrayList<ushort> mTags
                = null;
            public ArrayList<Node> mChildren
                = null;

            public Node()
            { 
            }

            public Node(BinarySerializer reader)
            {
                Load(reader); 
            }

            public void AddTag(ushort tag)
            {
                if (mTags == null) { mTags = new ArrayList<ushort>(new ushort[] { tag }); }
                else if (!mTags.Contains(tag)) { mTags.Add(tag); }
            }

            public void AddChild(Node child)
            {
                if (mChildren == null) { mChildren = new ArrayList<Node>(new Node[] { child }); }
                else { mChildren.Add(child); }
            }

            public int CommonPrefix(string str1, string str2)
            {
                int len = Math.Min(str1.Length, str2.Length);
                for (int i = 0; i < len; i++)
                {
                    if (str1[i] != str2[i]) { return i; }
                }
                return len;
            }

            //public void Output(string prefix)
            //{
            //    Console.WriteLine(prefix + mSubstr);
            //    if (mChildren != null)
            //    {
            //        foreach (Node child in mChildren)
            //        {
            //            child.Output(prefix + "  ");
            //        }
            //    }
            //}

            public bool Contains(string substr)
            {
                if (substr.StartsWith(mSubstr, StringComparison.Ordinal))
                {
                    // case 1: exact match
                    if (mSubstr.Length == substr.Length)
                    {
                        return true;
                    }
                    // case 2: substr goes over
                    else
                    {
                        string newSubstr = substr.Substring(mSubstr.Length);
                        if (mChildren != null)
                        {
                            foreach (Node child in mChildren)
                            {
                                if (child.Contains(newSubstr)) { return true; }
                            }
                        }
                        return false;
                    }
                }
                // other cases
                else
                {
                    return false;
                }
            }

            public Node GetNode(string substr)
            {
                if (substr.StartsWith(mSubstr, StringComparison.Ordinal))
                {
                    // case 1: exact match
                    if (mSubstr.Length == substr.Length)
                    {
                        return this;
                    }
                    // case 2: substr goes over
                    else
                    {
                        string newSubstr = substr.Substring(mSubstr.Length);
                        if (mChildren != null)
                        {
                            foreach (Node child in mChildren)
                            {
                                if (child.mSubstr.StartsWith(newSubstr[0].ToString(), StringComparison.Ordinal))
                                {
                                    return child.GetNode(newSubstr);
                                }
                            }
                        }
                        return this;
                    }
                }
                // other cases
                else
                {
                    return this;
                }
            }

            public bool Insert(string substr, ushort tag)
            {
                if (substr.StartsWith(mSubstr, StringComparison.Ordinal))
                {
                    // case 1: exact match
                    if (mSubstr.Length == substr.Length)
                    {
                        AddTag(tag);
                    }
                    // case 2: substr goes over 
                    else
                    {            
                        bool success = false;
                        string newSubstr = substr.Substring(mSubstr.Length);
                        if (mChildren != null)
                        {
                            foreach (Node child in mChildren)
                            {
                                if (child.Insert(newSubstr, tag)) { success = true; break; }
                            }
                        }
                        if (!success)
                        {
                            Node newNode = new Node();
                            newNode.mSubstr = newSubstr;
                            newNode.AddTag(tag);
                            AddChild(newNode);
                        }
                    }
                    return true;
                }               
                else
                {                                        
                    int prefixLen = CommonPrefix(substr, mSubstr);
                    // case 3: substr stops in the middle 
                    if (prefixLen > 0)
                    {
                        // first child
                        Node child1 = new Node();
                        child1.mSubstr = mSubstr.Substring(prefixLen);
                        child1.mTags = mTags;
                        child1.mChildren = mChildren;
                        // second child
                        Node child2 = new Node();
                        child2.mSubstr = substr.Substring(prefixLen);
                        child2.mTags = new ArrayList<ushort>(new ushort[] { tag });
                        // this node
                        mTags = null;
                        mChildren = new ArrayList<Node>(new Node[] { child1, child2 });
                        mSubstr = substr.Substring(0, prefixLen);                        
                        return true;
                    }
                    // case 4: no match 
                    else
                    {
                        return false;
                    }
                }
            }

            public void CollectTags(Set<ushort> tags)
            {
                if (mTags != null) { tags.AddRange(mTags); }
                if (mChildren != null)
                {
                    foreach (Node child in mChildren)
                    {
                        child.CollectTags(tags);
                    }
                }
            }

            public IEnumerable<ushort> PropagateTags()
            {
                if (mTags != null) { return mTags; }
                Set<ushort> tags = new Set<ushort>();
                if (mChildren != null)
                {
                    foreach (Node child in mChildren)
                    {
                        tags.AddRange(child.PropagateTags());
                    }
                }
                mTags = new ArrayList<ushort>(tags);
                return tags;
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                writer.WriteString(mSubstr);
                writer.WriteObject(mTags);
                writer.WriteObject(mChildren);
            }

            public void Load(BinarySerializer reader)
            {
                mSubstr = reader.ReadString();
                mTags = reader.ReadObject<ArrayList<ushort>>();
                mChildren = reader.ReadObject<ArrayList<Node>>();
            }
        }

        private Dictionary<string, ushort> mTagToIdMap
            = new Dictionary<string, ushort>();
        private ArrayList<string> mIdToTagMap
            = new ArrayList<string>();

        private Node mRoot
            = new Node();

        public PatriciaTree()
        {         
        }

        public PatriciaTree(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        private string Reverse(string str)
        {
            string revStr = "";
            foreach (char ch in str) { revStr = ch + revStr; }
            return revStr;
        }

        private ushort GetId(string tag)
        {
            ushort id;
            if (mTagToIdMap.TryGetValue(tag, out id))
            {
                return id;
            }
            else
            {
                id = (ushort)mIdToTagMap.Count;
                mIdToTagMap.Add(tag);
                mTagToIdMap.Add(tag, id);
                return id;
            }
        }

        public bool AddWordTagPair(string word, string tag)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            Utils.ThrowException(tag == null ? new ArgumentNullException("tag") : null); 
            word = "*" + Reverse(word) + (char)0;
            return mRoot.Insert(word, GetId(tag));
        }

        public bool Contains(string word)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            word = "*" + Reverse(word) + (char)0;
            return mRoot.Contains(word);
        }

        private Set<string> Convert(IEnumerable<ushort> tagIds)
        {
            Set<string> tags = new Set<string>();
            foreach (ushort id in tagIds)
            {
                if (id >= 0 && id < mIdToTagMap.Count)
                {
                    tags.Add(mIdToTagMap[id]);
                }
            }
            return tags;
        }

        public Set<string> GetTags(string word)
        {
            Utils.ThrowException(word == null ? new ArgumentNullException("word") : null);
            word = "*" + Reverse(word) + (char)0;
            Node node = mRoot.GetNode(word);
            if (node.mTags != null)
            {
                return Convert(node.mTags);
            }
            else
            {
                Set<ushort> tags = new Set<ushort>();
                node.CollectTags(tags);
                return Convert(tags);
            }
        }

        public void PropagateTags()
        {
            mRoot.PropagateTags();
        }

        private string GetAmbiguityClass(Set<string> tagClass)
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
            Set<string> tags = GetTags(word); // throws ArgumentNullException
            return GetAmbiguityClass(tags);
        }

        //public void Output()
        //{
        //    mRoot.Output("");
        //}

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            mRoot.Save(writer);
            mIdToTagMap.Save(writer);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            mRoot.Load(reader);
            mIdToTagMap.Load(reader);
            mTagToIdMap.Clear();
            for (int i = 0; i < mIdToTagMap.Count; i++)
            {
                mTagToIdMap.Add(mIdToTagMap[i], (ushort)i);
            }
        }
    }
}
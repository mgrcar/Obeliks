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
 *  File:    PatriciaTree.cs
 *  Desc:    Memory-efficient suffix tree implementation
 *  Created: May-2011
 *
 *  Authors: Miha Grcar
 *
 ***************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
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
            public ArrayList<string> mTags
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

            public void AddTag(string tag)
            {
                if (mTags == null) { mTags = new ArrayList<string>(new string[] { tag }); }
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
                if (substr.StartsWith(mSubstr))
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
                if (substr.StartsWith(mSubstr))
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
                                if (child.mSubstr.StartsWith(newSubstr[0].ToString()))
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

            public bool Insert(string substr, string tag)
            {
                if (substr.StartsWith(mSubstr))
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
                        child2.mTags = new ArrayList<string>(new string[] { tag });
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

            public void CollectTags(Set<string> tags)
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

            public IEnumerable<string> PropagateTags()
            {
                if (mTags != null) { return mTags; }
                Set<string> tags = new Set<string>();
                if (mChildren != null)
                {
                    foreach (Node child in mChildren)
                    {
                        tags.AddRange(child.PropagateTags());
                    }
                }
                mTags = new ArrayList<string>(tags);
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
                mTags = reader.ReadObject<ArrayList<string>>();
                mChildren = reader.ReadObject<ArrayList<Node>>();
            }
        }

        private Node mRoot
            = new Node();

        public PatriciaTree()
        {         
        }

        public PatriciaTree(BinarySerializer reader)
        {
            Load(reader);
        }

        private string Reverse(string str)
        {
            string revStr = "";
            foreach (char ch in str) { revStr = ch + revStr; }
            return revStr;
        }

        public bool AddWordTagPair(string word, string tag)
        {
            word = "*" + Reverse(word) + (char)0;
            return mRoot.Insert(word, tag);
        }

        public bool Contains(string word)
        {
            word = "*" + Reverse(word) + (char)0;
            return mRoot.Contains(word);
        }

        public Set<string> GetTags(string word)
        {
            word = "*" + Reverse(word) + (char)0;
            Node node = mRoot.GetNode(word);
            if (node.mTags != null)
            {
                return new Set<string>(node.mTags);
            }
            else
            {
                Set<string> tags = new Set<string>();
                node.CollectTags(tags);
                return tags;
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
            Set<string> tags = GetTags(word);
            return GetAmbiguityClass(tags);
        }

        //public void Output()
        //{
        //    mRoot.Output("");
        //}

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            mRoot.Save(writer);
        }

        public void Load(BinarySerializer reader)
        {
            mRoot.Load(reader);
        }
    }
}
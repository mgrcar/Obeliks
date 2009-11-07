/*==========================================================================;
 *
 *  (copyright)
 *
 *  File:          MetaTaggerTag\Tree.cs
 *  Version:       1.0
 *  Desc:		   Meta-tagger decision tree 
 *  Author:		   Miha Grcar
 *  Created on:    Jun-2009
 *  Last modified: Aug-2009
 *  Revision:      N/A
 *
 ***************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Latino;

namespace MetaTagger
{
    public class Tree
    {
        private class Node
        {
            public Node Parent
                = null;
            public ArrayList<Node> Children
                = new ArrayList<Node>();
            public string Condition;
            public int TargetClass;
            public Node(string attr, string val, int target_class)
            {
                Condition = string.Format("{0} = {1}", attr, val);
                TargetClass = target_class;
            }
        }

        private Node m_root
            = new Node("<root>", null, -1);

        public Tree(string file_name)
        {
            Regex regex = new Regex(@"^(?<tabs>\t*)(?<attr>[^\s]+)\s=\s(?<val>[^\s]*)\s=>\sTagger(?<class>[12])", RegexOptions.Compiled);
            StreamReader reader = new StreamReader(file_name);
            Node node = m_root;
            int level = -1;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                Match match = regex.Match(line);
                if (match.Success)
                {
                    int tabs = match.Result("${tabs}").Length;
                    string attr = match.Result("${attr}");
                    string val = match.Result("${val}");
                    int target_class = Convert.ToInt32(match.Result("${class}"));
                    //Console.WriteLine("{0} = {1}", attr, val);
                    int diff = tabs - level;
                    Node new_node = new Node(attr, val, target_class);
                    if (diff == 0) 
                    {
                        new_node.Parent = node.Parent;
                        node.Parent.Children.Add(new_node);
                    }
                    else if (diff == 1)
                    {
                        new_node.Parent = node;
                        node.Children.Add(new_node);
                    }
                    else if (diff < 0)
                    {
                        for (int i = 0; i < -diff + 1; i++) { node = node.Parent; }
                        new_node.Parent = node;
                        node.Children.Add(new_node);
                    }
                    level = tabs;
                    node = new_node;
                }               
            }
            reader.Close();
        }

        private void ToString(Node node, string tab, StringBuilder str_build)
        {
            string new_tab = "";
            if (node != m_root)
            {
                str_build.Append(tab);
                str_build.Append(node.Condition);
                str_build.Append(" => Tagger");
                str_build.AppendLine(node.TargetClass.ToString());
                new_tab = tab + "\t";
            }
            foreach (Node child in node.Children)
            {
                ToString(child, new_tab, str_build);
            }
        }

        public override string ToString()
        {
            StringBuilder str_build = new StringBuilder();
            ToString(m_root, "", str_build);
            return str_build.ToString();
        }

        public int Classify(IEnumerable<KeyDat<string, string>> example)
        {
            Set<string> attributes = new Set<string>();
            foreach (KeyDat<string, string> attribute in example)
            {
                attributes.Add(string.Format("{0} = {1}", attribute.Key, attribute.Dat));
            }
            Node node = m_root;
            int target_class = -1;
            while (node != null)
            {
                Node match = null;
                foreach (Node child in node.Children)
                {
                    if (attributes.Contains(child.Condition))
                    {
                        match = child;
                        target_class = child.TargetClass;
                        break;
                    }
                }
                node = match;
            }
            return target_class;
        }
    }
}

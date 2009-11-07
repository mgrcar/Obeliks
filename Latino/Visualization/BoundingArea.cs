/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          BoundingArea.cs
 *  Version:       1.0
 *  Desc:		   Bounding area as set of rectangles
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: Jun-2009
 *  Revision:      Jun-2009
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Collections.Generic;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |		 
       |  Class BoundingArea 
       |
       '-----------------------------------------------------------------------
    */
    public class BoundingArea : ICloneable<BoundingArea>
    {
        private ArrayList<RectangleF> m_rects
            = new ArrayList<RectangleF>();
        private RectangleF m_bounding_box
            = RectangleF.Empty;
        public BoundingArea()
        {
        }
        public BoundingArea(float left, float top, float width, float height)
        {
            m_rects = new ArrayList<RectangleF>(new RectangleF[] { new RectangleF(left, top, width, height) });
            UpdateBoundingBox();
        }
        public BoundingArea(RectangleF rect) 
        {
            m_rects = new ArrayList<RectangleF>(new RectangleF[] { rect });
            UpdateBoundingBox();
        }
        public BoundingArea(IEnumerable<RectangleF> rects) 
        {
            m_rects = new ArrayList<RectangleF>(rects); // throws ArgumentNullException
            if (m_rects.Count > 0) { UpdateBoundingBox(); }
        }
        public ArrayList<RectangleF>.ReadOnly Rectangles
        {
            get { return m_rects; }
        }
        public void AddRectangles(params RectangleF[] rects)
        {
            AddRectangles((IEnumerable<RectangleF>)rects); // throws ArgumentNullException
        }
        public void AddRectangles(IEnumerable<RectangleF> rects)
        {
            Utils.ThrowException(rects == null ? new ArgumentNullException("rects") : null);
            m_rects.AddRange(rects);
            if (m_rects.Count > 0) { UpdateBoundingBox(); }
        }
        public void Transform(TransformParams tr)
        {
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            for (int i = 0; i < m_rects.Count; i++)
            {
                m_rects[i] = tr.Transform(m_rects[i]);
            }
            m_bounding_box = tr.Transform(m_bounding_box);
        }
        public void Inflate(float x, float y)
        {
            Utils.ThrowException(x < 0 ? new ArgumentOutOfRangeException("x") : null);
            Utils.ThrowException(y < 0 ? new ArgumentOutOfRangeException("y") : null);
            for (int i = 0; i < m_rects.Count; i++)
            {
                RectangleF rect = m_rects[i];
                rect.Inflate(x, y);
                m_rects[i] = rect;
            }
            m_bounding_box.Inflate(x, y);
        }
        public bool IntersectsWith(BoundingArea.ReadOnly other) 
        {
            Utils.ThrowException(other == null ? new ArgumentNullException("other") : null);
            if (m_bounding_box.IntersectsWith(other.BoundingBox))
            {
                foreach (RectangleF rect in m_rects)
                {
                    foreach (RectangleF other_rect in other.Rectangles)
                    {
                        if (rect.IntersectsWith(other_rect)) { return true; }
                    }
                }
            }
            return false;
        }
        public RectangleF BoundingBox
        {
            get { return m_bounding_box; }
        }
        private void UpdateBoundingBox() 
        {
            float min_x = float.MaxValue, min_y = float.MaxValue;
            float max_x = float.MinValue, max_y = float.MinValue;
            foreach (RectangleF rect in m_rects)
            {
                if (rect.X < min_x) { min_x = rect.X; }
                if (rect.X + rect.Width > max_x) { max_x = rect.X + rect.Width; }
                if (rect.Y < min_y) { min_y = rect.Y; }
                if (rect.Y + rect.Height > max_y) { max_y = rect.Y + rect.Height; }
            }
            m_bounding_box = new RectangleF(min_x, min_y, max_x - min_x, max_y - min_y);
        }
        public void Optimize()
        {
            m_rects = RTree.FullyOptimizeBoundingArea(this);
        }
        // *** ICloneable<BoundingArea> interface implementation ***
        public BoundingArea Clone()
        {
            BoundingArea clone = new BoundingArea();
            clone.m_rects = m_rects.Clone();
            clone.m_bounding_box = m_bounding_box;
            return clone;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        // *** Implicit cast to a read-only adapter ***
        public static implicit operator BoundingArea.ReadOnly(BoundingArea bounding_area)
        {
            if (bounding_area == null) { return null; }
            return new BoundingArea.ReadOnly(bounding_area);
        }
        /* .-----------------------------------------------------------------------
           |		 
           |  Class BoundingArea.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<BoundingArea>
        {
            private BoundingArea m_bounding_area;
            public ReadOnly(BoundingArea bounding_area)
            {
                Utils.ThrowException(bounding_area == null ? new ArgumentNullException("bounding_area") : null);
                m_bounding_area = bounding_area;
            }
            public ArrayList<RectangleF>.ReadOnly Rectangles
            {
                get { return m_bounding_area.Rectangles; }
            }
            public bool IntersectsWith(BoundingArea.ReadOnly other)
            {
                return m_bounding_area.IntersectsWith(other);
            }
            public RectangleF BoundingBox 
            {
                get { return m_bounding_area.BoundingBox; }
            }
            // *** IReadOnlyAdapter interface implementation ***
            public BoundingArea GetWritableCopy()
            {
                return m_bounding_area.Clone();
            }
            object IReadOnlyAdapter.GetWritableCopy()
            {
                return GetWritableCopy();
            }
#if PUBLIC_INNER
            public
#else
            internal
#endif
            BoundingArea Inner
            {
                get { return m_bounding_area; }
            }
        }
        /* .-----------------------------------------------------------------------
           |
           |  Class RTree
           |
           '-----------------------------------------------------------------------
        */
        public class RTree
        {
            private const int ENTRIES_PER_NODE
                = 3;
            private const int MIN_ENTRIES_PER_NODE
                = 1;
            private Node m_root
                = new Node();
            //private static StreamWriter writer
            //    = new StreamWriter("c:\\r_tree_log.txt");
            // 
            // *** Utilities ***
            //
            private Node ChooseLeaf(Entry new_entry)
            {
                //writer.Write("ChooseLeaf({0})", new_entry.Id);
                // CL1: initialize
                Node node = m_root;
                while (!node.IsLeaf) // CL2: leaf check
                {
                    // CL3: choose subtree
                    Entry min_diff_entry = null;
                    float min_diff = float.MaxValue;
                    float min_area = float.MaxValue;
                    foreach (Entry entry in node.Entries)
                    {
                        float entry_area = entry.GetArea();
                        RectangleF bb = RectangleF.Union(entry.BoundingBox, new_entry.BoundingBox);
                        float area_diff = bb.Width * bb.Height - entry_area;
                        if (area_diff < min_diff)
                        {
                            min_diff = area_diff;
                            min_diff_entry = entry;
                            min_area = entry_area;
                        }
                        else if (area_diff == min_diff && entry_area < min_area)
                        {
                            min_diff_entry = entry;
                            min_area = entry_area;
                        }
                    }
                    // CL4: descend until a leaf is reached
                    node = min_diff_entry.ChildNode;
                }
                //writer.WriteLine("->{0}", node);
                //writer.Flush();
                return node;
            }
            private void AdjustTree(Node node_1, Node node_2)
            {
                while (node_1 != m_root) // AT2: check if done
                {
                    Node parent_1 = node_1.Parent.Owner;
                    // AT3: adjust covering rectangle in parent entry
                    node_1.Parent.UpdateBoundingBox();
                    // AT4: propagate node split upward
                    if (node_2 != null)
                    {
                        Entry entry = new Entry();
                        entry.ChildNode = node_2;
                        entry.UpdateBoundingBox();
                        if (parent_1.Entries.Count < ENTRIES_PER_NODE)
                        {
                            parent_1.AddEntry(entry);
                            node_2 = null;
                        }
                        else
                        {
                            Node parent_2 = SplitNode(parent_1, entry);
                            node_2 = parent_2;
                            if (parent_1 == m_root)
                            {
                                // (I4: grow tree taller)
                                m_root = new Node();
                                Entry entry_1 = new Entry();
                                Entry entry_2 = new Entry();
                                entry_1.ChildNode = parent_1;
                                entry_1.UpdateBoundingBox();
                                entry_2.ChildNode = parent_2;
                                entry_2.UpdateBoundingBox();
                                m_root.AddEntry(entry_1);
                                m_root.AddEntry(entry_2);
                                break;
                            }
                        }
                    }
                    // AT5: move up to next level
                    node_1 = parent_1;
                }                
            }
            private void PickSeeds(ArrayList<Entry> entries, ref Entry seed_1, ref Entry seed_2)
            {
                // PS1: calculate inefficiency of grouping entries together            
                float max_diff = float.MinValue;
                Pair<int, int> max_diff_pair = new Pair<int, int>(-1, -1);
                for (int i = 0; i < entries.Count; i++)
                {
                    for (int j = 0; j < entries.Count; j++)
                    {
                        if (i != j)
                        {
                            RectangleF bb = RectangleF.Union(entries[i].BoundingBox, entries[j].BoundingBox);
                            float diff = bb.Width * bb.Height - entries[i].GetArea() - entries[j].GetArea();
                            if (diff > max_diff)
                            {
                                max_diff = diff;
                                max_diff_pair = new Pair<int, int>(i, j);
                            }
                        }
                    }
                }
                // PS2: choose the most wasteful pair
                seed_1 = entries[max_diff_pair.First];
                seed_2 = entries[max_diff_pair.Second];
                if (max_diff_pair.First > max_diff_pair.Second)
                {
                    entries[max_diff_pair.First] = entries.Last;
                    entries.RemoveRange(entries.Count - 1, 1);
                    entries[max_diff_pair.Second] = entries.Last;
                    entries.RemoveRange(entries.Count - 1, 1);
                }
                else
                {
                    entries[max_diff_pair.Second] = entries.Last;
                    entries.RemoveRange(entries.Count - 1, 1);
                    entries[max_diff_pair.First] = entries.Last;
                    entries.RemoveRange(entries.Count - 1, 1);
                }
            }
            private Entry PickNext(ArrayList<Entry> entries, RectangleF bb_1, RectangleF bb_2, ref float area_1, ref float area_2,
                ref RectangleF ret_bb_1, ref RectangleF ret_bb_2, ref float ret_diff_1, ref float ret_diff_2)
            {
                // PN1: determine cost of putting each entry in each group
                float max_diff = float.MinValue;
                int max_diff_idx = -1;
                area_1 = bb_1.Width * bb_1.Height;
                area_2 = bb_2.Width * bb_2.Height;
                for (int i = 0; i < entries.Count; i++)
                {
                    RectangleF new_bb_1 = RectangleF.Union(bb_1, entries[i].BoundingBox);
                    RectangleF new_bb_2 = RectangleF.Union(bb_2, entries[i].BoundingBox);
                    float area_diff_1 = new_bb_1.Width * new_bb_1.Height - area_1;
                    float area_diff_2 = new_bb_2.Width * new_bb_2.Height - area_2;
                    float diff = Math.Abs(area_diff_1 - area_diff_2);
                    if (diff > max_diff)
                    {
                        max_diff = diff;
                        max_diff_idx = i;
                        ret_bb_1 = new_bb_1;
                        ret_bb_2 = new_bb_2;
                        ret_diff_1 = area_diff_1;
                        ret_diff_2 = area_diff_2;
                    }
                }
                // PN2: find entry with greatest preference for one group
                Entry max_diff_entry = entries[max_diff_idx];
                entries[max_diff_idx] = entries.Last;
                entries.RemoveRange(entries.Count - 1, 1);
                return max_diff_entry;
            }
            private Node SplitNode(Node node, Entry entry)
            {
                ArrayList<Entry> entries = new ArrayList<Entry>(ENTRIES_PER_NODE + 1);
                entries.Add(entry);
                entries.AddRange(node.Entries);
                // QS1: pick first entry for each group
                Entry seed_1 = null, seed_2 = null;
                PickSeeds(entries, ref seed_1, ref seed_2);
                node.Entries.Clear();
                node.AddEntry(seed_1);
                Node node_2 = new Node();
                node_2.AddEntry(seed_2);
                RectangleF bb_1 = seed_1.BoundingBox;
                RectangleF bb_2 = seed_2.BoundingBox;
                while (entries.Count > 0)
                {
                    // QS2: check if done
                    if (node.Entries.Count + entries.Count <= MIN_ENTRIES_PER_NODE)
                    {
                        node.AddEntries(entries);
                        return node_2;
                    }
                    if (node_2.Entries.Count + entries.Count <= MIN_ENTRIES_PER_NODE)
                    {
                        node_2.AddEntries(entries);
                        return node_2;
                    }
                    // QS3: select entry to assign
                    float area_1 = 0, area_2 = 0;
                    RectangleF new_bb_1 = RectangleF.Empty, new_bb_2 = RectangleF.Empty;
                    float area_diff_1 = 0, area_diff_2 = 0;
                    Entry next_entry = PickNext(entries, bb_1, bb_2, ref area_1, ref area_2, ref new_bb_1, ref new_bb_2, ref area_diff_1, ref area_diff_2);
                    if (area_diff_1 < area_diff_2)
                    {
                        node.AddEntry(next_entry);
                        bb_1 = new_bb_1;
                    }
                    else if (area_diff_1 > area_diff_2)
                    {
                        node_2.AddEntry(next_entry);
                        bb_2 = new_bb_2;
                    }
                    else if (area_1 < area_2)
                    {
                        node.AddEntry(next_entry);
                        bb_1 = new_bb_1;
                    }
                    else if (area_1 > area_2)
                    {
                        node_2.AddEntry(next_entry);
                        bb_2 = new_bb_2;
                    }
                    else if (node.Entries.Count < node_2.Entries.Count)
                    {
                        node.AddEntry(next_entry);
                        bb_1 = new_bb_1;
                    }
                    else
                    {
                        node_2.AddEntry(next_entry);
                        bb_2 = new_bb_2;
                    }
                }
                return node_2;
            }
            //
            // *** Insertion ***
            //
            private void Insert(Entry entry)
            {
                // I1: find position for new record
                Node leaf = ChooseLeaf(entry);
                // I2: add record to leaf node
                Node leaf_2 = null;
                if (leaf.Entries.Count < ENTRIES_PER_NODE)
                {
                    leaf.AddEntry(entry);
                }
                else
                {
                    leaf_2 = SplitNode(leaf, entry);
                }
                // I3: propagate changes upward
                if (leaf == m_root)
                {
                    if (leaf_2 != null)
                    {
                        // I4: grow tree taller
                        m_root = new Node();
                        Entry entry_1 = new Entry();
                        Entry entry_2 = new Entry();
                        entry_1.ChildNode = leaf;
                        entry_1.UpdateBoundingBox();
                        entry_2.ChildNode = leaf_2;
                        entry_2.UpdateBoundingBox();
                        m_root.AddEntry(entry_1);
                        m_root.AddEntry(entry_2);
                    }
                }
                else
                {
                    AdjustTree(leaf, leaf_2);
                }
                //writer.WriteLine(ToString());
                //writer.Flush();
            }
            private Entry Insert(RectangleF rect)
            {
                Entry entry = new Entry(rect);
                Insert(entry);
                return entry;
            }
            //
            // *** Deletion (fast, no balancing) ***
            //
            private bool RemoveEntry(Entry entry)
            {
                Node node = entry.Owner;
                for (int i = 0; i < node.Entries.Count; i++)
                {
                    if (node.Entries[i] == entry)
                    {
                        node.Entries[i] = node.Entries.Last;
                        node.Entries.RemoveRange(node.Entries.Count - 1, 1);
                        break;
                    }
                }
                return node.Entries.Count == 0;
            }
            private void Delete(Entry entry)
            {
                while (RemoveEntry(entry))
                {
                    entry = entry.Owner.Parent;
                    if (entry == null) { break; }
                    entry.ChildNode = null;
                }
                if (entry != null)
                {
                    if (entry.Owner.Parent != null)
                    {
                        entry.Owner.Parent.UpdateBoundingBox();
                    }
                }
            }
            private void Delete(IEnumerable<Entry> entries)
            {
                foreach (Entry entry in entries)
                {
                    Delete(entry);
                }
            }
            //
            // *** Debugging ***
            //
            //private void ToString(Node node, string prefix, StringBuilder str_bld)
            //{
            //    str_bld.Append(prefix);
            //    str_bld.AppendLine(node.ToString());
            //    if (!node.IsLeaf)
            //    {
            //        foreach (Entry entry in node.Entries)
            //        {
            //            ToString(entry.ChildNode, prefix + "\t", str_bld);
            //        }
            //    }
            //}
            //public override string ToString()
            //{
            //    StringBuilder str_bld = new StringBuilder();
            //    ToString(m_root, "", str_bld);
            //    return str_bld.ToString();
            //}
            //
            // *** Bounding area optimization ***
            //
            private void Fetch(RectangleF rect, float rect_area, Node node, ArrayList<Entry> result)
            {
                if (node.IsLeaf)
                {
                    foreach (Entry entry in node.Entries)
                    {
                        RectangleF bb = RectangleF.Union(entry.BoundingBox, rect);
                        if (entry.GetArea() + rect_area > bb.Width * bb.Height)
                        {
                            result.Add(entry);
                        }
                    }
                }
                else
                {
                    foreach (Entry entry in node.Entries)
                    {
                        RectangleF bb = RectangleF.Union(entry.BoundingBox, rect);
                        if (entry.GetArea() + rect_area > bb.Width * bb.Height)
                        {
                            Fetch(rect, rect_area, entry.ChildNode, result);
                        }
                    }
                }
            }
            private ArrayList<Entry> Fetch(RectangleF rect)
            {
                ArrayList<Entry> result = new ArrayList<Entry>();
                Fetch(rect, rect.Width * rect.Height, m_root, result);
                return result;
            }
            private Entry GetAnyLeafEntry()
            {
                Node node = m_root;
                while (!node.IsLeaf) 
                { 
                    node = node.Entries[0].ChildNode; 
                }
                return node.Entries[0];
            }
            private static ArrayList<RectangleF> OptimizeBoundingArea(IEnumerable<RectangleF> bounding_area)
            {
                RTree r_tree = new RTree();
                int rect_count = 0;
                foreach (RectangleF rect in bounding_area)
                {
                    r_tree.Insert(rect);
                    rect_count++;
                }
                ArrayList<RectangleF> result = new ArrayList<RectangleF>(rect_count);
                while (r_tree.m_root.Entries.Count > 0)
                {
                    Entry entry = r_tree.GetAnyLeafEntry();
                    RectangleF bb = entry.BoundingBox;
                    float area = 0;
                    while (bb.Width * bb.Height - area > 0.1f) // *** increase this threshold?
                    {
                        area = bb.Width * bb.Height;                            
                        ArrayList<Entry> query_result = r_tree.Fetch(bb);
                        r_tree.Delete(query_result);
                        foreach (Entry result_entry in query_result)
                        {
                            bb = RectangleF.Union(bb, result_entry.BoundingBox);                                    
                        }
                    }
                    result.Add(bb);
                }
                return result;
            }
            public static ArrayList<RectangleF> OptimizeBoundingArea(BoundingArea bounding_area)
            {
                return OptimizeBoundingArea(bounding_area.Rectangles);
            }
            public static ArrayList<RectangleF> FullyOptimizeBoundingArea(BoundingArea bounding_area)
            {
                int rect_count = bounding_area.Rectangles.Count;
                ArrayList<RectangleF> rects = OptimizeBoundingArea(bounding_area.Rectangles);
                while (rects.Count < rect_count)
                {
                    rect_count = rects.Count;
                    rects = OptimizeBoundingArea(rects);
                }
                return rects;
            }
            /* .-----------------------------------------------------------------------
               |
               |  Class Entry
               |
               '-----------------------------------------------------------------------
            */
            private class Entry
            {
                //private static int m_entry_id
                //    = 0; 
                private Node m_owner
                    = null;
                private Node m_child_node
                    = null;
                private RectangleF m_bounding_box
                    = RectangleF.Empty;
                //private int m_id
                //    = ++m_entry_id;
                public Entry()
                {
                }
                public Entry(RectangleF rect)
                {
                    m_bounding_box = rect;
                }
                //public int Id
                //{
                //    get { return m_id; }
                //}
                public float GetArea()
                {
                    return BoundingBox.Width * BoundingBox.Height;
                }
                public void UpdateBoundingBox()
                {
                    if (ChildNode != null && ChildNode.Entries.Count > 0)
                    {
                        m_bounding_box = ChildNode.Entries[0].m_bounding_box;
                        for (int i = 1; i < ChildNode.Entries.Count; i++)
                        {
                            m_bounding_box = RectangleF.Union(m_bounding_box, ChildNode.Entries[i].m_bounding_box);
                        }
                    }
                }
                public Node ChildNode
                {
                    get { return m_child_node; }
                    set
                    {
                        m_child_node = value;
                        if (value != null)
                        {
                            value.Parent = this;
                        }
                    }
                }
                public Node Owner
                {
                    get { return m_owner; }
                    set { m_owner = value; } // *** used only by Node
                }
                public RectangleF BoundingBox
                {
                    get { return m_bounding_box; }
                    set { m_bounding_box = value; }
                }
            }
            /* .-----------------------------------------------------------------------
               |
               |  Class Node
               |
               '-----------------------------------------------------------------------
            */
            private class Node
            {
                private Entry m_parent
                    = null;
                private ArrayList<Entry> m_entries
                    = new ArrayList<Entry>(ENTRIES_PER_NODE);
                public bool IsLeaf
                {
                    get { return m_entries.Count == 0 || m_entries[0].ChildNode == null; }
                }
                public Entry Parent
                {
                    get { return m_parent; }
                    set { m_parent = value; } // *** used only by Entry
                }
                public ArrayList<Entry> Entries
                {
                    get { return m_entries; }
                }
                public void AddEntry(Entry entry)
                {
                    m_entries.Add(entry);
                    entry.Owner = this;
                }
                public void AddEntries(IEnumerable<Entry> entries)
                {
                    foreach (Entry entry in entries)
                    {
                        AddEntry(entry);
                    }
                }
                //public override string ToString()
                //{
                //    string node_str = "(";
                //    foreach (Entry entry in m_entries)
                //    {
                //        node_str += entry.Id + ",";
                //    }
                //    return node_str.TrimEnd(',') + ")";
                //}
            }
        }
    }
}
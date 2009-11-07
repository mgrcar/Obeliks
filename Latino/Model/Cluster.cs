/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Cluster.cs
 *  Version:       1.0
 *  Desc:		   Holds information about a cluster
 *  Author:        Miha Grcar 
 *  Created on:    Aug-2009
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Cluster
       |
       '-----------------------------------------------------------------------
    */
    public class Cluster : ICloneable<Cluster>, IDeeplyCloneable<Cluster>, ISerializable
    {
        private Cluster m_parent
            = null;
        private ArrayList<Cluster> m_children
            = new ArrayList<Cluster>();
        private ArrayList<Pair<double, int>> m_items
            = new ArrayList<Pair<double, int>>();

        public Cluster()
        { 
        }

        public Cluster(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public Cluster Parent
        {
            get { return m_parent; }
            set { m_parent = null; }
        }

        public ArrayList<Cluster> Children
        {
            get { return m_children; }
        }

        public ArrayList<Pair<double, int>> Items
        {
            get { return m_items; }
        }

        public SparseVector<double>.ReadOnly ComputeCentroid<LblT>(IExampleCollection<LblT, SparseVector<double>.ReadOnly> dataset, CentroidType type)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Dictionary<int, double> tmp = new Dictionary<int, double>();
            double wgt_sum = 0;
            foreach (Pair<double, int> wgt_vec in m_items)
            {
                Utils.ThrowException((wgt_vec.Second < 0 || wgt_vec.Second >= dataset.Count) ? new IndexOutOfRangeException("Items (dataset index)") : null);
                foreach (IdxDat<double> item in dataset[wgt_vec.Second].Example)
                {
                    if (tmp.ContainsKey(item.Idx))
                    {
                        tmp[item.Idx] += wgt_vec.First * item.Dat;
                    }
                    else
                    {
                        tmp.Add(item.Idx, wgt_vec.First * item.Dat);
                    }
                }
                wgt_sum += wgt_vec.First;
            }
            Utils.ThrowException(wgt_sum == 0 ? new ArgumentValueException("Items (weights)") : null);
            SparseVector<double> centroid = new SparseVector<double>();
            switch (type)
            {
                case CentroidType.Sum:
                    foreach (KeyValuePair<int, double> item in tmp)
                    {
                        centroid.InnerIdx.Add(item.Key);
                        centroid.InnerDat.Add(item.Value);
                    }
                    break;
                case CentroidType.Avg:
                    foreach (KeyValuePair<int, double> item in tmp)
                    {
                        centroid.InnerIdx.Add(item.Key);
                        centroid.InnerDat.Add(item.Value / wgt_sum);
                    }
                    break;
                case CentroidType.NrmL2:
                    double vec_len = 0;
                    foreach (KeyValuePair<int, double> item in tmp)
                    {
                        vec_len += item.Value * item.Value;
                    }
                    Utils.ThrowException(vec_len == 0 ? new InvalidOperationException() : null);
                    vec_len = Math.Sqrt(vec_len);
                    foreach (KeyValuePair<int, double> item in tmp)
                    {
                        centroid.InnerIdx.Add(item.Key);
                        centroid.InnerDat.Add(item.Value / vec_len);
                    }
                    break;
            }
            centroid.Sort();
            return centroid;
        }        

        public override string ToString()
        {
            return ToString("C");
        }

        public string ToString(string format)
        {
            if (format == "C") // cluster 
            {
                return string.Format("( Parent:{0} Children:{1} Items:{2} )", m_parent == null, m_children.Count, m_items);
            }
            else if (format == "CC") // cluster compact 
            {
                ArrayList<int> tmp = new ArrayList<int>(m_items.Count);
                foreach (IPair<double, int> item in m_items)
                {
                    tmp.Add(item.Second);
                }
                return tmp.ToString();
            }
            else if (format == "CCW") // cluster compact with weights
            {
                return m_items.ToString();
            }
            else if (format == "T") // tree 
            {
                StringBuilder str_builder = new StringBuilder();
                ToString("", str_builder, /*compact=*/false);
                return str_builder.ToString().TrimEnd('\n', '\r');
            }
            else if (format == "TC") // tree compact
            {
                StringBuilder str_builder = new StringBuilder();
                ToString("", str_builder, /*compact=*/true);
                return str_builder.ToString().TrimEnd('\n', '\r');
            }
            else
            {
                throw new ArgumentNotSupportedException("format");
            }
        }

        private void ToString(string tab, StringBuilder str_builder, bool compact)
        {
            str_builder.Append(tab);
            str_builder.AppendLine(ToString(compact ? "CC" : "CCW"));
            foreach (Cluster child in m_children)
            {
                child.ToString(tab + "\t", str_builder, compact);
            }
        }

        // *** Cloneable<ClusteringResult> interface implementation ***

        public Cluster Clone()
        {
            // *** quick and dirty
            BinarySerializer mem_ser = new BinarySerializer();
            Save(mem_ser);
            mem_ser.Stream.Position = 0;
            return new Cluster(mem_ser);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable<ClusteringResult> interface implementation ***

        public Cluster DeepClone()
        {
            return Clone();
        }

        object IDeeplyCloneable.DeepClone()
        {
            return Clone();
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteObject(m_parent);            
            m_children.Save(writer); // *** this will not work properly if two or more parents share a child cluster
            m_items.Save(writer);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_parent = reader.ReadObject<Cluster>();
            m_children = new ArrayList<Cluster>(reader);
            m_items = new ArrayList<Pair<double, int>>(reader);
        }        
    }
}

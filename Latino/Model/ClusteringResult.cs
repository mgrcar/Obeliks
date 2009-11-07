/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Cluster.cs
 *  Version:       1.0
 *  Desc:		   Custering result (output of clustering algorithms)
 *  Author:        Miha Grcar 
 *  Created on:    Aug-2009
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;
using System.Text;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class ClusteringResult
       |
       '-----------------------------------------------------------------------
    */
    public class ClusteringResult : ICloneable<ClusteringResult>, IDeeplyCloneable<ClusteringResult>, ISerializable
    {
        private ArrayList<Cluster> m_roots
            = new ArrayList<Cluster>();

        public ClusteringResult()
        { 
        }

        public ClusteringResult(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public ArrayList<Cluster> Roots
        {
            get { return m_roots; }
        }

        public IDataset<Cluster, ExT> GetClusteringDataset<LblT, ExT>(IDataset<LblT, ExT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Dataset<Cluster, ExT> clustering_dataset = new Dataset<Cluster, ExT>();
            foreach (Cluster cluster in m_roots)
            {
                foreach (Pair<double, int> ex_info in cluster.Items)
                {
                    Utils.ThrowException(ex_info.Second < 0 || ex_info.Second >= dataset.Count ? new ArgumentValueException("Roots (cluster items)") : null);
                    clustering_dataset.Add(cluster, dataset[ex_info.Second].Example);
                }
            }
            return clustering_dataset;
        }

        public override string ToString()
        {
            return ToString("T");
        }

        public string ToString(string format)
        {
            StringBuilder str_builder = new StringBuilder();
            foreach (Cluster root in m_roots)
            {
                str_builder.AppendLine(root.ToString(format)); // throws ArgumentNotSupportedException
            }
            return str_builder.ToString().TrimEnd('\n', '\r');
        }

        // *** ICloneable<ClusteringResult> interface implementation ***

        public ClusteringResult Clone()
        {
            ClusteringResult clone = new ClusteringResult();
            clone.m_roots = m_roots.DeepClone();
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable<ClusteringResult> interface implementation ***

        public ClusteringResult DeepClone()
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
            m_roots.Save(writer); // throws serialization-related exceptions
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_roots = new ArrayList<Cluster>(reader); // throws serialization-related exceptions
        }
    }
}

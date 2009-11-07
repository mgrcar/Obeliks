/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          BatchUpdateCentroidClassifier.cs
 *  Version:       1.0
 *  Desc:		   Batch-update centroid classifier 
 *  Author:        Miha Grcar
 *  Created on:    May-2009
 *  Last modified: May-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class BatchUpdateCentroidClassifier<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class BatchUpdateCentroidClassifier<LblT> : IModel<LblT, SparseVector<double>.ReadOnly>
    {
        private Dictionary<LblT, CentroidData> m_centroids
            = null;
        private IEqualityComparer<LblT> m_lbl_cmp
            = null;
        private int m_iterations
            = 20;
        private double m_damping
            = 0.8;
        private bool m_positive_values_only
            = false;

        public BatchUpdateCentroidClassifier()
        {
        }

        public BatchUpdateCentroidClassifier(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public IEqualityComparer<LblT> LabelEqualityComparer
        {
            get { return m_lbl_cmp; }
            set { m_lbl_cmp = value; }
        }

        public int Iterations
        {
            get { return m_iterations; }
            set
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("Iterations") : null);
                m_iterations = value;
            }
        }

        public double Damping
        {
            get { return m_damping; }
            set 
            {
                Utils.ThrowException((value <= 0.0 || value > 1.0) ? new ArgumentOutOfRangeException("Damping") : null);
                m_damping = value;
            }
        }

        public bool PositiveValuesOnly
        {
            get { return m_positive_values_only; }
            set { m_positive_values_only = value; }
        }

        // *** IModel<LblT, SparseVector<double>.ReadOnly> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(SparseVector<double>.ReadOnly); }
        }

        public bool IsTrained
        {
            get { return m_centroids != null; }
        }

        public void Train(IExampleCollection<LblT, SparseVector<double>.ReadOnly> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count == 0 ? new ArgumentValueException("dataset") : null);
            m_centroids = new Dictionary<LblT, CentroidData>();
            foreach (LabeledExample<LblT, SparseVector<double>.ReadOnly> labeled_example in dataset)
            {
                if (!m_centroids.ContainsKey(labeled_example.Label))
                {
                    CentroidData centroid_data = new CentroidData();
                    centroid_data.AddToSum(labeled_example.Example);
                    m_centroids.Add(labeled_example.Label, centroid_data);
                }
                else
                {
                    CentroidData centroid_data = m_centroids[labeled_example.Label];
                    centroid_data.AddToSum(labeled_example.Example);
                }               
            }
            foreach (CentroidData vec_data in m_centroids.Values)
            {
                vec_data.UpdateCentroidLen();
            }
            double learn_rate = 1;
            for (int iter = 1; iter <= m_iterations; iter++)
            {
                Utils.VerboseLine("Iteration {0} / {1} ...", iter, m_iterations);
                // classify training documents
                int i = 0;
                int num_miscfy = 0;
                foreach (LabeledExample<LblT, SparseVector<double>.ReadOnly> labeled_example in dataset)
                {
                    Utils.Verbose("\rExample {0} / {1} ...", ++i, dataset.Count);
                    double max_sim = double.MinValue;
                    CentroidData assigned_centroid = null;
                    CentroidData actual_centroid = null;
                    SparseVector<double>.ReadOnly vec = labeled_example.Example;
                    foreach (KeyValuePair<LblT, CentroidData> labeled_centroid in m_centroids)
                    {                        
                        double sim = labeled_centroid.Value.GetSimilarity(vec);
                        if (sim > max_sim) { max_sim = sim; assigned_centroid = labeled_centroid.Value; }
                        if (labeled_centroid.Key.Equals(labeled_example.Label)) { actual_centroid = labeled_centroid.Value; }
                    }                        
                    if (assigned_centroid != actual_centroid)
                    {                        
                        assigned_centroid.AddToDiff(-learn_rate, vec);
                        actual_centroid.AddToDiff(learn_rate, vec);
                        num_miscfy++;
                    }                        
                }
                Utils.VerboseLine("");
                Utils.VerboseLine("Training set error rate: {0:0.00}%", (double)num_miscfy / (double)dataset.Count * 100.0);
                // update centroids
                i = 0;
                foreach (CentroidData centroid_data in m_centroids.Values)
                {
                    Utils.Verbose("\rCentroid {0} / {1} ...", ++i, m_centroids.Count);
                    centroid_data.UpdateCentroid(m_positive_values_only);
                    centroid_data.UpdateCentroidLen();
                }
                Utils.VerboseLine("");
                learn_rate *= m_damping;
            }
        }

        void IModel<LblT>.Train(IExampleCollection<LblT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is IExampleCollection<LblT, SparseVector<double>.ReadOnly>) ? new ArgumentTypeException("dataset") : null);
            Train((IExampleCollection<LblT, SparseVector<double>.ReadOnly>)dataset); // throws ArgumentValueException
        }

        public ClassifierResult<LblT> Classify(SparseVector<double>.ReadOnly example)
        {
            Utils.ThrowException(m_centroids == null ? new InvalidOperationException() : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            ClassifierResult<LblT> result = new ClassifierResult<LblT>();
            foreach (KeyValuePair<LblT, CentroidData> labeled_centroid in m_centroids)
            {
                double sim = labeled_centroid.Value.GetSimilarity(example);
                result.Items.Add(new KeyDat<double, LblT>(sim, labeled_centroid.Key));
            }
            result.Items.Sort(new DescSort<KeyDat<double, LblT>>());
            return result;
        }

        ClassifierResult<LblT> IModel<LblT>.Classify(object example)
        {
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            Utils.ThrowException(!(example is SparseVector<double>.ReadOnly) ? new ArgumentTypeException("example") : null);
            return Classify((SparseVector<double>.ReadOnly)example); // throws InvalidOperationException
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteBool(m_centroids != null);
            if (m_centroids != null) 
            { 
                Utils.SaveDictionary(m_centroids, writer); 
            }
            writer.WriteInt(m_iterations);
            writer.WriteDouble(m_damping);
            writer.WriteBool(m_positive_values_only);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions            
            m_centroids = reader.ReadBool() ? Utils.LoadDictionary<LblT, CentroidData>(reader) : null;
            m_iterations = reader.ReadInt();
            m_damping = reader.ReadDouble();
            m_positive_values_only = reader.ReadBool();
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class CentroidData
           |
           '-----------------------------------------------------------------------
        */
        // *** would it make sense to use Centroid<LblT> within CentroidData?
        private class CentroidData : ISerializable
        {
            public Dictionary<int, double> CentroidSum
                = new Dictionary<int, double>();
            public Dictionary<int, double> CentroidDiff
                = new Dictionary<int, double>();
            public double CentroidLen
                = -1;

            public CentroidData()
            { 
            }

            public CentroidData(BinarySerializer reader)
            {
                Load(reader); 
            }

            public void AddToSum(SparseVector<double>.ReadOnly vec)
            {
                foreach (IdxDat<double> item in vec)
                {
                    if (CentroidSum.ContainsKey(item.Idx))
                    {
                        CentroidSum[item.Idx] += item.Dat;
                    }
                    else
                    {
                        CentroidSum.Add(item.Idx, item.Dat);
                    }
                }
            }

            public void AddToDiff(double mult, SparseVector<double>.ReadOnly vec)
            {                
                foreach (IdxDat<double> item in vec)
                {
                    if (CentroidDiff.ContainsKey(item.Idx))
                    {
                        CentroidDiff[item.Idx] += item.Dat * mult;
                    }
                    else
                    {
                        CentroidDiff.Add(item.Idx, item.Dat * mult);
                    }
                }
            }

            public void UpdateCentroid(bool positive_values_only)
            {
                foreach (KeyValuePair<int, double> item in CentroidDiff)
                {
                    if (CentroidSum.ContainsKey(item.Key))
                    {
                        CentroidSum[item.Key] += item.Value;
                    }
                    else 
                    {
                        CentroidSum.Add(item.Key, item.Value);
                    }
                }
                CentroidDiff.Clear();
                if (positive_values_only)
                {
                    Dictionary<int, double> tmp = new Dictionary<int, double>();
                    foreach (KeyValuePair<int, double> item in CentroidSum)
                    {
                        if (item.Value > 0)
                        {
                            tmp.Add(item.Key, item.Value);
                        }
                    }
                    CentroidSum = tmp;
                }
            }

            public void UpdateCentroidLen()
            {
                CentroidLen = 0;
                foreach (double val in CentroidSum.Values)
                {
                    CentroidLen += val * val;
                }
                //Utils.VerboseLine(CentroidLen); // *** CentroidLen overflow?
                CentroidLen = Math.Sqrt(CentroidLen);
            }

            public double GetSimilarity(SparseVector<double>.ReadOnly vec)
            {
                double result = 0;
                foreach (IdxDat<double> item in vec)
                {
                    if (CentroidSum.ContainsKey(item.Idx))
                    {
                        result += item.Dat * CentroidSum[item.Idx]; 
                    }
                }
                return result / CentroidLen;
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                Utils.SaveDictionary(CentroidSum, writer);
                writer.WriteDouble(CentroidLen);
            }

            public void Load(BinarySerializer reader)
            {
                CentroidSum = Utils.LoadDictionary<int, double>(reader);
                CentroidLen = reader.ReadDouble();
            }
        }
    }
}

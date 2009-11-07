/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          CentroidClassifier.cs
 *  Version:       1.0
 *  Desc:		   Centroid classifier
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: May-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class CentroidClassifier<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class CentroidClassifier<LblT> : IModel<LblT, SparseVector<double>.ReadOnly>
    {
        private ArrayList<Pair<LblT, SparseVector<double>.ReadOnly>> m_centroids
            = null;
        private IEqualityComparer<LblT> m_lbl_cmp
            = null;
        private ISimilarity<SparseVector<double>.ReadOnly> m_similarity
            = new CosineSimilarity();
        private bool m_normalize
            = false;

        public CentroidClassifier()
        {
        }

        public CentroidClassifier(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public bool NormalizeCentroids
        {
            get { return m_normalize; }
            set { m_normalize = value; }
        }

        public IEqualityComparer<LblT> LabelEqualityComparer
        {
            get { return m_lbl_cmp; }
            set { m_lbl_cmp = value; }
        }

        public ISimilarity<SparseVector<double>.ReadOnly> Similarity
        {
            get { return m_similarity; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Similarity") : null);
                m_similarity = value;
            }
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
            m_centroids = new ArrayList<Pair<LblT, SparseVector<double>.ReadOnly>>();
            Dictionary<LblT, ArrayList<SparseVector<double>.ReadOnly>> tmp = new Dictionary<LblT, ArrayList<SparseVector<double>.ReadOnly>>(m_lbl_cmp);
            foreach (LabeledExample<LblT, SparseVector<double>.ReadOnly> labeled_example in dataset)
            {
                if (!tmp.ContainsKey(labeled_example.Label))
                {
                    tmp.Add(labeled_example.Label, new ArrayList<SparseVector<double>.ReadOnly>(new SparseVector<double>.ReadOnly[] { labeled_example.Example }));
                }
                else
                {
                    tmp[labeled_example.Label].Add(labeled_example.Example);
                }
            }
            foreach (KeyValuePair<LblT, ArrayList<SparseVector<double>.ReadOnly>> centroid_data in tmp)
            {
                SparseVector<double> centroid = ModelUtils.ComputeCentroid(centroid_data.Value, m_normalize ? CentroidType.NrmL2 : CentroidType.Avg);
                m_centroids.Add(new Pair<LblT, SparseVector<double>.ReadOnly>(centroid_data.Key, centroid));
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
            foreach (Pair<LblT, SparseVector<double>.ReadOnly> labeled_centroid in m_centroids)
            {
                double sim = m_similarity.GetSimilarity(labeled_centroid.Second, example);
                result.Items.Add(new KeyDat<double, LblT>(sim, labeled_centroid.First));
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
            writer.WriteObject(m_centroids);
            writer.WriteObject<ISimilarity<SparseVector<double>.ReadOnly>>(m_similarity);
            writer.WriteBool(m_normalize);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_centroids = reader.ReadObject<ArrayList<Pair<LblT, SparseVector<double>.ReadOnly>>>();
            m_similarity = reader.ReadObject<ISimilarity<SparseVector<double>.ReadOnly>>();
            m_normalize = reader.ReadBool();
        }
    }
}

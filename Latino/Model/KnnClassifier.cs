/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          KnnClassifier.cs
 *  Version:       1.0
 *  Desc:		   K-nearest neighbors classifier 
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class KnnClassifier<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public class KnnClassifier<LblT, ExT> : IModel<LblT, ExT>
    {
        private ArrayList<LabeledExample<LblT, ExT>> m_examples
            = null;
        private IEqualityComparer<LblT> m_lbl_cmp
            = null;
        private ISimilarity<ExT> m_similarity
            = null;
        private int m_k
            = 10;
        private bool m_soft_voting
            = true;

        public KnnClassifier(ISimilarity<ExT> similarity)
        {
            Similarity = similarity; // throws ArgumentNullException
        }

        public KnnClassifier(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public IEqualityComparer<LblT> LabelEqualityComparer
        {
            get { return m_lbl_cmp; }
            set { m_lbl_cmp = value; }
        }

        public ISimilarity<ExT> Similarity
        {
            get { return m_similarity; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Similarity") : null);
                m_similarity = value;
            }
        }

        public int K
        {
            get { return m_k; }
            set
            {
                Utils.ThrowException(value < 1 ? new ArgumentOutOfRangeException("K") : null);
                m_k = value;
            }
        }

        public bool SoftVoting
        {
            get { return m_soft_voting; }
            set { m_soft_voting = value; }
        }

        // *** IModel<LblT, ExT> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(ExT); }
        }

        public bool IsTrained
        {
            get { return m_examples != null; }
        }

        public void Train(IExampleCollection<LblT, ExT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count == 0 ? new ArgumentValueException("dataset") : null);
            m_examples = new ArrayList<LabeledExample<LblT, ExT>>(dataset);
        }

        void IModel<LblT>.Train(IExampleCollection<LblT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is IExampleCollection<LblT, ExT>) ? new ArgumentTypeException("dataset") : null);
            Train((IExampleCollection<LblT, ExT>)dataset); // throws ArgumentValueException
        }

        public ClassifierResult<LblT> Classify(ExT example)
        {
            Utils.ThrowException((m_examples == null || m_similarity == null) ? new InvalidOperationException() : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            ArrayList<KeyDat<double, LabeledExample<LblT, ExT>>> tmp = new ArrayList<KeyDat<double, LabeledExample<LblT, ExT>>>(m_examples.Count);
            foreach (LabeledExample<LblT, ExT> labeled_example in m_examples)
            {
                double sim = m_similarity.GetSimilarity(example, labeled_example.Example);
                tmp.Add(new KeyDat<double, LabeledExample<LblT, ExT>>(sim, labeled_example));
            }
            tmp.Sort(new DescSort<KeyDat<double, LabeledExample<LblT, ExT>>>());
            Dictionary<LblT, double> voting = new Dictionary<LblT, double>(m_lbl_cmp);
            int n = Math.Min(m_k, tmp.Count);
            double value;
            if (m_soft_voting) // "soft" voting
            {
                for (int i = 0; i < n; i++)
                {
                    KeyDat<double, LabeledExample<LblT, ExT>> item = tmp[i];
                    if (!voting.TryGetValue(item.Dat.Label, out value))
                    {
                        voting.Add(item.Dat.Label, item.Key);
                    }
                    else
                    {
                        voting[item.Dat.Label] = value + item.Key;
                    }
                }
            }
            else // normal voting
            {
                for (int i = 0; i < n; i++)
                {
                    KeyDat<double, LabeledExample<LblT, ExT>> item = tmp[i];
                    if (!voting.TryGetValue(item.Dat.Label, out value))
                    {
                        voting.Add(item.Dat.Label, 1);
                    }
                    else
                    {
                        voting[item.Dat.Label] = value + 1.0;
                    }
                }
            }
            ClassifierResult<LblT> classifier_result = new ClassifierResult<LblT>();
            foreach (KeyValuePair<LblT, double> item in voting)
            {
                classifier_result.Items.Add(new KeyDat<double, LblT>(item.Value, item.Key));
            }
            classifier_result.Items.Sort(new DescSort<KeyDat<double, LblT>>());
            return classifier_result;
        }

        ClassifierResult<LblT> IModel<LblT>.Classify(object example)
        {
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            Utils.ThrowException(!(example is ExT) ? new ArgumentTypeException("example") : null);
            return Classify((ExT)example); // throws InvalidOperationException
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            m_examples.Save(writer);
            writer.WriteObject<ISimilarity<ExT>>(m_similarity);
            writer.WriteInt(m_k);
            writer.WriteBool(m_soft_voting);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_examples = new ArrayList<LabeledExample<LblT, ExT>>(reader);
            m_similarity = reader.ReadObject<ISimilarity<ExT>>();
            m_k = reader.ReadInt();
            m_soft_voting = reader.ReadBool();
        }
    }
}

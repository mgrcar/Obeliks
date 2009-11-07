/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Dataset.cs
 *  Version:       1.0
 *  Desc:		   Dataset for training ML models
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Dataset<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public class Dataset<LblT, ExT> : IDataset<LblT, ExT>
    {
        private ArrayList<LabeledExample<LblT, ExT>> m_items
            = new ArrayList<LabeledExample<LblT, ExT>>();

        public Dataset()
        {
        }

        public Dataset(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public void Add(LblT label, ExT example)
        {
            Utils.ThrowException(label == null ? new ArgumentNullException("label") : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);            
            m_items.Add(new LabeledExample<LblT, ExT>(label, example));
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index); // throws ArgumentOutOfRangeException
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public void Shuffle()
        {
            m_items.Shuffle();
        }

        public void Shuffle(Random rnd)
        {
            m_items.Shuffle(rnd); // throws ArgumentNullException
        }

        public void SplitForCrossValidation(int num_folds, int fold, ref Dataset<LblT, ExT> train_set, ref Dataset<LblT, ExT> test_set)
        {
            Utils.ThrowException(m_items.Count < 2 ? new InvalidOperationException() : null);
            Utils.ThrowException((num_folds < 2 || num_folds > m_items.Count) ? new ArgumentOutOfRangeException("num_folds") : null);
            Utils.ThrowException((fold < 1 || fold > num_folds) ? new ArgumentOutOfRangeException("fold") : null);
            train_set = new Dataset<LblT, ExT>();
            test_set = new Dataset<LblT, ExT>();
            double step = (double)m_items.Count / (double)num_folds;
            double d = 0;
            for (int i = 0; i < num_folds; i++, d += step)
            {
                int end_j = (int)Math.Round(d + step);
                if (i == fold - 1)
                {
                    for (int j = (int)Math.Round(d); j < end_j; j++)
                    {
                        test_set.Add(m_items[j].Label, m_items[j].Example);
                    }
                }
                else
                {
                    for (int j = (int)Math.Round(d); j < end_j; j++)
                    {
                        train_set.Add(m_items[j].Label, m_items[j].Example);
                    }
                }
            }
        }

        // *** IDataset<LblT, ExT> interface implementation ***

        public Type ExampleType
        {
            get { return typeof(ExT); }
        }

        public int Count
        {
            get { return m_items.Count; }
        }

        public LabeledExample<LblT, ExT> this[int index]
        {
            get
            {
                Utils.ThrowException((index < 0 || index >= m_items.Count) ? new ArgumentOutOfRangeException("index") : null);
                return m_items[index];
            }
        }

        object IEnumerableList.this[int index]
        {
            get { return this[index]; } // throws ArgumentOutOfRangeException
        }

        public IEnumerator<LabeledExample<LblT, ExT>> GetEnumerator()
        {
            return new ListEnum<LabeledExample<LblT, ExT>>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ListEnum(this);
        }

        public IDataset<LblT, NewExT> ConvertDataset<NewExT>(bool move)
        {
            return (IDataset<LblT, NewExT>)ConvertDataset(typeof(NewExT), move); // throws ArgumentNotSupportedException
        }

        public IDataset<LblT> ConvertDataset(Type new_ex_type, bool move)
        {
            Utils.ThrowException(new_ex_type == null ? new ArgumentNullException("new_ex_type") : null);
            if (new_ex_type == typeof(SparseVector<double>))
            {
                Dataset<LblT, SparseVector<double>> new_dataset = new Dataset<LblT, SparseVector<double>>();
                for (int i = 0; i < m_items.Count; i++)
                {
                    LabeledExample<LblT, ExT> example = m_items[i];
                    new_dataset.Add(example.Label, ModelUtils.ConvertExample<SparseVector<double>>(example.Example));
                    if (move) { m_items[i] = new LabeledExample<LblT, ExT>(); }
                }
                if (move) { m_items.Clear(); }
                return new_dataset;
            }
            else if (new_ex_type == typeof(SparseVector<double>.ReadOnly))
            {
                Dataset<LblT, SparseVector<double>.ReadOnly> new_dataset = new Dataset<LblT, SparseVector<double>.ReadOnly>();
                for (int i = 0; i < m_items.Count; i++)
                {
                    LabeledExample<LblT, ExT> example = m_items[i];
                    new_dataset.Add(example.Label, ModelUtils.ConvertExample<SparseVector<double>.ReadOnly>(example.Example));
                    if (move) { m_items[i] = new LabeledExample<LblT, ExT>(); }
                }
                if (move) { m_items.Clear(); }
                return new_dataset;
            }
            else if (new_ex_type == typeof(BinaryVector<int>))
            {
                Dataset<LblT, BinaryVector<int>> new_dataset = new Dataset<LblT, BinaryVector<int>>();
                for (int i = 0; i < m_items.Count; i++)
                {
                    LabeledExample<LblT, ExT> example = m_items[i];
                    new_dataset.Add(example.Label, ModelUtils.ConvertExample<BinaryVector<int>>(example.Example));
                    if (move) { m_items[i] = new LabeledExample<LblT, ExT>(); }
                }
                if (move) { m_items.Clear(); }
                return new_dataset;
            }
            else if (new_ex_type == typeof(BinaryVector<int>.ReadOnly))
            {
                Dataset<LblT, BinaryVector<int>.ReadOnly> new_dataset = new Dataset<LblT, BinaryVector<int>.ReadOnly>();
                for (int i = 0; i < m_items.Count; i++)
                {
                    LabeledExample<LblT, ExT> example = m_items[i];
                    new_dataset.Add(example.Label, ModelUtils.ConvertExample<BinaryVector<int>.ReadOnly>(example.Example));
                    if (move) { m_items[i] = new LabeledExample<LblT, ExT>(); }
                }
                if (move) { m_items.Clear(); }
                return new_dataset;
            }
            //else if (new_ex_type == typeof(SvmFeatureVector))
            //{
            //    Dataset<LblT, SvmFeatureVector> new_dataset = new Dataset<LblT, SvmFeatureVector>();
            //    for (int i = 0; i < m_items.Count; i++) 
            //    {
            //        LabeledExample<LblT, ExT> example = m_items[i];
            //        new_dataset.Add(example.Label, ModelUtils.ConvertVector<SvmFeatureVector>(example.Example));
            //        if (move) { m_items[i] = new LabeledExample<LblT, ExT>(); }
            //    }
            //    if (move) { m_items.Clear(); }
            //    return new_dataset;
            //}
            else
            {
                throw new ArgumentNotSupportedException("new_ex_type");
            }
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            m_items.Save(writer); // throws serialization-related exceptions
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_items.Load(reader); // throws serialization-related exceptions
        }
    }
}
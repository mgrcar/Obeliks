/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          MaxEntClassifier.cs
 *  Version:       1.0
 *  Desc:		   Maximum entropy classifier (LATINO wrapper)
 *  Author:        Miha Grcar
 *  Created on:    Oct-2009
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
       |  Class MaximumEntropyClassifier<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class MaximumEntropyClassifier<LblT> : IModel<LblT, BinaryVector<int>.ReadOnly>
    {
        private bool m_move_data
            = false;
        private int m_num_iter
            = 100;
        private int m_cut_off
            = 0;
        private int m_num_threads
            = 1;
        private SparseMatrix<double> m_lambda
            = null;
        private LblT[] m_idx_to_lbl
            = null;

        public MaximumEntropyClassifier()
        {
        }

        public MaximumEntropyClassifier(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public bool MoveData
        {
            get { return m_move_data; }
            set { m_move_data = value; }
        }

        public int NumIter
        {
            get { return m_num_iter; }
            set
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("NumIter") : null);
                m_num_iter = value;
            }
        }

        public int CutOff
        {
            get { return m_cut_off; }
            set
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("CutOff") : null);
                m_cut_off = value;
            }
        }

        public int NumThreads
        {
            get { return m_num_threads; }
            set 
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("NumThreads") : null);
                m_num_threads = value;             
            }
        }

        // *** IModel<LblT, BinaryVector<int>.ReadOnly> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(BinaryVector<int>.ReadOnly); }
        }

        public bool IsTrained
        {
            get { return m_lambda != null; }
        }

        public void Train(IExampleCollection<LblT, BinaryVector<int>.ReadOnly> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count == 0 ? new ArgumentValueException("dataset") : null);
            m_lambda = null; // allow GC to collect this
            m_lambda = MaxEnt.Gis(dataset, m_cut_off, m_num_iter, m_move_data, /*mtx_file_name=*/null, ref m_idx_to_lbl, m_num_threads);
        }

        void IModel<LblT>.Train(IExampleCollection<LblT> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is IExampleCollection<LblT, BinaryVector<int>.ReadOnly>) ? new ArgumentTypeException("dataset") : null);
            Train((IExampleCollection<LblT, BinaryVector<int>.ReadOnly>)dataset); // throws ArgumentValueException
        }

        public ClassifierResult<LblT> Classify(BinaryVector<int>.ReadOnly example)
        {
            Utils.ThrowException(m_lambda == null ? new InvalidOperationException() : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            return MaxEnt.Classify(example, m_lambda, m_idx_to_lbl);
        }

        ClassifierResult<LblT> IModel<LblT>.Classify(object example)
        {
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            Utils.ThrowException(!(example is BinaryVector<int>.ReadOnly) ? new ArgumentTypeException("example") : null);
            return Classify((BinaryVector<int>.ReadOnly)example); // throws InvalidOperationException
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteBool(m_move_data);
            writer.WriteInt(m_num_iter);
            writer.WriteInt(m_cut_off);
            writer.WriteInt(m_num_threads);
            m_lambda.Save(writer);
            new ArrayList<LblT>(m_idx_to_lbl).Save(writer);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions            
            m_move_data = reader.ReadBool();
            m_num_iter = reader.ReadInt();
            m_cut_off = reader.ReadInt();
            m_num_threads = reader.ReadInt();
            m_lambda = new SparseMatrix<double>(reader);
            m_idx_to_lbl = new ArrayList<LblT>(reader).ToArray();
        }
    }
}

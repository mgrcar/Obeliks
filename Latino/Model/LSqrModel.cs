/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          LSqrModel.cs
 *  Version:       1.0
 *  Desc:		   Least-squares linear regression model
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: Jul-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class LSqrModel
       |
       '-----------------------------------------------------------------------
    */
    public class LSqrModel : IModel<double, SparseVector<double>.ReadOnly>
    {
        private ArrayList<double> m_sol
            = null;
        private int m_num_iter
            = -1;

        public LSqrModel()
        {
        }

        public LSqrModel(int num_iter)
        {
            m_num_iter = num_iter;
        }

        public LSqrModel(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public ArrayList<double>.ReadOnly Solution
        {
            get
            {
                Utils.ThrowException(m_sol == null ? new InvalidOperationException() : null);
                return m_sol;
            }
        }

        // *** IModel<double, SparseVector<double>.ReadOnly> interface implementation ***

        public Type RequiredExampleType
        {
            get { return typeof(SparseVector<double>.ReadOnly); }
        }

        public bool IsTrained
        {
            get { return m_sol != null; }
        }

        public void Train(IExampleCollection<double, SparseVector<double>.ReadOnly> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(dataset.Count == 0 ? new ArgumentValueException("dataset") : null);
            LSqrSparseMatrix mat = new LSqrSparseMatrix(dataset.Count);
            double[] rhs = new double[dataset.Count];
            int sol_size = -1;
            int i = 0;
            foreach (LabeledExample<double, SparseVector<double>.ReadOnly> labeled_example in dataset)
            {
                if (labeled_example.Example.LastNonEmptyIndex + 1 > sol_size) 
                { 
                    sol_size = labeled_example.Example.LastNonEmptyIndex + 1; 
                }
                foreach (IdxDat<double> item in labeled_example.Example)
                {
                    mat.InsertValue(i, item.Idx, item.Dat);
                }
                rhs[i++] = labeled_example.Label;
            }
            LSqrSparseMatrix mat_t = new LSqrSparseMatrix(sol_size);
            i = 0;
            foreach (LabeledExample<double, SparseVector<double>.ReadOnly> labeled_example in dataset)
            {
                foreach (IdxDat<double> item in labeled_example.Example)
                {
                    mat_t.InsertValue(item.Idx, i, item.Dat);
                }
                i++;
            }
            int num_iter = m_num_iter < 0 ? sol_size + dataset.Count + 50 : m_num_iter;
            m_sol = new ArrayList<double>(LSqrDll.DoLSqr(sol_size, mat, mat_t, rhs, num_iter));
            mat.Dispose();
            mat_t.Dispose();
        }

        void IModel<double>.Train(IExampleCollection<double> dataset)
        {
            Utils.ThrowException(dataset == null ? new ArgumentNullException("dataset") : null);
            Utils.ThrowException(!(dataset is IExampleCollection<double, SparseVector<double>.ReadOnly>) ? new ArgumentTypeException("dataset") : null);
            Train((IExampleCollection<double, SparseVector<double>.ReadOnly>)dataset); // throws ArgumentValueException
        }

        public ClassifierResult<double> Classify(SparseVector<double>.ReadOnly example)
        {
            Utils.ThrowException(m_sol == null ? new InvalidOperationException() : null);
            Utils.ThrowException(example == null ? new ArgumentNullException("example") : null);
            double result = 0;
            foreach (IdxDat<double> item in example)
            {
                result += m_sol[item.Idx] * item.Dat;
            }
            return new ClassifierResult<double>(new KeyDat<double, double>[] { new KeyDat<double, double>(result, result) });
        }

        ClassifierResult<double> IModel<double>.Classify(object example)
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
            writer.WriteInt(m_num_iter);
            writer.WriteObject(m_sol);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_num_iter = reader.ReadInt();
            m_sol = reader.ReadObject<ArrayList<double>>();
        }
    }
}
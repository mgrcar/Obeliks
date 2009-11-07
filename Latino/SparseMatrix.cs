/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          SparseMatrix.cs
 *  Version:       2.0
 *  Desc:		   Sparse matrix data structure 
 *  Author:        Miha Grcar
 *  Created on:    Apr-2007
 *  Last modified: Mar-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class SparseMatrix<T>
       |
       '-----------------------------------------------------------------------
    */
    public class SparseMatrix<T> : IEnumerable<IdxDat<SparseVector<T>>>, ICloneable<SparseMatrix<T>>, IDeeplyCloneable<SparseMatrix<T>>,
        IContentEquatable<SparseMatrix<T>>, ISerializable
    {
        private ArrayList<SparseVector<T>> m_rows
            = new ArrayList<SparseVector<T>>();

        public SparseMatrix()
        {
        }

        public SparseMatrix(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        private void SetRowListSize(int size)
        {
            //System.Diagnostics.Debug.Assert(size > m_rows.Count);
            int add_rows = size - m_rows.Count;
            for (int i = 0; i < add_rows; i++)
            {
                m_rows.Add(new SparseVector<T>());
            }
        }

        public void TrimRows()
        {
            int row_idx = m_rows.Count - 1;
            for (; row_idx >= 0; row_idx--)
            {
                if (m_rows[row_idx].Count > 0) { break; }
            }
            m_rows.RemoveRange(row_idx + 1, m_rows.Count - (row_idx + 1));
        }

        public bool ContainsRowAt(int row_idx)
        {
            Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
            return row_idx < m_rows.Count && m_rows[row_idx].Count > 0;
        }

        public bool ContainsColAt(int col_idx)
        {
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            foreach (SparseVector<T> row in m_rows)
            {
                if (row.ContainsAt(col_idx)) { return true; }
            }
            return false;
        }

        public bool ContainsAt(int row_idx, int col_idx)
        {
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            if (!ContainsRowAt(row_idx)) { return false; } // throws ArgumentOutOfRangeException
            return m_rows[row_idx].ContainsAt(col_idx);
        }

        public int GetFirstNonEmptyRowIdx()
        {
            for (int row_idx = 0; row_idx < m_rows.Count; row_idx++)
            {
                if (m_rows[row_idx].Count > 0) { return row_idx; }
            }
            return -1;
        }

        public int GetLastNonEmptyRowIdx()
        {
            for (int row_idx = m_rows.Count - 1; row_idx >= 0; row_idx--)
            {
                if (m_rows[row_idx].Count > 0) { return row_idx; }
            }
            return -1;
        }

        public int GetFirstNonEmptyColIdx()
        {
            int min_idx = int.MaxValue;
            foreach (SparseVector<T> row in m_rows)
            {
                if (row.FirstNonEmptyIndex != -1 && row.FirstNonEmptyIndex < min_idx)
                {
                    min_idx = row.FirstNonEmptyIndex;
                }
            }
            return min_idx == int.MaxValue ? -1 : min_idx;
        }

        public int GetLastNonEmptyColIdx()
        {
            int max_idx = -1;
            foreach (SparseVector<T> row in m_rows)
            {
                if (row.LastNonEmptyIndex > max_idx)
                {
                    max_idx = row.LastNonEmptyIndex;
                }
            }
            return max_idx;
        }

        public override string ToString()
        {
            return m_rows.ToString();
        }

        public string ToString(string format)
        {
            if (format == "C") // compact format
            {
                return m_rows.ToString();
            }
            else if (format == "E") // extended format
            {
                StringBuilder str_bld = new StringBuilder();
                int row_count = m_rows.Count; // this will show empty rows at the end if the matrix is not trimmed
                int col_count = GetLastNonEmptyColIdx() + 1;
                for (int row_idx = 0; row_idx < row_count; row_idx++)
                {
                    for (int col_idx = 0; col_idx < col_count; col_idx++)
                    {
                        if (ContainsAt(row_idx, col_idx))
                        {
                            str_bld.Append(this[row_idx, col_idx]);
                        }
                        else
                        {
                            str_bld.Append("-");
                        }
                        if (col_idx != col_count - 1) { str_bld.Append("\t"); }
                    }
                    if (row_idx != row_count - 1) { str_bld.Append("\n"); }
                }
                return str_bld.ToString();
            }
            else
            {
                throw new ArgumentNotSupportedException("format");
            }
        }

        public bool IndexOf(T val, ref int row_idx, ref int col_idx)
        {
            Utils.ThrowException(val == null ? new ArgumentNullException("val") : null);
            for (int i = 0; i < m_rows.Count; i++)
            {
                SparseVector<T> row = m_rows[i];
                foreach (IdxDat<T> item in row)
                {
                    if (item.Dat.Equals(val))
                    {
                        row_idx = i;
                        col_idx = item.Idx;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Contains(T val)
        {
            int foo = 0, bar = 0;
            return IndexOf(val, ref foo, ref bar); // throws ArgumentNullException
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int GetRowCount()
        {
            int count = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                if (row.Count > 0) { count++; }
            }
            return count;
        }

        public int CountValues()
        {
            int count = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                count += row.Count;
            }
            return count;
        }

        public double GetSparseness(int num_rows, int num_cols)
        {
            Utils.ThrowException(num_rows <= 0 ? new ArgumentException("num_rows") : null);
            Utils.ThrowException(num_cols <= 0 ? new ArgumentException("num_cols") : null);
            int all_values = num_rows * num_cols;
            return (double)(all_values - CountValues()) / (double)all_values;
        }

        public SparseVector<T> this[int row_idx]
        {
            get
            {
                Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
                if (row_idx >= m_rows.Count) { return null; }
                return m_rows[row_idx].Count > 0 ? m_rows[row_idx] : null;
            }
            set
            {
                Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
                if (value != null)
                {
                    if (row_idx >= m_rows.Count) { SetRowListSize(row_idx + 1); }
                    m_rows[row_idx] = value;
                }
                else if (row_idx < m_rows.Count)
                {
                    m_rows[row_idx].Clear();
                }
            }
        }

        public T this[int row_idx, int col_idx]
        {
            get
            {
                Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
                Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
                Utils.ThrowException(row_idx >= m_rows.Count ? new ArgumentValueException("row_idx") : null);
                return m_rows[row_idx][col_idx]; // throws ArgumentValueException
            }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("value") : null);
                Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
                Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
                if (row_idx >= m_rows.Count) { SetRowListSize(row_idx + 1); }
                m_rows[row_idx][col_idx] = value;
            }
        }

        public SparseVector<T> GetColCopy(int col_idx)
        {
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            SparseVector<T> col = new SparseVector<T>();
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                int direct_idx = row.GetDirectIdx(col_idx);
                if (direct_idx >= 0)
                {
                    col.InnerIdx.Add(row_idx);
                    col.InnerDat.Add(row.GetDirect(direct_idx).Dat);
                }
                row_idx++;
            }
            return col;
        }

        public SparseMatrix<T> GetTransposedCopy()
        {
            SparseMatrix<T> tr_mat = new SparseMatrix<T>();
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                foreach (IdxDat<T> item in row)
                {
                    if (item.Idx >= tr_mat.m_rows.Count)
                    {
                        tr_mat.SetRowListSize(item.Idx + 1);
                    }
                    tr_mat.m_rows[item.Idx].InnerIdx.Add(row_idx);
                    tr_mat.m_rows[item.Idx].InnerDat.Add(item.Dat);
                }
                row_idx++;
            }
            return tr_mat;
        }

        public bool Remove(T val)
        {
            Utils.ThrowException(val == null ? new ArgumentNullException("val") : null);
            bool val_found = false;
            foreach (SparseVector<T> row in m_rows)
            {
                val_found = row.Remove(val);
            }
            return val_found;
        }

        public void RemoveAt(int row_idx, int col_idx)
        {
            Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            if (row_idx < m_rows.Count)
            {
                m_rows[row_idx].RemoveAt(col_idx);
            }
        }

        public void RemoveRowAt(int row_idx)
        {
            this[row_idx] = null; // throws ArgumentOutOfRangeException
        }

        public void RemoveColAt(int col_idx)
        {
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            foreach (SparseVector<T> row in m_rows)
            {
                row.RemoveAt(col_idx);
            }
        }

        public void PurgeRowAt(int row_idx)
        {
            Utils.ThrowException(row_idx < 0 ? new ArgumentOutOfRangeException("row_idx") : null);
            if (row_idx < m_rows.Count)
            {
                m_rows.RemoveAt(row_idx);
            }
        }

        public void PurgeColAt(int col_idx)
        {
            Utils.ThrowException(col_idx < 0 ? new ArgumentOutOfRangeException("col_idx") : null);
            foreach (SparseVector<T> row in m_rows)
            {
                row.PurgeAt(col_idx);
            }
        }

        public void Clear()
        {
            m_rows.Clear();
        }

        public void AppendCols(SparseMatrix<T>.ReadOnly other_matrix, int this_matrix_num_cols)
        {
            Utils.ThrowException(this_matrix_num_cols < 0 ? new ArgumentOutOfRangeException("this_matrix_num_cols") : null);
            int other_matrix_num_rows = other_matrix.GetLastNonEmptyRowIdx() + 1;
            if (m_rows.Count < other_matrix_num_rows)
            {
                SetRowListSize(other_matrix_num_rows);
            }
            for (int row_idx = 0; row_idx < other_matrix_num_rows; row_idx++)
            {
                m_rows[row_idx].Append(other_matrix.Inner.m_rows[row_idx], this_matrix_num_cols); // throws ArgumentOutOfRangeException
                row_idx++;
            }
        }

        public void Merge(SparseMatrix<T>.ReadOnly other_matrix, IBinaryOperator<T> binary_operator)
        {
            Utils.ThrowException(binary_operator == null ? new ArgumentNullException("binary_operator") : null);
            int other_matrix_num_rows = other_matrix.GetLastNonEmptyRowIdx() + 1;
            if (m_rows.Count < other_matrix_num_rows)
            {
                SetRowListSize(other_matrix_num_rows);
            }
            for (int row_idx = 0; row_idx < other_matrix_num_rows; row_idx++)
            {
                m_rows[row_idx].Merge(other_matrix.Inner.m_rows[row_idx], binary_operator);       
            }
        }

        public void PerformUnaryOperation(IUnaryOperator<T> unary_operator)
        {
            Utils.ThrowException(unary_operator == null ? new ArgumentNullException("unary_operator") : null);
            foreach (SparseVector<T> row in m_rows)
            {
                row.PerformUnaryOperation(unary_operator); 
            }
        }

        public void Symmetrize(IBinaryOperator<T> bin_op)
        {
            SparseMatrix<T> tr_mat = GetTransposedCopy();
            tr_mat.RemoveDiagonal();
            tr_mat.Merge(this, bin_op); // throws ArgumentNullException
            m_rows = tr_mat.m_rows;
        }

        public bool IsSymmetric()
        {
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                foreach (IdxDat<T> item in row)
                {
                    if (item.Idx >= m_rows.Count) { return false; }
                    int direct_idx = m_rows[item.Idx].GetDirectIdx(row_idx);
                    if (direct_idx < 0) { return false; }
                    T val = m_rows[item.Idx].GetDirect(direct_idx).Dat;
                    if (!item.Dat.Equals(val)) { return false; }
                }
                row_idx++;
            }
            return true;
        }

        public void SetDiagonal(int mtx_size, T val)
        {
            Utils.ThrowException(mtx_size < 0 ? new ArgumentOutOfRangeException("mtx_size") : null);
            Utils.ThrowException(val == null ? new ArgumentNullException("val") : null);
            if (mtx_size > m_rows.Count) { SetRowListSize(mtx_size); }
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                row[row_idx++] = val;
            }
        }

        public void RemoveDiagonal()
        {
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                row.RemoveAt(row_idx++);
            }
        }

        public bool ContainsDiagonalElement()
        {
            int row_idx = 0;
            foreach (SparseVector<T> row in m_rows)
            {
                if (row.ContainsAt(row_idx++)) { return true; }
            }
            return false;
        }

        // *** IEnumerable<IdxDat<SparseVector<T>>> interface implementation ***

        public IEnumerator<IdxDat<SparseVector<T>>> GetEnumerator()
        {
            return new MatrixEnumerator(this);
        }

        // *** IEnumerable interface implementation ***

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // *** ICloneable interface implementation ***

        public SparseMatrix<T> Clone()
        {
            SparseMatrix<T> clone = new SparseMatrix<T>();
            foreach (SparseVector<T> row in m_rows)
            {
                clone.m_rows.Add(row.Clone());
            }
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable interface implementation ***

        public SparseMatrix<T> DeepClone()
        {
            SparseMatrix<T> clone = new SparseMatrix<T>();
            clone.m_rows = m_rows.DeepClone();
            return clone;
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<SparseMatrix<T>> interface implementation ***

        public bool ContentEquals(SparseMatrix<T> other)
        {
            if (other == null) { return false; }
            int row_count = GetLastNonEmptyRowIdx() + 1;
            if (row_count != other.GetLastNonEmptyRowIdx() + 1) { return false; }
            for (int row_idx = 0; row_idx < row_count; row_idx++)
            {
                if (!m_rows[row_idx].ContentEquals(other.m_rows[row_idx])) { return false; }
            }
            return true;
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is SparseMatrix<T>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((SparseMatrix<T>)other);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            int row_count = GetLastNonEmptyRowIdx() + 1;
            // the following statements throw serialization-related exceptions            
            writer.WriteInt(row_count);
            for (int row_idx = 0; row_idx < row_count; row_idx++)
            {
                m_rows[row_idx].Save(writer);
            }
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_rows.Clear();
            // the following statements throw serialization-related exceptions
            int row_count = reader.ReadInt();
            SetRowListSize(row_count);
            for (int row_idx = 0; row_idx < row_count; row_idx++)
            {
                m_rows[row_idx] = new SparseVector<T>(reader);
            }
        }

        // *** Implicit cast to a read-only adapter ***

        public static implicit operator SparseMatrix<T>.ReadOnly(SparseMatrix<T> matrix)
        {
            if (matrix == null) { return null; }
            return new SparseMatrix<T>.ReadOnly(matrix);
        }

        // *** Equality comparer ***

        // *** note that two matrices are never equal if one is trimmed and the other is not
        public static IEqualityComparer<SparseMatrix<T>> GetEqualityComparer()
        {
            return new GenericEqualityComparer<SparseMatrix<T>>();
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class MatrixEnumerator
           |
           '-----------------------------------------------------------------------
        */
        private class MatrixEnumerator : IEnumerator<IdxDat<SparseVector<T>>>
        {
            private SparseMatrix<T> m_matrix;
            private int m_row_idx
                = -1;

            public MatrixEnumerator(SparseMatrix<T> matrix)
            {
                m_matrix = matrix;
            }

            // *** IEnumerator<IdxDat<SparseVector<T>>> interface implementation ***

            public IdxDat<SparseVector<T>> Current
            {
                get { return new IdxDat<SparseVector<T>>(m_row_idx, m_matrix.m_rows[m_row_idx]); } // throws ArgumentOutOfRangeException
            }

            // *** IEnumerator interface implementation ***

            object IEnumerator.Current
            {
                get { return Current; } // throws ArgumentOutOfRangeException
            }

            public bool MoveNext()
            {
                m_row_idx++;
                while (m_row_idx < m_matrix.m_rows.Count && m_matrix.m_rows[m_row_idx].Count == 0)
                {
                    m_row_idx++;
                }
                if (m_row_idx == m_matrix.m_rows.Count)
                {
                    Reset();
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                m_row_idx = -1;
            }

            // *** IDisposable interface implementation ***

            public void Dispose()
            {
            }
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class ReadOnlyMatrixEnumerator
           |
           '-----------------------------------------------------------------------
        */
        private class ReadOnlyMatrixEnumerator : IEnumerator<IdxDat<SparseVector<T>.ReadOnly>>
        {
            private SparseMatrix<T> m_matrix;
            private int m_row_idx
                = -1;

            public ReadOnlyMatrixEnumerator(SparseMatrix<T> matrix)
            {
                m_matrix = matrix;
            }

            // *** IEnumerator<IdxDat<SparseVector<T>.ReadOnly>> interface implementation ***

            public IdxDat<SparseVector<T>.ReadOnly> Current
            {
                get { return new IdxDat<SparseVector<T>.ReadOnly>(m_row_idx, m_matrix.m_rows[m_row_idx]); } // throws ArgumentOutOfRangeException
            }

            // *** IEnumerator interface implementation ***

            object IEnumerator.Current
            {
                get { return Current; } // throws ArgumentOutOfRangeException
            }

            public bool MoveNext()
            {
                m_row_idx++;
                while (m_row_idx < m_matrix.m_rows.Count && m_matrix.m_rows[m_row_idx].Count == 0)
                {
                    m_row_idx++;
                }
                if (m_row_idx == m_matrix.m_rows.Count)
                {
                    Reset();
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                m_row_idx = -1;
            }

            // *** IDisposable interface implementation ***

            public void Dispose()
            {
            }
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class SparseMatrix<T>.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<SparseMatrix<T>>, IEnumerable<IdxDat<SparseVector<T>.ReadOnly>>, IContentEquatable<SparseMatrix<T>.ReadOnly>,
            ISerializable
        {
            private SparseMatrix<T> m_matrix;

            public ReadOnly(SparseMatrix<T> matrix)
            {
                Utils.ThrowException(matrix == null ? new ArgumentNullException("matrix") : null);
                m_matrix = matrix;
            }

            public ReadOnly(BinarySerializer reader)
            {
                m_matrix = new SparseMatrix<T>(reader); // throws ArgumentNullException, serialization-related exceptions
            }

            public bool ContainsRowAt(int row_idx)
            {
                return m_matrix.ContainsRowAt(row_idx);
            }

            public bool ContainsColAt(int col_idx)
            {
                return m_matrix.ContainsColAt(col_idx);
            }

            public bool ContainsAt(int row_idx, int col_idx)
            {
                return m_matrix.ContainsAt(row_idx, col_idx);
            }

            public int GetFirstNonEmptyRowIdx()
            {
                return m_matrix.GetFirstNonEmptyRowIdx();
            }

            public int GetLastNonEmptyRowIdx()
            {
                return m_matrix.GetLastNonEmptyRowIdx();
            }

            public int GetFirstNonEmptyColIdx()
            {
                return m_matrix.GetFirstNonEmptyColIdx();
            }

            public int GetLastNonEmptyColIdx()
            {
                return m_matrix.GetLastNonEmptyColIdx();
            }

            public override string ToString()
            {
                return m_matrix.ToString();
            }

            public string ToString(string format)
            {
                return m_matrix.ToString(format);
            }

            public bool IndexOf(T val, ref int row_idx, ref int col_idx)
            {
                return m_matrix.IndexOf(val, ref row_idx, ref col_idx);
            }

            public bool Contains(T val)
            {
                return m_matrix.Contains(val);
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public int GetRowCount()
            {
                return m_matrix.GetRowCount();
            }

            public int CountValues()
            {
                return m_matrix.CountValues();
            }

            public double GetSparseness(int num_rows, int num_cols)
            {
                return m_matrix.GetSparseness(num_rows, num_cols);
            }

            public SparseVector<T>.ReadOnly this[int row_idx]
            {
                get { return m_matrix[row_idx]; }
            }

            public T this[int row_idx, int col_idx]
            {
                get { return m_matrix[row_idx, col_idx]; }
            }

            public SparseVector<T> GetColCopy(int col_idx)
            {
                return m_matrix.GetColCopy(col_idx);
            }

            public SparseMatrix<T> GetTransposedCopy()
            {
                return m_matrix.GetTransposedCopy();
            }

            public bool IsSymmetric()
            {
                return m_matrix.IsSymmetric();
            }

            public bool HasDiagonal()
            {
                return m_matrix.ContainsDiagonalElement();
            }

            // *** IReadOnlyAdapter interface implementation ***

            public SparseMatrix<T> GetWritableCopy()
            {
                return m_matrix.Clone();
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
            SparseMatrix<T> Inner
            {
                get { return m_matrix; }
            }

            // *** IEnumerable<IdxDat<SparseVector<T>.ReadOnly>> interface implementation ***

            public IEnumerator<IdxDat<SparseVector<T>.ReadOnly>> GetEnumerator()
            {
                return new ReadOnlyMatrixEnumerator(m_matrix);
            }

            // *** IEnumerable interface implementation ***

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            // *** IContentEquatable<SparseMatrix<T>.ReadOnly> interface implementation ***

            public bool ContentEquals(SparseMatrix<T>.ReadOnly other)
            {
                return other != null && m_matrix.ContentEquals(other.Inner);
            }

            bool IContentEquatable.ContentEquals(object other)
            {
                Utils.ThrowException(other != null && !(other is SparseMatrix<T>.ReadOnly) ? new ArgumentTypeException("other") : null);
                return ContentEquals((SparseMatrix<T>.ReadOnly)other);
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                m_matrix.Save(writer);
            }

            // *** Equality comparer ***

            // *** note that two matrices are never equal if one is trimmed and the other is not
            public static IEqualityComparer<SparseMatrix<T>.ReadOnly> GetEqualityComparer()
            {
                return new GenericEqualityComparer<SparseMatrix<T>.ReadOnly>();
            }
        }
    }
}

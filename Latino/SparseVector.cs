/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          SparseVector.cs
 *  Version:       1.0
 *  Desc:		   Sparse vector data structure 
 *  Author:        Miha Grcar
 *  Created on:    Mar-2007
 *  Last modified: May-2009
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
       |  Class SparseVector<T>
       |
       '-----------------------------------------------------------------------
    */
    public class SparseVector<T> : IEnumerable<IdxDat<T>>, ICloneable<SparseVector<T>>, IDeeplyCloneable<SparseVector<T>>, IContentEquatable<SparseVector<T>>,
        ISerializable
    {
        private ArrayList<int> m_idx
            = new ArrayList<int>();
        private ArrayList<T> m_dat
            = new ArrayList<T>();

        public SparseVector()
        {
        }

        public SparseVector(int capacity)
        {
            m_idx = new ArrayList<int>(capacity); // throws ArgumentOutOfRangeException
            m_dat = new ArrayList<T>(capacity);
        }

        public SparseVector(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public SparseVector(IEnumerable<T> vals)
        {
            AddRange(vals); // throws ArgumentNullException, ArgumentValueException
        }

        public SparseVector(IEnumerable<IdxDat<T>> sorted_list)
        {
            AddRange(sorted_list); // throws ArgumentNullException, ArgumentValueException
        }

#if PUBLIC_INNER
        public
#else
        internal 
#endif
        ArrayList<int> InnerIdx
        {
            get { return m_idx; }
        }

#if PUBLIC_INNER
        public
#else
        internal 
#endif
        ArrayList<T> InnerDat
        {
            get { return m_dat; }
        }

        public bool ContainsAt(int index)
        {
            Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
            return m_idx.BinarySearch(index) >= 0;
        }

        public int FirstNonEmptyIndex
        {
            get
            {
                if (m_idx.Count == 0) { return -1; }
                return m_idx[0];
            }
        }

        public int LastNonEmptyIndex
        {
            get
            {
                if (m_idx.Count == 0) { return -1; }
                return m_idx.Last;
            }
        }

        public IdxDat<T> First
        {
            get { return new IdxDat<T>(m_idx[0], m_dat[0]); } // throws ArgumentOutOfRangeException
        }

        public IdxDat<T> Last
        {
            get { return new IdxDat<T>(m_idx.Last, m_dat.Last); } // throws ArgumentOutOfRangeException
        }

        public override string ToString()
        {
            StringBuilder str_bld = new StringBuilder("{");
            for (int i = 0; i < m_idx.Count; i++)
            {
                str_bld.Append(" ");
                str_bld.Append(string.Format("( {0} {1} )", m_idx[i], m_dat[i]));
            }
            str_bld.Append(" }");
            return str_bld.ToString();
        }

        public void Append(SparseVector<T>.ReadOnly other_vec, int this_vec_len)
        {
            Utils.ThrowException(other_vec == null ? new ArgumentNullException("other_vec") : null);
            Utils.ThrowException(this_vec_len <= LastNonEmptyIndex ? new ArgumentOutOfRangeException("this_vec_len") : null);
            foreach (IdxDat<T> item_info in other_vec)
            {
                m_idx.Add(item_info.Idx + this_vec_len);
                m_dat.Add(item_info.Dat); // *** note that the elements are not cloned (you need to clone them yourself if needed)
            }
        }

        public void Merge(SparseVector<T>.ReadOnly other_vec, IBinaryOperator<T> binary_operator)
        {
            Utils.ThrowException(other_vec == null ? new ArgumentNullException("other_vec") : null);
            Utils.ThrowException(binary_operator == null ? new ArgumentNullException("binary_operator") : null);
            ArrayList<int> other_idx = other_vec.Inner.InnerIdx;
            ArrayList<T> other_dat = other_vec.Inner.InnerDat;
            ArrayList<int> new_idx = new ArrayList<int>(m_idx.Count + other_idx.Count);
            ArrayList<T> new_dat = new ArrayList<T>(m_dat.Count + other_dat.Count);
            int i = 0, j = 0;
            while (i < m_idx.Count && j < other_idx.Count)
            {
                int a_idx = m_idx[i];
                int b_idx = other_idx[j];
                if (a_idx == b_idx)
                {
                    T value = binary_operator.PerformOperation(m_dat[i], other_dat[j]); 
                    if (value != null) { new_idx.Add(a_idx); new_dat.Add(value); }
                    i++;
                    j++;
                }
                else if (a_idx < b_idx)
                {
                    new_idx.Add(a_idx); new_dat.Add(m_dat[i]); 
                    i++;
                }
                else
                {
                    new_idx.Add(b_idx); new_dat.Add(other_dat[j]); 
                    j++;
                }
            }
            for (; i < m_idx.Count; i++)
            {
                new_idx.Add(m_idx[i]); new_dat.Add(m_dat[i]); 
            }
            for (; j < other_idx.Count; j++)
            {
                new_idx.Add(other_idx[j]); new_dat.Add(other_dat[j]); 
            }
            m_idx = new_idx;
            m_dat = new_dat;
        }

        public void PerformUnaryOperation(IUnaryOperator<T> unary_operator)
        {
            Utils.ThrowException(unary_operator == null ? new ArgumentNullException("unary_operator") : null);
            for (int i = m_dat.Count - 1; i >= 0; i--)
            {
                T value = unary_operator.PerformOperation(m_dat[i]);
                if (value == null)
                {
                    RemoveDirect(i);
                }
                else
                {
                    SetDirect(i, value); 
                }
            }
        }

        // *** Direct access ***

        public IdxDat<T> GetDirect(int direct_idx)
        {
            return new IdxDat<T>(m_idx[direct_idx], m_dat[direct_idx]); // throws ArgumentOutOfRangeException
        }

        public int GetIdxDirect(int direct_idx)
        {
            return m_idx[direct_idx]; // throws ArgumentOutOfRangeException
        }

        public T GetDatDirect(int direct_idx)
        {
            return m_dat[direct_idx]; // throws ArgumentOutOfRangeException
        }

        public void SetDirect(int direct_idx, T value)
        {
            Utils.ThrowException(value == null ? new ArgumentNullException("value") : null);
            m_dat[direct_idx] = value; // throws ArgumentOutOfRangeException
        }

        public void RemoveDirect(int direct_idx)
        {
            m_idx.RemoveAt(direct_idx); // throws ArgumentOutOfRangeException
            m_dat.RemoveAt(direct_idx);
        }

        public int GetDirectIdx(int index)
        {
            Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
            int direct_idx = m_idx.BinarySearch(index);
            return direct_idx;
        }

        public int GetDirectIdx(int index, int direct_start_idx)
        {
            Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
            Utils.ThrowException((direct_start_idx < 0 || direct_start_idx >= m_idx.Count) ? new ArgumentOutOfRangeException("direct_start_idx") : null);
            int direct_idx = m_idx.BinarySearch(direct_start_idx, m_idx.Count - direct_start_idx, index, /*comparer=*/null);
            return direct_idx;
        }

        // *** Partial IList<T> interface implementation ***

        public int IndexOf(T item)
        {
            Utils.ThrowException(item == null ? new ArgumentNullException("item") : null);
            for (int i = 0; i < m_dat.Count; i++)
            {
                if (m_dat[i].Equals(item)) { return m_idx[i]; }
            }
            return -1;
        }

        public void RemoveAt(int index)
        {
            Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
            int direct_idx = m_idx.BinarySearch(index);
            if (direct_idx >= 0) 
            { 
                m_idx.RemoveAt(direct_idx); 
                m_dat.RemoveAt(direct_idx); 
            }
        }

        public void PurgeAt(int index)
        {
            Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
            int direct_idx = m_idx.BinarySearch(index);
            if (direct_idx >= 0) 
            {                 
                m_idx.RemoveAt(direct_idx);
                m_dat.RemoveAt(direct_idx);
            } 
            else 
            { 
                direct_idx = ~direct_idx; 
            }
            for (int i = direct_idx; i < m_idx.Count; i++) 
            { 
                m_idx[i]--;
            }
        }

        public T this[int index]
        {
            get
            {
                Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
                int direct_idx = m_idx.BinarySearch(index);
                Utils.ThrowException(direct_idx < 0 ? new ArgumentValueException("index") : null);
                return m_dat[direct_idx];
            }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("value") : null);
                Utils.ThrowException(index < 0 ? new ArgumentOutOfRangeException("index") : null);
                int direct_idx = m_idx.BinarySearch(index);
                if (direct_idx >= 0)
                {
                    m_dat[direct_idx] = value;
                }
                else
                {
                    m_idx.Insert(~direct_idx, index);
                    m_dat.Insert(~direct_idx, value);
                }
            }
        }

        public void Add(T val)
        {
            Utils.ThrowException(val == null ? new ArgumentNullException("val") : null);
            m_idx.Add(LastNonEmptyIndex + 1);
            m_dat.Add(val);
        }

        public void AddRange(IEnumerable<T> vals)
        {
            Utils.ThrowException(vals == null ? new ArgumentNullException("vals") : null);
            int idx = LastNonEmptyIndex + 1;
            foreach (T val in vals)
            {
                Utils.ThrowException(val == null ? new ArgumentValueException("vals") : null);
                m_idx.Add(idx++);
                m_dat.Add(val);
            }
        }

        public void AddRange(IEnumerable<IdxDat<T>> sorted_list)
        {
            Utils.ThrowException(sorted_list == null ? new ArgumentNullException("sorted_list") : null);
            int i = 0;
            int old_idx = -1;
            ArrayList<int> new_idx = new ArrayList<int>(m_idx.Count * 2);
            ArrayList<T> new_dat = new ArrayList<T>(m_dat.Count * 2);
            IEnumerator<IdxDat<T>> enumer = sorted_list.GetEnumerator();
            bool item_avail = enumer.MoveNext();
            IdxDat<T> item;
            if (item_avail)
            {
                item = enumer.Current;
                Utils.ThrowException((item.Dat == null || item.Idx <= old_idx) ? new ArgumentValueException("sorted_list") : null);
                old_idx = item.Idx;
            }
            else
            {
                return;
            }
            while (i < m_idx.Count && item_avail)
            {
                if (item.Idx > m_idx[i])
                {
                    new_idx.Add(m_idx[i]);
                    new_dat.Add(m_dat[i]);
                    i++;
                }
                else if (item.Idx < m_idx[i])
                {
                    new_idx.Add(item.Idx);
                    new_dat.Add(item.Dat);
                    item_avail = enumer.MoveNext();
                    if (item_avail)
                    {
                        item = enumer.Current;
                        Utils.ThrowException((item.Dat == null || item.Idx <= old_idx) ? new ArgumentValueException("sorted_list") : null);
                        old_idx = item.Idx;
                    }
                }
                else
                {
                    throw new ArgumentValueException("sorted_list");
                }
            }
            while (item_avail)
            {
                new_idx.Add(item.Idx);
                new_dat.Add(item.Dat);
                item_avail = enumer.MoveNext();
                if (item_avail)
                {
                    item = enumer.Current;
                    Utils.ThrowException((item.Dat == null || item.Idx <= old_idx) ? new ArgumentValueException("sorted_list") : null);
                    old_idx = item.Idx;
                }
            }
            while (i < m_idx.Count)
            {
                new_idx.Add(m_idx[i]);
                new_dat.Add(m_dat[i]);
                i++;
            }
            m_idx = new_idx;
            m_dat = new_dat;
        }

#if PUBLIC_INNER
        public
#else
        internal 
#endif
        void Sort()
        {
            IdxDat<int>[] tmp = new IdxDat<int>[m_idx.Count];
            for (int i = 0; i < m_idx.Count; i++)
            {
                tmp[i] = new IdxDat<int>(m_idx[i], i);
            }
            Array.Sort(tmp);
            ArrayList<T> new_dat = new ArrayList<T>(m_dat.Count);
            for (int i = 0; i < tmp.Length; i++)
            {
                m_idx[i] = tmp[i].Idx;
                new_dat.Add(m_dat[tmp[i].Dat]);
            }
            m_dat = new_dat;
        }

        // *** Partial ICollection<T> interface implementation ***

        public void Clear()
        {             
            m_idx.Clear();
            m_dat.Clear();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0; // throws ArgumentNullException
        }

        public int Count
        {
            get { return m_idx.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            Utils.ThrowException(item == null ? new ArgumentNullException("item") : null);
            bool val_found = false;
            for (int direct_idx = m_dat.Count - 1; direct_idx >= 0; direct_idx--)
            {
                if (item.Equals(m_dat[direct_idx]))
                {
                    m_idx.RemoveAt(direct_idx);
                    m_dat.RemoveAt(direct_idx);
                    val_found = true;
                }
            }
            return val_found;
        }

        // *** IEnumerable<IdxDat<T>> interface implementation ***

        public IEnumerator<IdxDat<T>> GetEnumerator()
        {
            return new SparseVectorEnumerator(this);
        }

        // *** IEnumerable interface implementation ***

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // *** ICloneable interface implementation ***

        public SparseVector<T> Clone()
        {
            SparseVector<T> clone = new SparseVector<T>();
            clone.m_idx = m_idx.Clone();
            clone.m_dat = m_dat.Clone();
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable interface implementation ***

        public SparseVector<T> DeepClone()
        {
            SparseVector<T> clone = new SparseVector<T>();
            clone.m_idx.Capacity = m_idx.Capacity;
            clone.m_dat.Capacity = m_dat.Capacity;
            for (int i = 0; i < m_idx.Count; i++)
            {
                clone.m_idx.Add(m_idx[i]);
                clone.m_dat.Add((T)Utils.Clone(m_dat[i], /*deep_clone=*/true));
            }
            return clone;
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<SparseVector<T>> interface implementation ***

        public bool ContentEquals(SparseVector<T> other)
        {
            if (other == null || m_idx.Count != other.m_idx.Count) { return false; }
            for (int i = 0; i < m_idx.Count; i++)
            {
                if (m_idx[i] != other.m_idx[i] || !Utils.ObjectEquals(m_dat[i], other.m_dat[i], /*deep_cmp=*/true)) 
                { 
                    return false; 
                }
            }
            return true;
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is SparseVector<T>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((SparseVector<T>)other);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exception
            writer.WriteInt(m_idx.Count);
            for (int i = 0; i < m_idx.Count; i++)
            {
                writer.WriteInt(m_idx[i]);
                writer.WriteValueOrObject<T>(m_dat[i]);
            }
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_idx.Clear();
            m_dat.Clear();
            // the following statements throw serialization-related exception
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                m_idx.Add(reader.ReadInt());
                m_dat.Add(reader.ReadValueOrObject<T>());
            }
        }

        // *** Implicit cast to a read-only adapter ***

        public static implicit operator SparseVector<T>.ReadOnly(SparseVector<T> vec)
        {
            if (vec == null) { return null; }
            return new SparseVector<T>.ReadOnly(vec);
        }

        // *** Equality comparer ***

        public static IEqualityComparer<SparseVector<T>> GetEqualityComparer()
        {
            return new GenericEqualityComparer<SparseVector<T>>();
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class SparseVectorEnumerator
           |
           '-----------------------------------------------------------------------
        */
        private class SparseVectorEnumerator : IEnumerator<IdxDat<T>>
        {
            private SparseVector<T> m_vec;
            private int m_item_idx
                = -1;

            public SparseVectorEnumerator(SparseVector<T> vec)
            {
                m_vec = vec;
            }

            // *** IEnumerator<IdxDat<T>> interface implementation ***

            public IdxDat<T> Current
            {
                get { return new IdxDat<T>(m_vec.m_idx[m_item_idx], m_vec.m_dat[m_item_idx]); } // throws ArgumentOutOfRangeException
            }

            // *** IEnumerator interface implementation ***

            object IEnumerator.Current
            {
                get { return Current; } // throws ArgumentOutOfRangeException
            }

            public bool MoveNext()
            {
                m_item_idx++;
                if (m_item_idx == m_vec.m_idx.Count)
                {
                    Reset();
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                m_item_idx = -1;
            }

            // *** IDisposable interface implementation ***

            public void Dispose()
            {
            }
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class SparseVector<T>.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<SparseVector<T>>, IEnumerable<IdxDat<T>>, IContentEquatable<SparseVector<T>.ReadOnly>,
            ISerializable
        {
            private SparseVector<T> m_vec;

            public ReadOnly(SparseVector<T> vec)
            {
                Utils.ThrowException(vec == null ? new ArgumentNullException("vec") : null);
                m_vec = vec;
            }

            public ReadOnly(BinarySerializer reader)
            {
                m_vec = new SparseVector<T>(reader); // throws ArgumentNullException, serialization-related exceptions
            }

            public bool ContainsAt(int index)
            {
                return m_vec.ContainsAt(index);
            }

            public int FirstNonEmptyIndex
            {
                get { return m_vec.FirstNonEmptyIndex; }
            }

            public int LastNonEmptyIndex
            {
                get { return m_vec.LastNonEmptyIndex; }
            }

            public IdxDat<T> First
            {
                get { return m_vec.First; }
            }

            public IdxDat<T> Last
            {
                get { return m_vec.Last; }
            }

            public override string ToString()
            {
                return m_vec.ToString();
            }

            // *** Direct access ***

            public IdxDat<T> GetDirect(int direct_idx)
            {
                return m_vec.GetDirect(direct_idx);
            }

            public int GetDirectIdx(int index)
            {
                return m_vec.GetDirectIdx(index);
            }

            public int GetDirectIdx(int index, int direct_start_idx)
            {
                return m_vec.GetDirectIdx(index, direct_start_idx);
            }

            // *** IReadOnlyAdapter interface implementation ***

            public SparseVector<T> GetWritableCopy()
            {
                return m_vec.Clone();
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
            SparseVector<T> Inner
            {
                get { return m_vec; }
            }

            // *** Partial IList<T> interface implementation ***

            public int IndexOf(T item)
            {
                return m_vec.IndexOf(item);
            }

            public T this[int idx]
            {
                get { return m_vec[idx]; }
                set { m_vec[idx] = value; }
            }

            // *** Partial ICollection<T> interface implementation ***

            public bool Contains(T item)
            {
                return m_vec.Contains(item);
            }

            public int Count
            {
                get { return m_vec.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            // *** IEnumerable<IdxDat<T>> interface implementation ***

            public IEnumerator<IdxDat<T>> GetEnumerator()
            {
                return m_vec.GetEnumerator();
            }

            // *** IEnumerable interface implementation ***

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)m_vec).GetEnumerator();
            }

            // *** IContentEquatable<SparseVector<T>.ReadOnly> interface implementation ***

            public bool ContentEquals(SparseVector<T>.ReadOnly other)
            {
                return other != null && m_vec.ContentEquals(other.Inner);
            }

            bool IContentEquatable.ContentEquals(object other)
            {
                Utils.ThrowException((other != null && !(other is SparseVector<T>.ReadOnly)) ? new ArgumentTypeException("other") : null);
                return ContentEquals((SparseVector<T>.ReadOnly)other);
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                m_vec.Save(writer);
            }

            // *** Equality comparer ***

            public static IEqualityComparer<SparseVector<T>.ReadOnly> GetEqualityComparer()
            {
                return new GenericEqualityComparer<SparseVector<T>.ReadOnly>();
            }
        }
    }
}

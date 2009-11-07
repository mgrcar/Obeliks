/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          BinaryVector.cs
 *  Version:       1.0
 *  Desc:		   Binary vector data structure 
 *  Author:        Miha Grcar
 *  Created on:    Oct-2009
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class BinaryVector<T>
       |
       '-----------------------------------------------------------------------
    */
    public class BinaryVector<T> : ICollection<T>, ICollection, IEnumerable<T>, ICloneable<BinaryVector<T>>, IDeeplyCloneable<BinaryVector<T>>, IContentEquatable<BinaryVector<T>>, 
        ISerializable where T : IComparable<T>
    {
        private List<T> m_vec
            = new List<T>();

        public BinaryVector()
        {
        }

        public BinaryVector(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public BinaryVector(IEnumerable<T> items)
        {
            AddRange(items); // throws ArgumentNullException
        }

#if PUBLIC_INNER
        public
#else
        internal
#endif
        List<T> Inner
        {
            get { return m_vec; }
        }

        private void RemoveDuplicates()
        {
            int j = 0;
            int i = 1;
            for (; i < m_vec.Count; i++)
            {
                if (m_vec[j].CompareTo(m_vec[i]) < 0)
                {
                    if (i != ++j)
                    {
                        m_vec[j] = m_vec[i];
                    }
                }
            }
            if (i != ++j) 
            { 
                m_vec.RemoveRange(j, m_vec.Count - j); 
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            Utils.ThrowException(items == null ? new ArgumentNullException("items") : null);
            int old_len = m_vec.Count;
            m_vec.AddRange(items);
            if (m_vec.Count == old_len) { return; }
            m_vec.Sort();
            RemoveDuplicates();
        }

        public T[] ToArray()
        {
            T[] array = new T[m_vec.Count];
            CopyTo(array, 0);
            return array;
        }

        public T this[int idx]
        {
            get { return m_vec[idx]; } // throws ArgumentOutOfRangeException
        }

        public override string ToString()
        {
            StringBuilder str_builder = new StringBuilder("{");
            foreach (T item in m_vec)
            {
                str_builder.Append(" ");
                str_builder.Append(item);
            }
            str_builder.Append(" }");
            return str_builder.ToString();
        }

        // *** ICollection<T> interface implementation ***

        public void Add(T item)
        {
            int idx = m_vec.BinarySearch(item);
            if (idx < 0) { idx = ~idx; }
            m_vec.Insert(idx, item);
        }

        public void Clear()
        {
            m_vec.Clear();
        }

        public bool Contains(T item)
        {
            return m_vec.BinarySearch(item) >= 0;
        }

        public void CopyTo(T[] array, int index)
        {
            Utils.ThrowException(array == null ? new ArgumentNullException("array") : null);
            Utils.ThrowException(index + m_vec.Count > array.Length ? new ArgumentOutOfRangeException("index") : null);
            foreach (T item in m_vec)
            {
                array.SetValue(item, index++);
            }
        }

        public int Count
        {
            get { return m_vec.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int idx = m_vec.BinarySearch(item);
            if (idx >= 0)
            {
                m_vec.RemoveAt(idx);
                return true;
            }
            return false;
        }

        // *** ICollection interface implementation ***

        void ICollection.CopyTo(Array array, int index)
        {
            Utils.ThrowException(array == null ? new ArgumentNullException("array") : null);
            Utils.ThrowException(index + m_vec.Count > array.Length ? new ArgumentOutOfRangeException("index") : null);
            foreach (T item in m_vec)
            {
                array.SetValue(item, index++);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        // *** IEnumerable<T> interface implementation ***

        public IEnumerator<T> GetEnumerator()
        {
            return m_vec.GetEnumerator();
        }

        // *** IEnumerable interface implementation ***

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // *** ICloneable interface implementation ***

        public BinaryVector<T> Clone()
        {
            BinaryVector<T> clone = new BinaryVector<T>();
            clone.m_vec = new List<T>(m_vec);
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable interface implementation ***

        public BinaryVector<T> DeepClone()
        {
            List<T> vec = new List<T>(m_vec.Count);
            foreach (T item in m_vec)
            {
                vec.Add((T)Utils.Clone(item, /*deep_clone=*/true));
            }
            BinaryVector<T> clone = new BinaryVector<T>();
            clone.m_vec = vec;
            return clone;
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<BinaryVector<T>> interface implementation ***

        public bool ContentEquals(BinaryVector<T> other)
        {
            if (other == null || m_vec.Count != other.m_vec.Count) { return false; }
            for (int i = 0; i < m_vec.Count; i++)
            {
                if (m_vec[i].CompareTo(other.m_vec[i]) != 0) { return false; }
            }
            return true;
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is BinaryVector<T>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((BinaryVector<T>)other);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions 
            writer.WriteInt(m_vec.Count);
            foreach (T item in m_vec)
            {
                writer.WriteValueOrObject<T>(item);
            }
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_vec.Clear();
            // the following statements throw serialization-related exceptions 
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                m_vec.Add(reader.ReadValueOrObject<T>());
            }
        }

        // *** Implicit cast to a read-only adapter ***

        public static implicit operator BinaryVector<T>.ReadOnly(BinaryVector<T> vec)
        {
            if (vec == null) { return null; }
            return new BinaryVector<T>.ReadOnly(vec);
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class BinaryVector<T>.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<BinaryVector<T>>, ICollection, IEnumerable<T>, IEnumerable, IContentEquatable<BinaryVector<T>.ReadOnly>, ISerializable
        {
            private BinaryVector<T> m_vec;

            public ReadOnly(BinaryVector<T> vec)
            {
                Utils.ThrowException(vec == null ? new ArgumentNullException("vec") : null);
                m_vec = vec;
            }

            public ReadOnly(BinarySerializer reader)
            {
                m_vec = new BinaryVector<T>(reader); 
            }

            public T[] ToArray()
            {
                return m_vec.ToArray();
            }

            public T this[int idx]
            {
                get { return m_vec[idx]; }
            }

            public override string ToString()
            {
                return m_vec.ToString();
            }

            // *** IReadOnlyAdapter interface implementation ***

            public BinaryVector<T> GetWritableCopy()
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
            BinaryVector<T> Inner
            {
                get { return m_vec; }
            }

            // *** Partial ICollection<T> interface implementation ***

            public bool Contains(T item)
            {
                return m_vec.Contains(item);
            }

            public void CopyTo(T[] array, int index)
            {
                m_vec.CopyTo(array, index);
            }

            public int Count
            {
                get { return m_vec.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            // *** ICollection interface implementation ***

            void ICollection.CopyTo(Array array, int index)
            {
                ((ICollection)m_vec).CopyTo(array, index);
            }

            bool ICollection.IsSynchronized
            {
                get { throw new NotSupportedException(); }
            }

            object ICollection.SyncRoot
            {
                get { throw new NotSupportedException(); }
            }

            // *** IEnumerable<T> interface implementation ***

            public IEnumerator<T> GetEnumerator()
            {
                return m_vec.GetEnumerator();
            }

            // *** IEnumerable interface implementation ***

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)m_vec).GetEnumerator();
            }

            // *** IContentEquatable<BinaryVector<T>.ReadOnly> interface implementation ***

            public bool ContentEquals(BinaryVector<T>.ReadOnly other)
            {
                return other != null && m_vec.ContentEquals(other.Inner);
            }

            bool IContentEquatable.ContentEquals(object other)
            {
                Utils.ThrowException((other != null && !(other is BinaryVector<T>.ReadOnly)) ? new ArgumentTypeException("other") : null);
                return ContentEquals((BinaryVector<T>.ReadOnly)other);
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                m_vec.Save(writer);
            }
        }
    }
}

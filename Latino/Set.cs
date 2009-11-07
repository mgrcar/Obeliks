/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Set.cs
 *  Version:       1.0
 *  Desc:		   Set data structure based on Dictionary
 *  Author:        Miha Grcar
 *  Created on:    Mar-2007
 *  Last modified: May-2008
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
       |  Class Set<T>
       |
       '-----------------------------------------------------------------------
    */
    public class Set<T> : ICollection<T>, ICollection, IEnumerable<T>, ICloneable<Set<T>>, IDeeplyCloneable<Set<T>>, IContentEquatable<Set<T>>, ISerializable
    {
        private Dictionary<T, object> m_items
            = new Dictionary<T, object>();

        public Set()
        {
        }

        public Set(IEqualityComparer<T> comparer)
        {
            m_items = new Dictionary<T, object>(comparer);
        }

        public Set(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public Set(BinarySerializer reader, IEqualityComparer<T> comparer)
        {
            m_items = new Dictionary<T, object>(comparer);
            Load(reader); // throws ArgumentNullException
        }

        public Set(IEnumerable<T> items)
        {
            AddRange(items); // throws ArgumentNullException
        }

        public Set(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            m_items = new Dictionary<T, object>(comparer);
            AddRange(items); // throws ArgumentNullException
        }

        public void SetItems(IEnumerable<T> items)
        {
            m_items.Clear();
            AddRange(items); // throws ArgumentNullException
        }

        public void AddRange(IEnumerable<T> items)
        {
            Utils.ThrowException(items == null ? new ArgumentNullException("items") : null);
            foreach (T item in items)
            {
                if (!m_items.ContainsKey(item)) // throws ArgumentNullException
                {
                    m_items.Add(item, null);
                }
            }
        }

        public static Set<T> Union(Set<T>.ReadOnly a, Set<T>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            Set<T> c = (Set<T>)a.GetWritableCopy();
            c.AddRange(b);
            return c;
        }

        public static Set<T> Intersection(Set<T>.ReadOnly a, Set<T>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            Set<T> c = new Set<T>();
            if (b.Count < a.Count) { Set<T>.ReadOnly tmp; tmp = a; a = b; b = tmp; }
            foreach (T item in a)
            {
                if (b.Contains(item)) { c.Add(item); }
            }
            return c;
        }

        public static Set<T> Difference(Set<T>.ReadOnly a, Set<T>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            Set<T> c = new Set<T>();
            foreach (T item in a)
            {
                if (!b.Contains(item)) { c.Add(item); }
            }
            return c;
        }

        public static double JaccardSimilarity(Set<T>.ReadOnly a, Set<T>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            Set<T> c = Intersection(a, b);
            double div = (double)(a.Count + b.Count - c.Count);
            if (div == 0) { return 1; } // *** if both sets are empty, the similarity is 1
            return (double)c.Count / div;
        }

        public T[] ToArray()
        {
            T[] array = new T[m_items.Count];
            CopyTo(array, 0);
            return array;
        }

        public T Any
        { 
            get
            {
                foreach (KeyValuePair<T, object> item in m_items)
                {
                    return item.Key;
                }
                throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            StringBuilder str_bld = new StringBuilder("{");
            foreach (T item in m_items.Keys)
            {
                str_bld.Append(" ");
                str_bld.Append(item);
            }
            str_bld.Append(" }");
            return str_bld.ToString();
        }

        // *** ICollection<T> interface implementation ***

        public void Add(T item)
        {
            if (!m_items.ContainsKey(item)) // throws ArgumentNullException
            {
                m_items.Add(item, null);
            }
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public bool Contains(T item)
        {
            return m_items.ContainsKey(item); // throws ArgumentNullException
        }

        public void CopyTo(T[] array, int index)
        {
            Utils.ThrowException(array == null ? new ArgumentNullException("array") : null);
            Utils.ThrowException(index + m_items.Count > array.Length ? new ArgumentOutOfRangeException("index") : null);
            foreach (T item in m_items.Keys)
            {
                array.SetValue(item, index++);
            }
        }

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return m_items.Remove(item); // throws ArgumentNullException
        }

        // *** ICollection interface implementation ***

        void ICollection.CopyTo(Array array, int index)
        {
            Utils.ThrowException(array == null ? new ArgumentNullException("array") : null);
            Utils.ThrowException(index + m_items.Count > array.Length ? new ArgumentOutOfRangeException("index") : null);
            foreach (T item in m_items.Keys)
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
            return m_items.Keys.GetEnumerator();
        }

        // *** IEnumerable interface implementation ***

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // *** ICloneable interface implementation ***

        public Set<T> Clone()
        {
            return new Set<T>(m_items.Keys);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable interface implementation ***

        public Set<T> DeepClone()
        {
            Set<T> clone = new Set<T>();
            foreach (T item in m_items.Keys)
            {
                clone.Add((T)Utils.Clone(item, /*deep_clone=*/true));
            }
            return clone;
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<Set<T>> interface implementation ***

        public bool ContentEquals(Set<T> other)
        {
            if (other == null || Count != other.Count) { return false; }
            foreach (T item in m_items.Keys)
            {
                if (!other.Contains(item)) { return false; }
            }
            return true;
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is Set<T>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((Set<T>)other);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions 
            writer.WriteInt(m_items.Count); 
            foreach (KeyValuePair<T, object> item in m_items)
            {
                writer.WriteValueOrObject<T>(item.Key); 
            }
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_items.Clear();
            // the following statements throw serialization-related exceptions 
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                m_items.Add(reader.ReadValueOrObject<T>(), null); 
            }
        }

        // *** Implicit cast to a read-only adapter ***

        public static implicit operator Set<T>.ReadOnly(Set<T> set)
        {
            if (set == null) { return null; }
            return new Set<T>.ReadOnly(set);
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class Set<T>.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<Set<T>>, ICollection, IEnumerable<T>, IEnumerable, IContentEquatable<Set<T>.ReadOnly>, ISerializable
        {
            private Set<T> m_set;

            public ReadOnly(Set<T> set)
            {
                Utils.ThrowException(set == null ? new ArgumentNullException("set") : null);
                m_set = set;
            }

            public ReadOnly(BinarySerializer reader)
            {
                m_set = new Set<T>(reader); // throws ArgumentNullException, serialization-related exceptions
            }

            public T[] ToArray()
            {
                return m_set.ToArray();
            }

            public T Any
            {
                get { return m_set.Any; }
            }

            public override string ToString()
            {
                return m_set.ToString();
            }

            // *** IReadOnlyAdapter interface implementation ***

            public Set<T> GetWritableCopy()
            {
                return m_set.Clone();
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
            Set<T> Inner
            {
                get { return m_set; }
            }

            // *** Partial ICollection<T> interface implementation ***

            public bool Contains(T item)
            {
                return m_set.Contains(item);
            }

            public void CopyTo(T[] array, int index)
            {
                m_set.CopyTo(array, index);
            }

            public int Count
            {
                get { return m_set.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            // *** ICollection interface implementation ***

            void ICollection.CopyTo(Array array, int index)
            {
                ((ICollection)m_set).CopyTo(array, index);
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
                return m_set.GetEnumerator();
            }

            // *** IEnumerable interface implementation ***

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)m_set).GetEnumerator();
            }

            // *** IContentEquatable<Set<T>.ReadOnly> interface implementation ***

            public bool ContentEquals(Set<T>.ReadOnly other)
            {
                return other != null && m_set.ContentEquals(other.Inner);
            }

            bool IContentEquatable.ContentEquals(object other)
            {
                Utils.ThrowException((other != null && !(other is Set<T>.ReadOnly)) ? new ArgumentTypeException("other") : null);
                return ContentEquals((Set<T>.ReadOnly)other);
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                m_set.Save(writer);
            }
        }
    }
}
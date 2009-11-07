/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ArrayList.cs
 *  Version:       1.0
 *  Desc:		   Dynamic array data structure 
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
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
       |  Class ArrayList<T>
       |
       '-----------------------------------------------------------------------
    */
    public class ArrayList<T> : List<T>, ICloneable<ArrayList<T>>, IDeeplyCloneable<ArrayList<T>>, IContentEquatable<ArrayList<T>>, ISerializable
    {
        public ArrayList()
        {
        }

        public ArrayList(int capacity) : base(capacity) // throws ArgumentOutOfRangeException
        {
        }

        public ArrayList(IEnumerable<T> collection) : base(collection) // throws ArgumentNullException
        {
        }

        public ArrayList(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public new ArrayList<T> FindAll(Predicate<T> match)
        {
            return new ArrayList<T>(base.FindAll(match)); // throws ArgumentNullException
        }

        public new ArrayList<T> GetRange(int index, int count)
        {
            return new ArrayList<T>(base.GetRange(index, count)); // throws ArgumentException, ArgumentOutOfRangeException
        }

        public int InsertSorted(T item)
        {
            return InsertSorted(item, /*comparer=*/null);
        }

        public int InsertSorted(T item, IComparer<T> comparer)
        {
            return InsertSorted(0, Count, item, comparer);
        }

        public int InsertSorted(int index, int count, T item, IComparer<T> comparer)
        {
            int idx = BinarySearch(index, count, item, comparer); // throws ArgumentOutOfRangeException, ArgumentException, InvalidOperationException
            if (idx < 0) { idx = ~idx; }
            Insert(idx, item);
            return idx;
        }

        public void Shuffle()
        {
            Shuffle(new Random());
        }

        public void Shuffle(Random rnd)
        {
            Utils.ThrowException(rnd == null ? new ArgumentNullException("rnd") : null);
            // Durstenfeld's shuffle algorithm
            int n = Count;
            while (n > 1)
            {
                int k = rnd.Next(n);
                n--;
                T tmp = this[n];
                this[n] = this[k];
                this[k] = tmp;
            }
        }

        public T First
        {
            get { return this[0]; } // throws ArgumentOutOfRangeException
            set { this[0] = value; } // throws ArgumentOutOfRangeException
        }

        public T Last
        {
            get { return this[Count - 1]; } // throws ArgumentOutOfRangeException
            set { this[Count - 1] = value; } // throws ArgumentOutOfRangeException
        }

        public NewT[] ToArray<NewT>()
        {
            return ToArray<NewT>(/*fmt_provider=*/null); // throws InvalidCastException, FormatException, OverflowException
        }

        public NewT[] ToArray<NewT>(IFormatProvider fmt_provider)
        { 
            NewT[] array = new NewT[Count];
            for (int i = 0; i < Count; i++)
            {
                array[i] = (NewT)Utils.ChangeType(this[i], typeof(NewT)); // throws InvalidCastException, FormatException, OverflowException
            }
            return array;
        }

        public override string ToString()
        {
            StringBuilder str_bld = new StringBuilder("(");
            foreach (T item in this)
            {
                str_bld.Append(" ");
                str_bld.Append(item.ToString());
            }
            str_bld.Append(" )");
            return str_bld.ToString();
        }

        // *** ICloneable interface implementation ***

        public ArrayList<T> Clone()
        {
            return new ArrayList<T>(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable interface implementation ***

        public ArrayList<T> DeepClone()
        {
            ArrayList<T> clone = new ArrayList<T>(Capacity);
            foreach (T item in this)
            {
                clone.Add((T)Utils.Clone(item, /*deep_clone=*/true));
            }
            return clone;
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<ArrayList<T>> interface implementation ***

        public bool ContentEquals(ArrayList<T> other)
        {
            if (other == null || Count != other.Count) { return false; }
            for (int i = 0; i < Count; i++)
            {
                if (!Utils.ObjectEquals(this[i], other[i], /*deep_cmp=*/true)) { return false; }
            }
            return true;
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is ArrayList<T>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((ArrayList<T>)other);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt(Count); 
            foreach (T item in this) { writer.WriteValueOrObject<T>(item); } 
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            Clear();
            // the following statements throw serialization-related exceptions
            int count = reader.ReadInt(); 
            for (int i = 0; i < count; i++) { Add(reader.ReadValueOrObject<T>()); } 
        }

        // *** Implicit cast to a read-only adapter ***

        public static implicit operator ArrayList<T>.ReadOnly(ArrayList<T> list)
        {
            if (list == null) { return null; }
            return new ArrayList<T>.ReadOnly(list);
        }

        // *** Equality comparer ***

        public static IEqualityComparer<ArrayList<T>> GetEqualityComparer()
        {
            return new GenericEqualityComparer<ArrayList<T>>();
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class ArrayList<T>.ReadOnly
           |
           '-----------------------------------------------------------------------
        */
        public class ReadOnly : IReadOnlyAdapter<ArrayList<T>>, ICollection, IEnumerable<T>, IEnumerable, IContentEquatable<ArrayList<T>.ReadOnly>, ISerializable
        {
            private ArrayList<T> m_list;

            public ReadOnly(ArrayList<T> list)
            {
                Utils.ThrowException(list == null ? new ArgumentNullException("list") : null);
                m_list = list;
            }

            public ReadOnly(BinarySerializer reader)
            {
                m_list = new ArrayList<T>(reader); 
            }

            public int BinarySearch(T item)
            {
                return m_list.BinarySearch(item);
            }

            public void ConvertAll<NewT>(Converter<T, NewT> converter)
            {
                m_list.ConvertAll<NewT>(converter);
            }

            public bool Exists(Predicate<T> match)
            {
                return m_list.Exists(match);
            }

            public T Find(Predicate<T> match)
            {
                return m_list.Find(match);
            }

            public ArrayList<T> FindAll(Predicate<T> match)
            {
                return m_list.FindAll(match);
            }

            public int FindIndex(Predicate<T> match)
            {
                return m_list.FindIndex(match);
            }

            public T FindLast(Predicate<T> match)
            {
                return m_list.FindLast(match);
            }

            public int FindLastIndex(Predicate<T> match)
            {
                return m_list.FindLastIndex(match);
            }

            public void ForEach(Action<T> action)
            {
                m_list.ForEach(action);
            }

            public ArrayList<T> GetRange(int index, int count)
            {
                return m_list.GetRange(index, Count);
            }

            public int LastIndexOf(T item)
            {
                return m_list.LastIndexOf(item);
            }

            public T[] ToArray()
            {
                return m_list.ToArray();
            }

            public bool TrueForAll(Predicate<T> match)
            {
                return m_list.TrueForAll(match);
            }

            public T First
            {
                get { return m_list.First; }
            }

            public T Last
            {
                get { return m_list.Last; }
            }

            public NewT[] ToArray<NewT>()
            {
                return m_list.ToArray<NewT>();
            }

            public NewT[] ToArray<NewT>(IFormatProvider fmt_provider)
            {
                return m_list.ToArray<NewT>(fmt_provider);
            }

            public override string ToString()
            {
                return m_list.ToString();
            }

            // *** IReadOnlyAdapter interface implementation ***

            public ArrayList<T> GetWritableCopy()
            {
                return m_list.Clone();
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
            ArrayList<T> Inner
            {
                get { return m_list; }
            }

            // *** Partial IList<T> interface implementation ***

            public int IndexOf(T item)
            {
                return m_list.IndexOf(item);
            }

            public T this[int index]
            {
                get { return m_list[index]; }
            }

            // *** Partial ICollection<T> interface implementation ***

            public bool Contains(T item)
            {
                return m_list.Contains(item);
            }

            public void CopyTo(T[] array, int index)
            {
                m_list.CopyTo(array, index);
            }

            public int Count
            {
                get { return m_list.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            // *** ICollection interface implementation ***

            void ICollection.CopyTo(Array array, int index)
            {
                ((ICollection)m_list).CopyTo(array, index);
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
                return m_list.GetEnumerator();
            }

            // *** IEnumerable interface implementation ***

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            // *** IContentEquatable<ArrayList<T>.ReadOnly> interface implementation ***

            public bool ContentEquals(ArrayList<T>.ReadOnly other)
            {
                return other != null && m_list.ContentEquals(other.Inner);
            }

            bool IContentEquatable.ContentEquals(object other)
            {
                Utils.ThrowException((other != null && !(other is ArrayList<T>.ReadOnly)) ? new ArgumentTypeException("other") : null);
                return ContentEquals((ArrayList<T>.ReadOnly)other);
            }

            // *** ISerializable interface implementation ***

            public void Save(BinarySerializer writer)
            {
                m_list.Save(writer);
            }

            // *** Equality comparer ***

            public static IEqualityComparer<ArrayList<T>.ReadOnly> GetEqualityComparer()
            {
                return new GenericEqualityComparer<ArrayList<T>.ReadOnly>();
            }
        }
    }
}

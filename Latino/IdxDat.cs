/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IdxDat.cs
 *  Version:       1.0
 *  Desc:		   Indexed item data structure 
 *  Author:        Miha Grcar
 *  Created on:    Mar-2007
 *  Last modified: Dec-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Struct IdxDat<T>
       |
       '-----------------------------------------------------------------------
    */
    public struct IdxDat<T> : IPair<int, T>, IComparable<IdxDat<T>>, IComparable, IEquatable<IdxDat<T>>, ISerializable
    {
        private int m_idx;
        private T m_dat;

        public IdxDat(BinarySerializer reader)
        {
            m_idx = -1;
            m_dat = default(T);
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public IdxDat(int idx, T dat)
        {
            m_idx = idx;
            m_dat = dat;
        }

        public IdxDat(int idx)
        {
            m_idx = idx;
            m_dat = default(T);
        }

        public int Idx
        {
            get { return m_idx; }
        }

        public T Dat
        {
            get { return m_dat; }
            set { m_dat = value; }
        }

        public override int GetHashCode()
        {
            return m_idx.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("( {0} {1} )", m_idx, m_dat);
        }

        public static bool operator ==(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx == b.m_idx;
        }

        public static bool operator !=(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx != b.m_idx;
        }

        public static bool operator >(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx > b.m_idx;
        }

        public static bool operator <(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx < b.m_idx;
        }

        public static bool operator >=(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx >= b.m_idx;
        }

        public static bool operator <=(IdxDat<T> a, IdxDat<T> b)
        {
            return a.m_idx <= b.m_idx;
        }

        // *** IPair<int, T> interface implementation ***

        public int First
        {
            get { return m_idx; }
        }

        public T Second
        {
            get { return m_dat; }
        }

        // *** IComparable<IdxDat<T>> interface implementation ***

        public int CompareTo(IdxDat<T> other)
        {
            return m_idx.CompareTo(other.Idx);
        }

        // *** IComparable interface implementation ***

        int IComparable.CompareTo(object obj)
        {
            Utils.ThrowException(!(obj is IdxDat<T>) ? new ArgumentTypeException("obj") : null);
            return CompareTo((IdxDat<T>)obj);
        }

        // *** IEquatable<IdxDat<T>> interface implementation ***

        public bool Equals(IdxDat<T> other)
        {
            return other.m_idx == m_idx;
        }

        public override bool Equals(object obj)
        {
            Utils.ThrowException(!(obj is IdxDat<T>) ? new ArgumentTypeException("obj") : null);
            return Equals((IdxDat<T>)obj);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteInt(m_idx); 
            writer.WriteValueOrObject(m_dat); 
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_idx = reader.ReadInt(); 
            m_dat = reader.ReadValueOrObject<T>(); 
        }
    }
}

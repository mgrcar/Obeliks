/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          KeyDat.cs
 *  Version:       1.0
 *  Desc:		   Dictionary item data structure 
 *  Author:        Miha Grcar
 *  Created on:    Mar-2007
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Struct KeyDat<KeyT, DatT>
       |
       '-----------------------------------------------------------------------
    */
    public struct KeyDat<KeyT, DatT> : IPair<KeyT, DatT>, IComparable<KeyDat<KeyT, DatT>>, IComparable, IEquatable<KeyDat<KeyT, DatT>>, ISerializable where KeyT : IComparable<KeyT>
    {
        private KeyT m_key;
        private DatT m_dat;

        public KeyDat(BinarySerializer reader)
        {
            m_key = default(KeyT);
            m_dat = default(DatT);
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public KeyDat(KeyT key, DatT dat)
        {
            Utils.ThrowException(key == null ? new ArgumentNullException("key") : null);
            m_key = key;
            m_dat = dat;
        }

        public KeyDat(KeyT key)
        {
            Utils.ThrowException(key == null ? new ArgumentNullException("key") : null);
            m_key = key;
            m_dat = default(DatT);
        }

        public KeyT Key
        {
            get { return m_key; }
            set
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Key") : null);
                m_key = value;
            }
        }

        public DatT Dat
        {
            get { return m_dat; }
            set { m_dat = value; }
        }

        public override int GetHashCode()
        {
            return m_key.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("( {0} {1} )", m_key, m_dat);
        }

        public static bool operator ==(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) == 0;
        }

        public static bool operator !=(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) != 0;
        }

        public static bool operator >(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) > 0;
        }

        public static bool operator <(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) < 0;
        }

        public static bool operator >=(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) >= 0;
        }

        public static bool operator <=(KeyDat<KeyT, DatT> a, KeyDat<KeyT, DatT> b)
        {
            return a.m_key.CompareTo(b.m_key) <= 0;
        }

        // *** IPair<KeyT, DatT> interface implementation ***

        public KeyT First
        {
            get { return m_key; }
        }

        public DatT Second
        {
            get { return m_dat; }
        }

        // *** IComparable<KeyDat<KeyT, DatT>> interface implementation ***

        public int CompareTo(KeyDat<KeyT, DatT> other)
        {
            return m_key.CompareTo(other.Key);
        }

        // *** IComparable interface implementation ***

        int IComparable.CompareTo(object obj)
        {
            Utils.ThrowException(!(obj is KeyDat<KeyT, DatT>) ? new ArgumentTypeException("obj") : null);
            return CompareTo((KeyDat<KeyT, DatT>)obj);
        }

        // *** IEquatable<KeyDat<KeyT, DatT>> interface implementation ***

        public bool Equals(KeyDat<KeyT, DatT> other)
        {
            return other.m_key.Equals(m_key);
        }

        public override bool Equals(object obj)
        {
            Utils.ThrowException(!(obj is KeyDat<KeyT, DatT>) ? new ArgumentTypeException("obj") : null);
            return Equals((KeyDat<KeyT, DatT>)obj);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteValueOrObject<KeyT>(m_key); 
            writer.WriteValueOrObject<DatT>(m_dat);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_key = reader.ReadValueOrObject<KeyT>(); 
            m_dat = reader.ReadValueOrObject<DatT>(); 
        }
    }
}

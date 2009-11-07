/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Pair.cs
 *  Version:       1.0
 *  Desc:		   Pair data structure 
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: Aug-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Struct Pair<FirstT, SecondT>
       |
       '-----------------------------------------------------------------------
    */
    public struct Pair<FirstT, SecondT> : IPair<FirstT, SecondT>, IDeeplyCloneable<Pair<FirstT, SecondT>>, IContentEquatable<Pair<FirstT, SecondT>>,
        IComparable<Pair<FirstT, SecondT>>, IComparable, ISerializable
    {
        private FirstT m_first;
        private SecondT m_second;

        public Pair(BinarySerializer reader)
        {
            m_first = default(FirstT);
            m_second = default(SecondT);
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public Pair(FirstT first, SecondT second)
        {
            m_first = first;
            m_second = second;
        }

        public FirstT First
        {
            get { return m_first; }
            set { m_first = value; }
        }

        public SecondT Second
        {
            get { return m_second; }
            set { m_second = value; }
        }

        public override string ToString()
        {
            return string.Format("( {0}, {1} )", m_first, m_second);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Pair<FirstT, SecondT> first, Pair<FirstT, SecondT> second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(Pair<FirstT, SecondT> first, Pair<FirstT, SecondT> second)
        {
            return !first.Equals(second);
        }

        // *** IDeeplyCloneable interface implementation ***

        public Pair<FirstT, SecondT> DeepClone()
        {
            return new Pair<FirstT, SecondT>((FirstT)Utils.Clone(m_first, /*deep_clone=*/true), (SecondT)Utils.Clone(m_second, /*deep_clone=*/true));
        }

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<Pair<FirstT, SecondT>> interface implementation ***

        public bool ContentEquals(Pair<FirstT, SecondT> other)
        {
            return Utils.ObjectEquals(m_first, other.m_first, /*deep_cmp=*/true) && Utils.ObjectEquals(m_second, other.m_second, /*deep_cmp=*/true);
        }

        bool IContentEquatable.ContentEquals(object other)
        {
            Utils.ThrowException(other == null ? new ArgumentNullException("other") : null);
            Utils.ThrowException(!(other is Pair<FirstT, SecondT>) ? new ArgumentTypeException("other") : null);
            return ContentEquals((Pair<FirstT, SecondT>)other);
        }

        // *** IComparable<Pair<FirstT, SecondT>> interface implementation ***

        public int CompareTo(Pair<FirstT, SecondT> other)
        {
            if (m_first == null && other.m_first == null) { return 0; }
            else if (m_first == null) { return 1; }
            else if (other.m_first == null) { return -1; }
            else
            {
                int val = ((IComparable<FirstT>)m_first).CompareTo(other.m_first); // throws InvalidCastException
                if (val != 0) { return val; }
                else if (m_second == null && other.m_second == null) { return 0; }
                else if (m_second == null) { return 1; }
                else if (other.m_second == null) { return -1; }
                else { return ((IComparable<SecondT>)m_second).CompareTo(other.m_second); } // throws InvalidCastException
            }
        }

        // *** IComparable interface implementation ***

        int IComparable.CompareTo(object obj)
        {
            Utils.ThrowException(!(obj == null || obj is Pair<FirstT, SecondT>) ? new ArgumentTypeException("obj") : null);
            return CompareTo((Pair<FirstT, SecondT>)obj);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteValueOrObject<FirstT>(m_first); 
            writer.WriteValueOrObject<SecondT>(m_second); 
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_first = reader.ReadValueOrObject<FirstT>(); 
            m_second = reader.ReadValueOrObject<SecondT>(); 
        }

        // *** Equality comparer ***

        public static IEqualityComparer<Pair<FirstT, SecondT>> GetEqualityComparer()
        {
            return new GenericEqualityComparer<Pair<FirstT, SecondT>>();
        }
    }
}

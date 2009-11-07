/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          LabeledExample.cs
 *  Version:       1.0
 *  Desc:		   Labeled example data structure 
 *  Author:        Miha Grcar
 *  Created on:    Jan-2009
 *  Last modified: Jan-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Struct LabeledExample<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public struct LabeledExample<LblT, ExT> : ISerializable
    {
        private LblT m_lbl;
        private ExT m_ex;

        public LabeledExample(BinarySerializer reader)
        {
            m_lbl = default(LblT);
            m_ex = default(ExT);
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public LabeledExample(LblT lbl, ExT ex)
        {
            m_lbl = lbl;
            m_ex = ex;
        }

        public LblT Label
        {
            get { return m_lbl; }
            set { m_lbl = value; }
        }

        public ExT Example
        {
            get { return m_ex; }
            set { m_ex = value; }
        }

        public override string ToString()
        {
            return string.Format("( {0}, {1} )", m_lbl, m_ex);
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            writer.WriteValueOrObject<LblT>(m_lbl);
            writer.WriteValueOrObject<ExT>(m_ex);
        }

        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_lbl = reader.ReadValueOrObject<LblT>();
            m_ex = reader.ReadValueOrObject<ExT>();
        }
    }
}
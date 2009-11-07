/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ListEnum.cs
 *  Version:       1.0
 *  Desc:		   Generic list enumerator
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: Jan-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class ListEnum
       |
       '-----------------------------------------------------------------------
    */
    public class ListEnum : IEnumerator
    {
        private IEnumerableList m_list;
        private int m_current_idx
            = -1;

        public ListEnum(IEnumerableList list)
        {
            Utils.ThrowException(list == null ? new ArgumentNullException("list") : null);
            m_list = list;
        }

        // *** IEnumerator interface implementation ***

        public void Reset()
        {
            m_current_idx = -1;
        }

        public object Current
        {
            get { return m_list[m_current_idx]; } // throws ArgumentOutOfRangeException
        }

        public bool MoveNext()
        {
            m_current_idx++;
            if (m_current_idx >= m_list.Count) 
            { 
                Reset(); 
                return false; 
            }
            return true;
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class ListEnum<T>
       |
       '-----------------------------------------------------------------------
    */
    public class ListEnum<T> : IEnumerator<T>
    {
        private IEnumerableList<T> m_list;
        private int m_current_idx
            = -1;

        public ListEnum(IEnumerableList<T> list)
        {
            Utils.ThrowException(list == null ? new ArgumentNullException("list") : null);
            m_list = list;
        }

        // *** IEnumerator<T> interface implementation ***

        public void Reset()
        {
            m_current_idx = -1;
        }

        public T Current
        {
            get { return m_list[m_current_idx]; } // throws ArgumentOutOfRangeException
        }

        object IEnumerator.Current
        {
            get { return Current; } // throws ArgumentOutOfRangeException
        }

        public bool MoveNext()
        {
            m_current_idx++;
            if (m_current_idx >= m_list.Count) 
            { 
                Reset(); 
                return false; 
            }
            return true;
        }

        public void Dispose()
        {
        }
    }
}
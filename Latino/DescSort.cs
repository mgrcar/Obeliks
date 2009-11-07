/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DescSort.cs
 *  Version:       1.0
 *  Desc:		   Utility class for descending sort
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
       |  Class DescSort
       |
       '-----------------------------------------------------------------------
    */
    public class DescSort : IComparer<object>, IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null) { return 0; }
            else if (x == null) { return -1; }
            else if (y == null) { return 1; }
            else { return ((IComparable)y).CompareTo(x); } // throws InvalidCastException
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class DescSort<T>
       |
       '-----------------------------------------------------------------------
    */
    public class DescSort<T> : IComparer<T> where T : IComparable<T>
    {
        public int Compare(T x, T y)
        {
            if (x == null && y == null) { return 0; }
            else if (x == null) { return -1; }
            else if (y == null) { return 1; }
            else { return y.CompareTo(x); } 
        }
    }
}

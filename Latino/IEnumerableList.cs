/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IEnumerableList.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: May-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System.Collections;
using System.Collections.Generic;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IEnumerableList
       |
       '-----------------------------------------------------------------------
    */
    public interface IEnumerableList : IEnumerable
    {
        object this[int index] { get; }
        int Count { get; }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IEnumerableList<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface IEnumerableList<T> : IEnumerable<T>, IEnumerableList
    {
        new T this[int index] { get; }
    }
}
/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IReadOnlyAdapter.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: May-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IReadOnlyAdapter
       |
       '-----------------------------------------------------------------------
    */
    public interface IReadOnlyAdapter
    {
        object GetWritableCopy();
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IReadOnlyAdapter<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface IReadOnlyAdapter<T> : IReadOnlyAdapter
    {
        new T GetWritableCopy();
    } 
}
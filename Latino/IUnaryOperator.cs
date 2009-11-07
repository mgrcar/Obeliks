/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IUnaryOperator.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IUnaryOperator<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface IUnaryOperator<T>
    {
        T PerformOperation(T arg);
    }
}
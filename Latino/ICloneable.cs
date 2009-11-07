/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ICloneable.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: May-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface ICloneable<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface ICloneable<T> : ICloneable
    {
        new T Clone();
    }
}
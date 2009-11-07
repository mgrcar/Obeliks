/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IContentEquatable.cs
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
       |  Interface IContentEquatable
       |
       '-----------------------------------------------------------------------
    */
    public interface IContentEquatable
    {
        bool ContentEquals(object other);
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IContentEquatable<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface IContentEquatable<T> : IContentEquatable
    {
        bool ContentEquals(T other);
    }
}
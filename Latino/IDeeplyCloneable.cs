/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IDeeplyCloneable.cs
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
       |  Interface IDeeplyCloneable
       |
       '-----------------------------------------------------------------------
    */
    public interface IDeeplyCloneable
    {
        object DeepClone();
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IDeeplyCloneable<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface IDeeplyCloneable<T> : IDeeplyCloneable
    {
        new T DeepClone();
    }
}
/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ISerializable.cs
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
       |  Interface ISerializable
       |
       '-----------------------------------------------------------------------
    */
    public interface ISerializable
    {
        // *** note that you need to implement a constructor that loads an instance if the class implements ISerializable
        void Save(BinarySerializer writer);
    }
}
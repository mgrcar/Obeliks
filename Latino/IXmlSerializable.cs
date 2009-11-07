/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IXmlSerializable.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Nov-2007
 *  Last modified: May-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System.Xml;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IXmlSerializable
       |
       '-----------------------------------------------------------------------
    */
    public interface IXmlSerializable
    {
        // *** note that you should implement a constructor that loads an instance if the class implements IXmlSerializable
        void SaveXml(XmlWriter writer);
    }
}
/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Exceptions.cs
 *  Version:       1.0
 *  Desc:		   Additional exception classes
 *  Author:		   Miha Grcar
 *  Created on:    Feb-2008
 *  Last modified: May-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class ArgumentNotSupportedException
       |
       '-----------------------------------------------------------------------
    */
    public class ArgumentNotSupportedException : ArgumentException
    {
        public ArgumentNotSupportedException(string param_name) : base("The argument is not supported.", param_name)
        {
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class ArgumentTypeException
       |
       '-----------------------------------------------------------------------
    */
    public class ArgumentTypeException : ArgumentException
    {
        public ArgumentTypeException(string param_name) : base("The argument is not of one of the expected types.", param_name)
        {
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class ArgumentValueException
       |
       '-----------------------------------------------------------------------
    */
    public class ArgumentValueException : ArgumentException
    {
        public ArgumentValueException(string param_name) : base("The argument value or state is not valid.", param_name)
        {
        }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Class XmlFormatException
       |
       '-----------------------------------------------------------------------
    */
    public class XmlFormatException : Exception
    {
        public XmlFormatException() : base("The XML document is not in the expected format.")
        {
        }
    }
}

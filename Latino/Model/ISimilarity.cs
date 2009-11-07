/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ISimilarity.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Oct-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface ISimilarity<T>
       |
       '-----------------------------------------------------------------------
    */
    public interface ISimilarity<T> : ISerializable
    {
        double GetSimilarity(T a, T b);
    }
}

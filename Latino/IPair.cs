/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IPair.cs
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
       |  Interface IPair<FirstT, SecondT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IPair<FirstT, SecondT>
    {
        FirstT First { get; }
        SecondT Second { get; }
    }
}
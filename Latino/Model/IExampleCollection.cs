/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IExampleCollection.cs
 *  Version:       1.0
 *  Desc:		   Interface definition
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Oct-2008
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Interface IExampleCollection<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IExampleCollection<LblT> : IEnumerableList
    {
        Type ExampleType { get; }
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IExampleCollection<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IExampleCollection<LblT, ExT> : IExampleCollection<LblT>, IEnumerableList<LabeledExample<LblT, ExT>>
    {
    }
}

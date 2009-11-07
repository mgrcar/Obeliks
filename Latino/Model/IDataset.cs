/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IDataset.cs
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
       |  Interface IDataset<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IDataset<LblT> : IExampleCollection<LblT>, ISerializable
    {
        IDataset<LblT> ConvertDataset(Type new_ex_type, bool move);
        IDataset<LblT, ExT> ConvertDataset<ExT>(bool move);
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IDataset<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IDataset<LblT, ExT> : IDataset<LblT>, IExampleCollection<LblT, ExT>
    {
    }
}

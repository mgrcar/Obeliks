/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          IModel.cs
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
       |  Interface IModel<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IModel<LblT> : ISerializable
    {
        Type RequiredExampleType { get; }
        bool IsTrained { get; }
        void Train(IExampleCollection<LblT> dataset);
        ClassifierResult<LblT> Classify(object example);
    }

    /* .-----------------------------------------------------------------------
       |
       |  Interface IModel<LblT, ExT>
       |
       '-----------------------------------------------------------------------
    */
    public interface IModel<LblT, ExT> : IModel<LblT>
    {
        void Train(IExampleCollection<LblT, ExT> dataset);
        ClassifierResult<LblT> Classify(ExT example);
    }
}

/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DotProductSimilarity.cs
 *  Version:       1.0
 *  Desc:		   Similarity implementation
 *  Authors:       Miha Grcar, Matjaz Jursic
 *  Created on:    Dec-2008
 *  Last modified: May-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class DotProductSimilarity
       |
       '-----------------------------------------------------------------------
    */
    public class DotProductSimilarity : ISimilarity<SparseVector<double>.ReadOnly>
    {
        public DotProductSimilarity()
        {
        }

        public DotProductSimilarity(BinarySerializer reader)
        {
        }

        // *** ISimilarity<SparseVector<double>.ReadOnly> interface implementation ***

        public double GetSimilarity(SparseVector<double>.ReadOnly a, SparseVector<double>.ReadOnly b)
        {
            Utils.ThrowException(a == null ? new ArgumentNullException("a") : null);
            Utils.ThrowException(b == null ? new ArgumentNullException("b") : null);
            double dot_prod = 0;
            int i = 0, j = 0;
            int a_count = a.Count;
            int b_count = b.Count;
            if (a_count == 0 || b_count == 0) { return 0; }
            ArrayList<int> a_idx = a.Inner.InnerIdx;
            ArrayList<double> a_dat = a.Inner.InnerDat;
            ArrayList<int> b_idx = b.Inner.InnerIdx;
            ArrayList<double> b_dat = b.Inner.InnerDat;            
            int a_idx_i = a_idx[0];
            int b_idx_j = b_idx[0];
            while (true)
            {
                if (a_idx_i < b_idx_j)
                {
                    if (++i == a_count) { break; }
                    a_idx_i = a_idx[i];
                }
                else if (a_idx_i > b_idx_j)
                {
                    if (++j == b_count) { break; }
                    b_idx_j = b_idx[j];
                }
                else
                {
                    dot_prod += a_dat[i] * b_dat[j];
                    if (++i == a_count || ++j == b_count) { break; }
                    a_idx_i = a_idx[i];
                    b_idx_j = b_idx[j];                    
                }
            }
            return dot_prod;
        }

        // *** ISerializable interface implementation ***

        public void Save(BinarySerializer writer)
        {
        }
    }
}
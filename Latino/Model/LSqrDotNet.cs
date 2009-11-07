/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          LSqrDotNet.cs
 *  Version:       1.0
 *  Desc:		   C# wrapper for LSQR DLL
 *  Author:        Miha Grcar 
 *  Created on:    Oct-2007
 *  Last modified: Nov-2007
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |		 
       |  Class LSqrDll
       |
       '-----------------------------------------------------------------------
    */
    public static class LSqrDll
    {
#if DEBUG
        private const string LSQR_DLL = "LSqrDebug.dll";
#else
        private const string LSQR_DLL = "LSqr.dll";
#endif

        // *** External functions ***

        [DllImport(LSQR_DLL)]
        public static extern int NewMatrix(int row_count);
        [DllImport(LSQR_DLL)]
        public static extern void DeleteMatrix(int id);
        [DllImport(LSQR_DLL)]
        public static extern void InsertValue(int mat_id, int row_idx, int col_idx, double val);
        [DllImport(LSQR_DLL)]
        public static extern IntPtr DoLSqr(int mat_id, int mat_transp_id, double[] rhs, int max_iter);

        // *** Wrapper for external DoLSqr ***

        public static double[] DoLSqr(int num_cols, LSqrSparseMatrix mat, LSqrSparseMatrix mat_transp, double[] rhs, int max_iter)
        {
            IntPtr sol_ptr = DoLSqr(mat.Id, mat_transp.Id, rhs, max_iter);
            double[] sol = new double[num_cols];
            Marshal.Copy(sol_ptr, sol, 0, sol.Length);
            Marshal.FreeHGlobal(sol_ptr);
            return sol;
        }
    }

    /* .-----------------------------------------------------------------------
       |		 
       |  Class LSqrSparseMatrix
       |
       '-----------------------------------------------------------------------
    */
    public class LSqrSparseMatrix : IDisposable
    {
        private int m_id;

        public LSqrSparseMatrix(int row_count)
        {
            m_id = LSqrDll.NewMatrix(row_count);
        }

        ~LSqrSparseMatrix()
        {
            Dispose();
        }

        public int Id
        {
            get { return m_id; }
        }

        public void InsertValue(int row_idx, int col_idx, double val)
        {
            LSqrDll.InsertValue(m_id, row_idx, col_idx, val);
        }

        public static LSqrSparseMatrix FromDenseMatrix(double[,] mat)
        {
            LSqrSparseMatrix lsqr_mat = new LSqrSparseMatrix(mat.GetLength(0));
            for (int row = 0; row < mat.GetLength(0); row++)
            {
                for (int col = 0; col < mat.GetLength(1); col++)
                {
                    if (mat[row, col] != 0)
                    {
                        lsqr_mat.InsertValue(row, col, mat[row, col]);
                    }
                }
            }
            return lsqr_mat;
        }

        public static LSqrSparseMatrix TransposeFromDenseMatrix(double[,] mat)
        {
            LSqrSparseMatrix lsqr_mat = new LSqrSparseMatrix(mat.GetLength(1));
            for (int col = 0; col < mat.GetLength(1); col++)
            {
                for (int row = 0; row < mat.GetLength(0); row++)
                {
                    if (mat[row, col] != 0)
                    {
                        lsqr_mat.InsertValue(col, row, mat[row, col]);
                    }
                }
            }
            return lsqr_mat;
        }

        // *** IDisposable interface implementation ***

        public void Dispose()
        {
            if (m_id >= 0)
            {
                LSqrDll.DeleteMatrix(m_id);
                m_id = -1;
            }
        }
    }
}
/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          WeightedNetwork.cs 
 *  Version:       1.0
 *  Desc:		   Wighted network data structure 
 *  Author:        Miha Grcar
 *  Created on:    Jan-2008
 *  Last modified: Mar-2009
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;

namespace Latino
{     
    /* .-----------------------------------------------------------------------
       |
       |  Class WeightedNetwork<VtxT>
       |
       '-----------------------------------------------------------------------
    */
    public class WeightedNetwork<VtxT> : Network<VtxT, double>, ICloneable<WeightedNetwork<VtxT>>, IDeeplyCloneable<WeightedNetwork<VtxT>>
    {
        public WeightedNetwork() : base()
        {
        }

        public WeightedNetwork(IEqualityComparer<VtxT> vtx_cmp) : base(vtx_cmp)
        {
        }

        public WeightedNetwork(BinarySerializer reader) : base(reader) // throws ArgumentNullException, serialization-related exceptions
        {
        }

        public WeightedNetwork(BinarySerializer reader, IEqualityComparer<VtxT> vtx_cmp) : base(reader, vtx_cmp) // throws ArgumentNullException, serialization-related exceptions
        {
        }

        // *** Operations ***

        public bool HasPositiveEdges()
        {
            foreach (IdxDat<SparseVector<double>> row in m_mtx)
            {
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (item.Dat <= 0) { return false; }
                }
            }
            return true;
        }

        public bool HasNonNegativeEdges()
        {
            foreach (IdxDat<SparseVector<double>> row in m_mtx)
            {
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (item.Dat < 0) { return false; }
                }
            }
            return true;
        }

        public bool ContainsZeroEdge(double eps)
        {
            Utils.ThrowException(eps < 0 ? new ArgumentOutOfRangeException("eps") : null);
            foreach (IdxDat<SparseVector<double>> row in m_mtx)
            {
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (Math.Abs(item.Dat) <= eps) { return true; }
                }
            }
            return false;
        }

        public bool IsUndirected(double eps)
        {
            Utils.ThrowException(eps < 0 ? new ArgumentOutOfRangeException("eps") : null);
            foreach (IdxDat<SparseVector<double>> row in m_mtx)
            {
                foreach (IdxDat<double> item in row.Dat)
                {
                    if (m_mtx[item.Idx] == null) { return false; }
                    int direct_idx = m_mtx[item.Idx].GetDirectIdx(row.Idx);
                    if (direct_idx < 0) { return false; }
                    double val = m_mtx[item.Idx].GetDirect(direct_idx).Dat;
                    if (Math.Abs(item.Dat - val) > eps) { return false; }
                }
            }
            return true;
        }

        private KeyDat<int, int>[] GetShortestPaths(IEnumerable<int> src_vtx)
        {
            Set<KeyDat<int, int>> visited = new Set<KeyDat<int, int>>();
            foreach (int item in src_vtx) { visited.Add(new KeyDat<int, int>(item, 0)); }
            Queue<KeyDat<int, int>> queue = new Queue<KeyDat<int, int>>(visited);
            while (queue.Count > 0)
            {
                KeyDat<int, int> vtx_kd = queue.Dequeue();
                SparseVector<double> vtx_info = m_mtx[vtx_kd.Key];
                if (vtx_info != null)
                {
                    foreach (IdxDat<double> item in vtx_info)
                    {
                        if (!visited.Contains(new KeyDat<int, int>(item.Idx)))
                        {
                            KeyDat<int, int> new_vtx_kd = new KeyDat<int, int>(item.Idx, vtx_kd.Dat + 1);
                            visited.Add(new_vtx_kd);
                            queue.Enqueue(new_vtx_kd);
                        }
                    }
                }
            }
            return visited.ToArray();
        }

        public double[] PageRank()
        {
            return PageRank(/*src_vtx_list=*/null, /*damping=*/0.85); // throws InvalidOperationException, ArgumentOutOfRangeException
        }

        public double[] PageRank(IEnumerable<int> src_vtx_list)
        {
            return PageRank(src_vtx_list, /*damping=*/0.85); // throws InvalidOperationException, ArgumentOutOfRangeException
        }

        public double[] PageRank(IEnumerable<int> src_vtx_list, double damping)
        {
            return PageRank(src_vtx_list, damping, /*max_steps=*/10000, /*eps=*/0.00001); // throws InvalidOperationException, ArgumentOutOfRangeException
        }

        public double[] PageRank(IEnumerable<int> src_vtx_list, double damping, int max_steps, double eps)
        {
            return PageRank(src_vtx_list, damping, max_steps, eps, /*init_pr=*/null, /*no_bounding=*/true, /*inlinks=*/null); // throws InvalidOperationException, ArgumentOutOfRangeException
        }

        public double[] PageRank(IEnumerable<int> src_vtx_list, double damping, int max_steps, double eps, double[] init_pr, bool no_bounding, SparseMatrix<double>.ReadOnly inlinks)
        {            
            Utils.ThrowException((m_vtx.Count == 0 || !HasPositiveEdges()) ? new InvalidOperationException() : null);
            Utils.ThrowException((damping < 0 || damping >= 1) ? new ArgumentOutOfRangeException("damping") : null);
            Utils.ThrowException(max_steps <= 0 ? new ArgumentOutOfRangeException("max_steps") : null);
            Utils.ThrowException(eps < 0 ? new ArgumentOutOfRangeException("eps") : null);
            Utils.ThrowException((init_pr != null && init_pr.Length != m_vtx.Count) ? new ArgumentValueException("init_pr") : null);
            // *** inlinks needs to be the transposed form of this.Edges; to check if this is true, uncomment the following line            
            //Utils.ThrowException((inlinks != null && !((SparseMatrix<double>.ReadOnly)m_mtx.GetTransposedCopy()).ContentEquals(inlinks)) ? new ArgumentValueException("inlinks") : null);            
            int src_vtx_count = 0; 
            if (src_vtx_list != null)
            {
                foreach (int src_vtx_idx in src_vtx_list)
                {
                    src_vtx_count++;
                    Utils.ThrowException((src_vtx_idx < 0 || src_vtx_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("src_vtx_list item") : null);
                }
            }
            // precompute weight sums
            double[] wgt_sum = new double[m_vtx.Count];
            for (int i = 0; i < m_vtx.Count; i++)
            {
                wgt_sum[i] = 0;
                if (m_mtx[i] != null)
                {
                    foreach (IdxDat<double> other_vtx in m_mtx[i])
                    {
                        wgt_sum[i] += other_vtx.Dat;
                    }
                }
            }
            // initialize rank_vec
            double[] rank_vec = new double[m_vtx.Count];            
            if (init_pr != null)
            {
                double rank_sum = 0;
                foreach (double val in init_pr) 
                { 
                    rank_sum += val;
                    Utils.ThrowException(val < 0 ? new ArgumentOutOfRangeException("init_pr item") : null);
                }
                Utils.ThrowException(rank_sum == 0 ? new ArgumentValueException("init_pr") : null);
                for (int vtx_idx = 0; vtx_idx < m_vtx.Count; vtx_idx++)
                {
                    rank_vec[vtx_idx] = init_pr[vtx_idx] / rank_sum; 
                }
            }
            else
            {
                if (src_vtx_count == 0)
                {
                    double init_rank = 1.0 / (double)m_vtx.Count;
                    for (int vtx_idx = 0; vtx_idx < m_vtx.Count; vtx_idx++) { rank_vec[vtx_idx] = init_rank; }
                }
                else
                {
                    double init_rank = 1.0 / (double)src_vtx_count;
                    for (int vtx_idx = 0; vtx_idx < m_vtx.Count; vtx_idx++) { rank_vec[vtx_idx] = 0; }
                    foreach (int src_vtx_idx in src_vtx_list) { rank_vec[src_vtx_idx] = init_rank; }
                }
            }
            // transpose adjacency matrix
            if (inlinks == null)
            {
                inlinks = m_mtx.GetTransposedCopy();
            }
            // compute shortest paths
            KeyDat<int, int>[] vtx_info = null;
            if (src_vtx_count > 0 && !no_bounding)
            {
                KeyDat<int, int>[] tmp = GetShortestPaths(src_vtx_list);
                vtx_info = new KeyDat<int, int>[tmp.Length];
                int i = 0;
                foreach (KeyDat<int, int> item in tmp) { vtx_info[i++] = new KeyDat<int, int>(item.Dat, item.Key); }
                Array.Sort(vtx_info);
            }
            // main loop
            int step = 0;
            double diff;       
            do
            {
                //DateTime then = DateTime.Now;
                // compute new Page Rank for each vertex
                double[] new_rank_vec = new double[m_vtx.Count];
                if (src_vtx_count == 0 || no_bounding)
                {
                    for (int vtx_idx = 0; vtx_idx < m_vtx.Count; vtx_idx++)
                    {
                        double new_rank = 0;
                        if (inlinks.ContainsRowAt(vtx_idx))
                        {
                            foreach (IdxDat<double> other_vtx in inlinks[vtx_idx])
                            {
                                new_rank += other_vtx.Dat / wgt_sum[other_vtx.Idx] * (double)rank_vec[other_vtx.Idx];
                            }
                        }
                        new_rank_vec[vtx_idx] = new_rank;
                    }
                }
                else
                {
                    for (int i = 0; i < vtx_info.Length && (init_pr != null || vtx_info[i].Key <= step + 1); i++)
                    {
                        int vtx_idx = vtx_info[i].Dat;
                        double new_rank = 0;
                        if (inlinks.ContainsRowAt(vtx_idx))
                        {
                            foreach (IdxDat<double> other_vtx in inlinks[vtx_idx])
                            {
                                new_rank += other_vtx.Dat / wgt_sum[other_vtx.Idx] * (double)rank_vec[other_vtx.Idx];
                            }
                        }
                        new_rank_vec[vtx_idx] = new_rank;
                    }
                }
                // normalize new_rank_vec by distributing (1.0 - rank_sum) to source vertices
                double rank_sum = 0;
                for (int i = 0; i < new_rank_vec.Length; i++) { rank_sum += new_rank_vec[i]; }
                if (rank_sum <= 0.999999)
                {
                    if (src_vtx_count == 0)
                    {
                        double distr_rank = (1.0 - rank_sum) / (double)m_vtx.Count;
                        for (int i = 0; i < new_rank_vec.Length; i++) { new_rank_vec[i] += distr_rank; }
                    }
                    else
                    {
                        double distr_rank = (1.0 - rank_sum) / (double)src_vtx_count;
                        foreach (int src_vtx_idx in src_vtx_list) { new_rank_vec[src_vtx_idx] += distr_rank; }
                    }
                }
                // incorporate damping factor
                if (src_vtx_count == 0)
                {
                    double distr_rank = (1.0 - damping) / (double)m_vtx.Count;
                    for (int i = 0; i < new_rank_vec.Length; i++) { new_rank_vec[i] = damping * new_rank_vec[i] + distr_rank; }
                }
                else
                {
                    double distr_rank = (1.0 - damping) / (double)src_vtx_count;
                    for (int i = 0; i < new_rank_vec.Length; i++) { new_rank_vec[i] = damping * new_rank_vec[i]; }
                    foreach (int src_vtx_idx in src_vtx_list) { new_rank_vec[src_vtx_idx] += distr_rank; }
                }
                // compute difference
                diff = 0;
                for (int i = 0; i < m_vtx.Count; i++)
                {
                    diff += Math.Abs(rank_vec[i] - new_rank_vec[i]);
                }
                rank_vec = new_rank_vec;
                step++;
                //Console.WriteLine("Step {0}\tTime {1}", step, (DateTime.Now - then).TotalMilliseconds);
            } while (step < max_steps && diff > eps);
            return rank_vec;
        }

        // *** ICloneable<Network<VtxT, EdgeT>> interface adaptation ***

        new public WeightedNetwork<VtxT> Clone()
        {
            WeightedNetwork<VtxT> clone = new WeightedNetwork<VtxT>();
            clone.m_vtx = m_vtx.Clone();
            clone.m_mtx = m_mtx.Clone();
            int i = 0;
            foreach (VtxT vtx in clone.m_vtx)
            {
                if (vtx != null)
                {
                    clone.m_vtx_to_idx.Add(vtx, i++);
                }
            }
            return clone;
        }

        // *** IDeeplyCloneable<Network<VtxT, EdgeT>> interface adaptation ***

        new public WeightedNetwork<VtxT> DeepClone()
        {
            WeightedNetwork<VtxT> clone = new WeightedNetwork<VtxT>();
            clone.m_vtx = m_vtx.DeepClone();
            clone.m_mtx = m_mtx.DeepClone();
            int i = 0;
            foreach (VtxT vtx in clone.m_vtx)
            {
                if (vtx != null)
                {
                    clone.m_vtx_to_idx.Add(vtx, i++);
                }
            }
            return clone;
        }
    }
}
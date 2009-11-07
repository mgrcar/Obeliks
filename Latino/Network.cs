/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Network.cs 
 *  Version:       1.0
 *  Desc:		   Network data structure 
 *  Author:        Miha Grcar
 *  Created on:    Jan-2008
 *  Last modified: Mar-2009
 *  Revision:      Oct-2009
 * 
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Class Network<VtxT, EdgeT>
       |
       '-----------------------------------------------------------------------
    */
    public class Network<VtxT, EdgeT> : ICloneable<Network<VtxT, EdgeT>>, IDeeplyCloneable<Network<VtxT, EdgeT>>, IContentEquatable<Network<VtxT, EdgeT>>, 
        ISerializable
    {
        protected SparseMatrix<EdgeT> m_mtx
            = new SparseMatrix<EdgeT>();
        protected ArrayList<VtxT> m_vtx 
            = new ArrayList<VtxT>();        
        protected Dictionary<VtxT, int> m_vtx_to_idx;

        public Network()
        {
            m_vtx_to_idx = new Dictionary<VtxT, int>();
        }

        public Network(IEqualityComparer<VtxT> vtx_cmp)
        {
            m_vtx_to_idx = new Dictionary<VtxT, int>(vtx_cmp);
        }

        public Network(BinarySerializer reader)
        {
            Load(reader); // throws ArgumentNullException, serialization-related exceptions
        }

        public Network(BinarySerializer reader, IEqualityComparer<VtxT> vtx_cmp)
        {
            Load(reader, vtx_cmp); // throws ArgumentNullException, serialization-related exceptions
        }

        public override string ToString()
        {
            StringBuilder str_bld = new StringBuilder();
            for (int i = 0; i < m_vtx.Count; i++)
            {
                str_bld.Append(m_vtx[i]);
                str_bld.Append(": { ");
                if (m_mtx[i] != null)
                {
                    foreach (IdxDat<EdgeT> vtx_info in m_mtx[i])
                    {
                        str_bld.Append(string.Format("( {0} {1} ) ", m_vtx[vtx_info.Idx], vtx_info.Dat));
                    }                    
                }
                str_bld.AppendLine("}");
            }
            return str_bld.ToString().TrimEnd('\n', '\r');
        }

        public string ToString(string format)
        {
            if (format == "DEF") // default
            {
                return ToString();
            }
            else if (format == "AMC") // adjacency matrix - compact
            {
                return m_mtx.ToString("C");
            }
            else if (format == "AME") // adjacency matrix - extended
            {
                return m_mtx.ToString("E");
            }
            else
            {
                throw new ArgumentNotSupportedException("format");
            }
        }   
     
        // *** Vertices ***

        public int AddVertex(VtxT vtx)
        {
            m_vtx.Add(vtx);
            if (vtx != null) { m_vtx_to_idx.Add(vtx, m_vtx.Count - 1); } // throws ArgumentException
            return m_vtx.Count - 1;
        }

        public void SetVertexAt(int idx, VtxT vtx)
        {
            if (m_vtx[idx] != null) { m_vtx_to_idx.Remove(m_vtx[idx]); } // throws ArgumentOutOfRangeException
            m_vtx[idx] = vtx;
            if (vtx != null) { m_vtx_to_idx.Add(vtx, idx); } // throws ArgumentException
        }

        public void RemoveVertexAt(int idx)
        {
            if (m_vtx[idx] != null) { m_vtx_to_idx.Remove(m_vtx[idx]); } // throws ArgumentOutOfRangeException
            for (int i = idx + 1; i < m_vtx.Count; i++)
            {
                if (m_vtx[i] != null) { m_vtx_to_idx[m_vtx[i]]--; }
            }
            m_vtx.RemoveAt(idx);
            m_mtx.PurgeColAt(idx);
            m_mtx.PurgeRowAt(idx);
        }

        public void RemoveVertex(VtxT vtx)
        {
            RemoveVertexAt(m_vtx_to_idx[vtx]); // throws ArgumentNullException, KeyNotFoundException
        }

        public bool IsVertex(VtxT vtx)
        {
            return m_vtx_to_idx.ContainsKey(vtx); // throws ArgumentNullException
        }

        public int GetVertexIdx(VtxT vtx)
        {
            return m_vtx_to_idx[vtx]; // throws ArgumentNullException, KeyNotFoundException
        }

        public ArrayList<VtxT>.ReadOnly Vertices
        {
            get { return m_vtx; }
        }

        // *** Edges ***

        public void SetEdgeAt(int vtx_1_idx, int vtx_2_idx, EdgeT val)
        {
            Utils.ThrowException((vtx_1_idx < 0 || vtx_1_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_1_idx") : null);
            Utils.ThrowException((vtx_2_idx < 0 || vtx_2_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_2_idx") : null);
            m_mtx[vtx_1_idx, vtx_2_idx] = val; // throws ArgumentNullException
        }

        public void SetEdge(VtxT vtx_1, VtxT vtx_2, EdgeT val)
        {
            int vtx_1_idx = m_vtx_to_idx[vtx_1], vtx_2_idx = m_vtx_to_idx[vtx_2]; // throws ArgumentNullException, KeyNotFoundException            
            m_mtx[vtx_1_idx, vtx_2_idx] = val; // throws ArgumentNullException
        }

        public bool IsEdgeAt(int vtx_1_idx, int vtx_2_idx)
        {
            Utils.ThrowException((vtx_1_idx < 0 || vtx_1_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_1_idx") : null);
            Utils.ThrowException((vtx_2_idx < 0 || vtx_2_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_2_idx") : null);
            return m_mtx.ContainsAt(vtx_1_idx, vtx_2_idx);
        }

        public bool IsEdge(VtxT vtx_1, VtxT vtx_2)
        {
            int vtx_1_idx = m_vtx_to_idx[vtx_1], vtx_2_idx = m_vtx_to_idx[vtx_2]; // throws ArgumentNullException, KeyNotFoundException
            return m_mtx.ContainsAt(vtx_1_idx, vtx_2_idx);
        }

        public EdgeT GetEdgeAt(int vtx_1_idx, int vtx_2_idx)
        {
            Utils.ThrowException((vtx_1_idx < 0 || vtx_1_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_1_idx") : null);
            Utils.ThrowException((vtx_2_idx < 0 || vtx_2_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_2_idx") : null);
            return m_mtx[vtx_1_idx, vtx_2_idx]; // throws ArgumentValueException
        }

        public EdgeT GetEdge(VtxT vtx_1, VtxT vtx_2)
        {
            int vtx_1_idx = m_vtx_to_idx[vtx_1], vtx_2_idx = m_vtx_to_idx[vtx_2]; // throws ArgumentNullException, KeyNotFoundException            
            return m_mtx[vtx_1_idx, vtx_2_idx]; // throws ArgumentValueException
        }

        public void RemoveEdgeAt(int vtx_1_idx, int vtx_2_idx)
        {
            Utils.ThrowException((vtx_1_idx < 0 || vtx_1_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_1_idx") : null);
            Utils.ThrowException((vtx_2_idx < 0 || vtx_2_idx >= m_vtx.Count) ? new ArgumentOutOfRangeException("vtx_2_idx") : null);
            m_mtx.RemoveAt(vtx_1_idx, vtx_2_idx);
        }

        public void RemoveEdge(VtxT vtx_1, VtxT vtx_2)
        {
            int vtx_1_idx = m_vtx_to_idx[vtx_1], vtx_2_idx = m_vtx_to_idx[vtx_2]; // throws ArgumentNullException, KeyNotFoundException            
            m_mtx.RemoveAt(vtx_1_idx, vtx_2_idx); 
        }

        public SparseMatrix<EdgeT>.ReadOnly Edges
        {
            get { return m_mtx; }
        }

        public void ClearEdges()
        {
            m_mtx.Clear();
        }

        // *** Operations ***

        public void Clear()
        {
            m_mtx.Clear();
            m_vtx.Clear();
            m_vtx_to_idx.Clear();
        }

        public void PerformEdgeOperation(IUnaryOperator<EdgeT> unary_op)
        {
            m_mtx.PerformUnaryOperation(unary_op); // throws ArgumentNullException
        }

        public void ToUndirected(IBinaryOperator<EdgeT> bin_op)
        {
            m_mtx.Symmetrize(bin_op); // throws ArgumentNullException
        }

        public bool IsUndirected()
        {
            return m_mtx.IsSymmetric();
        }

        public void SetLoops(EdgeT val)
        {
            m_mtx.SetDiagonal(m_vtx.Count, val); // throws ArgumentNullException
        }

        public void RemoveLoops()
        {
            m_mtx.RemoveDiagonal();
        }

        public bool ContainsLoop()
        {
            return m_mtx.ContainsDiagonalElement();
        }

        public void InvertEdges()
        {
            m_mtx = m_mtx.GetTransposedCopy();
        }

        public double GetSparseness()
        {
            return m_mtx.GetSparseness(m_vtx.Count, m_vtx.Count); // throws ArgumentException
        }

        public SparseMatrix<EdgeT>[] GetComponentsUndirected(ref int[] seeds, bool seeds_only)
        {
            Utils.ThrowException(!IsUndirected() ? new InvalidOperationException() : null);
            ArrayList<int> seed_list = new ArrayList<int>();
            ArrayList<SparseMatrix<EdgeT>> components = new ArrayList<SparseMatrix<EdgeT>>();
            Set<int> unvisited = new Set<int>();
            for (int j = 0; j < m_vtx.Count; j++) { unvisited.Add(j); }
            while (unvisited.Count > 0)
            {
                int seed_idx = unvisited.Any;
                SparseMatrix<EdgeT> component = new SparseMatrix<EdgeT>();
                seed_list.Add(seed_idx);
                Queue<int> queue = new Queue<int>(new int[] { seed_idx });
                unvisited.Remove(seed_idx);
                while (queue.Count > 0)
                {
                    int vtx_idx = queue.Dequeue();
                    SparseVector<EdgeT> vtx_info = m_mtx[vtx_idx];
                    if (vtx_info != null)
                    {
                        if (!seeds_only)
                        {
                            component[vtx_idx] = vtx_info.Clone();
                        }
                        foreach (IdxDat<EdgeT> other_vtx in vtx_info)
                        {
                            if (unvisited.Contains(other_vtx.Idx))
                            {
                                unvisited.Remove(other_vtx.Idx);                                
                                queue.Enqueue(other_vtx.Idx);
                            }
                        }
                    }
                }
                if (!seeds_only && component.GetLastNonEmptyRowIdx() >= 0)
                {
                    components.Add(component);
                }
            }
            seeds = seed_list.ToArray();           
            return components.ToArray();
        }

        // *** ICloneable<Network<VtxT, EdgeT>> interface implementation ***

        public Network<VtxT, EdgeT> Clone()
        {
            Network<VtxT, EdgeT> clone = new Network<VtxT, EdgeT>();
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

        object ICloneable.Clone()
        {
            return Clone();
        }

        // *** IDeeplyCloneable<Network<VtxT, EdgeT>> interface implementation ***

        public Network<VtxT, EdgeT> DeepClone()
        {
            Network<VtxT, EdgeT> clone = new Network<VtxT, EdgeT>();
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

        object IDeeplyCloneable.DeepClone()
        {
            return DeepClone();
        }

        // *** IContentEquatable<Network<VtxT, EdgeT>> interface implementation ***

        public bool ContentEquals(Network<VtxT, EdgeT> other)
        {
            return other != null && m_mtx.ContentEquals(other.m_mtx) && m_vtx.ContentEquals(other.m_vtx);
        }

        public bool ContentEquals(object other)
        {
            Utils.ThrowException((other != null && !(other is Network<VtxT, EdgeT>)) ? new ArgumentTypeException("other") : null);
            return ContentEquals((Network<VtxT, EdgeT>)other);
        }

        // *** ISerializable interface implementation ***

        public void Load(BinarySerializer reader)
        {
            Load(reader, /*vtx_cmp=*/null); // throws ArgumentNullException, serialization-related exceptions
        }

        public void Load(BinarySerializer reader, IEqualityComparer<VtxT> vtx_cmp)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            // the following statements throw serialization-related exceptions
            m_mtx.Load(reader);
            m_vtx.Load(reader);
            m_vtx_to_idx = new Dictionary<VtxT, int>(vtx_cmp);
            int i = 0;
            foreach (VtxT vtx in m_vtx)
            {
                if (vtx != null)
                {
                    m_vtx_to_idx.Add(vtx, i++);
                }
            }
        }

        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            // the following statements throw serialization-related exceptions
            m_mtx.Save(writer);
            m_vtx.Save(writer);
        }
    }
}

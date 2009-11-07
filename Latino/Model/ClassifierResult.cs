/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          ClassifierResult.cs
 *  Version:       1.0
 *  Desc:		   Classifier result (output of ML models)
 *  Author:        Miha Grcar
 *  Created on:    Aug-2007
 *  Last modified: Oct-2009
 *  Revision:      Oct-2009
 *
 ***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace Latino.Model
{
    /* .-----------------------------------------------------------------------
       |
       |  Class ClassifierResult<LblT>
       |
       '-----------------------------------------------------------------------
    */
    public class ClassifierResult<LblT> : IEnumerableList<KeyDat<double, LblT>>
    {
        private ArrayList<KeyDat<double, LblT>> m_class_scores
            = new ArrayList<KeyDat<double, LblT>>();
        private static DescSort<KeyDat<double, LblT>> m_desc_sort
            = new DescSort<KeyDat<double, LblT>>();

        public ClassifierResult()
        {
        }

        public ClassifierResult(IEnumerable<KeyDat<double, LblT>> class_scores)
        {
            Utils.ThrowException(class_scores == null ? new ArgumentNullException("class_scores") : null);
            AddRange(class_scores);
        }

        public void AddRange(IEnumerable<KeyDat<double, LblT>> class_scores)
        {
            foreach (KeyDat<double, LblT> class_score in class_scores)
            {
                m_class_scores.Add(class_score);
            }
            m_class_scores.Sort(m_desc_sort);
        }

        public ArrayList<KeyDat<double, LblT>> Items
        {
            get { return m_class_scores; }
        }

        public double GetScoreAt(int idx)
        {
            Utils.ThrowException((idx < 0 || idx >= m_class_scores.Count) ? new ArgumentOutOfRangeException("idx") : null);
            return m_class_scores[idx].Key;
        }

        public LblT GetClassLabelAt(int idx)
        {
            Utils.ThrowException((idx < 0 || idx >= m_class_scores.Count) ? new ArgumentOutOfRangeException("idx") : null);
            return m_class_scores[idx].Dat;
        }

        public double BestScore
        {
            get
            {
                Utils.ThrowException(m_class_scores.Count == 0 ? new InvalidOperationException() : null);
                return m_class_scores[0].Key;
            }
        }

        public LblT BestClassLabel
        {
            get
            {
                Utils.ThrowException(m_class_scores.Count == 0 ? new InvalidOperationException() : null);
                return m_class_scores[0].Dat;
            }
        }

        public override string ToString()
        {
            return m_class_scores.ToString();
        }

        // *** IEnumerableList<KeyDat<double, LblT>> interface implementation ***

        public int Count
        {
            get { return m_class_scores.Count; }
        }

        public KeyDat<double, LblT> this[int idx]
        {
            get
            {
                Utils.ThrowException((idx < 0 || idx >= m_class_scores.Count) ? new ArgumentOutOfRangeException("idx") : null);
                return m_class_scores[idx];
            }
        }

        object IEnumerableList.this[int idx]
        {
            get { return this[idx]; } // throws ArgumentOutOfRangeException
        }

        public IEnumerator<KeyDat<double, LblT>> GetEnumerator()
        {
            return new ListEnum<KeyDat<double, LblT>>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ListEnum(this);
        }
    }
}
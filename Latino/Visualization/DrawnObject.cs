/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DrawnObject.cs
 *  Version:       1.0
 *  Desc:		   Drawable object
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: May-2009
 *  Revision:      May-2009
 *
 ***************************************************************************/

using System;
using System.Drawing;

namespace Latino.Visualization
{ 
    /* .-----------------------------------------------------------------------
       |		 
       |  Abstract class DrawnObject
       |
       '-----------------------------------------------------------------------
    */
    public abstract class DrawnObject : IDrawableObject
    {
        protected Pen m_pen
            = Pens.Black;
        protected BoundingArea m_bounding_area
            = null;
        public Pen Pen
        {
            get { return m_pen; }
            set 
            {
                Utils.ThrowException(value == null ? new ArgumentNullException("Pen") : null);
                m_pen = value; 
            }
        }
        protected void InvalidateBoundingArea()
        {
            m_bounding_area = null;
        }
        // *** IDrawableObject interface implementation ***
        public virtual IDrawableObject[] GetObjectsAt(float x, float y, TransformParams tr, ref float[] dist_array)
        {
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            float dist = 0;
            IDrawableObject drawable_object = GetObjectAt(x, y, tr, ref dist);
            if (drawable_object != null)
            {
                dist_array = new float[] { dist };
                return new IDrawableObject[] { drawable_object };
            }
            else
            {
                dist_array = new float[] { };
                return new IDrawableObject[] { };
            }
        }
        public virtual IDrawableObject[] GetObjectsIn(BoundingArea.ReadOnly area, TransformParams tr)
        {
            Utils.ThrowException(area == null ? new ArgumentNullException("area") : null);
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);            
            if (GetBoundingArea(tr).IntersectsWith(area)) 
            {
                return new IDrawableObject[] { this };
            }
            else
            {
                return new IDrawableObject[] { };
            }
        }
        public virtual BoundingArea GetBoundingArea(TransformParams tr)
        {
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            if (m_bounding_area == null) { m_bounding_area = GetBoundingArea(); }
            BoundingArea bounding_area = m_bounding_area.Clone();
            bounding_area.Transform(tr);
            lock (m_pen) { bounding_area.Inflate(m_pen.Width / 2f + 5f, m_pen.Width / 2f + 5f); }
            return bounding_area;
        }
        public virtual void Draw(Graphics gfx, TransformParams tr, BoundingArea.ReadOnly bounding_area)
        {
            Draw(gfx, tr);
        }
        // *** The following functions need to be implemented in derived classes ***
        public abstract void Draw(Graphics gfx, TransformParams tr);
        public abstract IDrawableObject GetObjectAt(float x, float y, TransformParams tr, ref float dist); 
        public abstract BoundingArea GetBoundingArea();
    }
}
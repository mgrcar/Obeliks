/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Ellipse.cs
 *  Version:       1.0
 *  Desc:		   Drawable ellipse
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: May-2009
 *  Revision:      May-2009
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |		 
       |  Class Ellipse 
       |
       '-----------------------------------------------------------------------
    */
    public class Ellipse : FilledDrawnObject
    {
        private float m_x;
        private float m_y;
        private float m_r_x;
        private float m_r_y;
        public Ellipse(float x, float y, float r_x, float r_y)
        {
            m_x = x;
            m_y = y;
            m_r_x = r_x;
            m_r_y = r_y;
        }
        public static void Draw(float x, float y, float r_x, float r_y, Graphics gfx, Pen pen, Brush brush, TransformParams tr)
        {
            Utils.ThrowException(gfx == null ? new ArgumentNullException("gfx") : null);
            Utils.ThrowException(pen == null ? new ArgumentNullException("pen") : null);
            Utils.ThrowException(brush == null ? new ArgumentNullException("brush") : null);
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            x -= r_x;
            y -= r_y;
            Vector2D center = tr.Transform(new Vector2D(x, y));
            float d_x = tr.Transform(2f * r_x);
            float d_y = tr.Transform(2f * r_y);
            lock (brush) { gfx.FillEllipse(brush, center.X, center.Y, d_x, d_y); }
            lock (pen) { gfx.DrawEllipse(pen, center.X, center.Y, d_x, d_y); }
        }
        public static BoundingArea GetBoundingArea(float x, float y, float r_x, float r_y)
        {
            float left = x - r_x;
            float top = y - r_y;
            float d_x = 2f * r_x;
            float d_y = 2f * r_y;
            return new BoundingArea(left, top, d_x, d_y);
        }
        public static bool IsObjectAt(float pt_x, float pt_y, TransformParams tr, float c_x, float c_y, float r_x, float r_y)
        {
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            Vector2D center = tr.Transform(new Vector2D(c_x, c_y));
            Vector2D pt = new Vector2D(pt_x, pt_y);
            if (pt == center) { return true; }
            float angle = (pt - center).GetAngle();
            float x = (float)Math.Cos(angle) * tr.Transform(r_x);
            float y = (float)Math.Sin(angle) * tr.Transform(r_y);
            float r = new Vector2D(x, y).GetLength();
            return (center - pt).GetLength() <= r;            
        }
        public float X
        {
            get { return m_x; }
            set 
            { 
                m_x = value; 
                InvalidateBoundingArea(); 
            }
        }
        public float Y
        {
            get { return m_y; }
            set 
            { 
                m_y = value; 
                InvalidateBoundingArea(); 
            }
        }
        public float RadiusX
        {
            get { return m_r_x; }
            set 
            { 
                m_r_x = value; 
                InvalidateBoundingArea(); 
            }
        }
        public float RadiusY
        {
            get { return m_r_y; }
            set 
            { 
                m_r_y = value; 
                InvalidateBoundingArea(); 
            }
        }
        public override void Draw(Graphics gfx, TransformParams tr)
        {
            Draw(m_x, m_y, m_r_x, m_r_y, gfx, m_pen, m_brush, tr); // throws ArgumentNullException, ArgumentValueException
        }
        public override IDrawableObject GetObjectAt(float x, float y, TransformParams tr, ref float dist)
        {
            dist = 0;
            return IsObjectAt(x, y, tr, m_x, m_y, m_r_x, m_r_y) ? this : null; // throws ArgumentValueException
        }
        public override BoundingArea GetBoundingArea()
        {
            return GetBoundingArea(m_x, m_y, m_r_x, m_r_y);
        }
    }
}
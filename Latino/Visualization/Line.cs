/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Line.cs
 *  Version:       1.0
 *  Desc:		   Drawable line
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: May-2009
 *  Revision:      May-2009
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.IO;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |		 
       |  Class Line 
       |
       '-----------------------------------------------------------------------
    */
    public class Line : DrawnObject
    {
        private float m_x1;
        private float m_y1;
        private float m_x2;
        private float m_y2;
        private static float m_hit_dist
            = 3;
        private static float m_max_box_area
            = 1000;
        public Line(float x1, float y1, float x2, float y2)
        {
            m_x1 = x1;
            m_y1 = y1;
            m_x2 = x2;
            m_y2 = y2;
        }
        public static void Draw(float x1, float y1, float x2, float y2, Graphics gfx, Pen pen, TransformParams tr)
        {
            Utils.ThrowException(gfx == null ? new ArgumentNullException("gfx") : null);
            Utils.ThrowException(pen == null ? new ArgumentNullException("pen") : null);
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            Vector2D pt1 = tr.Transform(new Vector2D(x1, y1));
            Vector2D pt2 = tr.Transform(new Vector2D(x2, y2));
            gfx.DrawLine(pen, pt1, pt2);
        }
        private static bool LineIntersectVertical(Vector2D pt1, Vector2D pt2, float x, ref float y)
        {
            if (pt1.X > pt2.X) { Vector2D tmp = pt1; pt1 = pt2; pt2 = tmp; } // swap points
            if (pt1.X < x && pt2.X > x)
            {
                float dY = pt2.Y - pt1.Y;
                if (dY == 0)
                {
                    y = pt1.Y;
                    return true;
                }
                float dX = pt2.X - pt1.X;
                float dx = x - pt1.X;
                float dy = dx * dY / dX;
                y = pt1.Y + dy;
                return true;
            }
            return false;
        }
        private static bool LineIntersectHorizontal(Vector2D pt1, Vector2D pt2, float y, ref float x)
        {
            return LineIntersectVertical(new Vector2D(pt1.Y, pt1.X), new Vector2D(pt2.Y, pt2.X), y, ref x);
        }
        private static bool LineIntersectRectangle(Vector2D pt1, Vector2D pt2, RectangleF rect, ref Vector2D isect_pt1, ref Vector2D isect_pt2)
        {
            float y = 0, x = 0;
            ArrayList<Vector2D> points = new ArrayList<Vector2D>(2); 
            if (LineIntersectVertical(pt1, pt2, rect.X, ref y))
            {
                if (y > rect.Y && y < rect.Y + rect.Height) { points.Add(new Vector2D(rect.X, y)); }
            }
            if (LineIntersectVertical(pt1, pt2, rect.X + rect.Width, ref y))
            {
                if (y > rect.Y && y < rect.Y + rect.Height) { points.Add(new Vector2D(rect.X + rect.Width, y)); }
            }
            if (LineIntersectHorizontal(pt1, pt2, rect.Y, ref x))
            {
                if (x > rect.X && x < rect.X + rect.Width) { points.Add(new Vector2D(x, rect.Y)); }
            }
            if (LineIntersectHorizontal(pt1, pt2, rect.Y + rect.Height, ref x))
            {
                if (x > rect.X && x < rect.X + rect.Width) { points.Add(new Vector2D(x, rect.Y + rect.Height)); }
            }
            if (points.Count == 2)
            {
                isect_pt1 = points[0];
                isect_pt2 = points[1];
                return true;
            }
            else if (points.Count == 1)
            {
                isect_pt1 = points[0];
                isect_pt2 = VisualizationUtils.PointInsideRect(pt1.X, pt1.Y, rect) ? pt1 : pt2;
                return true;
            }
            else if (VisualizationUtils.PointInsideRect(pt1.X, pt1.Y, rect) && VisualizationUtils.PointInsideRect(pt2.X, pt2.Y, rect)) 
            {
                isect_pt1 = pt1;
                isect_pt2 = pt2;
                return true;
            }
            return false;
        }
        public static void Draw(float x1, float y1, float x2, float y2, Graphics gfx, Pen pen, TransformParams tr, BoundingArea.ReadOnly bounding_area)
        {
#if !NO_PARTIAL_RENDERING
            Utils.ThrowException(gfx == null ? new ArgumentNullException("gfx") : null);
            Utils.ThrowException(pen == null ? new ArgumentNullException("pen") : null);
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            Utils.ThrowException(bounding_area == null ? new ArgumentNullException("bounding_area") : null);
            Vector2D pt1 = tr.Transform(new Vector2D(x1, y1));
            Vector2D pt2 = tr.Transform(new Vector2D(x2, y2));
            Vector2D isect_pt1 = new Vector2D();
            Vector2D isect_pt2 = new Vector2D();
            BoundingArea inflated_area = bounding_area.GetWritableCopy();
            inflated_area.Inflate(pen.Width / 2f + 5f, pen.Width / 2f + 5f);
            ArrayList<KeyDat<float, PointInfo>> points = new ArrayList<KeyDat<float, PointInfo>>();
            foreach (RectangleF rect in inflated_area.Rectangles)
            {
                if (LineIntersectRectangle(pt1, pt2, rect, ref isect_pt1, ref isect_pt2))
                {
                    float dist_pt1 = (pt1 - isect_pt1).GetLength(); // *** don't need sqrt here
                    float dist_pt2 = (pt1 - isect_pt2).GetLength(); // *** don't need sqrt here
                    bool start_pt1 = dist_pt1 < dist_pt2;
                    points.Add(new KeyDat<float, PointInfo>(dist_pt1, new PointInfo(isect_pt1, start_pt1)));
                    points.Add(new KeyDat<float, PointInfo>(dist_pt2, new PointInfo(isect_pt2, !start_pt1)));
                }
            }
            points.Sort();
            int ref_count = 0;
            int start_idx = 0;
            for (int i = 0; i < points.Count; i++)
            {
                PointInfo point_info = points[i].Dat;
                if (point_info.IsStartPoint)
                {
                    ref_count++;
                }
                else
                {
                    ref_count--;
                    if (ref_count == 0)
                    {
                        gfx.DrawLine(pen, points[start_idx].Dat.Point, point_info.Point);
                        start_idx = i + 1;
                    }
                }
            }
#else
            Draw(x1, y1, x2, y2, gfx, pen, tr);
#endif
        }
        public static BoundingArea GetBoundingArea(float x1, float y1, float x2, float y2)
        {
#if !SIMPLE_BOUNDING_AREA
            if (x1 == x2 || y1 == y2) { return new BoundingArea(VisualizationUtils.CreateRectangle(x1, y1, x2, y2)); }
            float delta = Math.Abs((x2 - x1) / (y2 - y1));
            float step_max = (float)Math.Sqrt(m_max_box_area / delta + delta * m_max_box_area);
            Vector2D line = new Vector2D(x1, y1, x2, y2);
            float line_len = line.GetLength();
            if (step_max >= line_len) { return new BoundingArea(VisualizationUtils.CreateRectangle(x1, y1, x2, y2)); }
            BoundingArea bounding_area = new BoundingArea();
            int steps = (int)Math.Ceiling(line_len / step_max);
            Vector2D step_vec = line;
            step_vec.SetLength(line_len / (float)steps);
            Vector2D pt1 = new Vector2D(x1, y1);
            Vector2D pt2;
            for (int i = 0; i < steps - 1; i++)
            {
                pt2 = pt1 + step_vec;
                bounding_area.AddRectangles(VisualizationUtils.CreateRectangle(pt1.X, pt1.Y, pt2.X, pt2.Y));
                pt1 = pt2;
            }
            pt2 = new Vector2D(x2, y2);
            bounding_area.AddRectangles(VisualizationUtils.CreateRectangle(pt1.X, pt1.Y, pt2.X, pt2.Y));
            return bounding_area;
#else
            BoundingArea bounding_area = new BoundingArea();
            bounding_area.AddRectangles(VisualizationUtils.CreateRectangle(x1, y1, x2, y2));
            return bounding_area;
#endif
        }
        public static bool IsObjectAt(float pt_x, float pt_y, TransformParams tr, float x1, float y1, float x2, float y2, ref float dist)
        {
            Utils.ThrowException(tr.NotSet ? new ArgumentValueException("tr") : null);
            Vector2D pt1 = tr.Transform(new Vector2D(x1, y1));
            Vector2D pt2 = tr.Transform(new Vector2D(x2, y2));
            return VisualizationUtils.TestLineHit(new Vector2D(pt_x, pt_y), pt1, pt2, m_hit_dist, ref dist);
        }
        public float X
        {
            get { return m_x1; }
            set
            {
                m_x1 = value;
                InvalidateBoundingArea();
            }
        }
        public float Y
        {
            get { return m_y1; }
            set
            {
                m_y1 = value;
                InvalidateBoundingArea();
            }
        }
        public float X2
        {
            get { return m_x2; }
            set
            {
                m_x2 = value;
                InvalidateBoundingArea();
            }
        }
        public float Y2
        {
            get { return m_y2; }
            set
            {
                m_y2 = value;
                InvalidateBoundingArea();
            }
        }
        public static float LineHitMaxDist
        {
            get { return m_hit_dist; }
            set 
            {
                Utils.ThrowException(value < 0 ? new ArgumentOutOfRangeException("LineHitMaxDist") : null);
                m_hit_dist = value; 
            }
        }
        public static float MaxBoxArea
        {
            get { return m_max_box_area; }
            set
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("MaxBoxArea") : null);
                m_max_box_area = value;
            }
        }
        public override void Draw(Graphics gfx, TransformParams tr)
        {
            Draw(m_x1, m_y1, m_x2, m_y2, gfx, m_pen, tr); // throws ArgumentNullException, ArgumentValueException
        }
        public override void Draw(Graphics gfx, TransformParams tr, BoundingArea.ReadOnly bounding_area)
        {
            Draw(m_x1, m_y1, m_x2, m_y2, gfx, m_pen, tr, bounding_area); // throws ArgumentNullException, ArgumentValueException
        }
        public override IDrawableObject GetObjectAt(float x, float y, TransformParams tr, ref float dist)
        {
            return IsObjectAt(x, y, tr, m_x1, m_y1, m_x2, m_y2, ref dist) ? this : null; // throws ArgumentValueException
        }
        public override BoundingArea GetBoundingArea()
        {
            return GetBoundingArea(m_x1, m_y1, m_x2, m_y2);
        }
        /* .-----------------------------------------------------------------------
           |		 
           |  Class PointInfo 
           |
           '-----------------------------------------------------------------------
        */
        private class PointInfo
        {
            public Vector2D Point;
            public bool IsStartPoint;
            public PointInfo(Vector2D pt, bool is_start_pt)
            {
                Point = pt;
                IsStartPoint = is_start_pt;
            }
        }
    }
}
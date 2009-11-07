/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          VisualizationUtils.cs
 *  Version:       1.0
 *  Desc:		   Visualization-related utilities
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
       |  Static class VisualizationUtils
       |
       '-----------------------------------------------------------------------
    */
    public static class VisualizationUtils
    {
        public static bool TestLineHit(Vector2D test_pt, Vector2D line_tail, Vector2D line_head, float max_dist, ref float dist)
        {
            dist = float.MaxValue;
            if (line_tail != line_head)
            {
                Vector2D edge = line_head - line_tail;                     
                Vector2D edge_normal = edge.Normal();
                float intrsct_x = 0, intrsct_y = 0;
                float pos_a = 0, pos_b = 0;
                Vector2D.Intersect(test_pt, edge_normal, line_tail, edge, ref intrsct_x, ref intrsct_y, ref pos_a, ref pos_b);                
                if (pos_b >= 0f && pos_b <= 1f)
                {
                    Vector2D dist_vec = new Vector2D(intrsct_x, intrsct_y) - test_pt;
                    dist = dist_vec.GetLength();
                }
                dist = Math.Min((line_tail - test_pt).GetLength(), dist);
            }            
            dist = Math.Min((line_head - test_pt).GetLength(), dist);
            return dist <= max_dist;
        }

        public static RectangleF CreateRectangle(float x1, float y1, float x2, float y2)
        { 
            return new RectangleF(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x1 - x2), Math.Abs(y1 - y2));
        }

        public static bool PointInsideRect(double x, double y, RectangleF rect)
        {
            return x >= rect.X && x <= rect.X + rect.Width && y >= rect.Y && y <= rect.Y + rect.Height;
        }
    }
}
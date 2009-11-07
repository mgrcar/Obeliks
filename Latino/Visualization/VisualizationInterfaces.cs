/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          VisualizationInterfaces.cs
 *  Version:       1.0
 *  Desc:		   Visualization-related interfaces
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: May-2008
 *  Revision:      May-2008
 *
 ***************************************************************************/

using System.Drawing;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |		 
       |  Interface IDrawableObject
       |
       '-----------------------------------------------------------------------
    */
    public interface IDrawableObject
    {
        void Draw(Graphics gfx, TransformParams tr);
        void Draw(Graphics gfx, TransformParams tr, BoundingArea.ReadOnly bounding_area);
        BoundingArea GetBoundingArea(TransformParams tr);
        IDrawableObject[] GetObjectsAt(float x, float y, TransformParams tr, ref float[] dist_array);
        IDrawableObject[] GetObjectsIn(BoundingArea.ReadOnly area, TransformParams tr);
    }
}
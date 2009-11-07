/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DrawableGroup.cs
 *  Version:       1.0
 *  Desc:		   Group of drawable objects
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
       |  Class DrawableGroup
       |
       '-----------------------------------------------------------------------
    */
    public class DrawableGroup : IDrawableObject
    {
        private ArrayList<IDrawableObject> m_drawable_objects
            = new ArrayList<IDrawableObject>();
        public ArrayList<IDrawableObject> DrawableObjects
        {
            get { return m_drawable_objects; }
        }
        // *** IDrawableObject interface implementation ***
        public void Draw(Graphics gfx, TransformParams tr)
        {
            foreach (IDrawableObject drawable_object in m_drawable_objects)
            {
                drawable_object.Draw(gfx, tr);
            }
        }
        public void Draw(Graphics gfx, TransformParams tr, BoundingArea.ReadOnly bounding_area)
        {
            foreach (IDrawableObject drawable_object in m_drawable_objects)
            {
                if (drawable_object.GetBoundingArea(tr).IntersectsWith(bounding_area))
                {
                    drawable_object.Draw(gfx, tr, bounding_area);
                }
            }
        }
        public IDrawableObject[] GetObjectsAt(float x, float y, TransformParams tr, ref float[] dist_array)
        {
            ArrayList<ObjectInfo> aux = new ArrayList<ObjectInfo>();
            for (int i = m_drawable_objects.Count - 1; i >= 0; i--)
            {
                IDrawableObject[] objects_at_xy = m_drawable_objects[i].GetObjectsAt(x, y, tr, ref dist_array);
                for (int j = 0; j < objects_at_xy.Length; j++)
                {
                    aux.Add(new ObjectInfo(aux.Count, dist_array[j], objects_at_xy[j]));
                }
            }
            aux.Sort();
            IDrawableObject[] result = new IDrawableObject[aux.Count];
            dist_array = new float[aux.Count];
            int k = 0;
            foreach (ObjectInfo object_info in aux)
            {
                result[k] = object_info.DrawableObject;
                dist_array[k++] = object_info.Dist;
            }            
            return result;
        }
        public IDrawableObject[] GetObjectsIn(BoundingArea.ReadOnly bounding_area, TransformParams tr)
        {
            ArrayList<IDrawableObject> result = new ArrayList<IDrawableObject>();
            for (int i = m_drawable_objects.Count - 1; i >= 0; i--)
            {
                result.AddRange(m_drawable_objects[i].GetObjectsIn(bounding_area, tr));
            }
            return result.ToArray();
        }
        public BoundingArea GetBoundingArea(TransformParams tr)
        {
            BoundingArea bounding_area = new BoundingArea();
            foreach (IDrawableObject drawable_object in m_drawable_objects)
            {
                bounding_area.AddRectangles(drawable_object.GetBoundingArea(tr).Rectangles);
            }
            // bounding_area.OptimizeArea();
            return bounding_area;
        }
        /* .-----------------------------------------------------------------------
           |		 
           |  Class ObjectInfo
           |
           '-----------------------------------------------------------------------
        */
        private class ObjectInfo : IComparable<ObjectInfo>
        {
            public float Dist;
            public int Idx;
            public IDrawableObject DrawableObject;
            public ObjectInfo(int idx, float dist, IDrawableObject drawable_object)
            {
                Idx = idx;
                Dist = dist;
                DrawableObject = drawable_object;
            }
            // *** IComparable<ObjectInfo> interface implementation ***
            public int CompareTo(ObjectInfo other)
            {
                if (Dist < other.Dist) { return -1; }
                else if (Dist > other.Dist) { return 1; }
                else if (Idx < other.Idx) { return -1; }
                else if (Idx > other.Idx) { return 1; }
                else { return 0; }
            }
        }
    }
}

/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          TransformParams.cs
 *  Version:       1.0
 *  Desc:		   Scale-and-translate transform parameters
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
       |  Struct TransformParams
       |
       '-----------------------------------------------------------------------
    */
    public struct TransformParams
    {
        private float m_translate_x;
        private float m_translate_y;
        private float m_scale_factor;
        private static TransformParams m_identity
            = new TransformParams(0, 0, 1);
        public TransformParams(float translate_x, float translate_y, float scale_factor)
        {
            Utils.ThrowException(scale_factor <= 0 ? new ArgumentOutOfRangeException("scale_factor") : null);
            m_translate_x = translate_x;
            m_translate_y = translate_y;
            m_scale_factor = scale_factor;
        }
        public TransformParams(float scale_factor) : this(0, 0, scale_factor) // throws ArgumentOutOfRangeException
        {
        }
        public TransformParams(float translate_x, float translate_y) : this(translate_x, translate_y, 1)
        {
        }
        public float TranslateX
        {
            get { return m_translate_x; }
            set { m_translate_x = value; }
        }
        public float TranslateY
        {
            get { return m_translate_y; }
            set { m_translate_y = value; }
        }
        public float ScaleFactor
        {
            get { return m_scale_factor; }
            set
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("ScaleFactor") : null);
                m_scale_factor = value;
            }
        }
        public bool NotSet
        {
            get { return m_scale_factor == 0; }
        }
        public TransformParams Inverse
        {
            get
            {
                Utils.ThrowException(m_scale_factor == 0 ? new InvalidOperationException() : null);
                return new TransformParams(-m_translate_x, -m_translate_y, 1f / m_scale_factor);
            }
        }
        public static TransformParams Identity
        {
            get { return m_identity; }
        }
        public RectangleF Transform(RectangleF rect)
        {
            // scale
            rect.X *= m_scale_factor;
            rect.Y *= m_scale_factor;
            rect.Width *= m_scale_factor;
            rect.Height *= m_scale_factor;
            // translate
            rect.X += m_translate_x;
            rect.Y += m_translate_y;
            return rect;
        }
        public Vector2D Transform(Vector2D vec)
        {
            // scale
            vec.X *= m_scale_factor;
            vec.Y *= m_scale_factor;
            // translate
            vec.X += m_translate_x;
            vec.Y += m_translate_y;
            return vec;
        }
        public float Transform(float len)
        {
            return len * m_scale_factor;
        }
    }
}
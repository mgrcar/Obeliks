/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          Vector2D.cs
 *  Version:       1.0
 *  Desc:		   Geometric vector data structure
 *  Author:		   Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: May-2009
 *  Revision:      May-2009
 *
 ***************************************************************************/

using System;
using System.Drawing;

namespace Latino
{
    /* .-----------------------------------------------------------------------
       |
       |  Struct Vector2D
       |
       '-----------------------------------------------------------------------
    */
    public struct Vector2D : IEquatable<Vector2D>, ISerializable
    {
        private float m_x;
        private float m_y;
        public Vector2D(BinarySerializer reader) : this()
        {
            Load(reader); // throws ArgumentNullException
        }
        public Vector2D(float x, float y)
        {
            m_x = x;
            m_y = y;
        }
        public Vector2D(PointF pt) : this(pt.X, pt.Y) 
        {
        }
        public Vector2D(float x, float y, float x_2, float y_2) : this(x_2 - x, y_2 - y) 
        {
        }
        public Vector2D(Vector2D vec_1, Vector2D vec_2) : this(vec_2.X - vec_1.X, vec_2.Y - vec_1.Y) 
        {
        }
        public Vector2D(PointF pt_1, PointF pt_2) : this(pt_2.X - pt_1.X, pt_2.Y - pt_1.Y)
        {
        }
        public float X
        {
            get { return m_x; }
            set { m_x = value; }
        }
        public float Y
        {
            get { return m_y; }
            set { m_y = value; }
        }
        public void Set(float x, float y)
        {
            m_x = x;
            m_y = y;
        }
        public float GetLength()
        {
            return (float)Math.Sqrt(m_x * m_x + m_y * m_y);
        }
        public void SetLength(float new_len)
        {
            float len = GetLength(); 
            Utils.ThrowException(len == 0 ? new InvalidOperationException() : null);
            m_x = m_x / len * new_len;
            m_y = m_y / len * new_len;
        }
        public Vector2D UnitVector()
        {
            Vector2D unit_vec = this;
            unit_vec.SetLength(1); // throws InvalidOperationException
            return unit_vec;
        }
        public Vector2D Normal() // returns an orthogonal clockwise-oriented vector of the same length
        {
            Utils.ThrowException(GetLength() == 0 ? new InvalidOperationException() : null);
            return new Vector2D(m_y, -m_x);
        }
        public float GetAngle() // returns a value within [0, 2PI)
        {
            Vector2D unit_vec = UnitVector(); // throws InvalidOperationException
            float angle;
            if (m_x >= 0) { angle = (float)Math.Asin(unit_vec.Y); }
            else { angle = (float)(Math.PI - Math.Asin(unit_vec.Y)); }
            if (angle < 0) { angle += (float)(2.0 * Math.PI); }
            return angle;
        }
        public static Vector2D GetRndVec(float len, Random rnd)
        {
            Utils.ThrowException(len < 0 ? new ArgumentOutOfRangeException("len") : null);
            Utils.ThrowException(rnd == null ? new ArgumentNullException("rnd") : null);
            Vector2D rnd_vec;
            do
            {
                rnd_vec = new Vector2D((float)rnd.NextDouble(), (float)rnd.NextDouble());
            }
            while (rnd_vec.GetLength() == 0);
            rnd_vec.SetLength(len);
            return rnd_vec;
        }
        public static Vector2D GetFromAngleAndLength(float angle, float len)
        {
            Utils.ThrowException(len < 0 ? new ArgumentOutOfRangeException("length") : null);
            return new Vector2D((float)Math.Cos(angle) * len, (float)Math.Sin(angle) * len); 
        }
        public static bool Intersect(Vector2D A, Vector2D a, Vector2D B, Vector2D b, ref float x, ref float y, ref bool seg_intersect)
        {
            Utils.ThrowException(a.GetLength() == 0 ? new ArgumentOutOfRangeException("a") : null);
            Utils.ThrowException(b.GetLength() == 0 ? new ArgumentOutOfRangeException("b") : null);
            float x_1 = A.X;
            float y_1 = A.Y;
            float x_2 = A.X + a.X;
            float y_2 = A.Y + a.Y;
            float x_3 = B.X;
            float y_3 = B.Y;
            float x_4 = B.X + b.X;
            float y_4 = B.Y + b.Y;
            float div = (y_4 - y_3) * (x_2 - x_1) - (x_4 - x_3) * (y_2 - y_1);
            if (div == 0) { return false; } // the two vectors are parallel or coincident
            float ua = ((x_4 - x_3) * (y_1 - y_3) - (y_4 - y_3) * (x_1 - x_3)) / div;
            float ub = ((x_2 - x_1) * (y_1 - y_3) - (y_2 - y_1) * (x_1 - x_3)) / div;
            x = x_1 + ua * (x_2 - x_1);
            y = y_1 + ua * (y_2 - y_1);
            seg_intersect = ua >= 0.0 && ua <= 1.0 && ub >= 0.0 && ub <= 1.0;
            return true;
        }
        public static bool Intersect(Vector2D A, Vector2D a, Vector2D B, Vector2D b, ref float x, ref float y, ref float ua, ref float ub)
        {
            Utils.ThrowException(a.GetLength() == 0 ? new ArgumentOutOfRangeException("a") : null);
            Utils.ThrowException(b.GetLength() == 0 ? new ArgumentOutOfRangeException("b") : null);
            float x_1 = A.X;
            float y_1 = A.Y;
            float x_2 = A.X + a.X;
            float y_2 = A.Y + a.Y;
            float x_3 = B.X;
            float y_3 = B.Y;
            float x_4 = B.X + b.X;
            float y_4 = B.Y + b.Y;
            float div = (y_4 - y_3) * (x_2 - x_1) - (x_4 - x_3) * (y_2 - y_1);
            if (div == 0) { return false; } // the two vectors are parallel or coincident
            ua = ((x_4 - x_3) * (y_1 - y_3) - (y_4 - y_3) * (x_1 - x_3)) / div;
            ub = ((x_2 - x_1) * (y_1 - y_3) - (y_2 - y_1) * (x_1 - x_3)) / div;
            x = x_1 + ua * (x_2 - x_1);
            y = y_1 + ua * (y_2 - y_1);
            return true;
        }
        public static float DotProduct(Vector2D vec_1, Vector2D vec_2)
        {
            return vec_1.X * vec_2.X + vec_1.Y * vec_2.Y;
        }
        public static Vector2D operator +(Vector2D vec_1, Vector2D vec_2)
        {
            return new Vector2D(vec_1.X + vec_2.X, vec_1.Y + vec_2.Y);
        }
        public static Vector2D operator -(Vector2D vec_1, Vector2D vec_2)
        {
            return new Vector2D(vec_1.X - vec_2.X, vec_1.Y - vec_2.Y); 
        }
        public static Vector2D operator *(Vector2D vec, float scalar)
        {
            return new Vector2D(vec.X * scalar, vec.Y * scalar); 
        }
        public static Vector2D operator *(float scalar, Vector2D vec)
        {
            return vec * scalar; 
        }
        public static Vector2D operator /(Vector2D vec, float scalar)
        {
            Utils.ThrowException(scalar == 0 ? new ArgumentOutOfRangeException("scalar") : null);
            return new Vector2D(vec.X / scalar, vec.Y / scalar); 
        }
        public static Vector2D operator -(Vector2D vec)
        {
            return new Vector2D(-vec.X, -vec.Y);
        }
        public static bool operator ==(Vector2D vec_1, Vector2D vec_2)
        {
            return vec_1.m_x == vec_2.m_x && vec_1.m_y == vec_2.m_y;
        }
        public static bool operator !=(Vector2D vec_1, Vector2D vec_2)
        {
            return !(vec_1 == vec_2);
        }
        public static implicit operator PointF(Vector2D vec)
        {
            return new PointF(vec.m_x, vec.m_y);
        }
        public static implicit operator Vector2D(PointF pt)
        {
            return new Vector2D(pt.X, pt.Y);
        }
        public override string ToString()
        {
            return string.Format("( {0} {1} )", m_x, m_y);
        }
        public override int GetHashCode()
        {
            return m_x.GetHashCode() ^ m_y.GetHashCode();
        }
        // *** IEquatable<Vector2D> interface implementation ***
        public bool Equals(Vector2D other)
        {
            return m_x == other.m_x && m_y == other.m_y;
        }
        public override bool Equals(object obj)
        {
            Utils.ThrowException(!(obj is Vector2D) ? new ArgumentTypeException("obj") : null);
            return Equals((Vector2D)obj);
        }
        // *** ISerializable interface implementation ***
        public void Save(BinarySerializer writer)
        {
            Utils.ThrowException(writer == null ? new ArgumentNullException("writer") : null);
            writer.WriteFloat(m_x);
            writer.WriteFloat(m_y);
        }
        public void Load(BinarySerializer reader)
        {
            Utils.ThrowException(reader == null ? new ArgumentNullException("reader") : null);
            m_x = reader.ReadFloat();
            m_y = reader.ReadFloat();
        }
    }
}

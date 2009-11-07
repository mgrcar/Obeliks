/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DrawableObjectEventArgs.cs
 *  Version:       1.0
 *  Desc:		   Drawable object event arguments class
 *  Author:        Miha Grcar
 *  Created on:    Jun-2009
 *  Last modified: Jun-2009
 *  Revision:      Jun-2009
 *
 ***************************************************************************/

using System.Windows.Forms;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |
       |  Class DrawableObjectEventArgs
       |
       '-----------------------------------------------------------------------
    */
    public class DrawableObjectEventArgs : MouseEventArgs
    {
        private IDrawableObject[] m_drawable_objects
            = null;
        new public static readonly DrawableObjectEventArgs Empty
            = new DrawableObjectEventArgs(MouseButtons.None, 0, 0, 0, 0);
        private ContextMenuStrip m_context_menu
            = null;
        private string m_tool_tip_text
            = null;
        public DrawableObjectEventArgs(MouseEventArgs mouse_args) : base(mouse_args.Button, mouse_args.Clicks, mouse_args.X, mouse_args.Y, mouse_args.Delta)
        { 
        }
        public DrawableObjectEventArgs(MouseEventArgs mouse_args, IDrawableObject[] drawable_objects) : this(mouse_args)
        {
            m_drawable_objects = drawable_objects;
        }
        public DrawableObjectEventArgs(MouseButtons buttons, int clicks, int x, int y, int delta) : base(buttons, clicks, x, y, delta)
        {
        }
        public DrawableObjectEventArgs(MouseButtons buttons, int clicks, int x, int y, int delta, IDrawableObject[] drawable_objects) : base(buttons, clicks, x, y, delta) 
        {
            m_drawable_objects = drawable_objects;
        }
        public DrawableObjectEventArgs(IDrawableObject[] drawable_objects) : this(MouseButtons.None, 0, 0, 0, 0, drawable_objects)
        {
        }
        public IDrawableObject[] DrawableObjects
        {
            get { return m_drawable_objects; }
        }
        public ContextMenuStrip ContextMenuStrip
        {
            get { return m_context_menu; }
            set { m_context_menu = value; }
        }
        public string ToolTipText
        {
            get { return m_tool_tip_text; }
            set { m_tool_tip_text = value; }
        }
    }
}

/*==========================================================================;
 *
 *  This file is part of LATINO. See http://latino.sf.net
 *
 *  File:          DrawableObjectViewer.cs
 *  Version:       1.0
 *  Desc:		   Drawable object viewer
 *  Author:        Miha Grcar
 *  Created on:    Mar-2008
 *  Last modified: Jun-2009
 *  Revision:      Jun-2009
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Drawing.Text;

namespace Latino.Visualization
{
    /* .-----------------------------------------------------------------------
       |
       |  Control DrawableObjectViewer
       |
       '-----------------------------------------------------------------------
    */
    public partial class DrawableObjectViewer : UserControl
    {   
        private const int RENDER_LAYER = 1;
        private const int MAIN_LAYER   = 2;

        private float m_scale_factor
            = 1;
        
        private Color m_canvas_color
            = Color.White;
        private Size m_canvas_size
            = new Size(800, 600);

        private Dictionary<int, BitmapInfo> m_bmp_cache
            = new Dictionary<int, BitmapInfo>();

        private IDrawableObject m_drawable_object
            = null;

        private EditableBitmap canvas_view
            = null;

        private IDrawableObject[] m_target_objs
            = new IDrawableObject[] { };
        private Set<IDrawableObject> m_target_obj_set
            = new Set<IDrawableObject>();

        private ContextMenuStrip m_canvas_menu
            = null;
        
        private ToolTip m_drawable_object_tip
            = new ToolTip();
        
        [DllImport("user32", CharSet = CharSet.Auto)]
        private extern static IntPtr SendMessage(IntPtr h_wnd, int msg, int w_param, IntPtr l_param);

        private static void SetupGraphics(Graphics gfx)
        {
            gfx.SmoothingMode = SmoothingMode.AntiAlias;
            gfx.TextRenderingHint = TextRenderingHint.AntiAlias;        
        }

        private BitmapInfo PrepareBitmap(int id, int width, int height)
        {
            if (!m_bmp_cache.ContainsKey(id))
            {
                EditableBitmap bmp = new EditableBitmap(width, height, PixelFormat.Format24bppRgb);
                Graphics gfx = Graphics.FromImage(bmp.Bitmap);
                SetupGraphics(gfx);                               
                gfx.Clear(m_canvas_color); 
                BitmapInfo bmp_info = new BitmapInfo(bmp, gfx);
                m_bmp_cache.Add(id, bmp_info);
                return bmp_info;
            }
            else
            {
                BitmapInfo bmp_info = m_bmp_cache[id];
                if (bmp_info.Bitmap.Width >= width && bmp_info.Bitmap.Height >= height) 
                {
                    bmp_info.Graphics.Clip = new Region(new Rectangle(0, 0, width, height)); 
                    bmp_info.Graphics.Clear(m_canvas_color); 
                    return bmp_info;
                } 
                // remove old bitmap 
                bmp_info.Dispose();
                m_bmp_cache.Remove(id);
                // create new bitmap
                EditableBitmap bmp = new EditableBitmap(width, height, PixelFormat.Format24bppRgb);
                Graphics gfx = Graphics.FromImage(bmp.Bitmap);
                SetupGraphics(gfx);
                gfx.Clear(m_canvas_color); 
                bmp_info = new BitmapInfo(bmp, gfx);
                m_bmp_cache.Add(id, bmp_info);
                return bmp_info;
            }
        }

        private void SetupCanvas(int width, int height)
        {
            if (canvas_view != null) { canvas_view.Dispose(); }
            BitmapInfo main_layer = m_bmp_cache[MAIN_LAYER];
            canvas_view = main_layer.EditableBitmap.CreateView(new Rectangle(0, 0, width, height));
        }

        private Rectangle GetEnclosingRect(RectangleF rect)
        {
            return new Rectangle((int)Math.Floor(rect.X), (int)Math.Floor(rect.Y), (int)Math.Ceiling(rect.Width + 1f), (int)Math.Ceiling(rect.Height + 1f));
        }
        
        public DrawableObjectViewer()
        {
            InitializeComponent();
            if (picBoxCanvas.Image == null)
            {
                Draw();
            }
        }

        [Browsable(false)]        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDrawableObject DrawableObject
        {
            get { return m_drawable_object; }
            set 
            { 
                m_drawable_object = value;
                m_target_obj_set.Clear();
                m_target_objs = new IDrawableObject[] { };
                Draw();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Size), "800, 600")]
        public Size CanvasSize
        {
            get { return m_canvas_size; }
            set 
            {
                //Utils.ThrowException(m_drawable_object != null ? new InvalidOperationException() : null);
                m_canvas_size = value;
                Draw();
            }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Color), "White")]
        public Color CanvasColor
        {
            get { return m_canvas_color; }
            set 
            {
                //Utils.ThrowException(m_drawable_object != null ? new InvalidOperationException() : null);
                m_canvas_color = value;
                Draw();
            }
        }

        [Category("Appearance")]
        [DefaultValue(1f)]
        public float ScaleFactor
        {
            get { return m_scale_factor; }
            set
            {
                Utils.ThrowException(value <= 0 ? new ArgumentOutOfRangeException("ScaleFactor") : null);
                m_scale_factor = value;
                Draw();
            }
        }

        [Category("Layout")]
        [DefaultValue(true)]
        new public bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        [Category("Appearance")]
        [DefaultValue(typeof(Cursor), "Default")]
        public Cursor CanvasCursor
        {
            get { return picBoxCanvas.Cursor; }
            set { picBoxCanvas.Cursor = value; }
        }

        [Category("Behavior")]
        [DefaultValue(null)]
        public ContextMenuStrip CanvasContextMenuStrip
        {
            get { return m_canvas_menu; }
            set { m_canvas_menu = value; }
        }

        [Category("Behavior")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ToolTip DrawableObjectToolTip
        {
            get { return m_drawable_object_tip; }            
        }

        public void ExtendBoundingArea(BoundingArea bounding_area, params IDrawableObject[] drawable_objects)
        {
            Utils.ThrowException(bounding_area == null ? new ArgumentNullException("bounding_area") : null);
            Utils.ThrowException(drawable_objects == null ? new ArgumentNullException("drawable_objects") : null);
            TransformParams tr = new TransformParams(0, 0, m_scale_factor);
            foreach (IDrawableObject drawable_object in drawable_objects)
            {
                bounding_area.AddRectangles(drawable_object.GetBoundingArea(tr).Rectangles);
            }
        }

        public void Draw()
        {
            const int WM_SETREDRAW = 0x000B;
            SendMessage(Handle, WM_SETREDRAW, 0, IntPtr.Zero); // supress redrawing
            TransformParams tr = new TransformParams(0, 0, m_scale_factor);
            int width = (int)Math.Ceiling(tr.Transform(m_canvas_size.Width));
            int height = (int)Math.Ceiling(tr.Transform(m_canvas_size.Height));
            if (m_drawable_object == null)
            {
                PrepareBitmap(MAIN_LAYER, width, height);
                SetupCanvas(width, height);
                picBoxCanvas.Image = canvas_view.Bitmap;
            }
            else
            {
                BitmapInfo main_layer = PrepareBitmap(MAIN_LAYER, width, height);
                m_drawable_object.Draw(main_layer.Graphics, tr);
                SetupCanvas(width, height);
                picBoxCanvas.Image = canvas_view.Bitmap;
            }
            SendMessage(Handle, WM_SETREDRAW, 1, IntPtr.Zero); // resume redrawing
            AutoScroll = true; // update scrollbars
            Refresh(); // repaint control
        }

        public void Draw(params IDrawableObject[] drawable_objects)
        {
            Draw(new BoundingArea(), drawable_objects);
        }

        // ****** Debugging ******
        private int m_draw_count
            = 0;
        private TimeSpan m_draw_time
            = TimeSpan.Zero;
        private ArrayList<double> m_draw_info
            = new ArrayList<double>();

        private void FpsInfo_Click(object sender, EventArgs e)
        {
            double stdev = 0;
            double avg = (double)m_draw_time.TotalMilliseconds / (double)m_draw_count;
            foreach (double val in m_draw_info)
            {
                stdev += (val - avg) * (val - avg);
            }
            stdev = Math.Sqrt(stdev / (double)m_draw_info.Count);
            m_draw_count = 0;
            m_draw_time = TimeSpan.Zero;
            m_draw_info.Clear();
            FpsInfo.Text = string.Format("{0:0.00} ({1:0.00}) ms / draw", avg, stdev);           
        }
        // ***********************

        public void Draw(BoundingArea.ReadOnly bounding_area, params IDrawableObject[] drawable_objects)
        {            
            Utils.ThrowException(bounding_area == null ? new ArgumentNullException("bounding_area") : null);
            Utils.ThrowException(drawable_objects == null ? new ArgumentNullException("drawable_objects") : null);
            DateTime start_time = DateTime.Now;
            BoundingArea extended_area = bounding_area.GetWritableCopy();
            ExtendBoundingArea(extended_area, drawable_objects);
#if !NO_BB_SIMPLIFICATION
            extended_area.Optimize();
#endif
            TransformParams tr = new TransformParams(0, 0, m_scale_factor);
            Set<IDrawableObject> outdated_objects = new Set<IDrawableObject>(drawable_objects);
            drawable_objects = m_drawable_object.GetObjectsIn(extended_area, tr);

            Rectangle enclosing_rect = GetEnclosingRect(extended_area.BoundingBox);

            BitmapInfo render_layer = PrepareBitmap(RENDER_LAYER, enclosing_rect.Width, enclosing_rect.Height);
            TransformParams render_tr = new TransformParams(-enclosing_rect.X, -enclosing_rect.Y, m_scale_factor);
            
            BoundingArea extended_area_tr = extended_area.Clone();
            extended_area_tr.Transform(new TransformParams(-enclosing_rect.X, -enclosing_rect.Y, 1));

            for (int i = drawable_objects.Length - 1; i >= 0; i--)
            {
                if (outdated_objects.Contains(drawable_objects[i]))
                {
                    drawable_objects[i].Draw(render_layer.Graphics, render_tr);
                }
                else
                {
                    drawable_objects[i].Draw(render_layer.Graphics, render_tr, extended_area_tr);
                }
            }
            BitmapInfo main_layer = m_bmp_cache[MAIN_LAYER];
            Graphics canvas_gfx = Graphics.FromHwnd(picBoxCanvas.Handle);
            foreach (RectangleF rect in extended_area.Rectangles)
            {
                Rectangle view_area = GetEnclosingRect(rect);
                view_area.X -= enclosing_rect.X;
                view_area.Y -= enclosing_rect.Y;
                view_area.Intersect(new Rectangle(0, 0, enclosing_rect.Width, enclosing_rect.Height));
                EditableBitmap view = render_layer.EditableBitmap.CreateView(view_area);
                main_layer.Graphics.DrawImageUnscaled(view.Bitmap, view_area.X + enclosing_rect.X, view_area.Y + enclosing_rect.Y);
                // clipping to visible area?!?
                canvas_gfx.DrawImageUnscaled(view.Bitmap, view_area.X + enclosing_rect.X, view_area.Y + enclosing_rect.Y);
                //view_on_view.Dispose();
                view.Dispose();
            }
            canvas_gfx.Dispose();

            TimeSpan draw_time = DateTime.Now - start_time;
            m_draw_time += draw_time;
            m_draw_count++;

            FpsInfo.Text = string.Format("{0:0.00} ms / draw", (double)m_draw_time.TotalMilliseconds / (double)m_draw_count);
            m_draw_info.Add(draw_time.TotalMilliseconds);
            FpsInfo.Refresh();
        }

        public delegate void DrawableObjectEventHandler(object sender, DrawableObjectEventArgs args);

        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectClick
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectDoubleClick
            = null;

        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseEnter
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseLeave
            = null;

        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseDown
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseUp
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseMove
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectMouseHover
            = null;

        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectContextMenuStripRequest
            = null;
        [Category("DrawableObject Event")]
        public event DrawableObjectEventHandler DrawableObjectToolTipRequest
            = null;

        [Category("Canvas Event")]
        public event MouseEventHandler CanvasClick
            = null;
        [Category("Canvas Event")]
        public event MouseEventHandler CanvasDoubleClick
            = null;

        //[Category("Canvas Event")]
        //public event (Mouse?)EventHandler CanvasMouseEnter
        //    = null;
        //[Category("Canvas Event")]
        //public event EventHandler CanvasMouseLeave
        //    = null;

        [Category("Canvas Event")]
        public event MouseEventHandler CanvasMouseDown
            = null;
        [Category("Canvas Event")]
        public event MouseEventHandler CanvasMouseUp
            = null;
        [Category("Canvas Event")]
        public event MouseEventHandler CanvasMouseMove
            = null;
        [Category("Canvas Event")]
        public event EventHandler CanvasMouseHover
            = null;

        private void DrawableObjectViewer_MouseMove(object sender, MouseEventArgs args)
        {
            const int WM_ACTIVATE = 0x0006;
            if (m_drawable_object != null)
            {
                TransformParams tr = new TransformParams(0, 0, m_scale_factor);
                float[] dist_array = null;
                IDrawableObject[] new_target_objs = m_drawable_object.GetObjectsAt(args.X, args.Y, tr, ref dist_array);
                Set<IDrawableObject> new_target_obj_set = new Set<IDrawableObject>(new_target_objs);
                ArrayList<IDrawableObject> exit_objs = new ArrayList<IDrawableObject>();
                ArrayList<IDrawableObject> enter_objs = new ArrayList<IDrawableObject>();
                if (DrawableObjectMouseLeave != null || DrawableObjectToolTipRequest != null)
                {                    
                    foreach (IDrawableObject obj in m_target_objs)
                    {
                        if (!new_target_obj_set.Contains(obj))
                        {
                            exit_objs.Add(obj);
                        }
                    }
                }
                if (DrawableObjectMouseEnter != null || DrawableObjectToolTipRequest != null)
                {                   
                    foreach (IDrawableObject obj in new_target_objs)
                    {
                        if (!m_target_obj_set.Contains(obj))
                        {
                            enter_objs.Add(obj);
                        }
                    }
                }
                if (DrawableObjectMouseLeave != null && exit_objs.Count > 0)
                {
                    DrawableObjectMouseLeave(this, new DrawableObjectEventArgs(args, exit_objs.ToArray()));
                }
                if (DrawableObjectMouseEnter != null && enter_objs.Count > 0)
                {
                    DrawableObjectMouseEnter(this, new DrawableObjectEventArgs(args, enter_objs.ToArray()));
                }
                if (DrawableObjectToolTipRequest != null && (enter_objs.Count > 0 || exit_objs.Count > 0))
                {
                    if (new_target_objs.Length > 0)
                    {
                        m_drawable_object_tip.SetToolTip(picBoxCanvas, null);
                        DrawableObjectEventArgs event_args = new DrawableObjectEventArgs(args, new_target_objs);
                        DrawableObjectToolTipRequest(this, event_args);
                        m_drawable_object_tip.SetToolTip(picBoxCanvas, event_args.ToolTipText);
                        SendMessage(picBoxCanvas.Handle, WM_ACTIVATE, 0, IntPtr.Zero); // *** I'm not sure why this is required but it is :)
                    }
                    else
                    {
                        m_drawable_object_tip.SetToolTip(picBoxCanvas, null);
                    }
                }
                m_target_obj_set = new_target_obj_set;
                m_target_objs = new_target_objs;
            }
            if (m_target_objs.Length == 0)
            {
                if (CanvasMouseMove != null) 
                { 
                    CanvasMouseMove(this, args); 
                }
            }
            else 
            {
                if (DrawableObjectMouseMove != null) 
                { 
                    DrawableObjectMouseMove(this, new DrawableObjectEventArgs(args, m_target_objs)); 
                }
            }
        }

        private void DrawableObjectViewer_MouseHover(object sender, EventArgs args)
        {
            if (m_target_objs.Length > 0)
            {
                if (DrawableObjectMouseHover != null)
                {
                    DrawableObjectMouseHover(this, new DrawableObjectEventArgs(m_target_objs));
                }
            }
            else
            {
                if (CanvasMouseHover != null)
                {
                    CanvasMouseHover(this, EventArgs.Empty);
                }
            }
        }

        private void DrawableObjectViewer_MouseDown(object sender, MouseEventArgs args)
        {
            Focus();
            if (m_target_objs.Length > 0)
            {
                if (DrawableObjectMouseDown != null)
                {
                    DrawableObjectMouseDown(this, new DrawableObjectEventArgs(args, m_target_objs));
                }
            }
            else
            {
                if (CanvasMouseDown != null)
                {
                    CanvasMouseDown(this, args);
                }
            }
        }

        private void DrawableObjectViewer_MouseUp(object sender, MouseEventArgs args)
        {
            if (m_target_objs.Length > 0)
            {
                if (DrawableObjectMouseUp != null)
                {
                    DrawableObjectMouseUp(this, new DrawableObjectEventArgs(args, m_target_objs));
                }
                if ((args.Button & MouseButtons.Right) == MouseButtons.Right && DrawableObjectContextMenuStripRequest != null)
                {
                    DrawableObjectEventArgs event_args = new DrawableObjectEventArgs(args, m_target_objs);
                    DrawableObjectContextMenuStripRequest(this, event_args);
                    if (event_args.ContextMenuStrip != null)
                    {
                        event_args.ContextMenuStrip.Show(picBoxCanvas, args.X, args.Y);
                    }
                }
            }
            else
            {
                if (CanvasMouseUp != null)
                {
                    CanvasMouseUp(this, args);
                }
                if ((args.Button & MouseButtons.Right) == MouseButtons.Right && m_canvas_menu != null)
                {
                    m_canvas_menu.Show(picBoxCanvas, args.X, args.Y);
                }
            }
        }

        private void DrawableObjectViewer_MouseClick(object sender, MouseEventArgs args)
        {
            if (DrawableObjectClick != null && m_target_objs.Length > 0)
            {
                DrawableObjectClick(this, new DrawableObjectEventArgs(args, m_target_objs));
            }
            if (CanvasClick != null && m_target_objs.Length == 0)
            {
                CanvasClick(this, args);
            }
        }

        private void DrawableObjectViewer_MouseDoubleClick(object sender, MouseEventArgs args)
        {
            if (DrawableObjectDoubleClick != null && m_target_objs.Length > 0)
            {
                DrawableObjectDoubleClick(this, new DrawableObjectEventArgs(args, m_target_objs));
            }
            if (CanvasDoubleClick != null && m_target_objs.Length == 0)
            {
                CanvasDoubleClick(this, args);
            }
        }

        private void DrawableObjectViewer_MouseLeave(object sender, EventArgs args)
        {            
            if (m_target_obj_set.Count > 0)
            {
                if (DrawableObjectMouseLeave != null)
                {
                    DrawableObjectMouseLeave(this, new DrawableObjectEventArgs(m_target_objs)); 
                }
                m_target_obj_set.Clear();
                m_target_objs = new IDrawableObject[] { };
                m_drawable_object_tip.SetToolTip(picBoxCanvas, null);
            }
        }

        /* .-----------------------------------------------------------------------
           |
           |  Class BitmapInfo
           |
           '-----------------------------------------------------------------------
        */
        private class BitmapInfo : IDisposable
        {
            public EditableBitmap EditableBitmap
                = null;
            public Graphics Graphics
                = null;
            public BitmapInfo(EditableBitmap bmp, Graphics gfx)
            {
                EditableBitmap = bmp;
                Graphics = gfx;
            }
            public Bitmap Bitmap
            {
                get { return EditableBitmap.Bitmap; }
            }
            // *** IDisposable interface implementation ***
            public void Dispose()
            {
                if (Graphics != null) { Graphics.Dispose(); }
                if (EditableBitmap != null) { EditableBitmap.Dispose(); }
            }
        }
    }
}
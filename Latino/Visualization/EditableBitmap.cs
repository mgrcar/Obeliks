using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Latino.Visualization
{
    public class EditableBitmap : IDisposable
    {
        Bitmap bitmap;
        int stride;
        int pixelFormatSize;
        
        SharedPinnedByteArray byteArray;

        /// <summary>
        /// Gets the pixel format size in bytes (not bits, as with Image.GetPixelFormatSize()).
        /// </summary>
        public int PixelFormatSize
        {
            get { return pixelFormatSize; }
        }

        /// <summary>
        /// Gets the stride of the bitmap.
        /// </summary>
        public int Stride
        {
            get { return stride; }
        }

        /// <summary>
        /// Gets the underlying <see cref="System.Drawing.Bitmap"/>
        /// that this EditableBitmap wraps.
        /// </summary>
        public Bitmap Bitmap
        {
            get { return bitmap; }
            set { bitmap = value; }
        }

        /// <summary>
        /// Gets an array that contains the bitmap bit buffer.
        /// </summary>
        public byte[] Bits
        {
            get { return byteArray.bits; }
        }

        private EditableBitmap owner;

        /// <summary>
        /// The <see cref="EditableBitmap"/> that this <see cref="EditableBitmap"/> is a view on.
        /// This property's value will be null if this EditableBitmap is not a view on another 
        /// <see cref="EditableBitmap"/>.
        /// </summary>
        public EditableBitmap Owner
        {
            get { return owner; }
        }


        /// <summary>
        /// Gets a safe pointer to the buffer containing the bitmap bits.
        /// </summary>
        public IntPtr BitPtr
        {
            get
            {
                return byteArray.bitPtr;
            }
        }

        /// <summary>
        /// Creates a new EditableBitmap with the specified pixel format, 
        /// and copies the bitmap passed in onto the buffer.
        /// </summary>
        /// <param name="source">The bitmap to copy from.</param>
        /// <param name="format">The PixelFormat for the new bitmap.</param>
        public EditableBitmap(Bitmap source, PixelFormat format)
            : this(source.Width, source.Height, format)
        {
            //NOTE: This ONLY preserves the first frame of the image.
            //It does NOT copy EXIF properties, multiple frames, etc.
            //In places where preserving them is necessary, it must 
            //be done manually.
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImageUnscaledAndClipped(source, new Rectangle(0, 0, source.Width, source.Height));
            g.Dispose();
        }

        /// <summary>
        /// Creates a new EditableBitmap with the specified pixel format and size, 
        /// and copies the bitmap passed in onto the buffer. The source bitmap is stretched to 
        /// fit the new size.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="format"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public EditableBitmap(Bitmap source, PixelFormat format, int newWidth, int newHeight)
            : this(newWidth, newHeight, format)
        {
            //NOTE: This ONLY preserves the first frame of the image.
            //It does NOT copy EXIF properties, multiple frames, etc.
            //In places where preserving them is necessary, it must 
            //be done manually.
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawImage(source, 0, 0, newWidth,newHeight);
            g.Dispose();
        }

        /// <summary>
        /// Creates a new EditableBitmap containing a copy of the specified source bitmap.
        /// </summary>
        /// <param name="source"></param>
        public EditableBitmap(Bitmap source) : this(source,source.PixelFormat)
        {

        }

        /// <summary>
        /// Creates a new, blank EditableBitmap with the specified width, height, and pixel format.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        public EditableBitmap(int width, int height, PixelFormat format)
        {
            pixelFormatSize = Image.GetPixelFormatSize(format) / 8;
            stride = width * pixelFormatSize;
            int padding=(stride % 4);
            stride += padding==0?0:4 - padding;//pad out to multiple of 4
            byteArray=new SharedPinnedByteArray(stride * height);
            bitmap = new Bitmap(width, height, stride, format, byteArray.bitPtr);
        }

        #region View Support

        /// <summary>
        /// Creates an <see cref="EditableBitmap"/> as a view on a section of an existing <see cref="EditableBitmap"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="viewArea"></param>
        protected EditableBitmap(EditableBitmap source, Rectangle viewArea)
        {
            owner=source;
            pixelFormatSize=source.pixelFormatSize;
            byteArray=source.byteArray;
            byteArray.AddReference();
            stride = source.stride;
            
            try
            {
                startOffset=source.startOffset+(stride*viewArea.Y)+(viewArea.X*pixelFormatSize);
                bitmap = new Bitmap(viewArea.Width, viewArea.Height, stride, source.Bitmap.PixelFormat, 
                    (IntPtr)(((int)byteArray.bitPtr)+startOffset));
            }
            finally
            {   
                if(bitmap==null)
                    byteArray.ReleaseReference();
            }

        }
	
        /// <summary>
        /// Creates an <see cref="EditableBitmap"/> as a view on a section of an existing <see cref="EditableBitmap"/>.
        /// </summary>
        /// <param name="viewArea">The area that should form the bounds of the view.</param>
        public EditableBitmap CreateView(Rectangle viewArea)
        {
            if(disposed)
                throw new ObjectDisposedException("this");
            return new EditableBitmap(this, viewArea);
        }

        private int startOffset;

        /// <summary>
        /// If this <see cref="EditableBitmap"/> is a view on another <see cref="EditableBitmap"/> instance,
        /// this property gets the index where the pixels that are within the view's pixel area start.
        /// </summary>
        public int StartOffset
        {
            get { return startOffset; }
        }

        #endregion


        private bool disposed;

        public bool Disposed
        {
            get { return disposed; }
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        protected void Dispose(bool disposing)
        {
            if(disposed)
                return;
            
            bitmap.Dispose();
            byteArray.ReleaseReference();
            disposed = true;

            //Set managed object refs to null if explicitly disposing, so that they can be cleaned up by the GC.
            if (disposing)
            {
              owner=null;
              bitmap=null;
            }
        }

        ~EditableBitmap()
        {
            Dispose(false);
        }
    }

    internal class SharedPinnedByteArray
    {
        internal byte[] bits;
        internal GCHandle handle;
        internal IntPtr bitPtr;

        int refCount;

        public SharedPinnedByteArray(int length)
        {
            bits = new byte[length];
            // if not pinned the GC can move around the array
            handle = GCHandle.Alloc(bits, GCHandleType.Pinned);
            bitPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bits, 0);
            refCount++;
        }

        internal void AddReference()
        {
            refCount++;
        }

        internal void ReleaseReference()
        {
            refCount--;
            if(refCount<=0)
                Destroy();
        }

        bool destroyed;
        private void Destroy()
        {
            if(!destroyed)
            {
                handle.Free();
                bits=null;
                destroyed=true;
            }
        }

        ~SharedPinnedByteArray()
        {
            Destroy();
        }
    }
}

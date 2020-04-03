// Please do not remove :)
// Written by Kourosh Derakshan
//
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.Image {
    /// <summary>
    /// Allows reading of embedded thumbnail image from the EXIF information in an image.
    /// </summary>
    public abstract class ExifThumbReader {
        // GDI plus functions
        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int GdipGetPropertyItem(IntPtr image, int propid, int size, IntPtr buffer);

        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int GdipGetPropertyItemSize(IntPtr image, int propid, out int size);

        [DllImport("gdiplus.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int GdipLoadImageFromFile(string filename, out IntPtr image);

        [DllImport("gdiplus.dll", EntryPoint = "GdipDisposeImage", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int GdipDisposeImage(IntPtr image);





        // EXIT tag value for thumbnail data. Value specified by EXIF standard
        private static int THUMBNAIL_DATA = 0x501B;

        /// <summary>
        /// Reads the thumbnail in the given image. If no thumbnail is found, returns null
        /// </summary>
        public static System.Drawing.Image ReadThumb(string imagePath) {
            const int GDI_ERR_PROP_NOT_FOUND = 19;  // Property not found error
            const int GDI_ERR_OUT_OF_MEMORY = 3;

            IntPtr hImage = IntPtr.Zero;
            IntPtr buffer = IntPtr.Zero;    // Holds the thumbnail data
            int ret;
            ret = GdipLoadImageFromFile(imagePath, out hImage);

            try {
                if (ret != 0)
                    throw createException(ret);

                int propSize;
                ret = GdipGetPropertyItemSize(hImage, THUMBNAIL_DATA, out propSize);
                // Image has no thumbnail data in it. Return null
                if (ret == GDI_ERR_PROP_NOT_FOUND)
                    return null;
                if (ret != 0)
                    throw createException(ret);


                // Allocate a buffer in memory
                buffer = Marshal.AllocHGlobal(propSize);
                if (buffer == IntPtr.Zero)
                    throw createException(GDI_ERR_OUT_OF_MEMORY);

                ret = GdipGetPropertyItem(hImage, THUMBNAIL_DATA, propSize, buffer);
                if (ret != 0)
                    throw createException(ret);

                // buffer has the thumbnail data. Now we have to convert it to
                // an Image
                return convertFromMemory(buffer);
            }

            finally {
                // Free the buffer
                if (buffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(buffer);

                GdipDisposeImage(hImage);
            }
        }

        /// <summary>
        /// Generates an exception depending on the GDI+ error codes (I removed some error
        /// codes)
        /// </summary>
        private static Exception createException(int gdipErrorCode) {
            switch (gdipErrorCode) {
                case 1:
                    return new ExternalException("Gdiplus Generic Error", -2147467259);
                case 2:
                    return new ArgumentException("Gdiplus Invalid Parameter");
                case 3:
                    return new OutOfMemoryException("Gdiplus Out Of Memory");
                case 4:
                    return new InvalidOperationException("Gdiplus Object Busy");
                case 5:
                    return new OutOfMemoryException("Gdiplus Insufficient Buffer");
                case 7:
                    return new ExternalException("Gdiplus Generic Error", -2147467259);
                case 8:
                    return new InvalidOperationException("Gdiplus Wrong State");
                case 9:
                    return new ExternalException("Gdiplus Aborted", -2147467260);
                case 10:
                    return new FileNotFoundException("Gdiplus File Not Found");
                case 11:
                    return new OverflowException("Gdiplus Over flow");
                case 12:
                    return new ExternalException("Gdiplus Access Denied", -2147024891);
                case 13:
                    return new ArgumentException("Gdiplus Unknown Image Format");
                case 18:
                    return new ExternalException("Gdiplus Not Initialized", -2147467259);
                case 20:
                    return new ArgumentException("Gdiplus Property Not Supported Error");
            }

            return new ExternalException("Gdiplus Unknown Error", -2147418113);
        }



        /// <summary>
        /// Converts the IntPtr buffer to a property item and then converts its 
        /// value to a Drawing.Image item
        /// </summary>
        private static System.Drawing.Image convertFromMemory(IntPtr thumbData) {
            propertyItemInternal prop =
                (propertyItemInternal)Marshal.PtrToStructure
                (thumbData, typeof(propertyItemInternal));

            // The image data is in the form of a byte array. Write all 
            // the bytes to a stream and create a new image from that stream
            byte[] imageBytes = prop.Value;
            MemoryStream stream = new MemoryStream(imageBytes.Length);
            stream.Write(imageBytes, 0, imageBytes.Length);

            return System.Drawing.Image.FromStream(stream);
        }

        /// <summary>
        /// Used in Marshal.PtrToStructure().
        /// We need this dummy class because Imaging.PropertyItem is not a "blittable"
        /// class and Marshal.PtrToStructure only accepted blittable classes.
        /// (It's not blitable because it uses a byte[] array and that's not a blittable
        /// type. See MSDN for a definition of Blittable.)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private class propertyItemInternal {
            public int id = 0;
            public int len = 0;
            public short type = 0;
            public IntPtr value = IntPtr.Zero;

            public byte[] Value {
                get {
                    byte[] bytes = new byte[(uint)len];
                    Marshal.Copy(value, bytes, 0, len);
                    return bytes;
                }
            }
        }
    }
}
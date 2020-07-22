﻿// Please do not remove :)
// Written by Kourosh Derakshan
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        private const int THUMBNAIL_DATA = 0x501B;

        /// <summary>
        /// Reads the thumbnail in the given image. If no thumbnail is found, returns null
        /// </summary>
        public static async Task<System.Drawing.Image> ReadThumb(string imagePath) {
            const int GDI_ERR_PROP_NOT_FOUND = 19;  // Property not found error
            const int GDI_ERR_OUT_OF_MEMORY = 3;
            var buffer = IntPtr.Zero;    // Holds the thumbnail data
            var ret = GdipLoadImageFromFile(imagePath, out var hImage);

            try {
                if (ret != 0) {
                    throw createException(ret);
                }

                ret = GdipGetPropertyItemSize(hImage, THUMBNAIL_DATA, out var propSize);
                // Image has no thumbnail data in it. Return null
                if (ret == GDI_ERR_PROP_NOT_FOUND) {
                    return null;
                }

                if (ret != 0) {
                    throw createException(ret);
                }

                // Allocate a buffer in memory
                buffer = Marshal.AllocHGlobal(propSize);
                if (buffer == IntPtr.Zero) {
                    throw createException(GDI_ERR_OUT_OF_MEMORY);
                }

                ret = GdipGetPropertyItem(hImage, THUMBNAIL_DATA, propSize, buffer);
                if (ret != 0) {
                    throw createException(ret);
                }

                // buffer has the thumbnail data. Now we have to convert it to
                // an Image
                return await convertFromMemory(buffer).ConfigureAwait(true);
            }

            finally {
                // Free the buffer
                if (buffer != IntPtr.Zero) {
                    Marshal.FreeHGlobal(buffer);
                }

                GdipDisposeImage(hImage);
            }
        }

        /// <summary>
        /// Generates an exception depending on the GDI+ error codes (I removed some error
        /// codes)
        /// </summary>
        private static Exception createException(int gdipErrorCode) {
            return gdipErrorCode switch
            {
                1 => new ExternalException("Gdiplus Generic Error", -2147467259),
                2 => new ArgumentException("Gdiplus Invalid Parameter"),
                3 => new OutOfMemoryException("Gdiplus Out Of Memory"),
                4 => new InvalidOperationException("Gdiplus Object Busy"),
                5 => new OutOfMemoryException("Gdiplus Insufficient Buffer"),
                7 => new ExternalException("Gdiplus Generic Error", -2147467259),
                8 => new InvalidOperationException("Gdiplus Wrong State"),
                9 => new ExternalException("Gdiplus Aborted", -2147467260),
                10 => new FileNotFoundException("Gdiplus File Not Found"),
                11 => new OverflowException("Gdiplus Over flow"),
                12 => new ExternalException("Gdiplus Access Denied", -2147024891),
                13 => new ArgumentException("Gdiplus Unknown Image Format"),
                18 => new ExternalException("Gdiplus Not Initialized", -2147467259),
                20 => new ArgumentException("Gdiplus Property Not Supported Error"),
                _ => new ExternalException("Gdiplus Unknown Error", -2147418113),
            };
        }

        /// <summary>
        /// Converts the IntPtr buffer to a property item and then converts its
        /// value to a Drawing.Image item
        /// </summary>
        private static async Task<System.Drawing.Image> convertFromMemory(IntPtr thumbData) {
            var prop =
                (propertyItemInternal)Marshal.PtrToStructure
                (thumbData, typeof(propertyItemInternal));

            // The image data is in the form of a byte array. Write all 
            // the bytes to a stream and create a new image from that stream
            var imageBytes = prop.Value;
            using var stream = new MemoryStream(imageBytes.Length);
            await stream.WriteAsync(imageBytes, 0, imageBytes.Length).ConfigureAwait(true);
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
                    var bytes = new byte[(uint)len];
                    Marshal.Copy(value, bytes, 0, len);
                    return bytes;
                }
            }
        }
    }
}

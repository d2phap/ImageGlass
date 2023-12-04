/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.


Credit to Daniel Peñalba:
https://stackoverflow.com/questions/21751747/extract-thumbnail-for-any-file-in-windows
*/

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageGlass.Base;


public static class ShellThumbnailApi
{
    // Shell interface and structs
    #region Shell interface and structs

    private const string IShellItem2Guid = "7E9FB0D3-919F-4307-AB2E-9B1860310C93";


    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int SHCreateItemFromParsingName(
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        // The following parameter is not used - binding context.
        IntPtr pbc,
        ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItem shellItem);


    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr hObject);


    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern unsafe IntPtr memcpy(void* dst, void* src, UIntPtr count);



    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
    private interface IShellItem
    {
        void BindToHandler(IntPtr pbc,
            [MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
            [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            out IntPtr ppv);

        void GetParent(out IShellItem ppsi);
        void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);
        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        void Compare(IShellItem psi, uint hint, out int piOrder);
    }


    private enum SIGDN : uint
    {
        NORMALDISPLAY = 0,
        PARENTRELATIVEPARSING = 0x80018001,
        PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
        DESKTOPABSOLUTEPARSING = 0x80028000,
        PARENTRELATIVEEDITING = 0x80031001,
        DESKTOPABSOLUTEEDITING = 0x8004c000,
        FILESYSPATH = 0x80058000,
        URL = 0x80068000
    }


    private enum HResult
    {
        Ok = 0x0000,
        False = 0x0001,
        InvalidArguments = unchecked((int)0x80070057),
        OutOfMemory = unchecked((int)0x8007000E),
        NoInterface = unchecked((int)0x80004002),
        Fail = unchecked((int)0x80004005),
        ElementNotFound = unchecked((int)0x80070490),
        TypeElementNotFound = unchecked((int)0x8002802B),
        NoObject = unchecked((int)0x800401E5),
        Win32ErrorCanceled = 1223,
        Canceled = unchecked((int)0x800704C7),
        ResourceInUse = unchecked((int)0x800700AA),
        AccessDenied = unchecked((int)0x80030005)
    }


    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        [PreserveSig]
        HResult GetImage(
            [In, MarshalAs(UnmanagedType.Struct)] NativeSize size,
            [In] ShellThumbnailOptions flags,
            [Out] out IntPtr phbm);
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct NativeSize
    {
        private int width;
        private int height;

        public int Width
        {
            set { width = value; }
        }

        public int Height
        {
            set { height = value; }
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }


    #endregion // Shell interface and structs


    /// <summary>
    /// Gets thumbnail from file.
    /// </summary>
    public static Bitmap? GetThumbnail(string fileName, int width, int height, ShellThumbnailOptions options)
    {
        if (!File.Exists(fileName)) return null;

        Bitmap? clonedBitmap = null;
        var hBitmap = new IntPtr();

        try
        {
            hBitmap = GetHBitmap(Path.GetFullPath(fileName), width, height, options);

            // Original code returned the bitmap directly:
            //   return GetBitmapFromHBitmap( hBitmap );
            // I'm making a clone first, so I can dispose of the original bitmap.
            // The returned clone should be managed and not need disposing of. (I think...)
            var thumbnail = GetBitmapFromHBitmap(hBitmap);
            clonedBitmap = thumbnail.Clone() as Bitmap;
            thumbnail.Dispose();
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            if (ex.ErrorCode == -2147175936 && options.HasFlag(ShellThumbnailOptions.ThumbnailOnly)) // -2147175936 == 0x8004B200
            {
                clonedBitmap = null;
            }
            else
            {
                throw;
            }
        }
        finally
        {
            // delete HBitmap to avoid memory leaks
            DeleteObject(hBitmap);
        }

        return clonedBitmap;
    }


    // Private methods
    #region Private methods

    private static Bitmap? GetBitmapFromHBitmap(IntPtr nativeHBitmap)
    {
        var bmp = Image.FromHbitmap(nativeHBitmap);

        if (Image.GetPixelFormatSize(bmp.PixelFormat) < 32)
            return bmp;

        return CreateAlphaBitmap(bmp, PixelFormat.Format32bppArgb);
    }


    private static unsafe Bitmap? CreateAlphaBitmap(Bitmap srcBitmap, PixelFormat targetPixelFormat)
    {
        var result = new Bitmap(srcBitmap.Width, srcBitmap.Height, targetPixelFormat);

        var bmpBounds = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);
        var srcData = srcBitmap.LockBits(bmpBounds, ImageLockMode.ReadOnly, srcBitmap.PixelFormat);
        var destData = result.LockBits(bmpBounds, ImageLockMode.ReadOnly, targetPixelFormat);

        var srcDataPtr = (byte*)srcData.Scan0;
        var destDataPtr = (byte*)destData.Scan0;

        try
        {
            for (int y = 0; y <= srcData.Height - 1; y++)
            {
                for (int x = 0; x <= srcData.Width - 1; x++)
                {
                    //this is really important because one stride may be positive and the other negative
                    var position = srcData.Stride * y + 4 * x;
                    var position2 = destData.Stride * y + 4 * x;

                    memcpy(destDataPtr + position2, srcDataPtr + position, (UIntPtr)4);
                }
            }
        }
        finally
        {
            srcBitmap.UnlockBits(srcData);
            result.UnlockBits(destData);
        }

        using (srcBitmap)
            return result;
    }


    private static IntPtr GetHBitmap(string fileName, int width, int height, ShellThumbnailOptions options)
    {
        IShellItem nativeShellItem;
        var shellItem2Guid = new Guid(IShellItem2Guid);
        int retCode = SHCreateItemFromParsingName(fileName, IntPtr.Zero, ref shellItem2Guid, out nativeShellItem);

        if (retCode != 0)
            throw Marshal.GetExceptionForHR(retCode) ?? new Exception($"Error with code: {retCode}");

        var nativeSize = new NativeSize();
        nativeSize.Width = width;
        nativeSize.Height = height;

        IntPtr hBitmap;
        HResult hr = ((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize, options, out hBitmap);

        Marshal.ReleaseComObject(nativeShellItem);

        if (hr == HResult.Ok) return hBitmap;

        throw Marshal.GetExceptionForHR((int)hr) ?? new Exception($"Error with code: {(int)hr}");
    }

    #endregion // Private methods

}



/// <summary>
/// <c>SIIGBF</c> Flags: <see href="https://learn.microsoft.com/en-gb/windows/win32/api/shobjidl_core/nf-shobjidl_core-ishellitemimagefactory-getimage" />
/// </summary>
[Flags]
public enum ShellThumbnailOptions
{
    /// <summary>
    /// <c>SIIGBF_RESIZETOFIT</c>c>: Shrink the bitmap as necessary to fit, preserving its aspect ratio. Returns thumbnail if available, else icon.
    /// </summary>
    None = 0x00,


    /// <summary>
    /// <c>SIIGBF_BIGGERSIZEOK</c>: Passed by callers if they want to stretch the returned image themselves. For example, if the caller passes an icon size of 80x80, a 96x96 thumbnail could be returned. This action can be used as a performance optimization if the caller expects that they will need to stretch the image. Note that the Shell implementation of IShellItemImageFactory performs a GDI stretch blit. If the caller wants a higher quality image stretch than provided through that mechanism, they should pass this flag and perform the stretch themselves.
    /// </summary>    
    BiggerSizeOk = 0x01,


    /// <summary>
    /// <c>SIIGBF_MEMORYONLY</c>: Return the item only if it is already in memory. Do not access the disk even if the item is cached. Note that this only returns an already-cached icon and can fall back to a per-class icon if an item has a per-instance icon that has not been cached. Retrieving a thumbnail, even if it is cached, always requires the disk to be accessed, so GetImage should not be called from the UI thread without passing SIIGBF_MEMORYONLY.
    /// </summary>
    InMemoryOnly = 0x02,


    /// <summary>
    /// <c>SIIGBF_ICONONLY</c>: Return only the icon, never the thumbnail.
    /// </summary>
    IconOnly = 0x04,


    /// <summary>
    /// <c>SIIGBF_THUMBNAILONLY</c>: Return only the thumbnail, never the icon. Note that not all items have thumbnails, so SIIGBF_THUMBNAILONLY will cause the method to fail in these cases.
    /// </summary>
    ThumbnailOnly = 0x08,


    /// <summary>
    /// <c>SIIGBF_INCACHEONLY</c>: Allows access to the disk, but only to retrieve a cached item. This returns a cached thumbnail if it is available. If no cached thumbnail is available, it returns a cached per-instance icon but does not extract a thumbnail or icon.
    /// </summary>
    InCacheOnly = 0x10,


    /// <summary>
    /// <c>SIIGBF_CROPTOSQUARE</c>: Introduced in Windows 8. If necessary, crop the bitmap to a square.
    /// </summary>
    Win8CropToSquare = 0x20,


    /// <summary>
    /// <c>SIIGBF_WIDETHUMBNAILS</c>: Introduced in Windows 8. Stretch and crop the bitmap to a 0.7 aspect ratio.
    /// </summary>
    Win8WideThumbnails = 0x40,


    /// <summary>
    /// <c>SIIGBF_ICONBACKGROUND</c>: Introduced in Windows 8. If returning an icon, paint a background using the associated app's registered background color.
    /// </summary>
    Win8IconBackground = 0x80,


    /// <summary>
    /// <c>SIIGBF_SCALEUP</c>: Introduced in Windows 8. If necessary, stretch the bitmap so that the height and width fit the given size.
    /// </summary>
    Win8ScaleUp = 0x100,
}


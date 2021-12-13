using System.Runtime.InteropServices;

namespace ImageGlass.PhotoBox;

internal enum TernaryRasterOperations : int
{
    SRCCOPY = 0xCC0020,
    SRCPAINT = 15597702,    // dest = source OR dest
    SRCAND = 8913094,       // dest = source AND dest
    SRCINVERT = 6684742,    // dest = source XOR dest
    SRCERASE = 4457256,     // dest = source AND (NOT dest )
    NOTSRCCOPY = 3342344,   // dest = (NOT source)
    NOTSRCERASE = 1114278,  // dest = (NOT src) AND (NOT dest) 
    MERGECOPY = 12583114,   // dest = (source AND pattern)
    MERGEPAINT = 12255782,  // dest = (NOT source) OR dest
    PATCOPY = 15728673,     // dest = pattern
    PATPAINT = 16452105,    // dest = DPSnoo
    PATINVERT = 5898313,    // dest = pattern XOR dest
    DSTINVERT = 5570569,    // dest = (NOT dest)
    BLACKNESS = 66,         // dest = BLACK
    WHITENESS = 16711778,   // dest = WHITE
}

internal enum StretchBltMode : int
{
    STRETCH_ANDSCANS = 1,
    STRETCH_ORSCANS = 2,
    STRETCH_DELETESCANS = 3,
    STRETCH_HALFTONE = 4,
}

internal class WinApi
{
    /// <summary>
    /// Creates a memory device context (DC) compatible with the specified device.
    /// </summary>
    /// <param name="hdc">A handle to an existing DC. If this handle is NULL,
    /// the function creates a memory DC compatible with the application's current screen.</param>
    /// <returns>
    /// If the function succeeds, the return value is the handle to a memory DC.
    /// If the function fails, the return value is <see cref="IntPtr.Zero"/>.
    /// </returns>
    [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
    internal static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);



    /// <summary>
    /// Selects an object into the specified device context (DC). The new object replaces the previous object of the same type.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="hgdiobj">A handle to the object to be selected. The specified object must have been created by using one of the following functions.</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);



    /// <summary>
    /// Releases a device context, freeing it for use by other applications.
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="hdc">A handle to the device context.</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);



    /// <summary>
    /// Retrieves the current stretching mode. The stretching mode defines how color data is added to or removed from bitmaps that are stretched or compressed when the StretchBlt function is called.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    internal static extern int GetStretchBltMode(IntPtr hdc);



    /// <summary>
    /// Sets the bitmap stretching mode in the specified device context.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="iStretchMode"></param>
    /// <returns></returns>
    [DllImport("gdi32.dll")]
    internal static extern int SetStretchBltMode(IntPtr hdc, StretchBltMode iStretchMode);



    /// <summary>Deletes the specified device context (DC).</summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <returns><para>If the function succeeds, the return value is nonzero.</para><para>If the function fails, the return value is zero.</para></returns>
    /// <remarks>An application must not delete a DC whose handle was obtained by calling the <c>GetDC</c> function. Instead, it must call the <c>ReleaseDC</c> function to free the DC.</remarks>
    [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
    internal static extern bool DeleteDC([In] IntPtr hdc);



    /// <summary>Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.</summary>
    /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
    /// <returns>
    ///  <para>If the function succeeds, the return value is nonzero.</para>
    ///  <para>If the specified handle is not valid or is currently selected into a DC, the return value is zero.</para>
    /// </returns>
    /// <remarks>
    ///  <para>Do not delete a drawing object (pen or brush) while it is still selected into a DC.</para>
    ///  <para>When a pattern brush is deleted, the bitmap associated with the brush is not deleted. The bitmap must be deleted independently.</para>
    /// </remarks>
    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DeleteObject([In] IntPtr hObject);



    /// <summary>
    /// Performs a bit-block transfer of the color data corresponding to a
    /// rectangle of pixels from the specified source device context into
    /// a destination device context.
    /// </summary>
    /// <param name="hdc">Handle to the destination device context.</param>
    /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
    /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
    /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
    /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
    /// <param name="hdcSrc">Handle to the source device context.</param>
    /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
    /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
    /// <param name="dwRop">A raster-operation code.</param>
    /// <returns>
    ///    <c>true</c> if the operation succeedes, <c>false</c> otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);



    /// <summary>
    /// Copies a bitmap from a source rectangle into a destination rectangle, stretching or compressing the bitmap to fit the dimensions of the destination rectangle, if necessary. The system stretches or compresses the bitmap according to the stretching mode currently set in the destination device context.
    /// </summary>
    /// <param name="hdcDest">A handle to the destination device context.</param>
    /// <param name="nXOriginDest">The x-coordinate, in logical units, of the upper-left corner of the destination rectangle.</param>
    /// <param name="nYOriginDest">The y-coordinate, in logical units, of the upper-left corner of the destination rectangle.</param>
    /// <param name="nWidthDest">The width, in logical units, of the destination rectangle.</param>
    /// <param name="nHeightDest">The height, in logical units, of the destination rectangle.</param>
    /// <param name="hdcSrc">A handle to the source device context.</param>
    /// <param name="nXOriginSrc">The x-coordinate, in logical units, of the upper-left corner of the source rectangle.</param>
    /// <param name="nYOriginSrc">The y-coordinate, in logical units, of the upper-left corner of the source rectangle.</param>
    /// <param name="nWidthSrc">The width, in logical units, of the source rectangle.</param>
    /// <param name="nHeightSrc">The height, in logical units, of the source rectangle.</param>
    /// <param name="dwRop">The raster operation to be performed. Raster operation codes define how the system combines colors in output operations that involve a brush, a source bitmap, and a destination bitmap.</param>
    /// <returns>
    ///   <para>If the function succeeds, the return value is nonzero.</para>
    ///   <para>If the function fails, the return value is zero.</para>
    /// </returns>
    [DllImport("gdi32.dll")]
    internal static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
    int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, TernaryRasterOperations dwRop);


}
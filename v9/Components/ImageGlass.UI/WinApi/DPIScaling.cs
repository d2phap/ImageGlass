
using System.Runtime.InteropServices;

namespace ImageGlass.UI.WinApi;

public static class DPIScaling
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern void ReleaseDC(IntPtr hWnd);


    private enum DeviceCaps
    {
        /// <summary>
        /// Horizontal width in pixels
        /// </summary>
        HORZRES = 8,

        /// <summary>
        /// Logical pixels inch in X
        /// </summary>
        LOGPIXELSX = 88,

        /// <summary>
        /// Logical pixels inch in Y
        /// </summary>
        LOGPIXELSY = 90,

        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118
    }

    public const int WM_DPICHANGED = 0x02E0;
    public const int DPI_DEFAULT = 96;

    /// <summary>
    /// Gets, sets current DPI scaling value
    /// </summary>
    public static int CurrentDPI { get; set; } = DPI_DEFAULT;

    public static short LOWORD(int number)
    {
        return (short)number;
    }

    /// <summary>
    /// Get system Dpi.
    /// NOTE: the this.DeviceDpi property is not accurate
    /// </summary>
    /// <returns></returns>
    public static int GetSystemDpi()
    {
        //var hdc = GetDC(IntPtr.Zero);

        //var val = GetDeviceCaps(hdc, DeviceCaps.LOGPIXELSX);

        //ReleaseDC(hdc);
        //return val;


        var frm = new Form();
        return frm.DeviceDpi;
    }

    /// <summary>
    /// Get DPI Scale factor
    /// </summary>
    /// <returns></returns>
    public static double GetDPIScaleFactor()
    {
        return (double)CurrentDPI / DPI_DEFAULT;
    }

    /// <summary>
    /// Transform a number to a new number after applying DPI Scale Factor
    /// </summary>
    /// <param name="num">A float number</param>
    /// <returns></returns>
    public static T Transform<T>(float num)
    {
        var type = typeof(T);
        var value = num * GetDPIScaleFactor();

        return (T)Convert.ChangeType(value, type);
    }

    /// <summary>
    /// Transform a number to a new number after applying DPI Scale Factor
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static int Transform(int num)
    {
        return (int)Math.Round(num * GetDPIScaleFactor());
    }

}

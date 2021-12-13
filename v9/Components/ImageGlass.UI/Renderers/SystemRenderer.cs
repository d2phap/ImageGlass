
using System.Runtime.InteropServices;

namespace ImageGlass.UI;

public static class SystemRenderer
{
    [DllImport("uxtheme.dll")]
    private static extern int SetWindowTheme(
        [In] IntPtr hwnd,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszSubAppName,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszSubIdList
    );

    /// <summary>
    /// Apply system theme to control
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static int ApplyTheme(Control control)
    {
        return ApplyTheme(control, "Explorer");
    }

    /// <summary>
    /// Apply system theme to control (customize)
    /// </summary>
    /// <param name="control"></param>
    /// <param name="theme"></param>
    /// <returns></returns>
    public static int ApplyTheme(Control control, string theme)
    {
        try
        {
            if (control != null)
            {
                if (control.IsHandleCreated)
                {
                    return SetWindowTheme(control.Handle, theme, null);
                }
            }
        }
        catch
        {
            return 0;
        }
        return 1;
    }

    /// <summary>
    /// Remove system theme
    /// </summary>
    /// <param name="control"></param>
    /// <returns></returns>
    public static int ClearTheme(Control control)
    {
        return ApplyTheme(control, string.Empty);
    }
}

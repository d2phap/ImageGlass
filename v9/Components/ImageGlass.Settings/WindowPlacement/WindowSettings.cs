
using System.Runtime.InteropServices;

namespace ImageGlass.Settings;


/// <summary>
/// Provides extra and correct settings for Window
/// </summary>
public class WindowSettings
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

    [DllImport("user32.dll")]
    private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);


    #region General window placement get/set functions
    /// <summary>
    /// Gets placement value of the given window using WinAPI
    /// </summary>
    /// <param name="win">A window</param>
    /// <returns></returns>
    public static WindowPlacement GetPlacementFromWindow(Form win)
    {
        _ = GetWindowPlacement(win.Handle, out WindowPlacement wp);

        return wp;
    }


    /// <summary>
    /// Sets placement to Window using WinAPI.
    /// Load window placement details for previous application session from app settings.
    /// Note: If window was closed on a monitor that is now disconnected from the computer,
    ///       this function will place the window onto a visible monitor.
    /// </summary>
    /// <param name="frm">A form window</param>
    public static void SetPlacementToWindow(Form frm, WindowPlacement wp)
    {
        // change window state 'Minimized' to 'Normal'
        wp.showCmd = wp.showCmd == WindowState.Minimized ? WindowState.Normal : wp.showCmd;

        try
        {
            _ = SetWindowPlacement(frm.Handle, ref wp);
        }
        catch { }
    }

    #endregion


    #region FrmMain placement

    /// <summary>
    /// Updates the given WindowPlacement object to FrmMain config
    /// </summary>
    /// <param name="wp"></param>
    public static void SetFrmMainPlacementConfig(WindowPlacement wp)
    {
        Config.FrmMainPositionX = wp.normalPosition.Left;
        Config.FrmMainPositionY = wp.normalPosition.Top;
        Config.FrmMainWidth = wp.normalPosition.Right - wp.normalPosition.Left;
        Config.FrmMainHeight = wp.normalPosition.Bottom - wp.normalPosition.Top;

        Config.FrmMainState = wp.showCmd;
    }


    /// <summary>
    /// Retrieves WindowPlacement object from WinMain config
    /// </summary>
    /// <returns></returns>
    public static WindowPlacement GetFrmMainPlacementFromConfig()
    {
        return new WindowPlacement(new Rect(
            Config.FrmMainPositionX,
            Config.FrmMainPositionY,
            Config.FrmMainPositionX + Config.FrmMainWidth,
            Config.FrmMainPositionY + Config.FrmMainHeight
          ), Config.FrmMainState);
    }

    #endregion

}
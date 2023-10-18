/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
*/
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
    public static WindowPlacement GetPlacementFromWindow(Form win)
    {
        _ = GetWindowPlacement(win.Handle, out WindowPlacement wp);

        if (wp.showCmd == WindowState.Normal
            && (wp.normalPosition.Left != win.Bounds.Left || wp.normalPosition.Top != win.Bounds.Top))
        {
            wp = new WindowPlacement(new WpRect(win.Left, win.Top, win.Right, win.Bottom));
        }

        return wp;
    }


    /// <summary>
    /// Sets placement to Window using WinAPI.
    /// Load window placement details for previous application session from app settings.
    /// Note: If window was closed on a monitor that is now disconnected from the computer,
    ///       this function will place the window onto a visible monitor.
    /// </summary>
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

    #endregion // General window placement get/set functions


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
    /// Retrieves WindowPlacement object from FrmMain config
    /// </summary>
    public static WindowPlacement GetFrmMainPlacementFromConfig(int extraX = 0, int extraY = 0)
    {
        return new WindowPlacement(new WpRect(
            Config.FrmMainPositionX + extraX,
            Config.FrmMainPositionY + extraY,
            Config.FrmMainPositionX + extraX + Config.FrmMainWidth,
            Config.FrmMainPositionY + extraY + Config.FrmMainHeight
          ), Config.FrmMainState);
    }

    #endregion // FrmMain placement


    #region FrmSettings placement

    /// <summary>
    /// Updates the given WindowPlacement object to FrmMain config
    /// </summary>
    /// <param name="wp"></param>
    public static void SetFrmSettingsPlacementConfig(WindowPlacement wp)
    {
        Config.FrmSettingsPositionX = wp.normalPosition.Left;
        Config.FrmSettingsPositionY = wp.normalPosition.Top;
        Config.FrmSettingsWidth = wp.normalPosition.Right - wp.normalPosition.Left;
        Config.FrmSettingsHeight = wp.normalPosition.Bottom - wp.normalPosition.Top;

        Config.FrmSettingsState = wp.showCmd;
    }


    /// <summary>
    /// Retrieves WindowPlacement object from FrmSettings config
    /// </summary>
    public static WindowPlacement GetFrmSettingsPlacementFromConfig()
    {
        return new WindowPlacement(new WpRect(
            Config.FrmSettingsPositionX,
            Config.FrmSettingsPositionY,
            Config.FrmSettingsPositionX + Config.FrmSettingsWidth,
            Config.FrmSettingsPositionY + Config.FrmSettingsHeight
          ), Config.FrmSettingsState);
    }

    #endregion // FrmSettings placement


    /// <summary>
    /// Gets WindowPlacement
    /// </summary>
    public static WindowPlacement GetPlacement(Rectangle bounds, FormWindowState formState)
    {
        var state = ToWindowState(formState);
        var rect = new WpRect(bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height);

        return new WindowPlacement(rect, state);
    }


    /// <summary>
    /// Converts <see cref="FormWindowState"/> to <see cref="WindowState"/>
    /// </summary>
    /// <param name="ws"></param>
    /// <returns></returns>
    public static WindowState ToWindowState(FormWindowState ws)
    {
        if (ws == FormWindowState.Maximized)
        {
            return WindowState.Maximized;
        }

        if (ws == FormWindowState.Minimized)
        {
            return WindowState.Minimized;
        }

        return WindowState.Normal;
    }


}
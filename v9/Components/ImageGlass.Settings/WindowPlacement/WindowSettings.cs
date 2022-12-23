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
        return new WindowPlacement(new WpRect(
            Config.FrmMainPositionX,
            Config.FrmMainPositionY,
            Config.FrmMainPositionX + Config.FrmMainWidth,
            Config.FrmMainPositionY + Config.FrmMainHeight
          ), Config.FrmMainState);
    }


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

    #endregion

}
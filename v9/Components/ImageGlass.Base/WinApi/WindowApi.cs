/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010-2022 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Foundation;

namespace ImageGlass.Base.WinApi;

public class WindowApi
{
    /// <summary>
    /// The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, 
    /// which tells the function what value of the enum to set.
    /// </summary>
    private enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3,
    }

    private enum DWMWINDOWATTRIBUTE_UNDOCUMENTED
    {
        DWMWA_SYSTEMBACKDROP_TYPE = 38,
        DWMWA_MICA_EFFECT = 1029,
    }


    // Set error mode
    #region Set error mode

    [DllImport("kernel32.dll")]
    private static extern ErrorModes SetErrorMode(ErrorModes uMode);

    [Flags]
    public enum ErrorModes : uint
    {
        SYSTEM_DEFAULT = 0x0,
        SEM_FAILCRITICALERRORS = 0x0001,
        SEM_NOGPFAULTERRORBOX = 1 << 1,
        SEM_NOALIGNMENTFAULTEXCEPT = 1 << 2,
        SEM_NOOPENFILEERRORBOX = 1 << 15
    }

    #endregion


    // Simulate mouse click
    #region Simulate mouse click

    [DllImport("user32.dll")]
    static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);


    private struct INPUT
    {
        public uint Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
    }

    private struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    #endregion


    // Show window API
    #region Show window API

    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hWnd, ShowWindowCommands msg);


    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
    /// </summary>
    public enum ShowWindowCommands: uint {
        SW_HIDE = 0,
        /// <summary>
        /// Activates and displays a window. If the window is minimized or maximized,
        /// the system restores it to its original size and position. An application
        /// should specify this flag when displaying the window for the first time.
        /// </summary>
        SW_SHOWNORMAL = 1,
        /// <summary>
        /// Activates the window and displays it as a minimized window.
        /// </summary>
        SW_SHOWMINIMIZED = 2,
        /// <summary>
        /// Activates the window and displays it as a maximized window.
        /// </summary>
        SW_SHOWMAXIMIZED = 3,
        /// <summary>
        /// Displays a window in its most recent size and position. This value is similar
        /// to SW_SHOWNORMAL, except that the window is not activated.
        /// </summary>
        SW_SHOWNOACTIVATE = 4,
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        SW_SHOW = 5,
        /// <summary>
        /// Minimizes the specified window and activates the next top-level window in
        /// the Z order.
        /// </summary>
        SW_MINIMIZE = 6,
        /// <summary>
        /// Displays the window as a minimized window. This value is similar to
        /// SW_SHOWMINIMIZED, except the window is not activated.
        /// </summary>
        SW_SHOWMINNOACTIVE = 7,
        /// <summary>
        /// Displays the window in its current size and position. This value is similar
        /// to SW_SHOW, except that the window is not activated.
        /// </summary>
        SW_SHOWNA = 8,
        /// <summary>
        /// Activates and displays the window. If the window is minimized or maximized,
        /// the system restores it to its original size and position.
        /// An application should specify this flag when restoring a minimized window.
        /// </summary>
        SW_RESTORE = 9,
        /// <summary>
        /// Sets the show state based on the SW_ value specified in the STARTUPINFO structure
        /// passed to the CreateProcess function by the program that started the application.
        /// </summary>
        SW_SHOWDEFAULT = 10,
        /// <summary>
        /// Minimizes a window, even if the thread that owns the window is not responding.
        /// This flag should only be used when minimizing windows from a different thread.
        /// </summary>
        SW_FORCEMINIMIZE = 11,
    }

    #endregion


    // Dark mode for title bar
    #region Dark mode for title bar

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    #endregion // Dark mode for title bar


    /// <summary>
    /// <para>Issue #360: IG periodically searching for dismounted device.</para>
    /// <para>
    /// Controls whether the system will handle the specified types of serious errors
    /// or whether the process will handle them.
    /// </para>
    /// <para>
    /// Best practice is that all applications call the process-wide SetErrorMode
    /// function with a parameter of SEM_FAILCRITICALERRORS at startup. This is to
    /// prevent error mode dialogs from hanging the application.
    /// </para>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-seterrormode
    /// </summary>
    /// <param name="mode"></param>
    public static void SetAppErrorMode(ErrorModes mode = ErrorModes.SEM_FAILCRITICALERRORS)
    {
        _ = SetErrorMode(mode);
    }


    /// <summary>
    /// Sets window state
    /// </summary>
    /// <param name="wndHandle"></param>
    /// <param name="cmd"></param>
    public static void ShowAppWindow(IntPtr wndHandle, ShowWindowCommands cmd)
    {
        _ = ShowWindow(wndHandle, cmd);
    }
    

    /// <summary>
    /// Simulates left mouse click on a window
    /// </summary>
    /// <param name="wndHandle"></param>
    /// <param name="clientPoint"></param>
    public static void ClickOnWindow(IntPtr wndHandle, Point clientPoint)
    {
        var oldPos = Cursor.Position;

        // get screen coordinates
        ClientToScreen(wndHandle, ref clientPoint);

        // set cursor on coords, and press mouse
        Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

        // left button down data
        var inputMouseDown = new INPUT
        {
            Type = 0
        };
        inputMouseDown.Data.Mouse.Flags = 0x0002;

        // left button up data
        var inputMouseUp = new INPUT
        {
            Type = 0
        };
        inputMouseUp.Data.Mouse.Flags = 0x0004;

        var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
        _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

        // return mouse 
        Cursor.Position = oldPos;
    }


    /// <summary>
    /// Sets dark mode for window title bar.
    /// </summary>
    public static void SetImmersiveDarkMode(IntPtr wndHandle, bool enabled)
    {
        // ~< 20H1
        if (!BHelper.IsOSBuildOrGreater(18985)) return;

        unsafe
        {
            PInvoke.DwmSetWindowAttribute(new HWND(wndHandle),
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                &enabled, sizeof(uint));
        }
    }


    /// <summary>
    /// Sets rounded corners for Windows 11.
    /// </summary>
    public static void SetRoundCorner(IntPtr wndHandle)
    {
        if (!BHelper.IsOS(WindowsOS.Win11)) return;

        unsafe
        {
            var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

            PInvoke.DwmSetWindowAttribute(new HWND(wndHandle),
                DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                &preference, sizeof(uint));
        }
    }



    /// <summary>
    /// Sets windows backdrop.
    /// </summary>
    public static void SetWindowsBackdrop(IntPtr wndHandle, DWM_SYSTEMBACKDROP_TYPE type = DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE)
    {
        if (!BHelper.IsOS(WindowsOS.Win11)) return;

        unsafe
        {
            if (BHelper.IsOS(WindowsOS.Win11_22H2))
            {
                var attr = DWMWINDOWATTRIBUTE_UNDOCUMENTED.DWMWA_SYSTEMBACKDROP_TYPE;

                _ = PInvoke.DwmSetWindowAttribute(new HWND(wndHandle),
                   (DWMWINDOWATTRIBUTE)attr,
                   &type, sizeof(uint));
            }
            else
            {
                var attr = DWMWINDOWATTRIBUTE_UNDOCUMENTED.DWMWA_MICA_EFFECT;
                var preference = 1;

                _ = PInvoke.DwmSetWindowAttribute(new HWND(wndHandle),
                   (DWMWINDOWATTRIBUTE)attr,
                   &preference, sizeof(uint));
            }
        }
    }


}


public enum DWM_SYSTEMBACKDROP_TYPE
{
    /// <summary>
    /// Let OS decides.
    /// </summary>
    DWMSBT_AUTO = 0,

    /// <summary>
    /// No effect.
    /// </summary>
    DWMSBT_NONE = 1,

    /// <summary>
    /// Mica effect.
    /// </summary>
    DWMSBT_MAINWINDOW = 2,

    /// <summary>
    /// Acrylic effect.
    /// </summary>
    DWMSBT_TRANSIENTWINDOW = 3,

    /// <summary>
    /// Draw the backdrop material effect corresponding to a window with a tabbed title bar.
    /// </summary>
    DWMSBT_TABBEDWINDOW = 4,
}

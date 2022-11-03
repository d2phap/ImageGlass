/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ImageGlass.Base;

namespace ImageGlass.Library.WinAPI {
    public class CornerApi {
        /// <summary>
        /// The enum flag for DwmSetWindowAttribute's second parameter,
        /// which tells the function what attribute to set.
        /// </summary>
        private enum DWMWINDOWATTRIBUTE {
            DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        /// <summary>
        /// The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, 
        /// which tells the function what value of the enum to set.
        /// </summary>
        private enum DWM_WINDOW_CORNER_PREFERENCE {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3,
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attribute, ref int pvAttribute, uint cbAttribute);


        /// <summary>
        /// Apply rounded corners for Windows 11
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static void ApplyCorner(IntPtr handle) {
            if (!Helpers.IsOS(WindowsOS.Win11)) {
                return;
            }


            var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            var preference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

            DwmSetWindowAttribute(handle, attribute, ref preference, sizeof(uint));
        }


        /// <summary>
        /// Sets dark mode for window title bar.
        /// </summary>
        public static void SetImmersiveDarkMode(IntPtr wndHandle, bool enabled) {
            static bool IsWindows10OrGreater(int build = -1) {
                return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
            }

            var attribute = 0;
            if (IsWindows10OrGreater(18985)) {
                attribute = (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;
            }
            else if (IsWindows10OrGreater(17763)) {
                attribute = (int)DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            }

            var enableDarkMode = enabled ? 1 : 0;

            if (attribute != 0) {
                _ = DwmSetWindowAttribute(wndHandle, (DWMWINDOWATTRIBUTE)attribute, ref enableDarkMode, sizeof(int));
            }
        }


        // Simulate mouse click
        #region Simulate mouse click

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        private struct INPUT {
            public uint Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct MOUSEKEYBDHARDWAREINPUT {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        private struct MOUSEINPUT {
            public int X;
            public int Y;
            public uint MouseData;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        #endregion

        public static void ClickOnWindow(IntPtr wndHandle, Point clientPoint) {
            var oldPos = Cursor.Position;

            // get screen coordinates
            ClientToScreen(wndHandle, ref clientPoint);

            // set cursor on coords, and press mouse
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            // left button down data
            var inputMouseDown = new INPUT {
                Type = 0
            };
            inputMouseDown.Data.Mouse.Flags = 0x0002;

            // left button up data
            var inputMouseUp = new INPUT {
                Type = 0
            };
            inputMouseUp.Data.Mouse.Flags = 0x0004;

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            _ = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            // return mouse 
            Cursor.Position = oldPos;
        }
    }
}

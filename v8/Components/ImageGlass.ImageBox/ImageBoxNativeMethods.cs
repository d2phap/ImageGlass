using System;
using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    internal class NativeMethods {
        #region Externals

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point point);

        #endregion

        #region Constants

        internal const int WM_MOUSEHWHEEL = 0x20e;

        internal const int WM_MOUSEWHEEL = 0x20a;

        internal const int WS_BORDER = 0x00800000;

        internal const int WS_EX_CLIENTEDGE = 0x200;

        #endregion

        #region Constructors

        private NativeMethods() { }

        #endregion
    }
}

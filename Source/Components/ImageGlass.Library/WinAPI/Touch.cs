/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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

Author: Kevin Routley - August 2019
*/

/********************************************
 * Windows functions and structures required
 * to handle touch support (WM_GESTURE).
 * 
 * Based on the Microsoft documentation and the
 * Windows 7 Sample: MTGestures
 ********************************************/

using ImageGlass.Base;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace ImageGlass.Library.WinAPI {
    public static class Touch {
        public const int WM_GESTURE = 0x0119;
        public const int WM_GESTURENOTIFY = 0x011A;

        public enum Action {
            None,
            SwipeLeft,
            SwipeRight,
            RotateCCW,
            RotateCW,
            ZoomIn,
            ZoomOut,
            SwipeUp,
            SwipeDown,
        }

        #region P/Invoke functions
        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetGestureConfig(IntPtr hWnd, int dwReserved, int cIDs, ref GESTURECONFIG pGestureConfig, int cbSize);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo);
        #endregion

        #region Windows Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct GESTURECONFIG {
            public int dwID; // gesture ID
            public int dwWant; // settings related to gesture ID that are to be

            // turned on
            public int dwBlock; // settings related to gesture ID that are to be

            #region Other

            // turned off

            #endregion Other
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS {
            public short x;
            public short y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTUREINFO {
            public int cbSize; // size, in bytes, of this structure

            // (including variable length Args
            // field)
            public int dwFlags; // see GF_* flags
            public int dwID; // gesture ID, see GID_* defines
            public IntPtr hwndTarget; // handle to window targeted by this

            // gesture
            [MarshalAs(UnmanagedType.Struct)]
            internal POINTS ptsLocation; // current location of this gesture
            public int dwInstanceID; // internally used
            public int dwSequenceID; // internally used
            public long ullArguments; // arguments for gestures whose

            // arguments fit in 8 BYTES
            public int cbExtraArgs; // size, in bytes, of extra arguments,

            #region Other

            // if any, that accompany this gesture

            #endregion Other
        }
        #endregion

        #region State
        private static GESTURECONFIG TouchConfig = new GESTURECONFIG {
            dwID = 0,
            dwWant = 1,
            dwBlock = 0
        };

        private static readonly int ConfigSize = Marshal.SizeOf(new GESTURECONFIG());

        private static GESTUREINFO gi = new GESTUREINFO() {
            cbSize = Marshal.SizeOf(new GESTUREINFO())
        };

        private static bool _isSwipe = false;
        private static Point _ptFirst = new Point();
        private static Point _ptSecond = new Point();

        private static Form _touchForm;
        private static int _iArgs;

        #endregion

        #region Constants
        private const long ULL_ARGUMENTS_BIT_MASK = 0x00000000FFFFFFFF;

        // Gesture message ids
        private const int GID_BEGIN = 1;
        private const int GID_END = 2;
        private const int GID_ZOOM = 3;
        private const int GID_PAN = 4;
        private const int GID_ROTATE = 5;

        // Gesture info ids
        private const int GF_BEGIN = 1;
        private const int GF_INERTIA = 2;
        private const int GF_END = 4;
        #endregion

        /// <summary>
        /// The point at which the Zoom action is taking place.
        /// NOTE: valid *only* when action is zoom/pinch
        /// </summary>
        public static Point ZoomLocation { get; private set; }

        /// <summary>
        /// The 'size' of the zoom/pinch action. Essentially,
        /// the number of zoom steps to take.
        /// NOTE: valid *only* when action is zoom/pinch
        /// </summary>
        public static int ZoomFactor { get; private set; }

        private static double ArgToRadians(long arg) {
            return ((arg / 65535.0) * 4.0 * Math.PI) - (2.0 * Math.PI);
        }

        /// <summary>
        /// Let Windows know we are accepting any and all WM_GESTURE
        /// (touch) messages.
        /// </summary>
        /// <param name="form">the main form</param>
        /// <returns>false if something failed</returns>
        public static bool AcceptTouch(Form form) {
            App.LogItAsync("WM_GESTURENOTIFY").RunSynchronously();
            _touchForm = form;
            return SetGestureConfig(form.Handle, 0, 1, ref TouchConfig, ConfigSize);
        }

        /// <summary>
        /// Translate a WM_GESTURE (touch) message into a supported
        /// action. We will get a lot of "intermediate" messages
        /// which don't result in an action, the action takes place
        /// on the _end_ of the touch gesture.
        /// </summary>
        /// <param name="m">the message</param>
        /// <param name="act">the resulting touch action</param>
        /// <returns></returns>
        public static bool DecodeTouch(Message m, out Action act) {
            act = Action.None;

            if (!GetGestureInfo(m.LParam, ref gi)) {
                return false;
            }

            switch ((int)m.WParam) {
                case GID_END:
                    // Empirically I found this is the 'best' way to handle 
                    // swipe end, instead of GF_END under GID_PAN.
                    if (_isSwipe) {
                        _isSwipe = false;
                        _ptSecond.X = gi.ptsLocation.x;
                        _ptSecond.Y = gi.ptsLocation.y;
                        _ptSecond = _touchForm.PointToClient(_ptSecond);

                        App.LogItAsync(string.Format("PANNING.END ({0},{1})", _ptSecond.X, _ptSecond.Y)).RunSynchronously();

                        var dVert = (_ptSecond.Y - _ptFirst.Y);
                        var dHorz = (_ptSecond.X - _ptFirst.X);

                        if (Math.Abs(dVert) > Math.Abs(dHorz)) {
                            if (dVert > 0) {
                                act = Action.SwipeDown;
                            }
                            else {
                                act = Action.SwipeUp;
                            }
                        }
                        else {
                            if (dHorz > 0) {
                                act = Action.SwipeRight;
                            }
                            else {
                                act = Action.SwipeLeft;
                            }
                        }
                    }
                    break;
                case GID_ROTATE:
                    switch (gi.dwFlags) {
                        case GF_BEGIN:
                            App.LogItAsync("GID_ROTATE.GF_BEG").RunSynchronously();
                            break;
                        case GF_END:
                            var rads = ArgToRadians(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK);
                            App.LogItAsync(string.Format("GID_ROTATE.GF_END ({0})", rads)).RunSynchronously();

                            if (rads > 0.0) {
                                act = Action.RotateCCW;
                            }
                            else {
                                act = Action.RotateCW;
                            }

                            break;
                    }
                    break;
                case GID_PAN:
                    if (gi.dwFlags == GF_BEGIN) {
                        _ptFirst.X = gi.ptsLocation.x;
                        _ptFirst.Y = gi.ptsLocation.y;
                        _ptFirst = _touchForm.PointToClient(_ptFirst);
                        App.LogItAsync(string.Format("GID_PAN.GF_BEGIN ({0},{1})", _ptFirst.X, _ptFirst.Y)).RunSynchronously();
                        _isSwipe = true;
                    }
                    break;
                case GID_ZOOM:
                    if (gi.dwFlags == GF_BEGIN) {
                        // The zoom center and factor are derived from the first and last data points
                        _ptFirst.X = gi.ptsLocation.x;
                        _ptFirst.Y = gi.ptsLocation.y;
                        _ptFirst = _touchForm.PointToClient(_ptFirst);
                        _iArgs = (int)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK);

                        App.LogItAsync(string.Format("GID_ZOOM.GF_BEGIN ({0},{1})", _ptFirst.X, _ptFirst.Y)).RunSynchronously();
                    }
                    if (gi.dwFlags == GF_END) {
                        _ptSecond.X = gi.ptsLocation.x;
                        _ptSecond.Y = gi.ptsLocation.y;
                        _ptSecond = _touchForm.PointToClient(_ptSecond);

                        // This is the center of the zoom
                        ZoomLocation = new Point((_ptFirst.X + _ptSecond.X) / 2,
                                                 (_ptFirst.Y + _ptSecond.Y) / 2);

                        // This is the size of the spread/pinch. The direction 
                        // dictates whether this is a spread or a pinch; the
                        // size indicates the magnitude.
                        var factor = (double)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK) / _iArgs;

                        if (factor < 1.0)  // pinch
                        {
                            act = Action.ZoomOut;
                            ZoomFactor = (int)(1.0 / factor);
                        }
                        else // zoom
                        {
                            act = Action.ZoomIn;
                            ZoomFactor = (int)factor;
                        }

                        App.LogItAsync($"GID_ZOOM.GF_END ({factor}:{ZoomFactor})").RunSynchronously();
                    }
                    break;
                default:
                    App.LogItAsync("GID_?").RunSynchronously();
                    break;
            }

            return true;
        }
    }
}

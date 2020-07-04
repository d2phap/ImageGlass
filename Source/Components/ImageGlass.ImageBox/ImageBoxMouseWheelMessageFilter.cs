using System;
using System.Drawing;
using System.Windows.Forms;

// Cyotek ImageBox
// Copyright (c) 2010-2015 Cyotek Ltd.
// http://cyotek.com
// http://cyotek.com/blog/tag/imagebox

// Licensed under the MIT License. See license.txt for the full text.

// If you use this control in your applications, attribution, donations or contributions are welcome.

// This code is derived from http://stackoverflow.com/a/13292894/148962 and http://stackoverflow.com/a/11034674/148962

namespace ImageGlass {
    /// <summary>
    /// A message filter for WM_MOUSEWHEEL and WM_MOUSEHWHEEL. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:System.Windows.Forms.IMessageFilter"/>
    internal sealed class ImageBoxMouseWheelMessageFilter: IMessageFilter {
        #region Member Declarations

        private static ImageBoxMouseWheelMessageFilter _instance;

        private static bool _active;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that prevents a default instance of this class from being created.
        /// </summary>
        private ImageBoxMouseWheelMessageFilter() { }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets or sets a value indicating whether the filter is active
        /// </summary>
        /// <value>
        /// <c>true</c> if the message filter is active, <c>false</c> if not.
        /// </value>
        public static bool Active {
            get { return _active; }
            set {
                if (_active != value) {
                    _active = value;

                    if (_active) {
                        if (_instance == null) {
                            _instance = new ImageBoxMouseWheelMessageFilter();
                        }
                        Application.AddMessageFilter(_instance);
                    }
                    else {
                        if (_instance != null) {
                            Application.RemoveMessageFilter(_instance);
                        }
                    }
                }
            }
        }

        #endregion

        #region IMessageFilter Interface

        /// <summary>
        /// Filters out a message before it is dispatched.
        /// </summary>
        /// <param name="m">  [in,out] The message to be dispatched. You cannot modify this message. </param>
        /// <returns>
        /// <c>true</c> to filter the message and stop it from being dispatched; <c>false</c> to allow the message to
        /// continue to the next filter or control.
        /// </returns>
        /// <seealso cref="M:System.Windows.Forms.IMessageFilter.PreFilterMessage(Message@)"/>
        bool IMessageFilter.PreFilterMessage(ref Message m) {
            bool result;

            switch (m.Msg) {
                case NativeMethods.WM_MOUSEWHEEL: // 0x020A
                case NativeMethods.WM_MOUSEHWHEEL: // 0x020E
                    IntPtr hControlUnderMouse;

                    hControlUnderMouse = NativeMethods.WindowFromPoint(new Point((int)m.LParam));
                    if (hControlUnderMouse == m.HWnd) {
                        // already headed for the right control
                        result = false;
                    }
                    else {
                        ImageBox control;

                        control = Control.FromHandle(hControlUnderMouse) as ImageBox;

                        if (control == null || !control.AllowUnfocusedMouseWheel) {
                            // window under the mouse either isn't managed, isn't an imagebox,
                            // or it is an imagebox but the unfocused whell option is disabled.
                            // whatever the case, do not try and handle the message
                            result = false;
                        }
                        else {
                            // redirect the message to the control under the mouse
                            NativeMethods.SendMessage(hControlUnderMouse, m.Msg, m.WParam, m.LParam);

                            // eat the message (otherwise it's possible two controls will scroll
                            // at the same time, which looks awful... and is probably confusing!)
                            result = true;
                        }
                    }
                    break;
                default:
                    // not a message we can process, don't try and block it
                    result = false;
                    break;
            }

            return result;
        }

        #endregion
    }
}

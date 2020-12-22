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
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageGlass.Library.WinAPI {
    /// <summary>
    /// Make the frameless form movable when dragging itself or its controls
    /// </summary>
    public class MovableForm {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        private readonly Form _form;
        private bool _isKeyDown = true;

        #region Public props

        /// <summary>
        /// Manually enable / disable moving
        /// </summary>
        public bool IsAllowMoving { get; set; } = true;

        /// <summary>
        /// Gets, sets the mouse button press for moving
        /// </summary>
        public MouseButtons MouseButton { get; set; } = MouseButtons.Left;

        /// <summary>
        /// Gets, sets the Key press for moving
        /// </summary>
        public Keys Key { get; set; } = Keys.None;

        /// <summary>
        /// Gets, sets the controls that do not require special Key holding to move
        /// </summary>
        public HashSet<string> FreeMoveControlNames { get; set; } = new HashSet<string>();

        #endregion

        /// <summary>
        /// Initialize the MovableForm
        /// </summary>
        /// <param name="form">The form to make it movable</param>
        public MovableForm(Form form) => _form = form;

        #region Public methods

        /// <summary>
        /// Enable moving ability on this form
        /// </summary>
        public void Enable() {
            _isKeyDown = this.Key == Keys.None;

            _form.KeyDown += this.Form_KeyDown;
            _form.KeyUp += this.Form_KeyUp;
            _form.MouseDown += Event_MouseDown;
        }

        /// <summary>
        /// Enable moving ability on the given controls
        /// </summary>
        /// <param name="controls"></param>
        public void Enable(params Control[] controls) {
            _isKeyDown = this.Key == Keys.None;

            foreach (var item in controls) {
                item.MouseDown += Event_MouseDown;
            }
        }

        /// <summary>
        /// Disable moving ability on this form
        /// </summary>
        public void Disable() {
            _form.KeyDown -= Form_KeyDown;
            _form.KeyUp -= Form_KeyUp;
            _form.MouseDown -= Event_MouseDown;
        }

        /// <summary>
        /// Disable moving ability on the given controls
        /// </summary>
        /// <param name="controls"></param>
        public void Disable(params Control[] controls) {
            foreach (var item in controls) {
                item.MouseDown -= Event_MouseDown;
            }
        }

        #endregion

        #region Events: Frameless form moving

        private void Form_KeyDown(object sender, KeyEventArgs e) {
            if (this.Key == Keys.None) {
                _isKeyDown = true;
            }
            else {
                _isKeyDown = e.KeyData == this.Key;
            }
        }

        private void Form_KeyUp(object sender, KeyEventArgs e) {
            _isKeyDown = this.Key == Keys.None;
        }

        private void Event_MouseDown(object sender, MouseEventArgs e) {
            // check if 'sender' can move without keydown event
            var control = (Control)sender;
            var isFreeMove = this.FreeMoveControlNames.Count > 0
                && this.FreeMoveControlNames.Contains(control.Name);

            if (e.Clicks == 1
                && e.Button == this.MouseButton
                && this.IsAllowMoving
                && (_isKeyDown || isFreeMove)) {
                ReleaseCapture();
                SendMessage(_form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

    }
}

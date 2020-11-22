/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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

using System;
using System.Windows.Forms;

namespace ImageGlass.UI {
    public class NakedTabControl: TabControl {
        public NakedTabControl() {
            // hide the one-line navigation buttons
            if (!this.DesignMode) {
                this.Multiline = true;
            }
        }

        protected override void WndProc(ref Message m) {
            // removed tab border
            if (m.Msg == 0x1328 && !this.DesignMode) {
                m.Result = new IntPtr(1);
            }
            else {
                base.WndProc(ref m);
            }
        }
    }
}

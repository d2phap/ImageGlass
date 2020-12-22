/*
A custom class to deal with several issues with the behavior of tooltips in the
standard Toolstrip.

From: Ivan Ičin
https://www.codeproject.com/Articles/376643/ToolStrip-with-Custom-ToolTip
Slightly tweaked by Kevin Routley for cleanup and ImageGlass specific requirements.
This is a much cleaner solution than earlier attempts.

See Github issues #426, 409 for references.

Issues solved:
1. The tooltip would not vanish when the user clicked the toolstrip button.
2. The tooltip would "flash" when the user re-visited the button. Namely,
   the initial delay time for the tooltip was too low.


Part of
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.UI {
    public class ToolStripToolTip: ToolStrip {
        private ToolStripItem mouseOverItem;
        private Point mouseOverPoint;
        private readonly Timer timer;
        private ToolTip _tooltip;
        public int ToolTipInterval = 4000;
        public string ToolTipText;

        /// <summary>
        /// Gets, sets value indicates that the tooltip direction is top or bottom
        /// </summary>
        public bool ToolTipShowUp { get; set; } = false;


        /// <summary>
        /// Gets, sets value indicates that the tooltip is shown
        /// </summary>
        public bool HideTooltips { get; set; } = false;

        private ToolbarAlignment _alignment;

        private ToolTip Tooltip {
            get {
                if (_tooltip == null) {
                    _tooltip = new ToolTip();
                    Tooltip.AutomaticDelay = 2000;
                    Tooltip.InitialDelay = 2000;
                }
                return _tooltip;
            }
        }

        /// <summary>
        /// Gets, sets items alignment
        /// </summary>
        public ToolbarAlignment Alignment {
            get => _alignment;
            set {
                this._alignment = value;

                this.UpdateAlignment();
            }
        }

        #region Protected methods
        protected override void OnMouseMove(MouseEventArgs mea) {
            base.OnMouseMove(mea);

            if (HideTooltips) return;

            var newMouseOverItem = this.GetItemAt(mea.Location);
            if (mouseOverItem != newMouseOverItem ||
                (Math.Abs(mouseOverPoint.X - mea.X) > SystemInformation.MouseHoverSize.Width || (Math.Abs(mouseOverPoint.Y - mea.Y) > SystemInformation.MouseHoverSize.Height))) {
                mouseOverItem = newMouseOverItem;
                mouseOverPoint = mea.Location;
                Tooltip.Hide(this);
                timer.Stop();
                timer.Start();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);
            var newMouseOverItem = this.GetItemAt(e.Location);
            if (newMouseOverItem != null) {
                Tooltip.Hide(this);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mea) {
            base.OnMouseUp(mea);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var newMouseOverItem = this.GetItemAt(mea.Location);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        protected override void OnMouseLeave(EventArgs e) {
            base.OnMouseLeave(e);
            timer.Stop();
            Tooltip.Hide(this);
        }

        private void timer_Tick(object sender, EventArgs e) {
            timer.Stop();
            try {
                Point currentMouseOverPoint;
                if (ToolTipShowUp) {
                    currentMouseOverPoint = this.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y - Cursor.Current.Size.Height + Cursor.Current.HotSpot.Y - this.Height / 2));
                }
                else {
                    currentMouseOverPoint = this.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y + Cursor.Current.Size.Height - Cursor.Current.HotSpot.Y));
                }

                if (mouseOverItem == null) {
                    if (!string.IsNullOrEmpty(ToolTipText)) {
                        Tooltip.Show(ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                    }
                }
                // TODO: revisit this; toolbar buttons like to disappear, if changed.
                else if (((!(mouseOverItem is ToolStripDropDownButton) && !(mouseOverItem is ToolStripSplitButton)) ||
#pragma warning disable IDE0038 // Use pattern matching
                    ((mouseOverItem is ToolStripDropDownButton) && !((ToolStripDropDownButton)mouseOverItem).DropDown.Visible) ||
#pragma warning restore IDE0038 // Use pattern matching
#pragma warning disable IDE0038 // Use pattern matching
                    ((mouseOverItem is ToolStripSplitButton) && !((ToolStripSplitButton)mouseOverItem).DropDown.Visible)) && !string.IsNullOrEmpty(mouseOverItem.ToolTipText) && Tooltip != null) {
                    Tooltip.Show(mouseOverItem.ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                }
            }
            catch { }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                timer.Dispose();
                Tooltip.Dispose();
            }
        }

        #endregion

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            this.UpdateAlignment();
        }

        public ToolStripToolTip() : base() {
            ShowItemToolTips = false;
            timer = new Timer {
                Enabled = false,
                Interval = 200 // KBR enforce long initial time SystemInformation.MouseHoverTime;
            };
            timer.Tick += timer_Tick;
        }

        /// <summary>
        /// Update the alignment if toolstrip items
        /// </summary>
        public void UpdateAlignment() {
            if (this.Items.Count == 0) {
                return;
            }

            var firstBtn = this.Items[0];
            var defaultMargin = new Padding(3, firstBtn.Margin.Top, firstBtn.Margin.Right, firstBtn.Margin.Bottom);

            // reset the alignment to left
            firstBtn.Margin = defaultMargin;

            if (this.Alignment == ToolbarAlignment.CENTER) {
                // get the correct content width, excluding the sticky right items
                var toolbarContentWidth = 0;
                foreach (ToolStripItem item in this.Items) {
                    if (item.Alignment == ToolStripItemAlignment.Right) {
                        toolbarContentWidth += item.Width * 2;
                    }
                    else {
                        toolbarContentWidth += item.Width;
                    }

                    // reset margin
                    item.Margin = defaultMargin;
                }

                // if the content cannot fit the toolbar size:
                // (toolbarContentWidth > toolMain.Size.Width)
                if (this.OverflowButton.Visible) {
                    // align left
                    firstBtn.Margin = defaultMargin;
                }
                else {
                    // the default margin (left alignment)
                    var margin = defaultMargin;

                    // get the gap of content width and toolbar width
                    var gap = Math.Abs(this.Width - toolbarContentWidth);

                    // update the left margin value
                    margin.Left = gap / 2;

                    // align the first item
                    firstBtn.Margin = margin;
                }
            }
        }
    }
}

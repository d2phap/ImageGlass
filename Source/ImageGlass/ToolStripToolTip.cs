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
Copyright (C) 2018 DUONG DIEU PHAP
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

namespace ImageGlass
{
    public class ToolStripToolTip : ToolStrip
    {
        ToolStripItem mouseOverItem = null;
        Point mouseOverPoint;
        Timer timer;
        private ToolTip _tooltip;
        public int ToolTipInterval = 4000;
        public string ToolTipText;
        public bool ToolTipShowUp;

        protected override void OnMouseMove(MouseEventArgs mea)
        {
            base.OnMouseMove(mea);
            ToolStripItem newMouseOverItem = this.GetItemAt(mea.Location);
            if (mouseOverItem != newMouseOverItem ||
                (Math.Abs(mouseOverPoint.X - mea.X) > SystemInformation.MouseHoverSize.Width || (Math.Abs(mouseOverPoint.Y - mea.Y) > SystemInformation.MouseHoverSize.Height)))
            {
                mouseOverItem = newMouseOverItem;
                mouseOverPoint = mea.Location;
                Tooltip.Hide(this);
                timer.Stop();
                timer.Start();
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            ToolStripItem newMouseOverItem = this.GetItemAt(e.Location);
            if (newMouseOverItem != null)
            {
                Tooltip.Hide(this);
            }
        }

        protected override void OnMouseUp(MouseEventArgs mea)
        {
            base.OnMouseUp(mea);
            ToolStripItem newMouseOverItem = this.GetItemAt(mea.Location);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            timer.Stop();
            Tooltip.Hide(this);
            mouseOverPoint = new Point(-50, -50);
            mouseOverItem = null;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            try
            {
                Point currentMouseOverPoint;
                if (ToolTipShowUp)
                    currentMouseOverPoint = this.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y - Cursor.Current.Size.Height + Cursor.Current.HotSpot.Y));
                else
                    currentMouseOverPoint = this.PointToClient(new Point(Control.MousePosition.X, Control.MousePosition.Y + Cursor.Current.Size.Height - Cursor.Current.HotSpot.Y));

                if (mouseOverItem == null)
                {
                    if (ToolTipText != null && ToolTipText.Length > 0)
                    {
                        Tooltip.Show(ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                    }
                }
                else if ((!(mouseOverItem is ToolStripDropDownButton) && !(mouseOverItem is ToolStripSplitButton)) ||
                    ((mouseOverItem is ToolStripDropDownButton) && !((ToolStripDropDownButton)mouseOverItem).DropDown.Visible) ||
                    (((mouseOverItem is ToolStripSplitButton) && !((ToolStripSplitButton)mouseOverItem).DropDown.Visible)))
                {
                    if (mouseOverItem.ToolTipText != null && mouseOverItem.ToolTipText.Length > 0 && Tooltip != null)
                    {
                        Tooltip.Show(mouseOverItem.ToolTipText, this, currentMouseOverPoint, ToolTipInterval);
                    }
                }
            }
            catch
            { }
        }

        private ToolTip Tooltip
        {
            get
            {
                if (_tooltip == null)
                {
                    _tooltip = new ToolTip();
                    Tooltip.AutomaticDelay = 2000;
                    Tooltip.InitialDelay = 2000;
                }
                return _tooltip;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                timer.Dispose();
                Tooltip.Dispose();
            }
        }

        public ToolStripToolTip() : base()
        {
            ShowItemToolTips = false;
            timer = new Timer();
            timer.Enabled = false;
            timer.Interval = 200; // KBR enforce long initial time SystemInformation.MouseHoverTime;
            timer.Tick += new EventHandler(timer_Tick);
        }
    }
}

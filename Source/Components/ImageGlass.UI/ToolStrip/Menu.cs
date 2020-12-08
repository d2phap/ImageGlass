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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ImageGlass.UI {
    public static class Menu {
        /// <summary>
        /// Clone ToolStripMenu item
        /// </summary>
        /// <param name="mnu">ToolStripMenuItem</param>
        /// <returns></returns>
        public static ToolStripMenuItem Clone(ToolStripMenuItem mnu) {
            var m = new ToolStripMenuItem();

            //clone all events
            var eventsField = typeof(Component).GetField("events", BindingFlags.NonPublic | BindingFlags.Instance);
            var eventHandlerList = eventsField.GetValue(mnu);
            eventsField.SetValue(m, eventHandlerList);

            //clone all properties
            m.AccessibleName = mnu.AccessibleName;
            m.AccessibleRole = mnu.AccessibleRole;
            m.Alignment = mnu.Alignment;
            m.AllowDrop = mnu.AllowDrop;
            m.Anchor = mnu.Anchor;
            m.AutoSize = mnu.AutoSize;
            m.AutoToolTip = mnu.AutoToolTip;
            m.BackColor = Color.Transparent;
            m.BackgroundImage = mnu.BackgroundImage;
            m.BackgroundImageLayout = mnu.BackgroundImageLayout;
            m.Checked = mnu.Checked;
            m.CheckOnClick = mnu.CheckOnClick;
            m.CheckState = mnu.CheckState;
            m.DisplayStyle = mnu.DisplayStyle;
            m.Dock = mnu.Dock;
            m.DoubleClickEnabled = mnu.DoubleClickEnabled;
            m.DropDown = mnu.DropDown;
            m.Enabled = mnu.Enabled;
            m.Font = mnu.Font;
            m.ForeColor = mnu.ForeColor;
            m.Image = mnu.Image;
            m.ImageAlign = mnu.ImageAlign;
            m.ImageScaling = mnu.ImageScaling;
            m.ImageTransparentColor = mnu.ImageTransparentColor;
            m.Margin = mnu.Margin;
            m.MergeAction = mnu.MergeAction;
            m.MergeIndex = mnu.MergeIndex;
            m.Name = mnu.Name;
            m.Overflow = mnu.Overflow;
            m.Padding = mnu.Padding;
            m.RightToLeft = mnu.RightToLeft;

            m.ShortcutKeys = mnu.ShortcutKeys;
            m.ShowShortcutKeys = mnu.ShowShortcutKeys;
            m.ShortcutKeyDisplayString = mnu.ShortcutKeyDisplayString;
            m.Tag = mnu.Tag;
            m.Text = mnu.Text;
            m.TextAlign = mnu.TextAlign;
            m.TextDirection = mnu.TextDirection;
            m.TextImageRelation = mnu.TextImageRelation;
            m.ToolTipText = mnu.ToolTipText;

            m.Available = mnu.Available;

            if (!mnu.AutoSize) {
                m.Size = mnu.Size;
            }
            return m;
        }

    }
}

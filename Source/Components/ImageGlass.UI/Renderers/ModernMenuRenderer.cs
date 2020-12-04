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

using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass.UI.Renderers {
    public class ModernMenuRenderer: ToolStripProfessionalRenderer {
        private Theme theme { get; set; }

        public ModernMenuRenderer(Theme theme) : base(new ModernColors(theme)) {
            this.theme = theme;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) {
            if (e.Item.Enabled) {
                // on hover
                if (e.Item.Selected) {
                    e.TextColor = theme.MenuTextHoverColor;
                }
                else {
                    e.TextColor = theme.MenuTextColor;
                }

                base.OnRenderItemText(e);
            }
            else {
                // KBR 20190615 this step appears to be unnecessary [and prevents the menu from auto-collapsing]
                //e.Item.Enabled = true;

                if (theme.MenuBackgroundColor.GetBrightness() > 0.5) //light background color
                {
                    e.TextColor = Theme.DarkenColor(theme.MenuBackgroundColor, 0.5f);
                }
                else //dark background color
                {
                    e.TextColor = Theme.LightenColor(theme.MenuBackgroundColor, 0.5f);
                }

                base.OnRenderItemText(e);

                // KBR 20190615 this step appears to be unnecessary [and prevents the menu from auto-collapsing]
                //e.Item.Enabled = false;
            }
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e) {
            if (e.Vertical || !(e.Item is ToolStripSeparator)) {
                base.OnRenderSeparator(e);
            }
            else {
                var tsBounds = new Rectangle(Point.Empty, e.Item.Size);

                var lineY = tsBounds.Bottom - (tsBounds.Height / 2);
                var lineLeft = tsBounds.Left;
                var lineRight = tsBounds.Right;

                using (var pen = new Pen(Color.Black)) {
                    if (theme.MenuBackgroundColor.GetBrightness() > 0.5) //light background color
                    {
                        pen.Color = Color.FromArgb(35, 0, 0, 0);
                    }
                    else //dark background color
                    {
                        pen.Color = Color.FromArgb(35, 255, 255, 255);
                    }

                    e.Graphics.DrawLine(pen, lineLeft, lineY, lineRight, lineY);
                }
                base.OnRenderSeparator(e);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e) {
            if (e.ToolStrip is ToolStripDropDown) {
                // draw background
                using (var brush = new SolidBrush(theme.MenuBackgroundColor)) {
                    e.Graphics.FillRectangle(brush, e.AffectedBounds);
                }

                // draw border
                using var pen = new Pen(Color.Black);
                if (theme.MenuBackgroundColor.GetBrightness() > 0.5) //light background color
                {
                    pen.Color = Color.FromArgb(35, 0, 0, 0);
                }
                else //dark background color
                {
                    pen.Color = Color.FromArgb(35, 255, 255, 255);
                }

                e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
            }

            base.OnRenderToolStripBackground(e);
        }


        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) {
            var textColor = e.Item.Selected ? theme.MenuTextHoverColor : theme.MenuTextColor;
            using var pen = new Pen(textColor, DPIScaling.Transform<float>(1));

            e.Graphics.DrawLine(pen,
                e.Item.Width - (5 * e.Item.Height / 8),
                3 * e.Item.Height / 8,
                e.Item.Width - (4 * e.Item.Height / 8),
                e.Item.Height / 2);

            e.Graphics.DrawLine(pen,
                e.Item.Width - (4 * e.Item.Height / 8),
                e.Item.Height / 2,
                e.Item.Width - (5 * e.Item.Height / 8),
                5 * e.Item.Height / 8);


            // Render ShortcutKeyDisplayString for menu item with dropdown
            if (e.Item is ToolStripMenuItem) {
                var mnu = e.Item as ToolStripMenuItem;

                if (!string.IsNullOrWhiteSpace(mnu.ShortcutKeyDisplayString)) {
                    var shortcutSize = e.Graphics.MeasureString(mnu.ShortcutKeyDisplayString, mnu.Font);
                    var shortcutRect = new RectangleF(e.ArrowRectangle.X - shortcutSize.Width - DPIScaling.Transform<float>(13),
                        e.Item.Height / 2 - shortcutSize.Height / 2,
                        shortcutSize.Width,
                        shortcutSize.Height);

                    e.Graphics.DrawString(mnu.ShortcutKeyDisplayString,
                        e.Item.Font,
                        new SolidBrush(textColor),
                        shortcutRect);
                }
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e) {
            var textColor = e.Item.Selected ? theme.MenuTextHoverColor : theme.MenuTextColor;
            using var pen = new Pen(textColor, DPIScaling.Transform<float>(2));

            e.Graphics.DrawLine(pen,
                (2 * e.Item.Height / 10) + 1,
                e.Item.Height / 2,
                (4 * e.Item.Height / 10) + 1,
                7 * e.Item.Height / 10);

            e.Graphics.DrawLine(pen,
                4 * e.Item.Height / 10,
                7 * e.Item.Height / 10,
                8 * e.Item.Height / 10,
                3 * e.Item.Height / 10);
        }
    }

    public class ModernColors: ProfessionalColorTable {
        public override Color MenuItemSelected => theme.MenuBackgroundHoverColor;
        public override Color MenuBorder => Color.Transparent;
        public override Color MenuItemBorder => Color.Transparent;

        public override Color ImageMarginGradientBegin => Color.Transparent;
        public override Color ImageMarginGradientMiddle => Color.Transparent;
        public override Color ImageMarginGradientEnd => Color.Transparent;
        public override Color SeparatorDark => Color.Transparent;
        public override Color SeparatorLight => Color.Transparent;
        public override Color CheckBackground => Color.Transparent;
        public override Color CheckPressedBackground => Color.Transparent;
        public override Color CheckSelectedBackground => Color.Transparent;
        public override Color ButtonSelectedBorder => Color.Transparent;
        public override Color ToolStripDropDownBackground => base.ToolStripDropDownBackground;

        private Theme theme { get; set; }

        public ModernColors(Theme theme) {
            this.theme = theme;
        }
    }
}

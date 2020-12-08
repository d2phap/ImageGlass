/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2015 DUONG DIEU PHAP
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
    public class ToolStripRenderer: ToolStripSystemRenderer {
        public Color ThemeBackgroundColor { get; set; } = Color.White;
        public Color ThemeTextColor { get; set; } = Color.Black;

        public ToolStripRenderer(Color backgroundColor, Color textColor) {
            this.ThemeBackgroundColor = backgroundColor;
            this.ThemeTextColor = textColor;
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            // Disable the base() method here to remove unwanted border of toolbar
            // base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e) {
            #region Draw Background
            const float space = 0.12f;
            var btn = (ToolStripOverflowButton)e.Item;
            var brushBg = new SolidBrush(Color.Black);

            // hover/selected state
            if (btn.Selected) {
                brushBg = new SolidBrush(Theme.LightenColor(this.ThemeBackgroundColor, 0.15f));

                e.Graphics.FillRectangle(brushBg,
                    new RectangleF(
                        e.Item.Bounds.Width * space,
                        e.Item.Bounds.Height * space,
                        e.Item.Bounds.Width * (1 - (space * 2)),
                        e.Item.Bounds.Height * (1 - (space * 2))
                    )
                );
            }
            else if (btn.DropDown.Visible) {
                brushBg = new SolidBrush(Theme.DarkenColor(this.ThemeBackgroundColor, 0.15f));

                e.Graphics.FillRectangle(brushBg,
                    new RectangleF(
                        e.Item.Bounds.Width * space,
                        e.Item.Bounds.Height * space,
                        e.Item.Bounds.Width * (1 - (space * 2)),
                        e.Item.Bounds.Height * (1 - (space * 2))
                    )
                );
            }

            brushBg.Dispose();
            #endregion

            #region Draw "..."
            var brushFont = new SolidBrush(this.ThemeTextColor);
            var font = new Font(FontFamily.GenericSerif, 10, FontStyle.Bold);
            var fontSize = e.Graphics.MeasureString("…", font);

            e.Graphics.DrawString("…",
                font,
                brushFont,
                (e.Item.Bounds.Width / 2) - (fontSize.Width / 2),
                (e.Item.Bounds.Height / 2) - (fontSize.Height / 2)
            );

            font.Dispose();
            brushFont.Dispose();
            #endregion

        }
    }
}

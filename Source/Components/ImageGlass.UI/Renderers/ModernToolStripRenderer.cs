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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ImageGlass.Base;

namespace ImageGlass.UI.Renderers {
    public class ModernToolStripRenderer: ToolStripSystemRenderer {
        private Theme theme { get; set; }

        public ModernToolStripRenderer(Theme theme) {
            this.theme = theme;
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e) {
            // Disable the base() method here to remove unwanted border of toolbar
            // base.OnRenderToolStripBorder(e);
        }

        protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e) {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            #region Draw Background
            const float space = 0.12f;
            var btn = (ToolStripOverflowButton)e.Item;
            var brushBg = new SolidBrush(Color.Black);
            var borderRadius = Helpers.IsOS(WindowsOS.Win11) ? 5 : 1;
            var rect = new RectangleF(
                e.Item.Bounds.Width * space,
                e.Item.Bounds.Height * space,
                e.Item.Bounds.Width * (1 - (space * 2)),
                e.Item.Bounds.Height * (1 - (space * 2))
            );
            using var path = Theme.GetRoundRectanglePath(rect, borderRadius);

            // on pressed
            if (btn.Pressed) {
                brushBg = new SolidBrush(theme.AccentDarkColor);
                e.Graphics.FillPath(brushBg, path);
            }
            // on hover
            else if (btn.Selected) {
                brushBg = new SolidBrush(theme.AccentLightColor);
                e.Graphics.FillPath(brushBg, path);
            }

            brushBg.Dispose();
            #endregion


            #region Draw "..."
            var brushFont = new SolidBrush(theme.TextInfoColor);
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


        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e) {
            var isBtn = e.Item.GetType().Name == nameof(ToolStripButton);

            if (isBtn) {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

                var btn = e.Item as ToolStripButton;
                var borderRadius = Helpers.IsOS(WindowsOS.Win11) ? 5 : 1;
                using var path = Theme.GetRoundRectanglePath(btn.ContentRectangle, borderRadius);

                // on pressed
                if (btn.Pressed) {
                    using var brush = new SolidBrush(theme.AccentDarkColor);
                    e.Graphics.FillPath(brush, path);
                }
                // on hover
                else if (btn.Selected) {
                    using var brush = new SolidBrush(theme.AccentLightColor);
                    e.Graphics.FillPath(brush, path);
                }
                // on checked
                else if (btn.Checked) {
                    if (e.Item.Enabled) {
                        using var brush = new SolidBrush(theme.AccentColor);
                        e.Graphics.FillPath(brush, path);
                    }
                    // on checked + disabled
                    else {
                        using var brush = new SolidBrush(Color.FromArgb(80, theme.AccentColor));
                        e.Graphics.FillPath(brush, path);
                    }
                }

                return;
            }


            base.OnRenderButtonBackground(e);
        }

    }
}

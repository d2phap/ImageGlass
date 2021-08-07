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
using System.Drawing;
using System.Drawing.Drawing2D;
using ImageGlass.Base;
using ImageGlass.ImageListView;

namespace ImageGlass.UI.Renderers {
    /// <summary>
    /// Displays items with large tiles.
    /// </summary>
    public class ModernThumbnailRenderer: ImageListView.ImageListView.ImageListViewRenderer {
        private Theme theme { get; set; }



        /// <summary>
        /// Initializes a new instance of the ModernThumbnailRenderer class.
        /// </summary>
        /// <param name="theme"></param>
        public ModernThumbnailRenderer(Theme theme) {
            this.theme = theme;
        }


        /// <summary>
        /// Returns item size for the given view mode.
        /// </summary>
        /// <param name="view">The view mode for which the item measurement should be made.</param>
        /// <returns>The item size.</returns>
        public override Size MeasureItem(View view) {
            var sz = base.MeasureItem(view);

            if (view != View.Details) {
                var textHeight = ImageListView.Font.Height;

                sz.Width += textHeight * 2 / 5;
                sz.Height -= textHeight / 2;
            }

            return sz;
        }

        /// <summary>
        /// Draws the specified item on the given graphics.
        /// </summary>
        /// <param name="g">The System.Drawing.Graphics to draw on.</param>
        /// <param name="item">The ImageListViewItem to draw.</param>
        /// <param name="state">The current view state of item.</param>
        /// <param name="bounds">The bounding rectangle of item in client coordinates.</param>
        public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds) {
            if (ImageListView.View == View.Details) {
                base.DrawItem(g, item, state, bounds);
                return;
            }

            g.SmoothingMode = SmoothingMode.HighQuality;
            var borderRadius = Helpers.IsOS(WindowsOS.Win11) ? 5 : 1;
            var itemPadding = new Size(5, 5);
            var itemMargin = new Size(5, 5);
            var itemBounds = new Rectangle(new(
                bounds.X,
                bounds.Y + itemMargin.Height),
                new(bounds.Width - itemMargin.Width, bounds.Height - 2 * itemMargin.Width));

            // on selected
            if ((state & ItemState.Selected) != ItemState.None) {
                using var brush = new SolidBrush(theme.AccentColor);
                Utility.FillRoundedRectangle(g, brush, itemBounds, borderRadius);
            }

            // on hover
            if ((state & ItemState.Hovered) != ItemState.None) {
                using var brush = new SolidBrush(theme.AccentLightColor);
                Utility.FillRoundedRectangle(g, brush, itemBounds, borderRadius);
            }

            // Draw the image
            var img = item.GetCachedImage(CachedImageType.Thumbnail);
            if (img != null) {
                var pos = Utility.GetSizedImageBounds(img, new Rectangle(
                    itemBounds.Location + itemPadding, new Size(
                        itemBounds.Width - 2 * itemPadding.Width,
                        itemBounds.Height - 2 * itemPadding.Width)));

                g.DrawImage(img, pos);
            }
        }
    }

}

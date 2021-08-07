using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ImageGlass.ImageListView;

namespace ImageGlass.UI.Renderers {
    /// <summary>
    /// Displays items with large tiles.
    /// </summary>
    public class ModernThumbnailRenderer: ImageListView.ImageListView.ImageListViewRenderer {
        private Font mCaptionFont;
        private int mTileWidth;
        private int mTextHeight;
        private Theme theme { get; set; }

        /// <summary>
        /// Gets or sets the width of the tile.
        /// </summary>
        public int TileWidth { get { return mTileWidth; } set { mTileWidth = value; } }

        private Font CaptionFont {
            get {
                if (mCaptionFont == null)
                    mCaptionFont = new Font(ImageListView.Font, FontStyle.Bold);
                return mCaptionFont;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ModernThumbnailRenderer class.
        /// </summary>
        /// <param name="theme"></param>
        public ModernThumbnailRenderer(Theme theme) : this(150) {
            this.theme = theme;
        }


        /// <summary>
        /// Initializes a new instance of the ModernThumbnailRenderer class.
        /// </summary>
        /// <param name="tileWidth">Width of tiles in pixels.</param>
        public ModernThumbnailRenderer(int tileWidth) {
            mTileWidth = tileWidth;
        }

        /// <summary>
        /// Releases managed resources.
        /// </summary>
        public override void Dispose() {
            if (mCaptionFont != null)
                mCaptionFont.Dispose();

            base.Dispose();
        }
        /// <summary>
        /// Returns item size for the given view mode.
        /// </summary>
        /// <param name="view">The view mode for which the item measurement should be made.</param>
        /// <returns>The item size.</returns>
        public override Size MeasureItem(ImageListView.View view) {
            var sz = base.MeasureItem(view);
            if (view != ImageGlass.ImageListView.View.Details) {
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
            if (ImageListView.View != ImageGlass.ImageListView.View.Details) {
                g.SmoothingMode = SmoothingMode.HighQuality;
                var itemPadding = new Size(5, 5);

                // on selected
                if ((state & ItemState.Selected) != ItemState.None) {
                    using var brush = new SolidBrush(theme.AccentColor);
                    Utility.FillRoundedRectangle(g, brush, bounds, 5);
                }

                // on hover
                if ((state & ItemState.Hovered) != ItemState.None) {
                    using var brush = new SolidBrush(theme.AccentLightColor);
                    Utility.FillRoundedRectangle(g, brush, bounds, 5);
                }

                // Draw the image
                var img = item.GetCachedImage(CachedImageType.Thumbnail);
                if (img != null) {
                    var pos = Utility.GetSizedImageBounds(img, new Rectangle(
                        bounds.Location + itemPadding, new Size(
                            bounds.Width - 2 * itemPadding.Width,
                            bounds.Height - 2 * itemPadding.Width)));

                    g.DrawImage(img, pos);
                }
            }
            else {
                base.DrawItem(g, item, state, bounds);
            }
        }
    }

}

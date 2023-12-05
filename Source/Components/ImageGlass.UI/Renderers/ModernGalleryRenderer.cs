/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using ImageGlass.Gallery;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using View = ImageGlass.Gallery.View;

namespace ImageGlass.UI;


/// <summary>
/// Displays items with large tiles.
/// </summary>
public class ModernGalleryRenderer : StyleRenderer
{
    private IgTheme Theme { get; set; }
    private float DpiScale => (float)ImageGalleryOwner.DeviceDpi / DpiApi.DPI_DEFAULT;


    /// <summary>
    /// Initializes a new instance of the ModernGalleryRenderer class.
    /// </summary>
    /// <param name="theme"></param>
    public ModernGalleryRenderer(IgTheme theme)
    {
        Theme = theme;
    }


    public override void InitializeGraphics(Graphics g)
    {
        base.InitializeGraphics(g);

        // set scroll bar dark/light mode
        _ = ExplorerApi.SetWindowTheme(ImageGalleryOwner.VScrollBar.Handle,
            "ScrollBar", Theme.Settings.IsDarkMode);

        _ = ExplorerApi.SetWindowTheme(ImageGalleryOwner.HScrollBar.Handle,
            "ScrollBar", Theme.Settings.IsDarkMode);
    }


    /// <summary>
    /// Returns item size for the given view mode.
    /// </summary>
    /// <param name="view">The view mode for which the item measurement should be made.</param>
    /// <returns>The item size.</returns>
    public override Size MeasureItem(View view)
    {
        var sz = base.MeasureItem(view);
        var textHeight = ImageGalleryOwner.Font.Height;

        sz.Width += textHeight * 2 / 5;
        sz.Height -= textHeight / 2;

        return sz;
    }

    /// <summary>
    /// Draws the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The <see cref="ImageGalleryItem"/> to draw.</param>
    /// <param name="state">The current view state of item.</param>
    /// <param name="bounds">The bounding rectangle of item in client coordinates.</param>
    public override void DrawItem(Graphics g, ImageGalleryItem item, ItemState state, Rectangle bounds)
    {
        var itemPadding = DpiApi.Scale(new SizeF(4, 4)).ToSize();
        var textSize = new Size(0, 0);


        // background
        #region Draw background

        using var bgBrush = new SolidBrush(Color.Transparent);
        using var penBorder = new Pen(bgBrush.Color, DpiApi.Scale(1.05f))
        {
            Alignment = PenAlignment.Inset,
        };
        bounds.Inflate(-(int)penBorder.Width, -(int)penBorder.Width);

        // on pressed
        if (state.HasFlag(ItemState.Pressed))
        {
            penBorder.Color = bgBrush.Color = Theme.Colors.GalleryItemActiveColor;
        }
        // on hover
        else if (state.HasFlag(ItemState.Hovered))
        {
            penBorder.Width = 0;
            penBorder.Color = bgBrush.Color = Theme.Colors.GalleryItemHoverColor;
        }
        // on selected
        else if (state.HasFlag(ItemState.Selected))
        {
            penBorder.Color = bgBrush.Color = Theme.Colors.GalleryItemSelectedColor;
        }
        // on focused
        else if (state.HasFlag(ItemState.Focused))
        {
            penBorder.Color = bgBrush.Color = Theme.Colors.GalleryItemSelectedColor;
        }

        var radius = BHelper.GetItemBorderRadius((int)bounds.Height, Const.THUMBNAIL_HEIGHT);


        // draw background
        g.FillRoundedRectangle(bgBrush, bounds, radius);
        g.DrawRoundedRectangle(penBorder, bounds, radius);

        #endregion


        // display text
        #region Display text

        if (ImageGalleryOwner.ShowItemText)
        {
            textSize = TextRenderer.MeasureText(item.Text, ImageGalleryOwner.Font);

            var foreColor = Theme.Colors.GalleryTextColor;

            if (state.HasFlag(ItemState.Disabled))
            {
                // light background color
                if (Theme.Colors.GalleryBgColor.GetBrightness() > 0.5)
                {
                    foreColor = Theme.Colors.GalleryBgColor.WithBrightness(-0.5f);
                }
                // dark background color
                else
                {
                    foreColor = Theme.Colors.GalleryBgColor.WithBrightness(0.5f);
                }
            }

            var text = item.Text;
            var textRegion = new RectangleF(
                bounds.Left + itemPadding.Width,
                bounds.Bottom - textSize.Height - itemPadding.Height,
                bounds.Width - itemPadding.Width * 2,
                textSize.Height);

            if (textSize.Width > textRegion.Width)
            {
                text = BHelper.EllipsisText(text, ImageGalleryOwner.Font, textRegion.Width, g);
            }


            // create text image
            using var textBmp = BHelper.CreateImageFromText(text, ImageGalleryOwner.Font, foreColor);

            var loc = new PointF(
                textRegion.X + (textRegion.Width / 2 - textBmp.Width / 2),
                textRegion.Y + (textRegion.Height / 2 - textBmp.Height / 2));

            if (state == ItemState.Pressed)
            {
                loc.Y += 1;
            }
            g.DrawImage(textBmp, loc);
        }

        #endregion


        // thumbnail image
        #region Thumbnail image

        // get image
        var img = item.GetCachedImage(CachedImageType.Thumbnail);
        if (img is null) return;


        // image bound
        var imgBound = new Rectangle(
            bounds.X + itemPadding.Width,
            bounds.Y + itemPadding.Height,
            bounds.Width - 2 * itemPadding.Width,
            bounds.Height - (2 * itemPadding.Height) - textSize.Height);
        var imgRect = Utility.GetSizedImageBounds(img, imgBound);

        if (state.HasFlag(ItemState.Pressed) || state.HasFlag(ItemState.Disabled))
        {
            // change opacity of the image
            var cMatrix = new ColorMatrix { Matrix33 = 0.7f };
            var imgAttrs = new ImageAttributes();
            imgAttrs.SetColorMatrix(cMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // pressed style
            if (state.HasFlag(ItemState.Pressed))
            {
                imgRect.Y += 1;
            }

            g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttrs);
        }
        else
        {
            g.DrawImage(img, imgRect, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        #endregion

    }



}


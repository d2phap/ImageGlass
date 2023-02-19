/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageGlass.UI;

public class ModernToolbarRenderer : ToolStripSystemRenderer
{
    private ModernToolbar Toolbar { get; set; }
    private IgTheme Theme => Toolbar.Theme ?? new();
    private float DpiScale => (float)Toolbar.DeviceDpi / DpiApi.DPI_DEFAULT;


    public ModernToolbarRenderer(ModernToolbar control)
    {
        Toolbar = control;
    }

    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        // Disable the base() method here to remove unwanted border of toolbar
        // base.OnRenderToolStripBorder(e);
    }


    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        // Draw Background
        #region Draw Background

        using var brushBg = new SolidBrush(Color.Transparent);
        var rect = new Rectangle(
            0,
            Toolbar.DefaultGap,
            e.Item.Width - 2,
            e.Item.Height - 2 - Toolbar.DefaultGap * 2
        );
        var radius = BHelper.GetItemBorderRadius(rect.Width, Constants.TOOLBAR_ICON_HEIGHT);


        // on pressed
        if (e.Item.Pressed)
        {
            brushBg.Color = Theme.Colors.ToolbarItemActiveColor;
        }
        // on hover
        else if (e.Item.Selected)
        {
            brushBg.Color = Theme.Colors.ToolbarItemHoverColor;
        }

        using var penBorder = new Pen(brushBg.Color, DpiApi.Transform(1.05f))
        {
            Alignment = PenAlignment.Outset,
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        // draw
        e.Graphics.FillRoundedRectangle(brushBg, rect, radius);
        e.Graphics.DrawRoundedRectangle(penBorder, rect, radius);

        #endregion // Draw Background


        // Draw "..."
        #region Draw "..."

        const string ELLIPSIS = "...";
        using var font = new Font(e.Item.Font.FontFamily, e.Item.Font.Size * 1.4f, FontStyle.Bold);

        var fontColor = e.Item.ForeColor.WithAlpha(e.Item.Pressed ? 140 : 180);
        var textImg = ThemeUtils.CreateImageFromText(ELLIPSIS, font, fontColor);

        var posX = (e.Item.Width / 2) - (textImg.Width / 2) - 1;
        var posY = (e.Item.Height / 2) - (textImg.Height / 2);

        // on pressed
        if (e.Item.Pressed) posY += 1;

        e.Graphics.DrawImage(textImg, posX, posY);

        #endregion // Draw "..."

    }


    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var isBtn = e.Item.GetType().Name == nameof(ToolStripButton);

        if (!isBtn)
        {
            base.OnRenderButtonBackground(e);
            return;
        }


        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var btn = e.Item as ToolStripButton;
        var rect = btn.ContentRectangle;
        rect.Inflate(-btn.Padding.All, -btn.Padding.All);
        rect.Location = new(1, 1);

        var radius = BHelper.GetItemBorderRadius(rect.Height, Constants.TOOLBAR_ICON_HEIGHT);
        using var brush = new SolidBrush(Color.Transparent);


        // on pressed
        if (btn.Pressed)
        {
            brush.Color = Theme.Colors.ToolbarItemActiveColor;
        }
        // on hover
        else if (btn.Selected)
        {
            brush.Color = Theme.Colors.ToolbarItemHoverColor;
        }
        // on checked
        else if (btn.Checked)
        {
            if (e.Item.Enabled)
            {
                brush.Color = Theme.Colors.ToolbarItemSelectedColor;
            }
            // on checked + disabled
            else
            {
                brush.Color = Color.FromArgb(80, Theme.Colors.ToolbarItemSelectedColor);
            }
        }
        else
        {
            return;
        }

        using var penBorder = new Pen(brush.Color, DpiApi.Transform(1f))
        {
            Alignment = PenAlignment.Outset,
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };

        // draw
        e.Graphics.FillRoundedRectangle(brush, rect, radius);
        e.Graphics.DrawRoundedRectangle(penBorder, rect, radius);
    }


    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Image is null) return;
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        var rect = e.ImageRectangle;

        // fix image alignment
        rect.X -= 1;
        rect.Y -= 1;

        // change opacity of the image
        var cMatrix = new ColorMatrix { Matrix33 = 0.7f };
        var imgAttrs = new ImageAttributes();
        imgAttrs.SetColorMatrix(cMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);


        // Disabled state
        if (!e.Item.Enabled)
        {
            var grayedImg = CreateDisabledImage(e.Image);

            e.Graphics.DrawImage(grayedImg, e.ImageRectangle,
                0, 0, e.Image.Width, e.Image.Height,
                GraphicsUnit.Pixel, imgAttrs);

            return;
        }

        // on pressed state
        if (e.Item.Pressed)
        {
            // move the image down 1px for "pressed" effect
            rect.Y += 1;
        }

        e.Graphics.DrawImage(e.Image, rect, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, e.Item.Pressed ? imgAttrs : null);
    }


    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var x = e.Item.Width / 2 - 1;
        var penWidth = DpiApi.Transform(1f);

        using var penDark = new Pen(Color.Black.WithAlpha(100), penWidth);
        using var penLight = new Pen(Color.White.WithAlpha(100), penWidth);

        // dark line
        e.Graphics.DrawLine(penDark, x, 0, x, e.Item.Height);

        // light line
        e.Graphics.DrawLine(penLight, x + 1, 0, x + 1, e.Item.Height);

    }


    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip is not ModernToolbar toolBar)
        {
            base.OnRenderToolStripBackground(e);
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var rect = new Rectangle(0, 0, toolBar.Width, toolBar.Height);
        rect.Inflate(10, 10);
        rect.Location = new Point(-5, -5);

        // none-transparent background
        if (!toolBar.EnableTransparent)
        {
            using var parentBgBrush = new SolidBrush(toolBar.TopLevelControl.BackColor);
            e.Graphics.FillRectangle(parentBgBrush, rect);
        }

        // draw
        using var brush = new SolidBrush(e.BackColor);
        e.Graphics.FillRectangle(brush, rect);

        base.OnRenderToolStripBackground(e);
    }


    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (e.Item.TextImageRelation != TextImageRelation.ImageBeforeText)
        {
            base.OnRenderItemText(e);
            return;
        }

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;


        // create text image
        var textColor = e.TextColor.WithAlpha(e.Item.Pressed ? 180 : 255);
        using var textBmp = ThemeUtils.CreateImageFromText(e.Text, e.TextFont, e.TextColor);

        var loc = new PointF(
            e.TextRectangle.X + (e.TextRectangle.Height * 0.3f),
            e.TextRectangle.Y + (e.TextRectangle.Height / 2 - textBmp.Height / 2));

        if (e.Item.Pressed)
        {
            loc.Y += 1;
        }

        e.Graphics.DrawImage(textBmp, loc);
    }

}

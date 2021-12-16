/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageGlass.UI;

public class ModernToolbarRenderer : ToolStripSystemRenderer
{
    private ModernToolbar Toolbar { get; set; }
    System.Windows.Forms.Timer _paintTimer = new();

    private IgTheme Theme => Toolbar.Theme ?? new();


    public ModernToolbarRenderer(ModernToolbar control)
    {
        Toolbar = control;
    }


    private int BorderRadius(int itemHeight)
    {
        var radius = (int)(itemHeight * 1.0f / Constants.TOOLBAR_ICON_HEIGHT * 3);

        // min border radius = 5
        return Math.Max(radius, 5);
    }


    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        // Disable the base() method here to remove unwanted border of toolbar
        // base.OnRenderToolStripBorder(e);
    }


    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        #region Draw Background
        using var brushBg = new SolidBrush(Color.Black);
        var rect = new Rectangle(
            0,
            Toolbar.DefaultGap,
            e.Item.Width - 2,
            e.Item.Height - 2 - Toolbar.DefaultGap * 2
        );

        using var path = ThemeUtils.GetRoundRectanglePath(rect, BorderRadius(rect.Width));


        // on pressed
        if (e.Item.Pressed)
        {
            brushBg.Color = Theme.Settings.ToolbarItemActiveColor;
            e.Graphics.FillPath(brushBg, path);
        }
        // on hover
        else if (e.Item.Selected)
        {
            brushBg.Color = Theme.Settings.ToolbarItemHoverColor;
            e.Graphics.FillPath(brushBg, path);
        }

        #endregion


        #region Draw "..."
        const string ELLIPSIS = "...";
        using var font = new Font(FontFamily.GenericSerif, Toolbar.IconHeight / 3, FontStyle.Bold);
        var fontSize = e.Graphics.MeasureString(ELLIPSIS, font);
        using var brushFont = new SolidBrush(Color.FromArgb(180, Theme.Settings.TextColor));

        var posX = (e.Item.Width / 2) - (fontSize.Width / 2) - 1;
        var posY = (e.Item.Height / 2) - (fontSize.Height / 2);

        // on pressed
        if (e.Item.Pressed)
        {
            posY += 1;
            brushFont.Color = Color.FromArgb(140, Theme.Settings.TextColor);
        }

        e.Graphics.DrawString(ELLIPSIS, font, brushFont, posX, posY);
        #endregion
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

        using var brush = new SolidBrush(Color.Transparent);
        using var path = ThemeUtils.GetRoundRectanglePath(rect, BorderRadius(rect.Height));


        // on pressed
        if (btn.Pressed)
        {
            brush.Color = Theme.Settings.ToolbarItemActiveColor;
        }
        // on hover
        else if (btn.Selected)
        {
            brush.Color = Theme.Settings.ToolbarItemHoverColor;
        }
        // on checked
        else if (btn.Checked)
        {
            if (e.Item.Enabled)
            {
                brush.Color = Theme.Settings.ToolbarItemSelectedColor;
            }
            // on checked + disabled
            else
            {
                brush.Color = Color.FromArgb(80, Theme.Settings.ToolbarItemSelectedColor);
            }
        }
        else
        {
            return;
        }


        // draw
        e.Graphics.FillPath(brush, path);
    }


    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        if (e.Image is null) return;

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
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            using var brush = new SolidBrush(Color.Cyan);

            // move the image down 1px for "pressed" effect
            var rect = e.ImageRectangle;
            rect.Y += 1;

            e.Graphics.DrawImage(e.Image, rect, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, imgAttrs);
            return;
        }

        base.OnRenderItemImage(e);
    }


    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var x = e.Item.Width / 2 - 1;

        using var penDark = new Pen(Color.FromArgb(100, 0, 0, 0));
        using var penLight = new Pen(Color.FromArgb(100, 255, 255, 255));

        // dark line
        e.Graphics.DrawLine(penDark, x, 0, x, e.Item.Height);

        // light line
        e.Graphics.DrawLine(penLight, x + 1, 0, x + 1, e.Item.Height);

    }


    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        using var brush = new SolidBrush(Theme.Settings.ToolbarBgColor);
        var rect = new Rectangle(0, 0, e.ToolStrip.Width, e.ToolStrip.Height);
        rect.Inflate(10, 10);
        rect.Location = new Point(-5, -5);

        // draw
        e.Graphics.FillRectangle(brush, rect);

        base.OnRenderToolStripBackground(e);
    }


    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        // on pressed
        if (e.Item.Pressed && e.Item.TextImageRelation == TextImageRelation.ImageBeforeText)
        {
            using var brush = new SolidBrush(Color.FromArgb(180, e.TextColor));
            var loc = new Point(e.TextRectangle.X + 2, e.TextRectangle.Y + 1);

            e.Graphics.DrawString(e.Text, e.TextFont, brush, loc);
        }
        else
        {
            base.OnRenderItemText(e);
        }
    }

}

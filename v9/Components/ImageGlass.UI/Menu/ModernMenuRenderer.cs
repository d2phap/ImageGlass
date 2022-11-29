/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;


public class ModernMenuRenderer : ToolStripProfessionalRenderer
{
    private IgTheme _theme { get; set; }


    public ModernMenuRenderer(IgTheme theme) : base(new ModernMenuColors(theme))
    {
        _theme = theme;
    }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        if (e.Item.Enabled)
        {
            // on hover
            if (e.Item.Selected)
            {
                e.TextColor = _theme.Settings.MenuTextHoverColor;
            }
            else
            {
                e.TextColor = _theme.Settings.MenuTextColor;
            }

            base.OnRenderItemText(e);
        }
        else
        {
            if (_theme.Settings.MenuBgColor.GetBrightness() > 0.5) // light background color
            {
                e.TextColor = ThemeUtils.DarkenColor(_theme.Settings.MenuBgColor, 0.5f);
            }
            else // dark background color
            {
                e.TextColor = ThemeUtils.LightenColor(_theme.Settings.MenuBgColor, 0.5f);
            }

            base.OnRenderItemText(e);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        if (e.Vertical || e.Item is not ToolStripSeparator)
        {
            base.OnRenderSeparator(e);
        }
        else
        {
            var tsBounds = new Rectangle(Point.Empty, e.Item.Size);

            var lineY = tsBounds.Bottom - (tsBounds.Height / 2);
            var lineLeft = tsBounds.Left;
            var lineRight = tsBounds.Right;

            using var pen = new Pen(Color.Black);
            if (_theme.Settings.MenuBgColor.GetBrightness() > 0.5) // light background color
            {
                pen.Color = Color.FromArgb(35, 0, 0, 0);
            }
            else // dark background color
            {
                pen.Color = Color.FromArgb(35, 255, 255, 255);
            }

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawLine(pen, lineLeft, lineY, lineRight, lineY);

            base.OnRenderSeparator(e);
        }
    }

    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        var textColor = e.Item.Selected ? _theme.Settings.MenuTextHoverColor : _theme.Settings.MenuTextColor;

        if (!e.Item.Enabled)
        {
            if (_theme.Settings.MenuBgColor.GetBrightness() > 0.5) //light background color
            {
                textColor = ThemeUtils.DarkenColor(_theme.Settings.MenuBgColor, 0.5f);
            }
            else //dark background color
            {
                textColor = ThemeUtils.LightenColor(_theme.Settings.MenuBgColor, 0.5f);
            }
        }


        using var pen = new Pen(textColor, DpiApi.Transform<float>(1.15f));
        pen.LineJoin = LineJoin.Round;
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var point1 = new PointF(
            e.Item.Width - (5 * e.Item.Height / 8),
            3 * e.Item.Height / 8);
        var point2 = new PointF(
            e.Item.Width - (4 * e.Item.Height / 8),
            e.Item.Height / 2);
        var point3 = new PointF(
            e.Item.Width - (5 * e.Item.Height / 8),
            5 * e.Item.Height / 8);

        var path = new GraphicsPath();
        path.AddLines(new PointF[] { point1, point2, point3 });

        e.Graphics.DrawPath(pen, path);


        // Render ShortcutKeyDisplayString for menu item with dropdown
        if (e.Item is ToolStripMenuItem)
        {
            var mnu = e.Item as ToolStripMenuItem;

            if (!string.IsNullOrWhiteSpace(mnu?.ShortcutKeyDisplayString))
            {
                var shortcutSize = e.Graphics.MeasureString(mnu.ShortcutKeyDisplayString, mnu.Font);
                var shortcutRect = new RectangleF(e.ArrowRectangle.X - shortcutSize.Width - DpiApi.Transform<float>(13),
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

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        var textColor = e.Item.Selected ? _theme.Settings.MenuTextHoverColor : _theme.Settings.MenuTextColor;
        using var pen = new Pen(textColor, DpiApi.Transform<float>(1.5f));
        pen.LineJoin = LineJoin.Round;
        
        // left margin
        var left = 5;

        var point1 = new PointF(
            (3 * e.Item.Height / 10) + left,
            (5 * e.Item.Height / 10));
        var point2 = new PointF(
            (4 * e.Item.Height / 10) + left,
            (6.5f * e.Item.Height / 10));
        var point3 = new PointF(
            (6.5f * e.Item.Height / 10) + left,
            (3 * e.Item.Height / 10));

        var path = new GraphicsPath();
        path.AddLines(new PointF[] { point1, point2, point3 });

        e.Graphics.DrawPath(pen, path);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        // hover on enable item
        if (e.Item.Selected && e.Item.Enabled)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // boundary
            var rect = new Rectangle(
                5, 2,
                e.Item.Bounds.Width - 9,
                e.Item.Bounds.Height - 5);

            using var brush = new SolidBrush(_theme.Settings.MenuBgHoverColor);
            using var path = BHelper.GetRoundRectanglePath(rect, BHelper.GetItemBorderRadius(rect.Height, Constants.MENU_ICON_HEIGHT));
            using var penBorder = new Pen(Color.FromArgb(brush.Color.A, brush.Color));

            // draw
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(penBorder, path);

            return;
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }


    // render menu dropdown background
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip is ToolStripDropDown)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // draw background
            using var brush = new SolidBrush(_theme.Settings.MenuBgColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
        else
        {
            base.OnRenderToolStripBackground(e);
        }
    }


    // render menu dropdown border
    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        if (BHelper.IsOS(WindowsOS.Win10))
        {
            // override default ugly border by a solid color
            using var penDefault = new Pen(_theme.Settings.MenuBgColor);
            e.Graphics.DrawRectangle(penDefault,
                0,
                0,
                e.AffectedBounds.Width,
                e.AffectedBounds.Height);
        }
        else
        {
            base.OnRenderToolStripBorder(e);
        }

        using var pen = new Pen(_theme.Settings.MenuBgColor);

        if (_theme.Settings.MenuBgColor.GetBrightness() > 0.5) // light background
        {
            pen.Color = Color.FromArgb(35, 0, 0, 0);
        }
        else // dark background
        {
            pen.Color = Color.FromArgb(35, 255, 255, 255);
        }

        var menuBorderRadius = BHelper.IsOS(WindowsOS.Win11OrLater) ? 8 : 0;
        using var path = BHelper.GetRoundRectanglePath(new()
        {
            X = 0,
            Y = 0,
            Width = e.AffectedBounds.Width - 1,
            Height = e.AffectedBounds.Height - 1,
        }, menuBorderRadius);

        e.Graphics.DrawPath(pen, path);
    }

}

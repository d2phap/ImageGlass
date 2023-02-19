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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ImageGlass.Base;
using ImageGlass.Base.WinApi;
using System.Drawing.Drawing2D;

namespace ImageGlass.UI;


public class ModernMenuRenderer : ToolStripProfessionalRenderer
{
    private IgTheme _theme { get; set; }
    private static Padding ContentMargin => DpiApi.Transform(new Padding(5, 2, 5, 2));


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
                e.TextColor = _theme.Colors.MenuTextHoverColor;
            }
            else
            {
                e.TextColor = _theme.Colors.MenuTextColor;
            }
        }
        else
        {
            if (_theme.Colors.MenuBgColor.GetBrightness() > 0.5) // light background color
            {
                e.TextColor = ThemeUtils.DarkenColor(_theme.Colors.MenuBgColor, 0.5f);
            }
            else // dark background color
            {
                e.TextColor = ThemeUtils.LightenColor(_theme.Colors.MenuBgColor, 0.5f);
            }
        }

        base.OnRenderItemText(e);
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

            var lineColor = _theme.Colors.MenuBgColor.Blend(_theme.Colors.MenuBgColor.InvertBlackOrWhite(), 0.9f);

            using var pen = new Pen(lineColor, DpiApi.Transform(1f));

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.DrawLine(pen, lineLeft, lineY, lineRight, lineY);

            base.OnRenderSeparator(e);
        }
    }


    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        var textColor = e.Item.Selected
            ? _theme.Colors.MenuTextHoverColor
            : _theme.Colors.MenuTextColor;

        if (!e.Item.Enabled)
        {
            textColor = _theme.Colors.MenuBgColor.InvertBlackOrWhite(100);
        }

        using var pen = new Pen(textColor, DpiApi.Transform(1.15f))
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        };
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
                var shortcutRect = new RectangleF(e.ArrowRectangle.X - shortcutSize.Width - DpiApi.Transform(13),
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
        // is radio button?
        var isRadioButton = false;
        if (e.Item.Tag is ModernMenuItemTag tag && tag != null)
        {
            isRadioButton = tag.SingleSelect;
        }

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;


        var markColor = e.Item.Selected
            ? _theme.Colors.MenuTextHoverColor
            : _theme.Colors.MenuTextColor;
        if (!e.Item.Enabled)
        {
            markColor = markColor.WithAlpha(50);
        }
        using var checkMarkBrush = new SolidBrush(markColor);


        // check rect
        var rect = new RectangleF(ContentMargin.Left, 0, e.Item.Height, e.Item.Height);
        rect.Inflate(-e.Item.Height * 0.2f, -e.Item.Height * 0.2f);

        // draw check mark for radio button
        if (isRadioButton)
        {
            var radioCheckMarkRect = rect;
            radioCheckMarkRect.Inflate(-rect.Height * 0.3f, -rect.Height * 0.3f);

            g.FillEllipse(checkMarkBrush, radioCheckMarkRect);
        }

        // draw check mark for checkbox
        else
        {
            using var pen = new Pen(checkMarkBrush, DpiApi.Transform(1.6f))
            {
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };

            var point1 = new PointF(
                rect.X + (2.5f * rect.Height / 10),
                rect.Y + (6 * rect.Height / 10));
            var point2 = new PointF(
                rect.X + (4 * rect.Height / 10),
                rect.Y + (7.5f * rect.Height / 10));
            var point3 = new PointF(
                rect.X + (7.5f * rect.Height / 10),
                rect.Y + (2.5f * rect.Height / 10));

            var path = new GraphicsPath();
            path.AddLines(new PointF[] { point1, point2, point3 });

            g.DrawPath(pen, path);
        }
    }


    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        // is radio button?
        var isRadioButton = false;
        if (e.Item.Tag is ModernMenuItemTag tag && tag != null)
        {
            isRadioButton = tag.SingleSelect;
        }

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;

        // draw background when hovering on enable item
        if (e.Item.Selected && e.Item.Enabled)
        {
            // boundary
            var rect = new Rectangle(
                ContentMargin.Left,
                ContentMargin.Top,
                e.Item.Bounds.Width - ContentMargin.Horizontal,
                e.Item.Bounds.Height - ContentMargin.Vertical);
            var radius = BHelper.GetItemBorderRadius(rect.Height, Constants.MENU_ICON_HEIGHT);

            using var brush = new SolidBrush(_theme.Colors.MenuBgHoverColor);
            using var penBorder = new Pen(brush, DpiApi.Transform(1f));

            // draw
            g.FillRoundedRectangle(brush, rect, radius);
            g.DrawRoundedRectangle(penBorder, rect, radius);
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }


        // draw check area box
        if (e.Item is ToolStripMenuItem mnu && mnu.CheckOnClick)
        {
            var bgColor = e.Item.Selected
                ? _theme.Colors.MenuTextHoverColor
                : _theme.Colors.MenuTextColor;

            if (_theme.Settings.IsDarkMode)
            {
                bgColor = e.Item.Enabled
                    ? bgColor.WithAlpha(20)
                    : bgColor.WithAlpha(7);
            }
            else
            {
                bgColor = e.Item.Enabled
                    ? bgColor.WithAlpha(80)
                    : bgColor.WithAlpha(30);
            }


            // left margin
            var rect = new RectangleF(ContentMargin.Left, 0, e.Item.Height, e.Item.Height);
            rect.Inflate(-e.Item.Height * 0.2f, -e.Item.Height * 0.2f);

            var radius = isRadioButton
                ? rect.Height
                : BHelper.GetItemBorderRadius((int)rect.Height, Constants.MENU_ICON_HEIGHT);

            using var checkAreaBrush = new SolidBrush(bgColor);
            using var checkAreaPen = new Pen(checkAreaBrush, DpiApi.Transform(1f))
            {
                Alignment = PenAlignment.Inset,
                LineJoin = LineJoin.Round,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };


            // draw radio button box
            if (isRadioButton)
            {
                var radioRect = rect;
                radioRect.Inflate(-rect.Height * 0.1f, -rect.Height * 0.1f);

                g.FillEllipse(checkAreaBrush, radioRect);
                g.DrawEllipse(checkAreaPen, radioRect);
            }

            // draw checkbox
            else
            {
                g.FillRoundedRectangle(checkAreaBrush, rect, radius);
                g.DrawRoundedRectangle(checkAreaPen, rect, radius);
            }
        }

    }


    // render menu dropdown background
    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip is ToolStripDropDown)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // draw background
            using var brush = new SolidBrush(_theme.Colors.MenuBgColor);
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
            using var penDefault = new Pen(_theme.Colors.MenuBgColor);
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

        using var pen = new Pen(_theme.Colors.MenuBgColor);

        if (_theme.Colors.MenuBgColor.GetBrightness() > 0.5) // light background
        {
            pen.Color = Color.FromArgb(35, 0, 0, 0);
        }
        else // dark background
        {
            pen.Color = Color.FromArgb(35, 255, 255, 255);
        }

        var menuBorderRadius = BHelper.IsOS(WindowsOS.Win11OrLater) ? 8 : 0;
        var rect = new RectangleF()
        {
            X = 0,
            Y = 0,
            Width = e.AffectedBounds.Width - 1,
            Height = e.AffectedBounds.Height - 1,
        };

        e.Graphics.DrawRoundedRectangle(pen, rect, menuBorderRadius);
    }


    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
    {
        // image rect
        var rect = new RectangleF(ContentMargin.Left, 0, e.Item.Height, e.Item.Height);
        rect.Inflate(-e.Item.Height * 0.15f, -e.Item.Height * 0.15f);

        e.Graphics.DrawImage(e.Image, rect);
    }
}



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

    private int BorderRadius(int itemHeight)
    {
        if (Helpers.IsOS(WindowsOS.Win10))
        {
            return 0;
        }

        var radius = (int)(itemHeight * 1.0f / Constants.MENU_ICON_HEIGHT * 3);

        // min border radius = 5
        return Math.Max(radius, 5);
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

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        if (e.ToolStrip is ToolStripDropDown)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            // draw background
            using var brush = new SolidBrush(_theme.Settings.MenuBgColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);

            // draw border
            using var pen = new Pen(Color.Black);
            if (_theme.Settings.MenuBgColor.GetBrightness() > 0.5) // light background color
            {
                pen.Color = Color.FromArgb(35, 0, 0, 0);
            }
            else // dark background color
            {
                pen.Color = Color.FromArgb(35, 255, 255, 255);
            }

            e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
        }
        else
        {
            base.OnRenderToolStripBackground(e);
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
        
        // left margin
        var left = 5;

        var point1 = new PointF(
            (2 * e.Item.Height / 10) + left,
            e.Item.Height / 2);
        var point2 = new PointF(
            (4 * e.Item.Height / 10) + left,
            7 * e.Item.Height / 10);
        var point3 = new PointF(
            8 * e.Item.Height / 10 + left,
            3 * e.Item.Height / 10);

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
            using var path = ThemeUtils.GetRoundRectanglePath(rect, BorderRadius(rect.Height));
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

}

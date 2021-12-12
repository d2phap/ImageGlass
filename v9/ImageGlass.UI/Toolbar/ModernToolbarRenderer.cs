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
using System.Drawing.Drawing2D;

namespace ImageGlass.UI.Toolbar;

public class ModernToolbarRenderer : ToolStripSystemRenderer
{
    private IgTheme Theme { get; set; }
    private const int BORDER_RADIUS = 5;

    public ModernToolbarRenderer(IgTheme theme)
    {
        Theme = theme;
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
        var brushBg = new SolidBrush(Color.Black);
        var rect = new Rectangle(
            e.Item.Margin.Left + 1,
            e.Item.Margin.Top + 1,
            e.Item.Width - e.Item.Margin.Top,
            e.Item.Height - e.Item.Margin.Top - (int)(e.Item.Margin.Bottom * 1.5)
        );

        using var path = ThemeUtils.GetRoundRectanglePath(rect, BORDER_RADIUS);


        // on pressed
        if (e.Item.Pressed)
        {
            brushBg = new SolidBrush(Theme.Settings.ToolbarItemActiveColor);
            e.Graphics.FillPath(brushBg, path);
        }
        // on hover
        else if (e.Item.Selected)
        {
            brushBg = new SolidBrush(Theme.Settings.ToolbarItemHoverColor);
            e.Graphics.FillPath(brushBg, path);
        }

        brushBg.Dispose();
        #endregion

        #region Draw "..."
        const string ELLIPSIS = "...";
        var font = new Font(FontFamily.GenericSerif, e.Item.Height / 6, FontStyle.Bold);
        var fontSize = e.Graphics.MeasureString(ELLIPSIS, font);
        var brushFont = new SolidBrush(Theme.Settings.TextColor);

        e.Graphics.DrawString(ELLIPSIS,
            font,
            brushFont,
            (e.Item.Width / 2) - (fontSize.Width / 2) - e.Item.Margin.Right,
            (e.Item.Height / 2) - (fontSize.Height / 2) + e.Item.Margin.Top
        );

        font.Dispose();
        brushFont.Dispose();
        #endregion

    }


    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
    {
        var isBtn = e.Item.GetType().Name == nameof(ToolStripButton);

        if (isBtn)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            var btn = e.Item as ToolStripButton;
            var rect = btn.ContentRectangle;
            rect.Inflate(-btn.Padding.All, -btn.Padding.All);
            rect.Location = new(1, 1);

            using var path = ThemeUtils.GetRoundRectanglePath(rect, BORDER_RADIUS);

            // on pressed
            if (btn.Pressed)
            {
                using var brush = new SolidBrush(Theme.Settings.ToolbarItemActiveColor);
                e.Graphics.FillPath(brush, path);
            }
            // on hover
            else if (btn.Selected)
            {
                using var brush = new SolidBrush(Theme.Settings.ToolbarItemHoverColor);
                e.Graphics.FillPath(brush, path);
            }
            // on checked
            else if (btn.Checked)
            {
                if (e.Item.Enabled)
                {
                    using var brush = new SolidBrush(Theme.Settings.ToolbarItemSelectedColor);
                    e.Graphics.FillPath(brush, path);
                }
                // on checked + disabled
                else
                {
                    using var brush = new SolidBrush(Color.FromArgb(80, Theme.Settings.ToolbarItemSelectedColor));
                    e.Graphics.FillPath(brush, path);
                }
            }

            return;
        }


        base.OnRenderButtonBackground(e);
    }

}

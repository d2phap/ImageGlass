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
using System.Reflection;

namespace ImageGlass.UI;


public class ModernTooltip : ToolTip
{
    private IColors Colors => BHelper.GetThemeColorPalatte(DarkMode, DesignMode);


    /// <summary>
    /// Toggles dark mode for this <see cref="ModernButton"/> control.
    /// </summary>
    public bool DarkMode { get; set; } = false;


    /// <summary>
    /// Gets, sets all padding of tooltip.
    /// </summary>
    public int AllPadding { get; set; } = (int)SystemInformation.MenuFont.SizeInPoints / 2;


    /// <summary>
    /// Gets tooltip handle.
    /// </summary>
    public IntPtr TooltipHandle
    {
        get
        {
            var prop = GetType().GetProperty("Handle", BindingFlags.Instance | BindingFlags.NonPublic);
            var handle = (IntPtr?)prop?.GetValue(this);

            return handle ?? IntPtr.Zero;
        }
    }


    public ModernTooltip() : base()
    {
        OwnerDraw = true;
        Draw += ModernTooltip_Draw;
        Popup += ModernTooltip_Popup;
    }


    protected override void Dispose(bool disposing)
    {
        Draw -= ModernTooltip_Draw;
        Popup -= ModernTooltip_Popup;

        base.Dispose(disposing);
    }


    private void ModernTooltip_Popup(object? sender, PopupEventArgs e)
    {
        _ = WindowApi.SetRoundCorner(TooltipHandle, WindowCorner.RoundSmall);

        // get the correct size of the tooltip
        var tooltipContent = GetToolTip(e.AssociatedControl);
        e.ToolTipSize = CalculateSize(tooltipContent);

        BackColor = Colors.AppBg;
        ForeColor = Colors.AppText;
    }


    private void ModernTooltip_Draw(object? sender, DrawToolTipEventArgs e)
    {
        e.DrawBackground();

        var padding = DpiApi.Scale(AllPadding);
        var bounds = e.Bounds;
        bounds.Offset(padding, 0);


        // draw tooltip title
        var titleFontHeight = 0;
        if (!string.IsNullOrWhiteSpace(ToolTipTitle))
        {
            using var titleFont = new Font(SystemInformation.MenuFont.FontFamily, SystemInformation.MenuFont.SizeInPoints, FontStyle.Bold);

            titleFontHeight = titleFont.Height;
            bounds.Offset(0, padding);

            TextRenderer.DrawText(e.Graphics, ToolTipTitle, titleFont, bounds, Colors.Accent, TextFormatFlags.Top);
        }


        // draw tooltip content
        bounds.Offset(0, titleFontHeight + padding);
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, SystemInformation.MenuFont, bounds, ForeColor, TextFormatFlags.Top);
    }


    /// <summary>
    /// Calculates tooltip size according to the system font.
    /// </summary>
    /// <param name="tooltipContent">The content of the tooltip excluding <c>ToolTipTitle</c>.</param>
    public Size CalculateSize(string tooltipContent)
    {
        using var g = Graphics.FromHwnd(TooltipHandle);
        if (g == null) return new();


        var padding = DpiApi.Scale(AllPadding);
        var titleSize = new SizeF();
        var contentSize = g.MeasureString(tooltipContent, SystemInformation.MenuFont);

        // get tooltip title size
        if (!string.IsNullOrWhiteSpace(ToolTipTitle))
        {
            using var titleFont = new Font(SystemInformation.MenuFont, FontStyle.Bold);
            titleSize = g.MeasureString(ToolTipTitle, titleFont);

            // bottom margin for tooltip title
            titleSize.Height += padding;
        }


        var tooltipWidth = Math.Max(titleSize.Width, contentSize.Width) + padding;
        var tooltipHeight = titleSize.Height + contentSize.Height + padding * 2;

        return new SizeF(tooltipWidth, tooltipHeight).ToSize();
    }

}


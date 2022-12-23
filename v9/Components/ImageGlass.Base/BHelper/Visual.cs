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
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ImageGlass.Base;

public static partial class BHelper
{

    /// <summary>
    /// Gets menu item border radius
    /// </summary>
    public static float GetItemBorderRadius(int itemHeight, int defaultItemHeight)
    {
        if (IsOS(WindowsOS.Win10))
        {
            return 0;
        }

        var radius = (itemHeight * 1.3f / defaultItemHeight * 3f);

        // min border radius = 4
        return Math.Max(radius, 4);
    }


    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    public static GraphicsPath GetRoundRectanglePath(RectangleF bounds, float radius, bool flatBottom = false, int bottomOffset = 0, bool flatTop = false)
    {
        var diameter = radius * 2;
        var size = new SizeF(diameter, diameter);
        var arc = new RectangleF(bounds.Location, size);
        var path = new GraphicsPath();

        if (radius == 0 || flatBottom && flatTop)
        {
            path.AddRectangle(bounds);
            return path;
        }

        if (flatTop)
        {
            // top
            PointF tr = new PointF(bounds.Right, bounds.Top);
            PointF tl = new PointF(bounds.Left, bounds.Top);
            path.AddLine(tl, tr);
            arc.X = bounds.Right - diameter;
        }
        else
        {
            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
        }

        if (flatBottom)
        {
            // bottom line
            PointF br = new PointF(bounds.Right, bounds.Bottom + bottomOffset);
            PointF bl = new PointF(bounds.Left, bounds.Bottom + bottomOffset);
            path.AddLine(br, bl);
        }
        else
        {
            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
        }

        path.CloseFigure();
        return path;
    }


    /// <summary>
    /// Creates default toolbar icon.
    /// </summary>
    public static Bitmap CreateDefaultToolbarIcon(int iconSize, bool darkMode)
    {
        var bmp = new Bitmap(iconSize, iconSize);

        var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var brush = new SolidBrush(darkMode ? Color.White.WithAlpha(100) : Color.Black.WithAlpha(100));

        var rect = new Rectangle(0, 0, iconSize - 1, iconSize - 1);
        g.FillEllipse(brush, rect);

        return bmp;
    }

}

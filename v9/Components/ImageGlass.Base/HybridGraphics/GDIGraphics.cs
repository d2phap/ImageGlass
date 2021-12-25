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

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageGlass.Base.HybridGraphics;

/// <summary>
/// An object that contains GDI+ graphics
/// </summary>
public class GDIGraphics : IHybridGraphics
{
    public Graphics g;

    public bool UseAntialias
    {
        get => g.SmoothingMode == SmoothingMode.AntiAlias;
        set => g.SmoothingMode = value ? SmoothingMode.AntiAlias : SmoothingMode.Default;
    }


    public GDIGraphics(Graphics g)
    {
        this.g = g;
    }


    // draw lines
    #region Draw lines

    public void DrawLine(Point p1, Point p2, Color c, float weight = 1f)
    {
        DrawLine(p1.X, p1.Y, p2.X, p2.Y, c, weight);
    }

    public void DrawLine(PointF p1, PointF p2, Color c, float weight = 1)
    {
        DrawLine(p1.X, p1.Y, p2.X, p2.Y, c, weight);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, Color c, float weight = 1)
    {
        using var p = new Pen(c, weight);
        g.DrawLine(p, x1, y1, x2, y2);
    }

    #endregion


    // draw shapes
    #region Draw shapes

    public void DrawEllipse(RectangleF rect, Color c, float weight = 1f)
    {
        using var p = new Pen(c, weight);
        g.DrawEllipse(p, rect);
    }

    public void FillEllipse(RectangleF rect, Color c)
    {
        using var b = new SolidBrush(c);
        g.FillEllipse(b, rect);
    }

    public void DrawPolygon(PointF[] ps, Color strokeColor, float weight, Color fillColor)
    {
        using var path = new GraphicsPath();
        var pt = new PointF[ps.Length];

        for (int i = 0; i < ps.Length; i++)
        {
            pt[i] = ps[i];
        }

        path.AddLines(pt);

        if (fillColor.A > 0)
        {
            using var b = new SolidBrush(fillColor);
            g.FillPath(b, path);
        }

        if (strokeColor.A > 0 && weight > 0)
        {
            using var p = new Pen(strokeColor, weight);
            g.DrawPath(p, path);
        }
    }

    public void DrawRectangle(float x, float y, float w, float h, Color c, float weight = 1f)
    {
        using var p = new Pen(c, weight);
        g.DrawRectangle(p, x, y, w, h);
    }

    public void FillRectangle(RectangleF rect, Color c)
    {
        using var b = new SolidBrush(c);
        g.FillRectangle(b, rect);
    }

    public void DrawRoundedRectangle(RectangleF rect, Color bgColor, Color borderColor, PointF radius = new(), float strokeWidth = 1f)
    {
        var path = GetRoundRectanglePath(rect, (int)radius.X);
        using var bgBrush = new SolidBrush(bgColor);
        using var borderPen = new Pen(borderColor, strokeWidth);

        g.FillPath(bgBrush, path);
        g.DrawPath(borderPen, path);
    }

    #endregion


    // draw text
    #region Draw text

    public void DrawText(string text, Font font, float fontSize, Color color, RectangleF region)
    {
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, brush, region);
    }

    public SizeF MeasureText(string text, Font font, float fontSize, SizeF layoutArea)
    {
        return g.MeasureString(text, font, layoutArea);
    }

    #endregion


    // draw image
    #region Draw image

    /// <param name="interpolationMode">0 = NearestNeighbor; 1 = Linear</param>
    public void DrawImage(Bitmap bmp, RectangleF destRect, RectangleF srcRect, float opacity = 1f, int interpolationMode = 0)
    {
        if (interpolationMode == 0)
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
        }

        g.DrawImage(bmp, destRect, srcRect, GraphicsUnit.Pixel);
    }

    public void DrawMemoryBitmap(MemoryBitmap mb, int x, int y)
    {
        DrawMemoryBitmap(x, y, mb.Width, mb.Height, mb.Width * sizeof(uint), mb.BufferPtr, 0, mb.BufferSize);
    }

    public void DrawMemoryBitmap(int x, int y, int w, int h, int stride, IntPtr buf, int offset, int length)
    {
        using var bmp = new Bitmap(w, h, stride, PixelFormat.Format32bppArgb, buf);
        g.DrawImage(bmp, x, y);
    }

    #endregion


    // others
    #region Others

    public void Flush()
    {
    }


    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    /// <returns></returns>
    private static GraphicsPath GetRoundRectanglePath(RectangleF bounds, float radius)
    {
        var diameter = radius * 2;
        var size = new SizeF(diameter, diameter);
        var arc = new RectangleF(bounds.Location, size);
        var path = new GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // top left arc  
        path.AddArc(arc, 180, 90);

        // top right arc  
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // bottom right arc  
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // bottom left arc 
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            g?.Dispose();
        }
    }

    #endregion

}

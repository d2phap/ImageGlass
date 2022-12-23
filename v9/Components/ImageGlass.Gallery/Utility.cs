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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using System.Drawing.Drawing2D;
using System.Text;

namespace ImageGlass.Gallery;


/// <summary>
/// Contains utility functions.
/// </summary>
public static class Utility
{
    #region Graphics Utilities
    /// <summary>
    /// Checks the stream header if it matches with
    /// any of the supported image file types.
    /// </summary>
    /// <param name="stream">An open stream pointing to an image file.</param>
    /// <returns>true if the stream is an image file (BMP, TIFF, PNG, GIF, JPEG, WMF, EMF, ICO, CUR);
    /// false otherwise.</returns>
    internal static bool IsImage(Stream stream)
    {
        // Sniff some bytes from the start of the stream
        // and check against magic numbers of supported 
        // image file formats
        var header = new byte[8];
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Read(header, 0, header.Length) != header.Length)
            return false;

        // BMP
        var bmpHeader = Encoding.ASCII.GetString(header, 0, 2);
        if (bmpHeader == "BM") // BM - Windows bitmap
            return true;
        else if (bmpHeader == "BA") // BA - Bitmap array
            return true;
        else if (bmpHeader == "CI") // CI - Color Icon
            return true;
        else if (bmpHeader == "CP") // CP - Color Pointer
            return true;
        else if (bmpHeader == "IC") // IC - Icon
            return true;
        else if (bmpHeader == "PT") // PI - Pointer
            return true;

        // TIFF
        var tiffHeader = Encoding.ASCII.GetString(header, 0, 4);
        if (tiffHeader == "MM\x00\x2a") // Big-endian
            return true;
        else if (tiffHeader == "II\x2a\x00") // Little-endian
            return true;

        // PNG
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
            header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return true;

        // GIF
        string gifHeader = Encoding.ASCII.GetString(header, 0, 4);
        if (gifHeader == "GIF8")
            return true;

        // JPEG
        if (header[0] == 0xFF && header[1] == 0xD8)
            return true;

        // WMF
        if (header[0] == 0xD7 && header[1] == 0xCD && header[2] == 0xC6 && header[3] == 0x9A)
            return true;

        // EMF
        if (header[0] == 0x01 && header[1] == 0x00 && header[2] == 0x00 && header[3] == 0x00)
            return true;

        // Windows Icons
        if (header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00) // ICO
            return true;
        else if (header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x02 && header[3] == 0x00) // CUR
            return true;

        return false;
    }
    
    /// <summary>
    /// Gets the scaled size of an image required to fit
    /// in to the given size keeping the image aspect ratio.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="fit">The size to fit in to.</param>
    /// <returns>New image size.</returns>
    internal static Size GetSizedImageBounds(Image image, Size fit)
    {
        var f = Math.Max(1f * image.Width / fit.Width, 1f * image.Height / fit.Height);
        if (f < 1.0f) f = 1.0f; // Do not upsize small images

        var width = (int)Math.Round(image.Width / f);
        var height = (int)Math.Round(image.Height / f);

        return new Size(width, height);
    }

    /// <summary>
    /// Gets the bounding rectangle of an image required to fit
    /// in to the given rectangle keeping the image aspect ratio.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="fit">The rectangle to fit in to.</param>
    /// <param name="hAlign">Horizontal image aligment in percent.</param>
    /// <param name="vAlign">Vertical image aligment in percent.</param>
    /// <returns>New image size.</returns>
    public static Rectangle GetSizedImageBounds(Image image, Rectangle fit, float hAlign, float vAlign)
    {
        if (hAlign < 0 || hAlign > 100.0f)
            throw new ArgumentException("hAlign must be between 0.0 and 100.0 (inclusive).", nameof(hAlign));

        if (vAlign < 0 || vAlign > 100.0f)
            throw new ArgumentException("vAlign must be between 0.0 and 100.0 (inclusive).", nameof(vAlign));

        var scaled = GetSizedImageBounds(image, fit.Size);
        var x = fit.Left + (int)(hAlign / 100.0f * (fit.Width - scaled.Width));
        var y = fit.Top + (int)(vAlign / 100.0f * (fit.Height - scaled.Height));

        return new Rectangle(x, y, scaled.Width, scaled.Height);
    }

    /// <summary>
    /// Gets the bounding rectangle of an image required to fit
    /// in to the given rectangle keeping the image aspect ratio.
    /// The image will be centered in the fit box.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="fit">The rectangle to fit in to.</param>
    /// <returns>New image size.</returns>
    public static Rectangle GetSizedImageBounds(Image image, Rectangle fit)
    {
        return GetSizedImageBounds(image, fit, 50.0f, 50.0f);
    }

    /// <summary>
    /// Gets a path representing a rounded rectangle.
    /// </summary>
    private static GraphicsPath GetRoundedRectanglePath(int x, int y, int width, int height, int radius)
    {
        var path = new GraphicsPath();
        path.AddLine(x + radius, y, x + width - radius, y);

        if (radius > 0)
            path.AddArc(x + width - 2 * radius, y, 2 * radius, 2 * radius, 270.0f, 90.0f);
        path.AddLine(x + width, y + radius, x + width, y + height - radius);

        if (radius > 0)
            path.AddArc(x + width - 2 * radius, y + height - 2 * radius, 2 * radius, 2 * radius, 0.0f, 90.0f);
        path.AddLine(x + width - radius, y + height, x + radius, y + height);

        if (radius > 0)
            path.AddArc(x, y + height - 2 * radius, 2 * radius, 2 * radius, 90.0f, 90.0f);
        path.AddLine(x, y + height - radius, x, y + radius);

        if (radius > 0)
            path.AddArc(x, y, 2 * radius, 2 * radius, 180.0f, 90.0f);

        return path;
    }

    /// <summary>
    /// Fills the interior of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="brush">The brush to use to fill the rectangle.</param>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
    /// <param name="width">Width of the rectangle to draw.</param>
    /// <param name="height">Height of the rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void FillRoundedRectangle(Graphics graphics, Brush brush, int x, int y, int width, int height, int radius)
    {
        using GraphicsPath path = GetRoundedRectanglePath(x, y, width, height, radius);
        graphics.FillPath(brush, path);
    }

    /// <summary>
    /// Draws the outline of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="pen">The pen to use to draw the rectangle.</param>
    /// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
    /// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
    /// <param name="width">Width of the rectangle to draw.</param>
    /// <param name="height">Height of the rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void DrawRoundedRectangle(Graphics graphics, Pen pen, int x, int y, int width, int height, int radius)
    {
        using GraphicsPath path = GetRoundedRectanglePath(x, y, width, height, radius);
        graphics.DrawPath(pen, path);
    }

    #endregion


}

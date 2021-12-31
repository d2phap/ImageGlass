
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
        byte[] header = new byte[8];
        stream.Seek(0, SeekOrigin.Begin);
        if (stream.Read(header, 0, header.Length) != header.Length)
            return false;

        // BMP
        string bmpHeader = Encoding.ASCII.GetString(header, 0, 2);
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
        string tiffHeader = Encoding.ASCII.GetString(header, 0, 4);
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
    /// Draws the given caption and text inside the given rectangle.
    /// </summary>
    internal static int DrawStringPair(Graphics g, Rectangle r, string caption, string text, Font font, Brush captionBrush, Brush textBrush)
    {
        using (StringFormat sf = new StringFormat())
        {
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Near;
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.FormatFlags = StringFormatFlags.NoWrap;

            SizeF szc = g.MeasureString(caption, font, r.Size, sf);
            int y = (int)szc.Height;
            if (szc.Width > r.Width) szc.Width = r.Width;
            Rectangle txrect = new Rectangle(r.Location, Size.Ceiling(szc));
            g.DrawString(caption, font, captionBrush, txrect, sf);
            txrect.X += txrect.Width;
            txrect.Width = r.Width;
            if (txrect.X < r.Right)
            {
                SizeF szt = g.MeasureString(text, font, r.Size, sf);
                y = Math.Max(y, (int)szt.Height);
                txrect = Rectangle.Intersect(r, txrect);
                g.DrawString(text, font, textBrush, txrect, sf);
            }

            return y;
        }
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
        float f = System.Math.Max((float)image.Width / (float)fit.Width, (float)image.Height / (float)fit.Height);
        if (f < 1.0f) f = 1.0f; // Do not upsize small images
        int width = (int)System.Math.Round((float)image.Width / f);
        int height = (int)System.Math.Round((float)image.Height / f);
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
            throw new ArgumentException("hAlign must be between 0.0 and 100.0 (inclusive).", "hAlign");
        if (vAlign < 0 || vAlign > 100.0f)
            throw new ArgumentException("vAlign must be between 0.0 and 100.0 (inclusive).", "vAlign");
        Size scaled = GetSizedImageBounds(image, fit.Size);
        int x = fit.Left + (int)(hAlign / 100.0f * (float)(fit.Width - scaled.Width));
        int y = fit.Top + (int)(vAlign / 100.0f * (float)(fit.Height - scaled.Height));

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
        GraphicsPath path = new GraphicsPath();
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
        using (GraphicsPath path = GetRoundedRectanglePath(x, y, width, height, radius))
        {
            graphics.FillPath(brush, path);
        }
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
    public static void FillRoundedRectangle(Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
    {
        FillRoundedRectangle(graphics, brush, (int)x, (int)y, (int)width, (int)height, (int)radius);
    }

    /// <summary>
    /// Fills the interior of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="brush">The brush to use to fill the rectangle.</param>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle rect, int radius)
    {
        FillRoundedRectangle(graphics, brush, rect.Left, rect.Top, rect.Width, rect.Height, radius);
    }

    /// <summary>
    /// Fills the interior of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="brush">The brush to use to fill the rectangle.</param>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void FillRoundedRectangle(Graphics graphics, Brush brush, RectangleF rect, float radius)
    {
        FillRoundedRectangle(graphics, brush, (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height, (int)radius);
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
        using (GraphicsPath path = GetRoundedRectanglePath(x, y, width, height, radius))
        {
            graphics.DrawPath(pen, path);
        }
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
    public static void DrawRoundedRectangle(Graphics graphics, Pen pen, float x, float y, float width, float height, float radius)
    {
        DrawRoundedRectangle(graphics, pen, (int)x, (int)y, (int)width, (int)height, (int)radius);
    }

    /// <summary>
    /// Draws the outline of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="pen">The pen to use to draw the rectangle.</param>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius)
    {
        DrawRoundedRectangle(graphics, pen, rect.Left, rect.Top, rect.Width, rect.Height, radius);
    }

    /// <summary>
    /// Draws the outline of a rounded rectangle.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    /// <param name="pen">The pen to use to draw the rectangle.</param>
    /// <param name="rect">The rectangle to draw.</param>
    /// <param name="radius">The radius of rounded corners.</param>
    public static void DrawRoundedRectangle(Graphics graphics, Pen pen, RectangleF rect, float radius)
    {
        DrawRoundedRectangle(graphics, pen, (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height, (int)radius);
    }

    #endregion


}


using System.Drawing.Drawing2D;

namespace D2DLibExport;

public class GDIGraphics : IHybridGraphics
{
    internal System.Drawing.Graphics g;

    public GDIGraphics(System.Drawing.Graphics g)
    {
        this.g = g;
    }

    public bool SmoothingMode
    {
        get { return g.SmoothingMode == System.Drawing.Drawing2D.SmoothingMode.AntiAlias; }
        set { g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; }
    }

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
        using (var p = new Pen(c, weight))
        {
            g.DrawLine(p, x1, y1, x2, y2);
        }
    }

    public void DrawRectangle(float x, float y, float w, float h, Color c, float weight = 1f)
    {
        using (var p = new Pen(c, weight))
        {
            g.DrawRectangle(p, x, y, w, h);
        }
    }

    public void DrawRoundedRectangle(RectangleF rect, Color bgColor, Color borderColor, PointF radius = new(), float strokeWidth = 1f)
    {
        var path = GetRoundRectanglePath(rect, (int)radius.X);
        using var bgBrush = new SolidBrush(bgColor);
        using var borderPen = new Pen(borderColor, strokeWidth);

        g.FillPath(bgBrush, path);
        g.DrawPath(borderPen, path);
    }

    public void FillRectangle(RectangleF rect, Color c)
    {
        using var b = new SolidBrush(c);
        g.FillRectangle(b, rect);
    }

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
        PointF[] pt = new PointF[ps.Length];

        for (int i = 0; i < ps.Length; i++)
        {
            pt[i] = ps[i];
        }

        path.AddLines(pt);

        if (fillColor.A > 0)
        {
            using (var b = new SolidBrush(fillColor))
            {
                g.FillPath(b, path);
            }
        }

        if (strokeColor.A > 0 && weight > 0)
        {
            using var p = new Pen(strokeColor, weight);
            g.DrawPath(p, path);
        }
    }

    public SizeF MeasureText(string text, Font font, float fontSize, SizeF layoutArea)
    {
        return g.MeasureString(text, font, layoutArea);
    }

    public void DrawText(string text, Font font, float fontSize, Color color, RectangleF region)
    {
        using var brush = new SolidBrush(color);
        g.DrawString(text, font, brush, region);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="destRect"></param>
    /// <param name="srcRect"></param>
    /// <param name="opacity"></param>
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
        using var bmp = new Bitmap(w, h, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, buf);
        g.DrawImage(bmp, 0, 0);
    }

    public void Flush()
    {
    }



    /// <summary>
    /// Gets rounded rectangle graphic path
    /// </summary>
    /// <param name="bounds">Input rectangle</param>
    /// <param name="radius">Border radius</param>
    /// <returns></returns>
    private GraphicsPath GetRoundRectanglePath(RectangleF bounds, float radius)
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

}

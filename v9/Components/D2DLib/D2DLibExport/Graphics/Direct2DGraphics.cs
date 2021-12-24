using unvell.D2DLib;

namespace D2DLibExport;
public class Direct2DGraphics : IHybridGraphics
{
    internal D2DGraphics g;

    public Direct2DGraphics(D2DGraphics g)
    {
        this.g = g;
    }

    public bool SmoothingMode
    {
        get { return g.Antialias; }
        set { g.Antialias = value; }
    }

    public void DrawLine(Point p1, Point p2, Color c, float weight = 1)
    {
        DrawLine(p1.X, p1.Y, p2.X, p2.Y, c, weight);
    }

    public void DrawLine(PointF p1, PointF p2, Color c, float weight = 1)
    {
        DrawLine(p1.X, p1.Y, p2.X, p2.Y, c, weight);
    }

    public void DrawLine(float x1, float y1, float x2, float y2, Color c, float weight = 1f)
    {
        g.DrawLine(x1, y1, x2, y2, D2DColor.FromGDIColor(c), weight);
    }

    public void DrawRectangle(float x, float y, float w, float h, Color c, float weight = 1f)
    {
        g.DrawRectangle(x, y, w, h, D2DColor.FromGDIColor(c), weight);
    }

    public void DrawRoundedRectangle(RectangleF rect, Color bgColor, Color borderColor, PointF radius = new(), float strokeWidth = 1f)
    {
        var bgRect = new D2DRoundedRect()
        {
            radiusX = radius.X,
            radiusY = radius.Y,
            rect = rect,
        };

        g.DrawRoundedRectangle(bgRect, D2DColor.FromGDIColor(borderColor), D2DColor.FromGDIColor(bgColor), strokeWidth);
    }

    public void FillRectangle(RectangleF rect, Color c)
    {
        g.FillRectangle(rect, D2DColor.FromGDIColor(c));
    }

    public void DrawEllipse(RectangleF rect, Color c, float weight = 1f)
    {
        g.DrawEllipse(rect.X, rect.Y, rect.Width, rect.Height, D2DColor.FromGDIColor(c), weight);
    }

    public void FillEllipse(RectangleF rect, Color c)
    {
        g.FillEllipse(rect.X, rect.Y, rect.Width, rect.Height, D2DColor.FromGDIColor(c));
    }

    public void DrawPolygon(PointF[] ps, Color strokeColor, float weight, Color fillColor)
    {
        D2DPoint[] pt = new D2DPoint[ps.Length];

        for (int i = 0; i < ps.Length; i++)
        {
            pt[i] = new D2DPoint(ps[i].X, ps[i].Y);
        }

        g.DrawPolygon(pt, D2DColor.FromGDIColor(strokeColor), weight, D2DDashStyle.Solid, D2DColor.FromGDIColor(fillColor));
    }

    public SizeF MeasureText(string text, Font font, float fontSize, SizeF layoutArea)
    {
        return g.MeasureText(text, font.Name, fontSize, layoutArea);
    }

    public void DrawText(string text, Font font, float fontSize, Color c, RectangleF region)
    {
        g.DrawTextCenter(text, D2DColor.FromGDIColor(c), font.Name, fontSize, region);
    }

    /// <summary>
    /// Draw image
    /// </summary>
    /// <param name="bmp"></param>
    /// <param name="destRect"></param>
    /// <param name="srcRect"></param>
    /// <param name="opacity"></param>
    /// <param name="interpolationMode">0 = NearestNeighbor; 1 = Linear</param>
    public void DrawImage(Bitmap bmp, RectangleF destRect, RectangleF srcRect, float opacity = 1f, int interpolationMode = 0)
    {
        g.DrawGDIBitmap(bmp.GetHbitmap(), destRect, srcRect, opacity, true, (D2DBitmapInterpolationMode)interpolationMode);
    }

    /// <summary>
    /// Draw image using Direct2D
    /// </summary>
    /// <param name="d2dBmp"></param>
    /// <param name="destRect"></param>
    /// <param name="srcRect"></param>
    /// <param name="opacity"></param>
    /// <param name="interpolationMode">0 = NearestNeighbor; 1 = Linear</param>
    public void DrawImage(D2DBitmap d2dBmp, RectangleF destRect, RectangleF srcRect, float opacity = 1f, int interpolationMode = 0)
    {
        g.DrawBitmap(d2dBmp, destRect, srcRect, opacity, (D2DBitmapInterpolationMode)interpolationMode);
    }

    public void DrawMemoryBitmap(MemoryBitmap bmp, int x, int y)
    {
        DrawMemoryBitmap(x, y, bmp.Width, bmp.Height, bmp.Width * sizeof(uint), bmp.BufferPtr, 0, bmp.BufferSize);
    }

    public void DrawMemoryBitmap(int x, int y, int w, int h, int stride, IntPtr buf, int offset, int length)
    {
        using (var bmp = g.Device.CreateBitmapFromMemory((uint)w, (uint)h, (uint)stride, buf, (uint)offset, (uint)length))
        {
            g.DrawBitmap(bmp, new D2DRect(x, y, w, h));
        }
    }

    public void Flush()
    {
        g.Flush();
    }

    
}
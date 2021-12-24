
namespace D2DLibExport;


public interface IHybridGraphics
{
	bool SmoothingMode { get; set; }
	void DrawLine(Point p1, Point p2, Color c, float weight = 1f);
	void DrawLine(float x1, float y1, float x2, float y2, Color c, float weight = 1f);
	void DrawLine(PointF p1, PointF p2, Color c, float weight = 1f);
	void DrawRectangle(float x, float y, float w, float h, Color c, float weight = 1f);
	void DrawRoundedRectangle(RectangleF rect, Color bgColor, Color borderColor, PointF radius = new(), float strokeWidth = 1f);
	void FillRectangle(RectangleF rect, Color c);
	void DrawEllipse(RectangleF rect, Color c, float weight = 1f);
	void FillEllipse(RectangleF rect, Color c);
	SizeF MeasureText(string text, Font font, float fontSize, SizeF layoutArea);
	void DrawText(string text, Font font, float fontSize, Color c, RectangleF region);
	void DrawImage(Bitmap bmp, RectangleF destRect, RectangleF srcRect, float opacity = 1f, int interpolationMode = 0);
	void DrawMemoryBitmap(int x, int y, int w, int h, int stride, IntPtr buf, int offset, int length);
	void DrawMemoryBitmap(MemoryBitmap mb, int x, int y);
	void DrawPolygon(PointF[] ps, Color strokeColor, float weight, Color fillColor);
	void Flush();
}

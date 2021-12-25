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

namespace ImageGlass.Base.HybridGraphics;


/// <summary>
/// An interface for both Direct2D and GDI+ graphics
/// </summary>
public interface IHybridGraphics : IDisposable
{
    bool UseAntialias { get; set; }


    // draw lines
    void DrawLine(float x1, float y1, float x2, float y2, Color c, float weight = 1f);
    void DrawLine(Point p1, Point p2, Color c, float weight = 1f);
    void DrawLine(PointF p1, PointF p2, Color c, float weight = 1f);


    // draw shapes
    void DrawEllipse(RectangleF rect, Color c, float weight = 1f);
    void FillEllipse(RectangleF rect, Color c);

    void DrawPolygon(PointF[] ps, Color strokeColor, float weight, Color fillColor);
    void DrawRectangle(float x, float y, float w, float h, Color c, float weight = 1f);
    void FillRectangle(RectangleF rect, Color c);
    void DrawRoundedRectangle(RectangleF rect, Color bgColor, Color borderColor, PointF radius = new(), float strokeWidth = 1f);


    // draw text
    void DrawText(string text, Font font, float fontSize, Color c, RectangleF region);
    SizeF MeasureText(string text, Font font, float fontSize, SizeF layoutArea);


    // draw image
    void DrawMemoryBitmap(MemoryBitmap mb, int x, int y);
    void DrawMemoryBitmap(int x, int y, int w, int h, int stride, IntPtr buf, int offset, int length);
    void DrawImage(Bitmap bmp, RectangleF destRect, RectangleF srcRect, float opacity = 1f, int interpolationMode = 0);
    

    // others
    void Flush();

}

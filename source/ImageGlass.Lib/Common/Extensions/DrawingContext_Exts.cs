/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia;
using Avalonia.Media;
using System.Globalization;

namespace ImageGlass.Common.Extensions;

public static class DrawingContext_Exts
{
    extension(DrawingContext g)
    {


        #region Draw/Fill ellipse

        /// <summary>
        /// Draws the outline and paints the interior of the specified ellipse.
        /// </summary>
        public void DrawEllipseEx(double x, double y, float radius, Color? borderColor, Color? fillColor, float strokeWidth = 1)
        {
            if (radius < 0) return;

            var rect = new Rect(x - radius, y - radius, radius * 2, radius * 2);

            g.DrawEllipseEx(rect, borderColor, fillColor, strokeWidth);
        }


        /// <summary>
        /// Draws the outline and paints the interior of the specified ellipse.
        /// </summary>
        public void DrawEllipseEx(Rect rect, Color? borderColor, Color? fillColor, float strokeWidth = 1)
        {
            if (rect.IsEmpty) return;

            g.DrawEllipseEx(rect.X, rect.Y, rect.Width, rect.Height, borderColor, fillColor, strokeWidth);
        }


        /// <summary>
        /// Draws the outline and paints the interior of the specified ellipse.
        /// </summary>
        public void DrawEllipseEx(double x, double y, double width, double height, Color? borderColor, Color? fillColor, float strokeWidth = 1)
        {
            if (width < 0 || height < 0) return;

            var brushFill = fillColor?.ToBrush();
            var penStroke = new Pen(borderColor?.ToBrush(), strokeWidth);

            g.DrawEllipse(brushFill, penStroke, new Rect(x, y, width, height));
        }

        #endregion // Draw/Fill ellipse



        #region Draw/Fill Rectangle

        /// <summary>
        /// Draws the outline and paints the interior of the specified rectangle.
        /// </summary>
        public void DrawRectangleEx(double x, double y, double width, double height, float radius, Color? borderColor, Color? fillColor = null, float strokeWidth = 1)
        {
            if (width < 0 || height < 0) return;

            g.DrawRectangleEx(new Rect(x, y, width, height), radius, borderColor, fillColor, strokeWidth);
        }


        /// <summary>
        /// Draws the outline and paints the interior of the specified rectangle.
        /// </summary>
        public void DrawRectangleEx(Rect rect, float radius, Color? borderColor, Color? fillColor = null, float strokeWidth = 1)
        {
            if (rect.IsEmpty) return;

            var brushFill = fillColor?.ToBrush();
            var penStroke = new Pen(borderColor?.ToBrush(), strokeWidth);


            if (radius > 0)
            {
                var roundedRect = new RoundedRect(rect, new CornerRadius(radius));
                g.DrawRectangle(brushFill, penStroke, roundedRect);
                return;
            }

            g.DrawRectangle(brushFill, penStroke, rect);
        }

        #endregion // Draw/Fill Rectangle



        #region Draw lines

        /// <summary>
        /// Draws a line between the specified points using the specified stroke style.
        /// </summary>
        public void DrawLineEx(double x1, double y1, double x2, double y2, Color c, float strokeWidth = 1)
        {
            g.DrawLineEx(new(x1, y1), new(x2, y2), c, strokeWidth);
        }

        /// <summary>
        /// Draws a line between the specified points using the specified stroke style.
        /// </summary>
        public void DrawLineEx(Point p1, Point p2, Color? c, float strokeWidth = 1)
        {
            if (p1 == p2 || strokeWidth <= 0) return;

            var penStroke = new Pen(c?.ToBrush(), strokeWidth);

            g.DrawLine(penStroke, p1, p2);
        }

        #endregion // Draw lines



        #region Draw / Measure text

        /// <summary>
        /// Draws the specified text using the format information provided.
        /// </summary>
        public void DrawTextEx(string text, FontFamily fontFamily, double fontSize, double x, double y, Color? c, double? textDpi = null, TextAlignment hAlign = TextAlignment.Start, bool isBold = false, bool isItalic = false)
        {
            var rect = new Rect(x, y, int.MaxValue, int.MaxValue);

            g.DrawTextEx(text, fontFamily, fontSize, rect, c, textDpi, hAlign, isBold, isItalic);
        }


        /// <summary>
        /// Draws the specified text using the format information provided.
        /// </summary>
        public void DrawTextEx(string text, FontFamily fontFamily, double fontSize, Rect rect, Color? c, double? textDpi = null, TextAlignment hAlign = TextAlignment.Start, bool isBold = false, bool isItalic = false)
        {
            var brushText = c?.ToBrush();
            var typeFace = new Typeface(fontFamily,
                isItalic ? FontStyle.Italic : FontStyle.Normal,
                isBold ? FontWeight.Bold : FontWeight.Normal);

            var ftext = new FormattedText(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeFace,
                fontSize,
                brushText)
            {
                TextAlignment = hAlign,
            };

            g.DrawText(ftext, rect.Position);
        }


        /// <summary>
        /// Measure text.
        /// </summary>
        public Size MeasureTextEx(string text, FontFamily fontFamily, double fontSize, bool isBold = false, bool isItalic = false)
        {
            var typeFace = new Typeface(fontFamily,
                isItalic ? FontStyle.Italic : FontStyle.Normal,
                isBold ? FontWeight.Bold : FontWeight.Normal);

            var ftext = new FormattedText(text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeFace,
                fontSize,
                null);

            var size = new Size(ftext.Width, ftext.Height);
            return size;
        }

        #endregion // Draw / Measure text



        #region Draw / Fill Geometry

        /// <summary>
        /// Draw geometry.
        /// </summary>
        public void DrawGeometryEx(Geometry? geo, Color? borderColor, Color? fillColor = null, float strokeWidth = 1f)
        {
            if (geo is null) return;

            var brushFill = fillColor?.ToBrush();
            var penStroke = new Pen(borderColor?.ToBrush(), strokeWidth);

            g.DrawGeometry(brushFill, penStroke, geo);
        }


        /// <summary>
        /// Get geometry from a combined 2 rectangles.
        /// </summary>
        public Geometry GetCombinedRectGeometryEx(
            Rect rect1,
            Rect rect2,
            float rect1Radius, float rect2Radius,
            GeometryCombineMode combineMode)
        {
            // create rounded rectangle geometries
            var geoRect1 = new RectangleGeometry(rect1, rect1Radius, rect1Radius);
            var geoRect2 = new RectangleGeometry(rect2, rect2Radius, rect2Radius);

            // combine them
            var geo = Geometry.Combine(geoRect1, geoRect2, combineMode);

            return geo;
        }





        #endregion // Draw / Fill Geometry

    }
}

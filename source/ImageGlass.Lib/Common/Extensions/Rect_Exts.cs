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
using SkiaSharp;
using System;

namespace ImageGlass.Common.Extensions;

public static class Rect_Exts
{
    private static readonly string _delimiter = ";";

    extension(Rect rect)
    {
        /// <summary>
        /// Checks if the specified rectangle has no area.
        /// </summary>
        public bool IsEmpty => rect.Size.IsEmpty;


        /// <summary>
        /// Ensures the rectangle location and size values are finite numbers.
        /// </summary>
        public Rect Normalize()
        {
            var x = double.IsFinite(rect.X) ? rect.X : 0;
            var y = double.IsFinite(rect.Y) ? rect.Y : 0;
            var w = double.IsFinite(rect.Width) ? rect.Width : 0;
            var h = double.IsFinite(rect.Height) ? rect.Height : 0;

            return new Rect(x, y, w, h);
        }


        /// <summary>
        /// Determines if two rectangles overlap in a 2D space.
        /// </summary>
        public bool IntersectsWith(Rect rect2)
        {
            if (rect.Width < 0f || rect2.Width < 0f) return false;

            if (rect2.X <= rect.X + rect.Width
                && rect2.X + rect2.Width >= rect.X
                && rect2.Y <= rect.Y + rect.Height)
            {
                return rect2.Y + rect2.Height >= rect.Y;
            }

            return false;
        }


        /// <summary>
        /// Calculates the intersection of a rectangle with a specified width and height.
        /// </summary>
        public Rect GetIntersection(double width, double height)
        {
            return rect.GetIntersection(new Rect(0, 0, width, height));
        }


        /// <summary>
        /// Calculates the intersection of two rectangles and returns the intersected area.
        /// </summary>
        public Rect GetIntersection(Rect rect2)
        {
            if (!rect.IntersectsWith(rect2)) return rect;

            var x = Math.Max(rect.X, rect2.X);
            var y = Math.Max(rect.Y, rect2.Y);
            var w = Math.Max(Math.Min(rect.X + rect.Width, rect2.X + rect2.Width) - x, 0);
            var h = Math.Max(Math.Min(rect.Y + rect.Height, rect2.Y + rect2.Height) - y, 0);

            return new Rect(x, y, w, h);
        }


        /// <summary>
        /// Centers another rectangle in this rectangle.
        /// </summary>
        public Rect CenterRectEx(Rect rect2, bool limitRect1Size = false)
        {
            var centeredRect = rect.CenterRect(rect2);
            if (!limitRect1Size) return centeredRect;


            var x = Math.Max(rect.X, centeredRect.X);
            var y = Math.Max(rect.Y, centeredRect.Y);
            var width = Math.Min(rect.Width, rect2.Width);
            var height = Math.Min(rect.Height, rect2.Height);

            return new Rect(x, y, width, height);
        }


        /// <summary>
        /// Converts the given rectangle to double array.
        /// </summary>
        public double[] ToArray()
        {
            return [rect.X, rect.Y, rect.Width, rect.Height];
        }


        /// <summary>
        /// Converts the rectangle to delimiter string.
        /// Example: <c>10;20;500;400</c>.
        /// </summary>
        public string ToStringDelimiter()
        {
            var str = string.Join(_delimiter, rect.ToArray());

            return str;
        }


        /// <summary>
        /// Converts to <see cref="SKRectI"/>.
        /// </summary>
        public SKRectI ToSKRectI()
        {
            return new SKRectI((int)rect.X, (int)rect.Y, (int)rect.Right, (int)rect.Bottom);
        }

    }


}

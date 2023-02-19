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

namespace ImageGlass.Base;

public static class ControlExtensions
{
    /// <summary>
    /// Scales the given number to the current control's DPI.
    /// </summary>
    public static T ScaleToDpi<T>(this Control control, T number)
    {
        float floatValue;
        try
        {
            var fNum = (float?)Convert.ChangeType(number, typeof(float));
            if (fNum == null) return number;

            floatValue = fNum.Value;
        }
        catch { return number; }


        var dpiScale = control.DeviceDpi / 96f;
        var scaledValue = floatValue * dpiScale;
        var type = typeof(T);

        return (T)Convert.ChangeType(scaledValue, type);
    }


    /// <summary>
    /// Scales the given size to the current control's DPI.
    /// </summary>
    public static SizeF ScaleToDpi(this Control control, SizeF size)
    {
        var w = control.ScaleToDpi(size.Width);
        var h = control.ScaleToDpi(size.Height);

        return new SizeF(w, h);
    }


    /// <summary>
    /// Scales the given size to the current control's DPI.
    /// </summary>
    public static Size ScaleToDpi(this Control control, Size size)
    {
        var w = control.ScaleToDpi(size.Width);
        var h = control.ScaleToDpi(size.Height);

        return new Size(w, h);
    }


    /// <summary>
    /// Scales the given padding to the current control's DPI.
    /// </summary>
    public static Padding ScaleToDpi(this Control control, Padding padding)
    {
        return new Padding(
            control.ScaleToDpi(padding.Left),
            control.ScaleToDpi(padding.Top),
            control.ScaleToDpi(padding.Right),
            control.ScaleToDpi(padding.Bottom));
    }


    /// <summary>
    /// Scales the given rectangle to the current control's DPI.
    /// </summary>
    public static Rectangle ScaleToDpi(this Control control, Rectangle rect)
    {
        return new Rectangle(rect.X, rect.Y,
            control.ScaleToDpi(rect.Width),
            control.ScaleToDpi(rect.Height));
    }


    /// <summary>
    /// Scales the given rectangle to the current control's DPI.
    /// </summary>
    public static RectangleF ScaleToDpi(this Control control, RectangleF rect)
    {
        return new RectangleF(rect.X, rect.Y,
            control.ScaleToDpi(rect.Width),
            control.ScaleToDpi(rect.Height));
    }

}

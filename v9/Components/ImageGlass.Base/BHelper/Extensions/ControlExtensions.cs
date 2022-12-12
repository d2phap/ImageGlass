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

using System;

namespace ImageGlass.Base;

public static class ControlExtensions
{
    /// <summary>
    /// Scales the given number according to the current control's DPI.
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
}

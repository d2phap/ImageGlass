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
namespace ImageGlass.Base;

public static partial class BHelper
{

    /// <summary>
    /// Gets menu item border radius
    /// </summary>
    /// <param name="itemHeight"></param>
    /// <returns></returns>
    public static int GetItemBorderRadius(int itemHeight, int defaultItemHeight)
    {
        if (IsOS(WindowsOS.Win10))
        {
            return 0;
        }

        var radius = (int)(itemHeight * 1.0f / defaultItemHeight * 3);

        // min border radius = 4
        return Math.Max(radius, 4);
    }

}

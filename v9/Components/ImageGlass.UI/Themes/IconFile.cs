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

using ImageGlass.Base;

namespace ImageGlass.UI;

public class IconFile
{
    /// <summary>
    /// Gets the SVG file path from the folder <see cref="Dir.Icons"/>.
    /// </summary>
    public static string GetFullPath(IconName svgIcon)
    {
        if (svgIcon == IconName.None) return string.Empty;

        var name = Enum.GetName(typeof(IconName), svgIcon);
        return App.StartUpDir(Dir.Icons, $"{name}.svg");
    }


    /// <summary>
    /// Reads svg file and returns the content.
    /// </summary>
    public static async Task<string> ReadIconTextAsync(IconName svgIcon)
    {
        return await File.ReadAllTextAsync(GetFullPath(svgIcon));
    }
}


/// <summary>
/// Represents name of the SVG icon in the <see cref="Dir.Icons"/> folder.
/// </summary>
/// <remarks>
/// <c>Note**:</c> The enum name is also icon file name (without extension).
/// </remarks>
public enum IconName
{
    /// <summary>
    /// No icon.
    /// </summary>
    None,

    Copy,
    ResetSelection,
    Selection,
    Setting,

    Delete,
    Edit,
    Moon,
    Sun,

    ArrowUp,
    ArrowDown,
    ArrowExchange,
}

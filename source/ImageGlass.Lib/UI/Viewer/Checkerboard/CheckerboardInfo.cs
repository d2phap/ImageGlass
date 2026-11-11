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
using ImageGlass.Common.Extensions;

namespace ImageGlass.UI.Viewer.Checkerboard;


/// <summary>
/// Holds information about a checkerboard.
/// </summary>
internal class CheckerboardInfo
{
    /// <summary>
    /// Represents the current mode of the checkerboard.
    /// </summary>
    public CheckerboardType Mode { get; set; } = CheckerboardType.None;


    /// <summary>
    /// Represents the size of checkerboard.
    /// </summary>
    public Size Size { get; set; } = new Size(10, 10);


    /// <summary>
    /// Represents the first color of the checkerboard.
    /// </summary>
    public Color Color1 { get; set; } = Colors.Black.WithAlpha(25);


    /// <summary>
    /// Represents the second color of the checkerboard.
    /// </summary>
    public Color Color2 { get; set; } = Colors.White.WithAlpha(25);

}

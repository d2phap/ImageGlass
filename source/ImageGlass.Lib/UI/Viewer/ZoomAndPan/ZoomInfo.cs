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

namespace ImageGlass.UI.Viewer.ZoomAndPan;

/// <summary>
/// Holds information about zooming feature.
/// </summary>
internal class ZoomInfo
{
    public const double MAX_ZOOM_SPEED = 500;


    /// <summary>
    /// Represents the zoom mode for a component.
    /// </summary>
    public ZoomMode Mode { get; set; } = ZoomMode.AutoZoom;


    /// <summary>
    /// The zoom speed, initialized to 0. Min is -500, max is 500.
    /// </summary>
    public double Speed { get; set; } = 0f;


    /// <summary>
    /// The minimum zoom factor, initialized to 0.01 (1%).
    /// </summary>
    public double Min { get; set; } = 0.01d; // 1%


    /// <summary>
    /// The maximum value, defaulting to 100 (10,000%).
    /// </summary>
    public double Max { get; set; } = 100d; // 10_000%


    /// <summary>
    /// The array of zoom levels.
    /// </summary>
    public double[] Levels { get; set; } = [];


    /// <summary>
    /// Represents a zoom factor with a default value of 1 (100%).
    /// </summary>
    public double Factor { get; set; } = 1f;


    /// <summary>
    /// Represents an old zoom factor with a default value of 1 (100%).
    /// </summary>
    public double OldFactor { get; set; } = 1f;


    /// <summary>
    /// Indicates whether the zoom operation is done manually (not by changing <see cref="Mode"/>).
    /// </summary>
    public bool IsManual { get; set; } = false;


    /// <summary>
    /// Represents the last known pointer position or zoom/pan anchor point.
    /// </summary>
    public Point ZoomedPoint { get; set; } = new();

}

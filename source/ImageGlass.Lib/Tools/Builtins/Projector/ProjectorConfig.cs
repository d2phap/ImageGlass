/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.Common.Types.JsonTypeConverters;
using ImageGlass.UI.Viewer;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Tools;


[JsonSerializable(typeof(ProjectorConfig))]
public partial class ProjectorConfigJsonContext : JsonSerializerContext { }


/// <summary>
/// How a projector window covers its screen.
/// </summary>
public enum ProjectorWindowMode
{
    FullScreen,
    Maximized,
    Normal,
}


/// <summary>
/// Provides settings for the Projector tool.
/// </summary>
public sealed class ProjectorConfig
{
    /// <summary>
    /// Gets, sets whether projectors follow the zoom, pan and transforms of the main viewer.
    /// </summary>
    public bool EnableViewSync { get; set; } = true;


    /// <summary>
    /// Gets, sets how projectors fit the photo while they do not follow the main viewer.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ZoomMode>))]
    public ZoomMode ZoomMode { get; set; } = ZoomMode.AutoZoom;


    /// <summary>
    /// Gets, sets where each projector was last shown, by projector number, so it opens there again.
    /// </summary>
    public List<ProjectorPlacement> Placements { get; set; } = [];
}


/// <summary>
/// The screen, window mode and background a projector was last shown with.
/// </summary>
public sealed class ProjectorPlacement
{
    /// <summary>
    /// Gets, sets the name the OS gives the screen.
    /// </summary>
    public string ScreenName { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets the left edge of the screen in desktop pixels, telling apart screens of the same name.
    /// </summary>
    public int ScreenX { get; set; }


    /// <summary>
    /// Gets, sets the top edge of the screen in desktop pixels.
    /// </summary>
    public int ScreenY { get; set; }


    /// <summary>
    /// Gets, sets how the projector prefers to cover a screen; over the main window it opens as a normal window.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ProjectorWindowMode>))]
    public ProjectorWindowMode WindowMode { get; set; } = ProjectorWindowMode.FullScreen;


    /// <summary>
    /// Gets, sets the background color in hex; empty follows the slideshow background color.
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;
}

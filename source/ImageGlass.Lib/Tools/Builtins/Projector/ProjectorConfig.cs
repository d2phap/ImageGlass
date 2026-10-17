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
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Tools;


[JsonSerializable(typeof(ProjectorConfig))]
public partial class ProjectorConfigJsonContext : JsonSerializerContext { }


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
    /// Gets, sets whether projectors highlight where the cursor is on the main viewer.
    /// </summary>
    public bool ShowPointer { get; set; }


    /// <summary>
    /// Gets, sets the background color of each projector in hex, by projector number; empty follows the slideshow background color.
    /// </summary>
    public List<string> BackgroundColors { get; set; } = [];
}

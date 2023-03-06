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
using ImageGlass.Settings;

namespace ImageGlass.UI;

public interface IToolConfig
{
    /// <summary>
    /// Gets tool id.
    /// </summary>
    string ToolId { get; init; }


    /// <summary>
    /// Loads and parse tool's config from app's <see cref="Config.ToolSettings"/>.
    /// </summary>
    void LoadFromAppConfig();


    /// <summary>
    /// Saves the tool's config to app's <see cref="Config.ToolSettings"/>.
    /// </summary>
    void SaveToAppConfig();

}

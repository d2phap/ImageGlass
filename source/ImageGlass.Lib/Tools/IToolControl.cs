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
using ImageGlass.Common.Types;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

/// <summary>
/// Interface for hosted tools that are displayed in <see cref="ToolHostControl"/>.
/// Extends <see cref="ITool"/> with hosted-UI-specific members.
/// </summary>
public interface IToolControl : ITool
{
    // Hosted tools are always hosted
    bool ITool.IsHosted => true;


    /// <summary>
    /// Gets the localized tool name, shown as a watermark in <see cref="ToolHostControl"/>.
    /// </summary>
    string ToolName { get; }


    /// <summary>
    /// Gets the icon shown beside <see cref="ToolName"/>, or <c>null</c> for none.
    /// </summary>
    ResxIconId? ToolIcon => null;


    /// <summary>
    /// Gets the value indicates that the tool contains settings UI,
    /// that can be open with <see cref="ShowSettingsWindowAsync"/>.
    /// </summary>
    bool HasSettingsUI { get; }


    /// <summary>
    /// Gets the value indicates that the tool is a Pro feature shown as a read-only preview.
    /// </summary>
    bool IsProPreview => false;


    /// <summary>
    /// Shows the tool settings window.
    /// </summary>
    Task ShowSettingsWindowAsync() => Task.CompletedTask;

}

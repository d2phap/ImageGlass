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

public class IgTool
{
    /// <summary>
    /// Id of the tool.
    /// </summary>
    public string ToolId { get; set; } = string.Empty;


    /// <summary>
    /// Name of the tool.
    /// </summary>
    public string ToolName { get; set;} = string.Empty;


    /// <summary>
    /// Executable file / command to launch the tool.
    /// </summary>
    public string Executable { get; set; } = string.Empty;


    /// <summary>
    /// Argument to pass to the <see cref="Executable"/>.
    /// </summary>
    public string? Argument { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets value indicating that the tool
    /// supports <c>ToolServerMsgs.TOOL_TERMINATE</c> message so that it can be closed by ImageGlass.
    /// It will be displayed in Tools menu with a checkbox.
    /// </summary>
    public bool? CanToggle { get; set; } = false;


    /// <summary>
    /// Checks if the current <see cref="IgTool"/> instance is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(ToolId) || string.IsNullOrEmpty(Executable);

}
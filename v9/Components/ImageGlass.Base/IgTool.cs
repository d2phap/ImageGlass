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
using System.Dynamic;

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
    /// Arguments to pass to the <see cref="Executable"/>.
    /// </summary>
    public string? Arguments { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets value indicates that the tool is integrated with <c>ImageGlass.Tools</c>.
    /// </summary>
    public bool? IsIntegrated { get; set; } = false;


    /// <summary>
    /// Gets, sets tool hotkeys.
    /// </summary>
    public List<Hotkey> Hotkeys { get; set; } = new List<Hotkey>();


    /// <summary>
    /// Checks if the current <see cref="IgTool"/> instance is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(ToolId) || string.IsNullOrEmpty(Executable);


    /// <summary>
    /// Converts <see cref="IgTool"/> instance to <see cref="ExpandoObject"/>.
    /// </summary>
    public ExpandoObject ToExpandoObject()
    {
        var obj = new ExpandoObject();

        _ = obj.TryAdd(nameof(ToolId), ToolId);
        _ = obj.TryAdd(nameof(ToolName), ToolName);
        _ = obj.TryAdd(nameof(Executable), Executable);
        _ = obj.TryAdd(nameof(Arguments), Arguments);
        _ = obj.TryAdd(nameof(IsIntegrated), IsIntegrated);
        _ = obj.TryAdd(nameof(Hotkeys), Hotkeys.Select(i => i.ToString()).ToArray());

        return obj;
    }
}
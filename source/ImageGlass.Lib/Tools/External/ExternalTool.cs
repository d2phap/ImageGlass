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
using ImageGlass.Common.Types;
using System.Text.Json.Serialization;

namespace ImageGlass.Tools;


[JsonSerializable(typeof(ExternalTool))]
public partial class IgToolJsonContext : JsonSerializerContext { }


/// <summary>
/// Registration record for an external tool stored in <c>igconfig.json</c>
/// (in <c>Config.Tools</c>). External tools are launched out-of-process; they
/// communicate with the host through the named-pipe IPC defined in
/// <see cref="ImageGlass.SDK"/> (<c>ToolMessage</c> + <c>IToolHostProxy</c>).
/// </summary>
public sealed class ExternalTool
{
    /// <summary>
    /// Unique identifier for this tool. Used as the key in
    /// <c>Config.ToolSettings</c> and by <c>Core.API.IG_OpenTool</c> /
    /// <c>IG_CloseTool</c> / <c>IG_ToggleTool</c>.
    /// Convention: <c>"ext.&lt;vendor&gt;.&lt;name&gt;"</c>.
    /// </summary>
    public required string ToolId { get; init; }

    /// <summary>
    /// Human-readable display name shown in menus.
    /// </summary>
    public string ToolName { get; init; } = string.Empty;

    /// <summary>
    /// Absolute path to the tool executable.
    /// </summary>
    public string Executable { get; init; } = string.Empty;

    /// <summary>
    /// Command-line arguments. Supports the <see cref="Const.FILE_MACRO"/> placeholder
    /// (replaced with the currently viewed file path at launch time).
    /// </summary>
    public string Arguments { get; init; } = string.Empty;

    /// <summary>
    /// When <c>true</c>, the tool is launched as an integrated child process
    /// connected to the host via <c>ToolPipeServer</c> (full IPC).
    /// When <c>false</c>, the tool is launched detached - no pipe, no IPC -
    /// effectively a fire-and-forget shell command.
    /// </summary>
    public bool IsIntegrated { get; init; }

    /// <summary>
    /// Optional global hotkeys for launching the tool.
    /// </summary>
    public Hotkey[] Hotkeys { get; init; } = [];
}

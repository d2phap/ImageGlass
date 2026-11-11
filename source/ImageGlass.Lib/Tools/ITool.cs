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
using ImageGlass.UI.Viewer;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;

/// <summary>
/// Base interface for all tools in the ImageGlass tool registry.
/// Contains common members for identity, settings, and viewer access.
/// Hosted tools (with UI in ToolHostControl) extend via <see cref="IToolControl"/>.
/// Non-hosted tools (modal windows, actions) implement this directly.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the unique tool identifier.
    /// </summary>
    string ToolId { get; }

    /// <summary>
    /// Gets whether this tool is hosted in <see cref="ToolHostControl"/>.
    /// Hosted tools use <see cref="ToolHostControl"/> for UI.
    /// Non-hosted tools use <see cref="ExecuteAsync"/> instead.
    /// </summary>
    bool IsHosted { get; }

    /// <summary>
    /// Gets, sets settings for this tool, written in app's config file.
    /// </summary>
    object? Settings { get; }

    /// <summary>
    /// Gets, sets the instance of Viewer control.
    /// Set by the tool manager before use.
    /// </summary>
    ViewerControl Viewer { get; set; }

    /// <summary>
    /// Loads and parses tool settings from JSON element.
    /// Called by the tool manager before the tool is opened/executed.
    /// </summary>
    void LoadSettings(JsonElement? jsonEl) { }

    /// <summary>
    /// Saves the tool settings as JSON element.
    /// Called by the tool manager when the tool is closed/completed.
    /// </summary>
    JsonElement? SaveSettings() => null;

    /// <summary>
    /// Executes the non-hosted tool. Called by <c>IG_OpenTool</c> for non-hosted tools.
    /// Hosted tools leave the default no-op
    /// (they are opened via <see cref="ToolHostControl"/> instead).
    /// </summary>
    Task ExecuteAsync(ToolExecutionContext context) => Task.CompletedTask;
}

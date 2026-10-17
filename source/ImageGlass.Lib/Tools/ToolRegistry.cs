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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Factory delegate that creates an <see cref="IToolControl"/> hosted tool instance.
/// </summary>
public delegate IToolControl ToolControlFactory(ViewerControl viewer);


/// <summary>
/// Central registry for all tools (hosted and non-hosted).
/// Built-in tools register during <see cref="ImageGlass.Common.ServiceProviders.AppAPIProvider"/> construction.
/// Also owns the <see cref="ExternalTools"/> process manager so external-tool lifetime
/// is bound to the registry's lifetime.
/// </summary>
public sealed class ToolRegistry : PhDisposable
{
    private readonly Dictionary<string, ITool> _tools = new();


    /// <summary>
    /// Process manager for external (out-of-process) tools.
    /// Owned by the registry; disposed automatically on registry disposal.
    /// </summary>
    public ToolProcessManager ExternalTools { get; } = new();


    /// <summary>
    /// Registers a tool.
    /// </summary>
    public void Register(string toolId, ITool tool)
    {
        _tools[toolId] = tool;
    }


    /// <summary>
    /// Removes all registered external tools. Used when <see cref="Config.Tools"/> is re-applied
    /// so edited tools are re-registered with their new values.
    /// </summary>
    public void RemoveExternalTools()
    {
        var ids = _tools.Where(kv => kv.Value is ExternalToolProxy).Select(kv => kv.Key).ToList();
        foreach (var id in ids) _tools.Remove(id);
    }


    /// <summary>
    /// Gets the tool for a tool ID, or null if not found.
    /// </summary>
    public ITool? Get(string toolId)
    {
        _tools.TryGetValue(toolId, out var tool);
        return tool;
    }


    /// <summary>
    /// Checks whether a tool ID is registered.
    /// </summary>
    public bool Contains(string toolId) => _tools.ContainsKey(toolId);


    /// <summary>
    /// Gets all registered external tool registrations for menu building.
    /// </summary>
    public IEnumerable<ExternalTool> GetAllExternalToolManifests()
    {
        foreach (var tool in _tools.Values)
        {
            if (tool is ExternalToolProxy proxy)
                yield return proxy.Tool;
        }
    }


    /// <summary>
    /// Gets all registered tool IDs.
    /// </summary>
    public IEnumerable<string> GetAllToolIds() => _tools.Keys;


    /// <summary>
    /// Loads settings for a tool from the app config.
    /// </summary>
    public static void LoadToolSettings(ITool tool)
    {
        JsonElement? jsonEl = Core.Config.ToolSettings.TryGetValue(tool.ToolId, out var el)
            ? el
            : null;

        tool.LoadSettings(jsonEl);
    }


    /// <summary>
    /// Saves settings for a tool to the app config.
    /// </summary>
    public static void SaveToolSettings(ITool tool)
    {
        var jsonEl = tool.SaveSettings();

        SetToolSettings(tool.ToolId, jsonEl);
    }


    /// <summary>
    /// Writes the settings of a tool to the app config; <c>null</c> removes them.
    /// </summary>
    public static void SetToolSettings(string toolId, JsonElement? jsonEl)
    {
        var toolSettings = Core.Config.ToolSettings;

        if (jsonEl is null)
        {
            toolSettings.Remove(toolId);
        }
        else
        {
            toolSettings[toolId] = jsonEl.Value;
        }

        // the getter hands out a new dictionary while the config holds none, so store this one
        Core.Config.ToolSettings = toolSettings;
    }


    /// <summary>
    /// Executes a non-hosted tool and saves its settings on completion.
    /// </summary>
    public static async Task ExecuteNonHostedToolAsync(ITool tool, ToolExecutionContext context)
    {
        try
        {
            await tool.ExecuteAsync(context);
        }
        finally
        {
            SaveToolSettings(tool);
        }
    }


    protected override void OnDisposing()
    {
        ExternalTools.Dispose();
        base.OnDisposing();
    }

}

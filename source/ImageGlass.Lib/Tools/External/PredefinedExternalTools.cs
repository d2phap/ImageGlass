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
using Avalonia.Input;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Tools;


/// <summary>
/// External tools seeded into <c>Config.Tools</c> by the app. Editable, but not deletable or re-id-able.
/// </summary>
public static class PredefinedExternalTools
{
    /// <summary>
    /// Tool id of the pre-configured ExifGlass registration.
    /// </summary>
    public const string EXIFGLASS_ID = "Tool_ExifGlass";


    private static readonly HashSet<string> _ids = new(StringComparer.OrdinalIgnoreCase)
    {
        EXIFGLASS_ID,
    };


    /// <summary>
    /// Creates the pre-configured registrations, in seeding order. Fresh instances per call.
    /// </summary>
    public static IEnumerable<ExternalTool> CreateAll()
    {
        // ExifGlass is distributed as a PATH command, an .app bundle, and a Flatpak app id
        var (exifGlassExe, exifGlassArgs) = BHelper.OS switch
        {
            OSType.Mac => ("/Applications/ExifGlass.app", Const.FILE_MACRO),
            OSType.Linux => ("flatpak", $"run io.github.d2phap.exifglass {Const.FILE_MACRO}"),
            _ => ("exifglass", Const.FILE_MACRO),
        };

        yield return new ExternalTool
        {
            ToolId = EXIFGLASS_ID,
            ToolName = "ExifGlass - EXIF Metadata viewer",
            Executable = exifGlassExe,
            Arguments = exifGlassArgs,
            IsIntegrated = true,
            Hotkeys = [new Hotkey(Key.X)],
        };
    }


    /// <summary>
    /// Whether <paramref name="toolId"/> belongs to a pre-configured tool.
    /// </summary>
    public static bool IsPredefined(string? toolId)
        => !string.IsNullOrEmpty(toolId) && _ids.Contains(toolId);


    /// <summary>
    /// Gets the pristine definition of a pre-configured tool, or <c>null</c> when the id is not one.
    /// </summary>
    public static ExternalTool? Find(string? toolId)
    {
        if (!IsPredefined(toolId)) return null;

        return CreateAll().FirstOrDefault(t =>
            string.Equals(t.ToolId, toolId, StringComparison.OrdinalIgnoreCase));
    }


    /// <summary>
    /// Appends missing pre-configured tools (matched by id); returns true when the list changed.
    /// </summary>
    public static bool Seed(ICollection<ExternalTool> tools)
    {
        var existingIds = new HashSet<string>(
            tools.Select(t => t.ToolId ?? string.Empty), StringComparer.OrdinalIgnoreCase);
        var changed = false;

        foreach (var tool in CreateAll())
        {
            if (existingIds.Contains(tool.ToolId)) continue;

            tools.Add(tool);
            changed = true;
        }

        return changed;
    }

}

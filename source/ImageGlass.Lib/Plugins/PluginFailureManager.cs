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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace ImageGlass.Plugins;


/// <summary>
/// Tracks per-native-plugin crash markers under <c>{ConfigDir}/_plugins/_quarantine/</c>.
/// A marker file disables the plugin for the current session and any future startup until cleared.
/// </summary>
/// <remarks>
/// Native plugins share the host process, so we cannot truly recover from a hard fault.
/// The best we can do is record a marker before doing anything risky, and refuse to load the
/// plugin again on the next launch if the marker is present.
/// </remarks>
public sealed class PluginFailureManager
{
    private const string QUARANTINE_DIR = "_quarantine";
    private const string LOADING_MARKER_EXT = ".loading";
    private const int SOFT_FAILURE_THRESHOLD = 3;

    private readonly string _quarantineDir;
    private readonly ConcurrentDictionary<string, int> _softFailureCounts = new(StringComparer.Ordinal);
    // plugin id -> why it was disabled, so the UI can show the real reason
    private readonly ConcurrentDictionary<string, string> _sessionDisabled = new(StringComparer.Ordinal);


    public PluginFailureManager()
    {
        _quarantineDir = BHelper.ConfigDir(Dir.Plugins, QUARANTINE_DIR);
    }


    /// <summary>
    /// Returns true if the plugin is permanently disabled (marker file present)
    /// or has been disabled for the current session.
    /// </summary>
    public bool IsQuarantined(string pluginId)
    {
        if (_sessionDisabled.ContainsKey(pluginId)) return true;
        return File.Exists(GetMarkerPath(pluginId));
    }


    /// <summary>
    /// Why the plugin is quarantined (session state, else the marker file), or <c>null</c>.
    /// </summary>
    public string? GetQuarantineReason(string pluginId)
    {
        if (_sessionDisabled.TryGetValue(pluginId, out var sessionReason)) return sessionReason;

        try
        {
            var markerPath = GetMarkerPath(pluginId);
            if (!File.Exists(markerPath)) return null;

            // marker layout: timestamp on the first line, reason on the second
            var lines = File.ReadAllLines(markerPath);
            return lines.Length > 1 && !string.IsNullOrWhiteSpace(lines[1]) ? lines[1] : null;
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Disables the plugin for the rest of the current session only.
    /// Used for managed exceptions where a full quarantine would be too aggressive.
    /// </summary>
    public void DisableForSession(string pluginId, string reason)
    {
        _sessionDisabled[pluginId] = reason;
        Debug.WriteLine($"[PluginFailureManager] Session-disabled '{pluginId}': {reason}");
    }


    /// <summary>
    /// Records a soft failure for the plugin. After repeated failures
    /// the plugin is session-disabled.
    /// </summary>
    public void RecordSoftFailure(string pluginId, string reason)
    {
        var count = _softFailureCounts.AddOrUpdate(pluginId, 1, static (_, v) => v + 1);
        Debug.WriteLine($"[PluginFailureManager] Soft failure #{count} for '{pluginId}': {reason}");
        if (count >= SOFT_FAILURE_THRESHOLD)
        {
            DisableForSession(pluginId, $"soft-failure threshold ({SOFT_FAILURE_THRESHOLD}) reached");
        }
    }


    /// <summary>
    /// Writes a persistent marker that disables the plugin on this and future startups
    /// until the user removes it (or the plugin is updated, which can clear it externally).
    /// </summary>
    public void Quarantine(string pluginId, string reason)
    {
        _sessionDisabled[pluginId] = reason;
        try
        {
            Directory.CreateDirectory(_quarantineDir);
            File.WriteAllText(GetMarkerPath(pluginId),
                $"{DateTimeOffset.UtcNow:O}\n{reason}\n");
            Debug.WriteLine($"[PluginFailureManager] Quarantined '{pluginId}': {reason}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginFailureManager] Failed to write marker for '{pluginId}': {ex.Message}");
        }
    }


    /// <summary>
    /// Clears the persistent quarantine marker for the plugin if present.
    /// </summary>
    public bool Clear(string pluginId)
    {
        _sessionDisabled.TryRemove(pluginId, out _);
        _softFailureCounts.TryRemove(pluginId, out _);
        ClearLoadingBreadcrumb(pluginId);
        try
        {
            var path = GetMarkerPath(pluginId);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginFailureManager] Failed to clear marker for '{pluginId}': {ex.Message}");
        }
        return false;
    }


    /// <summary>
    /// Returns true if a "loading" breadcrumb is present, indicating a previous load attempt
    /// did not complete gracefully (i.e. it hard-crashed the process).
    /// </summary>
    public bool HasLoadingBreadcrumb(string pluginId)
    {
        return File.Exists(GetLoadingMarkerPath(pluginId));
    }


    /// <summary>
    /// Writes a "loading" breadcrumb just before invoking risky native plugin code.
    /// Must be cleared once the load completes (gracefully or with a managed error).
    /// </summary>
    public void SetLoadingBreadcrumb(string pluginId)
    {
        try
        {
            Directory.CreateDirectory(_quarantineDir);
            File.WriteAllText(GetLoadingMarkerPath(pluginId), DateTimeOffset.UtcNow.ToString("O"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginFailureManager] Failed to write loading breadcrumb for '{pluginId}': {ex.Message}");
        }
    }


    /// <summary>
    /// Removes the "loading" breadcrumb for the plugin if present.
    /// </summary>
    public void ClearLoadingBreadcrumb(string pluginId)
    {
        try
        {
            var path = GetLoadingMarkerPath(pluginId);
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginFailureManager] Failed to clear loading breadcrumb for '{pluginId}': {ex.Message}");
        }
    }


    private string GetLoadingMarkerPath(string pluginId)
    {
        return Path.Combine(_quarantineDir, MakeFilenameSafe(pluginId) + LOADING_MARKER_EXT);
    }


    private string GetMarkerPath(string pluginId)
    {
        var safe = MakeFilenameSafe(pluginId);
        return Path.Combine(_quarantineDir, safe + ".marker");
    }


    private static string MakeFilenameSafe(string id)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = id.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (Array.IndexOf(invalid, chars[i]) >= 0) chars[i] = '_';
        }
        return new string(chars);
    }
}

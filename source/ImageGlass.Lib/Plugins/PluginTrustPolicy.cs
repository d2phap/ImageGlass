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
using ImageGlass.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ImageGlass.Plugins;


/// <summary>
/// Central policy that decides whether a native plugin is allowed to load.
/// A plugin runs only after the user explicitly enables it (consent), which pins the
/// SHA-256 of its native library in <c>Config.PluginTrust</c>. If the library later
/// changes, the pinned hash no longer matches and trust is withheld until the user
/// re-approves - this defends against a trusted plugin's binary being swapped.
/// </summary>
public static class PluginTrustPolicy
{
    /// <summary>
    /// Trust state of a plugin, used for both enforcement and UI display.
    /// </summary>
    public enum TrustState
    {
        /// <summary>The plugin library is missing or its manifest path is invalid.</summary>
        Missing,
        /// <summary>No trust entry exists; the plugin has never been enabled.</summary>
        Untrusted,
        /// <summary>A trust entry exists but is disabled by the user.</summary>
        Disabled,
        /// <summary>Enabled and the on-disk library matches the pinned hash.</summary>
        Trusted,
        /// <summary>Enabled but the library hash no longer matches the pin (needs re-consent).</summary>
        Changed,
    }


    /// <summary>
    /// Resolves and containment-validates the plugin's native library path,
    /// reusing the loader's path checks. Returns <c>null</c> if invalid.
    /// </summary>
    public static string? ResolveLibraryPath(PluginManifest manifest, string pluginDir)
    {
        return PluginRegistry.TryResolvePluginLibraryPath(manifest.Executable, pluginDir, out var path)
            ? path
            : null;
    }


    /// <summary>
    /// Computes the lowercase hex SHA-256 of a file, or <c>null</c> on any I/O error.
    /// </summary>
    public static string? ComputeSha256(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Enforcement gate used by the loader: returns <c>true</c> only when the plugin is
    /// enabled AND the on-disk library still matches the pinned SHA-256.
    /// </summary>
    public static bool IsTrusted(string pluginId, string libraryPath)
    {
        if (!Core.Config.PluginTrust.TryGetValue(pluginId, out var info) || info is null || !info.Enabled)
            return false;

        var hash = ComputeSha256(libraryPath);
        return hash is not null && string.Equals(hash, info.Hash, StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Normalizes an extension to the canonical persisted form: lowercase, leading dot.
    /// </summary>
    internal static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;

        var trimmed = extension.Trim().ToLowerInvariant();
        return trimmed.StartsWith('.') ? trimmed : "." + trimmed;
    }


    /// <summary>
    /// Gets the extensions the user switched off for this plugin. Both sets are non-null.
    /// </summary>
    public static (IReadOnlySet<string> Decode, IReadOnlySet<string> Encode) GetExtensionExclusions(string pluginId)
    {
        Core.Config.PluginTrust.TryGetValue(pluginId, out var info);

        return (
            info?.DisabledDecodeExtensions ?? EMPTY_EXCLUSIONS,
            info?.DisabledEncodeExtensions ?? EMPTY_EXCLUSIONS);
    }


    /// <summary>
    /// Whether the plugin may decode the given extension (i.e. the user has not switched it off).
    /// </summary>
    public static bool IsExtensionEnabledForDecode(string pluginId, string extension)
        => !GetExtensionExclusions(pluginId).Decode.Contains(NormalizeExtension(extension));


    /// <summary>
    /// Whether the plugin may encode the given extension.
    /// </summary>
    public static bool IsExtensionEnabledForEncode(string pluginId, string extension)
        => !GetExtensionExclusions(pluginId).Encode.Contains(NormalizeExtension(extension));


    /// <summary>
    /// Replaces both exclusion sets and persists. <c>false</c> when admin-locked, or when there is
    /// no trust entry yet (use <see cref="TrustAsync"/> then).
    /// </summary>
    public static async Task<bool> SetExtensionExclusionsAsync(string pluginId,
        IEnumerable<string>? disabledDecode, IEnumerable<string>? disabledEncode)
    {
        if (Config.IsConfigLocked(ConfigId.PluginTrust)) return false;
        if (!Core.Config.PluginTrust.ContainsKey(pluginId)) return false;

        await UpsertAsync(pluginId, old => new PluginTrustInfo
        {
            Enabled = old?.Enabled ?? false,
            Hash = old?.Hash ?? string.Empty,
            DisabledDecodeExtensions = Normalize(disabledDecode),
            DisabledEncodeExtensions = Normalize(disabledEncode),
        });
        return true;
    }


    /// <summary>
    /// Computes the current <see cref="TrustState"/> of a plugin for UI display.
    /// </summary>
    public static TrustState GetState(PluginManifest manifest, string pluginDir)
    {
        var libraryPath = ResolveLibraryPath(manifest, pluginDir);
        if (libraryPath is null || !File.Exists(libraryPath)) return TrustState.Missing;

        if (!Core.Config.PluginTrust.TryGetValue(manifest.Id, out var info) || info is null)
            return TrustState.Untrusted;

        if (!info.Enabled) return TrustState.Disabled;

        var hash = ComputeSha256(libraryPath);
        return hash is not null && string.Equals(hash, info.Hash, StringComparison.OrdinalIgnoreCase)
            ? TrustState.Trusted
            : TrustState.Changed;
    }


    /// <summary>
    /// Enables the plugin, pins the current library hash and persists. Pass the exclusion sets to
    /// apply them in the same write so the first load sees them; <c>null</c> keeps what is stored.
    /// <c>false</c> when admin-locked or the library could not be hashed.
    /// </summary>
    public static async Task<bool> TrustAsync(PluginManifest manifest, string pluginDir,
        IEnumerable<string>? disabledDecode = null, IEnumerable<string>? disabledEncode = null)
    {
        if (Config.IsConfigLocked(ConfigId.PluginTrust)) return false;

        var libraryPath = ResolveLibraryPath(manifest, pluginDir);
        if (libraryPath is null) return false;

        var hash = ComputeSha256(libraryPath);
        if (hash is null) return false;

        await UpsertAsync(manifest.Id, old => new PluginTrustInfo
        {
            Enabled = true,
            Hash = hash,
            DisabledDecodeExtensions = disabledDecode is null
                ? Clone(old?.DisabledDecodeExtensions) : Normalize(disabledDecode),
            DisabledEncodeExtensions = disabledEncode is null
                ? Clone(old?.DisabledEncodeExtensions) : Normalize(disabledEncode),
        });
        return true;
    }


    /// <summary>
    /// Disables the plugin (keeps a disabled entry and the user's format choices), then persists.
    /// Returns <c>false</c> if the setting is admin-locked.
    /// </summary>
    public static async Task<bool> DisableAsync(string pluginId)
    {
        if (Config.IsConfigLocked(ConfigId.PluginTrust)) return false;

        await UpsertAsync(pluginId, old => new PluginTrustInfo
        {
            Enabled = false,
            Hash = old?.Hash ?? string.Empty,
            DisabledDecodeExtensions = Clone(old?.DisabledDecodeExtensions),
            DisabledEncodeExtensions = Clone(old?.DisabledEncodeExtensions),
        });
        return true;
    }


    /// <summary>
    /// Drops the plugin's trust entry (used when the plugin is deleted), then persists the config.
    /// Returns <c>false</c> if the setting is admin-locked.
    /// </summary>
    public static async Task<bool> RemoveAsync(string pluginId)
    {
        if (Config.IsConfigLocked(ConfigId.PluginTrust)) return false;
        if (!Core.Config.PluginTrust.ContainsKey(pluginId)) return true;

        var trust = new Dictionary<string, PluginTrustInfo>(Core.Config.PluginTrust, StringComparer.Ordinal);
        trust.Remove(pluginId);
        Core.Config.PluginTrust = trust;

        await Core.Config.SaveAsync();
        return true;
    }


    private static readonly IReadOnlySet<string> EMPTY_EXCLUSIONS =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// Copy-on-write update of one trust entry: fresh info + fresh dictionary (the Config setter
    /// ignores an equal reference), then persists.
    /// </summary>
    private static async Task UpsertAsync(string pluginId, Func<PluginTrustInfo?, PluginTrustInfo> mutate)
    {
        Core.Config.PluginTrust.TryGetValue(pluginId, out var existing);

        var trust = new Dictionary<string, PluginTrustInfo>(Core.Config.PluginTrust, StringComparer.Ordinal)
        {
            [pluginId] = mutate(existing),
        };
        Core.Config.PluginTrust = trust;

        await Core.Config.SaveAsync();
    }


    /// <summary>
    /// Normalizes an exclusion set; empty collapses to <c>null</c> so it is omitted from the JSON.
    /// </summary>
    private static HashSet<string>? Normalize(IEnumerable<string>? extensions)
    {
        if (extensions is null) return null;

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ext in extensions)
        {
            var normalized = NormalizeExtension(ext);
            if (normalized.Length > 1) set.Add(normalized);
        }
        return set.Count == 0 ? null : set;
    }


    /// <summary>
    /// Duplicates an exclusion set so a replaced entry is never aliased into the new one.
    /// </summary>
    private static HashSet<string>? Clone(HashSet<string>? set)
        => set is null || set.Count == 0 ? null : new HashSet<string>(set, StringComparer.OrdinalIgnoreCase);
}

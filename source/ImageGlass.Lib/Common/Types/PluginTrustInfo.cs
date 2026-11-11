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
using ImageGlass.Common.Types.JsonTypeConverters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types;


/// <summary>
/// Per-plugin trust decision persisted in <c>Config.PluginTrust</c> (keyed by plugin id).
/// A native plugin is loaded only when <see cref="Enabled"/> is <c>true</c> AND the on-disk
/// library still hashes to <see cref="Hash"/> (the value pinned when the user enabled it).
/// Never mutate in place: copy, change, reassign.
/// </summary>
public sealed class PluginTrustInfo
{
    /// <summary>
    /// Whether the user has explicitly enabled (trusted) this plugin.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Lowercase hex SHA-256 of the plugin's native library, pinned at the moment of consent.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Extensions the user switched OFF for decoding; <c>null</c>/empty means all are active.
    /// Exclusions, not an allowlist, so a plugin update that adds a format is on by default.
    /// </summary>
    [JsonConverter(typeof(JsonHashSetToStringConverter))]
    public HashSet<string>? DisabledDecodeExtensions { get; set; }

    /// <summary>
    /// Extensions the user switched OFF for encoding. See <see cref="DisabledDecodeExtensions"/>.
    /// </summary>
    [JsonConverter(typeof(JsonHashSetToStringConverter))]
    public HashSet<string>? DisabledEncodeExtensions { get; set; }
}

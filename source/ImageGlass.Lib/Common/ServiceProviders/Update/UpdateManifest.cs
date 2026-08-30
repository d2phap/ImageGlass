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
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Source-generated JSON context for AOT-safe serialization of update manifest.
/// </summary>
[JsonSerializable(typeof(UpdateManifest))]
public partial class UpdateManifestJsonContext : JsonSerializerContext;


/// <summary>
/// Root response from the update metadata endpoint.
/// </summary>
public sealed class UpdateManifest
{
    [JsonPropertyName("apiVersion")]
    public double ApiVersion { get; set; }

    [JsonPropertyName("releases")]
    public UpdateReleases? Releases { get; set; }
}


/// <summary>
/// Contains release channels.
/// </summary>
public sealed class UpdateReleases
{
    [JsonPropertyName("stable")]
    public UpdateReleaseInfo? Stable { get; set; }

    [JsonPropertyName("beta")]
    public UpdateReleaseInfo? Beta { get; set; }
}


/// <summary>
/// Information about a single release.
/// </summary>
public sealed class UpdateReleaseInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("changelogUrl")]
    public string ChangelogUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to download/get the update. Falls back to <see cref="ChangelogUrl"/> when empty.
    /// </summary>
    [JsonPropertyName("updateUrl")]
    public string UpdateUrl { get; set; } = string.Empty;

    [JsonPropertyName("publishedDate")]
    public string PublishedDate { get; set; } = string.Empty;

    /// <summary>
    /// Per-platform artifacts (v2.0 schema). Null for v1.1.
    /// Key is platform identifier, e.g. "win-x64", "linux-x64", "osx-arm64".
    /// </summary>
    [JsonPropertyName("artifacts")]
    public Dictionary<string, UpdateArtifactInfo>? Artifacts { get; set; }
}


/// <summary>
/// Information about a downloadable artifact for a specific platform.
/// </summary>
public sealed class UpdateArtifactInfo
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Lowercase hex SHA-256; required to auto-install, unverified bytes are never installed.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>
    /// Artifact size in bytes; <c>0</c> when the manifest omits it.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Native package version; an MSIX identity is <c>Major.Minor.IgBundleBuild.0</c>, not the app version.
    /// </summary>
    [JsonPropertyName("packageVersion")]
    public string PackageVersion { get; set; } = string.Empty;


    /// <summary>
    /// Whether this artifact carries everything an auto-install needs.
    /// </summary>
    [JsonIgnore]
    public bool IsInstallable => !string.IsNullOrWhiteSpace(Url)
        && !string.IsNullOrWhiteSpace(Sha256);
}


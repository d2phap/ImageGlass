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
using System.Text.Json.Serialization;

namespace ImageGlass.Common.ServiceProviders.Licensing;


/// <summary>
/// Source-generated JSON context for the license model.
/// </summary>
[JsonSerializable(typeof(LicenseInfo))]
public partial class LicenseJsonContext : JsonSerializerContext;


/// <summary>
/// A Pro license. Every field except <see cref="Signature"/> is covered by the signature.
/// </summary>
public sealed class LicenseInfo
{
    /// <summary>
    /// Product the license unlocks. Always "ImageGlass"; anything else is rejected.
    /// </summary>
    [JsonPropertyName("product")]
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Schema version of the license file. Never selects the signed field set.
    /// </summary>
    [JsonPropertyName("licenseVersion")]
    public int LicenseVersion { get; set; }

    /// <summary>
    /// Human-readable license id, e.g. "IG10-7Q4KD-2M9XB".
    /// </summary>
    [JsonPropertyName("licenseId")]
    public string LicenseId { get; set; } = string.Empty;

    /// <summary>
    /// Id of the signing key, used to pick the public key for verification.
    /// </summary>
    [JsonPropertyName("keyId")]
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the license owner.
    /// </summary>
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Owning team or company. Null for an individual license.
    /// </summary>
    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Plan display name, e.g. "Pro Individual".
    /// </summary>
    [JsonPropertyName("plan")]
    public string Plan { get; set; } = string.Empty;

    /// <summary>
    /// Major version line the license covers, e.g. "10", or "all" for any version.
    /// </summary>
    [JsonPropertyName("versionScope")]
    public string VersionScope { get; set; } = string.Empty;

    /// <summary>
    /// Number of users or devices the license covers. At least 1.
    /// </summary>
    [JsonPropertyName("seatCount")]
    public int SeatCount { get; set; }

    /// <summary>
    /// Support tier: "none", "standard", "priority" or "custom".
    /// </summary>
    [JsonPropertyName("supportLevel")]
    public string SupportLevel { get; set; } = string.Empty;

    /// <summary>
    /// When the license was purchased, as an ISO-8601 UTC timestamp.
    /// </summary>
    [JsonPropertyName("purchaseDate")]
    public string PurchaseDate { get; set; } = string.Empty;

    /// <summary>
    /// Expiry as an ISO-8601 UTC timestamp. Null means perpetual.
    /// </summary>
    [JsonPropertyName("expiresAt")]
    public string? ExpiresAt { get; set; }

    /// <summary>
    /// True for a commercial source-code grant. Display only, unlocks no feature.
    /// </summary>
    [JsonPropertyName("sourceLicense")]
    public bool SourceLicense { get; set; }

    /// <summary>
    /// Where the license came from, e.g. "stripe", "manual" or "msstore".
    /// </summary>
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// The release this license was bought from. Null when unknown.
    /// </summary>
    [JsonPropertyName("initVersion")]
    public string? InitVersion { get; set; }

    /// <summary>
    /// Base64 RSA signature over all other fields. The only unsigned field.
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;
}

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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ImageGlass.Common.ServiceProviders.Licensing;


/// <summary>
/// Builds the signed text of a license: every field except the signature, minified.
/// </summary>
public static class LicenseSigningPayload
{
    /// <summary>
    /// Builds the exact text whose signature is created and verified.
    /// </summary>
    public static string Build(LicenseInfo lic)
    {
        // sorted by key at build time, so the order fields are listed here never
        // affects the output (safe to reorder or reformat this initializer)
        //
        // there is exactly one payload shape: LicenseVersion never selects the field set
        var fields = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["product"] = Str(lic.Product),
            ["licenseVersion"] = Int(lic.LicenseVersion),
            ["licenseId"] = Str(lic.LicenseId),
            ["keyId"] = Str(lic.KeyId),
            ["customerName"] = Str(lic.CustomerName),
            ["organizationName"] = NullableStr(lic.OrganizationName),
            ["plan"] = Str(lic.Plan),
            ["versionScope"] = Str(lic.VersionScope),
            ["seatCount"] = Int(lic.SeatCount),
            ["supportLevel"] = Str(lic.SupportLevel),
            ["purchaseDate"] = Str(lic.PurchaseDate),
            ["expiresAt"] = NullableStr(lic.ExpiresAt),
            ["sourceLicense"] = lic.SourceLicense ? "true" : "false",
            ["channel"] = Str(lic.Channel),
            ["initVersion"] = NullableStr(Normalize(lic.InitVersion)),
        };

        var sb = new StringBuilder(256);
        sb.Append('{');

        var first = true;
        foreach (var field in fields)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append(Str(field.Key)).Append(':').Append(field.Value);
        }

        sb.Append('}');
        return sb.ToString();
    }


    /// <summary>
    /// The base-10 form of an integer value.
    /// </summary>
    private static string Int(int value) => value.ToString(CultureInfo.InvariantCulture);


    /// <summary>
    /// "null" or the escaped string.
    /// </summary>
    private static string NullableStr(string? value) => value is null ? "null" : Str(value);


    /// <summary>
    /// Collapses a blank optional value to null, so "" and null sign identically.
    /// </summary>
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;


    /// <summary>
    /// A JSON string literal, escaping only quote, backslash, and control chars.
    /// </summary>
    private static string Str(string value)
    {
        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\t': sb.Append("\\t"); break;
                case '\n': sb.Append("\\n"); break;
                case '\f': sb.Append("\\f"); break;
                case '\r': sb.Append("\\r"); break;
                default:
                    if (ch < 0x20)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)ch).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                    break;
            }
        }
        sb.Append('"');
        return sb.ToString();
    }
}

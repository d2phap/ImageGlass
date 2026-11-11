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
using System.Globalization;

namespace ImageGlass.Common.ServiceProviders.Licensing;


/// <summary>
/// Decides whether a license covers the running app version. This is the single place the
/// version policy lives; edit here when a new major version ships.
/// </summary>
public static class LicenseScope
{
    /// <summary>
    /// Version scope that covers every release.
    /// </summary>
    public const string SCOPE_ALL = "all";


    /// <summary>
    /// Whether the license covers the app version that is running.
    /// </summary>
    public static bool CoversRunningApp(LicenseInfo license)
    {
        // a fault in here must never revoke Pro from a paying user, so it fails open
        try
        {
            var isExempt = IsScopeExempt();
            if (isExempt) return true;

            // an unreadable app version can't refuse anything
            var appVersion = Core.BuildInfo?.Version;
            var hasAppMajor = TryParseMajor(appVersion, out var appMajor);
            if (!hasAppMajor) return true;

            return Covers(license, appMajor);
        }
        catch
        {
            return true;
        }
    }


    /// <summary>
    /// Whether the license covers the given app major version. Lenient by design: a blank or
    /// unparseable scope counts as covering, matching the trust-based licensing policy.
    /// </summary>
    public static bool Covers(LicenseInfo license, int appMajor)
    {
        // the property is declared non-nullable, but the deserializer can still assign null
        var scope = license.VersionScope?.Trim();
        if (string.IsNullOrEmpty(scope)) return true;

        var coversEveryVersion = string.Equals(scope, SCOPE_ALL, StringComparison.OrdinalIgnoreCase);
        if (coversEveryVersion) return true;

        // to also refuse a license bought from an earlier release line, gate on
        // license.InitVersion here. Inert while only one major line exists.

        var hasLicenseMajor = TryParseMajor(scope, out var licenseMajor);
        if (!hasLicenseMajor) return true;

        return licenseMajor == appMajor;
    }


    /// <summary>
    /// Gets the running app's major version as text, to pair with a license's scope in a message.
    /// Falls back to the whole version string when it cannot be read.
    /// </summary>
    public static string GetRunningAppMajorText()
    {
        var appVersion = Core.BuildInfo?.Version ?? string.Empty;
        var hasAppMajor = TryParseMajor(appVersion, out var appMajor);

        return hasAppMajor ? appMajor.ToString(CultureInfo.InvariantCulture) : appVersion;
    }


    /// <summary>
    /// Whether this build ignores the version scope entirely.
    /// </summary>
    private static bool IsScopeExempt()
    {
        // a store grant covers every release, so a store build opts out of the scope check
        var provider = Core.StoreEntitlementProvider;
        if (provider is null) return false;

        return provider.IsStoreEntitled;
    }


    /// <summary>
    /// Reads the leading major version number, so a bare major and a full version string both
    /// resolve to the same value.
    /// </summary>
    private static bool TryParseMajor(string? value, out int major)
    {
        major = 0;
        if (string.IsNullOrWhiteSpace(value)) return false;

        var majorPart = value.Split('.', 2)[0].Trim();
        if (string.IsNullOrEmpty(majorPart)) return false;

        return int.TryParse(majorPart, NumberStyles.None, CultureInfo.InvariantCulture, out major);
    }
}

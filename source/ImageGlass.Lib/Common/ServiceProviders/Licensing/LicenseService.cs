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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace ImageGlass.Common.ServiceProviders.Licensing;


/// <summary>
/// Finds and verifies the active Pro license (local file + embedded key, no network).
/// </summary>
public static class LicenseService
{
    /// <summary>
    /// File-name suffix of a license file.
    /// </summary>
    public const string LICENSE_FILE_EXTENSION = ".iglicense.json";

    private const string LICENSE_FILE_PATTERN = "*.iglicense.json";

    /// <summary>
    /// Channel id recorded on a license granted by the Microsoft Store.
    /// </summary>
    public const string CHANNEL_MSSTORE = "msstore";

    /// <summary>
    /// Store brand names by channel id. Product names, so they are never localized. Add a store
    /// here when one ships.
    /// </summary>
    private static readonly Dictionary<string, string> _channelNames = new(StringComparer.OrdinalIgnoreCase)
    {
        [CHANNEL_MSSTORE] = "Microsoft Store",
    };

    /// <summary>
    /// Product name a license must carry to be accepted.
    /// </summary>
    private const string PRODUCT_NAME = "ImageGlass";

    /// <summary>
    /// Schema version stamped on a license the app builds for itself.
    /// </summary>
    private const int LICENSE_SCHEMA_VERSION = 1;

    /// <summary>
    /// Maps a license keyId to its embedded public-key resource. Add keys here to rotate.
    /// </summary>
    private static readonly Dictionary<string, string> _keyResources = new(StringComparer.Ordinal)
    {
        ["ig_license_2026"] = "ig_license_2026.spki.pem",
    };

    private static readonly Dictionary<string, string?> _pemCache = new(StringComparer.Ordinal);
    private static readonly Lock _lock = new();


    /// <summary>
    /// Finds the active license: the store entitlement, then the install folder, then the config
    /// folder. Returns the license that should enable Pro, or null to run as Classic. Never throws.
    /// </summary>
    /// <param name="outOfScopeLicense">
    /// The first authentic license that does not cover this app version, so the caller can
    /// name it in the upgrade prompt. Null when there is none.
    /// </param>
    /// <param name="expiredLicense">
    /// The first authentic license past its expiry, or null. Cleared when Pro does turn on.
    /// </param>
    public static LicenseInfo? LoadActive(out LicenseInfo? outOfScopeLicense,
        out LicenseInfo? expiredLicense)
    {
        outOfScopeLicense = null;
        expiredLicense = null;

        try
        {
            // a store purchase is proven by package identity, so it outranks any file on disk
            var storeLicense = LoadStoreEntitlement();
            if (storeLicense is not null)
            {
                var storeCoversThisApp = LicenseScope.CoversRunningApp(storeLicense);
                if (storeCoversThisApp) return storeLicense;

                outOfScopeLicense = storeLicense;
            }

            // install folder wins, so a machine-wide deployed license takes precedence
            foreach (var dir in new[] { BHelper.BaseDir(), BHelper.ConfigPath })
            {
                var files = EnumerateLicenseFiles(dir);

                foreach (var path in files)
                {
                    var isAuthentic = TryVerify(path, out var lic, out _);
                    if (!isAuthentic) continue;

                    // authentic but past its expiry instant: keep it so the caller can say so
                    var isWithinValidity = IsWithinValidity(lic);
                    if (!isWithinValidity)
                    {
                        expiredLicense ??= lic;
                        continue;
                    }

                    // authentic and current, but bought for another version line
                    var coversThisApp = LicenseScope.CoversRunningApp(lic);
                    if (!coversThisApp)
                    {
                        outOfScopeLicense ??= lic;
                        continue;
                    }

                    // Pro is on, so neither notice is worth showing any more
                    outOfScopeLicense = null;
                    expiredLicense = null;
                    return lic;
                }
            }
        }
        catch
        {
            // on any failure, run as Classic
        }

        return null;
    }


    /// <summary>
    /// Builds the license for a platform-store install, or null when this is not a store build.
    /// </summary>
    private static LicenseInfo? LoadStoreEntitlement()
    {
        // a faulty store provider must not stop the folder scan from finding a license
        try
        {
            var provider = Core.StoreEntitlementProvider;
            if (provider is null) return null;

            var isStoreEntitled = provider.IsStoreEntitled;
            if (!isStoreEntitled) return null;

            // the bundled file only supplies the details shown to the user; the store grants Pro
            var bundledDir = provider.BundledLicenseDirectory;
            var bundled = TryLoadFirstValidLicense(bundledDir, out _);
            if (bundled is not null) return bundled;

            return CreateStoreEntitlementLicense(provider);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Minimal license standing in for a store entitlement when no bundled file can be read. It
    /// carries no signature, so it must never be written to disk or exported. The version scope is
    /// left blank on purpose: the store exemption in <see cref="LicenseScope"/> is the one place
    /// that grants every release.
    /// </summary>
    private static LicenseInfo CreateStoreEntitlementLicense(IStoreEntitlementProvider provider) => new()
    {
        Product = PRODUCT_NAME,
        LicenseVersion = LICENSE_SCHEMA_VERSION,
        Plan = provider.PlanName,
        SeatCount = 1,
        Channel = provider.ChannelId,
    };


    /// <summary>
    /// Gets the brand name of the store a channel id refers to, or the channel id itself when it is
    /// not a known store.
    /// </summary>
    public static string GetChannelDisplayName(string? channelId)
    {
        var channel = channelId?.Trim() ?? string.Empty;
        if (channel.Length == 0) return string.Empty;

        var isKnownStore = _channelNames.TryGetValue(channel, out var name);
        return isKnownStore ? name! : channel;
    }


    /// <summary>
    /// Finds the signed license bundled with a store build, which the user may save for their other
    /// platforms. False when this is not a store build, or the bundled file is missing or unusable.
    /// </summary>
    /// <remarks>
    /// The caller must copy the file at <paramref name="filePath"/> byte for byte. Re-serializing
    /// the parsed license would change the bytes and break its signature.
    /// </remarks>
    public static bool TryGetExportableLicense(out string filePath, out LicenseInfo license)
    {
        filePath = string.Empty;
        license = null!;

        // no export offered when anything is off; it is a convenience, never a gate
        try
        {
            var provider = Core.StoreEntitlementProvider;
            if (provider is null) return false;

            var isStoreEntitled = provider.IsStoreEntitled;
            if (!isStoreEntitled) return false;

            var bundled = TryLoadFirstValidLicense(provider.BundledLicenseDirectory, out var bundledPath);
            if (bundled is null) return false;

            filePath = bundledPath;
            license = bundled;
            return true;
        }
        catch
        {
            return false;
        }
    }


    /// <summary>
    /// Installs a verified license into the user config folder and uninstalls the ones it replaces.
    /// </summary>
    /// <param name="error">The failure when this returns false, so the caller can report it.</param>
    /// <remarks>
    /// The install folder is left alone: a license deployed there is the admin's, not the user's.
    /// </remarks>
    public static bool TryInstall(string sourcePath, LicenseInfo license, out Exception? error)
    {
        error = null;
        string destPath;

        try
        {
            destPath = BHelper.ConfigDir(license.LicenseId + LICENSE_FILE_EXTENSION);

            // re-importing the installed file: nothing to copy, but still drop the others
            var isAlreadyInstalled = ArePathsEqual(sourcePath, destPath);
            if (!isAlreadyInstalled) File.Copy(sourcePath, destPath, true);
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }

        UninstallLicensesExcept(destPath);
        return true;
    }


    /// <summary>
    /// Deletes every expired, signature-verified license file so the app stops asking about it.
    /// </summary>
    /// <param name="error">The first failure, e.g. a license in a read-only install folder.</param>
    public static bool TryUninstallExpiredLicenses(out Exception? error)
    {
        error = null;

        foreach (var dir in new[] { BHelper.BaseDir(), BHelper.ConfigPath })
        {
            var files = EnumerateLicenseFiles(dir);

            foreach (var path in files)
            {
                var isAuthentic = TryVerify(path, out var lic, out _);
                if (!isAuthentic) continue;

                var isWithinValidity = IsWithinValidity(lic);
                if (isWithinValidity) continue;

                try { File.Delete(path); }
                catch (Exception ex) { error ??= ex; }
            }
        }

        return error is null;
    }


    /// <summary>
    /// Deletes every license installed in the user config folder except <paramref name="keepPath"/>.
    /// </summary>
    private static void UninstallLicensesExcept(string keepPath)
    {
        // File.Copy keeps the source timestamp, so a leftover can still sort newest and win
        var files = EnumerateLicenseFiles(BHelper.ConfigPath);

        foreach (var path in files)
        {
            // the license is already installed, so a stale file left behind must not fail it
            try
            {
                var isKeptFile = ArePathsEqual(path, keepPath);
                if (isKeptFile) continue;

                File.Delete(path);
            }
            catch { }
        }
    }


    /// <summary>
    /// True when both paths point at the same file.
    /// </summary>
    private static bool ArePathsEqual(string pathA, string pathB)
    {
        var fullPathA = Path.GetFullPath(pathA);
        var fullPathB = Path.GetFullPath(pathB);

        return string.Equals(fullPathA, fullPathB, StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Returns the newest authentic, in-validity license in a folder, and the path it came from.
    /// Version scope is left to the caller.
    /// </summary>
    private static LicenseInfo? TryLoadFirstValidLicense(string? dir, out string filePath)
    {
        filePath = string.Empty;
        var files = EnumerateLicenseFiles(dir);

        foreach (var path in files)
        {
            var isAuthentic = TryVerify(path, out var lic, out _);
            if (!isAuthentic) continue;

            var isWithinValidity = IsWithinValidity(lic);
            if (!isWithinValidity) continue;

            filePath = path;
            return lic;
        }

        return null;
    }


    /// <summary>
    /// Lists the license files directly in a folder, newest first. Not recursive, so a license in
    /// a subfolder is never picked up by accident. Returns empty on any problem.
    /// </summary>
    private static List<string> EnumerateLicenseFiles(string? dir)
    {
        if (string.IsNullOrEmpty(dir)) return [];

        var dirExists = Directory.Exists(dir);
        if (!dirExists) return [];

        List<string> files;
        try
        {
            files = new List<string>(Directory.EnumerateFiles(dir, LICENSE_FILE_PATTERN));
        }
        catch
        {
            return [];
        }

        files.Sort(static (a, b) => LastWrite(b).CompareTo(LastWrite(a)));
        return files;
    }


    /// <summary>
    /// Parses and verifies a license file's signature. Ignores expiry (see <see cref="IsWithinValidity"/>).
    /// </summary>
    public static bool TryVerify(string filePath, out LicenseInfo license, out string errorCode)
    {
        license = null!;
        errorCode = string.Empty;

        try
        {
            if (!File.Exists(filePath))
            {
                errorCode = "IGE_FILE_NOT_FOUND";
                return false;
            }

            var lic = BHelper.ReadJsonFromFile(filePath, LicenseJsonContext.Default.LicenseInfo);
            if (lic is null)
            {
                errorCode = "IGE_INVALID_JSON";
                return false;
            }

            if (!string.Equals(lic.Product, PRODUCT_NAME, StringComparison.Ordinal))
            {
                errorCode = "IGE_INVALID_PRODUCT_NAME";
                return false;
            }
            if (string.IsNullOrEmpty(lic.KeyId) || string.IsNullOrEmpty(lic.Signature))
            {
                errorCode = "IGE_INVALID_SIGNATURE";
                return false;
            }

            var pem = GetPublicKeyPem(lic.KeyId);
            if (pem is null)
            {
                errorCode = "IGE_INVALID_SIGNATURE";
                return false;
            }

            byte[] signature;
            try { signature = Convert.FromBase64String(lic.Signature); }
            catch
            {
                errorCode = "IGE_INVALID_SIGNATURE";
                return false;
            }

            var payload = Encoding.UTF8.GetBytes(LicenseSigningPayload.Build(lic));
            if (!LicenseVerifier.Verify(payload, signature, pem))
            {
                errorCode = "IGE_INVALID_SIGNATURE";
                return false;
            }

            license = lic;
            return true;
        }
        catch
        {
            errorCode = "IGE_INVALID_FILE";
            return false;
        }
    }


    /// <summary>
    /// True when the license is perpetual or its expiry instant has not passed yet.
    /// </summary>
    public static bool IsWithinValidity(LicenseInfo license)
    {
        var expiresAt = GetExpiryUtc(license);
        if (expiresAt is null) return true;

        // the whole timestamp counts, not just its date, so a same-day expiry lapses on the minute
        return DateTimeOffset.UtcNow <= expiresAt.Value;
    }


    /// <summary>
    /// Parses the expiry, or null when perpetual or unreadable (lenient, trust-based).
    /// </summary>
    public static DateTimeOffset? GetExpiryUtc(LicenseInfo license)
    {
        if (string.IsNullOrEmpty(license.ExpiresAt)) return null;

        var isParsed = DateTimeOffset.TryParse(license.ExpiresAt, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var expiresAt);

        return isParsed ? expiresAt : null;
    }


    private static DateTime LastWrite(string path)
    {
        try { return File.GetLastWriteTimeUtc(path); }
        catch { return DateTime.MinValue; }
    }


    /// <summary>
    /// Reads and caches the embedded public-key PEM for a keyId.
    /// </summary>
    private static string? GetPublicKeyPem(string keyId)
    {
        lock (_lock)
        {
            if (_pemCache.TryGetValue(keyId, out var cached)) return cached;

            string? pem = null;
            if (_keyResources.TryGetValue(keyId, out var resourceName))
            {
                try
                {
                    using var stream = typeof(LicenseService).Assembly.GetManifestResourceStream(resourceName);
                    if (stream is not null)
                    {
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        pem = reader.ReadToEnd();
                    }
                }
                catch
                {
                    pem = null;
                }
            }

            _pemCache[keyId] = pem;
            return pem;
        }
    }
}

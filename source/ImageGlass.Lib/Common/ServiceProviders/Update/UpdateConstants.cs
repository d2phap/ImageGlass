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

namespace ImageGlass.Common.ServiceProviders.Update;

internal static class UpdateConstants
{
    /// <summary>
    /// Update metadata endpoint (HTTPS only).
    /// </summary>
    public const string MetadataUrl = "https://imageglass.org/url/update";

    /// <summary>
    /// Fallback URL when update check fails.
    /// </summary>
    public const string FallbackReleasesUrl = "https://github.com/d2phap/ImageGlass/releases";

    /// <summary>
    /// Microsoft Store product page. The Store owns updating for that package, so its build sends
    /// the user here instead of to the download/changelog page.
    /// </summary>
    public const string MsStoreProductUrl = "ms-windows-store://pdp/?productid=9N33VZK3C7TH";

    /// <summary>
    /// Pro pricing page, the online purchase route offered next to the Store one.
    /// </summary>
    public const string ProPricingUrl = "https://imageglass.org/pricing";

    /// <summary>
    /// Maximum metadata response size (1 MB).
    /// </summary>
    public const long MaxMetadataSize = 1 * 1024 * 1024;

    /// <summary>
    /// Metadata fetch timeout.
    /// </summary>
    public static readonly TimeSpan MetadataTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Cache subfolder for the downloaded update package; NOT under _temp, which is wiped on exit.
    /// </summary>
    public const string PackageCacheDir = "_update";

    /// <summary>
    /// Maximum update package size (1 GB).
    /// </summary>
    public const long MaxPackageSize = 1024L * 1024 * 1024;

    /// <summary>
    /// Whole-download timeout; generous, since this runs in the background.
    /// </summary>
    public static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Lock file serializing the download across app instances.
    /// </summary>
    public const string DownloadLockFile = ".download.lock";

    /// <summary>
    /// Release notes of the pending update, cached so the install prompt works offline.
    /// </summary>
    public const string PendingReleaseFile = "release.json";


    /// <summary>
    /// Marker written just before an install; it survives only when that install did not land.
    /// </summary>
    public const string ApplyAttemptFile = ".apply-attempt";


    /// <summary>
    /// How many times to retry the download lock before giving up.
    /// </summary>
    public const int DownloadLockRetries = 60;

    /// <summary>
    /// Delay between download-lock attempts, in milliseconds.
    /// </summary>
    public const int DownloadLockRetryDelayMs = 500;

    /// <summary>
    /// Default background check interval (7 days).
    /// </summary>
    public static readonly TimeSpan BackgroundCheckInterval = TimeSpan.FromDays(7);

    /// <summary>
    /// Upper bound of the random offset added to <see cref="BackgroundCheckInterval"/> (24 hours),
    /// so same-cadence installs do not all check on the same day.
    /// </summary>
    public const int CheckJitterMinutes = 24 * 60;
}

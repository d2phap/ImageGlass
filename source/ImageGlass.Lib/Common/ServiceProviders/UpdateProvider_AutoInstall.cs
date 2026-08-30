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
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders.Update;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public sealed partial class UpdateProvider
{
    /// <summary>
    /// Whether installing an update in place is available at all (channel, license, admin policy).
    /// </summary>
    public static bool CanInstallUpdate => Core.UpdateInstaller?.IsSupported == true
        && !FeatureManager.IsLocked(API.IG_RestartToUpdate);


    /// <summary>
    /// Whether this build may download updates itself right now.
    /// </summary>
    public static bool CanAutoInstall => Core.IsProEnabled
        && Core.Config?.EnableAutoInstallUpdate == true
        && CanInstallUpdate;


    /// <summary>
    /// Whether a downloaded update is waiting and may be offered to the user.
    /// </summary>
    public static bool CanApplyPendingUpdate => Core.HasPendingUpdate && CanInstallUpdate;


    /// <summary>
    /// Path of the verified package waiting to be installed, or <c>null</c>.
    /// </summary>
    /// <remarks>
    /// By version alone: there is no manifest at restart time, and MSIX signature checking is the real gate.
    /// </remarks>
    public static string? GetPendingPackagePath()
    {
        var version = Core.Config?.UpdatePendingVersion;
        if (string.IsNullOrWhiteSpace(version)) return null;

        return AppUpdateDownloader.FindCachedPackage(version);
    }


    /// <summary>
    /// Returns the artifact this build can install, or <c>null</c> when the manifest has none.
    /// </summary>
    public static UpdateArtifactInfo? ResolveArtifact(UpdateReleaseInfo? release)
    {
        var key = Core.UpdateInstaller?.ArtifactKey;
        if (release?.Artifacts is null || string.IsNullOrEmpty(key)) return null;

        return release.Artifacts.TryGetValue(key, out var artifact) && artifact.IsInstallable
            ? artifact
            : null;
    }


    /// <summary>
    /// Forgets the pending update and deletes its package.
    /// </summary>
    public static void DiscardPendingUpdate()
    {
        AppUpdateDownloader.ClearCache();

        if (Core.Config is not null) Core.Config.UpdatePendingVersion = string.Empty;
    }


    /// <summary>
    /// Drops a pending update that already installed, or whose package went missing.
    /// </summary>
    public static async Task ReconcilePendingUpdateAsync()
    {
        try
        {
            var config = Core.Config;
            var pending = config?.UpdatePendingVersion;
            if (config is null || string.IsNullOrWhiteSpace(pending)) return;

            // the running build is at or past the pending one, so the update landed
            var isApplied = CompareVersions(Core.BuildInfo.Version, pending) <= 0;

            // a skip that raced an in-flight download can leave both flags set on the same version
            var isSkipped = !isApplied
                && string.Equals(pending, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase);

            // the attention UI must never point at a package that is no longer on disk
            var isMissing = !isApplied && !isSkipped && GetPendingPackagePath() is null;

            if (!isApplied && !isSkipped && !isMissing) return;

            var reason = isApplied ? "applied" : isSkipped ? "skipped" : "missing";
            UpdateTrace.Mark($"reconcile:{reason} {pending}");
            DiscardPendingUpdate();

            // persist now, so a crash before the next clean exit cannot resurrect the flag
            _ = await config.SaveAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"reconcile:error {ex.Message}");
        }
    }


    /// <summary>
    /// Downloads and verifies the package for a scheduled check; true when one is ready. Never throws.
    /// </summary>
    public static async Task<bool> TryDownloadUpdateAsync(UpdateCheckResult result, CancellationToken ct)
    {
        try
        {
            if (!CanAutoInstall) return false;

            var release = result.Release;
            if (result.Status != UpdateCheckStatus.UpdateAvailable || release is null) return false;

            var config = Core.Config;
            var installer = Core.UpdateInstaller;
            if (config is null || installer is null) return false;

            // an explicitly skipped version is never fetched behind the user's back
            if (string.Equals(release.Version, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase))
                return false;

            // already downloaded and verified in an earlier session
            if (string.Equals(config.UpdatePendingVersion, release.Version, StringComparison.OrdinalIgnoreCase)
                && GetPendingPackagePath() is not null) return true;

            if (!installer.RequiresDownload) return false;

            var artifact = ResolveArtifact(release);
            if (artifact is null)
            {
                UpdateTrace.Mark($"download:noArtifact key={installer.ArtifactKey}");
                return false;
            }

            if (installer.IsMeteredConnection)
            {
                UpdateTrace.Mark("download:skipMetered");
                return false;
            }

            // only one instance may fetch; a lock file, since a Mutex cannot be held across an await
            using var gate = AppUpdateDownloader.TryAcquireDownloadLock();
            if (gate is null)
            {
                UpdateTrace.Mark("download:skipBusy");
                return false;
            }

            var path = await AppUpdateDownloader
                .DownloadAsync(_httpClient, artifact, release.Version, null, ct)
                .ConfigureAwait(false);
            if (path is null) return false;

            // a skip during this download had no pending version to retract, so honour it now
            if (string.Equals(release.Version, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase))
            {
                UpdateTrace.Mark($"download:skippedMidFlight {release.Version}");
                AppUpdateDownloader.ClearCache();
                return false;
            }

            config.UpdatePendingVersion = release.Version;

            // an ignored update would otherwise leave its package behind on every release
            AppUpdateDownloader.PruneCacheExcept(release.Version);

            // config is otherwise only written on close, and a crash would lose the pending state
            _ = await Core.Config.SaveAsync().ConfigureAwait(false);

            UpdateTrace.Mark($"download:ready {release.Version}");
            return true;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"download:unhandled {ex.Message}");
            return false;
        }
    }
}

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
    /// Whether this build may download updates itself right now.
    /// </summary>
    public static bool CanAutoInstall => Core.IsProEnabled
        && Core.Config?.EnableAutoInstallUpdate == true
        && Core.UpdateInstaller?.IsSupported == true;


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
    public static void ReconcilePendingUpdate()
    {
        try
        {
            var pending = Core.Config?.UpdatePendingVersion;
            if (string.IsNullOrWhiteSpace(pending)) return;

            // the running build is at or past the pending one, so the update landed
            if (CompareVersions(Core.BuildInfo.Version, pending) <= 0)
            {
                UpdateTrace.Mark($"reconcile:applied {pending}");
                DiscardPendingUpdate();
                return;
            }

            // the attention UI must never point at a package that is no longer on disk
            if (GetPendingPackagePath() is null)
            {
                UpdateTrace.Mark($"reconcile:missing {pending}");
                DiscardPendingUpdate();
            }
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"reconcile:error {ex.Message}");
        }
    }


    /// <summary>
    /// Downloads and verifies the update package for a scheduled check; returns <c>true</c> when a
    /// package is ready to install. Never throws.
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

            // every instance runs the scheduled check, so only one of them may fetch.
            // a lock FILE, not a Mutex: Mutex is thread-affine and cannot be held across an await
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

            config.UpdatePendingVersion = release.Version;

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

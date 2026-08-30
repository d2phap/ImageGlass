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
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;

public partial class UpdateProvider
{

    #region Platform install surface (overridden per packaging format)

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual bool IsInstallSupported => false;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual string ArtifactKey => string.Empty;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual bool RequiresDownload => true;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual bool IsMeteredConnection => false;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public virtual Task<UpdateOpResult> ApplyAndRestartAsync(string packageFilePath,
        UpdateReleaseInfo release, CancellationToken ct = default)
        => Task.FromResult(UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates."));

    #endregion // Platform install surface



    #region Eligibility

    /// <summary>
    /// Whether installing an update in place is available at all (channel, admin policy).
    /// </summary>
    public bool CanInstallUpdate => IsInstallSupported && !FeatureManager.IsLocked(API.IG_InstallUpdate);


    /// <summary>
    /// Whether the user can install an update from inside the app (licence, channel, policy).
    /// </summary>
    public bool CanInstallUpdateInApp => Core.IsProEnabled && CanInstallUpdate;


    /// <summary>
    /// Whether this build may download updates in the background without being asked.
    /// </summary>
    public bool CanAutoInstall => CanInstallUpdateInApp
        && Core.Config.EnableAutoInstallUpdate;


    /// <summary>
    /// Whether a downloaded update is waiting and may be offered to the user.
    /// </summary>
    public bool CanApplyPendingUpdate => Core.HasPendingUpdate && CanInstallUpdate;

    #endregion // Eligibility



    /// <summary>
    /// Why the previous install attempt never landed, or <c>null</c>; set once by the reconcile pass.
    /// </summary>
    public UpdateOpResult? LastApplyFailure { get; private set; }


    /// <summary>
    /// Path of the verified package waiting to be installed, or <c>null</c>.
    /// </summary>
    /// <remarks>
    /// By version alone: there is no manifest at restart time, and MSIX signature checking is the real gate.
    /// </remarks>
    public string? GetPendingPackagePath()
    {
        var version = Core.Config.UpdatePendingVersion;
        if (string.IsNullOrWhiteSpace(version)) return null;

        return AppUpdateDownloader.FindCachedPackage(version);
    }


    /// <summary>
    /// Records that an install is starting, so a deployment that kills us can still be reported.
    /// </summary>
    protected static void MarkApplyAttempt(string version) => AppUpdateDownloader.MarkApplyAttempt(version);


    /// <summary>
    /// Release notes of the pending update, or <c>null</c> when they do not match what is armed.
    /// </summary>
    public UpdateReleaseInfo? GetPendingRelease()
    {
        var version = Core.Config.UpdatePendingVersion;
        if (string.IsNullOrWhiteSpace(version)) return null;

        var release = AppUpdateDownloader.ReadPendingRelease();
        return string.Equals(release?.Version, version, StringComparison.OrdinalIgnoreCase) ? release : null;
    }


    /// <summary>
    /// Returns the artifact this build can install, or <c>null</c> when the manifest has none.
    /// </summary>
    public UpdateArtifactInfo? ResolveArtifact(UpdateReleaseInfo? release)
    {
        if (release?.Artifacts is null || string.IsNullOrEmpty(ArtifactKey)) return null;

        return release.Artifacts.TryGetValue(ArtifactKey, out var artifact) && artifact.IsInstallable
            ? artifact
            : null;
    }


    /// <summary>
    /// Forgets the pending update and deletes its package.
    /// </summary>
    public void DiscardPendingUpdate()
    {
        AppUpdateDownloader.ClearCache();
        Core.Config.UpdatePendingVersion = string.Empty;
    }


    /// <summary>
    /// Drops a pending update that already installed, was skipped, or whose package went missing.
    /// </summary>
    public async Task ReconcilePendingUpdateAsync()
    {
        try
        {
            var config = Core.Config;
            var pending = config.UpdatePendingVersion;
            if (string.IsNullOrWhiteSpace(pending)) return;

            // the running build is at or past the pending one, so the update landed
            var isApplied = CompareVersions(Core.BuildInfo.Version, pending) <= 0;

            // a skip that raced an in-flight download can leave both flags set on the same version
            var isSkipped = !isApplied
                && string.Equals(pending, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase);

            // deployment shut us down, then failed: the marker outlived the install it was tracking
            var failedAttempt = !isApplied && !isSkipped
                && string.Equals(AppUpdateDownloader.ReadApplyAttempt(), pending, StringComparison.OrdinalIgnoreCase);

            // the attention UI must never point at a package that is no longer on disk
            var isMissing = !isApplied && !isSkipped && !failedAttempt && GetPendingPackagePath() is null;

            if (!isApplied && !isSkipped && !failedAttempt && !isMissing) return;

            var reason = isApplied ? "applied" : isSkipped ? "skipped" : failedAttempt ? "applyFailed" : "missing";
            UpdateTrace.Mark($"reconcile:{reason} {pending}");

            // retrying an install that already died mid-flight just loops, so drop it and say so
            if (failedAttempt)
            {
                LastApplyFailure = UpdateOpResult.Fail(
                    $"IGE: ImageGlass {pending} could not be installed, so the update was discarded.",
                    $"Windows shut the app down to install the package, then rejected it."
                    + $"{Environment.NewLine}Installed version: {Core.BuildInfo.Version}");
            }

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
    /// Downloads and verifies the package for a scheduled check. Never throws.
    /// </summary>
    public async Task<UpdateOpResult> TryDownloadUpdateAsync(UpdateCheckResult result, CancellationToken ct)
    {
        try
        {
            if (!CanAutoInstall) return UpdateOpResult.Skip();

            var release = result.Release;
            if (result.Status != UpdateCheckStatus.UpdateAvailable || release is null)
                return UpdateOpResult.Skip();

            var config = Core.Config;

            // an explicitly skipped version is never fetched behind the user's back
            if (string.Equals(release.Version, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase))
                return UpdateOpResult.Skip();

            // already downloaded and verified in an earlier session
            if (string.Equals(config.UpdatePendingVersion, release.Version, StringComparison.OrdinalIgnoreCase)
                && GetPendingPackagePath() is not null) return UpdateOpResult.Ok();

            if (!RequiresDownload) return UpdateOpResult.Skip();

            var artifact = ResolveArtifact(release);
            if (artifact is null)
            {
                UpdateTrace.Mark($"download:noArtifact key={ArtifactKey}");
                return UpdateOpResult.Skip();
            }

            if (IsMeteredConnection)
            {
                UpdateTrace.Mark("download:skipMetered");
                return UpdateOpResult.Skip();
            }

            return await DownloadAndArmAsync(release, artifact, null, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"download:unhandled {ex}");
            return UpdateOpResult.Fail(ex);
        }
    }


    /// <summary>
    /// Downloads the package for a user-initiated install, reporting progress 0-100. Never throws.
    /// </summary>
    /// <remarks>
    /// Ignores the metered check: an explicit click is consent.
    /// </remarks>
    public async Task<UpdateOpResult> TryDownloadForInstallAsync(UpdateReleaseInfo? release,
        IProgress<double>? progress, CancellationToken ct)
    {
        try
        {
            if (release is null) return UpdateOpResult.Fail("IGE: No release information was returned.");
            if (!CanInstallUpdateInApp)
                return UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates.");

            // already downloaded in an earlier session
            if (string.Equals(Core.Config.UpdatePendingVersion, release.Version, StringComparison.OrdinalIgnoreCase)
                && GetPendingPackagePath() is not null) return UpdateOpResult.Ok();

            if (!RequiresDownload) return UpdateOpResult.Skip();

            var artifact = ResolveArtifact(release);
            if (artifact is null)
            {
                return UpdateOpResult.Fail(
                    $"IGE: The update manifest has no installable '{ArtifactKey}' artifact for this build.");
            }

            return await DownloadAndArmAsync(release, artifact, progress, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"manual:unhandled {ex}");
            return UpdateOpResult.Fail(ex);
        }
    }


    /// <summary>
    /// Fetches and verifies the package, then records it as the pending update.
    /// </summary>
    private async Task<UpdateOpResult> DownloadAndArmAsync(UpdateReleaseInfo release,
        UpdateArtifactInfo artifact, IProgress<double>? progress, CancellationToken ct)
    {
        var config = Core.Config;

        // only one instance may fetch; a lock file, since a Mutex cannot be held across an await
        using var gate = await AppUpdateDownloader.AcquireDownloadLockAsync(ct).ConfigureAwait(false);
        if (gate is null)
        {
            UpdateTrace.Mark("download:busy");
            return UpdateOpResult.Fail("IGE: Another ImageGlass instance is already downloading this update.");
        }

        // the lock may have been held by an instance that finished the very same download
        if (string.Equals(config.UpdatePendingVersion, release.Version, StringComparison.OrdinalIgnoreCase)
            && GetPendingPackagePath() is not null) return UpdateOpResult.Ok();

        var download = await AppUpdateDownloader
            .DownloadAsync(_httpClient, artifact, release.Version, progress, ct)
            .ConfigureAwait(false);
        if (!download.IsSuccess) return download;

        // a skip during this download had no pending version to retract, so honour it now
        if (string.Equals(release.Version, config.UpdateSkippedVersion, StringComparison.OrdinalIgnoreCase))
        {
            UpdateTrace.Mark($"download:skippedMidFlight {release.Version}");
            DiscardPendingUpdate();
            return UpdateOpResult.Skip();
        }

        config.UpdatePendingVersion = release.Version;

        // the install prompt shows these, and it must work without going back to the network
        await AppUpdateDownloader.SavePendingReleaseAsync(release).ConfigureAwait(false);

        // an ignored update would otherwise leave its package behind on every release
        AppUpdateDownloader.PruneCacheExcept(release.Version);

        // config is otherwise only written on close, and a crash would lose the pending state
        _ = await config.SaveAsync().ConfigureAwait(false);

        UpdateTrace.Mark($"download:ready {release.Version}");
        return UpdateOpResult.Ok();
    }
}

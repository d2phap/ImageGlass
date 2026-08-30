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
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Checks for app updates and, where the packaging format allows it, installs them in place.
/// </summary>
public interface IUpdateProvider
{

    #region Platform surface

    /// <summary>
    /// Whether this build can install its own updates (channel, packaging and OS version).
    /// </summary>
    bool IsInstallSupported { get; }


    /// <summary>
    /// Key into <see cref="UpdateReleaseInfo.Artifacts"/>, e.g. <c>win-x64-msix</c>.
    /// </summary>
    string ArtifactKey { get; }


    /// <summary>
    /// Whether the host downloads the artifact itself; <c>false</c> when the platform fetches it.
    /// </summary>
    bool RequiresDownload { get; }


    /// <summary>
    /// Whether the connection is metered; <c>false</c> when unknown, so it never blocks updates.
    /// </summary>
    bool IsMeteredConnection { get; }


    /// <summary>
    /// Installs the package and relaunches; on success the process does not return.
    /// </summary>
    Task<UpdateOpResult> ApplyAndRestartAsync(string packageFilePath, UpdateReleaseInfo release,
        CancellationToken ct = default);

    #endregion // Platform surface



    #region Eligibility

    /// <summary>
    /// Whether installing an update in place is available at all (channel, admin policy).
    /// </summary>
    bool CanInstallUpdate { get; }


    /// <summary>
    /// Whether the user can install an update from inside the app (licence, channel, policy).
    /// </summary>
    bool CanInstallUpdateInApp { get; }


    /// <summary>
    /// Whether this build may download updates in the background without being asked.
    /// </summary>
    bool CanAutoInstall { get; }


    /// <summary>
    /// Whether a downloaded update is waiting and may be offered to the user.
    /// </summary>
    bool CanApplyPendingUpdate { get; }

    #endregion // Eligibility



    /// <summary>
    /// Fetches the manifest and compares versions. Never throws.
    /// </summary>
    Task<UpdateCheckResult> CheckForUpdateAsync(CancellationToken ct, bool isScheduled = false);


    /// <summary>
    /// Why the previous install attempt never landed, or <c>null</c>; set once by the reconcile pass.
    /// </summary>
    UpdateOpResult? LastApplyFailure { get; }


    /// <summary>
    /// Path of the verified package waiting to be installed, or <c>null</c>.
    /// </summary>
    string? GetPendingPackagePath();


    /// <summary>
    /// Returns the artifact this build can install, or <c>null</c> when the manifest has none.
    /// </summary>
    UpdateArtifactInfo? ResolveArtifact(UpdateReleaseInfo? release);


    /// <summary>
    /// Forgets the pending update and deletes its package.
    /// </summary>
    void DiscardPendingUpdate();


    /// <summary>
    /// Drops a pending update that already installed, was skipped, or whose package went missing.
    /// </summary>
    Task ReconcilePendingUpdateAsync();


    /// <summary>
    /// Downloads and verifies the package for a scheduled check. Never throws.
    /// </summary>
    Task<UpdateOpResult> TryDownloadUpdateAsync(UpdateCheckResult result, CancellationToken ct);


    /// <summary>
    /// Downloads the package for a user-initiated install, reporting progress 0-100. Never throws.
    /// </summary>
    Task<UpdateOpResult> TryDownloadForInstallAsync(UpdateReleaseInfo? release,
        IProgress<double>? progress, CancellationToken ct);

}

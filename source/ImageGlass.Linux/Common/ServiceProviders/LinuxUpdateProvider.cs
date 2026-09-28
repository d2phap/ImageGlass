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
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.Common.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;


/// <summary>
/// Update provider for Linux; installs in place for the AppImage and the Flatpak bundle only.
/// </summary>
public partial class LinuxUpdateProvider : UpdateProvider
{
    /// <summary>
    /// How long the relaunched build must stay up before this instance exits for it.
    /// </summary>
    private const int RELAUNCH_PROBE_MS = 2_000;

    private InterlockedBool _isApplying;


    public LinuxUpdateProvider()
    {
        // eligibility needs two host round trips, so answer them before anything asks
        _flatpakProbe = BHelper.IsFlatpakSandbox
            ? Task.Run(DetectFlatpakBundleAsync)
            : Task.FromResult<FlatpakInstall?>(null);
    }



    #region Platform surface

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsInstallSupported => BHelper.IsAppImage
        ? IsAppImageReplaceable
        : FlatpakBundle is not null;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ArtifactKey => BHelper.IsAppImage ? $"linux-{BHelper.ArchToken}-appimage"
        : BHelper.IsFlatpakSandbox ? $"linux-{BHelper.ArchToken}-flatpak"
        : string.Empty;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsMeteredConnection => XdgPortal.IsNetworkMetered();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async Task<UpdateOpResult> ApplyAndRestartAsync(string packageFilePath,
        UpdateReleaseInfo release, CancellationToken ct = default)
    {
        if (!IsInstallSupported)
        {
            return UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates.");
        }

        if (!File.Exists(packageFilePath))
        {
            return UpdateOpResult.Fail($"IGE: The downloaded update package is missing: {packageFilePath}");
        }

        // nothing kills this process mid-install as MSIX deployment does, so refuse a second click
        if (!_isApplying.SetTrue()) return UpdateOpResult.Skip();

        UpdateOpResult result;
        try
        {
            UpdateTrace.Mark($"apply:begin {release.Version}");

            // file copies and host commands, none of which may block the UI thread
            result = await Task.Run(() => BHelper.IsAppImage
                ? ApplyAppImageAsync(packageFilePath, release, ct)
                : ApplyFlatpakBundleAsync(packageFilePath, release), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:exception {ex}");
            result = UpdateOpResult.Fail(ex);
        }

        if (!result.IsSuccess)
        {
            UpdateTrace.Mark($"apply:failed {result.ErrorMessage}");
            _isApplying.SetFalse();
            return result;
        }

        // the new build already runs and holds the instance lock, so this one only has to leave
        UpdateTrace.Mark("apply:ok");
        Dispatcher.UIThread.Post(() => BHelper.ExitApp(false));

        return result;
    }

    #endregion // Platform surface



    #region Private Methods

    /// <summary>
    /// Forgets the installed update before relaunching, or this instance's close-time save re-arms it.
    /// </summary>
    private async Task ForgetInstalledUpdateAsync()
    {
        // config change handlers touch controls, so raise it where they live
        await Dispatcher.UIThread.InvokeAsync(DiscardPendingUpdate);
        _ = await Core.Config.SaveAsync().ConfigureAwait(false);
    }


    /// <summary>
    /// Starts the updated build; fails when it quits with an error within <see cref="RELAUNCH_PROBE_MS"/>.
    /// </summary>
    private static async Task<UpdateOpResult> RelaunchAsync(ProcessStartInfo psi)
    {
        // otherwise the new process hands its launch over to this exiting one, then quits
        Core.AppInstance.Dispose();

        try
        {
            using var proc = Process.Start(psi);
            if (proc is null)
            {
                return UpdateOpResult.Fail("IGE: The updated ImageGlass could not be started.", DescribeCommand(psi));
            }

            UpdateTrace.Mark($"apply:relaunched pid={proc.Id}");

            try
            {
                using var probe = new CancellationTokenSource(RELAUNCH_PROBE_MS);
                await proc.WaitForExitAsync(probe.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return UpdateOpResult.Ok();
            }

            // exit code 0 is not a failed start, e.g. the launch was handed to another instance
            if (proc.ExitCode == 0) return UpdateOpResult.Ok();

            UpdateTrace.Mark($"apply:relaunchFailed exit={proc.ExitCode}");
            return UpdateOpResult.Fail($"IGE: The updated ImageGlass quit at startup with exit code {proc.ExitCode}.",
                DescribeCommand(psi));
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:relaunchFailed {ex.Message}");
            return UpdateOpResult.Fail($"IGE: The updated ImageGlass could not be started: {ex.Message}",
                $"{DescribeCommand(psi)}{Environment.NewLine}{BHelper.GetExceptionDetails(ex)}");
        }
    }


    /// <summary>
    /// The command line of <paramref name="psi"/>, for an error's details.
    /// </summary>
    private static string DescribeCommand(ProcessStartInfo psi)
    {
        return $"Command: {psi.FileName} {string.Join(' ', psi.ArgumentList)}";
    }


    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    #endregion // Private Methods

}

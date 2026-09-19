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
using ImageGlass.Common;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.Win32.Common.WinAPI;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Networking.Connectivity;

namespace ImageGlass.Win32.Common.ServiceProviders;


/// <summary>
/// Update provider for Windows; installs an MSIX in place for the signed sideload flavour only.
/// </summary>
public class Win32UpdateProvider : UpdateProvider
{
    /// <summary>
    /// AddPackageOptions and AddPackageByUriAsync arrived in Windows 10 2004.
    /// </summary>
    private const int MIN_BUILD = 19041;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsInstallSupported => Win32AppIdentity.IsPackaged
        && Win32AppIdentity.IsUnvirtualizedResources
        && OperatingSystem.IsWindowsVersionAtLeast(10, 0, MIN_BUILD);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ArtifactKey => $"win-{BHelper.ArchToken}-msix";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool IsMeteredConnection
    {
        get
        {
            // an unreadable cost must not block updates forever, so failure reads as unmetered
            try
            {
                var cost = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();
                if (cost is null) return false;

                return cost.NetworkCostType != NetworkCostType.Unrestricted
                    || cost.Roaming
                    || cost.OverDataLimit;
            }
            catch { return false; }
        }
    }


    /// <summary>
    /// MSIX identity version of the running package; a separate axis from the app version, and the
    /// one Windows compares, so a stalled update almost always shows up here.
    /// </summary>
    private static Version InstalledPackageVersion
    {
        get
        {
            try
            {
                var v = global::Windows.ApplicationModel.Package.Current.Id.Version;
                return new Version(v.Major, v.Minor, v.Build, v.Revision);
            }
            catch { return new Version(0, 0, 0, 0); }
        }
    }


    /// <summary>
    /// Identity version declared inside an .msix, or <c>null</c> when it cannot be read.
    /// </summary>
    private static Version? ReadPackageIdentityVersion(string packageFilePath)
    {
        try
        {
            using var zip = ZipFile.OpenRead(packageFilePath);
            var entry = zip.GetEntry("AppxManifest.xml");
            if (entry is null) return null;

            using var reader = new StreamReader(entry.Open());
            var xml = reader.ReadToEnd();

            var tagStart = xml.IndexOf("<Identity", StringComparison.OrdinalIgnoreCase);
            if (tagStart < 0) return null;

            var tagEnd = xml.IndexOf('>', tagStart);
            if (tagEnd < 0) return null;

            const string VERSION_ATTR = "Version=\"";
            var attrStart = xml.IndexOf(VERSION_ATTR, tagStart, tagEnd - tagStart,
                StringComparison.OrdinalIgnoreCase);
            if (attrStart < 0) return null;

            attrStart += VERSION_ATTR.Length;
            var attrEnd = xml.IndexOf('"', attrStart);
            if (attrEnd < 0) return null;

            return Version.TryParse(xml[attrStart..attrEnd], out var v) ? v : null;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:identityUnreadable {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override async Task<UpdateOpResult> ApplyAndRestartAsync(string packageFilePath,
        UpdateReleaseInfo release, CancellationToken ct = default)
    {
        // repeated inline so the platform analyzer can see the guard on the API calls below
        if (!IsInstallSupported || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, MIN_BUILD))
        {
            return UpdateOpResult.Fail("IGE: This ImageGlass build cannot install its own updates.");
        }

        if (!File.Exists(packageFilePath))
        {
            return UpdateOpResult.Fail($"IGE: The downloaded update package is missing: {packageFilePath}");
        }

        // Windows takes us down before it reports a rejection, so refuse what it cannot accept
        var candidate = ReadPackageIdentityVersion(packageFilePath);
        if (candidate is not null && candidate.CompareTo(InstalledPackageVersion) <= 0)
        {
            DiscardPendingUpdate();
            _ = await Core.Config.SaveAsync().ConfigureAwait(false);

            UpdateTrace.Mark($"apply:notNewer {candidate} <= {InstalledPackageVersion}");
            return UpdateOpResult.Fail(
                $"IGE: The update package is version {candidate}, which is not newer than the installed "
                + $"{InstalledPackageVersion}, so Windows cannot install it.",
                $"Package: {packageFilePath}{Environment.NewLine}"
                + $"App version: {Core.BuildInfo.Version} -> {release.Version}{Environment.NewLine}"
                + "The MSIX identity version is a separate axis from the app version; the release "
                + "must raise IgBundleBuild as well.");
        }

        try
        {
            // must be registered before the shutdown starts, or Restart Manager won't relaunch us
            _ = Win32RestartApi.RegisterForRestart();

            // deployment kills this process before returning, so leave evidence for the next launch
            MarkApplyAttempt(release.Version);

            var pm = new PackageManager();
            var options = new AddPackageOptions
            {
                // we are the target app: let deployment close us, register, then Restart Manager relaunches
                ForceTargetAppShutdown = true,
            };

            UpdateTrace.Mark($"apply:begin {release.Version}");
            var result = await pm.AddPackageByUriAsync(new Uri(packageFilePath), options)
                .AsTask(ct)
                .ConfigureAwait(false);

            if (result.ExtendedErrorCode is not null)
            {
                var hr = $"0x{result.ExtendedErrorCode.HResult:X8}";
                var text = string.IsNullOrWhiteSpace(result.ErrorText)
                    ? result.ExtendedErrorCode.Message
                    : result.ErrorText;

                UpdateTrace.Mark($"apply:failed {hr} {text}");
                return UpdateOpResult.Fail($"IGE: Windows could not install the update: {text} ({hr})",
                    $"Package: {packageFilePath}{Environment.NewLine}"
                    + $"HRESULT: {hr}{Environment.NewLine}"
                    + $"Installed package version: {InstalledPackageVersion}{Environment.NewLine}"
                    + $"App version: {Core.BuildInfo.Version} -> {release.Version}{Environment.NewLine}"
                    + $"{result.ExtendedErrorCode}");
            }

            UpdateTrace.Mark("apply:ok");
            return UpdateOpResult.Ok();
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:exception {ex}");
            return UpdateOpResult.Fail(ex);
        }
    }
}

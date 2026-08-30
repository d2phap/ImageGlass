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
using ImageGlass.Common.ServiceProviders.Update;
using ImageGlass.Win32.Common.WinAPI;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.Networking.Connectivity;

namespace ImageGlass.Win32.Common.ServiceProviders;


/// <summary>
/// Installs an MSIX update in place, for the signed sideload flavour only.
/// </summary>
public class Win32MsixUpdateInstaller : IAppUpdateInstaller
{
    /// <summary>
    /// AddPackageOptions and AddPackageByUriAsync arrived in Windows 10 2004.
    /// </summary>
    private const int MIN_BUILD = 19041;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsSupported => Win32AppIdentity.IsPackaged
        && Win32AppIdentity.IsUnvirtualizedResources
        && OperatingSystem.IsWindowsVersionAtLeast(10, 0, MIN_BUILD);


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string ArtifactKey => $"win-{BHelper.ArchToken}-msix";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IsMeteredConnection
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
    /// <inheritdoc/>
    /// </summary>
    public async Task<bool> ApplyAndRestartAsync(string packageFilePath, UpdateReleaseInfo release,
        CancellationToken ct = default)
    {
        // repeated inline so the platform analyzer can see the guard on the API calls below
        if (!IsSupported || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, MIN_BUILD)) return false;

        try
        {
            // must be registered before the shutdown starts, or Restart Manager won't relaunch us
            _ = Win32RestartApi.RegisterForRestart();

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
                UpdateTrace.Mark($"apply:failed 0x{result.ExtendedErrorCode.HResult:X8} {result.ErrorText}");
                return false;
            }

            UpdateTrace.Mark("apply:ok");
            return true;
        }
        catch (Exception ex)
        {
            UpdateTrace.Mark($"apply:exception {ex.Message}");
            return false;
        }
    }
}

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
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.Update;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Linux.Common.ServiceProviders;


/// <summary>
/// Update provider for Linux; installs in place for the AppImage and the Flatpak bundle only.
/// </summary>
public partial class LinuxUpdateProvider : UpdateProvider
{
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

        return await RunInstallThenExitAsync(release.Version, () => BHelper.IsAppImage
            ? ApplyAppImageAsync(packageFilePath, release, ct)
            : ApplyFlatpakBundleAsync(packageFilePath, release), ct).ConfigureAwait(false);
    }

    #endregion // Platform surface



    #region Private Methods

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

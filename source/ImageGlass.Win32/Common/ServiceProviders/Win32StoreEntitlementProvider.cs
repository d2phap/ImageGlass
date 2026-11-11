/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using ImageGlass.Common.ServiceProviders.Licensing;
using System.IO;

namespace ImageGlass.Win32.Common.ServiceProviders;


/// <summary>
/// Windows implementation that reads the Microsoft Store entitlement from the MSIX package
/// identity, with no Store API call and no network.
/// </summary>
public class Win32StoreEntitlementProvider : IStoreEntitlementProvider
{
    /// <summary>
    /// Payload folder holding the license bundled for export. Staged by
    /// <c>__assets/win/script-pack-win-msix.ps1</c> for the msstore flavour only; the signed
    /// flavour refuses to build if it finds one.
    /// </summary>
    private const string BUNDLED_LICENSE_DIR = "_store";

    /// <summary>
    /// Plan sold by the Microsoft Store listing.
    /// </summary>
    private const string PLAN_NAME = "Pro Individual";


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <remarks>
    /// Package identity is the whole check. The Store sells this app as a paid product with a
    /// time-limited trial and refuses to launch it once that trial expires, so a running Store
    /// package is already a licensed one. Should the listing ever become free, or gain an
    /// unlimited trial, this has to be replaced by a live Store license query.
    /// </remarks>
    public bool IsStoreEntitled => Win32AppIdentity.IsMsStorePackage;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string ChannelId => LicenseService.CHANNEL_MSSTORE;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string PlanName => PLAN_NAME;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string? BundledLicenseDirectory
    {
        get
        {
            if (!Win32AppIdentity.IsMsStorePackage) return null;

            var dir = BHelper.BaseDir(BUNDLED_LICENSE_DIR);
            var dirExists = Directory.Exists(dir);
            return dirExists ? dir : null;
        }
    }
}

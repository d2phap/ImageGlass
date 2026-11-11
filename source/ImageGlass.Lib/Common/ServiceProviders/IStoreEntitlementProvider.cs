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
namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// Reports the Pro entitlement carried by a build distributed through a platform app store, where
/// the store itself decides who may run it. Implemented by whichever platform head ships a store
/// build; the slot stays <c>null</c> for a self-distributed build.
/// </summary>
public interface IStoreEntitlementProvider
{
    /// <summary>
    /// Whether this build is a store install that is already entitled to Pro.
    /// </summary>
    bool IsStoreEntitled { get; }

    /// <summary>
    /// Identifies the store in a license's channel field, e.g. <c>msstore</c>.
    /// </summary>
    string ChannelId { get; }

    /// <summary>
    /// Plan this store sells, shown when the build carries no bundled license file.
    /// </summary>
    string PlanName { get; }

    /// <summary>
    /// Folder next to the app holding the signed license bundled for use on other platforms, or
    /// <c>null</c> when this build carries none.
    /// </summary>
    string? BundledLicenseDirectory { get; }
}

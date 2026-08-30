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
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Installs an app update in place for the packaging format this build was distributed as.
/// </summary>
public interface IAppUpdateInstaller
{
    /// <summary>
    /// Whether this build can install its own updates (channel, packaging and OS version).
    /// </summary>
    bool IsSupported { get; }


    /// <summary>
    /// Key into <see cref="UpdateReleaseInfo.Artifacts"/>, e.g. <c>win-x64-msix</c>.
    /// </summary>
    string ArtifactKey { get; }


    /// <summary>
    /// Whether the host downloads the artifact itself; <c>false</c> when the platform fetches it.
    /// </summary>
    bool RequiresDownload => true;


    /// <summary>
    /// Whether the connection is metered; <c>false</c> when unknown, so it never blocks updates.
    /// </summary>
    bool IsMeteredConnection => false;


    /// <summary>
    /// Installs the package and relaunches; <c>false</c> means the app keeps running unchanged.
    /// </summary>
    Task<bool> ApplyAndRestartAsync(string packageFilePath, UpdateReleaseInfo release,
        CancellationToken ct = default);
}

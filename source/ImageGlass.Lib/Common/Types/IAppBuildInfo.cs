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
namespace ImageGlass.Common.Types;

public interface IAppBuildInfo
{
    /// <summary>
    /// Gets the app version string. e.g. <c>10.0.0.306</c>.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the full version string. e.g. <c>10.0.0.306-beta-win32-x64</c>.
    /// </summary>
    string FullVersion { get; }

    /// <summary>
    /// Gets the release type. e.g. <c>stable</c>, <c>beta</c>, <c>preview</c>.
    /// </summary>
    string ReleaseType { get; }

    /// <summary>
    /// Gets the update channel, either <c>stable</c> or <c>beta</c>.
    /// </summary>
    string UpdateChannel { get; }
}


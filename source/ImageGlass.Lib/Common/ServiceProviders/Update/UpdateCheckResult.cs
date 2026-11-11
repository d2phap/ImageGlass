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
using System;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Status of an update check operation.
/// </summary>
public enum UpdateCheckStatus
{
    /// <summary>Current version is up-to-date.</summary>
    NoUpdate,

    /// <summary>A newer version is available.</summary>
    UpdateAvailable,

    /// <summary>The check failed (network, parse, or other error).</summary>
    CheckFailed,
}


/// <summary>
/// Result of an update check operation.
/// </summary>
public sealed class UpdateCheckResult
{
    public UpdateCheckStatus Status { get; init; }
    public UpdateReleaseInfo? Release { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }


    public static UpdateCheckResult NoUpdate(UpdateReleaseInfo? release = null) => new()
    {
        Status = UpdateCheckStatus.NoUpdate,
        Release = release,
    };

    public static UpdateCheckResult Available(UpdateReleaseInfo release) => new()
    {
        Status = UpdateCheckStatus.UpdateAvailable,
        Release = release,
    };

    public static UpdateCheckResult Failed(string message, Exception? ex = null) => new()
    {
        Status = UpdateCheckStatus.CheckFailed,
        ErrorMessage = message,
        Exception = ex,
    };
}

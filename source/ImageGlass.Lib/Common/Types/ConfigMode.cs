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
using System.IO;

namespace ImageGlass.Common.Types;


/// <summary>
/// Decides which mode the config is in: portable when the <see cref="Const.PORTABLE_MARKER_FILE"/>
/// marker is in the startup dir. Resolved once; the path itself comes from BHelper.ConfigPath.
/// </summary>
public static class ConfigMode
{
    private static readonly (bool IsPortable, Exception? Error) _state = Resolve();


    /// <summary>
    /// Whether the marker file was found in the startup dir, so the config lives there.
    /// </summary>
    public static bool IsPortable => _state.IsPortable;


    /// <summary>
    /// The real error when portable mode cannot be honored; startup reports it and quits.
    /// </summary>
    public static Exception? PortableError => _state.Error;


    /// <summary>
    /// Full path of the portable marker file, whether or not it exists.
    /// </summary>
    public static string PortableMarkerPath => BHelper.BaseDir(Const.PORTABLE_MARKER_FILE);


    /// <summary>
    /// Detects portable mode, and checks the startup dir is writable when it is on.
    /// </summary>
    private static (bool IsPortable, Exception? Error) Resolve()
    {
        if (!File.Exists(PortableMarkerPath)) return (false, null);

        return (true, GetStartupDirAccessError());
    }


    /// <summary>
    /// Returns why the app cannot create files in the startup dir, or <c>null</c> when it can.
    /// </summary>
    private static Exception? GetStartupDirAccessError()
    {
        // probe a new per-process file, not the marker: no collision between instances, and a backup
        // tool holding the marker open cannot fail the launch
        var probeName = $"{Const.PORTABLE_MARKER_FILE}.{Environment.ProcessId}.tmp";
        var probePath = BHelper.BaseDir(probeName);

        try
        {
            using (File.Create(probePath, 1, FileOptions.DeleteOnClose)) { }
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

}

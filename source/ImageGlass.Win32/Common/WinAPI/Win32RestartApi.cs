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
using System;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ImageGlass.Win32.Common.WinAPI;


/// <summary>
/// Restart Manager registration, used so an MSIX self-update can relaunch the app it replaced.
/// </summary>
internal static class Win32RestartApi
{
    /// <summary>
    /// Asks Restart Manager to relaunch this process after deployment shuts it down.
    /// </summary>
    internal static unsafe bool RegisterForRestart()
    {
        try
        {
            // null command line: Restart Manager reuses the one this process was launched with
            var hr = PInvoke.RegisterApplicationRestart(null, 0);
            return hr.Succeeded;
        }
        catch { return false; }
    }
}

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
using Avalonia.Threading;
using Windows.Win32;
using Windows.Win32.System.Power;

namespace ImageGlass.Win32.Common;

/// <summary>
/// Power helpers to keep the system and the display awake.
/// </summary>
public static class Win32PowerApi
{
    /// <summary>
    /// Keeps the system and the display awake until <see cref="AllowSleep"/>.
    /// </summary>
    public static void PreventSleep()
    {
        SetExecutionState__(EXECUTION_STATE.ES_CONTINUOUS
            | EXECUTION_STATE.ES_SYSTEM_REQUIRED
            | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    }


    /// <summary>
    /// Releases the request held by <see cref="PreventSleep"/>.
    /// </summary>
    public static void AllowSleep()
    {
        SetExecutionState__(EXECUTION_STATE.ES_CONTINUOUS);
    }


    // the execution state is owned by the thread that set it
    private static void SetExecutionState__(EXECUTION_STATE state)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            _ = PInvoke.SetThreadExecutionState(state);
        }
        else
        {
            Dispatcher.UIThread.Post(() => _ = PInvoke.SetThreadExecutionState(state));
        }
    }
}

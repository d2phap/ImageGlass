/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Windows.Win32;
using Windows.Win32.System.Power;

namespace ImageGlass.Base.WinApi;


/// <summary>
/// Enables an application to inform the system that it is in use, thereby preventing the system from entering sleep or turning off the display while the application is running.
/// Ref: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate
/// </summary>
public static class SysExecutionState
{
    /// <summary>
    /// Prevents the system from entering sleep or turning off the display while the application is running.
    /// </summary>
    public static void PreventSleep()
    {
        _ = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS
            | EXECUTION_STATE.ES_SYSTEM_REQUIRED
            | EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    }


    /// <summary>
    /// Allow the system to enter sleep or turn off the display while the application is running.
    /// </summary>
    public static void AllowSleep()
    {
        _ = PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }
}

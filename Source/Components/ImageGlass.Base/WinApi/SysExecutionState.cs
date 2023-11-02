/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using System.Runtime.InteropServices;

namespace ImageGlass.Base.WinApi;


/// <summary>
/// Enables an application to inform the system that it is in use, thereby preventing the system from entering sleep or turning off the display while the application is running.
/// Ref: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate
/// </summary>
public static class SysExecutionState
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern ExecutionState SetThreadExecutionState(ExecutionState esFlags);

    [Flags]
    private enum ExecutionState : uint
    {
        ES_NONE = 0x00000000,
        ES_SYSTEM_REQUIRED = 0x00000001,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000
    }


    /// <summary>
    /// Prevents the system from entering sleep or turning off the display while the application is running.
    /// </summary>
    public static void PreventSleep()
    {
        SetThreadExecutionState(ExecutionState.ES_CONTINUOUS
            | ExecutionState.ES_SYSTEM_REQUIRED
            | ExecutionState.ES_DISPLAY_REQUIRED);
    }


    /// <summary>
    /// Allow the system to enter sleep or turn off the display while the application is running.
    /// </summary>
    public static void AllowSleep()
    {
        SetThreadExecutionState(ExecutionState.ES_CONTINUOUS);
    }
}

/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010-2022 DUONG DIEU PHAP
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

public class ErrorModeApi
{
    [DllImport("kernel32.dll")]
    private static extern ErrorModes SetErrorMode(ErrorModes uMode);

    [Flags]
    public enum ErrorModes : uint
    {
        SYSTEM_DEFAULT = 0x0,
        SEM_FAILCRITICALERRORS = 0x0001,
        SEM_NOGPFAULTERRORBOX = 1 << 1,
        SEM_NOALIGNMENTFAULTEXCEPT = 1 << 2,
        SEM_NOOPENFILEERRORBOX = 1 << 15
    }

    /// <summary>
    /// <para>Issue #360: IG periodically searching for dismounted device.</para>
    /// <para>
    /// Controls whether the system will handle the specified types of serious errors
    /// or whether the process will handle them.
    /// </para>
    /// <para>
    /// Best practice is that all applications call the process-wide SetErrorMode
    /// function with a parameter of SEM_FAILCRITICALERRORS at startup. This is to
    /// prevent error mode dialogs from hanging the application.
    /// </para>
    /// Ref: https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-seterrormode
    /// </summary>
    /// <param name="mode"></param>
    public static void SetAppErrorMode(ErrorModes mode = ErrorModes.SEM_FAILCRITICALERRORS)
    {
        _ = SetErrorMode(mode);
    }

}

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
using ImageGlass.Common.ServiceProviders;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;

namespace ImageGlass.Win32.Common.ServiceProviders;


/// <summary>
/// Windows implementation that resolves the connected pipe client's PID via
/// <c>GetNamedPipeClientProcessId</c> and compares it to the spawned tool process.
/// </summary>
public class Win32PipeSecurityProvider : IPipeSecurityProvider
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool VerifyClientProcess(SafePipeHandle serverPipeHandle, int expectedProcessId)
    {
        if (serverPipeHandle is null || serverPipeHandle.IsInvalid) return true;

        // Fail open when the PID cannot be resolved: CurrentUserOnly is the security
        // baseline; this check is only an extra layer against same-user pipe squatting.
        try
        {
            // CsWin32's SafeHandle-friendly overload manages the handle ref-count for us.
            if (!PInvoke.GetNamedPipeClientProcessId(serverPipeHandle, out var clientPid))
            {
                return true;
            }

            return clientPid == (uint)expectedProcessId;
        }
        catch
        {
            return true;
        }
    }
}

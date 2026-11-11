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
using Microsoft.Win32.SafeHandles;

namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// Verifies the OS identity of the process connected to a host pipe (Windows-only; null elsewhere).
/// </summary>
public interface IPipeSecurityProvider
{
    /// <summary>
    /// <c>true</c> if the client is the expected process (or identity is undeterminable); <c>false</c>
    /// only on a confirmed PID mismatch, which the caller must refuse.
    /// </summary>
    bool VerifyClientProcess(SafePipeHandle serverPipeHandle, int expectedProcessId);
}

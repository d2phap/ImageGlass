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
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Mac.Common;

/// <summary>
/// IOKit power-assertion helpers to keep the system and the display awake.
/// </summary>
public static partial class MacPowerApi
{
    private const string IOKIT = "/System/Library/Frameworks/IOKit.framework/IOKit";
    private const string CORE_FOUNDATION = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    private const string ASSERTION_TYPE = "PreventUserIdleDisplaySleep";
    private const uint ASSERTION_LEVEL_ON = 255;
    private const uint STRING_ENCODING_UTF8 = 0x08000100;
    private const uint NULL_ASSERTION_ID = 0;

    private static readonly Lock _lock = new();
    private static uint _assertionId = NULL_ASSERTION_ID;


    /// <summary>
    /// Holds a display-sleep assertion until <see cref="AllowSleep"/>.
    /// </summary>
    public static void PreventSleep(string reason)
    {
        lock (_lock)
        {
            if (_assertionId != NULL_ASSERTION_ID) return;

            var typeRef = nint.Zero;
            var nameRef = nint.Zero;
            try
            {
                typeRef = CFStringCreateWithCString(nint.Zero, ASSERTION_TYPE, STRING_ENCODING_UTF8);
                nameRef = CFStringCreateWithCString(nint.Zero, reason, STRING_ENCODING_UTF8);
                if (typeRef == nint.Zero || nameRef == nint.Zero) return;

                // kIOReturnSuccess == 0
                if (IOPMAssertionCreateWithName(typeRef, ASSERTION_LEVEL_ON, nameRef, out var id) == 0)
                {
                    _assertionId = id;
                }
            }
            catch
            {
                // best-effort
            }
            finally
            {
                if (typeRef != nint.Zero) CFRelease(typeRef);
                if (nameRef != nint.Zero) CFRelease(nameRef);
            }
        }
    }


    /// <summary>
    /// Releases the assertion held by <see cref="PreventSleep"/>.
    /// </summary>
    public static void AllowSleep()
    {
        lock (_lock)
        {
            if (_assertionId == NULL_ASSERTION_ID) return;

            try
            {
                _ = IOPMAssertionRelease(_assertionId);
            }
            catch
            {
                // best-effort
            }

            _assertionId = NULL_ASSERTION_ID;
        }
    }


    #region IOKit interop

    [LibraryImport(IOKIT)]
    private static partial int IOPMAssertionCreateWithName(nint assertionType, uint assertionLevel, nint assertionName, out uint assertionId);

    [LibraryImport(IOKIT)]
    private static partial int IOPMAssertionRelease(uint assertionId);

    [LibraryImport(CORE_FOUNDATION, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint CFStringCreateWithCString(nint allocator, string cStr, uint encoding);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial void CFRelease(nint cfRef);

    #endregion // IOKit interop
}

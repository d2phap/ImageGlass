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
using System.Text;

namespace ImageGlass.Mac.Common;


/// <summary>
/// The CoreFoundation strings, arrays and URLs the printing APIs pass around; a Create or Copy result is the caller's to release.
/// </summary>
internal static unsafe partial class MacCoreFoundation
{
    private const string CORE_FOUNDATION = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const uint STRING_ENCODING_UTF8 = 0x08000100;
    private const int MAX_PATH_BYTES = 4096;


    /// <summary>
    /// Creates a CFString; release it with <see cref="Release"/>.
    /// </summary>
    public static nint CreateString(string value) => CFStringCreateWithCString(0, value, STRING_ENCODING_UTF8);


    /// <summary>
    /// Creates a file URL from a path; release it with <see cref="Release"/>.
    /// </summary>
    public static nint CreateFileUrl(string path, bool isDirectory = false)
    {
        var bytes = Encoding.UTF8.GetBytes(path);
        fixed (byte* p = bytes)
        {
            return CFURLCreateFromFileSystemRepresentation(0, p, bytes.Length, (byte)(isDirectory ? 1 : 0));
        }
    }


    /// <summary>
    /// Gets the path of a file URL; <c>null</c> when it is not one.
    /// </summary>
    public static string? GetPath(nint url)
    {
        if (url == 0) return null;

        var buffer = stackalloc byte[MAX_PATH_BYTES];
        return CFURLGetFileSystemRepresentation(url, 1, buffer, MAX_PATH_BYTES) != 0
            ? Marshal.PtrToStringUTF8((nint)buffer)
            : null;
    }


    /// <summary>
    /// Reads a CFString; <c>null</c> for anything else.
    /// </summary>
    public static string? ReadString(nint cfString)
    {
        if (cfString == 0 || CFGetTypeID(cfString) != CFStringGetTypeID()) return null;

        var length = CFStringGetLength(cfString);
        var buffer = new char[length];
        fixed (char* p = buffer)
        {
            CFStringGetCharacters(cfString, new CFRange { Location = 0, Length = length }, p);
        }

        return new string(buffer);
    }


    public static nint GetCount(nint array) => array == 0 ? 0 : CFArrayGetCount(array);


    public static nint GetItem(nint array, nint index) => CFArrayGetValueAtIndex(array, index);


    public static void Release(nint cf)
    {
        if (cf != 0) CFRelease(cf);
    }


    #region CoreFoundation interop

    [StructLayout(LayoutKind.Sequential)]
    private struct CFRange
    {
        public nint Location;
        public nint Length;
    }

    [LibraryImport(CORE_FOUNDATION, StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint CFStringCreateWithCString(nint allocator, string cStr, uint encoding);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFURLCreateFromFileSystemRepresentation(nint allocator, byte* buffer, nint bufLen, byte isDirectory);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial byte CFURLGetFileSystemRepresentation(nint url, byte resolveAgainstBase, byte* buffer, nint maxBufLen);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nuint CFGetTypeID(nint cf);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nuint CFStringGetTypeID();

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFStringGetLength(nint str);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial void CFStringGetCharacters(nint str, CFRange range, char* buffer);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFArrayGetCount(nint array);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFArrayGetValueAtIndex(nint array, nint index);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial void CFRelease(nint cf);

    #endregion // CoreFoundation interop
}

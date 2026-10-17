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
using System.Runtime.InteropServices;
using System.Text;

namespace ImageGlass.Mac.Common;

/// <summary>
/// Code-signature checks through Security.framework.
/// </summary>
public static partial class MacCodeSignApi
{
    private const string SECURITY = "/System/Library/Frameworks/Security.framework/Security";
    private const string CORE_FOUNDATION = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    private const uint CS_DEFAULT_FLAGS = 0;
    private const uint CS_SIGNING_INFORMATION = 1 << 1;

    // what `codesign --verify --deep --strict` checks: every architecture, nested code, strict rules
    private const uint CS_STRICT_DEEP_VALIDATE = (1 << 0) | (1 << 3) | (1 << 4);

    private static readonly Lazy<nint> _teamIdKey = new(ReadTeamIdKey);


    /// <summary>
    /// Why the bundle fails strict validation pinned to this app's team (when it has one), or <c>null</c>.
    /// </summary>
    public static string? CheckSignedLikeThisApp(string bundlePath, out string? teamId)
    {
        teamId = null;
        nint self = 0, info = 0, requirement = 0, url = 0, code = 0, error = 0;

        try
        {
            var status = SecCodeCopySelf(CS_DEFAULT_FLAGS, out self);
            if (status != 0) return DescribeStatus(nameof(SecCodeCopySelf), status);

            if (SecCodeCopySigningInformation(self, CS_SIGNING_INFORMATION, out info) == 0)
            {
                teamId = ReadString(CFDictionaryGetValue(info, _teamIdKey.Value));
            }

            // an ad-hoc dev build's requirement is its own cdhash, which no other build satisfies
            if (teamId is not null)
            {
                status = SecCodeCopyDesignatedRequirement(self, CS_DEFAULT_FLAGS, out requirement);
                if (status != 0) return DescribeStatus(nameof(SecCodeCopyDesignatedRequirement), status);
            }

            url = CreateDirectoryUrl(bundlePath);
            if (url == 0) return $"Cannot create a URL for {bundlePath}.";

            status = SecStaticCodeCreateWithPath(url, CS_DEFAULT_FLAGS, out code);
            if (status != 0) return DescribeStatus(nameof(SecStaticCodeCreateWithPath), status);

            status = SecStaticCodeCheckValidityWithErrors(code, CS_STRICT_DEEP_VALIDATE, requirement, out error);
            if (status == 0) return null;

            // the error's own description is generic, but its user info names the offending files
            var message = DescribeStatus("Validation", status);
            return ReadDescription(error) is { } details ? $"{message}{Environment.NewLine}{details}" : message;
        }
        finally
        {
            ReadOnlySpan<nint> refs = [error, code, url, requirement, info, self];
            foreach (var cf in refs)
            {
                if (cf != 0) CFRelease(cf);
            }
        }
    }


    /// <summary>
    /// Security's own text for an <c>OSStatus</c>.
    /// </summary>
    private static string DescribeStatus(string step, int status)
    {
        var message = SecCopyErrorMessageString(status, 0);
        try
        {
            return $"{step}: {ReadString(message) ?? "unknown error"} (OSStatus {status})";
        }
        finally
        {
            if (message != 0) CFRelease(message);
        }
    }


    private static string? ReadDescription(nint cf)
    {
        if (cf == 0) return null;

        var description = CFCopyDescription(cf);
        try
        {
            return ReadString(description);
        }
        finally
        {
            if (description != 0) CFRelease(description);
        }
    }


    private static unsafe string? ReadString(nint cfString)
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


    private static unsafe nint CreateDirectoryUrl(string path)
    {
        var bytes = Encoding.UTF8.GetBytes(path);
        fixed (byte* p = bytes)
        {
            return CFURLCreateFromFileSystemRepresentation(0, p, bytes.Length, 1);
        }
    }


    /// <summary>
    /// The <c>kSecCodeInfoTeamIdentifier</c> dictionary key, an exported <c>CFStringRef</c>.
    /// </summary>
    private static nint ReadTeamIdKey()
    {
        var lib = NativeLibrary.Load(SECURITY);
        return Marshal.ReadIntPtr(NativeLibrary.GetExport(lib, "kSecCodeInfoTeamIdentifier"));
    }


    #region Security / CoreFoundation interop

    [StructLayout(LayoutKind.Sequential)]
    private struct CFRange
    {
        public nint Location;
        public nint Length;
    }

    [LibraryImport(SECURITY)]
    private static partial int SecCodeCopySelf(uint flags, out nint self);

    [LibraryImport(SECURITY)]
    private static partial int SecCodeCopySigningInformation(nint code, uint flags, out nint information);

    [LibraryImport(SECURITY)]
    private static partial int SecCodeCopyDesignatedRequirement(nint code, uint flags, out nint requirement);

    [LibraryImport(SECURITY)]
    private static partial int SecStaticCodeCreateWithPath(nint path, uint flags, out nint staticCode);

    [LibraryImport(SECURITY)]
    private static partial int SecStaticCodeCheckValidityWithErrors(nint staticCode, uint flags, nint requirement, out nint errors);

    [LibraryImport(SECURITY)]
    private static partial nint SecCopyErrorMessageString(int status, nint reserved);

    [LibraryImport(CORE_FOUNDATION)]
    private static unsafe partial nint CFURLCreateFromFileSystemRepresentation(nint allocator, byte* buffer, nint bufLen, byte isDirectory);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFDictionaryGetValue(nint dict, nint key);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFCopyDescription(nint cf);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nuint CFGetTypeID(nint cf);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nuint CFStringGetTypeID();

    [LibraryImport(CORE_FOUNDATION)]
    private static partial nint CFStringGetLength(nint str);

    [LibraryImport(CORE_FOUNDATION)]
    private static unsafe partial void CFStringGetCharacters(nint str, CFRange range, char* buffer);

    [LibraryImport(CORE_FOUNDATION)]
    private static partial void CFRelease(nint cf);

    #endregion // Security / CoreFoundation interop
}

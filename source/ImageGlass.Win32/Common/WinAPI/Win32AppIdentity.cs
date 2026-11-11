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
using ImageGlass.Common;
using System;
using System.IO;
using Windows.Win32;

namespace ImageGlass.Win32.Common;

/// <summary>
/// Runtime MSIX package-identity probe.
/// </summary>
public static class Win32AppIdentity
{
    private const int ERROR_SUCCESS = 0;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;

    /// <summary>
    /// Identity Name the packer stamps on the Microsoft Store flavour. Keep in sync with
    /// <c>-MsStoreIdentityName</c> in <c>__assets/win/script-pack-win-msix.ps1</c>.
    /// </summary>
    private const string MSSTORE_IDENTITY_NAME = "9662DuongDieuPhap.ImageGlass";

    /// <summary>
    /// Publisher id derived by Windows from <c>-MsStorePublisher</c>: the first 8 bytes of the
    /// SHA-256 of that DN in UTF-16LE, base32-encoded. Unlike the Identity Name, it cannot be
    /// claimed without a certificate for the same DN, so it is what makes this check meaningful.
    /// </summary>
    private const string MSSTORE_PUBLISHER_ID = "xjrmsrdc1fgj6";

    private static readonly Lazy<bool> _isPackaged = new(DetectPackaged);
    private static readonly Lazy<bool> _isUnvirtualizedResources = new(DetectUnvirtualizedResources);
    private static readonly Lazy<string?> _packageFullName = new(ReadPackageFullName);
    private static readonly Lazy<bool> _isMsStorePackage = new(DetectMsStorePackage);


    /// <summary>
    /// Whether the process runs with MSIX package identity (packaged / Store install).
    /// </summary>
    public static bool IsPackaged => _isPackaged.Value;


    /// <summary>
    /// Whether this packaged build opted out of resources virtualization (signed GitHub flavour).
    /// </summary>
    public static bool IsUnvirtualizedResources => _isUnvirtualizedResources.Value;


    /// <summary>
    /// Full name of the current package, or <c>null</c> when the process has no package identity.
    /// </summary>
    public static string? PackageFullName => _packageFullName.Value;



    /// <summary>
    /// Whether this is the Microsoft Store package rather than the signed sideload flavour.
    /// Both are MSIX, so <see cref="IsPackaged"/> alone cannot tell them apart.
    /// </summary>
    public static bool IsMsStorePackage => _isMsStorePackage.Value;


    private static bool DetectPackaged()
    {
        // null buffer -> ERROR_INSUFFICIENT_BUFFER when identity exists, APPMODEL_ERROR_NO_PACKAGE otherwise
        uint length = 0;
        return (int)PInvoke.GetCurrentPackageFullName(ref length, default) == ERROR_INSUFFICIENT_BUFFER;
    }


    private static bool DetectUnvirtualizedResources()
    {
        if (!_isPackaged.Value) return false;

        try
        {
            // deployed manifest is one level above the app dir (<packageRoot>\AppxManifest.xml)
            var manifest = Path.GetFullPath(BHelper.BaseDir("..", "AppxManifest.xml"));
            return File.Exists(manifest)
                && File.ReadAllText(manifest).Contains("unvirtualizedResources", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }


    private static string? ReadPackageFullName()
    {
        if (!_isPackaged.Value) return null;

        try
        {
            // first call reports the buffer size needed, in chars, including the null terminator
            uint length = 0;
            var probe = PInvoke.GetCurrentPackageFullName(ref length, default);
            if ((int)probe != ERROR_INSUFFICIENT_BUFFER) return null;
            if (length < 2) return null;

            var buffer = new char[length];
            var read = PInvoke.GetCurrentPackageFullName(ref length, buffer);
            if ((int)read != ERROR_SUCCESS) return null;

            return new string(buffer, 0, (int)length - 1);
        }
        catch { return null; }
    }


    private static bool DetectMsStorePackage()
    {
        var fullName = _packageFullName.Value;
        if (string.IsNullOrEmpty(fullName)) return false;

        // the full name is Name_Version_Arch_ResourceId_PublisherId
        var nameEnd = fullName.IndexOf('_');
        if (nameEnd <= 0) return false;

        var identityName = fullName[..nameEnd];
        var nameMatches = string.Equals(identityName, MSSTORE_IDENTITY_NAME, StringComparison.OrdinalIgnoreCase);
        if (!nameMatches) return false;

        // the Identity Name alone is free for anyone to claim, so the publisher has to match too
        var publisherStart = fullName.LastIndexOf('_');
        if (publisherStart <= nameEnd) return false;

        var publisherId = fullName[(publisherStart + 1)..];
        return string.Equals(publisherId, MSSTORE_PUBLISHER_ID, StringComparison.OrdinalIgnoreCase);
    }

}

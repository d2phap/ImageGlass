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
using ImageGlass.Common;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Win32.Common.ServiceProviders;

public class Win32PrintProvider : IPrintProvider
{
    private readonly HashSet<string> _nativeWin32Formats = [".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tif", ".tiff", ".fax"];


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task OpenPrintAsync(string? filePath, PhotoMetadata? meta, bool isClipboardFile)
    {
        var fileToPrint = filePath;
        var srcFilePath = meta?.FilePath ?? string.Empty;
        var ext = meta?.FileExtension.ToLowerInvariant() ?? string.Empty;
        var isNativeSingleFrameFormat = meta?.FrameCount == 1 && !_nativeWin32Formats.Contains(ext);


        // print clipboard image
        if (Core.ClipboardImage != null || isNativeSingleFrameFormat)
        {
            // save image to temp file
            fileToPrint = await Core.SavePhotoAsTempFileAsync();
        }
        // print an image file
        // rename ext FAX -> TIFF to multi-frame printing
        else if (ext.Equals(".fax", StringComparison.OrdinalIgnoreCase))
        {
            fileToPrint = BHelper.ConfigDir(Dir.Temporary, Path.GetFileNameWithoutExtension(srcFilePath) + ".tiff");
            File.Copy(srcFilePath, fileToPrint, true);
        }
        else if (meta?.FrameCount > 1
            && !ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)
            && !ext.Equals(".tif", StringComparison.OrdinalIgnoreCase)
            && !ext.Equals(".tiff", StringComparison.OrdinalIgnoreCase))
        {
            // save image to temp file
            fileToPrint = await Core.SavePhotoAsTempFileAsync();
        }

        if (string.IsNullOrEmpty(fileToPrint)) return;

        // open Print dialog
        Win32PrintApi.OpenPrintDialog(BHelper.GetRealPlatformPath(fileToPrint));
    }
}

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
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Cysharp.Text;
using System.Text;

namespace ImageGlass.Base;

public static class SavingExts
{
    /// <summary>
    /// Gets the supported extensions for saving in a pair of (.extension, description)
    /// </summary>
    public static List<SavingExt> SupportedExts => [
        // the order here matters
        new SavingExt { Ext = ".avif", Description = "AVIF" },
        new SavingExt { Ext = ".bmp", Description = "BMP" },
        new SavingExt { Ext = ".gif", Description = "GIF" },
        new SavingExt { Ext = ".jpg", Description = "JPG" },
        new SavingExt { Ext = ".jxl", Description = "JXL" },
        new SavingExt { Ext = ".png", Description = "PNG" },
        new SavingExt { Ext = ".tiff", Description = "TIFF" },
        new SavingExt { Ext = ".webp", Description = "WEBP" },

        new SavingExt { Ext = ".emf", Description = "EMF" },
        new SavingExt { Ext = ".exif", Description = "EXIF" },
        new SavingExt { Ext = ".ico", Description = "ICO" },
        new SavingExt { Ext = ".wmf", Description = "WMF" },
        new SavingExt { Ext = ".b64", Description = "Base64" },
        new SavingExt { Ext = ".txt", Description = "Base64 text" },
    ];


    /// <summary>
    /// Returns file extension filter string for <see cref="SaveFileDialog"/>.
    /// </summary>
    public static string GetFilterStringForSaveDialog()
    {
        using var sb = ZString.CreateStringBuilder();

        for (int i = 0; i < SupportedExts.Count; i++)
        {
            var item = SupportedExts[i];

            sb.Append($"{item.Description} (*{item.Ext})|*{item.Ext}");

            if (i < SupportedExts.Count - 1)
            {
                sb.Append('|');
            }
        }

        return sb.ToString();
    }


    /// <summary>
    /// Finds index of the input extension.
    /// </summary>
    /// <param name="ext">An extension, example: <c>.png</c></param>
    public static int IndexOf(string ext)
    {
        ext = ext.ToLowerInvariant();

        for (int i = 0; i < SupportedExts.Count; i++)
        {
            if (SupportedExts[i].Ext == ext)
            {
                return i;
            }
        }

        return -1;
    }
}



public record SavingExt
{
    public string Ext { get; set; } = "";
    public string Description { get; set; } = "";
}

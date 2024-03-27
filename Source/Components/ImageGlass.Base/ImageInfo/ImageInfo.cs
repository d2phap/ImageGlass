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

public static class ImageInfo
{
    public static string? AppName { get; set; } = null;
    public static string? Name { get; set; } = null;
    public static string? Path { get; set; } = null;
    public static string? FileSize { get; set; } = null;
    public static string? Dimension { get; set; } = null;
    public static string? FrameCount { get; set; } = null;
    public static string? ListCount { get; set; } = null;
    public static string? Zoom { get; set; } = null;
    public static string? ModifiedDateTime { get; set; } = null;

    public static string? ExifRating { get; set; } = null;
    public static string? ExifDateTime { get; set; } = null;
    public static string? ExifDateTimeOriginal { get; set; } = null;

    public static string? DateTimeAuto { get; set; } = null;
    public static string? ColorSpace { get; set; } = null;


    public static bool IsNull => AppName == null
        && Name == null
        && Path == null
        && FileSize == null
        && Dimension == null
        && FrameCount == null
        && ListCount == null
        && Zoom == null

        && DateTimeAuto == null
        && ModifiedDateTime == null
        && ExifDateTime == null
        && ExifDateTimeOriginal == null

        && ExifRating == null
        && ColorSpace == null;


    /// <summary>
    /// Gets complete information string in order
    /// </summary>
    public static string ToString(List<string> infoTags, bool isVirtualImage, string clipboardImageText = "")
    {
        using var strBuilder = ZString.CreateStringBuilder();
        int count = 0;

        // remove unsupported tags for virtual image
        if (isVirtualImage)
        {
            ListCount =
                Name =
                Path =
                FileSize =
                FrameCount =
                ModifiedDateTime =
                ExifRating =
                ExifDateTime =
                ExifDateTimeOriginal =
                DateTimeAuto =
                ColorSpace = null;

            if (!string.IsNullOrEmpty(clipboardImageText))
            {
                strBuilder.Append(clipboardImageText);
                count++;
            }
        }

        foreach (var tag in infoTags)
        {
            // get the property using name
            var str = typeof(ImageInfo).GetProperty(tag)?.GetValue(null)?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                if (count > 0)
                {
                    strBuilder.Append("  ︱  ");
                }

                strBuilder.Append(str);
                count++;
            }
        }

        return strBuilder.ToString();
    }

}

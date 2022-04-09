/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.Text;

namespace ImageGlass.Base;

public static class BasicInfo
{
    public static string? AppName { get; set; } = null;
    public static string? Name { get; set; } = null;
    public static string? Path { get; set; } = null;
    public static string? FileSize { get; set; } = null;
    public static string? Dimension { get; set; } = null;
    public static string? FramesCount { get; set; } = null;
    public static string? ListCount { get; set; } = null;
    public static string? Zoom { get; set; } = null;
    public static string? ModifiedDate { get; set; } = null;

    public static string? ExifRating { get; set; } = null;
    public static string? ExifDateTime { get; set; } = null;
    public static string? ExifDateTimeOriginal { get; set; } = null;


    public static bool IsNull => AppName == null
        && Name == null
        && Path == null
        && FileSize == null
        && Dimension == null
        && FramesCount == null
        && ListCount == null
        && Zoom == null
        && ModifiedDate == null

        && ExifRating == null
        && ExifDateTime == null
        && ExifDateTimeOriginal == null;


    /// <summary>
    /// Gets complete information string in order
    /// </summary>
    /// <param name="orders"></param>
    /// <returns></returns>
    public static string ToString(List<string> orders)
    {
        var strBuilder = new StringBuilder();
        int count = 0;


        foreach (var key in orders)
        {
            // get the property using name
            var str = typeof(BasicInfo).GetProperty(key)?.GetValue(null)?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                if (count > 0)
                {
                    strBuilder.Append("  |  ");
                }

                strBuilder.Append(str);
                count++;
            }
        }

        return strBuilder.ToString();
    }

}

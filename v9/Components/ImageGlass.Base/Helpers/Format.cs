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

using System.Globalization;

namespace ImageGlass.Base;

public partial class Helpers
{
    /// <summary>
    /// Formats the given file size as a human readable string.
    /// </summary>
    /// <param name="size">File size in bytes.</param>
    /// <returns>The formatted string.</returns>
    public static string FormatSize(long size)
    {
        var mod = 1024d;
        var sized = size * 1d;

        var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
        int i;
        for (i = 0; sized > mod; i++)
        {
            sized /= mod;
        }

        return string.Format("{0} {1}", Math.Round(sized, 2), units[i]);
    }

    /// <summary>
    /// Formats date time string to string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string FormatDateTime(string? str, bool includeTime = true)
    {
        var dt = ConvertDateTime(str);

        return FormatDateTime(dt, includeTime);
    }

    /// <summary>
    /// Formats <see cref="DateTime"/> to string
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="includeTime"></param>
    /// <returns></returns>
    public static string FormatDateTime(DateTime? dt, bool includeTime = true)
    {
        if (dt != null)
        {
            return dt.Value.ToString(includeTime ? Constants.DATETIME_FORMAT : Constants.DATE_FORMAT);
        }

        return string.Empty;
    }

    /// <summary>
    /// Convert date time string to <see cref="DateTime"/>
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime? ConvertDateTime(string? dt)
    {
        if (DateTime.TryParseExact(dt,
                    "yyyy:MM:dd HH:mm:ss",
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var dateTaken))
        {
            return dateTaken;
        }

        return null;
    }


    /// <summary>
    /// Sets the simple rating (0-5) from rating (0-99).
    /// </summary>
    public static int FormatStarRating(int rating)
    {
        var star = 0;

        if (rating >= 1 && rating <= 12)
            star = 1;
        else if (rating >= 13 && rating <= 37)
            star = 2;
        else if (rating >= 38 && rating <= 62)
            star = 3;
        else if (rating >= 63 && rating <= 87)
            star = 4;
        else if (rating >= 88 && rating <= 99)
            star = 5;

        return star;
    }

    /// <summary>
    /// Ellipses the given text
    /// </summary>
    /// <param name="text"></param>
    /// <param name="containerWidth"></param>
    /// <param name="g"></param>
    /// <returns></returns>
    public static string EllipsisText(string text, int containerWidth, Graphics g)
    {
        var textSize = g.MeasureString(text, Control.DefaultFont);
        var avgCharW = textSize.Width / text.Length;
        var maxCharsPerSide = (int)Math.Floor(containerWidth / avgCharW / 2d) - 1;

        var truncated = string.Concat(
            text.AsSpan(0, maxCharsPerSide),
            "…",
            text.AsSpan(text.Length - maxCharsPerSide, maxCharsPerSide));

        return truncated;
    }

}
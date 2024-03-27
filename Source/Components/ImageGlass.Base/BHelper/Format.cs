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
using System.Globalization;

namespace ImageGlass.Base;

public partial class BHelper
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

        var units = new string[] { "bytes", "KB", "MB", "GB", "TB", "PB" };
        int i;
        for (i = 0; sized > mod; i++)
        {
            sized /= mod;
        }

        return ZString.Format("{0} {1}", Math.Round(sized, 2), units[i]);
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
            return dt.Value.ToString(includeTime ? Const.DATETIME_FORMAT : Const.DATE_FORMAT);
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
    /// Formats the 0-99 rating to the simple rating (0-5).
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
    /// Formats the rating as stars.
    /// </summary>
    public static string FormatStarRatingText(int rating)
    {
        var star = FormatStarRating(rating);
        var ratingText = star > 0
            ? "".PadRight(star, '⭐').PadRight(5, '-').Replace("-", " -")
            : string.Empty;

        return ratingText;
    }


    /// <summary>
    /// Ellipses the given text.
    /// </summary>
    public static string EllipsisText(string text, Font font, float containerWidth, Graphics g)
    {
        var textSize = g.MeasureString(text, font);
        var avgCharW = textSize.Width / text.Length;
        var maxCharsPerSide = (int)Math.Floor(containerWidth / avgCharW / 2d);

        var truncated = string.Concat(
            text.AsSpan(0, maxCharsPerSide),
            "…",
            text.AsSpan(text.Length - maxCharsPerSide, maxCharsPerSide));

        return truncated;
    }


    /// <summary>
    /// Simplifies the fractions.
    /// </summary>
    public static int[] SimplifyFractions(params int[] numbers)
    {
        // get the greatest common divisor of the input numbers
        var gcd = numbers.Aggregate((currentGcd, arg) => CalculateGcd(currentGcd, arg));

        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] /= gcd;
        }

        return numbers;


        int CalculateGcd(int a, int b)
        {
            while (b > 0)
            {
                int rem = a % b;
                a = b;
                b = rem;
            }
            return a;
        }
    }


}
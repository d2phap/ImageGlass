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
using System.Globalization;
using System.Linq;

namespace ImageGlass.Common;

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

        return String.Format("{0} {1}", Math.Round(sized, 2), units[i]);
    }

    /// <summary>
    /// Formats a timestamp for persistence as culture-independent UTC, e.g. <c>2026-08-30T10:08:12.4924980Z</c>.
    /// </summary>
    public static string FormatUtcRoundtrip(DateTime dt)
    {
        return dt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }


    /// <summary>
    /// Reverses <see cref="FormatUtcRoundtrip"/> in UTC; also reads values written before that format.
    /// </summary>
    public static bool TryParseUtcRoundtrip(string? value, out DateTime utc)
    {
        utc = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(value)) return false;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)
            || DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt))
        {
            // values written before this format carried no zone, and every writer used UtcNow
            utc = dt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                : dt.ToUniversalTime();
            return true;
        }

        return false;
    }


    /// <summary>
    /// Formats date time string to string.
    /// </summary>
    public static string FormatDateTime(string? str, bool includeTime = true)
    {
        var dt = ConvertDateTime(str);

        return FormatDateTime(dt, includeTime);
    }

    /// <summary>
    /// Formats <see cref="DateTime"/> to string.
    /// </summary>
    public static string FormatDateTime(DateTime? dt, bool includeTime = true)
    {
        if (dt != null)
        {
            const string DATETIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
            const string DATE_FORMAT = "yyyy/MM/dd";

            return dt.Value.ToString(includeTime ? DATETIME_FORMAT : DATE_FORMAT);
        }

        return string.Empty;
    }

    /// <summary>
    /// Convert date time string to <see cref="DateTime"/>.
    /// </summary>
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
            ? string.Empty.PadRight(star, '⭐').PadRight(5, '✰').Replace("✰", " ✰")
            : string.Empty;

        return ratingText;
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
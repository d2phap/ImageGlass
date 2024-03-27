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
using System.Text;

namespace ImageGlass.Base;

public static class StringExtensions
{
    /// <summary>
    /// Capitalizes the first letter of the string.
    /// </summary>
    public static string? CapitalizeFirst(this string? thisString, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(thisString))
            return null;

        if (culture == null)
            return char.ToUpper(thisString[0]) + thisString.Substring(1);

        return char.ToUpper(thisString[0], culture) + thisString.Substring(1);
    }


    /// <summary>
    /// Replaces string.
    /// </summary>
    public static string ReplaceMultiple(this string value, IEnumerable<(string Variable, string Value)> toReplace)
    {
        using var result = ZString.CreateStringBuilder();
        result.Append(value);

        foreach (var item in toReplace)
        {
            result.Replace(item.Variable, item.Value);
        }

        return result.ToString();
    }
}

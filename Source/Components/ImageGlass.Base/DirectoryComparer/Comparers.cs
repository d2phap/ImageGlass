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
using Windows.Win32;

namespace ImageGlass.Base.DirectoryComparer;


public class StringNaturalComparer(bool orderByAsc = true, bool ignoreCase = false) : IComparer<string?>
{
    public bool OrderByAsc { get; set; } = orderByAsc;
    public bool IgnoreCase { get; set; } = ignoreCase;

    public int Compare(string? str1, string? str2)
    {
        str1 ??= "";
        str2 ??= "";

        if (IgnoreCase)
        {
            str1 = str1.ToLowerInvariant();
            str2 = str2.ToLowerInvariant();
        }

        if (OrderByAsc)
        {
            return PInvoke.StrCmpLogical(str1, str2);
        }

        return PInvoke.StrCmpLogical(str2, str1);
    }
}

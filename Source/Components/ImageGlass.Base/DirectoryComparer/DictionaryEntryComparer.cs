/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

using System.Collections;

namespace ImageGlass.Base.DirectoryComparer;


public class DictionaryEntryComparer : IComparer
{
    private readonly IComparer? _comparer = null;

    public DictionaryEntryComparer(IComparer nc)
    {
        _comparer = nc ?? throw new ArgumentNullException(nameof(nc));
    }

    public int Compare(object? x, object? y)
    {
        if ((x is DictionaryEntry entry) && (y is DictionaryEntry entry1))
        {
            return _comparer?.Compare(entry.Key, entry1.Key) ?? -1;
        }

        return -1;
    }
}

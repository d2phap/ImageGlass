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


public class FileLogicalComparer
{
    public ArrayList? Files { get; set; } = null;


    #region Local Functions

    public void AddFile(string file)
    {
        if (file == null)
        {
            return;
        }

        Files ??= new ArrayList();
        Files.Add(new DictionaryEntry(Path.GetFileName(file), file));
    }

    public void AddFiles(string[] f)
    {
        if (f == null)
        {
            return;
        }

        for (var i = 0; i < f.Length; i++)
        {
            AddFile(f[i]);
        }
    }

    public ArrayList? GetSorted()
    {
        Files?.Sort(new DictionaryEntryComparer(new NumericComparer()));
        return Files;
    }

    #endregion


    /// <summary>
    /// Sort an string array
    /// </summary>
    /// <param name="stringArray">String array</param>
    /// <returns></returns>
    public static string[]? Sort(string[]? stringArray)
    {
        if (stringArray == null)
        {
            return null;
        }

        var fc = new FileLogicalComparer();
        fc.AddFiles(stringArray);
        var list = fc.GetSorted();

        if (list == null)
        {
            return stringArray;
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is DictionaryEntry entry && entry.Value != null)
            {
                stringArray[i] = (string)entry.Value;
            }
        }

        return stringArray;
    }
}

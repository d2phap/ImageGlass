/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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

Author: Kevin Routley (aka fire-eggs)
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ImageGlass.Services.Configuration;

namespace ImageGlass.Services
{
    public static class ExplorerSortOrder
    {
        // Convert the Explorer column name to our currently available sorting order
        private static readonly Dictionary<string, ImageOrderBy> SortTranslation = new Dictionary<string, ImageOrderBy>()
        {
            { "System.DateModified", ImageOrderBy.LastWriteTime },
            { "System.ItemDate", ImageOrderBy.LastWriteTime },
            { "System.ItemTypeText", ImageOrderBy.Extension},
            { "System.FileExtension", ImageOrderBy.Extension},
            { "System.FileName", ImageOrderBy.Name},
            { "System.ItemNameDisplay", ImageOrderBy.Name},
            { "System.Size", ImageOrderBy.Length },
            { "System.DateCreated",ImageOrderBy.CreationTime},
            { "System.DateAccessed",ImageOrderBy.LastAccessTime},
        };

        [DllImport("ExplorerSortOrder32.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetExplorerSortOrder")]
        public static extern int GetExplorerSortOrder32(string path, ref StringBuilder str, int len, ref Int32 ascend);

        [DllImport("ExplorerSortOrder64.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "GetExplorerSortOrder")]
        public static extern int GetExplorerSortOrder64(string path, ref StringBuilder str, int len, ref Int32 ascend);

        /// <summary>
        /// Determines the sorting order of a Windows Explorer window which matches
        /// the given file path.
        ///
        /// "Failure" situations are:
        /// 1. unable to find an open Explorer window matching the file path
        /// 2. the Explorer sort order doesn't match
        /// </summary>
        /// <param name="fullpath">full path to file/folder in question</param>
        /// <param name="LoadOrder">the resulting sort order or null</param>
        /// <param name="ascending">the resulting sort direction or null</param>
        /// <returns>false on failure - out parameters will be null!</returns>
        public static bool GetExplorerSortOrder(string fullpath, out ImageOrderBy? LoadOrder, out bool? ascending)
        {
            // assume failure
            LoadOrder = null;
            ascending = null;

            try
            {
                string path = Path.GetDirectoryName(fullpath);

                StringBuilder sb = new StringBuilder(200);
                int res;
                int ascend = -1;
                if (IntPtr.Size == 8)
                    res = GetExplorerSortOrder64(path, ref sb, sb.Capacity, ref ascend);
                else
                    res = GetExplorerSortOrder32(path, ref sb, sb.Capacity, ref ascend);

                if (res != 0) // failure
                    return false;

                // Success! Attempt to translate the Explorer column to our supported
                // sort order values.
                string column = sb.ToString();
                if (SortTranslation.ContainsKey(column))
                    LoadOrder = SortTranslation[column];
                ascending = ascend > 0;

                return LoadOrder != null; // false on not-yet-supported column
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                return false; // failure
            }
        }


    }
}

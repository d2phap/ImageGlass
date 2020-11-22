/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.Comparer {
    public class WindowsDirectoryNaturalSort: IComparer<string> {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string filePath1, string filePath2) {
            var basename1 = Path.GetDirectoryName(filePath1);
            var basename2 = Path.GetDirectoryName(filePath2);

            return StrCmpLogicalW(basename1, basename2);
        }
    }

    public class ReverseWindowsDirectoryNaturalSort: IComparer<string> {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string filePath1, string filePath2) {
            var basename1 = Path.GetDirectoryName(filePath2);
            var basename2 =  Path.GetDirectoryName(filePath1);

            return StrCmpLogicalW(basename1, basename2);
        }
    }
}

/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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

using System.Collections;
using System.IO;

namespace ImageGlass.Library.Comparer {
    public class FileLogicalComparer {
        public ArrayList Files { get; set; } = null;

        #region Local Functions
        public void AddFile(string file) {
            if (file == null) return;
            if (Files == null) Files = new ArrayList();
            Files.Add(new DictionaryEntry(Path.GetFileName(file), file));
        }


        public void AddFiles(string[] f) {
            if (f == null) return;
            for (int i = 0; i < f.Length; i++) {
                AddFile(f[i]);
            }
        }

        public ArrayList GetSorted() {
            if (Files != null) {
                Files.Sort(new DictionaryEntryComparer(new NumericComparer()));
            }
            return Files;
        }
        #endregion


        /// <summary>
        /// Sort an string array
        /// </summary>
        /// <param name="stringArray">String array</param>
        /// <returns></returns>
        public static string[] Sort(string[] stringArray) {
            if (stringArray == null) return null;

            var fc = new FileLogicalComparer();
            fc.AddFiles(stringArray);
            ArrayList ds = fc.GetSorted();

            if (ds == null) return stringArray;

            for (int i = 0; i < ds.Count; i++) {
                stringArray[i] = (string)((DictionaryEntry)ds[i]).Value;
            }

            return stringArray;
        }

    }





}
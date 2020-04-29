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

using System;
using System.Collections;

namespace ImageGlass.Library.Comparer {
    public class DictionaryEntryComparer: IComparer {
        private IComparer nc = null;

        public DictionaryEntryComparer(IComparer nc) {
            if (nc == null) throw new Exception("Null IComparer");
            this.nc = nc;
        }

        public int Compare(object x, object y) {
            if ((x is DictionaryEntry) && (y is DictionaryEntry)) {
                return nc.Compare(((DictionaryEntry)x).Key, ((DictionaryEntry)y).Key);
            }
            return -1;
        }
    }
}

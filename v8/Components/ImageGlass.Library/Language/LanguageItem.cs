/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2016 DUONG DIEU PHAP
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

using System;
using System.Collections.Generic;

namespace ImageGlass.Library {
    public enum LanguageItemState {
        Inactive = 0,
        Active = 1
    }

    [Serializable]
    public class LanguageItem<K, V>: Dictionary<K, V> {
        /// <summary>
        /// ImageGlass version that supported
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets, sets value that indicates if this language string is active or not
        /// </summary>
        public LanguageItemState State { get; set; }

        /// <summary>
        /// Author remarks
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Language text object
        /// </summary>
        public LanguageItem() {
            Version = new Version("3.5.0.0");
            State = LanguageItemState.Inactive;
            Remarks = string.Empty;
        }

        protected LanguageItem(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) {
            throw new NotImplementedException();
        }
    }
}

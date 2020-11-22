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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;

namespace ImageGlass.Base {
    /// <summary>
    /// Contains the information of the editing associated app
    /// </summary>
    public class EditApp {
        /// <summary>
        /// Gets, sets extension. Ex: .png
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets, sets friendly app name.
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Gets, sets full path of app.
        /// </summary>
        public string AppPath { get; set; }

        /// <summary>
        /// Gets, sets arguments of app.
        /// </summary>
        public string AppArguments { get; set; }

        /// <summary>
        /// Gets the macro string
        /// </summary>
        public static string FileMacro { get; } = "<file>";

        /// <summary>
        /// Initial Image Editing App
        /// </summary>
        public EditApp() {
            Extension = string.Empty;
            AppName = string.Empty;
            AppPath = string.Empty;
            AppArguments = string.Empty;
        }

        /// <summary>
        /// Initial EditApp
        /// </summary>
        /// <param name="extension">Extension. Ex: .png</param>
        /// <param name="appName">Friendly app name.</param>
        /// <param name="appPath">Full path and arguments of app. Ex: C:\app\app.exe --help</param>
        public EditApp(string extension, string appName, string appPath, string arguments = "") {
            Extension = extension.ToLower();
            AppName = appName;
            AppPath = appPath;
            AppArguments = arguments;
        }

        /// <summary>
        /// Initial Image Editing Association.
        /// Throw InvalidCastException if @mixString is invalid
        /// </summary>
        /// <param name="mixString">EditApp string. Ex: .jpg|MS Paint|C:\app\mspaint.exe</param>
        public EditApp(string mixString) {
            var itemArray = mixString.Split("|".ToCharArray());

            if (itemArray.Length != 4) {
                throw new InvalidCastException("Invalid EditApp string format.");
            }

            Extension = itemArray[0].ToLower();
            AppName = itemArray[1];
            AppPath = itemArray[2];
            AppArguments = itemArray[3];
        }

        /// <summary>
        /// Convert ImageEditingAssociation object to string.
        /// </summary>
        /// <returns>ImageEditingAssociation string</returns>
        public override string ToString() {
            return $"{Extension}|{AppName}|{AppPath}|{AppArguments}";
        }
    }
}

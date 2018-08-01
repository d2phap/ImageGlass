/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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

using ImageGlass.Services.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace ImageGlass
{
    public static class LocalSetting
    {
        private static frmSetting _fSetting;
        private static frmColorPicker _fColorPicker;
        private static Theme.Theme _theme;
        

        #region Auto Properties
        /// <summary>
        /// Check if frmColorPicker is opening.
        /// This is for toggle Color Picker menu in frmMain
        /// </summary>
        public static bool IsColorPickerToolOpening { get; set; } = false;


        /// <summary>
        /// Check if frmColorPicker is not closed by user (toggle the menu / press ESC on frmColorPicker form). 
        /// This is for auto open Color Picker tool when startup
        /// </summary>
        public static bool IsShowColorPickerOnStartup { get; set; } = false;


        /// <summary>
        /// Gets, sets value if image data was modified
        /// </summary>
        public static string ImageModifiedPath { get; set; } = "";
        

        /// <summary>
        /// Gets, sets the 0-based index of the last view of Settings dialog tab.
        /// </summary>
        public static int SettingsTabLastView { get; set; } = 0;


        /// <summary>
        /// Gets, sets active value whenever hover on picturebox
        /// </summary>
        public static MainFormForceUpdateAction ForceUpdateActions { get; set; } = MainFormForceUpdateAction.NONE;


        /// <summary>
        /// Gets, sets copied filename collection (multi-copy)
        /// </summary>
        public static List<string> StringClipboard { get; set; } = new List<string>();


        /// <summary>
        /// Gets, sets value indicating that the image we are processing is memory data (clipboard / screenshot,...) or not
        /// </summary>
        public static bool IsTempMemoryData { get; set; } = false;


        #endregion



        #region LazyInitializer Properties

        /// <summary>
        /// Form frmSetting
        /// </summary>
        public static frmSetting FSetting
        {
            get { return LazyInitializer.EnsureInitialized(ref _fSetting); }
            set { _fSetting = value; }
        }

        /// <summary>
        /// Form frmColorPicker
        /// </summary>
        public static frmColorPicker FColorPicker
        {
            get { return LazyInitializer.EnsureInitialized(ref _fColorPicker); }
            set { _fColorPicker = value; }
        }


        /// <summary>
        /// Gets, sets current app theme
        /// </summary>
        public static Theme.Theme Theme
        {
            get => LazyInitializer.EnsureInitialized(ref _theme, () => new Theme.Theme());
            set => _theme = value;
        }


        #endregion


    }
}




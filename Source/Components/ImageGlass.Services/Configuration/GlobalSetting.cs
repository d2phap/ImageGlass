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

using ImageGlass.Base;
using ImageGlass.Library.FileAssociations;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageGlass.Services.Configuration
{
    public static class GlobalSetting
    {
        // Configurations file: igconfig.xml
        private static ConfigurationFile _configFile = new ConfigurationFile();





        #region Public Methods


        /// <summary>
        /// Get file extensions from registry
        /// Ex: *.svg;*.png;
        /// </summary>
        /// <returns></returns>
        public static string GetFileExtensionsFromRegistry()
        {
            StringBuilder exts = new StringBuilder();

            RegistryHelper reg = new RegistryHelper()
            {
                BaseRegistryKey = Registry.LocalMachine,
                SubKey = @"SOFTWARE\PhapSoftware\ImageGlass\Capabilities\FileAssociations"
            };
            var extList = reg.GetValueNames();

            foreach (var ext in extList)
            {
                exts.Append($"*{ext};");
            }

            return exts.ToString();
        }



        /// <summary>
        /// Gets a specify config. Return @defaultValue if not found.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="defaultValue">Default value</param>=
        /// <returns></returns>
        public static string GetConfig(string configKey, string @defaultValue = "")
        {
            // Read configs from file
            return _configFile.GetConfig(configKey, defaultValue);
        }


        /// <summary>
        /// Sets a specify config.
        /// </summary>
        /// <param name="configKey">Configuration key</param>
        /// <param name="value">Configuration value</param>
        public static void SetConfig(string configKey, string value)
        {
            // Write configs to file
            _configFile.SetConfig(configKey, value);
        }



        #endregion


    }
}

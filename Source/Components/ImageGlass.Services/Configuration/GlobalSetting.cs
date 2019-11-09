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
using ImageGlass.Heart;
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



        #region Private Properties

        /// <summary>
        /// Gets, sets image list
        /// </summary>
        public static Factory ImageList { get; set; } = new Factory();


        #endregion



        #region Public Properties


        /// <summary>
        /// ~User-selected action tied to key pairings. E.g. Left/Right arrows: prev/next image
        /// </summary>
        public static string KeyAssignments { get; set; } = $"" +
            $"{(int)KeyCombos.LeftRight},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.UpDown},{(int)AssignableActions.PanUpDown};" +
            $"{(int)KeyCombos.PageUpDown},{(int)AssignableActions.PrevNextImage};" +
            $"{(int)KeyCombos.SpaceBack},{(int)AssignableActions.PauseSlideshow};";

        #endregion



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



        #region Keyboard customization
        // The user is permitted to choose what action to associate to a key-pairing.
        // E.g. PageUp/PageDown to next/previous image.

        // The KeyPair -> action lookup
        private static Dictionary<KeyCombos, AssignableActions> KeyActionLookup;

        // Note: default value matches the IGV6 behavior
        private static string DEFAULT_KEY_ASSIGNMENTS = "0,0;1,2;2,0;3,4;";

        /// <summary>
        /// Load the KeyPair -> action values from the config file into the lookup
        /// dictionary.
        /// </summary>
        public static void LoadKeyAssignments()
        {
            try
            {
                KeyAssignments = GetConfig("KeyboardActions", DEFAULT_KEY_ASSIGNMENTS);
                SetKeyAssignments();
            }
            catch (Exception e)
            {
                ResetKeyActionsToDefault();
            }
        }

        private static void ResetKeyActionsToDefault()
        {
            KeyAssignments = DEFAULT_KEY_ASSIGNMENTS;
            SetKeyAssignments();
        }

        private static void SetKeyAssignments()
        {
            var part_sep = new [] { ',' };
            var pairs = KeyAssignments.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            KeyActionLookup = new Dictionary<KeyCombos, AssignableActions>();
            foreach (var pair in pairs)
            {
                var parts = pair.Split(part_sep);
                int part1 = int.Parse(parts[0]);
                int part2 = int.Parse(parts[1]);
                KeyActionLookup.Add((KeyCombos)part1, (AssignableActions)part2);
            }
        }

        /// <summary>
        /// For a given key-pair, return the user-chosen action
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static AssignableActions GetKeyAction(KeyCombos key)
        {
            try
            {
                return KeyActionLookup[key];
            }
            catch
            {
                // KBR 20170716 not quite sure how we might get here, but
                // don't blow up nastily if something went wrong loading 
                // the key assignments from the config file
                ResetKeyActionsToDefault();
                return KeyActionLookup[key];
            }
        }

        public static void SetKeyAction(KeyCombos which, int newval)
        {
            KeyActionLookup[which] = (AssignableActions)newval;
        }

        /// <summary>
        /// Write the key-pair customizations to the config file.
        /// </summary>
        public static void SaveKeyAssignments()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var key in KeyActionLookup.Keys)
            {
                sb.Append((int)key);
                sb.Append(',');
                sb.Append((int)KeyActionLookup[key]);
                sb.Append(';');
            }
            KeyAssignments = sb.ToString();
            SetConfig("KeyboardActions", KeyAssignments);
        }
        #endregion

    }
}

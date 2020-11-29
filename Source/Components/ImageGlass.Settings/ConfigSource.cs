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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ImageGlass.Base;

namespace ImageGlass.Settings {
    /// <summary>
    /// A unified list of default/user/admin settings, provides read/write the settings to source file.
    /// </summary>
    [Serializable]
    public class ConfigSource: Dictionary<string, string> {
        #region Public properties

        /// <summary>
        /// User config file
        /// </summary>
        public static string Filename => App.ConfigDir(PathType.File, "igconfig.xml");

        /// <summary>
        /// The default config file located in StartUpDir, the default configs if it does not exist in user's configs
        /// </summary>
        public static string DefaultConfigFilename => App.StartUpDir("igconfig.default.xml");

        /// <summary>
        /// The admin config file located in StartUpDir. All configs here will override user's configs and default configs
        /// </summary>
        public static string AdminConfigFilename => App.StartUpDir("igconfig.admin.xml");

        /// <summary>
        /// Gets the admin configs
        /// </summary>
        public Dictionary<string, string> AdminConfigs { get; private set; } = new();

        /// <summary>
        /// Config file description
        /// </summary>
        public string Description { get; set; } = "ImageGlass Configuration file";

        /// <summary>
        /// Config file version
        /// </summary>
        public string Version { get; set; } = "7.5";

        /// <summary>
        /// Gets, sets value indicates that the config file is compatible with this ImageGlass version or not
        /// </summary>
        public bool IsCompatible { get; set; } = true;

        #endregion


        #region Private methods
        /// <summary>
        /// Reads XML file and returns the Document object
        /// </summary>
        /// <returns></returns>
        private static XmlDocument ReadXMLFile(string filename) {
            var doc = new XmlDocument();

            try {
                doc.Load(filename);
            }
            catch (Exception) {
                return null;
            }

            return doc;
        }

        /// <summary>
        /// Loads the given filename, returns all configs
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> LoadConfigFile(string filename, bool isUserConfigFile = false) {
            var list = new Dictionary<string, string>();

            // file does not exist
            if (!File.Exists(filename)) {
                return list;
            }

            var doc = ReadXMLFile(filename);

            // config file is invalid
            if (doc == null) {
                this.IsCompatible = !isUserConfigFile;
                return list;
            }

            var root = doc.DocumentElement;// <ImageGlass>
            var nType = (XmlElement)root.SelectNodes("Configuration")[0]; // <Configuration>

            if (isUserConfigFile) {
                // Get <Info> element
                var nInfo = (XmlElement)nType.SelectNodes("Info")[0];// <Info>
                var version = nInfo.GetAttribute("version");
                this.IsCompatible = version == this.Version;
                this.Version = this.IsCompatible ? version : this.Version;
            }

            // Get <Content> element
            var nContent = (XmlElement)nType.SelectNodes("Content")[0];// <Content>

            // Get all config items
            var nItems = nContent.SelectNodes("Item");// <Item>

            foreach (var item in nItems) {
                var nItem = (XmlElement)item;
                var key = nItem.GetAttribute("key");

                //Issue #567: this breaks LastSeenImagePath when "\n" is part of the path
                var value = nItem.GetAttribute("value");
                if (key != "LastSeenImagePath")
                    value = value.Replace("\\n", "\n");

                list[key] = value;
            }

            return list;
        }

        /// <summary>
        /// Write configs to the given filename
        /// </summary>
        /// <param name="configs"></param>
        private void WriteConfigFile(Dictionary<string, string> configs, string filename) {
            var doc = new XmlDocument();
            var root = doc.CreateElement("ImageGlass"); // <ImageGlass>
            var nConfig = doc.CreateElement("Configuration"); // <Configuration>

            var nInfo = doc.CreateElement("Info"); // <Info>
            nInfo.SetAttribute("description", this.Description);
            nInfo.SetAttribute("version", this.Version);
            nConfig.AppendChild(nInfo); // <Info />

            // Write config items
            var nContent = doc.CreateElement("Content"); // <Content>

            foreach (var item in configs) {
                var nItem = doc.CreateElement("Item"); // <Item>
                nItem.SetAttribute("key", item.Key);
                nItem.SetAttribute("value", item.Value);
                nContent.AppendChild(nItem); // <Item />
            }

            nConfig.AppendChild(nContent); // </Content>
            root.AppendChild(nConfig); // </Configuration>
            doc.AppendChild(root); // </ImageGlass>

            try {
                doc.Save(filename);
            }
            catch { }
        }

        #endregion


        #region Public methods

        public ConfigSource() { }

        /// <summary>
        /// Throws NotImplementedException exception. It's not used!
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="streamingContext"></param>
        protected ConfigSource(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads all config files: user, default, admin, then unify configs for user
        /// </summary>
        public void LoadUserConfigs() {
            var userConfigs = LoadConfigFile(Filename, true);
            var defaultConfigs = LoadConfigFile(DefaultConfigFilename);
            this.AdminConfigs = LoadConfigFile(AdminConfigFilename);

            // take default config items if they don not exist in user configs
            foreach (var item in defaultConfigs) {
                if (!userConfigs.ContainsKey(item.Key)) {
                    userConfigs[item.Key] = item.Value;
                }
            }

            // override user configs by admin configs
            foreach (var item in this.AdminConfigs) {
                userConfigs[item.Key] = item.Value;
            }

            // set user configs to the dictionary
            this.Clear();
            foreach (var item in userConfigs) {
                this.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Write user configs to file
        /// </summary>
        public void WriteUserConfigs() {
            WriteConfigFile(this, Filename);
        }

        #endregion

    }
}

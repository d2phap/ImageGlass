/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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

using ImageGlass.Base;
using System;
using System.Collections.Generic;
using System.Xml;

namespace ImageGlass.Settings
{
    /// <summary>
    /// A unified list of default/user/admin settings, provides read/write the settings to source file.
    /// </summary>
    [Serializable]
    public class ConfigSource : Dictionary<string, string>
    {

        #region Public properties

        /// <summary>
        /// User config file
        /// </summary>
        public string Filename { get => App.ConfigDir("igconfig.xml"); }


        /// <summary>
        /// The default config file located in StartUpDir, the default configs if it does not exist in user's configs
        /// </summary>
        public string DefaultConfigFilename { get => App.StartUpDir("igconfig.default.xml"); }


        /// <summary>
        /// The admin config file located in StartUpDir. All configs here will override user's configs and default configs
        /// </summary>
        public string AdminConfigFilename { get => App.StartUpDir("igconfig.admin.xml"); }


        /// <summary>
        /// Gets the admin configs
        /// </summary>
        public Dictionary<string, string> AdminConfigs { get; private set; } = new Dictionary<string, string>();


        /// <summary>
        /// Config file description
        /// </summary>
        public string Description { get; set; } = "ImageGlass Configuration file";


        /// <summary>
        /// Config file version
        /// </summary>
        public string Version { get; set; } = "7.5";

        #endregion


        #region Private methods
        /// <summary>
        /// Reads XML file and returns the Document object
        /// </summary>
        /// <returns></returns>
        private XmlDocument ReadXMLFile(string filename)
        {
            var doc = new XmlDocument();

            try
            {
                doc.Load(filename);
            }
            catch (Exception)
            {
                // fix invalid XML file
                WriteConfigFile(new Dictionary<string, string>(), filename);

                // load again
                doc.Load(filename);
            }

            return doc;
        }


        /// <summary>
        /// Loads the given filename, returns all configs
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> LoadConfigFile(string filename)
        {
            var doc = ReadXMLFile(filename);

            XmlElement root = doc.DocumentElement;// <ImageGlass>
            XmlElement nType = (XmlElement)root.SelectNodes("Configuration")[0]; // <Configuration>

            // Get <Content> element
            XmlElement nContent = (XmlElement)nType.SelectNodes("Content")[0];// <Content>

            // Get all config items
            XmlNodeList nItems = nContent.SelectNodes("Item");// <Item>
            var configs = new Dictionary<string, string>();

            foreach (var item in nItems)
            {
                var nItem = (XmlElement)item;
                string key = nItem.GetAttribute("key");
                string value = nItem.GetAttribute("value").Replace("\\n", "\n");

                if (configs.ContainsKey(key))
                {
                    // override the existing key
                    configs[key] = value;
                }
                else
                {
                    configs.Add(key, value);
                }
            }

            return configs;
        }


        /// <summary>
        /// Write configs to the given filename
        /// </summary>
        /// <param name="configs"></param>
        private void WriteConfigFile(Dictionary<string, string> configs, string filename)
        {
            var doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass"); // <ImageGlass>
            XmlElement nConfig = doc.CreateElement("Configuration"); // <Configuration>


            XmlElement nInfo = doc.CreateElement("Info"); // <Info>
            nInfo.SetAttribute("description", this.Description);
            nInfo.SetAttribute("version", this.Version);
            nConfig.AppendChild(nInfo); // <Info />


            // Write config items
            XmlElement nContent = doc.CreateElement("Content"); // <Content>

            foreach (var item in configs)
            {
                XmlElement nItem = doc.CreateElement("Item"); // <Item>
                nItem.SetAttribute("key", item.Key);
                nItem.SetAttribute("value", item.Value);
                nContent.AppendChild(nItem); // <Item />
            }

            nConfig.AppendChild(nContent); // </Content>
            root.AppendChild(nConfig); // </Configuration>
            doc.AppendChild(root); // </ImageGlass>

            try
            {
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
        protected ConfigSource(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Loads all config files: user, default, admin, then unify configs for user
        /// </summary>
        public void LoadUserConfigs()
        {
            var userConfigs = LoadConfigFile(this.Filename);
            var defaultConfigs = LoadConfigFile(this.DefaultConfigFilename);
            this.AdminConfigs = LoadConfigFile(this.AdminConfigFilename);

            // take default config items if they don not exist in user configs
            foreach (var item in defaultConfigs)
            {
                if (!userConfigs.ContainsKey(item.Key))
                {
                    userConfigs[item.Key] = item.Value;
                }
            }

            // override user configs by admin configs
            foreach (var item in this.AdminConfigs)
            {
                userConfigs[item.Key] = item.Value;
            }


            // set user configs to the dictionary
            this.Clear();
            foreach (var item in userConfigs)
            {
                this.Add(item.Key, item.Value);
            }
        }


        /// <summary>
        /// Write user configs to file
        /// </summary>
        public void WriteUserConfigs()
        {
            WriteConfigFile(this, this.Filename);
        }


        #endregion

    }
}

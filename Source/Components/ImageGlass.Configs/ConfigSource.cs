using System;
using System.Collections.Generic;
using System.Xml;

namespace ImageGlass.Settings
{
    /// <summary>
    /// Configuration Source File
    /// </summary>
    public class ConfigSource : Dictionary<string, string>
    {

        #region Public properties

        /// <summary>
        /// User config file
        /// </summary>
        public string Filename { get => Helpers.ConfigDir("igconfig.xml"); }


        /// <summary>
        /// The default config file located in StartUpDir, the default configs if it does not exist in user's configs
        /// </summary>
        public string DefaultConfigFilename { get => Helpers.StartUpDir("igconfig.default.xml"); }


        /// <summary>
        /// The admin config file located in StartUpDir. All configs here will override user's configs and default configs
        /// </summary>
        public string AdminConfigFilename { get => Helpers.StartUpDir("igconfig.admin.xml"); }


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
        public string Version { get; set; } = "4.0";

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

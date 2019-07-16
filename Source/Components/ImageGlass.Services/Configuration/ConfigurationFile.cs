/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2017 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ImageGlass.Services.Configuration
{
    public class ConfigurationFile : IDictionary<string, string>
    {
        private Dictionary<string, string> _dictionary;
        public string Description { get; set; }
        public string Version { get; set; }
        public string Filename { get => GlobalSetting.ConfigDir("igconfig.xml"); }

        public ICollection<string> Keys => _dictionary.Keys;
        public ICollection<string> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Get / Set configuration stored in memory
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get {
                return _dictionary[key];
            }
            set => _dictionary[key] = value;
        }

        public ConfigurationFile() : base()
        {
            _dictionary = new Dictionary<string, string>();

            Description = "ImageGlass Configuration file";
            Version = "4.0";

            //Read all configs from file
            ReadConfigFile();
        }

        /// <summary>
        /// Read configuration strings from file
        /// </summary>
        public void ReadConfigFile()
        {
            // write default configs if not exist
            if(!File.Exists(Filename))
            {
                WriteConfigFile(writeEmptyConfigs: true);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            XmlElement root = doc.DocumentElement;// <ImageGlass>
            XmlElement nType = (XmlElement)root.SelectNodes("Configuration")[0]; //<Configuration>
            XmlElement n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

            //Get <Info> Attributes
            Description = n.GetAttribute("description");
            Version = n.GetAttribute("version");

            //Get <Content> element
            XmlElement nContent = (XmlElement)nType.SelectNodes("Content")[0];//<Content>

            //Get all config items
            XmlNodeList nItems = nContent.SelectNodes("Item");//<Item>

            foreach (var item in nItems)
            {
                XmlElement nItem = (XmlElement)item;
                string _key = nItem.GetAttribute("key");
                string _value = nItem.GetAttribute("value").Replace("\\n", "\n");

                try
                {
                    this[_key] = _value;
                }
                catch { }
            }

            doc = null;
            root = null;
            nType = null;
            n = null;
            nItems = null;
        }

        /// <summary>
        /// Export all configuration strings to xml file
        /// </summary>
        public void WriteConfigFile(bool writeEmptyConfigs = false)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");// <ImageGlass>
            XmlElement nType = doc.CreateElement("Configuration");// <Configuration>

            XmlElement nInfo = doc.CreateElement("Info");// <Info>
            nInfo.SetAttribute("description", Description);
            nInfo.SetAttribute("version", Version);
            nType.AppendChild(nInfo);// <Info />

            XmlElement nContent = doc.CreateElement("Content");// <Content>
            if (!writeEmptyConfigs)
            {
                foreach (var item in this)
                {
                    XmlElement n = doc.CreateElement("Item"); // <Item>
                    n.SetAttribute("key", item.Key);
                    n.SetAttribute("value", item.Value.ToString());
                    nContent.AppendChild(n);// <Item />
                }
            }
            nType.AppendChild(nContent);

            root.AppendChild(nType);// </Content>
            doc.AppendChild(root);// </ImageGlass>

            try
            {
                var dir = Path.GetDirectoryName(Filename);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                doc.Save(Filename);
            }
            catch (Exception) { }

            doc = null;
            root = null;
            nType = null;
        }

        /// <summary>
        /// Read configuration item from igconfig.xml file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetConfig(string key, string @defaultValue = null)
        {
            // write default configs if not exist
            if (!File.Exists(Filename))
            {
                WriteConfigFile(writeEmptyConfigs: true);
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            string xpath = "//Configuration/Content/Item[@key = \"" + key + "\"]";
            XmlElement nItem = (XmlElement)root.SelectNodes(xpath)[0]; //<Item />

            // Don't throw unnecessary NullReferenceExceptions
            if (nItem == null)
            {
                return defaultValue;
            }

            //Get all config items
            try
            {
                string value = nItem.GetAttribute("value").Replace("\\n", "\n");

                // KBR 20190716 if the value in xml exists but is empty, use the default
                if (string.IsNullOrWhiteSpace(value))
                    value = defaultValue;
                return value;
            }
            catch
            {
                if (defaultValue != null)
                {
                    return defaultValue;
                }

                return this[key];
            }
        }


        /// <summary>
        /// Write configuration item to igconfig.xml file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetConfig(string key, object value)
        {
            // update memory config
            this[key] = value.ToString();


            try
            {
                // update file config
                XmlDocument doc = new XmlDocument();

                doc.Load(Filename);

                var root = doc.DocumentElement;// <ImageGlass>
                var nItem = (XmlElement)root.SelectNodes("//Configuration/Content/Item[@key = \"" + key + "\"]")[0]; //<Item />

                if (nItem != null)
                {
                    nItem.SetAttribute("value", value.ToString());
                }
                else
                {
                    nItem = (XmlElement)root.SelectNodes("//Configuration/Content")[0]; //<Content>
                    XmlElement node = doc.CreateElement("Item");
                    node.SetAttribute("key", key);
                    node.SetAttribute("value", value.ToString());
                    nItem.AppendChild(node);
                }

                doc.Save(Filename);
            }
            catch (Exception)
            {
                WriteConfigFile(writeEmptyConfigs: true);
            }
        }

        

        #region auto-generated functions
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(string key, string value)
        {
            _dictionary.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return (_dictionary.TryGetValue(item.Key, out string value) && value.Equals(item.Key));
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            if (Contains(item))
            {
                _dictionary.Remove(item.Key);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}

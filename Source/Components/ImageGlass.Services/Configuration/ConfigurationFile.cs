using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ImageGlass.Services.Configuration
{
    public class ConfigurationFile : IDictionary<string, string>
    {
        private Dictionary<string, string> _dictionary;
        public string Description { get; set; }
        public string Version { get; set; }
        public string Filename { get => "igconfig.xml"; }

        public ICollection<string> Keys => _dictionary.Keys;
        public ICollection<string> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly { get; private set; }


        public string this[string key]
        {
            get {
                _dictionary[key] = GetConfig(key);
                return _dictionary[key];
            }
            set => _dictionary[key] = value;
        }

        public ConfigurationFile() : base()
        {
            _dictionary = new Dictionary<string, string>();

            Description = "ImageGlass Configuration file";
            Version = "4.0";

            // Init configs
            this["AutoUpdate"] = "7/26/1991 12:13:08 AM";
            this["ExtraExtensions"] = "*.hdr;*.exr;*.tga;*.psd;*.cr2;";
            this["WindowsBound"] = "214,85,855,597";
            // ...

            //Read all configs from file
            ReadConfigFile();
        }


        /// <summary>
        /// Read configuration strings from file
        /// </summary>
        public void ReadConfigFile()
        {
            //Clear all items
            this.Clear();

            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
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
        public void WriteConfigFile()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("ImageGlass");// <ImageGlass>
            XmlElement nType = doc.CreateElement("Configuration");// <Configuration>

            XmlElement nInfo = doc.CreateElement("Info");// <Info>
            nInfo.SetAttribute("description", Description);
            nInfo.SetAttribute("version", Version);
            nType.AppendChild(nInfo);// <Info />

            XmlElement nContent = doc.CreateElement("Content");// <Content>
            foreach (var item in this)
            {
                XmlElement n = doc.CreateElement("Item"); // <Item>
                n.SetAttribute("key", item.Key);
                n.SetAttribute("value", item.Value.ToString());
                nContent.AppendChild(n);// <Item />
            }
            nType.AppendChild(nContent);

            root.AppendChild(nType);// </Content>
            doc.AppendChild(root);// </ImageGlass>

            doc.Save(Filename);
        }

        /// <summary>
        /// Get configuration item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetConfig(string key, string @defaultValue = null)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            string xpath = "//Configuration/Content/Item[@key = \"" + key + "\"]";
            XmlElement nItem = (XmlElement)root.SelectNodes(xpath)[0]; //<Item />

            //Get all config items
            try
            {
                return nItem.GetAttribute("value").Replace("\\n", "\n");
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

        public void SetConfig(string key, string value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Filename);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            XmlElement nItem = (XmlElement)root.SelectNodes("//Configuration/Content/Item[@key = \"" + key + "\"]")[0]; //<Item />

            nItem.SetAttribute("value", value);
            doc.Save(Filename);


            //XmlDocument doc = new XmlDocument();
            //XmlElement root = doc.CreateElement("ImageGlass");// <ImageGlass>
            //XmlElement nType = doc.CreateElement("Configuration");// <Configuration>

            //XmlElement nInfo = doc.CreateElement("Info");// <Info>
            //nInfo.SetAttribute("description", Description);
            //nInfo.SetAttribute("version", Version);
            //nType.AppendChild(nInfo);// <Info />

            //XmlElement nContent = doc.CreateElement("Content");// <Content>
            //foreach (var item in this)
            //{
            //    XmlElement n = doc.CreateElement("Item"); // <Item>
            //    n.SetAttribute("key", item.Key);
            //    n.SetAttribute("value", item.Value.ToString());
            //    nContent.AppendChild(n);// <Item />
            //}
            //nType.AppendChild(nContent);

            //root.AppendChild(nType);// </Content>
            //doc.AppendChild(root);// </ImageGlass>

            //doc.Save(Filename);
        }




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
            string value;
            return (_dictionary.TryGetValue(item.Key, out value) && value.Equals(item.Key));
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
    }
}

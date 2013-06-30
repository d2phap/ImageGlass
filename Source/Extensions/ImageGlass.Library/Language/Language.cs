using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ImageGlass.Library
{
    public class Language
    {
        private string _langCode;
        private string _langName;
        private string _author;
        private string _description;
        private Dictionary<string, string> _Items;

        #region Properties
        /// <summary>
        /// Get, set code of language
        /// </summary>
        public string LangCode
        {
            get { return _langCode; }
            set { _langCode = value; }
        }

        //Get, set name of language
        public string LangName
        {
            get { return _langName; }
            set { _langName = value; }
        }

        //Get, set author
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        /// <summary>
        /// Get, set description
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Get, set list of language string
        /// </summary>
        public Dictionary<string, string> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }
        #endregion


        /// <summary>
        /// Set default values of Language
        /// </summary>
        public Language()
        {
            _langCode = "vi";
            _langName = "Tiếng Việt";
            _author = "Dương Diệu Pháp";
            _description = "Vietnamese";

            _Items = new Dictionary<string, string>();
        }


        /// <summary>
        /// Set values of Language
        /// </summary>
        /// <param name="fileName"></param>
        public Language(string fileName)
        {
            ReadLanguageFile(fileName);
        }


        /// <summary>
        /// Read language strings from file
        /// </summary>
        /// <param name="fileName">*.igLang path</param>
        public void ReadLanguageFile(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlElement root = (XmlElement)doc.DocumentElement;// <ImageGlass>
            XmlElement nType = (XmlElement)root.SelectNodes("Language")[0]; //<Language>
            XmlElement n = (XmlElement)nType.SelectNodes("Info")[0];//<Info>

            //Get <Info> Attributes
            this.LangCode = n.GetAttribute("langCode");
            this.LangName = n.GetAttribute("langName");
            this.Author = n.GetAttribute("author");
            this.Description = n.GetAttribute("description");

            //Get <Content> Attributes
            n = (XmlElement)nType.SelectNodes("Content")[0];//<Content>
            int _itemsCount = int.Parse(n.GetAttribute("count"));

            //Get Language string items
            this.Items = new Dictionary<string, string>();
            for (int i = 0; i < _itemsCount; i++)
            {
                XmlElement node = (XmlElement)n.SelectNodes("_" + (i + 1).ToString())[0];//<_1>
                string _key = node.GetAttribute("key");
                string _value = node.GetAttribute("value");

                this.Items.Add(_key, _value);
            }

            
        }

        

    }
}

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
        private string _fileName;
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
        /// Get, set language file path
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; }
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
            _langCode = "en";
            _langName = "English";
            _author = "Dương Diệu Pháp";
            _description = "English";
            _fileName = "";

            _Items = new Dictionary<string, string>();
        }


        /// <summary>
        /// Set values of Language
        /// </summary>
        /// <param name="fileName">*.igLang path</param>
        public Language(string fileName)
        {
            _fileName = fileName;
            ReadLanguageFile();
        }


        /// <summary>
        /// Read language strings from file
        /// </summary>
        public void ReadLanguageFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_fileName);
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

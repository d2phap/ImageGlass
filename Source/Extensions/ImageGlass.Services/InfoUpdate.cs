using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGlass.Services
{


    public class InfoUpdate
    {
        private Version _newVersion;
        private string _versionType;
        private string _level;
        private Uri _link;
        private string _size;
        private string _decription;

        #region Properties
        public Version NewVersion
        {
            get { return _newVersion; }
            set { _newVersion = value; }
        }        

        public string VersionType
        {
            get { return _versionType; }
            set { _versionType = value; }
        }        

        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }        

        public Uri Link
        {
            get { return _link; }
            set { _link = value; }
        }        

        public string Size
        {
            get { return _size; }
            set { _size = value; }
        }        

        public string Decription
        {
            get { return _decription; }
            set { _decription = value; }
        }
        #endregion

        /// <summary>
        /// Provides information of element 'Info> in 'Update>
        /// </summary>
        public InfoUpdate()
        {
            _newVersion = new System.Version("1.0.0.0");
            _versionType = "Stable";
            _level = "Recommended";
            _link = new Uri("http://imageglass.org");
            _size = "0 MB";
            _decription = string.Empty;           
        }

    }
}

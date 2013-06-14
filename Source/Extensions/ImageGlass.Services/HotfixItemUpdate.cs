using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGlass.Services
{
    public class HotfixItemUpdate
    {
        private string _level;

        public string Level
        {
            get { return _level; }
            set { _level = value; }
        }
        private Version _newVersion;

        public Version NewVersion
        {
            get { return _newVersion; }
            set { _newVersion = value; }
        }
        private string _oriFile;

        public string OriginalFile
        {
            get { return _oriFile; }
            set { _oriFile = value; }
        }
        private Uri _link;

        public Uri Link
        {
            get { return _link; }
            set { _link = value; }
        }
        private string _desFile;

        public string DestinaionFile
        {
            get { return _desFile; }
            set { _desFile = value; }
        }

        /// <summary>
        /// Provides information of hotfix item
        /// </summary>
        public HotfixItemUpdate()
        {
            _level = "Recommended";
            _newVersion = new Version("1.0.0.0");
            _oriFile = string.Empty;
            _desFile = string.Empty;
            _link = new Uri("http://imageglass.org");
        }
    }
}

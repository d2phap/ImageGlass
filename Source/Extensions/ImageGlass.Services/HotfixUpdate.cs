using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGlass.Services
{
    public class HotfixUpdate
    {
        private int _count;
        private List<HotfixItemUpdate> _ds;
        private string _temp;

        #region Properties
        public string TempDir
        {
            get { return _temp; }
            set { _temp = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }        

        public List<HotfixItemUpdate> HotfixItems
        {
            get { return _ds; }
            set { _ds = value; }
        }
        #endregion

        /// <summary>
        /// Provides information of hotfix update
        /// </summary>
        public HotfixUpdate()
        {
            _count = 0;
            _temp = "{root}";
            _ds = new List<HotfixItemUpdate>();
        }

    }
}

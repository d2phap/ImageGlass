/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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

using System.Collections.Generic;

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

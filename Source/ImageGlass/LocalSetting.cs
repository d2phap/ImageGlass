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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageGlass.Core;
using System.Drawing;
using Microsoft.Win32;
using System.Configuration;
using System.Windows.Forms;
using ImageGlass.Services.Configuration;

namespace ImageGlass
{
    public static class LocalSetting
    {
        private static frmFacebook _fFacebook = new frmFacebook();
        private static frmSetting _fSetting = new frmSetting();
        private static frmExtension _fExtension = new frmExtension();

        #region "Properties"
        /// <summary>
        /// Form frmFacebook
        /// </summary>
        public static frmFacebook FFacebook
        {
            get { return LocalSetting._fFacebook; }
            set { LocalSetting._fFacebook = value; }
        }

        /// <summary>
        /// Form frmSetting
        /// </summary>
        public static frmSetting FSetting
        {
            get { return LocalSetting._fSetting; }
            set { LocalSetting._fSetting = value; }
        }

        /// <summary>
        /// Form frmExtension
        /// </summary>
        public static frmExtension FExtension
        {
            get { return LocalSetting._fExtension; }
            set { LocalSetting._fExtension = value; }
        }
        #endregion
        
    }
}




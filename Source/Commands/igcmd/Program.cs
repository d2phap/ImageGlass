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
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Ionic.Zip;
using System.Text;
using ImageGlass.Theme;

namespace igcmd
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string[] args;
        [STAThread]
        static void Main(string[] argv)
        {
            args = argv;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string topcmd = args[0].ToLower().Trim();

            if (topcmd == "iglike")
            {
                Core.Like(args[1]);
            }
            else if (topcmd == "igdislike")
            {
                Core.Dislike(args[1]);
            }
            else if (topcmd == "igupdate")//kiem tra phien ban
            {
                Core.CheckForUpdate();
            }
            else if (topcmd == "igupload")
            {
                Core.Upload(args[1], args[2]);
            }
            else if (topcmd == "igautoupdate")//tu dong kiem tra phien ban
            {
                Core.AutoUpdate();
            }
            else if (topcmd == "igpacktheme")//dong goi theme thanh *.igtheme
            {
                //cmd: igcmd.exe igpacktheme "srcDir" "desFile"
                Core.PackTheme(args[1], args[2]);
            }
            else if (topcmd == "iginstalltheme")//cai dat theme
            {
                Core.InstallTheme(args[1]);
            }
            else if (topcmd == "igfollow")//đăng ký theo dõi thông tin ImageGlass
            {
                Core.Follow();
            }
        }



        
                


    }
}

﻿/*
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
using System.IO;
using ImageGlass.Theme;
using Ionic.Zip;
using System.Windows.Forms;
using System.Diagnostics;
using ImageGlass.Services;
using ImageGlass.Services.Configuration;

namespace igcmd
{
    public static class Core
    {
        /// <summary>
        /// Get directory of exe file (include \)
        /// </summary>
        public static string Path
        {
            get
            {
                return (Application.StartupPath + "\\").Replace("\\\\", "\\");
            }
        }

        /// <summary>
        /// Pack theme *.igtheme
        /// </summary>
        /// <param name="dir">Thư mục chứa tập tin</param>
        /// <param name="des">Đường dẫn tập tin *.igtheme</param>
        public static void PackTheme(string src, string des)
        {
            if (!Directory.Exists(src))
            {
                return;
            }

            src = (src + "\\").Replace("\\\\", "\\");
            Theme th = new Theme(src + "config.xml");

            //create dir if is not exist
            //des = (Application.StartupPath + "\\").Replace("\\\\", "\\") + "Themes\\";
            //Directory.CreateDirectory(des);

            //if file exist, rename & backup
            if (File.Exists(des))
            {
                File.Move(des, des + ".old");
            }

            try
            {
                using (ZipFile z = new ZipFile(des, Encoding.UTF8))
                {
                    z.AddDirectory(src, th.name);
                    z.Save();
                };
            }
            catch
            {
                //if file exist, rename & backup
                if (File.Exists(des + ".old"))
                {
                    File.Move(des + ".old", des);
                }
            }

            if (File.Exists(des + ".old"))
            {
                File.Delete(des + ".old");
            }
        }

        /// <summary>
        /// Check for update
        /// </summary>
        public static void AutoUpdate()
        {
            Update up = new Update(new Uri("http://www.imageglass.org/checkforupdate"),
                GlobalSetting.StartUpDir + "update.xml");

            if (File.Exists(GlobalSetting.StartUpDir + "update.xml"))
            {
                File.Delete(GlobalSetting.StartUpDir + "update.xml");
            }

            //save last update
            GlobalSetting.SetConfig("AutoUpdate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            
            if (!up.IsError &&
                up.CheckForUpdate(GlobalSetting.StartUpDir + "ImageGlass.exe") &&
                up.Info.VersionType.ToLower() == "stable")
            {
                frmCheckForUpdate f = new frmCheckForUpdate();
                f.ShowDialog();
            }

            Application.Exit();
        }

        /// <summary>
        /// Check for update
        /// </summary>
        public static void CheckForUpdate()
        {
            Application.Run(new frmCheckForUpdate());
        }

        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="filename"></param>
        public static void Upload(string cmd, string filename)
        {
            Application.Run(new frmUpload(cmd, filename));
        }

        /// <summary>
        /// Install theme
        /// </summary>
        public static void InstallTheme(string filename)
        {
            Application.Run(new frmInstallTheme(filename));
        }
        
    }
}

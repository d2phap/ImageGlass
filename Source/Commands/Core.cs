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
using System.IO;
using ImageGlass.Theme;
using Ionic.Zip;
using System.Windows.Forms;
using System.Diagnostics;
using ImageGlass.Services;

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
        /// Đóng gói theme thành *.igtheme
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
        /// Tính năng tự động kiểm tra cập nhật
        /// </summary>
        public static void AutoUpdate()
        {
            Update up = new Update(new Uri("http://www.imageglass.org/update.xml"), "C:\\ImageGlass-Update.xml");
            if (File.Exists("C:\\ImageGlass-Update.xml")) File.Delete("C:\\ImageGlass-Update.xml");

            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\PhapSoftware\ImageGlass\",
                "AutoUpdate", DateTime.Now.Day.ToString() + "/" +
                            DateTime.Now.Month.ToString() + "/" +
                            DateTime.Now.Year.ToString());

            if (up.CheckForUpdate(Application.StartupPath + "\\ImageGlass.exe") &&
                up.Info.VersionType.ToLower() != "stable")
            {
                frmCheckForUpdate f = new frmCheckForUpdate();
                f.ShowDialog();
            }

            Application.Exit();
        }

        /// <summary>
        /// Like ImageGlass
        /// </summary>
        /// <param name="version"></param>
        public static void Like(string version)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.facebook.com/ImageGlass");
            }
            catch { }
            //Application.Run(new frmLikeOrDislike("like", version));
        }

        /// <summary>
        /// Dislike ImageGlass
        /// </summary>
        /// <param name="version"></param>
        public static void Dislike(string version)
        {
            Application.Run(new frmLikeOrDislike("dislike", version));
        }

        /// <summary>
        /// Kiểm tra cập nhật
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
        /// Cài đặt theme
        /// </summary>
        public static void InstallTheme(string filename)
        {
            Application.Run(new frmInstallTheme(filename));
        }

        /// <summary>
        /// Đăng ký theo dõi thông tin từ ImageGlass
        /// </summary>
        public static void Follow()
        {
            Application.Run(new frmFollow());
        }

    }
}

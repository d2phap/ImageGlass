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
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace ImageGlass
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string[] args;
        public static string igPath = (Application.StartupPath + "\\").Replace("\\\\", "\\");
        [STAThread]
        static void Main(string[] argv)
        {
            args = argv;            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());


            //autoupdate--------------------------------------------------------------------------------
            string s = Setting.GetConfig("AutoUpdate", "1");

            if (s != "0")
            {
                DateTime lastUpdate = DateTime.Now;

                if(DateTime.TryParse(s, out lastUpdate))
                {
                    //Kiểm tra cập nhật 7 ngày / lần
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 7)
                    {
                        Process.Start(char.ConvertFromUtf32(34) +
                                Program.igPath + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "igautoupdate");
                    }
                }

            }

            try
            {
                //dang ky ext *.igtheme---------------------------------------------------------------------
                Registry.SetValue("HKEY_CLASSES_ROOT\\.igtheme\\", "", "ImageGlass.igtheme");
                Registry.SetValue("HKEY_CLASSES_ROOT\\ImageGlass.igtheme\\", "", "ImageGlass theme");

                //icon
                Registry.SetValue("HKEY_CLASSES_ROOT\\ImageGlass.igtheme\\DefaultIcon\\", "",
                    char.ConvertFromUtf32(34) + Program.igPath + "igcmd.exe" + char.ConvertFromUtf32(34) +
                    ",0");

                Registry.SetValue("HKEY_CLASSES_ROOT\\ImageGlass.igtheme\\shell\\open\\command\\", "",
                    char.ConvertFromUtf32(34) + Program.igPath + "igcmd.exe" + char.ConvertFromUtf32(34) +
                    " iginstalltheme " + char.ConvertFromUtf32(34) + "%1" + char.ConvertFromUtf32(34));
            }
            catch { }


            //xoa thu muc Temp---------------------------------------------------------------------------
            if (Directory.Exists(igPath + "Temp"))
            {
                try
                {
                    Directory.Delete(igPath + "Temp", true);
                }
                catch { }
            }



        }
    }
}

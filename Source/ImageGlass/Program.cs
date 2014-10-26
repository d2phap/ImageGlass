/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
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
using ImageGlass.Services.Configuration;

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


            //autoupdate----------------------------------------------------------------
            string s = GlobalSetting.GetConfig("AutoUpdate", DateTime.Now.ToString());

            if (s != "0")
            {
                DateTime lastUpdate = DateTime.Now;

                if(DateTime.TryParse(s, out lastUpdate))
                {
                    //Check for update every 7 days
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 7)
                    {
                        Process.Start(char.ConvertFromUtf32(34) +
                                Program.igPath + "igcmd.exe" +
                                char.ConvertFromUtf32(34), "igautoupdate");
                    }
                }
            }



        }
    }
}

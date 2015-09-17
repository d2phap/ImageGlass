/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2015 DUONG DIEU PHAP
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
using System.Windows.Forms;
using System.Diagnostics;
using ImageGlass.Services.Configuration;
using ImageGlass.Services.InstanceManagement;

namespace ImageGlass
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static string appGuid = "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";
        private static frmMain formMain;

        [STAThread]
        static void Main(string[] argv)
        {
            Guid guid = new Guid(appGuid);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //auto update----------------------------------------------------------------
            string s = GlobalSetting.GetConfig("AutoUpdate", "1/1/2015 0:0:0");

            if (s != "0")
            {
                DateTime lastUpdate = DateTime.Now;

                if (DateTime.TryParse(s, out lastUpdate))
                {
                    //Check for update every 7 days
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 7)
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = GlobalSetting.StartUpDir + "igcmd.exe";
                        p.StartInfo.Arguments = "igautoupdate";
                        p.Start();
                    }
                }
            }            

            //get current config
            GlobalSetting.IsAllowMultiInstances = bool.Parse(GlobalSetting.GetConfig("IsAllowMultiInstances", "true"));

            //check if allows multi instances
            if (GlobalSetting.IsAllowMultiInstances)
            {
                Application.Run(formMain = new frmMain());
            }
            else
            {
                //single instance is required
                using (SingleInstance singleInstance = new SingleInstance(guid))
                {
                    if (singleInstance.IsFirstInstance)
                    {
                        singleInstance.ArgumentsReceived += SingleInstance_ArgumentsReceived;
                        singleInstance.ListenForArgumentsFromSuccessiveInstances();

                        Application.Run(formMain = new frmMain());
                    }
                    else
                    {
                        singleInstance.PassArgumentsToFirstInstance(Environment.GetCommandLineArgs());
                    }
                }
            } //end check multi instances

        }

        private static void SingleInstance_ArgumentsReceived(object sender, ArgumentsReceivedEventArgs e)
        {
            if (formMain == null)
                return;

            Action<String[]> updateForm = arguments =>
            {
                formMain.WindowState = FormWindowState.Normal;
                formMain.LoadFromParams(arguments);
            };

            //Execute our delegate on the forms thread!
            formMain.Invoke(updateForm, (Object)e.Args); 

            // send our Win32 message to bring ImageGlass dialog to top
            NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
        }
    }


}

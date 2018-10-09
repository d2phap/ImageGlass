/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2018 DUONG DIEU PHAP
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
using System.IO;
using System.Globalization;
using System.Runtime;

namespace ImageGlass
{
    static class Program
    {
        private static string appGuid = "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";
        private static frmMain formMain;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

#if ERRORMODE
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
#endif

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            // Set up Startup Profile to improve launch performance
            // https://blogs.msdn.microsoft.com/dotnet/2012/10/18/an-easy-solution-for-improving-app-launch-performance/
            ProfileOptimization.SetProfileRoot(GlobalSetting.ConfigDir);
            ProfileOptimization.StartProfile("igstartup.profile");

#if ERRORMODE
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);
#endif

            // Windows Vista or later
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            
            Guid guid = new Guid(appGuid);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check if the start up directory writable
            GlobalSetting.IsStartUpDirWritable = GlobalSetting.CheckStartUpDirWritable();
            
            // Enable Portable mode as default if possible
            GlobalSetting.IsPortableMode = GlobalSetting.IsStartUpDirWritable;

            // Save App version
            GlobalSetting.SetConfig("AppVersion", Application.ProductVersion.ToString());

            #region Check First-launch Configs
            var firstLaunchVersion = 0;

            int.TryParse(GlobalSetting.GetConfig("FirstLaunchVersion", "0"), out firstLaunchVersion);

            if (firstLaunchVersion < GlobalSetting.FIRST_LAUNCH_VERSION)
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(GlobalSetting.StartUpDir, "igcmd.exe");
                p.StartInfo.Arguments = "firstlaunch";

                try
                {
                    p.Start();
                }
                catch { }

                Application.Exit();
                return;
            }
            #endregion


            #region Auto update
            string lastUpdateConfig = GlobalSetting.GetConfig("AutoUpdate", "7/26/1991 12:13:08 AM");

            if (lastUpdateConfig != "0")
            {
                DateTime lastUpdate = DateTime.Now;

                if (DateTime.TryParseExact(lastUpdateConfig, "M/d/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUpdate))
                {
                    //Check for update every 3 days
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 3)
                    {
                        RunCheckForUpdate();
                    }
                }
                else
                {
                    RunCheckForUpdate();
                }
            }

            
            void RunCheckForUpdate()
            {
                Process p = new Process();
                p.StartInfo.FileName = GlobalSetting.StartUpDir + "igcmd.exe";
                p.StartInfo.Arguments = "igautoupdate";
                p.Start();

                //save last update
                GlobalSetting.SetConfig("AutoUpdate", DateTime.Now.ToString("M/d/yyyy HH:mm:ss"));
            }
            #endregion


            #region Multi instances
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
            #endregion

        }


        private static void SingleInstance_ArgumentsReceived(object sender, ArgumentsReceivedEventArgs e)
        {
            if (formMain == null)
                return;

            Action<String[]> UpdateForm = arguments =>
            {
                formMain.WindowState = FormWindowState.Normal;
                formMain.LoadFromParams(arguments);
            };

            // KBR 20181009 Attempt to run a 2d instance of IG when multi-instance turned off. Primary instance
            // will crash if no file provided (e.g. by double-clicking on .EXE in explorer).
            int realcount = 0;
            foreach (var arg in e.Args)
                if (arg != null)
                    realcount++;
            string[] realargs = new string[realcount];
            Array.Copy(e.Args, realargs, realcount);

            //Execute our delegate on the forms thread!
            formMain.Invoke(UpdateForm, (Object)realargs); 

            // send our Win32 message to bring ImageGlass dialog to top
            NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
        }
    }


}

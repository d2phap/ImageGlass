/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageGlass.Base;
using ImageGlass.Services.InstanceManagement;
using ImageGlass.Settings;

namespace ImageGlass {
    internal static class Program {
        private const string appGuid = "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";
        private static frmMain formMain;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        // Issue #360: IG periodically searching for dismounted device
        [DllImport("kernel32.dll")]
        private static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes: uint {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOGPFAULTERRORBOX = 1 << 1,
            SEM_NOALIGNMENTFAULTEXCEPT = 1 << 2,
            SEM_NOOPENFILEERRORBOX = 1 << 15
        }

        // Issues #774, #855 : using this method is the ONLY way to successfully restore from minimized state!
        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint msg);

        private const uint SW_RESTORE = 0x09;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            // Issue #360: IG periodically searching for dismounted device
            // This _must_ be executed first!
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);

            // Set up Startup Profile to improve launch performance
            // https://blogs.msdn.microsoft.com/dotnet/2012/10/18/an-easy-solution-for-improving-app-launch-performance/
            ProfileOptimization.SetProfileRoot(App.ConfigDir(PathType.Dir));
            ProfileOptimization.StartProfile("igstartup.profile");

            // Load user configs
            Configs.Load();

            SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            #region Check config file compatibility
            if (!Configs.IsCompatible) {
                var msg = string.Format(Configs.Language.Items["_IncompatibleConfigs"], App.Version);
                var result = MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes) {
                    try {
                        Process.Start($"https://imageglass.org/docs/app-configs?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=incompatible_configs");
                    }
                    catch { }

                    return;
                }
            }
            #endregion


            #region Check First-launch Configs
            if (Configs.FirstLaunchVersion < Constants.FIRST_LAUNCH_VERSION) {
                using var p = new Process();
                p.StartInfo.FileName = App.StartUpDir("igcmd.exe");
                p.StartInfo.Arguments = "firstlaunch";

                try {
                    p.Start();
                }
                catch { }

                Application.Exit();
                return;
            }
            #endregion


            #region Auto check for update
            if (Configs.AutoUpdate != "0") {
                var lastUpdate = DateTime.Now;

                if (DateTime.TryParseExact(Configs.AutoUpdate, "M/d/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUpdate)) {
                    // Check for update every 3 days
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 3) {
                        CheckForUpdate(useAutoCheck: true);
                    }
                }
                else {
                    CheckForUpdate(useAutoCheck: true);
                }
            }
            #endregion


            #region Multi instances
            // check if allows multi instances
            if (Configs.IsAllowMultiInstances) {
                Application.Run(formMain = new frmMain());
            }
            else {
                var guid = new Guid(appGuid);

                // single instance is required
                using var singleInstance = new SingleInstance(guid);
                if (singleInstance.IsFirstInstance) {
                    singleInstance.ArgumentsReceived += SingleInstance_ArgsReceived;
                    singleInstance.ListenForArgumentsFromSuccessiveInstances();

                    Application.Run(formMain = new frmMain());
                }
                else {
                    _ = singleInstance.PassArgumentsToFirstInstanceAsync(Environment.GetCommandLineArgs());
                }
            } //end check multi instances
            #endregion

        }

        private static void SingleInstance_ArgsReceived(object sender, ArgumentsReceivedEventArgs e) {
            if (formMain == null)
                return;

            Action<string[]> UpdateForm = arguments => {

                // Issues #774, #855 : if IG is normal or maximized, do nothing. If IG is minimized,
                // restore it to previous state.
                if (formMain.WindowState == FormWindowState.Minimized) {
                    ShowWindow(formMain.Handle, SW_RESTORE);
                }

                formMain.LoadFromParams(arguments);
            };

            // KBR 20181009 Attempt to run a 2nd instance of IG when multi-instance turned off. Primary instance
            // will crash if no file provided (e.g. by double-clicking on .EXE in explorer).
            var realCount = 0;
            foreach (var arg in e.Args) {
                if (arg != null) {
                    realCount++;
                }
            }

            var realArgs = new string[realCount];
            Array.Copy(e.Args, realArgs, realCount);

            // Execute our delegate on the forms thread!
            formMain.Invoke(UpdateForm, (object)realArgs);

            // send our Win32 message to bring ImageGlass dialog to top
            NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// Check for updatae
        /// </summary>
        /// <param name="useAutoCheck">If TRUE, use "igautoupdate"; else "igupdate" for argument</param>
        public static void CheckForUpdate(bool useAutoCheck = false) {
            _ = Task.Run(() => {
                using var p = new Process();
                p.StartInfo.FileName = App.StartUpDir("igcmd.exe");
                p.StartInfo.Arguments = useAutoCheck ? "igautoupdate" : "igupdate";
                p.Start();

                p.WaitForExit();

                // There is a newer version
                Configs.IsNewVersionAvailable = p.ExitCode == 1;

                // save last update
                Configs.AutoUpdate = DateTime.Now.ToString("M/d/yyyy HH:mm:ss");
            });
        }
    }
}

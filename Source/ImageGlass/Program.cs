/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2022 DUONG DIEU PHAP
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
using ImageGlass.Services;
using ImageGlass.Services.InstanceManagement;
using ImageGlass.Settings;

namespace ImageGlass {
    internal static class Program {
        public const string APP_GUID = "{f2a83de1-b9ac-4461-81d0-cc4547b0b27b}";
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


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {
            // Issue #360: IG periodically searching for dismounted device
            // This MUST be executed first!
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

            // checks and enables Spider service
            _ = CheckAndRunSpiderServiceAsync();

            // check config file compatibility
            if (!CheckConfigFileCompatibility()) return;

            // check First-launch Configs
            if (!CheckFirstLaunchConfigs()) return;

            // check and run auto-update
            CheckAndRunAutoUpdate();

            // checks and runs app instance(s)
            RunAppInstances();

        }


        /// <summary>
        /// Checks the config file compatibility.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><c>true</c> if the config file is compatible.</item>
        ///   <item><c>false</c> if the config file needs user's attention.</item>
        /// </list>
        /// </returns>
        private static bool CheckConfigFileCompatibility() {
            var canContinue = true;

            if (!Configs.IsCompatible) {
                var msg = string.Format(Configs.Language.Items["_IncompatibleConfigs"], App.Version);
                var result = MessageBox.Show(msg, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes) {
                    try {
                        Process.Start($"https://imageglass.org/docs/app-configs?utm_source=app_{App.Version}&utm_medium=app_click&utm_campaign=incompatible_configs");
                    }
                    catch { }

                    canContinue = false;
                }
            }

            return canContinue;
        }


        /// <summary>
        /// Checks if the First launch configs dialog should be shown.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        ///   <item><c>true</c> if the config file is compatible.</item>
        ///   <item><c>false</c> if the config file needs user's attention.</item>
        /// </list>
        /// </returns>
        private static bool CheckFirstLaunchConfigs() {
            var canContinue = true;

            if (Configs.FirstLaunchVersion < Constants.FIRST_LAUNCH_VERSION) {
                using var p = new Process();
                p.StartInfo.FileName = App.StartUpDir("igcmd.exe");

                // update from <=v8.3 to v8.4
                if (Configs.FirstLaunchVersion >= 5) {
                    // show privacy update
                    p.StartInfo.Arguments = "firstlaunch 2";
                }
                else {
                    p.StartInfo.Arguments = "firstlaunch";
                }

                try {
                    p.Start();
                }
                catch { }

                Application.Exit();
                canContinue = false;
            }

            return canContinue;
        }


        /// <summary>
        /// Checks and runs auto-update.
        /// </summary>
        private static void CheckAndRunAutoUpdate() {
            if (Configs.AutoUpdate != "0") {
                if (DateTime.TryParseExact(
                    Configs.AutoUpdate,
                    "M/d/yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var lastUpdate)) {

                    // Check for update every 5 days
                    if (DateTime.Now.Subtract(lastUpdate).TotalDays > 5) {
                        CheckForUpdate(useAutoCheck: true);
                    }
                }
                else {
                    CheckForUpdate(useAutoCheck: true);
                }
            }
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


        /// <summary>
        /// Checks and runs app instance(s)
        /// </summary>
        private static void RunAppInstances() {
            if (Configs.IsAllowMultiInstances) {
                Application.Run(formMain = new frmMain());
            }
            else {
                var guid = new Guid(APP_GUID);

                // single instance is required
                using var instance = new SingleInstance(guid);
                if (instance.IsFirstInstance) {
                    instance.ArgumentsReceived += Instance_ArgsReceived;
                    instance.ListenForArgumentsFromSuccessiveInstances();

                    Application.Run(formMain = new frmMain());
                }
                else {
                    _ = instance.PassArgumentsToFirstInstanceAsync(Environment.GetCommandLineArgs());
                }
            }
        }


        private static void Instance_ArgsReceived(object sender, ArgumentsReceivedEventArgs e) {
            if (formMain == null) return;

            Action<string[]> UpdateForm = args => {
                // activate form
                _ = formMain.ToggleAppVisibilityAsync(true);

                // load image file from arg
                formMain.LoadFromParams(args);
            };

            // KBR 20181009 Attempt to run a 2nd instance of IG when multi-instance turned off.
            // Primary instance will crash if no file provided
            // (e.g. by double-clicking on .EXE in explorer).
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
        }


        /// <summary>
        /// Checks and enables Spider service
        /// </summary>
        /// <returns></returns>
        private static async Task CheckAndRunSpiderServiceAsync() {
            if (Configs.IsEnableSpiderService) {
                await Task.Delay(5000);

                SpiderService.Enable();
            }
        }

    }
}

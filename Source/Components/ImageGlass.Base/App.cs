/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2020 DUONG DIEU PHAP
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImageGlass.Base
{
    /// <summary>
    /// The base information of ImageGlass app
    /// </summary>
    public static class App
    {
        /// <summary>
        /// Gets the application version
        /// </summary>
        public static string Version { get => Application.ProductVersion; }


        /// <summary>
        /// Gets the application version
        /// </summary>
        public static string IGExePath { get => StartUpDir("ImageGlass.exe"); }


        /// <summary>
        /// Gets value of Portable mode if the startup dir is writable
        /// </summary>
        // Issue #676 switch to CheckFolderWritable
        public static bool IsPortable => Helpers.CheckFolderWritable(StartUpDir());


        /// <summary>
        /// Get the path based on the startup folder of ImageGlass.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string StartUpDir(params string[] paths)
        {
            var path = Application.StartupPath;

            var newPaths = paths.ToList();
            newPaths.Insert(0, path);

            return Path.Combine(newPaths.ToArray());
        }


        /// <summary>
        /// Returns the path based on the configuration folder of ImageGlass.
        /// For portable mode, ConfigDir = Installed Dir, else %appdata%\ImageGlass
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string ConfigDir(params string[] paths)
        {
            // use StartUp dir if it's writable
            var startUpDir = StartUpDir(paths);
            // Issue #676 switch to CheckFolderWritable
            if (Helpers.CheckFolderWritable(startUpDir))
            {
                return startUpDir;
            }

            // else, use AppData dir
            var igAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImageGlass");

            var newPaths = paths.ToList();
            newPaths.Insert(0, igAppDataDir);
            igAppDataDir = Path.Combine(newPaths.ToArray());

            return igAppDataDir;
        }


        /// <summary>
        /// Parse string to absolute path
        /// </summary>
        /// <param name="inputPath">The relative/absolute path of file/folder; or a URI Scheme</param>
        /// <returns></returns>
        public static string ToAbsolutePath(string inputPath)
        {
            var path = inputPath;
            var protocol = Constants.URI_SCHEME + ":";

            // If inputPath is URI Scheme
            if (path.StartsWith(protocol))
            {
                // Retrieve the real path
                path = Uri.UnescapeDataString(path).Remove(0, protocol.Length);
            }

            // Parse environment vars to absolute path
            path = Environment.ExpandEnvironmentVariables(path);

            return path;
        }


        /// <summary>
        /// Center the given form to the current screen.
        /// Note***: The method Form.CenterToScreen() contains a bug: 
        /// https://stackoverflow.com/a/6837499/2856887
        /// </summary>
        /// <param name="form">The form to center</param>
        public static void CenterFormToScreen(Form form)
        {
            var screen = Screen.FromControl(form);

            var workingArea = screen.WorkingArea;
            form.Location = new Point()
            {
                X = Math.Max(workingArea.X, workingArea.X + (workingArea.Width - form.Width) / 2),
                Y = Math.Max(workingArea.Y, workingArea.Y + (workingArea.Height - form.Height) / 2)
            };
        }


        /// <summary>
        /// Write log in DEBUG mode
        /// </summary>
        /// <param name="msg"></param>
        public static void LogIt(string msg)
        {
#if DEBUG
            try
            {
                var tempDir = App.ConfigDir(Dir.Log);
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }
                var path = Path.Combine(tempDir, "iglog.log");

                using (TextWriter tw = new StreamWriter(path, append: true))
                {
                    tw.WriteLine(msg);
                    tw.Flush();
                    tw.Close();
                }
            }
            catch { }
#endif
        }

    }
}

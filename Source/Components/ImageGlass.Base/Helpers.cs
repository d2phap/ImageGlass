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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ImageGlass.Base {
    /// <summary>
    /// The helper functions used globally
    /// </summary>
    public static class Helpers {

        #region Public functions

        /// <summary>
        /// Check if the given path (file or directory) is writable.
        /// </summary>
        /// <param name="type">Indicates if the given path is either file or directory</param>
        /// <param name="path">Full path of file or directory</param>
        /// <returns></returns>
        public static bool CheckPathWritable(PathType type, string path) {
            try {
                // if path is directory
                if (type == PathType.Dir) {
                    TestDirWrittable(path);
                }
                // If path is file
                else if (File.Exists(path)) {
                    using (File.OpenWrite(path)) { }
                }
                // if path is non-exist file
                else if (type == PathType.File) {
                    var dir = Path.GetDirectoryName(path);
                    TestDirWrittable(dir);
                }

                return true;
            }
            catch {
                return false;
            }
        }


        /// <summary>
        /// Convert string to int array, where numbers are separated by semicolons
        /// </summary>
        /// <param name="str">Input string. E.g. "12; -40; 50"</param>
        /// <param name="unsignedOnly">whether negative numbers are allowed</param>
        /// <param name="distinct">whether repitition of values is allowed</param>
        /// <returns></returns>
        public static int[] StringToIntArray(string str, bool unsignedOnly = false, bool distinct = false) {
            var args = str.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<int>();

            foreach (var item in args) {
                // Issue #677 : don't throw exception if we encounter invalid number, e.g. the comma-separated zoom values from pre-V7.5
                if (!int.TryParse(item, System.Globalization.NumberStyles.Integer, Constants.NumberFormat, out var num))
                    continue;

                if (unsignedOnly && num < 0) {
                    continue;
                }

                numbers.Add(num);
            }

            if (distinct) {
                numbers = numbers.Distinct().ToList();
            }

            return numbers.ToArray();
        }

        /// <summary>
        /// Convert int array to semi-colon delimited string
        /// </summary>
        /// <param name="array">Input int array</param>
        /// <returns></returns>
        public static string IntArrayToString(int[] array) {
            return string.Join(";", array);
        }

        /// <summary>
        /// Convert string to Rectangle - input string must have four integer values
        /// (Left;Top;Width;Height)
        /// </summary>
        /// <param name="str">Input string. E.g. "12; 40; 50; 60"</param>
        /// <returns></returns>
        public static Rectangle StringToRect(string str) {
            var args = StringToIntArray(str);

            if (args.Length == 4) {
                return new Rectangle(args[0], args[1], args[2], args[3]);
            }

            return new Rectangle();
        }

        /// <summary>
        /// Convert Rectangle to String
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static string RectToString(Rectangle rc) {
            return rc.Left + ";" + rc.Top + ";" + rc.Width + ";" + rc.Height;
        }


        /// <summary>
        /// Checks if the given rectangle is visible on any screen
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool IsVisibleOnAnyScreen(Rectangle rect) {
            foreach (var screen in Screen.AllScreens) {
                if (screen.WorkingArea.IntersectsWith(rect))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Get all controls by type
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Control> GetAllControls(Control control, Type type) {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        #endregion


        #region Private functions
        /// <summary>
        /// Tests if the given directory is writtable.
        /// </summary>
        /// <param name="dir"></param>
        private static void TestDirWrittable(string dir) {
            var isDirExist = Directory.Exists(dir);

            if (!isDirExist) {
                Directory.CreateDirectory(dir);
            }

            var sampleFile = Path.Combine(dir, "test_write_file.temp");

            using (File.Create(sampleFile)) { }
            File.Delete(sampleFile);

            if (!isDirExist) {
                Directory.Delete(dir, true);
            }
        }
        #endregion
    }
}

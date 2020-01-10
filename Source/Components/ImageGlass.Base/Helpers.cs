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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageGlass.Base
{
    /// <summary>
    /// The helper functions used globally
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Determine if a path to a folder is writable. This function assumes
        /// that the path is a directory and not a file. This function accepts
        /// a path of any format, unlike CheckPathWritable.
        /// </summary>
        /// <param name="path">the path to a folder</param>
        /// <returns>true if folder is writable</returns>
        public static bool CheckFolderWritable(string path)
        {
            try
            {
                var isDirExist = Directory.Exists(path);

                if (!isDirExist)
                {
                    Directory.CreateDirectory(path);
                }
                    
                var sampleFile = Path.Combine(path, "test_write_file.temp");

                using (File.Create(sampleFile)) { }
                File.Delete(sampleFile);

                if (!isDirExist)
                {
                    Directory.Delete(path, true);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the given path (file or directory) is writable. 
        /// Note**: This function does not handle the directory name that contains '.'
        /// E.g. C:\dir\new.dir will be treated as a File!
        /// </summary>
        /// <param name="path">Full path of file or directory</param>
        /// <returns></returns>
        public static bool CheckPathWritable(string path)
        {
            try
            {
                // If path is file
                if (Path.HasExtension(path))
                {
                    using (File.OpenWrite(path)) { }
                }
                // if path is directory
                else
                {
                    return CheckFolderWritable(path);
                }

                return true;
            }
            catch
            {
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
        public static int[] StringToIntArray(string str, bool unsignedOnly = false, bool distinct = false)
        {
            var args = str.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<int>();

            foreach (var item in args)
            {
                // Issue #677 : don't throw exception if we encounter invalid number, e.g. the comma-separated zoom values from pre-V7.5
                if (!int.TryParse(item, System.Globalization.NumberStyles.Integer, Constants.NumberFormat, out var num))
                    continue;

                if (unsignedOnly && num < 0)
                {
                    continue;
                }

                numbers.Add(num);
            }

            if (distinct)
            {
                numbers = numbers.Distinct().ToList();
            }

            return numbers.ToArray();
        }


        /// <summary>
        /// Convert int array to semi-colon delimited string
        /// </summary>
        /// <param name="array">Input int array</param>
        /// <returns></returns>
        public static string IntArrayToString(int[] array)
        {
            return string.Join(";", array);
        }


        /// <summary>
        /// Convert string to Rectangle - input string must have four integer values
        /// (Left;Top;Width;Height)
        /// </summary>
        /// <param name="str">Input string. E.g. "12; 40; 50; 60"</param>
        /// <returns></returns>
        public static Rectangle StringToRect(string str)
        {
            var args = StringToIntArray(str);

            if (args.Length == 4)
            {
                return new Rectangle(args[0], args[1], args[2], args[3]);
            }

            return new Rectangle();
        }


        /// <summary>
        /// Convert Rectangle to String
        /// </summary>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static string RectToString(Rectangle rc)
        {
            return rc.Left + ";" + rc.Top + ";" + rc.Width + ";" + rc.Height;
        }

    }
}

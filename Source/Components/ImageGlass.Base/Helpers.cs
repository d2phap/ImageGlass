/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
    /// The helper functions used globaly
    /// </summary>
    public static class Helpers
    {
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
                    var sampleFile = Path.Combine(path, "test_write_file.temp");

                    using (File.Create(sampleFile)) { }
                    File.Delete(sampleFile);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Convert string to int array
        /// </summary>
        /// <param name="str">Input string. E.g. "12, -40, 50"</param>
        /// <returns></returns>
        public static int[] StringToIntArray(string str, bool unsignedOnly = false, bool distinct = false)
        {
            var args = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<int>();

            foreach (var item in args)
            {
                var num = int.Parse(item, Constants.NumberFormat);
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
        /// Convert int array to string
        /// </summary>
        /// <param name="array">Input int array</param>
        /// <returns></returns>
        public static string IntArrayToString(int[] array)
        {
            return string.Join(";", array);
        }


        /// <summary>
        /// Convert string to Rectangle
        /// </summary>
        /// <param name="str">Input string. E.g. "12, 40, 50"</param>
        /// <returns></returns>
        public static Rectangle StringToRect(string str)
        {
            var args = StringToIntArray(str);

            if (args.Count() == 4)
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

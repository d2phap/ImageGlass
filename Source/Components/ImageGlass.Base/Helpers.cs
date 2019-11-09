using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageGlass.Base
{
    public class Helpers
    {
        /// <summary>
        /// Check if the given path (file or directory) is writable
        /// </summary>
        /// <param name="path">Full path of file or directory</param>
        /// <returns></returns>
        public static bool CheckPathWritable(string path)
        {
            try
            {
                // If path is file
                if (File.Exists(path))
                {
                    using (File.OpenWrite(path)) { }
                }
                // if path is directory
                else if (Directory.Exists(path))
                {
                    var sampleFile = Path.Combine(path, "test_write_file.temp");

                    using (File.Create(sampleFile)) { }
                    File.Delete(sampleFile);
                }
                // path not found
                else
                {
                    return false;
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
            var args = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
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
            return string.Join(",", array);
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
            return rc.Left + "," + rc.Top + "," + rc.Width + "," + rc.Height;
        }

    }
}

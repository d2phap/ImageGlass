using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Security.Permissions;

namespace ImageGlass.Settings
{
    public class Helpers
    {
        /// <summary>
        /// Check if the given dir is writable
        /// </summary>
        /// <param name="dir">Full path of dir</param>
        /// <returns></returns>
        public static bool IsDirWritable(string dir)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, dir);
            permissionSet.AddPermission(writePermission);

            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
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

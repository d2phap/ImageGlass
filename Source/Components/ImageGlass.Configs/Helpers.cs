using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

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
            if (IsDirWritable(startUpDir))
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


        
        

    }
}

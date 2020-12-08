/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.VisualBasic.FileIO;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageGlass.Library.Image {
    public static class ImageInfo {
        [StructLayout(LayoutKind.Sequential)]
        private struct SHELLEXECUTEINFO {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            public string lpClass;
            public IntPtr hkeyClass;
            public int dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int ShellExecuteEx(ref SHELLEXECUTEINFO s);

        /// <summary>
        /// Show file property dialog
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="hwnd"></param>
        public static void DisplayFileProperties(string fileName, IntPtr hwnd) {
            const int SEE_MASK_INVOKEIDLIST = 0xc;
            const int SW_SHOW = 5;
            var shInfo = new SHELLEXECUTEINFO();

            shInfo.cbSize = Marshal.SizeOf(shInfo);
            shInfo.lpFile = fileName;
            shInfo.nShow = SW_SHOW;
            shInfo.fMask = SEE_MASK_INVOKEIDLIST;
            shInfo.lpVerb = "properties";
            shInfo.lpParameters = "Details";
            shInfo.hwnd = hwnd;

            ShellExecuteEx(ref shInfo);
        }

        /// <summary>
        /// Get image type name
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static string GetImageFileType(string filename) {
            var ext = Path.GetExtension(filename).Replace(".", "").ToLower();

            switch (ext) {
                case "bmp":
                    return "Bitmap Image File";
                case "dib":
                    return "Device Independent Bitmap File";
                case "jpg":
                    return "JPEG Image File";
                case "jpeg":
                    return "Joint Photographic Experts Group";
                case "jfif":
                    return "JPEG File Interchange Format";
                case "jpe":
                    return "JPEG Image File";
                case "png":
                    return "Portable Network Graphics";
                case "gif":
                    return "Graphics Interchange Format File";
                case "ico":
                    return "Icon File";
                case "emf":
                    return "Enhanced Windows Metafile";
                case "exif":
                    return "Exchangeable Image Information File";
                case "wmf":
                    return "Windows Metafile";
                case "tif":
                    return "Tagged Image File";
                case "tiff":
                    return "Tagged Image File Format";
                default:
                    return ext.ToUpper() + " File";
            }
        }

        /// <summary>
        /// Get file size format
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetFileSize(string filename) {
            try {
                const double mod = 1024;
                var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };

                var fi = new FileInfo(filename);
                double sized = fi.Length * 1.0f;
                int i;

                for (i = 0; sized > mod; i++) {
                    sized /= mod;
                }

                return string.Format("{0} {1}", Math.Round(sized, 2), units[i]);
            }
            catch { }

            return " ";
        }

        /// <summary>
        /// Get image size, Width x height string
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static string GetWxHSize(string filename) {
            try {
                if (!string.Equals(Path.GetExtension(filename), ".ico", StringComparison.CurrentCultureIgnoreCase)) {
                    using (var img = System.Drawing.Image.FromFile(filename)) {
                        //get Width x Height
                        return Convert.ToString(img.Width) + " x " + Convert.ToString(img.Height);
                    }
                }
                else {
                    var ico = new Icon(filename);
                    //get Width x Height
                    return Convert.ToString(ico.Width) + " x " + Convert.ToString(ico.Height);
                }
            }
            catch {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get image resolution
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetImageResolution(string filename) {
            try {
                double h = 0;
                double v = 0;

                if (!string.Equals(Path.GetExtension(filename), ".ico", StringComparison.CurrentCultureIgnoreCase)) {
                    using (var img = System.Drawing.Image.FromFile(filename)) {
                        //get HorizontalResolution 
                        h = Math.Round((double)img.HorizontalResolution, 2);

                        //get VerticalResolution
                        v = Math.Round((double)img.VerticalResolution, 2);
                    }
                }
                else {
                    using (var ico = new Icon(filename)) {
                        //get HorizontalResolution 
                        h = Math.Round(ico.ToBitmap().HorizontalResolution, 2);

                        //get VerticalResolution
                        v = Math.Round(ico.ToBitmap().VerticalResolution, 2);
                    }
                }

                return string.Format("{0} x {1}", h, v);
            }
            catch { }

            return " ";
        }

        /// <summary>
        /// Get file creation time
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static DateTime GetCreateTime(string filename) {
            var fi = new FileInfo(filename);

            //get Create Time
            return fi.CreationTime;
        }

        /// <summary>
        /// Get file last access
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static DateTime GetLastAccess(string filename) {
            var fi = new FileInfo(filename);
            //get Create Time
            return fi.LastAccessTime;
        }

        /// <summary>
        /// Get file write time
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static DateTime GetWriteTime(string filename) {
            var fi = new FileInfo(filename);

            return fi.LastWriteTime;
        }

        /// <summary>
        /// Rename file
        /// </summary>
        /// <param name="oldFileName">old file name</param>
        /// <param name="newFileName">new file name</param>
        public static void RenameFile(string oldFileName, string newFileName) {
            // Issue 73: Windows ignores case-only changes
            if (string.Equals(oldFileName, newFileName, StringComparison.InvariantCultureIgnoreCase)) {
                // user changing only the case of the filename. Need to perform a trick.
                File.Move(oldFileName, oldFileName + "_imgglass_extra");
                File.Move(oldFileName + "_imgglass_extra", newFileName);
            }
            else {
                File.Move(oldFileName, newFileName);
            }
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="isMoveToRecycleBin">True: Move to Recycle bin | False: Delete permanently</param>
        /// <returns></returns>
        public static void DeleteFile(string fileName, bool isMoveToRecycleBin = true) {
            var option = isMoveToRecycleBin ? RecycleOption.SendToRecycleBin : RecycleOption.DeletePermanently;

            FileSystem.DeleteFile(fileName, UIOption.OnlyErrorDialogs, option);
        }
    }
}

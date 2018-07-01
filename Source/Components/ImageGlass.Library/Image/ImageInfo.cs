/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2014 DUONG DIEU PHAP
Project homepage: http://imageglass.org

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

using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;
using ImageGlass.Core;

namespace ImageGlass.Library.Image
{
    public class ImageInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SHELLEXECUTEINFO
        {
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
        public static void DisplayFileProperties(string fileName, IntPtr hwnd)
        {
            const int SEE_MASK_INVOKEIDLIST = 0xc;
            const int SW_SHOW = 5;
            SHELLEXECUTEINFO shInfo = new SHELLEXECUTEINFO();

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
        /// Convert image to another format
        /// </summary>
        /// <param name="pic">Image source</param>
        /// <param name="filename">Filename</param>
        public static string ConvertImage(System.Drawing.Image pic, string filename)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "BMP|*.bmp|EMF|*.emf|EXIF|*.exif|GIF|*.gif|ICO|*.ico|JPG|*.jpg|PNG|*.png|TIFF|*.tiff|WMF|*.wmf|Base64String (*.txt)|*.txt";
            s.FileName = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename).Substring(1);

            switch (ext.ToLower())
            {
                case "bmp":
                    s.FilterIndex = 1;
                    break;
                case "emf":
                    s.FilterIndex = 2;
                    break;
                case "exif":
                    s.FilterIndex = 3;
                    break;
                case "gif":
                    s.FilterIndex = 4;
                    break;
                case "ico":
                    s.FilterIndex = 5;
                    break;
                case "jpg":
                    s.FilterIndex = 6;
                    break;
                case "png":
                    s.FilterIndex = 7;
                    break;
                case "tiff":
                    s.FilterIndex = 8;
                    break;
                case "wmf":
                    s.FilterIndex = 9;
                    break;
            }
            

            if (s.ShowDialog() == DialogResult.OK)
            {
                // used to avoid the following consecutive exceptions:
                // System.ObjectDisposedException
                // System.Runtime.InteropServices.ExternalException
                var clonedPic = new Bitmap(pic);

                switch (s.FilterIndex)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 7:
                        Interpreter.SaveImage(clonedPic, s.FileName);
                        break;
                    case 2:
                        clonedPic.Save(s.FileName, ImageFormat.Emf);
                        break;
                    case 3:
                        clonedPic.Save(s.FileName, ImageFormat.Exif);
                        break;
                    case 5:
                        clonedPic.Save(s.FileName, ImageFormat.Icon);
                        break;
                    case 8:
                        clonedPic.Save(s.FileName, ImageFormat.Tiff);
                        break;
                    case 9:
                        clonedPic.Save(s.FileName, ImageFormat.Wmf);
                        break;
                    case 10:
                        using (MemoryStream ms = new MemoryStream())
                        {
                            try
                            {
                                clonedPic.Save(ms, ImageFormat.Png);
                                string base64string = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());

                                using (StreamWriter fs = new StreamWriter(s.FileName))
                                {
                                    fs.Write(base64string);
                                    fs.Flush();
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Sorry, ImageGlass cannot convert this image because this error: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        
                        break;
                }

                // free resources
                clonedPic.Dispose();

                return s.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Get image type name
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static string GetImageFileType(string filename)
        {
            string ext = System.IO.Path.GetExtension(filename).Replace(".", "").ToLower();

            switch (ext)
            {
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
        /// Get image file type
        /// </summary>
        /// <param name="extension">extension code</param>
        /// <param name="extensionOnly">Always true</param>
        /// <returns></returns>
        public static string GetImageFileType(string extension, bool extensionOnly)
        {
            string ext = extension.Replace(".", "").ToLower();

            switch (ext)
            {
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
        public static string GetFileSize(string filename)
        {
            try
            {
                double mod = 1024;
                string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };

                System.IO.FileInfo fi = new System.IO.FileInfo(filename);
                double sized = fi.Length * 1.0f;
                int i;

                for (i = 0; sized > mod; i++)
                {
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
        public static string GetWxHSize(string filename)
        {
            try
            {
                if (System.IO.Path.GetExtension(filename).ToLower() != ".ico")
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(filename);
                    //get Width x Height
                    return Convert.ToString(img.Width) + " x " + Convert.ToString(img.Height);
                }
                else
                {
                    Icon ico = new Icon(filename);
                    //get Width x Height
                    return Convert.ToString(ico.Width) + " x " + Convert.ToString(ico.Height);
                }
            }
            catch
            {
                return string.Empty;
            }

        }

        /// <summary>
        /// Get image resolution
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetImageResolution(string filename)
        {
            try
            {
                double h = 0;
                double v = 0;

                if (System.IO.Path.GetExtension(filename).ToLower() != ".ico")
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(filename);

                    //get HorizontalResolution 
                    h = Math.Round((double)img.HorizontalResolution, 2);

                    //get VerticalResolution
                    v = Math.Round((double)img.VerticalResolution, 2);
                }
                else
                {
                    Icon ico = new Icon(filename);

                    //get HorizontalResolution 
                    h = Math.Round(ico.ToBitmap().HorizontalResolution, 2);

                    //get VerticalResolution
                    v = Math.Round(ico.ToBitmap().VerticalResolution, 2);
                }

                return string.Format("{0} x {1}", h, v);
            }
            catch {}

            return " ";
        }
        

        /// <summary>
        /// Get file creation time
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static System.DateTime GetCreateTime(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);

            //get Create Time
            return fi.CreationTime;
        }

        /// <summary>
        /// Get file last access
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static System.DateTime GetLastAccess(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            //get Create Time
            return fi.LastAccessTime;
        }

        /// <summary>
        /// Get file write time
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns></returns>
        public static System.DateTime GetWriteTime(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);

            return fi.LastWriteTime;
        }

        /// <summary>
        /// Rename file
        /// </summary>
        /// <param name="oldFileName">old file name</param>
        /// <param name="newFileName">new file name</param>
        public static void RenameFile(string oldFileName, string newFileName)
        {
            // Issue 73: Windows ignores case-only changes
            if (oldFileName.ToLowerInvariant() == newFileName.ToLowerInvariant())
            {
                // user changing only the case of the filename. Need to perform a trick.
                File.Move(oldFileName, oldFileName + "_imgglass_extra");
                File.Move(oldFileName + "_imgglass_extra", newFileName);
            }
            else
            {
                File.Move(oldFileName, newFileName);
            }
        }


        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <returns></returns>
        public static void DeleteFile(string fileName)
        {
            FileSystem.DeleteFile(fileName, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
        }


        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">file name</param>
        /// <param name="isMoveToRecycleBin">True: Move to Recycle bin | False: Delete permanently</param>
        /// <returns></returns>
        public static void DeleteFile(string fileName, bool isMoveToRecycleBin)
        {
            if (isMoveToRecycleBin)
            {
                FileSystem.DeleteFile(fileName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            else
            {
                DeleteFile(fileName);
            }
        }



    }
}

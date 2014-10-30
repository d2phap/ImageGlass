/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;


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
            public string lpVerb;
            public string lpFile;
            public string lpParameters;
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

        [DllImport("shell32.dll", EntryPoint = "ShellExecuteEx")]
        private static extern int ShellExecute(ref SHELLEXECUTEINFO s);

        /// <summary>
        /// Hiển thị hộp thoại Properties của tập tin, thư mục
        /// </summary>
        /// <param name="fileName">Đường dẫn của tập tin</param>
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
            shInfo.hwnd = hwnd;

            ShellExecute(ref shInfo);
        }

        /// <summary>
        /// Convert image to another format
        /// </summary>
        /// <param name="pic">Image source</param>
        /// <param name="filename">Filename</param>
        public static void ConvertImage(System.Drawing.Image pic, string filename)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "BMP|*.bmp|EMF|*.emf|EXIF|*.exif|GIF|*.gif|ICO|*.ico|JPG|*.jpg|PNG|*.png|TIFF|*.tiff|WMF|*.wmf";
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
                switch (s.FilterIndex)
                {
                    case 1:
                        pic.Save(s.FileName, ImageFormat.Bmp);
                        break;
                    case 2:
                        pic.Save(s.FileName, ImageFormat.Emf);
                        break;
                    case 3:
                        pic.Save(s.FileName, ImageFormat.Exif);
                        break;
                    case 4:
                        pic.Save(s.FileName, ImageFormat.Gif);
                        break;
                    case 5:
                        pic.Save(s.FileName, ImageFormat.Icon);
                        break;
                    case 6:
                        pic.Save(s.FileName, ImageFormat.Jpeg);
                        break;
                    case 7:
                        pic.Save(s.FileName, ImageFormat.Png);
                        break;
                    case 8:
                        pic.Save(s.FileName, ImageFormat.Tiff);
                        break;
                    case 9:
                        pic.Save(s.FileName, ImageFormat.Wmf);
                        break;
                }
            }
        }

        /// <summary>
        /// Lấy tên loại định dạng của hình ảnh
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
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
        /// Lấy tên loại định dạng của hình ảnh
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="extensionOnly"></param>
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
        /// Lấy dung lượng của tập tin kèm theo đơn vị
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static string GetFileSize(string filename)
        {
            try
            {
                System.IO.FileInfo Fi = new System.IO.FileInfo(filename);
                double Size = Math.Round((Fi.Length*1.0) / 1024, 2);

                //get the size in KB
                if (Size < 1024)
                {
                    return Convert.ToString(Size) + " KB";
                }
                else //get the size in MB
                {
                    return Convert.ToString(Math.Round(Size / 1024, 2)) + " MB";
                }
            }
            catch
            {
                return " ";
            }
        }

        /// <summary>
        /// Lấy kích thước của tập tin hình ảnh, bao gồm W x H
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
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
        /// Lấy độ phân giải ngang của hình ảnh
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static double GetHorizontalResolution(string filename)
        {
            try
            {
                if (System.IO.Path.GetExtension(filename).ToLower() != ".ico")
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(filename);
                    //get HorizontalResolution 
                    return Math.Round((double)img.HorizontalResolution, 2);
                }
                else
                {
                    Icon ico = new Icon(filename);
                    //get HorizontalResolution 
                    return Math.Round(ico.ToBitmap().HorizontalResolution, 2);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lấy độ phân giải dọc của hình ảnh
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static double GetVerticalResolution(string filename)
        {
            try
            {
                if (System.IO.Path.GetExtension(filename).ToLower() != ".ico")
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(filename);
                    //get HorizontalResolution 
                    return Math.Round((double)img.VerticalResolution, 2);
                }
                else
                {
                    Icon ico = new Icon(filename);
                    //get HorizontalResolution 
                    return Math.Round(ico.ToBitmap().VerticalResolution, 2);
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lấy thời gian tạo ra tập tin
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static System.DateTime GetCreateTime(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);

            //get Create Time
            return fi.CreationTime;
        }

        /// <summary>
        /// Lấy thời gian truy cập mới nhất của tập tin
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static System.DateTime GetLastAccess(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            //get Create Time
            return fi.LastAccessTime;
        }

        /// <summary>
        /// Lấy thời gian ghi tập tin
        /// </summary>
        /// <param name="filename">Đường dẫn của tập tin</param>
        /// <returns></returns>
        public static System.DateTime GetWriteTime(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);

            return fi.LastWriteTime;
        }

        /// <summary>
        /// Đổi tên tập tin
        /// </summary>
        /// <param name="oldFileName">Đường dẫn tập tin cũ</param>
        /// <param name="newFileName">ĐƯờng dẫn tập tin mới</param>
        public static void RenameFile(string oldFileName, string newFileName)
        {
            File.Move(oldFileName, newFileName);
        }
                

        /// <summary>
        /// Xoá 1 tập tin (hiện hộp thoại)
        /// </summary>
        /// <param name="fileName">Đường dẫn tập tin</param>
        /// <returns></returns>
        public static void DeleteFile(string fileName)
        {
            FileSystem.DeleteFile(fileName, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
        }


        /// <summary>
        /// Xoá 1 tập tin (hiện hộp thoại | tuỳ chọn)
        /// </summary>
        /// <param name="fileName">Đường dẫn tập tin</param>
        /// <param name="isMoveToRecycleBin">True: Di chuyển vào Thùng rác | False: Xoá tức thì</param>
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

﻿/*
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
using System.IO;
using System.Net;

namespace ImageGlass.Library.Net
{
    public class FileDownloader
    {
        public event AmountDownloadedChangedEventHandler AmountDownloadedChanged;

        public delegate void AmountDownloadedChangedEventHandler(long iNewProgress);

        public event FileDownloadSizeObtainedEventHandler FileDownloadSizeObtained;

        public delegate void FileDownloadSizeObtainedEventHandler(long iFileSize);

        public event FileDownloadCompleteEventHandler FileDownloadComplete;

        public delegate void FileDownloadCompleteEventHandler();

        public event FileDownloadFailedEventHandler FileDownloadFailed;

        public delegate void FileDownloadFailedEventHandler(Exception ex);

        private string _currentFile = string.Empty;

        /// <summary>
        /// Tập tin hiện tại
        /// </summary>
        public string CurrentFile
        {
            get { return _currentFile; }
        }

        /// <summary>
        /// Tải 1 tập tin
        /// </summary>
        /// <param name="URL">Liên kết của tập tin</param>
        /// <param name="filename">Nơi lưu</param>
        /// <returns></returns>
        public bool DownloadFile(string URL, string filename)
        {
            try
            {
                _currentFile = GetFileName(URL);
                WebClient WC = new WebClient();
                WC.DownloadFile(URL, filename);
                if (FileDownloadComplete != null)
                {
                    FileDownloadComplete();
                }
                return true;
            }
            catch (Exception ex)
            {
                if (FileDownloadFailed != null)
                {
                    FileDownloadFailed(ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Lấy tên tập tin từ liên kết
        /// </summary>
        /// <param name="URL">Liên kết</param>
        /// <returns></returns>
        private string GetFileName(string URL)
        {
            try
            {
                return URL.Substring(URL.LastIndexOf("/") + 1);
            }
            catch
            {
                return URL;
            }
        }

        /// <summary>
        /// Tải 1 tập tin (hỗ trợ thông tin dung lượng tải)
        /// </summary>
        /// <param name="URL">Liên kết của tập tin</param>
        /// <param name="filename">Đương dẫn lưu tập tin</param>
        /// <returns></returns>
        public bool DownloadFileWithProgress(string URL, string filename)
        {
            FileStream fs = default(FileStream);
            try
            {
                _currentFile = GetFileName(URL);
                WebRequest wRemote = default(WebRequest);
                byte[] bBuffer = null;
                bBuffer = new byte[257];
                int iBytesRead = 0;
                int iTotalBytesRead = 0;

                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                wRemote = WebRequest.Create(URL);
                WebResponse myWebResponse = wRemote.GetResponse();

                if (FileDownloadSizeObtained != null)
                {
                    FileDownloadSizeObtained(myWebResponse.ContentLength);
                }
                Stream sChunks = myWebResponse.GetResponseStream();

                do
                {
                    iBytesRead = sChunks.Read(bBuffer, 0, 256);
                    fs.Write(bBuffer, 0, iBytesRead);
                    iTotalBytesRead += iBytesRead;

                    if (myWebResponse.ContentLength < iTotalBytesRead)
                    {
                        if (AmountDownloadedChanged != null)
                        {
                            AmountDownloadedChanged(myWebResponse.ContentLength);
                        }
                    }
                    else
                    {
                        if (AmountDownloadedChanged != null)
                        {
                            AmountDownloadedChanged(iTotalBytesRead);
                        }
                    }
                } while (!(iBytesRead == 0));

                sChunks.Close();
                fs.Close();

                if (FileDownloadComplete != null)
                {
                    FileDownloadComplete();
                }

                return true;
            }
            catch (Exception ex)
            {
                if ((fs != null))
                {
                    fs.Close();
                    fs = null;
                }

                if (FileDownloadFailed != null)
                {
                    FileDownloadFailed(ex);
                }
                return false;
            }
        }

        /// <summary>
        /// Định dạng đơn vị dung lượng tập tin
        /// </summary>
        /// <param name="size">Kích thước tập tin dạng số</param>
        /// <param name="donVi">Chuỗi đơn vị xuất ra</param>
        /// <returns></returns>
        public static string FormatFileSize(double size, ref string donVi)
        {
            try
            {
                int KB = 1024;
                long MB = KB * KB;

                // Return size of file in kilobytes.
                if (size < KB)
                {
                    donVi = " bytes";
                    return size.ToString("D");
                }
                else
                {
                    double fs = size / KB;

                    if (fs < 1000)
                    {
                        donVi = " KB";
                        return fs.ToString("N");
                    }
                    else if (fs < 1000000)
                    {
                        donVi = " MB";
                        return (size / MB).ToString("N");
                    }
                    else if (fs < 10000000)
                    {
                        donVi = " GB";
                        return (size / MB / KB).ToString("N");
                    }
                }
            }
            catch
            {
                return size.ToString();
            }

            return "";
        }
    }
}
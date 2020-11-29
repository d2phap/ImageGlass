/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2013 DUONG DIEU PHAP
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

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ImageGlass.Library.Net {
    public class FileDownloader {
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
        public string CurrentFile => _currentFile;

        /// <summary>
        /// Tải 1 tập tin
        /// </summary>
        /// <param name="URL">Liên kết của tập tin</param>
        /// <param name="filename">Nơi lưu</param>
        /// <returns></returns>
        public bool DownloadFile(string URL, string filename) {
            try {
                _currentFile = GetFileName(URL);
                var WC = new WebClient();
                WC.DownloadFile(URL, filename);
                FileDownloadComplete?.Invoke();
                return true;
            }
            catch (Exception ex) {
                FileDownloadFailed?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// Lấy tên tập tin từ liên kết
        /// </summary>
        /// <param name="URL">Liên kết</param>
        /// <returns></returns>
        private static string GetFileName(string URL) {
            try {
                return URL.Substring(URL.LastIndexOf("/") + 1);
            }
            catch {
                return URL;
            }
        }

        /// <summary>
        /// Tải 1 tập tin (hỗ trợ thông tin dung lượng tải)
        /// </summary>
        /// <param name="URL">Liên kết của tập tin</param>
        /// <param name="filename">Đương dẫn lưu tập tin</param>
        /// <returns></returns>
        public async Task<bool> DownloadFileWithProgressAsync(string URL, string filename) {
            FileStream fs = default;
            try {
                _currentFile = GetFileName(URL);
                WebRequest wRemote = default;
                var bBuffer = new byte[257];
                var iBytesRead = 0;
                var iTotalBytesRead = 0;

                fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                wRemote = WebRequest.Create(URL);
                var myWebResponse = await wRemote.GetResponseAsync().ConfigureAwait(true);

                FileDownloadSizeObtained?.Invoke(myWebResponse.ContentLength);
                var sChunks = myWebResponse.GetResponseStream();

                do {
                    iBytesRead = await sChunks.ReadAsync(bBuffer, 0, 256).ConfigureAwait(true);
                    await fs.WriteAsync(bBuffer, 0, iBytesRead).ConfigureAwait(true);
                    iTotalBytesRead += iBytesRead;

                    if (myWebResponse.ContentLength < iTotalBytesRead) {
                        AmountDownloadedChanged?.Invoke(myWebResponse.ContentLength);
                    }
                    else {
                        AmountDownloadedChanged?.Invoke(iTotalBytesRead);
                    }
                } while (iBytesRead != 0);

                sChunks.Close();
                fs.Close();

                FileDownloadComplete?.Invoke();

                return true;
            }
            catch (Exception ex) {
                if (fs != null) {
                    fs.Close();
                    fs = null;
                }

                FileDownloadFailed?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// Định dạng đơn vị dung lượng tập tin
        /// </summary>
        /// <param name="size">Kích thước tập tin dạng số</param>
        /// <param name="unit">Chuỗi đơn vị xuất ra</param>
        /// <returns></returns>
        public static string FormatFileSize(double size, ref string unit) {
            try {
                const int KB = 1024;
                const long MB = KB * KB;

                // Return size of file in kilobytes.
                if (size < KB) {
                    unit = " bytes";
                    return size.ToString("D");
                }
                else {
                    var fs = size / KB;

                    if (fs < 1000) {
                        unit = " KB";
                        return fs.ToString("N");
                    }
                    else if (fs < 1000000) {
                        unit = " MB";
                        return (size / MB).ToString("N");
                    }
                    else if (fs < 10000000) {
                        unit = " GB";
                        return (size / MB / KB).ToString("N");
                    }
                }
            }
            catch {
                return size.ToString();
            }

            return "";
        }
    }
}

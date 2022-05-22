/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace igcmdWin10 {
    public class WinShare {

        // declare datapackage
        private static DataPackage _dp;
        private static readonly List<string> _filenames = new List<string>();

        public static bool IsShareShown = false;

        public static void ShowShare(IntPtr windowHandle, string[] filenames) {
            if (filenames.Length == 0) return;

            IsShareShown = false;
            _filenames.Clear();
            _filenames.AddRange(filenames);

            var dtm = DataTransferManagerHelper.GetForWindow(windowHandle);

            // Set datapackage to dtm
            dtm.DataRequested += Dtm_DataRequested;

            // show window
            DataTransferManagerHelper.ShowShareUIForWindow(windowHandle);
        }

        private static async void Dtm_DataRequested(DataTransferManager sender, DataRequestedEventArgs e) {
            var deferral = e.Request.GetDeferral();

            // create datapackage
            _dp = e.Request.Data;

            // create List to hold all files to share
            var filesToShare = new List<IStorageItem>();

            if (_filenames.Count == 0) {
                return;
            }


            // Set properties of shareUI
            _dp.Properties.Title = $"Share from ImageGlass";

            try {
                if (_filenames.Count == 1) {
                    // only 1 photo is being shared
                    _dp.Properties.Description = Path.GetFileName(_filenames[0]);
                }
                else {
                    _dp.Properties.Description = string.Join("\r\n", _filenames);
                }

                for (var i = 0; i < _filenames.Count; i++) {
                    var imageFile = await StorageFile.GetFileFromPathAsync(_filenames[i]);
                    filesToShare.Add(imageFile);
                }

                _dp.SetStorageItems(filesToShare);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally {
                IsShareShown = true;
                deferral.Complete();
            }
        }
    }


    static class DataTransferManagerHelper {
        static readonly Guid _dtm_iid = new Guid(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);

        static IDataTransferManagerInterop DataTransferManagerInterop {
            get {
                return (IDataTransferManagerInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(DataTransferManager));
            }
        }

        public static DataTransferManager GetForWindow(IntPtr hwnd) {
            return DataTransferManagerInterop.GetForWindow(hwnd, _dtm_iid);
        }

        public static void ShowShareUIForWindow(IntPtr hwnd) {
            DataTransferManagerInterop.ShowShareUIForWindow(hwnd);
        }

        [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IDataTransferManagerInterop {
            DataTransferManager GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }
    }
}

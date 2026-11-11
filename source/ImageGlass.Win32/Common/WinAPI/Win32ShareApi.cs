/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ImageGlass.Win32.Common;

public static class Win32ShareApi
{
    // declare datapackage
    private static DataTransferManager? _manager;
    private static readonly List<string> _filePaths = [];


    /// <summary>
    /// Shows window Share dialog.
    /// </summary>
    public static void ShowShare(nint windowHandle, string[] filePaths)
    {
        if (filePaths.Length == 0) return;
        _filePaths.Clear();
        _filePaths.AddRange(filePaths);

        _manager?.DataRequested -= DataTransferManager_DataRequested;
        _manager ??= DataTransferManagerInterop.GetForWindow(windowHandle);

        // set datapackage to dtm
        _manager.DataRequested += DataTransferManager_DataRequested;

        // show window
        DataTransferManagerInterop.ShowShareUIForWindow(windowHandle);
    }


    private static async void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
    {
        Exception? error = null;

        // async void on a WinRT callback thread: an escaping exception kills the process silently
        try
        {
            if (_filePaths.Count == 0) return;
            var deferral = e.Request.GetDeferral();

            // create datapackage
            var dp = e.Request.Data;

            try
            {
                // Set properties of shareUI
                dp.Properties.Title = BHelper.AppDisplayName;
                dp.Properties.Description = string.Join("\r\n", _filePaths);

                // create List to hold all files to share
                var filesToShare = new List<IStorageItem>();

                for (var i = 0; i < _filePaths.Count; i++)
                {
                    var imageFile = await StorageFile.GetFileFromPathAsync(_filePaths[i]);
                    filesToShare.Add(imageFile);
                }

                dp.SetStorageItems(filesToShare);
            }
            catch (Exception ex)
            {
                error = ex;

                // replaces the generic "Try that again" text with the real reason
                e.Request.FailWithDisplayText(ex.Message);
            }
            finally
            {
                // release the Share UI before any dialog, or it waits on us and times out
                deferral.Complete();
            }
        }
        catch (Exception ex)
        {
            error ??= ex;
        }

        if (error is not null) ShowShareError(error);
    }


    /// <summary>
    /// Shows the share failure, marshalled to the UI thread since the data request may not run on it.
    /// </summary>
    private static void ShowShareError(Exception ex)
    {
        // the caller is an async void, so the reporter itself must not throw
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                _ = ModalWindow.ShowErrorAsync(null, new ModalWindowOptions
                {
                    Title = Core.Lang[LangId.Menu_MnuShare],
                    Heading = Core.Lang[LangId.Menu_MnuShare_Error],
                    Description = ex.Message,
                    Details = BHelper.GetExceptionDetails(ex),
                });
            });
        }
        catch { }
    }

}

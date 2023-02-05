/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Base;
using ImageGlass.Settings;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace igcmd10;

public class WinShare
{
    // declare datapackage
    private static DataPackage? _dp;
    private static readonly List<string> _filenames = new();

    public static bool IsShareShown = false;


    /// <summary>
    /// Show Share dialog
    /// </summary>
    /// <param name="windowHandle"></param>
    /// <param name="filenames"></param>
    public static void ShowShare(IntPtr windowHandle, string[] filenames)
    {
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


    private static async void Dtm_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
    {
        if (_filenames.Count == 0) return;
        var deferral = e.Request.GetDeferral();

        // create datapackage
        _dp = e.Request.Data;

        // create List to hold all files to share
        var filesToShare = new List<IStorageItem>();


        // Set properties of shareUI
        _dp.Properties.Title = $"ImageGlass {App.Version}";

        try
        {
            if (_filenames.Count == 1)
            {
                // only 1 photo is being shared
                _dp.Properties.Description = _filenames[0];
            }
            else
            {
                _dp.Properties.Description = string.Join("\r\n", _filenames);
            }

            for (var i = 0; i < _filenames.Count; i++)
            {
                var imageFile = await StorageFile.GetFileFromPathAsync(_filenames[i]);
                filesToShare.Add(imageFile);
            }

            _dp.SetStorageItems(filesToShare);
        }
        catch (Exception ex)
        {
            Config.ShowError(null, ex.Message);
        }
        finally
        {
            IsShareShown = true;
            deferral.Complete();
        }
    }
}


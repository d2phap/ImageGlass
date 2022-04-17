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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Library.WinAPI;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using System.Diagnostics;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.IGMethods contains methods for dynamic binding *
 * ****************************************************** */

public partial class FrmMain
{
    /// <summary>
    /// Open an image from file picker
    /// </summary>
    /// <returns></returns>
    private void IG_OpenFile()
    {
        OpenFilePicker();
    }

    private void IG_Refresh()
    {
        PicMain.Refresh();
    }

    private void IG_Reload()
    {
        _ = ViewNextCancellableAsync(0, isSkipCache: true);
    }

    private void IG_ReloadList()
    {
        _ = LoadImageListAsync(Local.Images.DistinctDirs, Local.Images.GetFileName(Local.CurrentIndex));
    }

    private void IG_ViewPreviousImage()
    {
        _ = ViewNextCancellableAsync(-1);
    }

    private void IG_ViewNextImage()
    {
        _ = ViewNextCancellableAsync(1);
    }

    private void IG_GoTo(int index)
    {
        GoToImageAsync(index);
    }

    private void IG_GoToFirst()
    {
        GoToImageAsync(0);
    }

    private void IG_GoToLast()
    {
        GoToImageAsync(Local.Images.Length - 1);
    }

    private void IG_ZoomIn()
    {
        PicMain.ZoomIn();
    }

    private void IG_ZoomOut()
    {
        PicMain.ZoomOut();
    }

    private void IG_SetZoom(float factor)
    {
        PicMain.ZoomFactor = factor;
    }

    private void IG_SetZoomMode(string mode)
    {
        Config.ZoomMode = Helpers.ParseEnum<ZoomMode>(mode);

        if (PicMain.ZoomMode == Config.ZoomMode)
        {
            PicMain.Refresh();
        }
        else
        {
            PicMain.ZoomMode = Config.ZoomMode;
        }

        // update menu items state
        MnuAutoZoom.Checked = Config.ZoomMode == ZoomMode.AutoZoom;
        MnuLockZoom.Checked = Config.ZoomMode == ZoomMode.LockZoom;
        MnuScaleToWidth.Checked = Config.ZoomMode == ZoomMode.ScaleToWidth;
        MnuScaleToHeight.Checked = Config.ZoomMode == ZoomMode.ScaleToHeight;
        MnuScaleToFill.Checked = Config.ZoomMode == ZoomMode.ScaleToFill;
        MnuScaleToFit.Checked = Config.ZoomMode == ZoomMode.ScaleToFit;

        // update toolbar items state
        UpdateToolbarItemsState();
    }


    private bool IG_ToggleToolbar()
    {
        Config.IsShowToolbar = !Config.IsShowToolbar;

        // Gallery bar
        Toolbar.Visible = Config.IsShowToolbar;

        return Config.IsShowToolbar;
    }


    private bool IG_ToggleGallery()
    {
        Config.IsShowThumbnail = !Config.IsShowThumbnail;

        // Gallery bar
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;

        return Config.IsShowCheckerBoard;
    }


    private bool IG_ToggleCheckerboard()
    {
        Config.IsShowCheckerBoard = !Config.IsShowCheckerBoard;

        if (Config.IsShowCheckerBoard)
        {
            if (Config.IsShowCheckerboardOnlyImageRegion)
            {
                PicMain.CheckerboardMode = CheckerboardMode.Image;
            }
            else
            {
                PicMain.CheckerboardMode = CheckerboardMode.Client;
            }
        }
        else
        {
            PicMain.CheckerboardMode = CheckerboardMode.None;
        }

        return Config.IsShowCheckerBoard;
    }


    private bool IG_ToggleTopMost()
    {
        Config.IsWindowAlwaysOnTop = !Config.IsWindowAlwaysOnTop;

        // Gallery bar
        TopMost = Config.IsWindowAlwaysOnTop;

        return Config.IsWindowAlwaysOnTop;
    }

    private void IG_ReportIssue()
    {
        try
        {
            // TODO:
            Process.Start("https://github.com/d2phap/ImageGlass/issues");
        }
        catch { }
    }

    private void IG_About()
    {
        var archInfo = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var appVersion = Application.ProductVersion.ToString() + $" ({archInfo})";

        var btnDonate = new TaskDialogButton("Donate", allowCloseDialog: false);
        var btnClose = new TaskDialogButton("Close", allowCloseDialog: true);

        btnDonate.Click += (object? sender, EventArgs e) =>
        {
            try
            {
                // TODO:
                Process.Start("https://imageglass.org/source#donation?utm_source=app_" + App.Version + "&utm_medium=app_click&utm_campaign=app_donation");
            }
            catch { }
        };


        TaskDialog.ShowDialog(new()
        {
            Icon = TaskDialogIcon.Information,
            Caption = $"About {Application.ProductName}",

            Heading = $"Version: {appVersion}",
            Text = $"Copyright © 2010-{DateTime.Now.Year} by Dương Diệu Pháp.\r\n" +
                $"All rights reserved.\r\n\r\n" +
                $"Homepage: https://imageglass.org\r\n" +
                $"GitHub: https://github.com/d2phap/ImageGlass" +
                $"",

            Buttons = new TaskDialogButtonCollection { btnDonate, btnClose },
        });
    }

    private void IG_Settings()
    {
        var path = App.ConfigDir(PathType.File, Source.UserFilename);
        var psi = new ProcessStartInfo(path)
        {
            UseShellExecute = true,
        };

        Process.Start(psi);
    }

    private void IG_Exit()
    {
        Application.Exit();
    }


    private void IG_Print()
    {
        // image error
        if (PicMain.Source == ImageSource.Null)
        {
            return;
        }

        var currentFile = Local.Images.GetFileName(Local.CurrentIndex);
        var fileToPrint = currentFile;

        if (Local.IsTempMemoryData || Local.Metadata?.FramesCount == 1)
        {
            // TODO: // save image to temp file
            //fileToPrint = SaveTemporaryMemoryData();
        }
        // rename ext FAX -> TIFF to multipage printing
        else if (Path.GetExtension(currentFile).Equals(".FAX", StringComparison.OrdinalIgnoreCase))
        {
            fileToPrint = App.ConfigDir(PathType.File, Dir.Temporary, Path.GetFileNameWithoutExtension(currentFile) + ".tiff");
            File.Copy(currentFile, fileToPrint, true);
        }

        PrintService.OpenPrintPictures(fileToPrint);

        // TODO:
        //try
        //{
        //    PrintService.OpenPrintPictures(fileToPrint);
        //}
        //catch
        //{
        //    fileToPrint = SaveTemporaryMemoryData();
        //    PrintService.OpenPrintPictures(fileToPrint);
        //}
    }

}


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
using ImageGlass.Base.WinApi;
using ImageGlass.Library.WinAPI;
using ImageGlass.PhotoBox;
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Collections.Specialized;
using System.Diagnostics;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.IGMethods contains methods for dynamic binding *
 * ****************************************************** */

public partial class FrmMain
{
    /// <summary>
    /// Opens file picker to choose an image
    /// </summary>
    /// <returns></returns>
    private void IG_OpenFile()
    {
        OpenFilePicker();
    }


    /// <summary>
    /// Refreshes image viewport.
    /// </summary>
    private void IG_Refresh()
    {
        PicMain.Refresh();
    }


    /// <summary>
    /// Reloads image file.
    /// </summary>
    private void IG_Reload()
    {
        _ = ViewNextCancellableAsync(0, isSkipCache: true);
    }


    /// <summary>
    /// Reloads images list
    /// </summary>
    private void IG_ReloadList()
    {
        _ = LoadImageListAsync(Local.Images.DistinctDirs, Local.Images.GetFileName(Local.CurrentIndex));
    }


    /// <summary>
    /// Views previous image
    /// </summary>
    private void IG_ViewPreviousImage()
    {
        _ = ViewNextCancellableAsync(-1);
    }


    /// <summary>
    /// View next image
    /// </summary>
    private void IG_ViewNextImage()
    {
        _ = ViewNextCancellableAsync(1);
    }


    /// <summary>
    /// Views an image by its index
    /// </summary>
    /// <param name="index"></param>
    private void IG_GoTo(int index)
    {
        GoToImage(index);
    }


    /// <summary>
    /// Views the first image in the list
    /// </summary>
    private void IG_GoToFirst()
    {
        GoToImage(0);
    }


    /// <summary>
    /// Views the last image in the list
    /// </summary>
    private void IG_GoToLast()
    {
        GoToImage(Local.Images.Length - 1);
    }


    /// <summary>
    /// Zooms into the image
    /// </summary>
    private void IG_ZoomIn()
    {
        PicMain.ZoomIn();
    }


    /// <summary>
    /// Zooms out of the image
    /// </summary>
    private void IG_ZoomOut()
    {
        PicMain.ZoomOut();
    }


    /// <summary>
    /// Zoom the image by a custom value
    /// </summary>
    /// <param name="factor"></param>
    private void IG_SetZoom(float factor)
    {
        PicMain.ZoomFactor = factor;
    }


    /// <summary>
    /// Sets the zoom mode value
    /// </summary>
    /// <param name="mode"><see cref="ZoomMode"/> value in string</param>
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


    /// <summary>
    /// Toggles <see cref="Toolbar"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleToolbar(bool? visible = null)
    {
        visible ??= !Config.ShowToolbar;
        Config.ShowToolbar = visible.Value;

        // Gallery bar
        Toolbar.Visible = Config.ShowToolbar;

        // update menu item state
        MnuToggleToolbar.Checked = Config.ShowToolbar;

        // update toolbar items state
        UpdateToolbarItemsState();

        return Config.ShowToolbar;
    }


    /// <summary>
    /// Toggles <see cref="Gallery"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleGallery(bool? visible = null)
    {
        visible ??= !Config.ShowThumbnails;
        Config.ShowThumbnails = visible.Value;

        var scrollBarSize = 0;
        Gallery.ScrollBars = Config.ShowThumbnailScrollbars;

        if (Config.ShowThumbnailScrollbars)
        {
            Gallery.HScrollBar.Height = Gallery.VScrollBar.Width = DpiApi.Transform(8);
            scrollBarSize = Gallery.HScrollBar.Height + 1;
        }

        // Gallery bar
        Sp1.Panel2Collapsed = !Config.ShowThumbnails;
        Sp1.SplitterDistance = Sp1.Height
            - Sp1.SplitterWidth
            - Config.ThumbnailSize
            - scrollBarSize
            - 30;

        // update menu item state
        MnuToggleThumbnails.Checked = Config.ShowThumbnails;

        // update toolbar items state
        UpdateToolbarItemsState();

        return Config.ShowThumbnails;
    }


    /// <summary>
    /// Toggles checkerboard background visibility
    /// </summary>
    /// <param name="visible"></param>
    /// <returns></returns>
    private bool IG_ToggleCheckerboard(bool? visible = null)
    {
        visible ??= !Config.ShowCheckerBoard;
        Config.ShowCheckerBoard = visible.Value;

        if (visible.Value)
        {
            if (Config.ShowCheckerboardOnlyImageRegion)
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

        // update menu item state
        MnuToggleCheckerboard.Checked = visible.Value;

        // update toolbar items state
        UpdateToolbarItemsState();


        return Config.ShowCheckerBoard;
    }


    /// <summary>
    /// Toggles form top most
    /// </summary>
    /// <param name="enableTopMost"></param>
    /// <returns></returns>
    private bool IG_ToggleTopMost(bool? enableTopMost = null)
    {
        enableTopMost ??= !Config.EnableWindowAlwaysOnTop;
        Config.EnableWindowAlwaysOnTop = enableTopMost.Value;

        // Gallery bar
        TopMost = Config.EnableWindowAlwaysOnTop;

        // update menu item state
        MnuToggleTopMost.Checked = TopMost;

        return Config.EnableWindowAlwaysOnTop;
    }


    /// <summary>
    /// Opens project site to report issue
    /// </summary>
    private void IG_ReportIssue()
    {
        Helpers.OpenUrl("https://github.com/d2phap/ImageGlass/issues?q=is%3Aissue+label%3Av9+", "app_report_issue");
    }

    /// <summary>
    /// Open About dialog
    /// </summary>
    private void IG_About()
    {
        var archInfo = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var appVersion = App.Version + $" beta 1 ({archInfo})";

        var btnDonate = new TaskDialogButton("Donate", allowCloseDialog: false);
        var btnClose = new TaskDialogButton("Close", allowCloseDialog: true);

        btnDonate.Click += (object? sender, EventArgs e) =>
        {
            Helpers.OpenUrl("https://imageglass.org/source#donation", "app_donation");
        };

        TaskDialog.ShowDialog(new()
        {
            Icon = new TaskDialogIcon(Icon),
            Caption = $"About",

            Heading = $"{Application.ProductName}\r\n" +
                $"Version: {appVersion}",
            Text = $"Copyright © 2010-{DateTime.Now.Year} by Dương Diệu Pháp.\r\n" +
                $"All rights reserved.\r\n\r\n" +
                $"Homepage: https://imageglass.org\r\n" +
                $"GitHub: https://github.com/d2phap/ImageGlass",

            Buttons = new TaskDialogButtonCollection { btnDonate, btnClose },
            AllowCancel = true,
        });
    }


    /// <summary>
    /// Check for updates
    /// </summary>
    /// <param name="showNewUpdate"></param>
    private void IG_CheckForUpdate(bool? showNewUpdate = null)
    {
        Program.CheckForUpdate(showNewUpdate);
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


    /// <summary>
    /// Prints the viewing image data
    /// </summary>
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


    /// <summary>
    /// Copy multiple files
    /// </summary>
    private void IG_CopyMultiFiles()
    {
        // get filename
        var filename = Local.Images.GetFileName(Local.CurrentIndex);

        if (Local.IsImageError || !File.Exists(filename))
        {
            return;
        }

        // update the list
        var fileList = new List<string>();
        fileList.AddRange(Local.StringClipboard);

        for (var i = 0; i < fileList.Count; i++)
        {
            if (!File.Exists(fileList[i]))
            {
                Local.StringClipboard.Remove(fileList[i]);
            }
        }

        // exit if duplicated filename
        if (Local.StringClipboard.IndexOf(filename) == -1)
        {
            // add filename to clipboard
            Local.StringClipboard.Add(filename);
        }

        var fileDropList = new StringCollection();
        fileDropList.AddRange(Local.StringClipboard.ToArray());

        Clipboard.Clear();
        Clipboard.SetFileDropList(fileDropList);

        PicMain.ShowMessage(
            string.Format(Config.Language[$"{Name}._CopyFileText"], Local.StringClipboard.Count),
            Config.InAppMessageDuration);
    }


    /// <summary>
    /// Cut multiple files
    /// </summary>
    private async void IG_CutMultiFiles()
    {
        // get filename
        var filename = Local.Images.GetFileName(Local.CurrentIndex);

        if (Local.IsImageError || !File.Exists(filename))
        {
            return;
        }

        // update the list
        var fileList = new List<string>();
        fileList.AddRange(Local.StringClipboard);

        Parallel.ForEach(fileList, f =>
        {
            if (!File.Exists(f))
            {
                Local.StringClipboard.Remove(f);
            }
        });

        // exit if duplicated filename
        if (Local.StringClipboard.IndexOf(filename) == -1)
        {
            // add filename to clipboard
            Local.StringClipboard.Add(filename);
        }

        var moveEffect = new byte[] { 2, 0, 0, 0 };
        using (var dropEffect = new MemoryStream())
        {
            await dropEffect.WriteAsync(moveEffect).ConfigureAwait(true);

            var fileDropList = new StringCollection();
            fileDropList.AddRange(Local.StringClipboard.ToArray());

            var data = new DataObject();
            data.SetFileDropList(fileDropList);
            data.SetData("Preferred DropEffect", dropEffect);

            Clipboard.Clear();
            Clipboard.SetDataObject(data, true);
        }

        PicMain.ShowMessage(
            string.Format(Config.Language[$"{Name}._CutFileText"], Local.StringClipboard.Count),
            Config.InAppMessageDuration);
    }


    /// <summary>
    /// Copies the ucrrent image path
    /// </summary>
    private void IG_CopyImagePath()
    {
        try
        {
            Clipboard.SetText(Local.Images.GetFileName(Local.CurrentIndex));

            PicMain.ShowMessage(Config.Language[$"{Name}._ImagePathCopied"], Config.InAppMessageDuration);
        }
        catch { }
    }


    /// <summary>
    /// Clears clipboard
    /// </summary>
    private void IG_ClearClipboard()
    {
        // clear copied files in clipboard
        if (Local.StringClipboard.Count > 0)
        {
            Local.StringClipboard = new();
            Clipboard.Clear();
        }

        PicMain.ShowMessage(Config.Language[$"{Name}._ClearClipboard"], Config.InAppMessageDuration);
    }


    /// <summary>
    /// Copies image data to clipboard
    /// </summary>
    private async void IG_CopyImageData()
    {
        var img = await Local.Images.GetAsync(Local.CurrentIndex);
        if (img != null)
        {
            Clipboard.SetImage(img.ImgData.Image);
            PicMain.ShowMessage(Config.Language[$"{Name}._CopyImageData"], Config.InAppMessageDuration);
        }
    }


    /// <summary>
    /// Open the current image's location
    /// </summary>
    private void IG_OpenLocation()
    {
        var filePath = Local.Images.GetFileName(Local.CurrentIndex);

        try
        {
            ExplorerApi.OpenFolderAndSelectItem(filePath);
        }
        catch
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
    }


    /// <summary>
    /// Opens image file properties dialog
    /// </summary>
    private void IG_OpenProperties()
    {
        var filePath = Local.Images.GetFileName(Local.CurrentIndex);
        ExplorerApi.DisplayFileProperties(filePath, Handle);
    }


    /// <summary>
    /// Shows OpenWith dialog
    /// </summary>
    private void IG_OpenWith()
    {
        using var p = new Process();
        p.StartInfo.FileName = "openwith";

        // Build the arguments
        var filePath = Local.Images.GetFileName(Local.CurrentIndex);
        p.StartInfo.Arguments = $"\"{filePath}\"";

        // show error dialog
        p.StartInfo.ErrorDialog = true;

        try
        {
            p.Start();
        }
        catch { }
    }


    /// <summary>
    /// Toggles image focus mode
    /// </summary>
    /// <param name="enable"></param>
    /// <returns></returns>
    private bool IG_ToggleImageFocus(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableImageFocus;
        Config.EnableImageFocus = enable.Value;

        if (enable.Value)
        {
            PicMain.AllowInternalPanningKeys = true;
            PicMain.AllowInternalZoomingKeys = true;
            PicMain.TabStop = true;
            PicMain.Focus();
        }
        else
        {
            PicMain.AllowInternalPanningKeys = false;
            PicMain.AllowInternalZoomingKeys = false;
            PicMain.TabStop = false;
            PicMain.Enabled = false;
            Focus();
            PicMain.Enabled = true;
        }

        // update menu item state
        MnuToggleImageFocus.Checked = enable.Value;

        // update toolbar items state
        UpdateToolbarItemsState();

        if (showInAppMessage)
        {
            var msgKey = Config.EnableImageFocus ? "_Enable" : "_Disable";
            PicMain.ShowMessage(Config.Language[$"{Name}.{nameof(MnuToggleImageFocus)}.{msgKey}"],
                Config.InAppMessageDuration);
        }

        return Config.EnableImageFocus;
    }

}


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
using DirectN;
using ImageGlass.Base;
using ImageGlass.Base.NamedPipes;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Library.WinAPI;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.Views;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Windows.Media.Imaging;
using WicNet;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.IGMethods contains methods for dynamic binding *
 * ****************************************************** */

public partial class FrmMain
{
    // to delay the cleaning task when disconnecting multiple slideshows
    private CancellationTokenSource _cleanSlideshowServerCancelToken = new();


    /// <summary>
    /// Opens file picker to choose an image
    /// </summary>
    /// <returns></returns>
    public void IG_OpenFile()
    {
        OpenFilePicker();
    }


    /// <summary>
    /// Open file picker and load the selected image
    /// </summary>
    public void OpenFilePicker()
    {
        var formats = Config.GetImageFormats(Config.AllFormats);
        using var o = new OpenFileDialog()
        {
            Filter = Config.Language[$"{Name}._OpenFileDialog"] + "|" + formats,
            CheckFileExists = true,
            RestoreDirectory = true,
        };

        if (o.ShowDialog() == DialogResult.OK)
        {
            PrepareLoading(o.FileName);
        }
    }



    /// <summary>
    /// Refreshes image viewport.
    /// </summary>
    public void IG_Refresh()
    {
        PicMain.Refresh();
    }


    /// <summary>
    /// Reloads image file.
    /// </summary>
    public void IG_Reload()
    {
        _ = ViewNextCancellableAsync(0, isSkipCache: true);
    }


    /// <summary>
    /// Reloads images list
    /// </summary>
    public void IG_ReloadList()
    {
        _ = LoadImageListAsync(Local.Images.DistinctDirs, Local.Images.GetFilePath(Local.CurrentIndex));
    }


    /// <summary>
    /// Views previous image
    /// </summary>
    public void IG_ViewPreviousImage()
    {
        _ = ViewNextCancellableAsync(-1);
    }


    /// <summary>
    /// View next image
    /// </summary>
    public void IG_ViewNextImage()
    {
        _ = ViewNextCancellableAsync(1);
    }


    /// <summary>
    /// Views an image by its index
    /// </summary>
    public void IG_GoTo()
    {
        if (Local.Images.Length == 0) return;

        var oldIndex = Local.CurrentIndex + 1;
        using var frm = new Popup(Config.Theme, Config.Language)
        {
            Title = Config.Language[$"{Name}.{nameof(MnuGoTo)}"],
            Value = oldIndex.ToString(),

            UnsignedIntValueOnly = true,
            TopMost = TopMost,

            Description = Config.Language[$"{Name}.{nameof(MnuGoTo)}._Description"],
        };


        if (frm.ShowDialog(this) != DialogResult.OK) return;

        if (int.TryParse(frm.Value.Trim(), out var newIndex))
        {
            newIndex--;

            if (newIndex != Local.CurrentIndex
                && 0 <= newIndex && newIndex < Local.Images.Length)
            {
                GoToImage(newIndex);
            }
        }
    }


    /// <summary>
    /// View image using index
    /// </summary>
    /// <param name="index">Image index</param>
    public void GoToImage(int index)
    {
        Local.CurrentIndex = index;
        _ = ViewNextCancellableAsync(0);
    }


    /// <summary>
    /// Views the first image in the list
    /// </summary>
    public void IG_GoToFirst()
    {
        GoToImage(0);
    }


    /// <summary>
    /// Views the last image in the list
    /// </summary>
    public void IG_GoToLast()
    {
        GoToImage(Local.Images.Length - 1);
    }


    // Panning ------------------------------------
    #region Panning

    /// <summary>
    /// Pans the viewing image to left
    /// </summary>
    public void IG_PanLeft()
    {
        _ = PanLeftAsync();
    }

    public async Task PanLeftAsync()
    {
        PicMain.StartAnimation(AnimationSource.PanLeft);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanLeft);
    }


    /// <summary>
    /// Pans the viewing image to right
    /// </summary>
    public void IG_PanRight()
    {
        _ = PanRightAsync();
    }

    public async Task PanRightAsync()
    {
        PicMain.StartAnimation(AnimationSource.PanRight);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanRight);
    }


    /// <summary>
    /// Pans the viewing image up.
    /// </summary>
    public void IG_PanUp()
    {
        _ = PanUpAsync();
    }

    public async Task PanUpAsync()
    {
        PicMain.StartAnimation(AnimationSource.PanUp);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanUp);
    }


    /// <summary>
    /// Pans the viewing image down.
    /// </summary>
    public void IG_PanDown()
    {
        _ = PanDownAsync();
    }

    public async Task PanDownAsync()
    {
        PicMain.StartAnimation(AnimationSource.PanDown);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanDown);
    }


    /// <summary>
    /// Pans the viewing image to left side
    /// </summary>
    public void IG_PanToLeftSide()
    {
        _ = PanToLeftSideAsync();
    }

    public async Task PanToLeftSideAsync()
    {
        var distanceX = PicMain.ImageSourceBounds.X;

        PicMain.PanDistance = distanceX * PicMain.ZoomFactor / 10;
        PicMain.StartAnimation(AnimationSource.PanLeft);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanLeft);
        PicMain.PanDistance = Config.PanSpeed;
    }


    /// <summary>
    /// Pans the viewing image to right side
    /// </summary>
    public void IG_PanToRightSide()
    {
        _ = PanToRightSideAsync();
    }

    public async Task PanToRightSideAsync()
    {
        var x = PicMain.SourceWidth - PicMain.ImageSourceBounds.Width;
        var distanceX = x + PicMain.ImageSourceBounds.X;

        PicMain.PanDistance = distanceX * PicMain.ZoomFactor / 10;
        PicMain.StartAnimation(AnimationSource.PanRight);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanRight);
        PicMain.PanDistance = Config.PanSpeed;
    }


    /// <summary>
    /// Pans the viewing image to top side
    /// </summary>
    public void IG_PanToTopSide()
    {
        _ = PanToTopSideAsync();
    }

    public async Task PanToTopSideAsync()
    {
        var distanceY = PicMain.ImageSourceBounds.Y;

        PicMain.PanDistance = distanceY * PicMain.ZoomFactor / 10;
        PicMain.StartAnimation(AnimationSource.PanUp);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanUp);
        PicMain.PanDistance = Config.PanSpeed;
    }


    /// <summary>
    /// Pans the viewing image to bottom side
    /// </summary>
    public void IG_PanToBottomSide()
    {
        _ = PanToBottomSideAsync();
    }

    public async Task PanToBottomSideAsync()
    {
        var y = PicMain.SourceHeight - PicMain.ImageSourceBounds.Height;
        var distanceY = y + PicMain.ImageSourceBounds.Y;

        PicMain.PanDistance = distanceY * PicMain.ZoomFactor / 10;
        PicMain.StartAnimation(AnimationSource.PanDown);
        await Task.Delay(200);

        PicMain.StopAnimation(AnimationSource.PanDown);
        PicMain.PanDistance = Config.PanSpeed;
    }

    #endregion // Panning


    // Zooming ------------------------------------
    #region Zooming

    /// <summary>
    /// Zooms into the image
    /// </summary>
    public void IG_ZoomIn()
    {
        _ = ZoomInAsync();
    }

    public async Task ZoomInAsync()
    {
        PicMain.StartAnimation(AnimationSource.ZoomIn);
        await Task.Delay(100);

        PicMain.StopAnimation(AnimationSource.ZoomIn);
    }


    /// <summary>
    /// Zooms out of the image
    /// </summary>
    public void IG_ZoomOut()
    {
        _ = ZoomOutAsync();
    }

    public async Task ZoomOutAsync()
    {
        PicMain.StartAnimation(AnimationSource.ZoomOut);
        await Task.Delay(100);

        PicMain.StopAnimation(AnimationSource.ZoomOut);
    }


    /// <summary>
    /// Shows Input dialog for custom zoom
    /// </summary>
    public void IG_CustomZoom()
    {
        if (PicMain.Source == ImageSource.Null) return;

        var oldZoom = PicMain.ZoomFactor * 100f;
        using var frm = new Popup(Config.Theme, Config.Language)
        {
            Title = Config.Language[$"{Name}.{nameof(MnuCustomZoom)}"],
            Value = oldZoom.ToString(),
            Thumbnail = SystemIconApi.GetSystemIcon(SHSTOCKICONID.SIID_FIND),

            UnsignedFloatValueOnly = true,
            TopMost = TopMost,

            Description = Config.Language[$"{Name}.{nameof(MnuCustomZoom)}._Description"],
        };


        if (frm.ShowDialog(this) != DialogResult.OK) return;

        if (int.TryParse(frm.Value.Trim(), out var newZoom))
        {
            PicMain.ZoomFactor = newZoom / 100f;
        }
    }


    /// <summary>
    /// Zoom to the current cursor location by the given factor.
    /// </summary>
    /// <param name="factor"></param>
    public void IG_SetZoom(float factor)
    {
        var point = PicMain.PointToClient(Cursor.Position);

        PicMain.ZoomToPoint(factor, point);
    }


    /// <summary>
    /// Sets the zoom mode value
    /// </summary>
    /// <param name="mode"><see cref="ZoomMode"/> value in string</param>
    public void IG_SetZoomMode(string mode)
    {
        Config.ZoomMode = BHelper.ParseEnum<ZoomMode>(mode);

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

    #endregion // Zooming


    /// <summary>
    /// Toggles <see cref="Toolbar"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    public bool IG_ToggleToolbar(bool? visible = null)
    {
        visible ??= !Config.ShowToolbar;
        Config.ShowToolbar = visible.Value;

        // Toolbar
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
    public bool IG_ToggleGallery(bool? visible = null)
    {
        visible ??= !Config.ShowThumbnails;
        Config.ShowThumbnails = visible.Value;

        var scrollBarSize = 0;
        Gallery.ScrollBars = Config.ShowThumbnailScrollbars;
        Gallery.ShowItemText = Config.ShowThumbnailFilename;

        if (Config.ShowThumbnailScrollbars)
        {
            Gallery.HScrollBar.Height = Gallery.VScrollBar.Width = DpiApi.Transform(8);
            scrollBarSize = Gallery.HScrollBar.Height + 1;
        }

        // Gallery bar
        Gallery.Height = Config.ThumbnailSize + scrollBarSize + 30;
        Sp1.Panel2Collapsed = !Config.ShowThumbnails;
        Sp1.SplitterDistance = Sp1.Height
            - Sp1.SplitterWidth
            - Gallery.Height;

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
    public bool IG_ToggleCheckerboard(bool? visible = null)
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
    public bool IG_ToggleTopMost(bool? enableTopMost = null, bool showInAppMessage = true)
    {
        enableTopMost ??= !Config.EnableWindowTopMost;
        Config.EnableWindowTopMost = enableTopMost.Value;

        TopMost = Config.EnableWindowTopMost;

        // update menu item state
        MnuToggleTopMost.Checked = TopMost;

        if (showInAppMessage)
        {
            var msgKey = Config.EnableWindowTopMost ? "_Enable" : "_Disable";
            PicMain.ShowMessage(Config.Language[$"{Name}.{nameof(MnuToggleTopMost)}.{msgKey}"], Config.InAppMessageDuration);
        }

        return Config.EnableWindowTopMost;
    }


    /// <summary>
    /// Opens project site to report issue
    /// </summary>
    public void IG_ReportIssue()
    {
        BHelper.OpenUrl("https://github.com/d2phap/ImageGlass/issues?q=is%3Aissue+label%3Av9+", "app_report_issue");
    }

    /// <summary>
    /// Open About dialog
    /// </summary>
    public void IG_About()
    {
        var archInfo = Environment.Is64BitProcess ? "64-bit" : "32-bit";
        var appVersion = App.Version + $" ({archInfo})";

        var btnDonate = new TaskDialogButton("Donate", allowCloseDialog: false);
        var btnCheckForUpdate = new TaskDialogButton("Check for update", allowCloseDialog: false);
        var btnClose = new TaskDialogButton("Close", allowCloseDialog: true);

        btnDonate.Click += (object? sender, EventArgs e) =>
        {
            BHelper.OpenUrl("https://imageglass.org/source#donation", "app_donation");
        };

        btnCheckForUpdate.Click += (_, _) => IG_CheckForUpdate(true);

        _ = TaskDialog.ShowDialog(new()
        {
            Icon = new TaskDialogIcon(Icon),
            Buttons = new TaskDialogButtonCollection {
                btnDonate, btnCheckForUpdate, btnClose
            },
            SizeToContent = true,
            AllowCancel = true,
            Caption = $"About",

            Heading = $"{App.AppName} beta 2\r\n" +
                $"A lightweight, versatile image viewer\r\n" +
                $"\r\n" +
                $"Version: {appVersion}\r\n" +
                $".NET Runtime: {Environment.Version.ToString()}",

            Text = $"Special thanks to:\r\n" +
                    $"◾ Logo designer: Nguyễn Quốc Tuấn.\r\n" +
                    $"◾ Collaborator: Kevin Routley (https://github.com/fire-eggs).\r\n" +
                    $"\r\n" +

                    $"Contact:\r\n" +
                    $"◾ Homepage: https://imageglass.org\r\n" +
                    $"◾ GitHub: https://github.com/d2phap/ImageGlass\r\n" +
                    $"◾ Email: phap@imageglass.org",

            Footnote = new()
            {
                Icon = TaskDialogIcon.ShieldSuccessGreenBar,
                Text = "" +
                    $"This software is released under the terms of the GNU General Public License v3.0.\r\n" +
                    $"Copyright © 2010-{DateTime.UtcNow.Year} by Dương Diệu Pháp. All rights reserved.",
            },

            Expander = new()
            {
                Position = TaskDialogExpanderPosition.AfterText,
                ExpandedButtonText = "Hide License and credits",
                CollapsedButtonText = "Show License and credits",
                Text = "\r\n" +
                    "LICENSE AND CREDITS\r\n" +
                    "\r\n" +
                    "◾ WicNet\r\n" +
                    "    Distributed under the terms of the MIT license.\r\n" +
                    "    Copyright © 2022 Simon Mourier. All rights reserved.\r\n" +
                    "\r\n" +

                    "◾ D2Phap.DXControl\r\n" +
                    "    Distributed under the terms of the MIT license.\r\n" +
                    "    Copyright © 2022 Dương Diệu Pháp. All rights reserved.\r\n" +
                    "\r\n" +

                    "◾ Magick.NET\r\n" +
                    "    Distributed under the terms of the MIT license.\r\n" +
                    "    Copyright © 2013-2022 Dirk Lemstra.\r\n" +
                    "\r\n" +

                    "◾ ExplorerSortOrder\r\n" +
                    "    Distributed under the terms of the Apache License 2.0.\r\n" +
                    "    Copyright © 2019 Kevin Routley.\r\n" +
                    "\r\n" +

                    "◾ ImageGlass.Gallery\r\n" +
                    "    This software uses code of ImageListView licensed under the Apache License 2.0.\r\n" +
                    "    Copyright © 2013 Özgür Özçıtak.\r\n" +

                    "\r\n" +
                    "◾ All Microsoft.Extensions libraries\r\n" +
                    "    Distributed under the terms of the MIT license.\r\n" +
                    "    Copyright © Microsoft Corporation. All rights reserved.",
            },
        });
    }


    /// <summary>
    /// Check for updates
    /// </summary>
    /// <param name="showNewUpdate"></param>
    public void IG_CheckForUpdate(bool? showNewUpdate = null)
    {
        Program.CheckForUpdate(showNewUpdate);
    }


    public void IG_Settings()
    {
        using var frmSettings = new FrmSettings()
        {
            CloseFormHotkey = Keys.Escape,
            StartPosition = FormStartPosition.CenterScreen,
        };

        frmSettings.ShowDialog();
    }


    public void IG_Exit()
    {
        Application.Exit();
    }


    /// <summary>
    /// Prints the viewing image data
    /// </summary>
    public void IG_Print()
    {
        _ = PrintAsync();
    }


    public async Task PrintAsync()
    {
        // image error
        if (PicMain.Source == ImageSource.Null)
        {
            return;
        }

        var currentFile = Local.Images.GetFilePath(Local.CurrentIndex);
        var fileToPrint = currentFile;
        var ext = Path.GetExtension(currentFile).ToUpperInvariant();
        var langPath = $"{Name}.{nameof(MnuPrint)}";

        PicMain.ShowMessage(Config.Language[$"{langPath}._CreatingFile"], "", delayMs: 500);


        // print clipboard image
        if (Local.ClipboardImage != null || Local.Metadata?.FramesCount == 1)
        {
            // save image to temp file
            fileToPrint = await Local.SaveImageAsTempFileAsync(quality: 100);
        }

        // print an image file
        // rename ext FAX -> TIFF to multipage printing
        else if (ext.Equals(".FAX", StringComparison.OrdinalIgnoreCase))
        {
            fileToPrint = App.ConfigDir(PathType.File, Dir.Temporary, Path.GetFileNameWithoutExtension(currentFile) + ".tiff");

            File.Copy(currentFile, fileToPrint, true);
        }
        else if (Local.Metadata?.FramesCount > 1
            && !ext.Equals(".GIF", StringComparison.OrdinalIgnoreCase)
            && !ext.Equals(".TIF", StringComparison.OrdinalIgnoreCase)
            && !ext.Equals(".TIFF", StringComparison.OrdinalIgnoreCase))
        {
            // save image to temp file
            fileToPrint = await Local.SaveImageAsTempFileAsync(quality: 100);
        }


        if (string.IsNullOrEmpty(fileToPrint))
        {
            _ = Config.ShowError(Config.Language[$"{langPath}._CreatingFileError"],
                Config.Language[langPath]);
        }
        else
        {
            try
            {
                PrintService.OpenPrintPictures(fileToPrint);
            }
            catch (Exception ex)
            {
                _ = Config.ShowError($"{ex.Source}:\r\n{ex.Message}", "",
                    Config.Language[$"{langPath}._Error"]);
            }
        }


        PicMain.ClearMessage();
    }


    public void IG_Share()
    {
        _ = ShowShareDialogAsync();
    }

    public async Task ShowShareDialogAsync()
    {
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var langPath = $"{Name}.{nameof(MnuShare)}";


        // print clipboard image
        if (Local.ClipboardImage != null)
        {
            PicMain.ShowMessage(Config.Language[$"{langPath}._CreatingFile"], "", delayMs: 500);

            // save image to temp file
            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }

        PicMain.ClearMessage();

        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(Config.Language[$"{langPath}._CreatingFileError"],
                Config.Language[langPath]);
        }
        else
        {
            var args = string.Format($"{IgCommands.SHARE} \"{filePath}\"");
            var result = await BHelper.RunIgcmd10(args);


            if (result == IgExitCode.Error)
            {
                _ = Config.ShowError(
                    description: Config.Language[$"{langPath}._Error"],
                    title: Config.Language[langPath],
                    heading: Config.Language["_._Error"]);
            }
        }
    }



    // Clipboard functions ------------------------------------
    #region Clipboard functions

    /// <summary>
    /// Copy single or multiple files
    /// </summary>
    public void IG_CopyFiles()
    {
        // get file path
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);

        if (Local.IsImageError || !File.Exists(filePath))
        {
            return;
        }

        var fileDropList = new StringCollection();

        if (Config.EnableCopyMultipleFiles)
        {
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
            if (Local.StringClipboard.IndexOf(filePath) == -1)
            {
                // add filename to clipboard
                Local.StringClipboard.Add(filePath);
            }

            fileDropList.AddRange(Local.StringClipboard.ToArray());
        }

        // single file copy
        else
        {
            fileDropList.Add(filePath);
        }

        Clipboard.Clear();
        Clipboard.SetFileDropList(fileDropList);


        PicMain.ShowMessage(
            string.Format(Config.Language[$"{Name}.{nameof(MnuCopy)}._Success"], Config.EnableCopyMultipleFiles ? Local.StringClipboard.Count : 1),
            Config.InAppMessageDuration);
    }


    /// <summary>
    /// Cut single or multiple filesp
    /// </summary>
    public void IG_CutFiles()
    {
        _ = CutFilesAsync();
    }

    public async Task CutFilesAsync()
    {
        // get file path
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);

        if (Local.IsImageError || !File.Exists(filePath))
        {
            return;
        }

        var fileDropList = new StringCollection();

        if (Config.EnableCutMultipleFiles)
        {
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
            if (Local.StringClipboard.IndexOf(filePath) == -1)
            {
                // add filename to clipboard
                Local.StringClipboard.Add(filePath);
            }

            fileDropList.AddRange(Local.StringClipboard.ToArray());
        }
        else
        {
            fileDropList.Add(filePath);
        }


        var moveEffect = new byte[] { 2, 0, 0, 0 };
        using var dropEffect = new MemoryStream();
        await dropEffect.WriteAsync(moveEffect).ConfigureAwait(true);

        var data = new DataObject();
        data.SetFileDropList(fileDropList);
        data.SetData("Preferred DropEffect", dropEffect);

        Clipboard.Clear();
        Clipboard.SetDataObject(data, true);


        PicMain.ShowMessage(
            string.Format(Config.Language[$"{Name}.{nameof(MnuCut)}._Success"], Config.EnableCutMultipleFiles ? Local.StringClipboard.Count : 1),
            Config.InAppMessageDuration);
    }


    /// <summary>
    /// Copies the ucrrent image path
    /// </summary>
    public void IG_CopyImagePath()
    {
        try
        {
            Clipboard.SetText(Local.Images.GetFilePath(Local.CurrentIndex));

            PicMain.ShowMessage(Config.Language[$"{Name}.{nameof(MnuCopyPath)}._Success"], Config.InAppMessageDuration);
        }
        catch { }
    }


    /// <summary>
    /// Clears clipboard
    /// </summary>
    public void IG_ClearClipboard()
    {
        // clear copied files in clipboard
        if (Local.StringClipboard.Count > 0)
        {
            Local.StringClipboard = new();
            Clipboard.Clear();
        }

        PicMain.ShowMessage(Config.Language[$"{Name}.{nameof(MnuClearClipboard)}._ClearClipboard"], Config.InAppMessageDuration);
    }


    /// <summary>
    /// Copies image data to clipboard
    /// </summary>
    public void IG_CopyImageData()
    {
        _ = CopyImageDataAsync();
    }


    public async Task CopyImageDataAsync()
    {
        if (PicMain.Source == ImageSource.Null) return;

        var bitmap = Local.ClipboardImage;
        if (bitmap == null)
        {
            var img = await Local.Images.GetAsync(Local.CurrentIndex);
            bitmap = img?.ImgData?.Image;
        }

        if (bitmap == null) return;

        var langPath = $"{Name}.{nameof(MnuCopyImageData)}";

        PicMain.ClearMessage();
        PicMain.ShowMessage(Config.Language[$"{langPath}._Copying"], "", delayMs: 1500);

        // copy the selected area
        if (!PicMain.SourceSelection.IsEmpty)
        {
            bitmap = BHelper.CropImage(bitmap, PicMain.SourceSelection);
        }

        await Task.Run(() => ClipboardEx.SetClipboardImage(bitmap));

        PicMain.ShowMessage(Config.Language[$"{langPath}._Success"], Config.InAppMessageDuration);
    }


    /// <summary>
    /// Pastes image from clipboard and opens it.
    /// </summary>
    public void IG_PasteImage()
    {
        // Is there a file in clipboard?
        if (Clipboard.ContainsFileDropList())
        {
            var sFile = Clipboard.GetData(DataFormats.FileDrop) as string[];

            // load file
            PrepareLoading(sFile[0]);
        }

        // Is there a image in clipboard?
        else if (ClipboardEx.ContainsImage())
        {
            var bmp = ClipboardEx.GetClipboardImage();

            LoadClipboardImage(bmp);
        }

        // Is there a filename in clipboard?
        else if (Clipboard.ContainsText())
        {
            // try to get absolute path
            var text = BHelper.ResolvePath(Clipboard.GetText());

            if (File.Exists(text) || Directory.Exists(text))
            {
                PrepareLoading(text);
            }
            // get image from Base64string 
            else
            {
                try
                {
                    var img = BHelper.ToWicBitmapSource(text);
                    LoadClipboardImage(img);
                }
                catch (Exception ex)
                {
                    var msg = Config.Language[$"{Name}.{nameof(MnuPasteImage)}._Error"];

                    PicMain.ShowMessage($"{ex.Source}: {ex.Message}", msg, Config.InAppMessageDuration * 2);
                }
            }
        }
    }

    public void LoadClipboardImage(WicBitmapSource? img)
    {
        // cancel the current loading image
        _loadCancelToken?.Cancel();

        Local.ClipboardImage?.Dispose();
        Local.ClipboardImage = img;
        Local.TempImagePath = null;

        PicMain.SetImage(new()
        {
            Image = img,
            FrameCount = 1,
            HasAlpha = true,
        });
        PicMain.ClearMessage();

        // reset zoom mode
        IG_SetZoomMode(Config.ZoomMode.ToString());

        UpdateImageInfo(ImageInfoUpdateTypes.All);
    }


    #endregion // Clipboard functions


    /// <summary>
    /// Open the current image's location
    /// </summary>
    public void IG_OpenLocation()
    {
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);

        try
        {
            ExplorerApi.OpenFolderAndSelectItem(filePath);
        }
        catch
        {
            using var proc = Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
    }


    /// <summary>
    /// Opens image file properties dialog
    /// </summary>
    public void IG_OpenProperties()
    {
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        ExplorerApi.DisplayFileProperties(filePath, Handle);
    }


    /// Saving ------------------------------------
    #region Saving

    /// <summary>
    /// Saves and overrides the current image.
    /// </summary>
    public void IG_Save()
    {
        if (Local.ClipboardImage != null)
        {
            IG_SaveAs();
            return;
        }

        var srcFilePath = Local.Images.GetFilePath(Local.CurrentIndex);
        Local.ImageModifiedPath = srcFilePath;

        _ = SaveImageAsync();
    }


    public async Task SaveImageAsync()
    {
        // use backup name to avoid variable conflict
        var filePath = Local.ImageModifiedPath;
        var langPath = $"{Name}.{nameof(MnuSave)}";


        // show override warning
        if (Config.ShowSaveOverrideConfirmation)
        {
            var result = Config.ShowWarning(
                description: filePath,
                note: Config.Language[$"{langPath}._ConfirmDescription"],
                title: Config.Language[langPath],
                heading: Config.Language[$"{langPath}._Confirm"],
                buttons: PopupButton.Yes_No,
                thumbnail: Gallery.Items[Local.CurrentIndex].ThumbnailImage,
                optionText: Config.Language["_._DoNotShowThisMessageAgain"],
                formOwner: this);

            // update ShowSaveOverrideConfirmation setting
            Config.ShowSaveOverrideConfirmation = !result.IsOptionChecked;

            if (result.ExitResult != PopupExitResult.OK) return;
        }


        PicMain.ShowMessage(filePath, Config.Language[$"{langPath}._Saving"]);

        var img = await Local.Images.GetAsync(Local.CurrentIndex);
        if (img?.ImgData?.Image == null)
        {
            Local.ImageModifiedPath = "";
            PicMain.ShowMessage("Image is null.", Config.Language[$"{langPath}._Error"], Config.InAppMessageDuration);
            return;
        }

        try
        {
            var lastWriteTime = File.GetLastWriteTime(filePath);

            await PhotoCodec.SaveAsync(filePath, filePath, Local.Images.ReadOptions, Local.CurrentChanges, Config.ImageEditQuality);

            // Issue #307: option to preserve the modified date/time
            if (Config.PreserveModifiedDate)
            {
                File.SetLastWriteTime(filePath, lastWriteTime);
            }

            // update cache of the modified item
            Gallery.Items[Local.CurrentIndex].UpdateDetails(true);

            PicMain.ShowMessage(filePath, Config.Language[$"{langPath}._Success"], Config.InAppMessageDuration);
        }
        catch (Exception ex)
        {
            PicMain.ClearMessage();
            _ = Config.ShowError(ex.Source + ":\r\n" + ex.Message,
                Config.Language[langPath],
                string.Format(Config.Language[$"{langPath}._Error"]), filePath);
        }


        Local.ImageModifiedPath = "";
    }


    /// <summary>
    /// Saves the viewing image as a new file.
    /// </summary>
    public void IG_SaveAs()
    {
        if (PicMain.Source == ImageSource.Null) return;

        var srcFilePath = "";
        var srcExt = ".png";

        if (Local.ClipboardImage == null)
        {
            srcFilePath = Local.Images.GetFilePath(Local.CurrentIndex);
            srcExt = Path.GetExtension(srcFilePath).ToLowerInvariant();

            if (string.IsNullOrEmpty(srcExt) || srcExt.Length < 2)
            {
                srcExt = ".png";
            }
        }

        using var saveDialog = new SaveFileDialog
        {
            Filter = SavingExts.GetFilterStringForSaveDialog(),
            FileName = string.IsNullOrEmpty(srcFilePath)
                ? $"untitle{srcExt}"
                : Path.GetFileNameWithoutExtension(srcFilePath),
            RestoreDirectory = true,
            SupportMultiDottedExtensions = true,
            Title = Config.Language[$"{Name}.{nameof(MnuSaveAs)}"],
            OverwritePrompt = !Config.ShowSaveOverrideConfirmation, // only show 1 prompt
        };


        // add custom places to the SaveFileDialog
        var dirPath = string.IsNullOrEmpty(srcFilePath)
            ? srcFilePath
            : Path.GetDirectoryName(srcFilePath);
        saveDialog.CustomPlaces.Add(dirPath);


        // Use the last-selected file extension, if available.
        var extIndex = !string.IsNullOrEmpty(Local.SaveAsFilterExt)
            ? SavingExts.IndexOf(Local.SaveAsFilterExt)
            : SavingExts.IndexOf(srcExt);

        saveDialog.FilterIndex = Math.Max(extIndex, 0) + 1;

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            var destExt = Path.GetExtension(saveDialog.FileName).ToLower();
            Local.SaveAsFilterExt = destExt;


            // show override warning
            if (File.Exists(saveDialog.FileName)
                && Config.ShowSaveOverrideConfirmation)
            {
                var langPath = $"{Name}.{nameof(MnuSave)}";

                var result = Config.ShowWarning(
                    description: srcFilePath,
                    note: Config.Language[$"{langPath}._ConfirmDescription"],
                    title: Config.Language[$"{Name}.{nameof(MnuSaveAs)}"],
                    heading: Config.Language[$"{langPath}._Confirm"],
                    buttons: PopupButton.Yes_No,
                    thumbnail: Gallery.Items[Local.CurrentIndex].ThumbnailImage,
                    optionText: Config.Language["_._DoNotShowThisMessageAgain"],
                    formOwner: this);

                // update ShowSaveOverrideConfirmation setting
                Config.ShowSaveOverrideConfirmation = !result.IsOptionChecked;

                if (result.ExitResult != PopupExitResult.OK)
                {
                    return;
                }
            }


            _ = SaveImageAsAsync(saveDialog.FileName, destExt, srcFilePath);
        }
    }


    /// <summary>
    /// Save image to file
    /// </summary>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="destExt">Destination file extension. E.g. ".png"</param>
    /// <param name="srcFilePath">Source file path</param>
    public async Task SaveImageAsAsync(string destFilePath, string destExt, string srcFilePath = "")
    {
        var hasSrcPath = !string.IsNullOrEmpty(srcFilePath);
        if (!hasSrcPath && Local.ClipboardImage == null)
        {
            return;
        }

        WicBitmapSource? clonedPic = null;
        IgPhoto? img = null;
        var srcExt = "";
        var langPath = $"{Name}.{nameof(MnuSave)}";

        PicMain.ShowMessage(destFilePath, Config.Language[$"{langPath}._Saving"]);

        // get the bitmap data from file
        if (hasSrcPath)
        {
            img = await Local.Images.GetAsync(Local.CurrentIndex);
            clonedPic = img?.ImgData?.Image?.Clone();

            srcExt = Path.GetExtension(srcFilePath).ToLowerInvariant();
        }
        // get bitmap from clipboard image
        else if (Local.ClipboardImage != null)
        {
            clonedPic = Local.ClipboardImage.Clone();
        }

        if (clonedPic == null)
        {
            PicMain.ShowMessage("Image is null.", Config.Language[$"{langPath}._Error"], Config.InAppMessageDuration);
            return;
        }

        try
        {
            // base64 format
            if (destExt == ".b64" || destExt == ".txt")
            {
                if (hasSrcPath)
                {
                    await PhotoCodec.SaveAsBase64Async(srcFilePath, destFilePath, Local.Images.ReadOptions, Local.CurrentChanges);
                }
                else if (Local.ClipboardImage != null)
                {
                    await PhotoCodec.SaveAsBase64Async(clonedPic, srcExt, destFilePath);
                }
            }
            // other formats
            else
            {
                if (hasSrcPath)
                {
                    await PhotoCodec.SaveAsync(srcFilePath, destFilePath, Local.Images.ReadOptions, Local.CurrentChanges, Config.ImageEditQuality);
                }
                else if (Local.ClipboardImage != null)
                {
                    await PhotoCodec.SaveAsync(clonedPic, destFilePath, Config.ImageEditQuality);
                }
            }

            PicMain.ShowMessage(destFilePath, Config.Language[$"{langPath}._Success"], Config.InAppMessageDuration);
        }
        catch (Exception ex)
        {
            PicMain.ClearMessage();
            _ = Config.ShowError(ex.Source + ":\r\n" + ex.Message,
                Config.Language[langPath],
                string.Format(Config.Language[$"{langPath}._Error"]), destFilePath);
        }
    }

    #endregion // Saving


    /// <summary>
    /// Shows OpenWith dialog
    /// </summary>
    public void IG_OpenWith()
    {
        _ = OpenWithAsync();
    }

    public async Task OpenWithAsync()
    {
        string? filePath;
        var langPath = $"{Name}.{nameof(MnuOpenWith)}";

        if (Local.ClipboardImage != null)
        {
            PicMain.ShowMessage(Config.Language[$"{langPath}._CreatingFile"], "", delayMs: 500);

            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }
        else
        {
            filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        }

        PicMain.ClearMessage();


        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(Config.Language[$"{langPath}._CreatingFileError"],
                Config.Language[langPath]);
        }
        else
        {
            using var p = new Process();
            p.StartInfo.FileName = "openwith";

            // Build the arguments
            p.StartInfo.Arguments = $"\"{filePath}\"";

            // show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();
            }
            catch { }
        }
    }


    public void IG_SetDefaultPhotoViewer()
    {
        _ = UpdateDefaultPhotoViewerAsync(true);
    }


    public void IG_UnsetDefaultPhotoViewer()
    {
        _ = UpdateDefaultPhotoViewerAsync(false);
    }


    public async Task UpdateDefaultPhotoViewerAsync(bool enable)
    {
        var allExts = Config.AllFormats;

        // Issue #664
        allExts.Remove(".ico");
        var extensions = Config.GetImageFormats(allExts);

        var cmd = enable
            ? IgCommands.SET_DEFAULT_PHOTO_VIEWER
            : IgCommands.UNSET_DEFAULT_PHOTO_VIEWER;

        // run command
        var result = await BHelper.RunIgcmd($"{cmd} {extensions}");

        var langPath = enable
            ? $"{Name}.{nameof(MnuSetDefaultPhotoViewer)}"
            : $"{Name}.{nameof(MnuUnsetDefaultPhotoViewer)}";

        var description = enable
            ? Config.Language[$"{langPath}._SuccessDescription"]
            : "";

        if (result == IgExitCode.Done)
        {
            _ = Config.ShowInfo(description,
                Config.Language[langPath],
                Config.Language[$"{langPath}._Success"]);
        }
        else
        {
            _ = Config.ShowError(Config.Language[$"{langPath}._Error"],
                Config.Language[langPath]);
        }
    }


    /// <summary>
    /// Renames the current image.
    /// </summary>
    public void IG_Rename()
    {
        var oldFilePath = Local.Images.GetFilePath(Local.CurrentIndex);
        if (!File.Exists(oldFilePath)) return;


        var currentFolder = Path.GetDirectoryName(oldFilePath) ?? "";
        var ext = Path.GetExtension(oldFilePath);
        var newName = Path.GetFileNameWithoutExtension(oldFilePath);
        var title = Config.Language[$"{Name}.{nameof(MnuRename)}"];

        using var frm = new Popup(Config.Theme, Config.Language)
        {
            Title = title,
            Value = newName,
            Thumbnail = Gallery.Items[Local.CurrentIndex].ThumbnailImage,
            ThumbnailOverlay = SystemIconApi.GetSystemIcon(SHSTOCKICONID.SIID_RENAME),

            FileNameValueOnly = true,
            TopMost = TopMost,

            Description = oldFilePath + "\r\n"
                + Config.Language[$"{Name}.{nameof(MnuRename)}._Description"],
        };

        if (frm.ShowDialog(this) != DialogResult.OK
            || string.IsNullOrEmpty(frm.Value.Trim())) return;

        newName = frm.Value.Trim() + ext;
        var newFilePath = Path.Combine(currentFolder, newName);


        try
        {
            // Issue 73: Windows ignores case-only changes
            if (string.Equals(oldFilePath, newFilePath, StringComparison.InvariantCultureIgnoreCase))
            {
                // user changing only the case of the filename. Need to perform a trick.
                File.Move(oldFilePath, oldFilePath + "_temp");
                File.Move(oldFilePath + "_temp", newFilePath);
            }
            else
            {
                File.Move(oldFilePath, newFilePath);
            }


            // TODO: once the realtime watcher implemented, delete this:
            Local.Images.SetFileName(Local.CurrentIndex, newFilePath);
            Gallery.Items[Local.CurrentIndex].FileName = newFilePath;
            Gallery.Items[Local.CurrentIndex].Text = newName;
            UpdateImageInfo(ImageInfoUpdateTypes.Name | ImageInfoUpdateTypes.Path);
            //////////////////////////////////////////////////////////////
        }
        catch (Exception ex)
        {
            Config.ShowError(ex.Message, title);
        }
    }


    /// <summary>
    /// Sends or permenantly deletes the current image.
    /// </summary>
    /// <param name="moveToRecycleBin"></param>
    public void IG_Delete(bool moveToRecycleBin = true)
    {
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        if (!File.Exists(filePath)) return;

        PopupResult? result = null;

        var title = moveToRecycleBin
            ? Config.Language[$"{Name}.{nameof(MnuMoveToRecycleBin)}"]
            : Config.Language[$"{Name}.{nameof(MnuDeleteFromHardDisk)}"];

        if (Config.ShowDeleteConfirmation)
        {
            var heading = moveToRecycleBin
                ? Config.Language[$"{Name}.{nameof(MnuMoveToRecycleBin)}._Description"]
                : Config.Language[$"{Name}.{nameof(MnuDeleteFromHardDisk)}._Description"];

            var overlayIcon = moveToRecycleBin
                ? SHSTOCKICONID.SIID_RECYCLER
                : SHSTOCKICONID.SIID_DELETE;

            var description = filePath + "\r\n" +
                    BHelper.FormatSize(Gallery.Items[Local.CurrentIndex].Details.FileSize);

            result = Config.ShowWarning(
                description: description,
                title: title,
                heading: heading,
                buttons: PopupButton.Yes_No,
                icon: overlayIcon,
                thumbnail: Gallery.Items[Local.CurrentIndex].ThumbnailImage,
                optionText: Config.Language["_._DoNotShowThisMessageAgain"],
                formOwner: this);

            // update the delete confirm setting
            Config.ShowDeleteConfirmation = !result.IsOptionChecked;
        }

        if (result == null || result.ExitResult == PopupExitResult.OK)
        {
            try
            {
                BHelper.DeleteFile(filePath, moveToRecycleBin);


                // TODO: once the realtime watcher implemented, delete this:
                Local.Images.Remove(Local.CurrentIndex);
                Gallery.Items.RemoveAt(Local.CurrentIndex);
                _ = ViewNextCancellableAsync(0);
                //////////////////////////////////////////////////////////////
            }
            catch (Exception ex)
            {
                Config.ShowError(ex.Message, title);
            }
        }
    }


    /// <summary>
    /// Sets the viewing image as desktop background.
    /// </summary>
    public void IG_SetDesktopBackground()
    {
        _ = SetDesktopBackgroundAsync();
    }

    public async Task SetDesktopBackgroundAsync()
    {
        // image error
        if (PicMain.Source == ImageSource.Null)
        {
            return;
        }

        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var ext = Path.GetExtension(filePath).ToUpperInvariant();
        var defaultExt = BHelper.IsOS(WindowsOS.Win7) ? ".bmp" : ".jpg";
        var langPath = $"{Name}.{nameof(MnuSetDesktopBackground)}";

        PicMain.ShowMessage(Config.Language[$"{langPath}._CreatingFile"], "", delayMs: 500);


        // print clipboard image
        if (Local.ClipboardImage != null)
        {
            // save image to temp file
            filePath = await Local.SaveImageAsTempFileAsync(defaultExt);
        }
        else if (ext != ".BMP")
        {
            if (ext != ".JPG"
                && ext != ".JPEG"
                && ext != ".PNG"
                && ext != ".GIF")
            {
                // save image to temp file
                filePath = await Local.SaveImageAsTempFileAsync(defaultExt);
            }
        }


        if (!File.Exists(filePath))
        {
            PicMain.ClearMessage();

            _ = Config.ShowError(Config.Language[$"{langPath}._CreatingFileError"],
                Config.Language[langPath]);
        }
        else
        {
            var args = string.Format($"{IgCommands.SET_WALLPAPER} \"{filePath}\" {(int)WallpaperStyle.Current}");
            var result = await BHelper.RunIgcmd(args);


            if (result == IgExitCode.Done)
            {
                PicMain.ShowMessage(
                    Config.Language[$"{langPath}._Success"],
                    Config.InAppMessageDuration);
            }
            else
            {
                PicMain.ClearMessage();

                _ = Config.ShowError(
                    description: Config.Language[$"{langPath}._Error"],
                    title: Config.Language[langPath],
                    heading: Config.Language["_._Error"]);
            }
        }
    }


    /// <summary>
    /// Sets the viewing image as lock screen background.
    /// </summary>
    public void IG_SetLockScreenBackground()
    {
        _ = SetLockScreenBackgroundAsync();
    }

    public async Task SetLockScreenBackgroundAsync()
    {
        // image error
        if (PicMain.Source == ImageSource.Null)
        {
            return;
        }

        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var ext = Path.GetExtension(filePath).ToUpperInvariant();
        var langPath = $"{Name}.{nameof(MnuSetLockScreen)}";

        PicMain.ShowMessage(Config.Language[$"{langPath}._CreatingFile"], "", delayMs: 500);


        // print clipboard image
        if (Local.ClipboardImage != null
            || (ext != ".BMP"
                && ext != ".JPG"
                && ext != ".JPEG"
                && ext != ".PNG"
                && ext != ".GIF"))
        {
            // save image to temp file
            filePath = await Local.SaveImageAsTempFileAsync(".jpg");
        }


        if (!File.Exists(filePath))
        {
            PicMain.ClearMessage();

            _ = Config.ShowError(Config.Language[$"{langPath}._CreatingFileError"],
                Config.Language[langPath]);
        }
        else
        {
            var args = string.Format($"{IgCommands.SET_LOCK_SCREEN} \"{filePath}\"");
            var result = await BHelper.RunIgcmd10(args);


            if (result == IgExitCode.Done)
            {
                PicMain.ShowMessage(
                    Config.Language[$"{langPath}._Success"],
                    Config.InAppMessageDuration);
            }
            else
            {
                PicMain.ClearMessage();

                _ = Config.ShowError(
                    description: Config.Language[$"{langPath}._Error"],
                    title: Config.Language[langPath],
                    heading: Config.Language["_._Error"]);
            }
        }
    }


    public bool IG_ToggleFullScreen(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableFullScreen;
        Config.EnableFullScreen = enable.Value;

        SetFullScreenMode(
            enable: enable.Value,
            changeWindowState: true,
            hideToolbar: Config.HideToolbarInFullscreen,
            hideThumbnails: Config.HideThumbnailsInFullscreen);

        // update menu item state
        MnuFullScreen.Checked = Config.EnableFullScreen;

        // update toolbar items state
        UpdateToolbarItemsState();

        if (showInAppMessage && Config.EnableFullScreen)
        {
            var langPath = $"{Name}.{nameof(MnuFullScreen)}";
            PicMain.ShowMessage(
                string.Format(Config.Language[$"{langPath}._EnableDescription"], MnuFullScreen.ShortcutKeyDisplayString),
                Config.Language[$"{langPath}._Enable"],
                Config.InAppMessageDuration);
        }

        return Config.EnableFullScreen;
    }


    /// <summary>
    /// Enter or Exit Full screen mode
    /// </summary>
    /// <param name="enable"></param>
    /// <param name="changeWindowState"></param>
    /// <param name="hideToolbar">Hide Toolbar</param>
    /// <param name="hideThumbnails">Hide Thumbnail bar</param>
    public void SetFullScreenMode(bool enable = true,
        bool changeWindowState = true,
        bool hideToolbar = false,
        bool hideThumbnails = false)
    {
        // full screen
        if (enable)
        {
            Visible = false;
            SuspendLayout();

            // back up the last states of the window
            _windowBound = Bounds;
            _windowState = WindowState;
            if (hideToolbar) _showToolbar = Config.ShowToolbar;
            if (hideThumbnails) _showThumbnails = Config.ShowThumbnails;

            if (changeWindowState)
            {
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Bounds = Screen.FromControl(this).Bounds;
            }

            // Hide toolbar
            if (hideToolbar)
            {
                IG_ToggleToolbar(false);
            }
            // hide thumbnail
            if (hideThumbnails)
            {
                IG_ToggleGallery(false);
            }

            // disable background transparency
            Toolbar.BackColor = Config.Theme.Colors.ToolbarBgColor.NoAlpha();
            PicMain.BackColor = Config.BackgroundColor.NoAlpha();
            Gallery.BackColor =
                Sp1.SplitterBackColor =
                Sp2.SplitterBackColor = Config.Theme.Colors.ThumbnailBarBgColor.NoAlpha();
            WindowApi.SetWindowFrame(Handle, new Padding(0));

            ResumeLayout(false);
            Visible = true;
        }

        // exit full screen
        else
        {
            SuspendLayout();

            // restore last state of the window
            if (hideToolbar) Config.ShowToolbar = _showToolbar;
            if (hideThumbnails) Config.ShowThumbnails = _showThumbnails;

            if (hideToolbar && Config.ShowToolbar)
            {
                // Show toolbar
                IG_ToggleToolbar(true);
            }
            if (hideThumbnails && Config.ShowThumbnails)
            {
                // Show thumbnail
                IG_ToggleGallery(true);
            }

            // re-enable background transparency
            Toolbar.BackColor = Config.Theme.Colors.ToolbarBgColor;
            PicMain.BackColor = Config.BackgroundColor;
            Gallery.BackColor =
                Sp1.SplitterBackColor =
                Sp2.SplitterBackColor = Config.Theme.Colors.ThumbnailBarBgColor;
            WindowApi.SetWindowFrame(Handle, BackdropMargin);

            ResumeLayout(false);


            // restore window state, size, position
            if (changeWindowState)
            {
                Config.FrmMainState = WindowSettings.ToWindowState(_windowState);

                // windows state
                if (_windowState == FormWindowState.Normal)
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                    WindowState = FormWindowState.Normal;

                    // Windows Bound (Position + Size)
                    Bounds = _windowBound;
                }
                else if (_windowState == FormWindowState.Maximized)
                {
                    // Windows Bound (Position + Size)
                    var wp = WindowSettings.GetFrmMainPlacementFromConfig();
                    WindowSettings.SetPlacementToWindow(this, wp);

                    // to make sure the SizeChanged event is not triggered
                    // before we set the window placement
                    FormBorderStyle = FormBorderStyle.Sizable;
                }
                else
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                }

                Config.UpdateFormIcon(this);
            }

        }


    }


    /// <summary>
    /// Starts a new slideshow
    /// </summary>
    public void IG_StartNewSlideshow()
    {
        _ = StartNewSlideshowAsync();
    }

    public async Task StartNewSlideshowAsync()
    {
        var slideshowIndex = 0;
        var serverCount = Local.SlideshowPipeServers.Count(s => s != null);

        if (serverCount == 0)
        {
            Local.SlideshowPipeServers.Clear();
        }
        else
        {
            var lastServer = Local.SlideshowPipeServers.FindLast(s => s != null);

            if (lastServer != null)
            {
                slideshowIndex = lastServer.TagNumber + 1;
            }
        }

        var pipeName = $"{Constants.SLIDESHOW_PIPE_PREFIX}{slideshowIndex}";

        // create a new slideshow pipe server
        var slideshowServer = new PipeServer(pipeName, PipeDirection.InOut, slideshowIndex);
        slideshowServer.ClientDisconnected += SlideshowServer_Disconnected;

        Local.SlideshowPipeServers.Add(slideshowServer);

        // start the server
        slideshowServer.Start();
        await Config.WriteAsync();

        // start slideshow client
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        await BHelper.RunIgcmd($"{IgCommands.START_SLIDESHOW} {slideshowIndex} \"{filePath}\"", false);

        // wait for client connection
        await slideshowServer.WaitForConnectionAsync();
        Config.EnableSlideshow = true;


        var fileListJson = BHelper.ToJson(Local.Images.FileNames);
        await slideshowServer.SendAsync($"{SlideshowPipeCommands.SET_IMAGE_LIST}={fileListJson}");


        // hide FrmMain
        SetFrmMainStateInSlideshow(Config.EnableSlideshow);

        // prevent system from entering sleep mode
        SysExecutionState.PreventSleep();
    }


    public void SlideshowServer_Disconnected(object? sender, DisconnectedEventArgs e)
    {
        var clonedList = Local.SlideshowPipeServers.ToList();
        var serverIndex = clonedList.FindIndex(s => s?.PipeName == e.PipeName);

        if (serverIndex != -1)
        {
            Local.SlideshowPipeServers[serverIndex].Stop();
            Local.SlideshowPipeServers[serverIndex].Dispose();
            Local.SlideshowPipeServers[serverIndex] = null;
        }


        _cleanSlideshowServerCancelToken.Cancel();
        _cleanSlideshowServerCancelToken = new();
        _ = CleanSlideshowServerListAsync(_cleanSlideshowServerCancelToken.Token);
    }

    public async Task CleanSlideshowServerListAsync(CancellationToken token = default)
    {
        try
        {
            await Task.Delay(500, token);
            token.ThrowIfCancellationRequested();

            var serverCount = Local.SlideshowPipeServers.Count(s => s != null);

            if (serverCount == 0)
            {
                Config.EnableSlideshow = false;
                Local.SlideshowPipeServers.Clear();

                // show FrmMain
                SetFrmMainStateInSlideshow(Config.EnableSlideshow);

                // allow system to enter sleep mode
                SysExecutionState.AllowSleep();
            }
        }
        catch (OperationCanceledException) { }
    }

    public void SetFrmMainStateInSlideshow(bool enableSlideshow)
    {
        if (InvokeRequired)
        {
            Invoke(SetFrmMainStateInSlideshow, enableSlideshow);
            return;
        }


        // hide FrmMain
        if (enableSlideshow && Config.HideFrmMainInSlideshow)
        {
            if (!Config.EnableFullScreen)
            {
                _windowState = WindowState;
            }

            WindowState = FormWindowState.Minimized;
        }

        // show FrmMain
        else if (!enableSlideshow && Config.HideFrmMainInSlideshow)
        {
            WindowState = _windowState;
        }
    }


    /// <summary>
    /// Disconnects all slideshow servers.
    /// </summary>
    public static void DisconnectAllSlideshowServers()
    {
        foreach (var server in Local.SlideshowPipeServers)
        {
            server?.Stop();
            server?.Dispose();
        }

        Local.SlideshowPipeServers.Clear();
    }


    /// <summary>
    /// Stops and closes all slideshows
    /// </summary>
    public void IG_CloseAllSlideshowWindows()
    {
        foreach (var server in Local.SlideshowPipeServers)
        {
            _ = server?.SendAsync(Constants.SLIDESHOW_PIPE_CMD_TERMINATE);
        }
    }


    /// <summary>
    /// Opens context menu at the current cursor position
    /// </summary>
    public void IG_OpenContextMenu()
    {
        MnuContext.Show(Cursor.Position);
    }


    /// <summary>
    /// Opens main menu at the current cursor position
    /// </summary>
    public void IG_OpenMainMenu()
    {
        MnuMain.Show(Cursor.Position);
    }


    /// <summary>
    /// Sets <see cref="FrmMain"/> window state
    /// </summary>
    public void IG_SetFrmMainState(string state)
    {
        WindowState = BHelper.ParseEnum<FormWindowState>(state);
    }


    /// <summary>
    /// Sets <see cref="FrmMain"/> movable state
    /// </summary>
    /// <param name="enable"></param>
    public void IG_SetFrmMainMoveable(bool? enable = null)
    {
        enable ??= true;
        _movableForm.Key = Keys.ShiftKey | Keys.Shift;

        if (enable.Value)
        {
            _movableForm.Enable();
            _movableForm.Enable(Toolbar, PicMain, Gallery);
        }
        else
        {
            _movableForm.Disable();
            _movableForm.Disable(Toolbar, PicMain, Gallery);
        }

    }


    /// <summary>
    /// Flips the viewing image.
    /// </summary>
    /// <param name="options"></param>
    public void IG_FlipImage(FlipOptions options)
    {
        _ = FlipImageAsync(options);
    }

    public async Task FlipImageAsync(FlipOptions options)
    {
        if (PicMain.Source == ImageSource.Null || options == FlipOptions.None) return;

        if (Local.ClipboardImage != null)
        {
            PhotoCodec.ApplyIgImgChanges(Local.ClipboardImage, new()
            {
                Flips = options,
            });

            PicMain.SetImage(new()
            {
                Image = Local.ClipboardImage,
                FrameCount = 1,
                HasAlpha = true,
            });

            return;
        }


        var img = await Local.Images.GetAsync(Local.CurrentIndex);
        if (img?.ImgData?.Image != null)
        {
            // update flip changes
            if (options.HasFlag(FlipOptions.Horizontal))
            {
                if (Local.CurrentChanges.Flips.HasFlag(FlipOptions.Horizontal))
                {
                    Local.CurrentChanges.Flips ^= FlipOptions.Horizontal;
                }
                else
                {
                    Local.CurrentChanges.Flips |= FlipOptions.Horizontal;
                }
            }

            if (options.HasFlag(FlipOptions.Vertical))
            {
                if (Local.CurrentChanges.Flips.HasFlag(FlipOptions.Vertical))
                {
                    Local.CurrentChanges.Flips ^= FlipOptions.Vertical;
                }
                else
                {
                    Local.CurrentChanges.Flips |= FlipOptions.Vertical;
                }
            }


            PhotoCodec.ApplyIgImgChanges(img.ImgData.Image, new IgImgChanges()
            {
                Flips = options,
            });
            PicMain.SetImage(img.ImgData);
        }

    }


    /// <summary>
    /// Crops the viewing image.
    /// </summary>
    public void IG_Crop()
    {
        _ = CropAsync();
    }


    public async Task CropAsync()
    {
        var img = await GetSelectedImageAreaAsync();
        if (img == null) return;

        LoadClipboardImage(img);

        // reset selection
        PicMain.ClientSelection = default;
    }


    /// <summary>
    /// Gets the selected image data clipped by the selection area.
    /// </summary>
    public async Task<WicBitmapSource?> GetSelectedImageAreaAsync()
    {
        if (PicMain.Source == ImageSource.Null || PicMain.SourceSelection.IsEmpty) return null;


        if (Local.ClipboardImage != null)
        {
            return BHelper.CropImage(Local.ClipboardImage, PicMain.SourceSelection);
        }


        var img = await Local.Images.GetAsync(Local.CurrentIndex);
        if (img == null) return null;

        return BHelper.CropImage(img.ImgData.Image, PicMain.SourceSelection);
    }


    /// <summary>
    /// Toggles crop tool.
    /// </summary>
    public bool IG_ToggleCropTool(bool? visible = null)
    {
        visible ??= !MnuCropTool.Checked;

        // update menu item state
        MnuCropTool.Checked =
            // set selection mode
            PicMain.EnableSelection = visible.Value;

        // update toolbar items state
        UpdateToolbarItemsState();

        var frm = new FrmCrop(this, Config.Theme);
        frm.Show();

        return visible.Value;
    }


}


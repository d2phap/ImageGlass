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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using ImageGlass.Base.PhotoBox;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.WinApi;
using ImageGlass.Library.WinAPI;
using ImageGlass.Settings;
using ImageGlass.Tools;
using ImageGlass.Viewer;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Unloads the viewing image.
    /// </summary>
    public void IG_Unload()
    {
        // cancel the current loading image
        _loadCancelToken?.Cancel();

        if (Local.ClipboardImage != null)
        {
            LoadClipboardImage(null);
            _ = ViewNextCancellableAsync(0, isSkipCache: true);
        }
        else
        {
            PicMain.SetImage(null);
            PicMain.ClearMessage();

            Local.Images.Unload(Local.CurrentIndex);

            Local.RaiseImageUnloadedEvent(new ImageEventArgs()
            {
                Index = Local.CurrentIndex,
                FilePath = Local.Images.GetFilePath(Local.CurrentIndex),
            });
        }
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
        using var frm = new Popup()
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


    /// <summary>
    /// Views an image frame
    /// </summary>
    /// <param name="frameIndex">Frame index</param>
    public void IG_ViewFrame(int frameIndex)
    {
        _ = ViewNextCancellableAsync(0, frameIndex: frameIndex);
    }


    /// <summary>
    /// Views the next image frame
    /// </summary>
    public void IG_ViewNextFrame()
    {
        Local.CurrentFrameIndex++;
        _ = ViewNextCancellableAsync(0, frameIndex: (int)Local.CurrentFrameIndex);
    }


    /// <summary>
    /// Views the previous image frame
    /// </summary>
    public void IG_ViewPreviousFrame()
    {
        Local.CurrentFrameIndex--;
        _ = ViewNextCancellableAsync(0, frameIndex: (int)Local.CurrentFrameIndex);
    }


    /// <summary>
    /// Views the first image frame
    /// </summary>
    public void IG_ViewFirstFrame()
    {
        _ = ViewNextCancellableAsync(0, frameIndex: 0);
    }


    /// <summary>
    /// Views the last image frame
    /// </summary>
    public void IG_ViewLastFrame()
    {
        var lastFrameIndex = (Local.Metadata?.FramesCount ?? 1) - 1;
        _ = ViewNextCancellableAsync(0, frameIndex: lastFrameIndex);
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
        using var frm = new Popup()
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
        PicMain.ZoomMode = Config.ZoomMode;

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

        // update window fit
        if (Config.EnableWindowFit)
        {
            FitWindowToImage(false);
        }

        return Config.ShowToolbar;
    }


    /// <summary>
    /// Toggles <see cref="Gallery"/> visibility
    /// </summary>
    /// <param name="visible"></param>
    public bool IG_ToggleGallery(bool? visible = null)
    {
        visible ??= !Config.ShowThumbnails;
        Config.ShowThumbnails = visible.Value;

        Gallery.ScrollBars = Config.ShowThumbnailScrollbars || Gallery.View == ImageGlass.Gallery.View.Thumbnails;
        Gallery.ShowItemText = Config.ShowThumbnailFilename;

        // update gallery size
        UpdateGallerySize();

        // toggle gallery
        Gallery.Visible = Config.ShowThumbnails;

        // update menu item state
        MnuToggleThumbnails.Checked = Config.ShowThumbnails;

        // update toolbar items state
        UpdateToolbarItemsState();


        // update window fit
        if (Config.EnableWindowFit)
        {
            FitWindowToImage(false);
        }

        return Config.ShowThumbnails;
    }


    /// <summary>
    /// Recalculates and updates size of <see cref="Gallery"/>.
    /// </summary>
    private void UpdateGallerySize()
    {
        if (!Config.ShowThumbnails) return;

        Gallery.SuspendLayout();

        var scrollBarSize = this.ScaleToDpi(1.5f); // random gap
        if (Gallery.ScrollBars && Gallery.HScrollBar.Visible)
        {
            Gallery.VScrollBar.Height =
                Gallery.HScrollBar.Height =
                    (int)(SystemInformation.HorizontalScrollBarHeight * 0.75f);

            scrollBarSize += Gallery.HScrollBar.Height;
        }

        // update thumbnail size
        Gallery.ThumbnailSize = this.ScaleToDpi(new Size(Config.ThumbnailSize, Config.ThumbnailSize));


        // Thumbnails view
        if (Gallery.View == ImageGlass.Gallery.View.Thumbnails)
        {
            var minWidth = Gallery.ThumbnailSize.Width
                + (int)scrollBarSize
                + (int)(Gallery.Renderer.MeasureItemMargin(Gallery.View).Width * 6.5f);

            Gallery.Width = Math.Max(minWidth, Config.ThumbnailBarWidth);
            Config.ThumbnailBarWidth = Gallery.Width;
        }
        // HorizontalStrip view
        else
        {
            // Gallery bar
            Gallery.Height = Gallery.ThumbnailSize.Height
                + (int)scrollBarSize
                + (int)(Gallery.Renderer.MeasureItemMargin(Gallery.View).Height * 6.5f);
        }

        Gallery.ResumeLayout(false);
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

            Heading = $"{App.AppName} beta 4\r\n" +
                $"A lightweight, versatile image viewer\r\n" +
                $"\r\n" +
                $"Version: {appVersion}\r\n" +
                $".NET Runtime: {Environment.Version.ToString()}",

            Text = $"Author: Dương Diệu Pháp\r\n" +
                    $"\r\n" +
                    $"Special thanks to:\r\n" +
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
                    "    Copyright © 2022-2023 Dương Diệu Pháp. All rights reserved.\r\n" +
                    "\r\n" +

                    "◾ Magick.NET\r\n" +
                    "    Distributed under the terms of the MIT license.\r\n" +
                    "    Copyright © 2013-2023 Dirk Lemstra.\r\n" +
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

        PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);


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
            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
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
                _ = Config.ShowError(this,
                    $"{ex.Source}:\r\n{ex.Message}", "",
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
            PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);

            // save image to temp file
            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }

        PicMain.ClearMessage();

        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
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
                    heading: Config.Language["_._Error"],
                    formOwner: this);
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
            string.Format(Config.Language[$"{Name}.{nameof(MnuCopyFile)}._Success"], Config.EnableCopyMultipleFiles ? Local.StringClipboard.Count : 1),
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
            string.Format(Config.Language[$"{Name}.{nameof(MnuCutFile)}._Success"], Config.EnableCutMultipleFiles ? Local.StringClipboard.Count : 1),
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
        }, enableFading: Config.EnableImageTransition);
        PicMain.ClearMessage();

        Local.ImageTransform.Clear();

        // reset zoom mode
        IG_SetZoomMode(Config.ZoomMode.ToString());

        LoadImageInfo(ImageInfoUpdateTypes.All);
    }


    #endregion // Clipboard functions


    /// <summary>
    /// Open the current image's location
    /// </summary>
    public void IG_OpenLocation()
    {
        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);

        BHelper.OpenFilePath(filePath);
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
        _ = SaveCurrentImageAsync();
    }

    public async Task<bool> SaveCurrentImageAsync()
    {
        var srcFilePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var langPath = $"{Name}.{nameof(MnuSave)}";


        // show override warning
        if (Config.ShowSaveOverrideConfirmation)
        {
            var result = Config.ShowWarning(
                description: srcFilePath,
                note: Config.Language[$"{langPath}._ConfirmDescription"],
                title: Config.Language[langPath],
                heading: Config.Language[$"{langPath}._Confirm"],
                buttons: PopupButton.Yes_No,
                thumbnail: Gallery.Items[Local.CurrentIndex].ThumbnailImage,
                optionText: Config.Language["_._DoNotShowThisMessageAgain"],
                formOwner: this);

            // update ShowSaveOverrideConfirmation setting
            Config.ShowSaveOverrideConfirmation = !result.IsOptionChecked;

            if (result.ExitResult != PopupExitResult.OK) return false;
        }

        return await SaveImageAsync(srcFilePath, srcFilePath);
    }


    /// <summary>
    /// Saves the viewing image as a new file.
    /// </summary>
    public void IG_SaveAs()
    {
        _ = SaveCurrentImageAsACopyAsync();
    }


    public async Task<bool> SaveCurrentImageAsACopyAsync()
    {
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

        // show dialog
        if (saveDialog.ShowDialog() != DialogResult.OK) return false;


        var destExt = Path.GetExtension(saveDialog.FileName).ToLowerInvariant();
        Local.SaveAsFilterExt = destExt;


        // show override warning
        if (File.Exists(saveDialog.FileName) && Config.ShowSaveOverrideConfirmation)
        {
            var langPath = $"{Name}.{nameof(MnuSave)}";
            var fi = new FileInfo(saveDialog.FileName);

            var result = Config.ShowWarning(
                description: saveDialog.FileName + "\r\n" + BHelper.FormatSize(fi.Length),
                note: Config.Language[$"{langPath}._ConfirmDescription"],
                title: Config.Language[$"{Name}.{nameof(MnuSaveAs)}"],
                heading: Config.Language[$"{langPath}._Confirm"],
                buttons: PopupButton.Yes_No,
                optionText: Config.Language["_._DoNotShowThisMessageAgain"],
                formOwner: this);

            // update ShowSaveOverrideConfirmation setting
            Config.ShowSaveOverrideConfirmation = !result.IsOptionChecked;

            if (result.ExitResult != PopupExitResult.OK)
            {
                return false;
            }
        }


        return await SaveImageAsync(saveDialog.FileName, srcFilePath);
    }


    /// <summary>
    /// Save the viewing image to file.
    ///   <para>
    ///     The source image is checked by this order:
    ///     <list type="number">
    ///       <item>Selected image area.</item>
    ///       <item><see cref="Local.ClipboardImage"/>.</item>
    ///       <item>Source <paramref name="srcFilePath"/> file.</item>
    ///     </list>
    ///   </para>
    /// </summary>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="srcFilePath">
    ///   Source file path.
    ///   <para>
    ///     <c>Note:**</c>
    ///     If it's empty, ImageGlass will check for the selection and clipboard image.
    ///   </para>
    /// </param>
    public async Task<bool> SaveImageAsync(string destFilePath, string srcFilePath = "")
    {
        var saveSource = ImageSaveSource.Undefined;
        var hasSrcPath = !string.IsNullOrEmpty(srcFilePath);
        var langPath = $"{Name}.{nameof(MnuSave)}";
        Exception? error = null;

        PicMain.ShowMessage(destFilePath, Config.Language[$"{langPath}._Saving"]);
        _fileWatcher.Stop();


        // save the selection
        var hasSelection = PicMain.EnableSelection && !PicMain.SourceSelection.IsEmpty;
        if (hasSelection)
        {
            using var selectedImg = await GetSelectedImageAreaAsync();
            error = await DoSaveAsync(selectedImg, srcFilePath, destFilePath);
            saveSource = ImageSaveSource.SelectedArea;
        }

        // save the clipboard image
        else if (Local.ClipboardImage != null)
        {
            error = await DoSaveAsync(Local.ClipboardImage, srcFilePath, destFilePath);
            saveSource = ImageSaveSource.Clipboard;
        }

        // save the image in the list
        else if (hasSrcPath)
        {
            error = await DoSaveAsync(srcFilePath, destFilePath);
            saveSource = ImageSaveSource.CurrentFile;
        }

        // image is empty
        else
        {
            if (Config.EnableFileWatcher)
            {
                _fileWatcher.Start();
            }

            return false;
        }


        // Error
        if (error != null)
        {
            PicMain.ClearMessage();

            _ = Config.ShowError(
                description: error.Source + ":\r\n" + error.Message + "\r\n\r\n" + destFilePath,
                title: Config.Language[langPath],
                heading: string.Format(Config.Language[$"{langPath}._Error"]),
                formOwner: this);

            if (Config.EnableFileWatcher)
            {
                _fileWatcher.Start();
            }

            return false;
        }

        // success
        if (saveSource == ImageSaveSource.SelectedArea)
        {
            // manually update the change if FileWatcher is not enabled
            if (!Config.EnableFileWatcher)
            {
                // reload to view the updated image
                IG_Reload();
            }

            // reset selection
            PicMain.ClientSelection = default;
        }
        else if (saveSource == ImageSaveSource.Clipboard)
        {
            // clear the clipboard image
            LoadClipboardImage(null);

            // manually update the change if FileWatcher is not enabled
            if (!Config.EnableFileWatcher)
            {
                // reload to view the updated image
                IG_Reload();
            }
        }

        PicMain.ShowMessage(destFilePath, Config.Language[$"{langPath}._Success"], Config.InAppMessageDuration);


        // file was overriden
        if (destFilePath.Equals(srcFilePath, StringComparison.InvariantCultureIgnoreCase))
        {
            // update cache of the modified item
            Gallery.Items[Local.CurrentIndex].UpdateThumbnail();
            Gallery.Items[Local.CurrentIndex].UpdateDetails(true);
        }


        // reload image
        IG_Reload();
        Gallery.Items[Local.CurrentIndex].UpdateThumbnail();
        if (Config.EnableFileWatcher)
        {
            _fileWatcher.Start();
        }


        // emits ImageSaved event
        Local.RaiseImageSavedEvent(new ImageSaveEventArgs(srcFilePath, destFilePath, saveSource));

        return true;
    }


    /// <summary>
    /// Saves the given <see cref="WicBitmapSource"/> image to file.
    /// </summary>
    private async Task<Exception?> DoSaveAsync(WicBitmapSource? wicImg, string srcPath, string destPath)
    {
        try
        {
            var lastWriteTime = File.GetLastWriteTime(destPath);

            // base64 format
            if (destPath.EndsWith(".b64", StringComparison.InvariantCultureIgnoreCase)
                || destPath.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                var srcExt = Path.GetExtension(srcPath);
                await PhotoCodec.SaveAsBase64Async(wicImg, srcExt, destPath, Local.ImageTransform);
            }
            // other formats
            else
            {
                await PhotoCodec.SaveAsync(wicImg, destPath, Local.ImageTransform, Config.ImageEditQuality);
            }

            // Issue #307: option to preserve the modified date/time
            if (Config.ShouldPreserveModifiedDate)
            {
                File.SetLastWriteTime(destPath, lastWriteTime);
            }

            // reset transformations
            Local.ImageTransform.Clear();
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
    }


    /// <summary>
    /// Saves the given image path to file.
    /// </summary>
    private async Task<Exception?> DoSaveAsync(string srcPath, string destPath)
    {
        try
        {
            var lastWriteTime = File.GetLastWriteTime(destPath);

            // base64 format
            if (destPath.EndsWith(".b64", StringComparison.InvariantCultureIgnoreCase)
                || destPath.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                await PhotoCodec.SaveAsBase64Async(srcPath, destPath, Local.Images.ReadOptions, Local.ImageTransform);
            }
            // other formats
            else
            {
                await PhotoCodec.SaveAsync(srcPath, destPath, Local.Images.ReadOptions, Local.ImageTransform, Config.ImageEditQuality);
            }

            // Issue #307: option to preserve the modified date/time
            if (Config.ShouldPreserveModifiedDate)
            {
                File.SetLastWriteTime(destPath, lastWriteTime);
            }

            // reset transformations
            Local.ImageTransform.Clear();
        }
        catch (Exception ex)
        {
            return ex;
        }

        return null;
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
            PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);

            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }
        else
        {
            filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        }

        PicMain.ClearMessage();


        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
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


    /// <summary>
    /// Open app for edit action.
    /// </summary>
    public void IG_OpenEditApp()
    {
        _ = OpenEditAppAsync();
    }

    public async Task OpenEditAppAsync()
    {
        var langPath = $"{Name}.{nameof(MnuEdit)}";

        // get file path to edit
        string? filePath;
        if (Local.ClipboardImage != null)
        {
            PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);
            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }
        else
        {
            filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        }
        PicMain.ClearMessage();


        if (!File.Exists(filePath))
        {
            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
                Config.Language[langPath]);

            return;
        }


        // get extension
        var ext = Path.GetExtension(filePath).ToLowerInvariant();

        // get app from the extension
        if (Config.EditApps.TryGetValue(ext, out var app) && app != null)
        {
            // open configured app for editing
            using var p = new Process();
            p.StartInfo.FileName = BHelper.ResolvePath(app.Executable);

            // build the arguments
            var args = app.Argument.Replace(Constants.FILE_MACRO, $"\"{filePath}\"");
            p.StartInfo.Arguments = $"{args}";

            // show error dialog
            p.StartInfo.ErrorDialog = true;

            try
            {
                p.Start();

                RunActionAfterEditing();
            }
            catch { }
        }
        else // Edit by default associated app
        {
            EditByDefaultApp(filePath);
        }
    }

    /// <summary>
    /// Runs the <see cref="Config.AfterEditingAction"/> action after done editing.
    /// </summary>
    public void RunActionAfterEditing()
    {
        if (Config.AfterEditingAction == AfterEditAppAction.Minimize)
        {
            foreach (var frm in Application.OpenForms)
            {
                (frm as Form).WindowState = FormWindowState.Minimized;
            }
        }
        else if (Config.AfterEditingAction == AfterEditAppAction.Close)
        {
            IG_Exit();
        }
    }

    /// <summary>
    /// Edits the viewing image by default app.
    /// </summary>
    public void EditByDefaultApp(string filePath)
    {
        var langPath = $"{Name}.{nameof(MnuEdit)}";

        // windows 11 sucks the verb 'edit'
        if (BHelper.IsOS(WindowsOS.Win11OrLater))
        {
            var mspaint11 = @"%LocalAppData%\Microsoft\WindowsApps\mspaint.exe";
            var mspaint11Path = BHelper.ResolvePath(mspaint11);

            if (!File.Exists(mspaint11Path))
            {
                _ = Config.ShowInfo(
                    description: filePath,
                    title: string.Format(Config.Language[langPath], ""),
                    heading: Config.Language[$"{langPath}._AppNotFound"],
                    formOwner: this);

                return;
            }

            using var p11 = new Process();
            p11.StartInfo.FileName = mspaint11Path;
            p11.StartInfo.Arguments = $"\"{filePath}\"";
            p11.StartInfo.UseShellExecute = true;

            try
            {
                p11.Start();

                RunActionAfterEditing();
            }
            catch (Exception ex)
            {
                _ = Config.ShowError(
                    description: ex.Message + $"\r\n\r\n{filePath}",
                    title: string.Format(Config.Language[langPath], "(MS Paint)"),
                    formOwner: this);
            }

            return;
        }


        // windows 10 or earlier ------------------------------
        var win32ErrorMsg = string.Empty;

        using var p10 = new Process();
        p10.StartInfo.FileName = $"\"{filePath}\"";
        p10.StartInfo.Verb = "edit";

        // first try: launch the associated app for editing
        try
        {
            p10.Start();

            RunActionAfterEditing();
        }
        catch (Win32Exception ex)
        {
            // file does not have associated app
            win32ErrorMsg = ex.Message;
        }
        catch { }

        if (string.IsNullOrEmpty(win32ErrorMsg)) return;


        // second try: use MS Paint to edit the file
        using var p = new Process();
        p.StartInfo.FileName = BHelper.ResolvePath("mspaint.exe");
        p.StartInfo.Arguments = $"\"{filePath}\"";
        p.StartInfo.UseShellExecute = true;


        try
        {
            p.Start();

            RunActionAfterEditing();
        }
        catch (Win32Exception)
        {
            // show error: file does not have associated app
            _ = Config.ShowError(
                description: win32ErrorMsg + $"\r\n\r\n{filePath}",
                title: string.Format(Config.Language[langPath], ""),
                formOwner: this);
        }
        catch { }
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

        // run command and show the results
        _ = await BHelper.RunIgcmd($"{cmd} {extensions} {IgCommands.SHOW_UI}");
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

        using var frm = new Popup()
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


            // manually update the change if FileWatcher is not enabled
            if (!Config.EnableFileWatcher)
            {
                Local.Images.SetFileName(Local.CurrentIndex, newFilePath);
                Gallery.Items[Local.CurrentIndex].FileName = newFilePath;
                Gallery.Items[Local.CurrentIndex].Text = newName;
                LoadImageInfo(ImageInfoUpdateTypes.Name | ImageInfoUpdateTypes.Path);
            }
        }
        catch (Exception ex)
        {
            Config.ShowError(this, ex.Message, title);
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


                // manually update the change if FileWatcher is not enabled
                if (!Config.EnableFileWatcher)
                {
                    Local.Images.Remove(Local.CurrentIndex);
                    Gallery.Items.RemoveAt(Local.CurrentIndex);
                    _ = ViewNextCancellableAsync(0);
                }
            }
            catch (Exception ex)
            {
                Config.ShowError(this, ex.Message, title);
            }
        }
    }


    /// <summary>
    /// Toggles image animation for the animated format.
    /// </summary>
    public void IG_ToggleImageAnimation(bool? enable = null)
    {
        enable ??= !PicMain.IsImageAnimating;

        if (enable.Value)
        {
            PicMain.StartAnimator();
        }
        else
        {
            PicMain.StopCurrentAnimator();
        }
    }


    /// <summary>
    /// Exports image frames.
    /// </summary>
    public void IG_ExportImageFrames()
    {
        _ = ExportImageFramesAsync();
    }

    public async Task ExportImageFramesAsync()
    {
        // image error
        if (PicMain.Source == ImageSource.Null) return;


        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);

        // clipboard image
        if (Local.ClipboardImage != null)
        {
            // save image to temp file
            filePath = await Local.SaveImageAsTempFileAsync(".png");
        }


        var args = string.Format($"{IgCommands.EXPORT_FRAMES} \"{filePath}\"");
        await BHelper.RunIgcmd(args);
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
        if (PicMain.Source == ImageSource.Null) return;

        var filePath = Local.Images.GetFilePath(Local.CurrentIndex);
        var ext = Path.GetExtension(filePath).ToUpperInvariant();
        var defaultExt = BHelper.IsOS(WindowsOS.Win7) ? ".bmp" : ".jpg";
        var langPath = $"{Name}.{nameof(MnuSetDesktopBackground)}";

        PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);


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

            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
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
                    heading: Config.Language["_._Error"],
                    formOwner: this);
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

        PicMain.ShowMessage(Config.Language[$"_._CreatingFile"], "", delayMs: 500);


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

            _ = Config.ShowError(this,
                Config.Language[$"_._CreatingFileError"],
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
                    heading: Config.Language["_._Error"] + $"({result})",
                    formOwner: this);
            }
        }
    }


    /// <summary>
    /// Toggles Window fit.
    /// </summary>
    public bool IG_ToggleWindowFit(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableWindowFit;
        Config.EnableWindowFit = enable.Value;

        if (Config.EnableWindowFit)
        {
            // exit full screen
            if (Config.EnableFullScreen)
            {
                IG_ToggleFullScreen(false, false);
            }
        }

        // set Window Fit mode
        FitWindowToImage();

        // update menu item state
        MnuWindowFit.Checked = Config.EnableWindowFit;

        // disable maximize button
        MaximizeBox = !Config.EnableWindowFit;

        // update toolbar items state
        UpdateToolbarItemsState();

        if (showInAppMessage)
        {
            var langPath = $"{Name}.{nameof(MnuWindowFit)}";
            var message = Config.EnableWindowFit
                ? Config.Language[$"{langPath}._Enable"]
                : Config.Language[$"{langPath}._Disable"];

            PicMain.ShowMessage("", message, Config.InAppMessageDuration);
        }

        return Config.EnableWindowFit;
    }

    public void FitWindowToImage(bool resetZoomMode = true)
    {
        if (!Config.EnableWindowFit || PicMain.Source == ImageSource.Null)
            return; // Nothing to do

        WindowState = FormWindowState.Normal;

        // get the gap of the window to the viewer control
        // it includes boder, titlebar, toolbar, gallery,...
        var horzGap = Width - ClientSize.Width;
        var vertGap = Height - ClientSize.Height;

        if (Config.ShowThumbnails)
        {
            if (Gallery.Dock == DockStyle.Left || Gallery.Dock == DockStyle.Right)
            {
                horzGap += Gallery.Width;
            }
            else
            {
                vertGap += Gallery.Height;
            }
        }
        if (Config.ShowToolbar)
        {
            if (Toolbar.Dock == DockStyle.Left || Toolbar.Dock == DockStyle.Right)
            {
                horzGap += Toolbar.Width;
            }
            else
            {
                vertGap += Toolbar.Height;
            }
        }


        // get current screen
        var workingArea = Screen.FromControl(this).WorkingArea;

        // get source image size
        var srcImgW = PicMain.SourceWidth;
        var srcImgH = PicMain.SourceHeight;

        // get zoom factor
        var zoomFactor = PicMain.ZoomFactor;
        if (resetZoomMode)
        {
            if (Config.ZoomMode == ZoomMode.LockZoom)
            {
                PicMain.SetZoomFactor(Config.ZoomLockValue / 100f, false);
            }
            else
            {
                var maxViewerWidth = workingArea.Width - horzGap;
                var maxViewerHeight = workingArea.Height - vertGap;

                // recalculate zoom factor for the new size
                zoomFactor = PicMain.CalculateZoomFactor(Config.ZoomMode, srcImgW, srcImgH, maxViewerWidth, maxViewerHeight);
            }
        }

        // get image size after zoomed
        var zoomImgW = (int)(srcImgW * zoomFactor);
        var zoomImgH = (int)(srcImgH * zoomFactor);

        // adjust the viewer size to fit the entire image
        // but not larger than desktop working area.
        var viewerBound = new Rectangle()
        {
            Width = Math.Min(zoomImgW, workingArea.Width - horzGap),
            Height = Math.Min(zoomImgH, workingArea.Height - vertGap),
        };

        // adjust viewer size and position to the desktop working area
        viewerBound = BHelper.CenterRectToRect(viewerBound,
            new Rectangle(
                workingArea.X + horzGap / 2,
                workingArea.Y + vertGap / 2,
                workingArea.Width - horzGap,
                workingArea.Height - vertGap),
            true);

        // add the gaps to make window bound
        var winBound = new Rectangle(
            viewerBound.X - horzGap / 2,
            viewerBound.Y - vertGap / 2,
            viewerBound.Width + horzGap,
            viewerBound.Height + vertGap);

        // check center window to screen option
        if (!Config.CenterWindowFit)
        {
            winBound.X = Left;
            winBound.Y = Top;
        }


        // set min size for window
        MinimumSize = new()
        {
            Width = horzGap + this.ScaleToDpi(50),
            Height = vertGap + this.ScaleToDpi(50),
        };

        // update window position and size
        SetBounds(winBound.X, winBound.Y, winBound.Width, winBound.Height, BoundsSpecified.All);

        if (resetZoomMode)
        {
            PicMain.SetZoomFactor(zoomFactor, false);
        }
    }


    /// <summary>
    /// Toggle framless mode
    /// </summary>
    public bool IG_ToggleFrameless(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableFrameless;
        Config.EnableFrameless = enable.Value;

        if (Config.EnableFrameless)
        {
            // exit full screen
            if (Config.EnableFullScreen)
            {
                IG_ToggleFullScreen(false, false);
            }
        }

        // set frameless mode
        FormBorderStyle = Config.EnableFrameless ? FormBorderStyle.None : FormBorderStyle.Sizable;

        // update menu item state
        MnuFrameless.Checked = Config.EnableFrameless;
        if (Config.EnableFrameless)
        {
            WindowApi.SetRoundCorner(Handle);
        }

        // update toolbar items state
        UpdateToolbarItemsState();

        if (showInAppMessage)
        {
            var langPath = $"{Name}.{nameof(MnuFrameless)}";

            if (Config.EnableFrameless)
            {
                PicMain.ShowMessage(
                    string.Format(Config.Language[$"{langPath}._EnableDescription"], MnuFrameless.ShortcutKeyDisplayString),
                    Config.Language[$"{langPath}._Enable"],
                    Config.InAppMessageDuration);
            }
            else
            {
                PicMain.ShowMessage("",
                    Config.Language[$"{langPath}._Disable"],
                    Config.InAppMessageDuration);
            }
        }

        return Config.EnableFrameless;
    }


    /// <summary>
    /// Toggles full screen mode.
    /// </summary>
    public bool IG_ToggleFullScreen(bool? enable = null, bool showInAppMessage = true)
    {
        enable ??= !Config.EnableFullScreen;
        Config.EnableFullScreen = enable.Value;

        if (Config.EnableFullScreen)
        {
            // exit full screen
            if (Config.EnableWindowFit)
            {
                IG_ToggleWindowFit(false, false);
            }

            // exit frameless
            if (Config.EnableFrameless)
            {
                IG_ToggleFrameless(false, false);
            }
        }

        // disable round corner in full screen
        WindowApi.SetRoundCorner(Handle, !Config.EnableFullScreen);

        SetFullScreenMode(
            enable: enable.Value,
            changeWindowState: true,
            hideToolbar: Config.HideToolbarInFullscreen,
            hideThumbnails: Config.HideThumbnailsInFullscreen);

        // update menu item state
        MnuFullScreen.Checked = Config.EnableFullScreen;

        // update toolbar items state
        UpdateToolbarItemsState();

        if (showInAppMessage)
        {
            var langPath = $"{Name}.{nameof(MnuFullScreen)}";

            if (Config.EnableFullScreen)
            {
                PicMain.ShowMessage(
                    string.Format(Config.Language[$"{langPath}._EnableDescription"], MnuFullScreen.ShortcutKeyDisplayString),
                    Config.Language[$"{langPath}._Enable"],
                    Config.InAppMessageDuration);
            }
            else
            {
                PicMain.ShowMessage("",
                    Config.Language[$"{langPath}._Disable"],
                    Config.InAppMessageDuration);
            }
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

            ResumeLayout(false);
            Visible = true;

            // disable free moving
            IG_SetWindowMoveable(false);
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

            ResumeLayout(false);

            // renable free moving
            IG_SetWindowMoveable(true);


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
    /// Toggles slideshow.
    /// </summary>
    public bool IG_ToggleSlideshow(bool? enable = null)
    {
        enable ??= !Config.EnableSlideshow;
        Config.EnableSlideshow = enable.Value;

        _ = SetSlideshowModeAsync(enable.Value);

        // update menu item state
        MnuSlideshow.Checked = Config.EnableSlideshow;

        // update toolbar items state
        UpdateToolbarItemsState();

        return Config.EnableSlideshow;
    }


    /// <summary>
    /// Enter or exit slideshow mode.
    /// </summary>
    public async Task SetSlideshowModeAsync(bool enable)
    {
        var tool = new IgTool()
        {
            ToolId = Constants.IGTOOL_SLIDESHOW,
            ToolName = "[Slideshow]",
            Executable = "igcmd.exe",
            Argument = $"{IgCommands.START_SLIDESHOW} {Constants.FILE_MACRO}",
            CanToggle = true,
        };
        var pipeName = $"{ImageGlassTool.PIPENAME_PREFIX}{tool.Executable}";


        if (enable)
        {
            // try to connect to slideshow tool
            if (await Local.OpenPipedToolAsync(tool) is not PipeServer toolServer)
            {
                SlideshowToolServer_ClientDisconnected(null, new DisconnectedEventArgs(pipeName));
                return;
            }


            Config.EnableSlideshow = true;
            toolServer.ClientDisconnected += SlideshowToolServer_ClientDisconnected;


            // send the list of images
            var data = new ImageListLoadedToolEventArgs()
            {
                Files = Local.Images.FileNames,
            };
            var jsonData = BHelper.ToJson(data);
            _ = toolServer.SendAsync(ToolServerMsgs.IMAGE_LIST_UPDATED, jsonData);


            // hide FrmMain
            SetFrmMainStateInSlideshow(Config.EnableSlideshow);

            // prevent system from entering sleep mode
            SysExecutionState.PreventSleep();
        }
        else
        {
            _ = Local.ClosePipedToolAsync(tool, (toolServer) =>
            {
                toolServer.ClientDisconnected -= SlideshowToolServer_ClientDisconnected;
            });


            SlideshowToolServer_ClientDisconnected(null, new DisconnectedEventArgs(pipeName));
        }
    }

    private void SlideshowToolServer_ClientDisconnected(object? sender, DisconnectedEventArgs e)
    {
        if (InvokeRequired)
        {
            Invoke(SlideshowToolServer_ClientDisconnected, sender, e);
            return;
        }

        Config.EnableSlideshow = false;

        // show FrmMain
        SetFrmMainStateInSlideshow(Config.EnableSlideshow);

        // allow system to enter sleep mode
        SysExecutionState.AllowSleep();

        // update menu item state
        MnuSlideshow.Checked = Config.EnableSlideshow;

        // update toolbar items state
        UpdateToolbarItemsState();
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
    public void IG_SetWindowMoveable(bool? enable = null)
    {
        enable ??= true;
        _movableForm.Key = Keys.ShiftKey | Keys.Shift;

        if (enable.Value)
        {
            _movableForm.Enable();
            _movableForm.Enable(Toolbar, PicMain);
        }
        else
        {
            _movableForm.Disable();
            _movableForm.Disable(Toolbar, PicMain);
        }

    }


    /// <summary>
    /// Flips the viewing image.
    /// </summary>
    /// <param name="options"></param>
    public void IG_FlipImage(FlipOptions options)
    {
        if (PicMain.Source == ImageSource.Null || Local.IsBusy) return;

        // update flip changes
        if (PicMain.FlipImage(options))
        {
            if (options.HasFlag(FlipOptions.Horizontal))
            {
                if (Local.ImageTransform.Flips.HasFlag(FlipOptions.Horizontal))
                {
                    Local.ImageTransform.Flips ^= FlipOptions.Horizontal;
                }
                else
                {
                    Local.ImageTransform.Flips |= FlipOptions.Horizontal;
                }
            }

            if (options.HasFlag(FlipOptions.Vertical))
            {
                if (Local.ImageTransform.Flips.HasFlag(FlipOptions.Vertical))
                {
                    Local.ImageTransform.Flips ^= FlipOptions.Vertical;
                }
                else
                {
                    Local.ImageTransform.Flips |= FlipOptions.Vertical;
                }
            }
        }
        else
        {
            PicMain.ShowMessage(
                text: Config.Language["_._NotSupported._Transformation"],
                heading: Config.Language["_._NotSupported"],
                durationMs: Config.InAppMessageDuration);
        }
    }


    /// <summary>
    /// Rotates the viewing image.
    /// </summary>
    public void IG_Rotate(RotateOption option)
    {
        if (PicMain.Source == ImageSource.Null || Local.IsBusy) return;

        var degree = option == RotateOption.Left ? -90 : 90;

        // update rotation changes
        if (PicMain.RotateImage(degree))
        {
            var currentRotation = Local.ImageTransform.Rotation + degree;
            if (Math.Abs(currentRotation) >= 360)
            {
                currentRotation = currentRotation % 360;
            }

            Local.ImageTransform.Rotation = currentRotation;
        }
        else
        {
            PicMain.ShowMessage(
                text: Config.Language["_._NotSupported._Transformation"],
                heading: Config.Language["_._NotSupported"],
                durationMs: Config.InAppMessageDuration);
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
    /// Toggle tool window
    /// </summary>
    private void ToggleTool(ToolForm form, bool visible)
    {
        var toolForm = Local.Tools[form.Name];
        toolForm.ToolFormClosing -= ToolForm_ToolFormClosing;
        toolForm.ToolFormClosing += ToolForm_ToolFormClosing;

        if (visible)
        {
            // set default location offset on the parent form
            var padding = DpiApi.Transform(10);
            var x = padding;
            var y = PicMain.Top + padding;
            var loc = PointToScreen(new Point(x, y));
            loc.Offset(-Left, -Top);

            var totolHeight = 0;
            foreach (var formName in Local.Tools.Keys)
            {
                totolHeight += Local.Tools[formName].Height + padding;
            }

            var workspaceHeight = Screen.FromControl(this).WorkingArea.Height - padding;
            var column = totolHeight / workspaceHeight;
            loc.X += column * Local.Tools[form.Name].Width;
            loc.Y += totolHeight - Local.Tools[form.Name].Height;

            Local.Tools[form.Name].InitLocation = loc;
            Local.Tools[form.Name].Show();
        }
        else
        {
            Local.Tools[form.Name].Close();
        }
    }

    private void ToolForm_ToolFormClosing(ToolFormClosingEventArgs e)
    {
        if (e.Name == nameof(FrmCrop))
        {
            // update menu item state
            MnuCropTool.Checked =
                // set selection mode
                PicMain.EnableSelection = false;
        }
        else if (e.Name == nameof(FrmColorPicker))
        {
            MnuColorPicker.Checked = false;
        }


        // update toolbar items state
        UpdateToolbarItemsState();
    }


    /// <summary>
    /// Toggles crop tool.
    /// </summary>
    public bool IG_ToggleCropTool(bool? visible = null)
    {
        visible ??= MnuCropTool.Checked;

        // update menu item state
        MnuCropTool.Checked =
            // set selection mode
            PicMain.EnableSelection = visible.Value;

        // update toolbar items state
        UpdateToolbarItemsState();

        Local.Tools.TryGetValue(nameof(FrmCrop), out var frm);
        if (frm == null)
        {
            Local.Tools.TryAdd(nameof(FrmCrop), new FrmCrop(this));
        }
        else if (frm.IsDisposed)
        {
            Local.Tools[nameof(FrmCrop)] = new FrmCrop(this);
        }

        ToggleTool(Local.Tools[nameof(FrmCrop)], visible.Value);

        return visible.Value;
    }


    /// <summary>
    /// Toggles Color picker tool.
    /// </summary>
    public bool IG_ToggleColorPicker(bool? visible = null)
    {
        visible ??= MnuColorPicker.Checked;

        // update menu item state
        MnuColorPicker.Checked = visible.Value;

        // update toolbar items state
        UpdateToolbarItemsState();

        Local.Tools.TryGetValue(nameof(FrmColorPicker), out var frm);
        if (frm == null)
        {
            Local.Tools.TryAdd(nameof(FrmColorPicker), new FrmColorPicker(this));
        }
        else if (frm.IsDisposed)
        {
            Local.Tools[nameof(FrmColorPicker)] = new FrmColorPicker(this);
        }

        ToggleTool(Local.Tools[nameof(FrmColorPicker)], visible.Value);

        return visible.Value;
    }

}


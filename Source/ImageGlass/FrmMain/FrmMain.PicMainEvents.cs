/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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

using Cysharp.Text;
using D2Phap;
using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Settings;
using ImageGlass.Viewer;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.PicMainEvents contains events of PicMain       *
 * ****************************************************** */

public partial class FrmMain
{
    private void PicMain_DragEnter(object sender, DragEventArgs e)
    {
        e.DropImageType = DropImageType.Link;
        e.Message = ZString.Format(Config.Language[$"{Name}._OpenWith"], "%1");
        e.MessageReplacementToken = App.AppName;
    }


    private void PicMain_DragLeave(object? sender, EventArgs e)
    {
        if (PicMain.ComparisonMode)
        {
            PicMain.ClearComparisonDropHighlight();
        }
    }


    private void PicMain_DragOver(object? sender, DragEventArgs e)
    {
        try
        {
            if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var data = e.Data.GetData(DataFormats.FileDrop, false);

            // observed: null w/ long path and long path support not enabled
            if (data == null)
                return;

            if (data is not string[] paths) return;
            var filePath = paths[0];

            if (PicMain.ComparisonMode && paths.Length == 1)
            {
                var targetPane = PicMain.GetDropTargetPane(new Point(e.X, e.Y));
                PicMain.SetComparisonDropHighlight(targetPane);
            }

            // KBR 20190617 Fix observed issue: dragging from CD/DVD would fail because
            // we set the drag effect to Move, which is not allowed
            // Drag file from DESKTOP to APP
            if (Local.Images.IndexOf(filePath) == -1
                && (e.AllowedEffect & DragDropEffects.Move) != 0)
            {
                e.Effect = DragDropEffects.Move;
            }
            // Drag file from APP to DESKTOP
            else
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        catch
        {
            // observed: exception with a long path and long path support enabled
        }
    }


    private void PicMain_DragDrop(object? sender, DragEventArgs e)
    {
        _ = HandlePicMainDragDropAsync(e);
    }


    private async Task HandlePicMainDragDropAsync(DragEventArgs e)
    {
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        if (e.Data.GetData(DataFormats.FileDrop, false) is not string[] paths) return;

        if (PicMain.ComparisonMode && paths.Length == 1)
        {
            var clientPoint = PicMain.PointToClient(new Point(e.X, e.Y));
            var sliderX = PicMain.ComparisonSliderScreenX;
            var filePath = BHelper.ResolvePath(paths[0]);

            PicMain.ClearComparisonDropHighlight();

            if (clientPoint.X < sliderX)
            {
                PrepareLoading(filePath, false);
            }
            else
            {
                await LoadComparisonImageAsync(filePath);
            }
            return;
        }

        if (paths.Length > 1)
        {
            await PrepareLoadingAsync(paths);
            return;
        }


        var resolvedPath = BHelper.ResolvePath(paths[0]);
        var imageIndex = Local.Images.IndexOf(resolvedPath);


        // get foreground shell
        if (Config.ShouldUseExplorerSortOrder)
        {
            var shell = new EggShell();
            Program.ForegroundShell = shell.GetForegroundWindowView();
        }

        // save init input path
        Program.UpdateInputImagePath(resolvedPath);


        // The file is located another folder, load the entire folder
        if (imageIndex == -1 || Program.CanUseForegroundShell())
        {
            PrepareLoading(resolvedPath, false);
        }
        // The file is in current folder AND it is the viewing image
        else if (Local.CurrentIndex == imageIndex)
        {
            //do nothing
        }
        // The file is in current folder AND it is NOT the viewing image
        else
        {
            Local.CurrentIndex = imageIndex;
            _ = ViewNextCancellableAsync(0);
        }
    }


    private void PicMain_KeyDown(object? sender, KeyEventArgs e)
    {
        var hotkey = new Hotkey(e.KeyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // zoom in
        if (actions.Contains(nameof(MnuZoomIn))
            || actions.Contains(nameof(IG_ZoomIn)))
        {
            if (PicMain.ZoomLevels.Length > 0)
            {
                PicMain.ZoomIn();
            }
            else
            {
                PicMain.StartAnimation(AnimationSource.ZoomIn);
            }

            return;
        }

        // zoom out
        if (actions.Contains(nameof(MnuZoomOut))
            || actions.Contains(nameof(IG_ZoomOut)))
        {
            if (PicMain.ZoomLevels.Length > 0)
            {
                PicMain.ZoomOut();
            }
            else
            {
                PicMain.StartAnimation(AnimationSource.ZoomOut);
            }

            return;
        }

        // pan left
        if (actions.Contains(nameof(MnuPanLeft))
            || actions.Contains(nameof(IG_PanLeft)))
        {
            PicMain.StartAnimation(AnimationSource.PanLeft);
            return;
        }

        // pan right
        if (actions.Contains(nameof(MnuPanRight))
            || actions.Contains(nameof(IG_PanRight)))
        {
            PicMain.StartAnimation(AnimationSource.PanRight);
            return;
        }

        // pan up
        if (actions.Contains(nameof(MnuPanUp))
            || actions.Contains(nameof(IG_PanUp)))
        {
            PicMain.StartAnimation(AnimationSource.PanUp);
            return;
        }

        // pan down
        if (actions.Contains(nameof(MnuPanDown))
            || actions.Contains(nameof(IG_PanDown)))
        {
            PicMain.StartAnimation(AnimationSource.PanDown);
            return;
        }
    }


    private void PicMain_KeyUp(object? sender, KeyEventArgs e)
    {
        // zooming
        if (PicMain.AnimationSource.HasFlag(AnimationSource.ZoomIn))
        {
            PicMain.StopAnimation(AnimationSource.ZoomIn);
        }

        if (PicMain.AnimationSource.HasFlag(AnimationSource.ZoomOut))
        {
            PicMain.StopAnimation(AnimationSource.ZoomOut);
        }

        // panning
        if (PicMain.AnimationSource.HasFlag(AnimationSource.PanLeft))
        {
            PicMain.StopAnimation(AnimationSource.PanLeft);
        }

        if (PicMain.AnimationSource.HasFlag(AnimationSource.PanRight))
        {
            PicMain.StopAnimation(AnimationSource.PanRight);
        }

        if (PicMain.AnimationSource.HasFlag(AnimationSource.PanUp))
        {
            PicMain.StopAnimation(AnimationSource.PanUp);
        }

        if (PicMain.AnimationSource.HasFlag(AnimationSource.PanDown))
        {
            PicMain.StopAnimation(AnimationSource.PanDown);
        }
    }


    private void PicMain_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ExecuteMouseAction(MouseClickEvent.LeftClick);
        }
        else if (e.Button == MouseButtons.Right)
        {
            var actionExecutable = ExecuteMouseAction(MouseClickEvent.RightClick);

            // handle right-click action for webview2
            if (PicMain.UseWebview2)
            {
                var point = this.PointToScreen(e.Location);
                point.X += PicMain.Left;
                point.Y += PicMain.Top;

                if (actionExecutable == nameof(IG_OpenMainMenu))
                {
                    MnuMain.Show(point);
                }
                else if (string.IsNullOrEmpty(actionExecutable) || actionExecutable == nameof(IG_OpenContextMenu))
                {
                    MnuContext.Show(point);
                }
            }
        }
        else if (e.Button == MouseButtons.Middle)
        {
            ExecuteMouseAction(MouseClickEvent.WheelClick);
        }
        else if (e.Button == MouseButtons.XButton1)
        {
            ExecuteMouseAction(MouseClickEvent.XButton1Click);
        }
        else if (e.Button == MouseButtons.XButton2)
        {
            ExecuteMouseAction(MouseClickEvent.XButton2Click);
        }
    }


    private void PicMain_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ExecuteMouseAction(MouseClickEvent.LeftDoubleClick);
        }
        else if (e.Button == MouseButtons.Right)
        {
            ExecuteMouseAction(MouseClickEvent.RightDoubleClick);
        }
    }


    private void PicMain_MouseWheel(object? sender, MouseEventArgs e)
    {
        MouseWheelAction action;
        var modifiers = PicMain.UseWebview2 ? PicMain.Web2LastMouseWheelModifiers : ModifierKeys;

        MouseWheelEvent eventType;
        if (modifiers.HasFlag(Keys.Control))
        {
            eventType = MouseWheelEvent.CtrlAndScroll;
        }
        else if (modifiers.HasFlag(Keys.Shift))
        {
            eventType = MouseWheelEvent.ShiftAndScroll;
        }
        else if (modifiers.HasFlag(Keys.Alt))
        {
            eventType = MouseWheelEvent.AltAndScroll;
        }
        else
        {
            eventType = MouseWheelEvent.Scroll;
        }

        // Get mouse wheel action
        #region Get mouse wheel action

        // get user-defined mouse wheel action
        if (Config.MouseWheelActions.TryGetValue(eventType, out MouseWheelAction value))
        {
            action = value;
        }
        // if not found, use the defaut mouse wheel action
        else
        {
            switch (eventType)
            {
                case MouseWheelEvent.Scroll:
                    action = MouseWheelAction.Zoom;
                    break;
                case MouseWheelEvent.CtrlAndScroll:
                    action = MouseWheelAction.PanVertically;
                    break;
                case MouseWheelEvent.ShiftAndScroll:
                    action = MouseWheelAction.PanHorizontally;
                    break;
                case MouseWheelEvent.AltAndScroll:
                    action = MouseWheelAction.BrowseImages;
                    break;
                default:
                    action = MouseWheelAction.DoNothing;
                    break;
            }
        }
        #endregion


        // Run mouse wheel action
        #region Run mouse wheel action

        if (action == MouseWheelAction.Zoom)
        {
            PicMain.ZoomByDeltaToPoint(e.Delta, e.Location);
        }
        else if (action == MouseWheelAction.PanVertically)
        {
            if (e.Delta > 0)
            {
                PicMain.PanUp(e.Delta + PicMain.PanDistance / 4);
            }
            else
            {
                PicMain.PanDown(Math.Abs(e.Delta) + PicMain.PanDistance / 4);
            }
        }
        else if (action == MouseWheelAction.PanHorizontally)
        {
            if (e.Delta > 0)
            {
                PicMain.PanLeft(e.Delta + PicMain.PanDistance / 4);
            }
            else
            {
                PicMain.PanRight(Math.Abs(e.Delta) + PicMain.PanDistance / 4);
            }
        }
        else if (action == MouseWheelAction.BrowseImages)
        {
            var paneAtCursor = PicMain.ComparisonMode
                ? PicMain.GetComparisonPaneAt(e.Location)
                : ImageGlass.Viewer.ComparisonPaneHover.None;

            if (paneAtCursor == ImageGlass.Viewer.ComparisonPaneHover.RightPane)
            {
                var delta = e.Delta < 0 ? 1 : -1;
                _ = CycleComparisonImageAsync(delta);
            }
            else
            {
                if (e.Delta < 0)
                {
                    IG_ViewImage(1);
                }
                else
                {
                    IG_ViewImage(-1);
                }
            }
        }
        #endregion
    }


    private void PicMain_OnNavLeftClicked(object? sender, MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(-1);
    }


    private void PicMain_OnNavRightClicked(object? sender, MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(1);
    }


    private async void PicMain_OnMotionBtnClicked(object sender, MouseEventArgs e)
    {
        var img = await Local.Images.GetAsync(Local.CurrentIndex);
        await img.OpenEmbeddedVideoFileAsync();
    }


    private void PicMain_OnZoomChanged(object? sender, ZoomEventArgs e)
    {
        // Handle window fit after zoom change
        if (Config.EnableWindowFit
            && !e.IsPreviewingImage
            && e.ChangeSource != ZoomChangeSource.SizeChanged
            && (e.IsManualZoom || e.IsZoomModeChange))
        {
            FitWindowToImage(e.ChangeSource == ZoomChangeSource.ZoomMode);
        }

        LoadImageInfo(ImageInfoUpdateTypes.Zoom);
    }


    private void PicMain_Web2NavigationCompleted(object sender, EventArgs e)
    {
        var langJson = BHelper.ToJson(Config.Language);
        _ = PicMain.LoadWeb2LanguageAsync(langJson);
    }


    private void PicMain_Web2PointerDown(object sender, MouseEventArgs e)
    {
        // make sure all menus closed when mouse clicked
        MnuMain.Close();
        MnuContext.Close();
        MnuSubMenu.Close();
    }


    private void PicMain_Web2KeyDown(object sender, KeyEventArgs e)
    {
        // pass keydown to FrmMain
        this.OnKeyDown(e);
    }


    private void PicMain_Web2KeyUp(object sender, KeyEventArgs e)
    {
        // pass keyup to FrmMain
        this.OnKeyUp(e);
    }


    // Comparison mode events
    #region Comparison mode events

    private void PicMain_ComparisonPaneClicked(object? sender, ComparisonPaneClickedEventArgs e)
    {
    }


    private void PicMain_ComparisonPaneFileDrop(object? sender, ComparisonPaneDropEventArgs e)
    {
        if (e.Pane == ComparisonPaneHover.RightPane)
        {
            _ = LoadComparisonImageAsync(e.FilePath);
        }
        else if (e.Pane == ComparisonPaneHover.LeftPane)
        {
            PrepareLoading(e.FilePath, false);
        }
    }


    /// <summary>
    /// Opens a file picker to select a comparison image.
    /// </summary>
    private async Task OpenComparisonImageAsync()
    {
        using var sb = ZString.CreateStringBuilder();
        foreach (var ext in Config.FileFormats)
        {
            sb.Append($"*{ext};");
        }

        using var o = new OpenFileDialog()
        {
            Title = Config.Language[$"FrmCompare.BtnSelectImage._DialogTitle"],
            Filter = Config.Language[$"{Name}._OpenFileDialog"] + "|" + sb.ToString(),
            CheckFileExists = true,
            RestoreDirectory = true,
        };

        // Set initial directory based on current image
        var currentPath = Local.Images.GetFilePath(Local.CurrentIndex);
        if (!string.IsNullOrEmpty(currentPath))
        {
            o.InitialDirectory = Path.GetDirectoryName(currentPath);
        }

        if (o.ShowDialog() == DialogResult.OK)
        {
            await LoadComparisonImageAsync(o.FileName);
        }
    }


    /// <summary>
    /// Loads an image file as the comparison image.
    /// </summary>
    private async Task LoadComparisonImageAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        _comparisonLoadCts?.Cancel();
        _comparisonLoadCts?.Dispose();
        _comparisonLoadCts = new CancellationTokenSource();
        var token = _comparisonLoadCts.Token;

        try
        {
            Gallery.ComparisonImagePath = filePath;
            Gallery.Refresh(true, false);

            if (ShouldUseWeb2ForComparison(filePath))
            {
                await PicMain.SetWeb2ComparisonModeAsync(true, token);
                if (token.IsCancellationRequested) return;

                var leftImagePath = Local.Images.GetFilePath(Local.CurrentIndex);
                var leftHtml = await ViewerCanvas.ReadImageAsHtmlAsync(leftImagePath, token);
                if (token.IsCancellationRequested) return;

                var rightHtml = await ViewerCanvas.ReadImageAsHtmlAsync(filePath, token);
                if (token.IsCancellationRequested) return;

                await PicMain.SetWeb2ComparisonImagesAsync(
                    leftImagePath,
                    leftHtml,
                    filePath,
                    rightHtml,
                    token);
            }
            else
            {
                var imgData = await PhotoCodec.LoadAsync(filePath, new CodecReadOptions()
                {
                    ColorProfileName = Config.ColorProfile,
                    FirstFrameOnly = true,
                }, null, token);

                if (token.IsCancellationRequested) return;

                if (imgData != null)
                {
                    PicMain.SetCompareImage(imgData, filePath);
                }

                await PicMain.SetWeb2ComparisonModeAsync(false, token);
            }

            if (token.IsCancellationRequested) return;

            PicMain.ShowMessage(
                Path.GetFileName(filePath),
                heading: Config.Language[$"{Name}.{nameof(MnuCompareTool)}"],
                durationMs: Config.InAppMessageDuration);

            LoadImageInfo();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            PicMain.ShowMessage(
                ex.Message,
                heading: Config.Language[$"{Name}.{nameof(MnuCompareTool)}"],
                durationMs: Config.InAppMessageDuration);
        }
    }


    /// <summary>
    /// Cycles the comparison image by the given delta (1 for next, -1 for previous).
    /// </summary>
    private async Task CycleComparisonImageAsync(int delta)
    {
        if (Local.Images.Length == 0) return;

        var currentCompareIndex = -1;
        var comparePath = PicMain.CompareImagePath;

        if (!string.IsNullOrEmpty(comparePath))
        {
            for (var i = 0; i < Local.Images.Length; i++)
            {
                if (string.Equals(Local.Images.GetFilePath(i), comparePath, StringComparison.OrdinalIgnoreCase))
                {
                    currentCompareIndex = i;
                    break;
                }
            }
        }

        if (currentCompareIndex < 0)
        {
            currentCompareIndex = Local.CurrentIndex;
        }

        // Calculate new index with wrapping
        var newIndex = currentCompareIndex + delta;
        if (newIndex >= Local.Images.Length) newIndex = 0;
        if (newIndex < 0) newIndex = Local.Images.Length - 1;

        if (newIndex == Local.CurrentIndex)
        {
            newIndex += delta;
            if (newIndex >= Local.Images.Length) newIndex = 0;
            if (newIndex < 0) newIndex = Local.Images.Length - 1;
        }

        var newPath = Local.Images.GetFilePath(newIndex);
        if (!string.IsNullOrEmpty(newPath))
        {
            await LoadComparisonImageAsync(newPath);
        }
    }

    #endregion // Comparison mode events

}

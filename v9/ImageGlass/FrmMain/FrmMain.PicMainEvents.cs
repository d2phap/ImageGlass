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
using ImageGlass.Views;

namespace ImageGlass;


/* ****************************************************** *
 * FrmMain.PicMainEvents contains events of PicMain       *
 * ****************************************************** */

public partial class FrmMain
{
    private void PicMain_DragOver(object sender, DragEventArgs e)
    {
        try
        {
            if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var dataTest = e.Data.GetData(DataFormats.FileDrop, false);

            // observed: null w/ long path and long path support not enabled
            if (dataTest == null)
                return;

            var filePath = ((string[])dataTest)[0];

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


    private void PicMain_DragDrop(object sender, DragEventArgs e)
    {
        _ = HandlePicMainDragDropAsync(e);
    }

    private async Task HandlePicMainDragDropAsync(DragEventArgs e)
    {
        // Drag file from DESKTOP to APP
        if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var filePaths = (string[]?)e.Data.GetData(DataFormats.FileDrop, false);

        if (filePaths.Length > 1)
        {
            await PrepareLoadingAsync(filePaths);

            return;
        }

        var filePath = BHelper.ResolvePath(filePaths[0]);
        var imageIndex = Local.Images.IndexOf(filePath);

        // The file is located another folder, load the entire folder
        if (imageIndex == -1)
        {
            PrepareLoading(filePath);
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


    private void PicMain_KeyDown(object? sender, KeyEventArgs e)
    {
        var hotkey = new Hotkey(e.KeyData);
        var actions = Config.GetHotkeyActions(CurrentMenuHotkeys, hotkey);

        // zoom in
        if (actions.Contains(nameof(MnuZoomIn))
            || actions.Contains(nameof(IG_ZoomIn)))
        {
            PicMain.StartAnimation(AnimationSource.ZoomIn);
            return;
        }

        // zoom out
        if (actions.Contains(nameof(MnuZoomOut))
            || actions.Contains(nameof(IG_ZoomOut)))
        {
            PicMain.StartAnimation(AnimationSource.ZoomOut);
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


    private void PicMain_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ExecuteMouseAction(MouseClickEvent.LeftClick);
        }
        else if (e.Button == MouseButtons.Right)
        {
            ExecuteMouseAction(MouseClickEvent.RightClick);
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
        else if (e.Button == MouseButtons.Middle)
        {
            ExecuteMouseAction(MouseClickEvent.WheelDoubleClick);
        }
        else if (e.Button == MouseButtons.XButton1)
        {
            ExecuteMouseAction(MouseClickEvent.XButton1DoubleClick);
        }
        else if (e.Button == MouseButtons.XButton2)
        {
            ExecuteMouseAction(MouseClickEvent.XButton2DoubleClick);
        }
    }


    private void PicMain_MouseWheel(object? sender, MouseEventArgs e)
    {
        MouseWheelAction action;

        var eventType = ModifierKeys switch
        {
            Keys.Control => MouseWheelEvent.PressCtrlAndScroll,
            Keys.Shift => MouseWheelEvent.PressShiftAndScroll,
            Keys.Alt => MouseWheelEvent.PressAltAndScroll,
            _ => MouseWheelEvent.Scroll,
        };


        // Get mouse wheel action
        #region Get mouse wheel action

        // get user-defined mouse wheel action
        if (Config.MouseWheelActions.ContainsKey(eventType))
        {
            action = Config.MouseWheelActions[eventType];
        }
        // if not found, use the defaut mouse wheel action
        else
        {
            switch (eventType)
            {
                case MouseWheelEvent.Scroll:
                    action = MouseWheelAction.Zoom;
                    break;
                case MouseWheelEvent.PressCtrlAndScroll:
                    action = MouseWheelAction.PanVertically;
                    break;
                case MouseWheelEvent.PressShiftAndScroll:
                    action = MouseWheelAction.PanHorizontally;
                    break;
                case MouseWheelEvent.PressAltAndScroll:
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
            if (e.Delta < 0)
            {
                IG_ViewNextImage();
            }
            else
            {
                IG_ViewPreviousImage();
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

    private void PicMain_OnZoomChanged(object? sender, ZoomEventArgs e)
    {
        // Handle window fit after zoom change
        if (Config.EnableWindowFit && (e.IsManualZoom || e.IsZoomModeChange))
        {
            FitWindowToImage(false);
        }

        LoadImageInfo(ImageInfoUpdateTypes.Zoom);
    }

}

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

using ImageGlass.Base;
using ImageGlass.Settings;

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

        var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

        if (filePaths.Length > 1)
        {
            await PrepareLoadingAsync(filePaths);

            return;
        }

        var filePath = Helpers.ResolvePath(filePaths[0]);
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


    private void PicMain_Click(object sender, EventArgs e)
    {
        if (Config.EnableImageFocusMode)
        {
            PicMain.Focus();
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
                    action = MouseWheelAction.ScrollVertically;
                    break;
                case MouseWheelEvent.PressShiftAndScroll:
                    action = MouseWheelAction.ScrollHorizontally;
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
            PicMain.ZoomToPoint(e.Delta, e.Location);
        }
        else if (action == MouseWheelAction.ScrollVertically)
        {
            if (e.Delta > 0)
            {
                PicMain.PanUp(e.Delta + PicMain.PanSpeed / 4);
            }
            else
            {
                PicMain.PanDown(Math.Abs(e.Delta) + PicMain.PanSpeed / 4);
            }
        }
        else if (action == MouseWheelAction.ScrollHorizontally)
        {
            if (e.Delta > 0)
            {
                PicMain.PanLeft(e.Delta + PicMain.PanSpeed / 4);
            }
            else
            {
                PicMain.PanRight(Math.Abs(e.Delta) + PicMain.PanSpeed / 4);
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


    private void PicMain_OnNavLeftClicked(MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(-1);
    }

    private void PicMain_OnNavRightClicked(MouseEventArgs e)
    {
        _ = ViewNextCancellableAsync(1);
    }

    private void PicMain_OnZoomChanged(PhotoBox.ZoomEventArgs e)
    {
        UpdateImageInfo(ImageInfoUpdateTypes.Zoom);
    }

}

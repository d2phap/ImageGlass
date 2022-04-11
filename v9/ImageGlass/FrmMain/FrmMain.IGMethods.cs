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
using ImageGlass.Settings;

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
    private void IG_OpenFile(string? args = null)
    {
        OpenFilePicker();
    }

    private void IG_Refresh(string? args = null)
    {
        PicMain.Refresh();
    }

    private void IG_Reload(string? args = null)
    {
        _ = ViewNextCancellableAsync(0, isSkipCache: true);
    }

    private void IG_ReloadList(string? args = null)
    {
        _ = LoadImageListAsync(Local.Images.DistinctDirs, Local.Images.GetFileName(Local.CurrentIndex));
    }

    private void IG_ViewPreviousImage(string? args = null)
    {
        _ = ViewNextCancellableAsync(-1);
    }

    private void IG_ViewNextImage(string? args = null)
    {
        _ = ViewNextCancellableAsync(1);
    }

    private void IG_GoTo(string? indexStr = null)
    {
        if (int.TryParse(indexStr, out int index))
        {
            GoToImageAsync(index);
        }
    }

    private void IG_GoToFirst(string? args = null)
    {
        GoToImageAsync(0);
    }

    private void IG_GoToLast(string? args = null)
    {
        GoToImageAsync(Local.Images.Length - 1);
    }

    private void IG_ZoomIn(string? args = null)
    {
        PicMain.ZoomIn();
    }

    private void IG_ZoomOut(string? args = null)
    {
        PicMain.ZoomOut();
    }

    private void IG_SetZoom(string? zoomFactor = null)
    {
        if (float.TryParse(zoomFactor, out float factor))
        {
            PicMain.ZoomFactor = factor;
        }
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
    }


    private bool IG_ToggleToolbar(string? args = null)
    {
        Config.IsShowToolbar = !Config.IsShowToolbar;

        // Gallery bar
        Toolbar.Visible = Config.IsShowToolbar;

        return Config.IsShowToolbar;
    }


    private bool IG_ToggleGallery(string? args = null)
    {
        Config.IsShowThumbnail = !Config.IsShowThumbnail;

        // Gallery bar
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;

        return Config.IsShowCheckerBoard;
    }


    private bool IG_ToggleCheckerboard(string? args = null)
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


    private bool IG_ToggleTopMost(string? args = null)
    {
        Config.IsWindowAlwaysOnTop = !Config.IsWindowAlwaysOnTop;

        // Gallery bar
        TopMost = Config.IsWindowAlwaysOnTop;

        return Config.IsWindowAlwaysOnTop;
    }


}


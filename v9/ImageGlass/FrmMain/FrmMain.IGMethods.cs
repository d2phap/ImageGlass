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
    private void IG_OpenFile(string args)
    {
        OpenFilePicker();
    }

    private void IG_ViewPreviousImage(string args)
    {
        _ = ViewNextCancellableAsync(-1);
    }

    private void IG_ViewNextImage(string args)
    {
        _ = ViewNextCancellableAsync(1);
    }

    private void IG_SetZoomMode(string mode)
    {
        Config.ZoomMode = Helpers.ParseEnum<ZoomMode>(mode);

        if (PicBox.ZoomMode == Config.ZoomMode)
        {
            PicBox.Refresh();
        }
        else
        {
            PicBox.ZoomMode = Config.ZoomMode;
        }
    }

    private bool IG_ToggleCheckerboard(string args)
    {
        Config.IsShowCheckerBoard = !Config.IsShowCheckerBoard;

        if (Config.IsShowCheckerBoard)
        {
            if (Config.IsShowCheckerboardOnlyImageRegion)
            {
                PicBox.CheckerboardMode = CheckerboardMode.Image;
            }
            else
            {
                PicBox.CheckerboardMode = CheckerboardMode.Client;
            }
        }
        else
        {
            PicBox.CheckerboardMode = CheckerboardMode.None;
        }

        return Config.IsShowCheckerBoard;
    }


    private bool IG_ToggleGallery(string args)
    {
        Config.IsShowThumbnail = !Config.IsShowThumbnail;

        // Gallery bar
        Sp1.Panel2Collapsed = !Config.IsShowThumbnail;

        return Config.IsShowCheckerBoard;
    }
    
}


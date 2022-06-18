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
using ImageGlass.Base.Photoing.Codecs;
using ImageGlass.Base.Services;
using ImageGlass.Settings;

namespace ImageGlass;

internal class Local
{
    #region Public events

    /// <summary>
    /// Occurs when <see cref="Images"/> is loaded.
    /// </summary>
    public static event ImageListLoadedHandler? OnImageListLoaded;
    public delegate void ImageListLoadedHandler(ImageListLoadedEventArgs e);

    /// <summary>
    /// Occurs when the requested image is being loaded.
    /// </summary>
    public static event ImageLoadingHandler? OnImageLoading;
    public delegate void ImageLoadingHandler(ImageLoadingEventArgs e);

    /// <summary>
    /// Occurs when the requested image is loaded.
    /// </summary>
    public static event ImageLoadedHandler? OnImageLoaded;
    public delegate void ImageLoadedHandler(ImageLoadedEventArgs e);

    /// <summary>
    /// Occurs when the first image is reached.
    /// </summary>
    public static event FirstImageReachedHandler? OnFirstImageReached;
    public delegate void FirstImageReachedHandler();

    /// <summary>
    /// Occurs when the last image is reached.
    /// </summary>
    public static event LastImageReachedHandler? OnLastImageReached;
    public delegate void LastImageReachedHandler();

    /// <summary>
    /// Occurs when the FrmMain's state needs to be updated.
    /// </summary>
    public static event FrmMainUpdateRequestedHandler? OnRequestUpdateFrmMain;
    public delegate void FrmMainUpdateRequestedHandler(ForceUpdateAction e);


    /// <summary>
    /// Raise <see cref="OnImageListLoaded"/> event.
    /// </summary>
    public static void RaiseImageListLoadedEvent(ImageListLoadedEventArgs e)
    {
        OnImageListLoaded?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="OnImageLoading"/> event.
    /// </summary>
    public static void RaiseImageLoadingEvent(ImageLoadingEventArgs e)
    {
        OnImageLoading?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="OnImageLoaded"/> event.
    /// </summary>
    public static void RaiseImageLoadedEvent(ImageLoadedEventArgs e)
    {
        OnImageLoaded?.Invoke(e);
    }


    /// <summary>
    /// Raise <see cref="OnFirstImageReached"/> event.
    /// </summary>
    public static void RaiseFirstImageReachedEvent()
    {
        OnFirstImageReached?.Invoke();
    }


    /// <summary>
    /// Raise <see cref="OnLastImageReached"/> event.
    /// </summary>
    public static void RaiseLastImageReachedEvent()
    {
        OnLastImageReached?.Invoke();
    }


    /// <summary>
    /// Raise <see cref="OnRequestUpdateFrmMain"/> event.
    /// </summary>
    /// <param name="e"></param>
    public static void UpdateFrmMain(ForceUpdateAction e)
    {
        OnRequestUpdateFrmMain?.Invoke(e);
    }


    #endregion


    #region Public properties

    public static IgMetadata? Metadata { get; set; }

    /// <summary>
    /// Gets, sets app state
    /// </summary>
    public static bool IsBusy { get; set; } = false;

    /// <summary>
    /// Gets, sets images list
    /// </summary>
    public static ImageBooster Images { get; set; } = new();

    /// <summary>
    /// Gets, sets index of the viewing image
    /// </summary>
    public static int CurrentIndex { get; set; } = -1;


    /// <summary>
    /// Gets, sets the value if the current image is error
    /// </summary>
    public static bool IsImageError { get; set; } = false;

    /// <summary>
    /// <para>The current "initial" path (file or dir) we're viewing. Used when the user changes the sort settings: we need to rebuild the image list, but otherwise we don't know what image/folder we started with.</para>
    /// <para>Here's what happened: I opened a folder with subfolders (child folders enabled). I was going through the images, and decided I wanted to change the sort order. Since the _current_ image was in a sub-folder, after a rescan of the image list, only the _sub_-folders images were re-loaded!</para>
    /// <para>But if we reload the list using the original image, then the original folder's images, and the sub-folders, are reloaded.</para>
    /// </summary>
    public static string InitialInputPath { get; set; } = "";

    /// <summary>
    /// The 'current' image sorting order. A reconciliation between the user's Settings selection and the sorting order from Windows Explorer, to be used to sort the active image list.
    /// </summary>
    public static ImageOrderBy ActiveImageLoadingOrder { get; set; }

    /// <summary>
    /// The 'current' image sorting direction. A reconciliation between the user's Settings selection and the sorting direction from Windows Explorer, to be used to sort the active image list.
    /// </summary>
    public static ImageOrderType ActiveImageLoadingOrderType { get; set; }

    /// <summary>
    /// Gets, sets color channel of image
    /// </summary>
    public static ColorChannel ImageChannel { get; set; } = ColorChannel.All;

    /// <summary>
    /// Gets, sets value if image data was modified
    /// </summary>
    public static string ImageModifiedPath { get; set; } = "";

    /// <summary>
    /// Gets, sets the temporary image data
    /// </summary>
    public static Bitmap? ClipboardImage { get; set; }

    /// <summary>
    /// Gets, sets copied filename collection (multi-copy)
    /// </summary>
    public static List<string> StringClipboard { get; set; } = new();

    #endregion


    #region Public Functions

    public static void InitImageList(
        IEnumerable<string>? list = null,
        List<string>? distinctDirsList = null)
    {
        Images.Dispose();
        Images = new(list)
        {
            MaxQueue = Config.ImageBoosterCachedCount,
            ImageChannel = ImageChannel,
            DistinctDirs = distinctDirsList ?? new(0),
        };
    }

    #endregion

}
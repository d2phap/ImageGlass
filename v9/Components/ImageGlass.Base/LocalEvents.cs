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

using ImageGlass.Base.Photoing.Codecs;
using System;

namespace ImageGlass.Base;


public class ImageEventArgs : EventArgs
{
    /// <summary>
    /// Gets the current viewing image's index.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// Gets the current viewing image's path.
    /// </summary>
    public string? FilePath { get; init; }
}

public class ImageLoadingEventArgs : ImageEventArgs
{
    /// <summary>
    /// Gets the loading image's index.
    /// </summary>
    public int NewIndex { get; init; }
}

public class ImageLoadedEventArgs : ImageEventArgs
{
    /// <summary>
    /// Gets the loaded image data.
    /// </summary>
    public IgPhoto? Data { get; init; }

    /// <summary>
    /// Gets the laoded image error.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Gets the value indicating that the viewer should reset zoom value.
    /// </summary>
    public bool ResetZoom { get; init; }
}

public class ImageListLoadedEventArgs : EventArgs
{
    public string? InitFilePath { get; init; }
}

public class ImageListLoadedToolEventArgs : ImageListLoadedEventArgs
{
    public List<string> Files { get; init; }
}


public class SlideshowWindowClosedEventArgs : EventArgs
{
    public int SlideshowIndex { get; init; }

    public SlideshowWindowClosedEventArgs(int slideshowIndex)
    {
        SlideshowIndex = slideshowIndex;
    }
}


public class ImageSaveEventArgs : EventArgs
{
    public string SrcFilePath { get; init; }
    public string DestFilePath { get; init; }
    public ImageSaveSource SaveSource { get; init; }

    public ImageSaveEventArgs(string srcFilePath, string destFilePath, ImageSaveSource saveSource)
    {
        SrcFilePath = srcFilePath;
        DestFilePath = destFilePath;
        SaveSource = saveSource;
    }
}


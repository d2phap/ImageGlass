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

namespace ImageGlass.Base;

public class ImageLoadingEventArgs
{
    public int CurrentIndex { get; init; }
    public int NewIndex { get; init; }
    public string? FilePath { get; init; }
}

public class ImageLoadedEventArgs
{
    public int Index { get; init; }

    public string? FilePath { get; init; }
    public IgPhoto? Data { get; init; }
    public Exception? Error { get; init; }
    public bool ResetZoom { get; init; }
}

public class ImageUnloadedEventArgs
{
    public int Index { get; init; }
    public string? FilePath { get; init; }
}

public class ImageListLoadedEventArgs
{
    public string? FilePath { get; init; }
}


public class SlideshowWindowClosedEventArgs
{
    public int SlideshowIndex { get; init; }

    public SlideshowWindowClosedEventArgs(int slideshowIndex)
    {
        SlideshowIndex = slideshowIndex;
    }
}


public class ImageSaveEventArgs
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


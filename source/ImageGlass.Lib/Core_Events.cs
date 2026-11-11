/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.Common;


public class ThemePackChangedEventArgs(string propName = "") : EventArgs
{
    /// <summary>
    /// Gets the property that triggered the event.
    /// If it's empty, the new theme pack is loaded.
    /// </summary>
    public string PropertyName => propName;
}


public class PhotoUnloadedEventArgs : EventArgs
{
    public bool IsClipboardPhoto { get; set; } = false;
    public int Index { get; set; } = -1;
    public string FilePath { get; set; } = string.Empty;
}


public class DestColorProfileChangedEventArgs(bool requiresPhotoReload) : EventArgs
{
    /// <summary>
    /// Whether the current photo must be re-decoded to re-apply the profile.
    /// <see langword="false"/> when only the monitor under the window changed: the new profile
    /// applies to later loads, but re-decoding now would flash the viewer.
    /// </summary>
    public bool RequiresPhotoReload { get; init; } = requiresPhotoReload;
}


public class PhotoSaveEventArgs(string srcFilePath, string destFilePath, ImageSaveSource saveSource) : EventArgs
{
    public string SrcFilePath { get; init; } = srcFilePath;
    public string DestFilePath { get; init; } = destFilePath;
    public bool IsSaveAsNewFile => !SrcFilePath.Equals(DestFilePath, StringComparison.OrdinalIgnoreCase);
    public ImageSaveSource SaveSource { get; init; } = saveSource;
}


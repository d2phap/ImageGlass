/*
ImageGlass - A Fast, Seamless Photo Viewer
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
using ImageGlass.UI.Viewer.Transitions;
using System;
using System.Threading;

namespace ImageGlass.Common.Photoing;


public enum PhotoState
{
    /// <summary>
    /// Photo is not loaded, metadata may be null.
    /// </summary>
    None,

    /// <summary>
    /// When photo metadata and preview are loaded.
    /// </summary>
    Preview,

    /// <summary>
    /// When photo bitmap is fully decoded.
    /// </summary>
    Loaded,
}


public class PhotoLoadingEventArgs(PhotoState state, Photo photo, CancellationToken token) : EventArgs
{
    /// <summary>
    /// Gets the loading state of photo.
    /// </summary>
    public PhotoState State => state;

    /// <summary>
    /// Gets the current photo instance.
    /// </summary>
    public Photo Photo => photo;

    /// <summary>
    /// Gets the current metadata instance.
    /// </summary>
    public PhotoMetadata Metadata => photo.Metadata;

    /// <summary>
    /// Gets the cancellation token of the current photo.
    /// </summary>
    public CancellationToken CancelToken => token;

}


public class PhotoLoadingOptions
{
    public bool UseCache { get; set; } = true;
    public bool ResetZoom { get; set; } = true;
    public ColorChannels Channels { get; set; } = ColorChannels.RGBA;

    /// <summary>
    /// Gets, sets the transition to play from the current photo, or <c>null</c> for none.
    /// </summary>
    public TransitionRequest? Transition { get; set; }
}


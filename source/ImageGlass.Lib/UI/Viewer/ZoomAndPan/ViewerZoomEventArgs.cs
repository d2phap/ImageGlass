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
using System;

namespace ImageGlass.UI.Viewer.ZoomAndPan;


/// <summary>
/// Zoom event arguments
/// </summary>
public class ViewerZoomEventArgs : EventArgs
{
    /// <summary>
    /// Gets, sets zoom factor
    /// </summary>
    public double ZoomFactor { get; set; } = 0f;


    /// <summary>
    /// Indicates that zoom factor is changed manually
    /// by setting <see cref="ViewerControl.ZoomFactor"/>
    /// </summary>
    public bool IsManualZoom { get; set; } = false;


    /// <summary>
    /// Gets, sets the value indicates that <see cref="ViewerControl.ZoomMode"/> is changed.
    /// </summary>
    public bool IsZoomModeChange { get; set; } = false;


    /// <summary>
    /// Gets, sets the value indicates that the displaying image is for temporarily previewing.
    /// </summary>
    public bool IsPreviewingImage { get; set; } = false;


    /// <summary>
    /// Gets, sets the source that caused zoom value changed.
    /// </summary>
    public ZoomChangeSource ChangeSource { get; set; } = ZoomChangeSource.Unknown;


    public ViewerZoomEventArgs() { }
}

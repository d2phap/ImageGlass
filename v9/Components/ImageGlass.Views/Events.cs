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

namespace ImageGlass.Views;

/// <summary>
/// Panning event arguments
/// </summary>
public class PanningEventArgs : EventArgs
{
    /// <summary>
    /// Gets current mouse pointer location on host control
    /// </summary>
    public PointF HostLocation { get; private set; } = new(0, 0);

    /// <summary>
    /// Gets panning start mouse pointer location on host control
    /// </summary>
    public PointF HostStartLocation { get; private set; } = new(0, 0);


    public PanningEventArgs(PointF loc, PointF startLoc)
    {
        HostLocation = loc;
        HostStartLocation = startLoc;
    }
}


/// <summary>
/// Zoom event arguments
/// </summary>
public class ZoomEventArgs : EventArgs
{
    /// <summary>
    /// Gets zoom factor
    /// </summary>
    public float ZoomFactor { get; private set; } = 0f;

    /// <summary>
    /// Gets, sets the value indicates that zoom factor is changed manually by <see cref="DXCanvas.ZoomFactor"/>
    /// </summary>
    public bool IsManualZoom { get; private set; } = false;

    /// <summary>
    /// Gets, sets the value indicates that <see cref="DXCanvas.ZoomMode"/> is changed.
    /// </summary>
    public bool IsZoomModeChange { get; private set; } = false;


    public ZoomEventArgs(float zoomFactor, bool isManualZoom, bool isZoomModeChanged)
    {
        ZoomFactor = zoomFactor;
        IsManualZoom = isManualZoom;
        IsZoomModeChange = isZoomModeChanged;
    }
}


/// <summary>
/// Mouse event arguments
/// </summary>
public class ImageMouseEventArgs : EventArgs
{
    /// <summary>
    /// Gets the x-coordinate of the image
    /// </summary>
    public float ImageX { get; private set; } = 0;

    /// <summary>
    /// Gets the y-coordinate of the image
    /// </summary>
    public float ImageY { get; private set; } = 0;

    /// <summary>
    /// Gets which mouse button was pressed
    /// </summary>
    public MouseButtons Button { get; private set; } = MouseButtons.Left;


    public ImageMouseEventArgs(float imgX, float imgY, MouseButtons button)
    {
        Button = button;
        ImageX = imgX;
        ImageY = imgY;
    }
}


/// <summary>
/// Selection event arguments
/// </summary>
public class SelectionEventArgs : EventArgs
{
    /// <summary>
    /// Gets the client selection area.
    /// </summary>
    public RectangleF ClientSelection { get; private set; }

    /// <summary>
    /// Gets the source selection area.
    /// </summary>
    public RectangleF SourceSelection { get; private set; }


    public SelectionEventArgs(RectangleF clientSelection, RectangleF sourceSelection)
    {
        ClientSelection = clientSelection;
        SourceSelection = sourceSelection;
    }
}

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
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using System;

namespace ImageGlass.UI.Viewer;

public class ViewerPointerEventArgs(PointerEventArgs e, PointerPoint p, PixelPoint srcPoint) : EventArgs
{
    /// <summary>
    /// Gets the pointer event data associated with the current input event.
    /// </summary>
    public PointerEventArgs Event => e;

    /// <summary>
    /// Gets pointer point.
    /// </summary>
    public PointerPoint Point => p;

    /// <summary>
    /// Gets the corresponding source image coordinates
    /// for the current position within the viewer control.
    /// It can be out of the image bounds.
    /// </summary>
    public PixelPoint SourcePoint => srcPoint;

}



/// <summary>
/// Event args for mouse click action dispatch from the viewer.
/// </summary>
public class ViewerPointerClickEventArgs(RoutedEventArgs e, MouseClickEvent clickEvent) : EventArgs
{
    /// <summary>
    /// Gets the pointer event data associated with the current input event.
    /// </summary>
    public RoutedEventArgs Event => e;

    /// <summary>
    /// Gets the mouse click event that triggered this action.
    /// </summary>
    public MouseClickEvent ClickEvent => clickEvent;
}



/// <summary>
/// Event args for mouse wheel action dispatch from the viewer.
/// </summary>
public class ViewerMouseWheelEventArgs(PointerWheelEventArgs e, MouseWheelEvent wheelEvent, double delta, Point position) : EventArgs
{
    /// <summary>
    /// Gets the pointer event data associated with the current input event.
    /// </summary>
    public PointerWheelEventArgs Event => e;

    /// <summary>
    /// Gets the mouse wheel event that triggered this action.
    /// </summary>
    public MouseWheelEvent WheelEvent => wheelEvent;

    /// <summary>
    /// Gets the scroll delta (positive = up/forward, negative = down/backward).
    /// Already multiplied by <see cref="Const.MOUSE_WHEEL_SCROLL_DELTA"/>.
    /// </summary>
    public double Delta => delta;

    /// <summary>
    /// Gets the pointer position relative to the viewer control.
    /// </summary>
    public Point Position => position;
}


/// <summary>
/// Event args for the playback state of a photo frame changes.
/// </summary>
public class PhotoFrameChangedEventArgs(bool canAnimate, bool isAnimating, bool isLivePhoto = false) : AnimatorFrameChangedEventArgs
{
    public bool CanAnimate => canAnimate;
    public bool IsAnimating => isAnimating;
    public bool IsLivePhoto => isLivePhoto;

}


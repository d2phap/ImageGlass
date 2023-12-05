/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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

namespace ImageGlass.Viewer;


[Flags]
public enum AnimationSource
{
    None = 0,

    PanLeft = 1 << 1,
    PanRight = 1 << 2,
    PanUp = 1 << 3,
    PanDown = 1 << 4,

    /// <summary>
    /// Zoom in animation. It does nothing if <see cref="DXCanvas.ZoomLevels"/> is set.
    /// </summary>
    ZoomIn = 1 << 5,
    /// <summary>
    /// Zoom out animation. It does nothing if <see cref="DXCanvas.ZoomLevels"/> is set.
    /// </summary>
    ZoomOut = 1 << 6,

    ImageFadeIn = 1 << 7,
}


[Flags]
public enum ImageSource
{
    Null = 0,

    Direct2D = 1 << 1,
    GDIPlus = 1 << 2,
    Webview2 = 1 << 3,
}


public enum MouseAndNavLocation
{
    Outside = 0,

    LeftNav = 1 << 1,
    RightNav = 1 << 2,

    BothNavs = 1 << 3,
}


public enum SelectionAction
{
    None,

    /// <summary>
    /// User is dragging to draw the selection.
    /// </summary>
    Drawing,

    /// <summary>
    /// User is resizing the selection.
    /// </summary>
    Resizing,

    // User is moving the selection.
    Moving,
}


public enum ImageDrawingState
{
    /// <summary>
    /// Image is not drawn.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Image is being drawn, or its animation is in progress.
    /// </summary>
    Drawing,

    /// <summary>
    /// Image is done all drawings, animations and shown on the canvas.
    /// </summary>
    Done,
}


public enum NavCheck
{
    Both,
    LeftOnly,
    RightOnly,
}


public enum AnimatorSource
{
    None = 0,
    GifAnimator,
    ImageAnimator,
}


public static class Web2BackendMsgNames
{
    public static string SET_HTML => "SET_HTML";
    public static string SET_IMAGE => "SET_IMAGE";
    public static string SET_ZOOM_MODE => "SET_ZOOM_MODE";
    public static string SET_ZOOM_FACTOR => "SET_ZOOM_FACTOR";
    public static string START_PANNING_ANIMATION => "START_PANNING_ANIMATION";
    public static string START_ZOOMING_ANIMATION => "START_ZOOMING_ANIMATION";
    public static string STOP_ANIMATIONS => "STOP_ANIMATIONS";
    public static string SET_MESSAGE => "SET_MESSAGE";
    public static string SET_NAVIGATION => "SET_NAVIGATION";

}


public static class Web2FrontendMsgNames
{
    public static string ON_ZOOM_CHANGED => "ON_ZOOM_CHANGED";
    public static string ON_POINTER_DOWN => "ON_POINTER_DOWN";
    public static string ON_MOUSE_WHEEL => "ON_MOUSE_WHEEL";
    public static string ON_FILE_DROP => "ON_FILE_DROP";
    public static string ON_NAV_CLICK => "ON_NAV_CLICK";

}

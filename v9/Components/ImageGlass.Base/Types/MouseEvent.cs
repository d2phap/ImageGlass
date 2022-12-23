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

namespace ImageGlass.Base;


/// <summary>
/// List of MouseClick / MouseDoubleClick events
/// </summary>
public enum MouseClickEvent
{
    LeftClick = 1,
    LeftDoubleClick = 2,

    RightClick = 3,
    RightDoubleClick = 4,

    XButton1Click = 5,
    XButton1DoubleClick = 6,

    XButton2Click = 7,
    XButton2DoubleClick = 8,

    WheelClick = 9,
    WheelDoubleClick = 10,
}


/// <summary>
/// List of MouseWheel events
/// </summary>
public enum MouseWheelEvent
{
    Scroll = 1,
    PressCtrlAndScroll = 2,
    PressShiftAndScroll = 3,
    PressAltAndScroll = 4,
}


/// <summary>
/// List of mouse wheel action for the <see cref="MouseWheelEvent"/>
/// </summary>
public enum MouseWheelAction
{
    DoNothing = 0,
    Zoom = 1,
    PanVertically = 2,
    PanHorizontally = 3,
    BrowseImages = 4
}

/*
ImageGlass Project - Image viewer for Windows
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
using ImageGlass.Common.Types.JsonTypeConverters;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types;


/// <summary>
/// List of mouse click events
/// </summary>
[JsonConverter(typeof(JsonStringEnumSafeConverter<MouseClickEvent>))]
public enum MouseClickEvent
{
    LeftClick = 1,
    LeftDoubleClick,
    RightClick,
    WheelClick,

    XButton1Click,
    XButton2Click,
}


/// <summary>
/// List of mouse wheel events
/// </summary>
[JsonConverter(typeof(JsonStringEnumSafeConverter<MouseWheelEvent>))]
public enum MouseWheelEvent
{
    Scroll = 1,
    CtrlAndScroll,
    ShiftAndScroll,
    AltAndScroll,
}


/// <summary>
/// List of mouse wheel action for the <see cref="MouseWheelEvent"/>
/// </summary>
[JsonConverter(typeof(JsonStringEnumSafeConverter<MouseWheelAction>))]
public enum MouseWheelAction
{
    DoNothing = 0,
    Zoom = 1,
    PanVertically = 2,
    PanHorizontally = 3,
    BrowseImages = 4
}


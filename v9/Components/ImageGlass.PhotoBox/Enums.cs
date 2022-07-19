/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

namespace ImageGlass.PhotoBox;


[Flags]
public enum AnimationSource
{
    None = 0,

    PanLeft = 1 << 1,
    PanRight = 1 << 2,
    PanUp = 1 << 3,
    PanDown = 1 << 4,

    ZoomIn = 1 << 5,
    ZoomOut = 1 << 6,
}


[Flags]
public enum ImageSource
{
    Null = 0,

    Direct2D = 1 << 1,
    GDIPlus = 1 << 2,
}


public enum MouseAndNavLocation
{
    Outside = 0,
    
    LeftNav = 1 << 1,
    RightNav = 1 << 2,
    
    BothNavs = 1 << 3,
}
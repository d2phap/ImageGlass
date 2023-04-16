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

---------------------
ImageGlass.WinTouch is based on WinTouch.NET
Url: https://github.com/rprouse/WinTouch.NET
License: MIT
---------------------
*/
namespace ImageGlass.WinTouch;


/// <summary>
/// Gesture configuration structure.
/// </summary>
public struct GestureConfig
{
    /// <summary>
    /// The identifier for the type of configuration that will have messages enabled or disabled.
    /// </summary>
    public GestureInfoId Id;

    /// <summary>
    /// The messages to enable.
    /// </summary>
    public uint Want;

    /// <summary>
    /// The messages to disable.
    /// </summary>
    public uint Block;

    public GestureConfig(GestureInfoId id, uint want, uint block)
    {
        Id = id;
        Want = want;
        Block = block;
    }
}



/// <summary>
/// Gesture configuration flags
/// </summary>
public class GestureConfigFlags
{
    public const int GC_ALLGESTURES = 0x00000001;
    public const int GC_ZOOM = 0x00000001;
    public const int GC_PAN = 0x00000001;
    public const int GC_PAN_WITH_SINGLE_FINGER_VERTICALLY = 0x00000002;
    public const int GC_PAN_WITH_SINGLE_FINGER_HORIZONTALLY = 0x00000004;
    public const int GC_PAN_WITH_GUTTER = 0x00000008;
    public const int GC_PAN_WITH_INERTIA = 0x00000010;
    public const int GC_ROTATE = 0x00000001;
    public const int GC_TWOFINGERTAP = 0x00000001;
    public const int GC_PRESSANDTAP = 0x00000001;
}

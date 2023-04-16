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
    public GestureConfigId Id;

    /// <summary>
    /// The messages to enable.
    /// </summary>
    public GestureConfigFlags Want;

    /// <summary>
    /// The messages to disable.
    /// </summary>
    public GestureConfigFlags Block;


    public GestureConfig(GestureConfigId id, GestureConfigFlags want, GestureConfigFlags block)
    {
        Id = id;
        Want = want;
        Block = block;
    }
}



/// <summary>
/// Gesture configuration ID
/// </summary>
public enum GestureConfigId : uint
{
    Undefined = 0,
    GID_ZOOM = 0x00000003,
    GID_PAN = 0x00000004,
    GID_ROTATE = 0x00000005,
    GID_TWOFINGERTAP = 0x00000006,
    GID_PRESSANDTAP = 0x00000007,
}


[Flags]
public enum GestureConfigFlags : uint
{
    None = 0,

    /// <summary>
    /// Use with <see cref="GestureConfigId.Undefined"/>
    /// </summary>
    GC_ALLGESTURES = 0x00000001,
    GC_ZOOM = 0x00000001,
    GC_PAN = 0x00000001,
    GC_PAN_WITH_SINGLE_FINGER_VERTICALLY = 0x00000002,
    GC_PAN_WITH_SINGLE_FINGER_HORIZONTALLY = 0x00000004,
    GC_PAN_WITH_GUTTER = 0x00000008,
    GC_PAN_WITH_INERTIA = 0x00000010,
    GC_ROTATE = 0x00000001,
    GC_TWOFINGERTAP = 0x00000001,
    GC_PRESSANDTAP = 0x00000001,
}

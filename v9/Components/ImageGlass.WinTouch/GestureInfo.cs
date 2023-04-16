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
using System.Runtime.InteropServices;

namespace ImageGlass.WinTouch;


/// <summary>
/// Stores information about a gesture.
/// </summary>
public class GestureInfo
{
    /// <summary>
    /// The size of the structure, in bytes. The caller must set this.
    /// </summary>
    public uint Size { get; set; }

    /// <summary>
    /// The state of the gesture.
    /// </summary>
    public GestureInfoFlags Flags { get; set; }

    /// <summary>
    /// The identifier of the gesture command.
    /// </summary>
    public GestureInfoId Id { get; set; }

    /// <summary>
    /// A handle to the window that is targeted by this gesture.
    /// </summary>
    public IntPtr Hwnd { get; set; }

    /// <summary>
    /// The coordinates associated with the gesture.
    /// These coordinates are always relative to the origin of the screen.
    /// </summary>
    public Point Location { get; set; }

    /// <summary>
    /// An internally used identifier for the structure.
    /// </summary>
    public uint InstanceId { get; set; }

    /// <summary>
    /// An internally used identifier for the sequence.
    /// </summary>
    public uint SequenceId { get; set; }

    /// <summary>
    /// A 64-bit unsigned integer that contains the arguments for gestures that fit into 8 bytes. 
    /// </summary>
    public ulong Arguments { get; set; }

    /// <summary>
    /// The size, in bytes, of extra arguments that accompany this gesture.
    /// </summary>
    public uint ExtraArguments { get; set; }


    /// <summary>
    /// Gets a value indicating whether the <see cref="GestureInfoFlags.GF_BEGIN"/> is set.
    /// </summary>
    public bool Begin => Flags.HasFlag(GestureInfoFlags.GF_BEGIN);

    /// <summary>
    /// Gets a value indicating whether the <see cref="GestureInfoFlags.GF_END"/> is set.
    /// </summary>
    public bool End => Flags.HasFlag(GestureInfoFlags.GF_END);

    /// <summary>
    /// Gets a value indicating whether the <see cref="GestureInfoFlags.GF_INERTIA"/> is set
    /// </summary>
    public bool Inertia => Flags.HasFlag(GestureInfoFlags.GF_INERTIA);

}


/// <summary>
/// Gesture ID, based on <c>GESTURECONFIG_ID</c> from CsWin32.
/// </summary>
public enum GestureInfoId : uint
{
    GID_BEGIN = 0x00000001,
    GID_END = 0x00000002,
    GID_ZOOM = 0x00000003,
    GID_PAN = 0x00000004,
    GID_ROTATE = 0x00000005,
    GID_TWOFINGERTAP = 0x00000006,
    GID_PRESSANDTAP = 0x00000007,
}


/// <summary>
/// Gesture flags - GestureInfo.flags
/// </summary>
[Flags]
public enum GestureInfoFlags
{
    GF_BEGIN = 0x1,
    GF_INERTIA = 0x2,
    GF_END = 0x4,
}


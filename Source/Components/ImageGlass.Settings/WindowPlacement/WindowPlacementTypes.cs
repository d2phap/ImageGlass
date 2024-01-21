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
using System.Runtime.InteropServices;

namespace ImageGlass.Settings;

public enum WindowState
{
    Normal = 1,
    Minimized = 2,
    Maximized = 3,
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WindowPlacement(WpRect normalPos, WindowState state = WindowState.Normal)
{
    public int length = Marshal.SizeOf(typeof(WindowPlacement));
    public int flags = 0;
    public WindowState showCmd = state;
    public WpPoint minPosition = new WpPoint(-1, -1);
    public WpPoint maxPosition = new WpPoint(-1, -1);
    public WpRect normalPosition = normalPos;
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WpPoint(int x, int y)
{
    public int X = x;
    public int Y = y;
}


/// <summary>
/// Interaction logic for WindowPlacement.
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
public struct WpRect(int left, int top, int right, int bottom)
{
    public int Left = left;
    public int Top = top;
    public int Right = right;
    public int Bottom = bottom;
}

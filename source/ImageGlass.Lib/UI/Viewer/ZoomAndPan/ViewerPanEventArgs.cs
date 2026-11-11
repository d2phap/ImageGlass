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
using System;

namespace ImageGlass.UI.Viewer.ZoomAndPan;


/// <summary>
/// Panning event arguments
/// </summary>
public class ViewerPanEventArgs(Rect oldSrcRect, Rect newSrcRect) : EventArgs
{
    /// <summary>
    /// Gets current mouse pointer location on host control
    /// </summary>
    public Rect OldSourceRect { get; private set; } = oldSrcRect;


    /// <summary>
    /// Gets panning start mouse pointer location on host control
    /// </summary>
    public Rect NewSourceRect { get; private set; } = newSrcRect;

}

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

namespace ImageGlass.Common.Types;


/// <summary>
/// The windowed layout of the main window, i.e. the state to return to when full screen mode is
/// turned off. Full screen hides the toolbar and gallery and drops frameless and window fit
/// through <c>Config</c>, so the value captured on entry is also what gets persisted, keeping the
/// saved config a description of the windowed layout rather than the full screen one.
/// </summary>
public readonly record struct WindowLayoutSnapshot
{
    /// <summary>
    /// Whether the window was maximized.
    /// </summary>
    public bool IsMaximized { get; init; }

    /// <summary>
    /// Window position and client size; <c>null</c> unless the window was in the normal state.
    /// </summary>
    public Rect? Bounds { get; init; }

    /// <summary>
    /// Whether the toolbar was visible.
    /// </summary>
    public bool ShowToolbar { get; init; }

    /// <summary>
    /// Whether the gallery was visible.
    /// </summary>
    public bool ShowGallery { get; init; }

    /// <summary>
    /// Whether frameless mode was on.
    /// </summary>
    public bool IsFrameless { get; init; }

    /// <summary>
    /// Whether window fit mode was on.
    /// </summary>
    public bool IsWindowFit { get; init; }
}

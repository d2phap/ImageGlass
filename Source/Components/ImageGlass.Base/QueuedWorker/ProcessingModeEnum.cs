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

namespace ImageGlass.Base.QueuedWorker;

/// <summary>
/// Represents the mode in which the work items of <see cref="QueuedWorker"/> are processed.
/// </summary>
public enum ProcessingMode
{
    /// <summary>
    /// Items are processed in the order they are received.
    /// </summary>
    FIFO,
    /// <summary>
    /// Items are processed in reverse order.
    /// </summary>
    LIFO,
}

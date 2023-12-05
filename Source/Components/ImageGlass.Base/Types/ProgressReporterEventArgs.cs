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
namespace ImageGlass.Base;

public class ProgressReporterEventArgs<T>(T data, string type = "") : EventArgs
{
    /// <summary>
    /// The type of the event.
    /// </summary>
    public string Type { get; init; } = type;

    /// <summary>
    /// Event data.
    /// </summary>
    public T Data { get; init; } = data;
}


public class ProgressReporterEventArgs(EventArgs data, string type = "") : ProgressReporterEventArgs<EventArgs>(data, type) { }

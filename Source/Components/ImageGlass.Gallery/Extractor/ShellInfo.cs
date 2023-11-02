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
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
namespace ImageGlass.Gallery;


/// <summary>
/// Contains shell icons and shell file type.
/// </summary>
public class ShellInfo
{
    /// <summary>
    /// Error.
    /// </summary>
    public Exception? Error = null;

    /// <summary>
    /// Mime type.
    /// </summary>
    public string? FileType = null;

    /// <summary>
    /// Small shell icon.
    /// </summary>
    public Image? SmallIcon = null;

    /// <summary>
    /// Large shell icon.
    /// </summary>
    public Image? LargeIcon = null;

}

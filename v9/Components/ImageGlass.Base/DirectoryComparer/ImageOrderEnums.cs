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

namespace ImageGlass.Base;

/// <summary>
/// The loading order list.
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderBy
{
    Name = 0,
    FileSize = 1,
    CreationTime = 2,
    Extension = 3,
    LastAccessTime = 4,
    LastWriteTime = 5,
    Rating = 6,
    Random = 7,
}


/// <summary>
/// The loading order types list
/// **If we need to rename, we MUST update the language string too.
/// Because the name is also language keyword!
/// </summary>
public enum ImageOrderType
{
    Asc = 0,
    Desc = 1,
}

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
/// Types of image infomation update request
/// </summary>
[Flags]
public enum ImageInfoUpdateTypes
{
    None = 0,

    All = 1 << 1,
    AppName = 1 << 2,
    Name = 1 << 3,
    Path = 1 << 4,
    FileSize = 1 << 5,
    Dimension = 1 << 6,
    ListCount = 1 << 7,
    Zoom = 1 << 8,
    FramesCount = 1 << 9,

    DateTimeAuto = 1 << 10,
    ModifiedDateTime = 1 << 11,
    ExifDateTime = 1 << 12,
    ExifDateTimeOriginal = 1 << 13,

    ExifRating = 1 << 14,
    ColorSpace = 1 << 15,
}

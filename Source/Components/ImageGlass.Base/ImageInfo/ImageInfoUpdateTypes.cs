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
    AppName = 1 << 1,
    Name = 1 << 2,
    Path = 1 << 3,
    FileSize = 1 << 4,
    Dimension = 1 << 5,
    ListCount = 1 << 6,
    Zoom = 1 << 7,
    FrameCount = 1 << 8,

    DateTimeAuto = 1 << 9,
    ModifiedDateTime = 1 << 10,
    ExifDateTime = 1 << 11,
    ExifDateTimeOriginal = 1 << 12,

    ExifRating = 1 << 13,
    ColorSpace = 1 << 14,
}

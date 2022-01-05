/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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

namespace ImageGlass.Gallery;

public class ImageDetails : Metadata
{
    public DateTime DateCreated { get; internal set; }
    public DateTime DateAccessed { get; internal set; }
    public DateTime DateModified { get; internal set; }
    public long FileSize { get; internal set; } = 0;

    public string FilePath { get; internal set; } = string.Empty;
    public string FileName { get; internal set; } = string.Empty;
    public string FileExtension { get; internal set; } = string.Empty;
    public string FolderPath { get; internal set; } = string.Empty;
    public string FolderName { get; internal set; } = string.Empty;

}

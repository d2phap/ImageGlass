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
using System;
using System.IO;

namespace ImageGlass.Common.ServiceProviders.FileSearchService;


/// <summary>
/// Immutable filesystem information captured while enumerating a folder.
/// </summary>
public sealed record FileSearchEntry(
    string FilePath,
    long FileSizeInBytes,
    DateTime FileCreationTimeUtc,
    DateTime FileLastWriteTimeUtc,
    DateTime FileLastAccessTimeUtc,
    FileAttributes Attributes)
{
    /// <summary>
    /// Creates an entry from an already-enumerated <see cref="FileInfo"/>.
    /// </summary>
    public static FileSearchEntry FromFileInfo(FileInfo file) => new(
        file.FullName,
        file.Length,
        file.CreationTimeUtc,
        file.LastWriteTimeUtc,
        file.LastAccessTimeUtc,
        file.Attributes);


    /// <summary>
    /// Creates an entry for a standalone path.
    /// </summary>
    public static FileSearchEntry FromPath(string filePath) => FromFileInfo(new FileInfo(filePath));
}

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
using ImageGlass.Common.ServiceProviders.FileSearchService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Common.Photoing;

public partial class PhotoManager
{
    protected volatile int _currentIndex = -1;


    // Public properties
    #region Public properties

    /// <summary>
    /// Gets index of the viewed photo.
    /// </summary>
    public int CurrentIndex => _currentIndex;


    /// <summary>
    /// Gets the current file path of the viewed photo.
    /// </summary>
    public string CurrentFilePath => GetFilePath(CurrentIndex);


    /// <summary>
    /// Gets the metadata of the viewed photo.
    /// </summary>
    public PhotoMetadata? CurrentMetadata => Get(CurrentIndex)?.Metadata;


    /// <summary>
    /// Gets current photo.
    /// </summary>
    public Photo? Current => Get(CurrentIndex);


    /// <summary>
    /// Gets a value indicating whether the current photo contains an error.
    /// </summary>
    public bool IsCurrentError => Current?.Error != null;


    /// <summary>
    /// The initial photo,
    /// can be the photo from the initial file path or the first photo of the directory.
    /// </summary>
    public Photo? InitPhoto { get; set; } = null;

    #endregion // Public properties


    /// <summary>
    /// Loads files from the input path, returns the initial photo.
    /// </summary>
    /// <param name="paths">Full path of file or directory</param>
    /// <param name="reloadInitPhoto">
    /// If <c>false</c>, the currently viewed photo is preserved and reused as the
    /// initial photo instead of being disposed during the list reset.
    /// </param>
    public Photo? StartLoadingFiles(ICollection<string> paths, string? currentFilePath,
        FileSearchOptions searchOptions, Action<FileSearchingEventArgs> progressFn,
        bool reloadInitPhoto)
    {
        // 1. stop any ongoing search
        Core.FileSearchProvider.CancelSearching();

        // enumeration emits absolute paths; inputs must match or the init photo never resolves
        var absPaths = paths.Select(BHelper.ResolvePath).ToList();
        currentFilePath = BHelper.ResolvePath(currentFilePath);

        // 2. get distinct dir paths for searching
        var inputPaths = BHelper.GetDistinctDirsFromPaths(absPaths);


        // 3. preserve the current photo if requested
        Photo? preservedPhoto = null;
        if (!reloadInitPhoto)
        {
            preservedPhoto = Current;
        }


        // 4. reset the list, MUST be after getting distinct dirs
        Clear(excludeFromDisposal: preservedPhoto);
        DistinctDirs = inputPaths.DirPaths;


        // 5. create init photo
        var initFilePath = currentFilePath;
        if (string.IsNullOrWhiteSpace(initFilePath))
        {
            initFilePath = inputPaths.FilePaths.FirstOrDefault() ?? "";
        }

        if (preservedPhoto is not null
            && string.Equals(preservedPhoto.FilePath, initFilePath, StringComparison.OrdinalIgnoreCase))
        {
            InitPhoto = preservedPhoto;
        }
        else if (!string.IsNullOrWhiteSpace(initFilePath))
        {
            // dispose the preserved photo if it doesn't match the init file path
            preservedPhoto?.Dispose();
            InitPhoto = new Photo(initFilePath);
        }


        // 6. start searching files in a new thread
        _ = Core.FileSearchProvider.SearchAsync(DistinctDirs, searchOptions, progressFn);

        return InitPhoto;
    }

}

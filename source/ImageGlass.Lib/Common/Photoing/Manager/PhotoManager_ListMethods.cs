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
using System.Collections.Generic;
using System.Linq;
using ImageGlass.Common.ServiceProviders.FileSearchService;

namespace ImageGlass.Common.Photoing;

public partial class PhotoManager
{

    /// <summary>
    /// Adds a file path to the collection.
    /// </summary>
    public void Add(string filePath, int index = -1)
    {
        Add([filePath], index);
    }


    /// <summary>
    /// Adds a list of file paths to the collection.
    /// </summary>
    public void Add(IEnumerable<string> filePaths, int index = -1)
    {
        var newItems = filePaths.Select(path => new Photo(path)).ToList();

        AddPhotos(newItems, index);
    }


    /// <summary>
    /// Adds file-search entries using their captured filesystem metadata.
    /// </summary>
    public void Add(IEnumerable<FileSearchEntry> entries, int index = -1)
    {
        var newItems = entries.Select(entry => new Photo(entry)).ToList();

        AddPhotos(newItems, index);
    }


    private void AddPhotos(List<Photo> newItems, int index)
    {
        lock (_lock)
        {
            var addedIndex = index;

            if (index < 0)
            {
                addedIndex = (int)Count;

                Items.AddRange(newItems);

                // only index the newly appended items
                for (int i = addedIndex; i < Count; i++)
                {
                    _dict[Items[i].FilePath] = i;
                }
            }
            else
            {
                Items.InsertRange(index, newItems);

                // re-index from insertion point (existing items shifted)
                for (int i = addedIndex; i < Count; i++)
                {
                    _dict[Items[i].FilePath] = i;
                }
            }
        }

        _ = OnPropertyChanged(nameof(Count));
    }


    /// <summary>
    /// Gets a photo from the list.
    /// </summary>
    public Photo? Get(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= Count) return null;
            return Items[index];
        }
    }


    /// <summary>
    /// Gets a photo from the list.
    /// </summary>
    public Photo? Get(string filePath)
    {
        lock (_lock)
        {
            var index = _dict.GetValueOrDefault(filePath, -1);
            return Get(index);
        }
    }


    /// <summary>
    /// Selects the specified file by its path, updating the current selection.
    /// </summary>
    public Photo? Select(string filePath)
    {
        var newSelectionIndex = IndexOf(filePath);

        return Select(newSelectionIndex);
    }


    /// <summary>
    /// Selects an item at the specified index, updating the current selection.
    /// </summary>
    public Photo? Select(int index)
    {
        Photo? result;

        lock (_lock)
        {
            // deselect old index
            if (0 <= CurrentIndex && CurrentIndex < Count)
            {
                Items[CurrentIndex].IsCurrent = false;
            }

            // validate new index
            if (index < 0 || index >= Count) return null;

            // select new index
            Items[index].IsCurrent = true;
            _currentIndex = index;
            result = Items[index];
        }

        _ = OnPropertyChanged(nameof(Current));
        _ = OnPropertyChanged(nameof(CurrentIndex));
        _ = OnPropertyChanged(nameof(CurrentFilePath));
        _ = OnPropertyChanged(nameof(CurrentMetadata));

        return result;
    }


    /// <summary>
    /// Checks whether the given file path is currently selected.
    /// </summary>
    public bool IsSelected(string filePath)
    {
        var index = IndexOf(filePath);

        return IsSelected(index);
    }


    /// <summary>
    /// Checks whether the specified index is currently selected.
    /// </summary>
    public bool IsSelected(int index)
    {
        return Get(index)?.IsCurrent ?? false;
    }


    /// <summary>
    /// Get file path of the photo at the specified index.
    /// </summary>
    public string GetFilePath(int index)
    {
        return Get(index)?.FilePath ?? string.Empty;
    }


    /// <summary>
    /// Set file path of the photo at the specified index.
    /// </summary>
    public void SetFilePath(int index, string filePath)
    {
        bool isCurrent;

        lock (_lock)
        {
            var photo = Get(index);
            if (photo is null) return;

            var oldFilePath = photo.FilePath;
            photo.FilePath = filePath;

            // remove the old path entry, then add the new one
            if (!string.Equals(oldFilePath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                _dict.TryRemove(oldFilePath, out _);
            }
            _dict[filePath] = index;

            isCurrent = index == CurrentIndex;
        }

        if (isCurrent)
        {
            _ = OnPropertyChanged(nameof(Current));
            _ = OnPropertyChanged(nameof(CurrentFilePath));
            _ = OnPropertyChanged(nameof(CurrentMetadata));
        }
    }


    /// <summary>
    /// Find index of the photo with the given file path.
    /// </summary>
    public int IndexOf(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return -1;

        if (_dict.TryGetValue(filePath, out var photoIndex))
        {
            return photoIndex;
        }

        return -1;
    }


    /// <summary>
    /// Checks if the photo is cached.
    /// </summary>
    public bool IsCached(int index)
    {
        var photo = Get(index);
        return photo?.State == PhotoState.Loaded;
    }


    /// <summary>
    /// Unloads and releases resources of the photo at the specified index from the collection.
    /// </summary>
    public void Unload(int index)
    {
        Get(index)?.Unload();
    }


    /// <summary>
    /// Removes the photo at the specified index from the collection.
    /// </summary>
    public void Remove(int index)
    {
        var photo = Get(index);
        if (photo is null) return;

        Remove(photo.FilePath);
    }


    /// <summary>
    /// Removes the photo by the given file path from the collection.
    /// </summary>
    public void Remove(string filePath)
    {
        // remove from cache tracking before modifying the list
        InvalidateCacheAt(filePath);

        bool isCurrentPhoto;
        Photo? removed;

        lock (_lock)
        {
            var index = IndexOf(filePath);
            if (index < 0) return;

            isCurrentPhoto = CurrentIndex == index;

            // update index of affected items
            for (int i = index + 1; i < Count; i++)
            {
                var newIndex = i - 1;
                var itemPath = GetFilePath(i);
                _dict[itemPath] = newIndex;
            }

            removed = Items[index];

            // remove from the lists
            _dict.Remove(filePath, out var _);
            Items.RemoveAt(index);
        }

        // dispose outside the lock to avoid blocking
        removed?.Dispose();

        _ = OnPropertyChanged(nameof(Count));

        if (isCurrentPhoto)
        {
            _ = OnPropertyChanged(nameof(Current));
            _ = OnPropertyChanged(nameof(CurrentFilePath));
            _ = OnPropertyChanged(nameof(CurrentMetadata));
        }
    }


    /// <summary>
    /// Clears all photos from the collection and releases any associated resources.
    /// </summary>
    /// <param name="excludeFromDisposal">
    /// A photo to exclude from disposal, allowing it to be reused after clearing.
    /// </param>
    public void Clear(Photo? excludeFromDisposal = null)
    {
        // cancel and clear all cached photos first
        ClearCache(excludeFromDisposal);

        Photo? initPhoto;
        Photo[] snapshot;

        lock (_lock)
        {
            // clear init photo
            initPhoto = InitPhoto;
            InitPhoto = null;
            _currentIndex = -1;

            // snapshot items, then clear collections
            snapshot = [.. Items];
            Items.Clear();
            _dict.Clear();
            DistinctDirs.Clear();
        }

        // dispose outside the lock to avoid blocking
        if (initPhoto is not null && !ReferenceEquals(initPhoto, excludeFromDisposal))
        {
            initPhoto.Dispose();
        }
        foreach (var item in snapshot)
        {
            if (ReferenceEquals(item, excludeFromDisposal)) continue;
            item.WithNoReactive(() => item.Dispose());
        }
    }

}

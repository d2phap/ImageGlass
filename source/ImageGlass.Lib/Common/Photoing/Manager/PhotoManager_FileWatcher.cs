/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using D2Phap.FileWatcherEx;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using ImageGlass.Common.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Event args for <see cref="PhotoManager.FileWatcherChanged"/>.
/// </summary>
public class FileWatcherChangedEventArgs(
    ChangeType changeType,
    IReadOnlyList<string> filePaths,
    IReadOnlyList<string>? oldFilePaths = null,
    string? affectedCurrentFilePath = null) : EventArgs
{
    /// <summary>
    /// Gets the type of change that occurred.
    /// </summary>
    public ChangeType ChangeType => changeType;

    /// <summary>
    /// Gets the affected file paths (new paths for rename, current path otherwise).
    /// </summary>
    public IReadOnlyList<string> FilePaths => filePaths;

    /// <summary>
    /// Gets the old file paths (only for <see cref="ChangeType.RENAMED"/>).
    /// </summary>
    public IReadOnlyList<string>? OldFilePaths => oldFilePaths;

    /// <summary>
    /// Gets the affected current file path.
    /// </summary>
    public string AffectedCurrentFilePath => affectedCurrentFilePath ?? string.Empty;
}


public partial class PhotoManager
{
    // File system watcher
    private FileSystemWatcherEx _fileWatcher = new();

    // Pending delete queue processed by a background thread
    private readonly ConcurrentQueue<string> _deleteQueue = new();
    private Thread? _deleteWorkerThread;
    private volatile bool _deleteWorkerRunning;

    // Debounce key caches (must be stable delegate instances for BHelper.Debounce)
    private readonly Action _processAddedFilesAction;
    private readonly Action _processChangedFilesAction;

    // Pending add / change buffers
    private readonly ConcurrentQueue<string> _pendingAdds = new();
    private readonly ConcurrentQueue<string> _pendingChanges = new();


    /// <summary>
    /// Raised after the photo list has been modified by a file system event.
    /// </summary>
    public event TEventHandler<PhotoManager, FileWatcherChangedEventArgs>? FileWatcherChanged;


    /// <summary>
    /// Gets the path of the current directory being watched.
    /// </summary>
    public string FileWatcherFolderPath => _fileWatcher.FolderPath;




    /// <summary>
    /// Start watching a directory for changes.
    /// </summary>
    public void StartFileWatcher(string dirPath)
    {
        StopFileWatcher();

        if (string.IsNullOrWhiteSpace(dirPath)) return;

        _fileWatcher.FolderPath = dirPath;
        _fileWatcher.IncludeSubdirectories = Core.Config.EnableSubfoldersLoading;

        _fileWatcher.OnCreated += FileWatcher_OnCreated;
        _fileWatcher.OnDeleted += FileWatcher_OnDeleted;
        _fileWatcher.OnChanged += FileWatcher_OnChanged;
        _fileWatcher.OnRenamed += FileWatcher_OnRenamed;

        // start background thread for processing deletes
        _deleteWorkerRunning = true;
        _deleteWorkerThread = new Thread(ProcessDeleteQueue)
        {
            Priority = ThreadPriority.BelowNormal,
            IsBackground = true,
            Name = "PhotoManager.DeleteWorker",
        };
        _deleteWorkerThread.Start();

        try
        {
            _fileWatcher.Start();
        }
        catch (ArgumentException)
        {
            // SymlinkAwareFileWatcher.Init() may throw when the watched
            // directory tree contains symlinks that resolve to paths already
            // registered (duplicate key). Fall back to a non-recursive watcher
            // so the app can still start.
            _fileWatcher.Stop();
            _fileWatcher = new FileSystemWatcherEx
            {
                FolderPath = dirPath,
                IncludeSubdirectories = false,
            };

            _fileWatcher.OnCreated += FileWatcher_OnCreated;
            _fileWatcher.OnDeleted += FileWatcher_OnDeleted;
            _fileWatcher.OnChanged += FileWatcher_OnChanged;
            _fileWatcher.OnRenamed += FileWatcher_OnRenamed;

            _fileWatcher.Start();
        }
    }


    /// <summary>
    /// Stops file watcher and unsubscribes all event handlers.
    /// </summary>
    public void StopFileWatcher()
    {
        _fileWatcher.Stop();

        _fileWatcher.OnCreated -= FileWatcher_OnCreated;
        _fileWatcher.OnDeleted -= FileWatcher_OnDeleted;
        _fileWatcher.OnChanged -= FileWatcher_OnChanged;
        _fileWatcher.OnRenamed -= FileWatcher_OnRenamed;

        // stop delete worker
        _deleteWorkerRunning = false;

        _fileWatcher = new FileSystemWatcherEx();
    }


    /// <summary>
    /// Disposes file watcher and cleans up resources.
    /// </summary>
    private void DisposeFileWatcher()
    {
        StopFileWatcher();
        _fileWatcher.Dispose();
    }


    /// <summary>
    /// Checks if the file extension is in the supported file formats.
    /// </summary>
    private static bool IsSupportedFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        var ext = Path.GetExtension(filePath);
        if (string.IsNullOrEmpty(ext)) return false;

        // a save in progress writes a sibling temp file that keeps the real extension
        if (SaveStaging.IsStagingFile(filePath)) return false;

        return Core.GetSupportedFileExtensions().Contains(ext);
    }


    /// <summary>
    /// Checks whether the file, or any sub-folder holding it, is excluded from browsing.
    /// Keeps the watcher from re-adding what the file search deliberately skipped.
    /// </summary>
    private bool IsExcludedFile(string? filePath)
    {
        return BHelper.IsPathSkippedUnderRoot(filePath, FileWatcherFolderPath,
            Core.Config.EnableHiddenImagesLoading);
    }



    #region File watcher event handlers

    private void FileWatcher_OnCreated(object? sender, FileChangedEvent e)
    {
        if (!IsSupportedFile(e.FullPath)) return;
        if (IsExcludedFile(e.FullPath)) return;
        if (IndexOf(e.FullPath) >= 0) return;

        _pendingAdds.Enqueue(e.FullPath);
        BHelper.Debounce(300, _processAddedFilesAction);
    }


    private void FileWatcher_OnDeleted(object? sender, FileChangedEvent e)
    {
        if (!IsSupportedFile(e.FullPath)) return;

        _deleteQueue.Enqueue(e.FullPath);
    }


    private void FileWatcher_OnChanged(object? sender, FileChangedEvent e)
    {
        if (!IsSupportedFile(e.FullPath)) return;

        // if the file is not in our list, treat it as a new file
        if (IndexOf(e.FullPath) < 0)
        {
            if (IsExcludedFile(e.FullPath)) return;

            _pendingAdds.Enqueue(e.FullPath);
            BHelper.Debounce(300, _processAddedFilesAction);
            return;
        }

        _pendingChanges.Enqueue(e.FullPath);
        BHelper.Debounce(300, _processChangedFilesAction);
    }


    private void FileWatcher_OnRenamed(object? sender, FileChangedEvent e)
    {
        var newFilePath = e.FullPath ?? "";
        var oldFilePath = e.OldFullPath ?? "";

        // a finished save renames its staging file onto the target: that is a content change,
        // not a new file, so routing it here avoids a duplicate entry for a photo already listed
        if (SaveStaging.IsStagingFile(oldFilePath))
        {
            FileWatcher_OnChanged(sender, e);
            return;
        }

        var oldSupported = IsSupportedFile(oldFilePath);
        var newSupported = IsSupportedFile(newFilePath);

        // neither old nor new is a supported file type
        if (!oldSupported && !newSupported) return;

        // old was supported, new is not -> treat as delete
        if (oldSupported && !newSupported)
        {
            _deleteQueue.Enqueue(oldFilePath);
            return;
        }

        // renamed into a hidden folder (or made hidden) -> drop it from the list
        var newExcluded = IsExcludedFile(newFilePath);

        // old was not supported, new is -> treat as add
        if (!oldSupported && newSupported)
        {
            if (newExcluded) return;

            _pendingAdds.Enqueue(newFilePath);
            BHelper.Debounce(300, _processAddedFilesAction);
            return;
        }

        // both supported: rename in-place
        var imgIndex = IndexOf(oldFilePath);
        if (imgIndex >= 0)
        {
            if (newExcluded)
            {
                _deleteQueue.Enqueue(oldFilePath);
                return;
            }

            SetFilePath(imgIndex, newFilePath);

            FileWatcherChanged?.Invoke(this, new FileWatcherChangedEventArgs(
                ChangeType.RENAMED,
                [newFilePath],
                [oldFilePath]));
        }
        else if (!newExcluded)
        {
            // file not in our list yet -> add it
            _pendingAdds.Enqueue(newFilePath);
            BHelper.Debounce(300, _processAddedFilesAction);
        }
    }

    #endregion // File watcher event handlers



    #region Batch processors

    /// <summary>
    /// Drains the pending-add queue, inserts all new files into the list
    /// at the correct sorted position, and raises a single event.
    /// </summary>
    private void ProcessPendingAdds()
    {
        // drain queue into a HashSet for O(1) dedup
        var newFilesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (_pendingAdds.TryDequeue(out var path))
        {
            // avoid duplicates
            if (IndexOf(path) < 0)
            {
                newFilesSet.Add(path);
            }
        }

        if (newFilesSet.Count == 0) return;
        var newFiles = new List<string>(newFilesSet);

        // determine sorted insertion index for each file
        var options = new FileSearchOptions()
        {
            AllowedExtensions = Core.GetSupportedFileExtensions(),
            UseExplorerSortOrder = Core.Config.EnableExplorerSortOrder,
            SearchSubDirectories = Core.Config.EnableSubfoldersLoading,
            GroupByDir = Core.Config.EnableImageFolderGrouping,
            IncludeHidden = Core.Config.EnableHiddenImagesLoading,
            OrderBy = Core.Config.ImageLoadingOrder,
            OrderType = Core.Config.ImageLoadingOrderType,
            ForegroundShell = Core.ShellProvider?.ForegroundShell,
        };


        List<string> currentPaths;
        string currentFilePath;
        int oldCurrentIndex;
        lock (_lock)
        {
            oldCurrentIndex = CurrentIndex;
            currentFilePath = CurrentFilePath;

            // pre-allocate and copy paths without LINQ
            currentPaths = new List<string>(Items.Count);
            for (var i = 0; i < Items.Count; i++)
            {
                currentPaths.Add(Items[i].FilePath);
            }
        }

        // build a combined list, sort it, then find each new file's position
        var combinedList = new List<string>(currentPaths.Count + newFiles.Count);
        combinedList.AddRange(currentPaths);
        combinedList.AddRange(newFiles);

        var sortedList = FileSearchProvider.SortFiles(combinedList, options).ToList();

        // insert each new file at its sorted position
        foreach (var filePath in newFiles)
        {
            var insertIndex = sortedList.IndexOf(filePath);

            // clamp to valid range (list may have shifted from prior inserts)
            lock (_lock)
            {
                if (insertIndex < 0 || insertIndex > (int)Count)
                {
                    insertIndex = (int)Count;
                }
            }

            Add(filePath, insertIndex);
        }

        // insertions shifted indexes, so clear cache index tracking;
        // the bitmap data in Photo objects is still valid and will be
        // re-discovered by the next caching pass
        lock (_cacheLock)
        {
            _cachedIndexes.Clear();
        }

        // restore the original index
        _currentIndex = IndexOf(currentFilePath);
        var affectedCurrentFilePath = CurrentIndex != oldCurrentIndex ? currentFilePath : null;

        FileWatcherChanged?.Invoke(this, new FileWatcherChangedEventArgs(
            ChangeType.CREATED,
            newFiles,
            null,
            affectedCurrentFilePath));
    }


    /// <summary>
    /// Drains the pending-change queue and raises a single event for all changed files.
    /// The actual reload of image data is left to the subscriber.
    /// </summary>
    private void ProcessPendingChanges()
    {
        var changedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (_pendingChanges.TryDequeue(out var path))
        {
            changedSet.Add(path);
        }

        if (changedSet.Count == 0) return;
        var changedList = new List<string>(changedSet);

        // invalidate cached data for changed files so stale bitmaps are not reused
        foreach (var path in changedList)
        {
            InvalidateCacheAt(path);
            ThumbnailDiskCache.Invalidate(path);
        }

        string currentFilePath;
        lock (_lock)
        {
            currentFilePath = CurrentFilePath;
        }

        // check if the currently viewed photo was changed
        var currentWasChanged = changedList.Contains(currentFilePath, StringComparer.OrdinalIgnoreCase);
        var affectedCurrentFilePath = currentWasChanged ? currentFilePath : null;

        FileWatcherChanged?.Invoke(this, new FileWatcherChangedEventArgs(
            ChangeType.CHANGED,
            changedList,
            null,
            affectedCurrentFilePath));
    }


    /// <summary>
    /// Background thread loop that processes queued file deletions.
    /// Batches multiple rapid deletions together before raising the event.
    /// </summary>
    private void ProcessDeleteQueue()
    {
        while (_deleteWorkerRunning)
        {
            if (_deleteQueue.IsEmpty)
            {
                Thread.Sleep(200);
                continue;
            }

            // small delay to collect more items that may arrive in a burst
            Thread.Sleep(100);

            var deletedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (_deleteQueue.TryDequeue(out var filePath))
            {
                deletedSet.Add(filePath);
            }
            var deletedList = new List<string>(deletedSet);

            if (deletedList.Count == 0) continue;


            string currentFilePath;
            lock (_lock)
            {
                currentFilePath = CurrentFilePath;
            }

            // check if the currently viewed photo was deleted
            var currentWasDeleted = deletedList.Contains(currentFilePath, StringComparer.OrdinalIgnoreCase);
            var affectedCurrentFilePath = currentWasDeleted ? currentFilePath : null;

            // remove from list
            foreach (var filePath in deletedList)
            {
                Remove(filePath);
            }

            FileWatcherChanged?.Invoke(this, new FileWatcherChangedEventArgs(
                ChangeType.DELETED,
                deletedList,
                null,
                affectedCurrentFilePath));
        }
    }

    #endregion // Batch processors


}

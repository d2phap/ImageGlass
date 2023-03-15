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

using FileWatcherEx;
using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass;

public partial class FrmMain
{
    // the list of local deleted files, need to be deleted in the memory list
    private readonly List<string> _queueListForDeleting = new();

    // File system watcher
    private FileSystemWatcherEx _fileWatcher = new();


    /// <summary>
    /// Initializes the file watcher. Only do this once.
    /// </summary>
    private void SetupFileWatcher()
    {
        // Start thread to watching deleted files
        var thDeleteWorker = new Thread(new ThreadStart(ProcessDeletedFilesQueue))
        {
            Priority = ThreadPriority.BelowNormal,
            IsBackground = true,
        };

        thDeleteWorker.Start();
    }


    /// <summary>
    /// Watch a path (file or folder) for changes.
    /// </summary>
    private void StartFileWatcher(string dirPath)
    {
        // reset the watcher
        StopFileWatcher();

        if (!Config.EnableFileWatcher) return;


        // From Issue #530: file watcher currently fails nastily if given a prefixed path
        var pathToWatch = BHelper.DePrefixLongPath(dirPath);

        // Watch all changes of current path
        _fileWatcher.FolderPath = pathToWatch;

        _fileWatcher.OnCreated += FileWatcher_OnCreated;
        _fileWatcher.OnDeleted += FileWatcher_OnDeleted;
        _fileWatcher.OnChanged += FileWatcher_OnChanged;
        _fileWatcher.OnRenamed += FileWatcher_OnRenamed;

        _fileWatcher.Start();
    }


    /// <summary>
    /// Stops file watcher.
    /// </summary>
    private void StopFileWatcher()
    {
        _fileWatcher.Stop();
        _fileWatcher = new()
        {
            IncludeSubdirectories = Config.EnableRecursiveLoading,

            // auto Invoke the form if required, no need to invidiually invoke in each event
            SynchronizingObject = this,
        };

        _fileWatcher.OnCreated -= FileWatcher_OnCreated;
        _fileWatcher.OnDeleted -= FileWatcher_OnDeleted;
        _fileWatcher.OnChanged -= FileWatcher_OnChanged;
        _fileWatcher.OnRenamed -= FileWatcher_OnRenamed;
    }


    /// <summary>
    /// Disposes file watcher.
    /// </summary>
    private void DisposeFileWatcher()
    {
        StopFileWatcher();

        _fileWatcher?.Dispose();
    }



    #region File System Watcher events

    private void FileWatcher_OnRenamed(object? sender, FileChangedEvent e)
    {
        if (InvokeRequired)
        {
            Invoke(FileWatcher_OnRenamed, sender, e);
            return;
        }

        var newFilePath = e.FullPath;
        var oldFilePath = e.OldFullPath;

        var oldExt = Path.GetExtension(oldFilePath).ToLower();
        var newExt = Path.GetExtension(newFilePath).ToLower();

        // Only watch the supported file types
        if (!Config.AllFormats.Contains(oldExt) && !Config.AllFormats.Contains(newExt))
        {
            return;
        }

        // Get index of renamed image
        var imgIndex = Local.Images.IndexOf(oldFilePath);

        // if user changed file extension
        if (oldExt.CompareTo(newExt) != 0)
        {
            // [old] && [new]: update filename only
            if (Config.AllFormats.Contains(oldExt) && Config.AllFormats.Contains(newExt))
            {
                if (imgIndex > -1)
                {
                    RenameAction();
                }
            }
            else
            {
                // [old] && ![new]: remove from image list
                if (Config.AllFormats.Contains(oldExt))
                {
                    DoDeleteFiles(oldFilePath);
                }
                // ![old] && [new]: add to image list
                else if (Config.AllFormats.Contains(newExt))
                {
                    FileWatcher_AddNewFileAction(newFilePath);
                }
            }
        }
        // if user changed filename only (not extension)
        else
        {
            if (imgIndex > -1)
            {
                RenameAction();
            }
        }

        void RenameAction()
        {
            // Rename file in image list
            Local.Images.SetFileName(imgIndex, newFilePath);

            // Update status bar title
            LoadImageInfo(ImageInfoUpdateTypes.Name | ImageInfoUpdateTypes.Path);

            try
            {
                //Rename image in thumbnail bar
                Gallery.Items[imgIndex].Text = Path.GetFileName(e.FullPath);
                Gallery.Items[imgIndex].Tag = newFilePath;
            }
            catch { }

            // User renamed the initial file - update in case of list reload
            if (oldFilePath == Local.InitialInputPath)
                Local.InitialInputPath = newFilePath;
        }
    }


    private void FileWatcher_OnChanged(object? sender, FileChangedEvent e)
    {
        if (Local.IsBusy) return;

        // Only watch the supported file types
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (!Config.AllFormats.Contains(ext)) return;

        // update the viewing image
        var imgIndex = Local.Images.IndexOf(e.FullPath);

        // KBR 20180827 When downloading using Chrome, the downloaded file quickly transits
        // from ".tmp" > ".jpg.crdownload" > ".jpg". The last is a "changed" event, and the
        // final ".jpg" cannot exist in the ImageList. Fire this off to the "rename" logic
        // so the new file is correctly added. [Could it be the "created" instead?]
        if (imgIndex == -1)
        {
            Invoke(FileWatcher_OnRenamed, sender, e);
            return;
        }

        if (imgIndex == Local.CurrentIndex && string.IsNullOrEmpty(Local.ImageModifiedPath))
        {
            _ = ViewNextCancellableAsync(0, false, true);
        }

        // update thumbnail
        Gallery.Items[imgIndex].UpdateThumbnail();
    }


    private void FileWatcher_OnCreated(object? sender, FileChangedEvent e)
    {
        // Only watch the supported file types
        var ext = Path.GetExtension(e.FullPath).ToLower();

        if (!Config.AllFormats.Contains(ext))
        {
            return;
        }

        if (Local.Images.IndexOf(e.FullPath) == -1)
        {
            FileWatcher_AddNewFileAction(e.FullPath);
        }
    }


    private void FileWatcher_OnDeleted(object? sender, FileChangedEvent e)
    {
        // Only watch the supported file types
        var ext = Path.GetExtension(e.FullPath).ToLower();
        if (!Config.AllFormats.Contains(ext))
        {
            return;
        }

        // add to queue list for deleting
        _queueListForDeleting.Add(e.FullPath);
    }


    private void FileWatcher_AddNewFileAction(string filePath)
    {
        // Add the new image to the list
        Local.Images.Add(filePath);

        // Add the new image to thumbnail bar
        Gallery.Items.Add(filePath);
        Gallery.Refresh();

        // File count has changed - update title bar
        LoadImageInfo(ImageInfoUpdateTypes.ListCount);

        // display the file just added
        if (Config.ShouldAutoOpenNewAddedImage)
        {
            Local.CurrentIndex = Local.Images.Length - 1;
            _ = ViewNextCancellableAsync(0);
        }
    }


    /// <summary>
    /// The queue thread to check the files needed to be deleted.
    /// </summary>
    private void ProcessDeletedFilesQueue()
    {
        while (true)
        {
            if (_queueListForDeleting.Count > 0)
            {
                var filePath = _queueListForDeleting[0];
                _queueListForDeleting.RemoveAt(0);

                DoDeleteFiles(filePath);
            }
            else
            {
                Thread.Sleep(200);
            }
        }
    }


    /// <summary>
    /// Proceeds deleting file in memory.
    /// </summary>
    private void DoDeleteFiles(string filePath)
    {
        if (InvokeRequired)
        {
            Invoke(DoDeleteFiles, filePath);
            return;
        }

        // Get index of deleted image
        var imgIndex = Local.Images.IndexOf(filePath);

        if (imgIndex > -1)
        {
            // delete image list
            Local.Images.Remove(imgIndex);

            // delete thumbnail list
            Gallery.Items.RemoveAt(imgIndex);

            // change the viewing image to memory data mode
            if (imgIndex == Local.CurrentIndex)
            {
                if (_queueListForDeleting.Count == 0)
                {
                    _ = ViewNextCancellableAsync(0);
                }
            }

            // If user deletes the initially loaded image, use the path instead, in case
            // of list re-load.
            if (filePath == Local.InitialInputPath)
                Local.InitialInputPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        }
    }

    #endregion

}

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

using ImageGlass.Base.Photoing.Codecs;
using System.ComponentModel;

namespace ImageGlass.Base.Services;


/// <summary>
/// Image booster service.
/// </summary>
public class ImageBooster : IDisposable
{
    #region IDisposable Disposing

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // stop the worker
            IsRunWorker = false;

            // Free any other managed objects here.
            // clear list and release resources
            Reset();

            Worker?.Dispose();
        }

        // Free any unmanaged objects here.
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ImageBooster()
    {
        Dispose(false);
    }

    #endregion


    #region PRIVATE PROPERTIES

    /// <summary>
    /// Image booster background service
    /// </summary>
    private readonly BackgroundWorker Worker = new();

    /// <summary>
    /// Controls worker state
    /// </summary>
    private bool IsRunWorker { get; set; } = false;

    /// <summary>
    /// The list of Imgs
    /// </summary>
    private List<IgPhoto> ImgList { get; } = new();

    /// <summary>
    /// The list of image index that waiting for loading
    /// </summary>
    private List<int> QueuedList { get; } = new();

    /// <summary>
    /// The list of image index that waiting for releasing resource
    /// </summary>
    private List<int> FreeList { get; } = new();


    #endregion


    #region PUBLIC PROPERTIES

    /// <summary>
    /// Gets, sets codec read options
    /// </summary>
    public CodecReadOptions ReadOptions { get; set; } = new();

    /// <summary>
    /// Gets, sets the distinct directories list
    /// </summary>
    public List<string> DistinctDirs { get; set; } = new();

    /// <summary>
    /// Gets length of Img list
    /// </summary>
    public int Length => ImgList.Count;

    /// <summary>
    /// Get filenames list
    /// </summary>
    public List<string> FileNames => ImgList.Select(i => i.Filename).ToList();

    /// <summary>
    /// Gets, sets the list of formats that only load the first page forcefully.
    /// </summary>
    public HashSet<string> SinglePageFormats { get; set; } = new();

    /// <summary>
    /// Gets, sets the number of maximum items in queue list for 1 direction (Next or Back navigation).
    /// The maximum number of items in queue list is 2x + 1.
    /// </summary>
    public int MaxQueue { get; set; } = 1;

    /// <summary>
    /// Gets, sets the value of <see cref="ColorChannel"/> to apply to the entire image list.
    /// </summary>
    public ColorChannel ImageChannel { get; set; } = ColorChannel.All;

    /// <summary>
    /// Gets, sets the maximum image dimension to cache.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public int MaxImageDimensionToCache { get; set; } = 0;

    /// <summary>
    /// Gets, sets the maximum image file size (in MB) to cache.
    /// If this value is <c>less than or equals 0</c>, the option will be ignored.
    /// </summary>
    public float MaxFileSizeInMbToCache { get; set; } = 0f;


    /// <summary>
    /// Occurs when the image is loaded.
    /// </summary>
    public event EventHandler<EventArgs>? OnFinishLoadingImage;

    #endregion


    /// <summary>
    /// Initializes <see cref="ImageBooster"/> instance.
    /// </summary>
    /// <param name="codec"></param>
    public ImageBooster(IEnumerable<string>? list = null)
    {
        if (list != null)
        {
            Add(list);
        }

        // background worker
        IsRunWorker = true;
        Worker.RunWorkerAsync(RunBackgroundWorker());
    }


    #region PRIVATE FUNCTIONS

    /// <summary>
    /// Preloads the images in <see cref="QueuedList"/>.
    /// </summary>
    private async Task RunBackgroundWorker()
    {
        while (IsRunWorker)
        {
            if (QueuedList.Count > 0)
            {
                // pop out the first item
                var index = QueuedList[0];
                var img = ImgList[index];
                QueuedList.RemoveAt(0);


                if (!img.IsDone)
                {
                    // start loading image file
                    await img.LoadAsync(ReadOptions with
                    {
                        FirstFrameOnly = SinglePageFormats.Contains(img.Extension),
                    }).ConfigureAwait(false);
                }
            }

            await Task.Delay(10).ConfigureAwait(false);
        }
    }


    /// <summary>
    /// Add index of the image to queue list
    /// </summary>
    /// <param name="index">Current index of image list</param>
    private List<int> GetQueueList(int index)
    {
        // check valid index
        if (index < 0 || index >= ImgList.Count) return new List<int>(0);

        var list = new HashSet<int> { index };

        var maxCachedItems = (MaxQueue * 2) + 1;
        var iRight = index;
        var iLeft = index;

        // add index in the range in order: index -> right -> left -> ...
        for (var i = 0; list.Count < maxCachedItems && list.Count < ImgList.Count; i++)
        {
            // if i is even number
            if ((i & 1) == 0)
            {
                // add right item: [index + 1; ...; to]
                iRight++;

                if (iRight < ImgList.Count)
                {
                    list.Add(iRight);
                }
                else
                {
                    list.Add(iRight - ImgList.Count);
                }
            }
            // if i is odd number
            else
            {
                // add left item: [index - 1; ...; from]
                iLeft--;

                if (iLeft >= 0)
                {
                    list.Add(iLeft);
                }
                else
                {
                    list.Add(ImgList.Count + iLeft);
                }
            }
        }

        // release the resources
        var freeListCloned = new List<int>(FreeList);
        foreach (var itemIndex in freeListCloned)
        {
            if (!list.Contains(itemIndex) && itemIndex >= 0 && itemIndex < ImgList.Count)
            {
                ImgList[itemIndex].Dispose();
                FreeList.Remove(itemIndex);
            }
        }

        // update new index of free list
        FreeList.AddRange(list);

        // get new queue list
        var newQueueList = new List<int>();

        foreach (var itemIndex in list)
        {
            try
            {
                var metadata = PhotoCodec.LoadMetadata(ImgList[itemIndex].Filename);

                // check image dimension
                var notExceedDimension = MaxImageDimensionToCache <= 0
                    || (metadata.Width <= MaxImageDimensionToCache
                        && metadata.Height <= MaxImageDimensionToCache);

                // check file size
                var notExceedFileSize = MaxFileSizeInMbToCache <= 0
                    || (metadata.FileSize / 1024f / 1024f <= MaxFileSizeInMbToCache);

                // only put the index to the queue if it does not exceed the size limit
                if (ImgList[itemIndex].IsDone || (notExceedDimension && notExceedFileSize))
                {
                    newQueueList.Add(itemIndex);
                }
            }
            catch { }
        }

        return newQueueList;
    }

    #endregion


    #region PUBLIC FUNCTIONS


    /// <summary>
    /// Cancels loading process of a <see cref="IgPhoto"/>.
    /// </summary>
    /// <param name="index">Item index</param>
    public void CancelLoading(int index)
    {
        if (0 <= index && index < ImgList.Count)
        {
            ImgList[index].CancelLoading();
        }
    }


    /// <summary>
    /// Gets image metadat
    /// </summary>
    /// <param name="index">Image index</param>
    /// <returns></returns>
    public IgMetadata? GetMetadata(int index)
    {
        try
        {
            if (ImgList[index].Metadata is null)
            {
                ImgList[index].Metadata = PhotoCodec.LoadMetadata(
                    ImgList[index].Filename, ReadOptions);
            }

            return ImgList[index].Metadata;
        }
        catch (ArgumentOutOfRangeException) { }

        return null;
    }


    /// <summary>
    /// Get Img data
    /// </summary>
    /// <param name="index">image index</param>
    /// <param name="useCache"></param>
    /// <param name="pageIndex">The index of image page to display (if it's multi-page). Set pageIndex = int.MinValue to use defaut page index</param>
    public async Task<IgPhoto?> GetAsync(
        int index,
        bool useCache = true,
        int pageIndex = int.MinValue,
        CancellationTokenSource? tokenSrc = null)
    {
        // reload fresh new image data
        if (!useCache)
        {
            await ImgList[index].LoadAsync(ReadOptions with
            {
                FirstFrameOnly = SinglePageFormats.Contains(ImgList[index].Extension),
            }, tokenSrc).ConfigureAwait(false);
        }

        // get image data from cache
        else
        {
            // update queue list according to index
            var queueItems = GetQueueList(index);

            if (!queueItems.Contains(index))
            {
                await ImgList[index].LoadAsync(ReadOptions with
                {
                    FirstFrameOnly = SinglePageFormats.Contains(ImgList[index].Extension),
                }, tokenSrc).ConfigureAwait(false);
            }
            else
            {
                QueuedList.Clear();
                QueuedList.AddRange(queueItems);
            }
        }

        // wait until the image loading is done
        if (ImgList.Count > 0)
        {
            while (!ImgList[index].IsDone)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        // Trigger event OnFinishLoadingImage
        OnFinishLoadingImage?.Invoke(this, EventArgs.Empty);

        // if there is no error
        if (ImgList.Count > 0)
        {
            return ImgList[index];
        }

        return null;
    }



    /// <summary>
    /// Adds a file path
    /// </summary>
    /// <param name="filePath">Image file path</param>
    public void Add(string filePath)
    {
        ImgList.Add(new IgPhoto(filePath));
    }


    /// <summary>
    /// Adds multiple file paths
    /// </summary>
    /// <param name="filePaths"></param>
    public void Add(IEnumerable<string> filePaths)
    {
        // import filenames to the list
        foreach (var filename in filePaths)
        {
            Add(filename);
        }
    }


    /// <summary>
    /// Checks if the image is cached.
    /// </summary>
    public bool IsCached(int index)
    {
        try
        {
            if (ImgList.Count > 0 && ImgList[index] != null)
            {
                return ImgList[index].ImgData.IsImageNull == false;
            }
        }
        catch (ArgumentOutOfRangeException) // force reload of empty list
        { }

        return false;
    }


    /// <summary>
    /// Get filename with the given index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Returns filename or empty string</returns>
    public string GetFilePath(int index)
    {
        try
        {
            if (ImgList.Count > 0 && ImgList[index] != null)
            {
                return ImgList[index].Filename;
            }
        }
        catch (ArgumentOutOfRangeException)
        { }

        return string.Empty;
    }


    /// <summary>
    /// Set filename
    /// </summary>
    /// <param name="index"></param>
    /// <param name="filename">Image filename</param>
    public void SetFileName(int index, string filename)
    {
        if (ImgList[index] != null)
        {
            ImgList[index].Filename = filename;
        }
    }


    /// <summary>
    /// Gets file extension. Ex: <c>.jpg</c>
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetFileExtension(int index)
    {
        var filename = GetFilePath(index);

        return Path.GetExtension(filename);
    }


    /// <summary>
    /// Find index with the given filename
    /// </summary>
    /// <param name="filename">Image filename</param>
    /// <returns></returns>
    public int IndexOf(string filename)
    {
        if (string.IsNullOrEmpty(filename.Trim()))
        {
            return -1;
        }

        // case sensitivity, esp. if filename passed on command line
        return ImgList.FindIndex(item => string.Equals(item.Filename, filename, StringComparison.InvariantCultureIgnoreCase));
    }


    /// <summary>
    /// Unload and release resources of item with the given index
    /// </summary>
    public void Unload(int index)
    {
        try
        {
            ImgList[index]?.Dispose();
        }
        catch (ArgumentOutOfRangeException) { }
    }


    /// <summary>
    /// Remove an item in the list with the given index
    /// </summary>
    /// <param name="index">Item index</param>
    public void Remove(int index)
    {
        Unload(index);
        ImgList.RemoveAt(index);
    }


    /// <summary>
    /// Clears and resets all resources 
    /// </summary>
    public void Reset()
    {
        // clear list and release resources
        Clear();

        // Clear lists
        FileNames.Clear();
        QueuedList.Clear();
        FreeList.Clear();
    }


    /// <summary>
    /// Empty and release resource of the list
    /// </summary>
    public void Clear()
    {
        // release the resources of the img list
        ClearCache();
        ImgList.Clear();
    }


    /// <summary>
    /// Clear all cached images and release resource of the list
    /// </summary>
    public void ClearCache()
    {
        // release the resources of the img list
        foreach (var item in ImgList)
        {
            item.Dispose();
        }

        QueuedList.Clear();
    }


    /// <summary>
    /// Update cached images
    /// </summary>
    public void UpdateCache()
    {
        // clear current queue list
        QueuedList.Clear();

        var cachedIndexList = ImgList
            .Select((item, index) => new { ImgItem = item, Index = index })
            .Where(item => item.ImgItem.IsDone)
            .Select(item => item.Index)
            .ToList();

        // release the cachced images
        foreach (var index in cachedIndexList)
        {
            ImgList[index].Dispose();
        }

        // add to queue list
        QueuedList.AddRange(cachedIndexList);
    }


    /// <summary>
    /// Check if the folder path of input filename exists in the list
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public bool ContainsDirPathOf(string filename)
    {
        var target = Path.GetDirectoryName(filename)?.ToUpperInvariant();

        var index = ImgList.FindIndex(item => Path.GetDirectoryName(item.Filename)?.ToUpperInvariant() == target);

        return index != -1;
    }

    #endregion


}

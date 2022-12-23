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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
using ImageGlass.Base.QueuedWorker;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents the cache manager responsible for asynchronously loading
/// shell info.
/// </summary>
internal class ShellInfoCacheManager : IDisposable
{
    #region Member Variables
    private QueuedWorker bw;
    private SynchronizationContext? context;
    private readonly SendOrPostCallback checkProcessingCallback;

    private ImageGallery _imageGallery;

    private Dictionary<string, CacheItem> shellCache;
    private Dictionary<string, bool> processing;

    private bool disposed;
    #endregion


    #region CacheItem Class
    /// <summary>
    /// Represents an item in the cache.
    /// </summary>
    private class CacheItem : IDisposable
    {
        private bool disposed;

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets the small shell icon.
        /// </summary>
        public Image? SmallIcon { get; private set; }

        /// <summary>
        /// Gets the large shell icon.
        /// </summary>
        public Image? LargeIcon { get; private set; }

        /// <summary>
        /// Gets the shell file type.
        /// </summary>
        public string FileType { get; private set; }

        /// <summary>
        /// Gets or sets the state of the cache item.
        /// </summary>
        public CacheState State { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem"/> class.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="smallIcon">The small shell icon.</param>
        /// <param name="largeIcon">The large shell icon.</param>
        /// <param name="filetype">The shell file type.</param>
        /// <param name="state">The cache state of the item.</param>
        public CacheItem(string extension, Image smallIcon, Image largeIcon, string filetype, CacheState state)
        {
            Extension = extension;
            SmallIcon = smallIcon;
            LargeIcon = largeIcon;
            FileType = filetype;
            State = state;
            disposed = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                if (SmallIcon != null)
                {
                    SmallIcon.Dispose();
                    SmallIcon = null;
                }
                if (LargeIcon != null)
                {
                    LargeIcon.Dispose();
                    LargeIcon = null;
                }

                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

#if DEBUG
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// CacheItem is reclaimed by garbage collection.
        /// </summary>
        ~CacheItem()
        {
            if (SmallIcon != null || LargeIcon != null)
                System.Diagnostics.Debug.Print("Finalizer of {0} called for non-empty cache item.", GetType());
            Dispose();
        }

#endif
    }
    #endregion


    #region CanContinueProcessingEventArgs
    /// <summary>
    /// Represents the event arguments for the <see cref="CanContinueProcessing"/> callback.
    /// </summary>
    private class CanContinueProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the file extension of the request.
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets whether this item should be processed.
        /// </summary>
        public bool ContinueProcessing { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanContinueProcessingEventArgs"/> class.
        /// </summary>
        /// <param name="extension">The file extension of the request.</param>
        public CanContinueProcessingEventArgs(string extension)
        {
            Extension = extension;
            ContinueProcessing = true;
        }

    }
    #endregion


    #region Properties
    /// <summary>
    /// Determines whether the cache manager retries loading items on errors.
    /// </summary>
    public bool RetryOnError { get; internal set; }

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ShellInfoCacheManager"/> class.
    /// </summary>
    /// <param name="owner">The owner control.</param>
    public ShellInfoCacheManager(ImageGallery owner)
    {
        context = null;
        bw = new QueuedWorker
        {
            Threads = 1,
            IsBackground = true,
            ThreadName = "Shell Info Cache Worker Thread"
        };
        bw.DoWork += Bw_DoWork;
        bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

        checkProcessingCallback = new SendOrPostCallback(CanContinueProcessing);

        _imageGallery = owner;
        RetryOnError = false;

        shellCache = new Dictionary<string, CacheItem>();
        processing = new Dictionary<string, bool>();

        disposed = false;
    }

    #endregion


    #region Context Callbacks
    /// <summary>
    /// Determines if the item should be processed.
    /// </summary>
    /// <param name="extension">The file extension to check.</param>
    /// <returns>true if the item should be processed; otherwise false.</returns>
    private bool OnCanContinueProcessing(string extension)
    {
        var arg = new CanContinueProcessingEventArgs(extension);
        context?.Send(checkProcessingCallback, arg);

        return arg.ContinueProcessing;
    }

    /// <summary>
    /// Determines if the item should be processed.
    /// </summary>
    /// <param name="argument">The event argument.</param>
    /// <returns>true if the item should be processed; otherwise false.</returns>
    private void CanContinueProcessing(object? argument)
    {
        var arg = argument as CanContinueProcessingEventArgs;
        if (arg is null) return;

        bool canProcess = true;

        // Is it already cached?
        if (shellCache.TryGetValue(arg.Extension, out CacheItem? existing))
        {
            if (existing.SmallIcon != null && existing.LargeIcon != null)
                canProcess = false;
        }

        arg.ContinueProcessing = canProcess;
    }
    
    #endregion
    

    #region QueuedBackgroundWorker Events
    /// <summary>
    /// Handles the RunWorkerCompleted event of the queued background worker.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="QueuedWorkerCompletedEventArgs"/> 
    /// instance containing the event data.</param>
    private void Bw_RunWorkerCompleted(object? sender, QueuedWorkerCompletedEventArgs e)
    {
        var result = e.Result as CacheItem;

        // Add to cache
        if (result != null)
        {
            // We are done processing
            processing.Remove(result.Extension);

            if (shellCache.TryGetValue(result.Extension, out CacheItem? existing))
            {
                existing.Dispose();
                shellCache.Remove(result.Extension);
            }
            shellCache.Add(result.Extension, result);
        }

        // Refresh the control lazily
        if (result != null && _imageGallery != null)
            _imageGallery.Refresh(false, true);

        // Raise the ShellInfoCached event
        if (result != null && _imageGallery != null)
            _imageGallery.OnShellInfoCached(new ShellInfoCachedEventArgs(result.Extension, result.SmallIcon, result.LargeIcon, result.FileType));

        // Raise the CacheError event
        if (e.Error != null && _imageGallery != null)
            _imageGallery.OnCacheErrorInternal(e.Error, CacheThread.ShellInfo);
    }

    /// <summary>
    /// Handles the DoWork event of the queued background worker.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="QueuedWorkerDoWorkEventArgs"/> instance 
    /// containing the event data.</param>
    private void Bw_DoWork(object? sender, QueuedWorkerDoWorkEventArgs e)
    {
        var extension = e.Argument as string;

        // Should we continue processing this item?
        // The callback checks if the item is already cached.
        if (!OnCanContinueProcessing(extension))
        {
            e.Cancel = true;
            return;
        }

        // Read shell info
        var info = Extractor.Current.GetShellInfo(extension);

        // Return the info
        CacheItem result;
        if ((info.SmallIcon == null || info.LargeIcon == null) && !RetryOnError)
            result = new CacheItem(extension, info.SmallIcon, info.LargeIcon, info.FileType, CacheState.Error);
        else
            result = new CacheItem(extension, info.SmallIcon, info.LargeIcon, info.FileType, CacheState.Cached);

        e.Result = result;
    }

    #endregion


    #region Instance Methods
    /// <summary>
    /// Pauses the cache threads. 
    /// </summary>
    public void Pause()
    {
        bw.Pause();
    }

    /// <summary>
    /// Resumes the cache threads. 
    /// </summary>
    public void Resume()
    {
        bw.Resume();
    }

    /// <summary>
    /// Gets the cache state of the specified item.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public CacheState GetCacheState(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        if (shellCache.TryGetValue(extension, out CacheItem? item))
            return item.State;

        return CacheState.Unknown;
    }

    /// <summary>
    /// Rebuilds the cache.
    /// Old items will be kept until they are overwritten
    /// by new ones.
    /// </summary>
    public void Rebuild()
    {
        foreach (CacheItem item in shellCache.Values)
            item.State = CacheState.Unknown;
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear()
    {
        foreach (CacheItem item in shellCache.Values)
            item.Dispose();

        shellCache.Clear();
        processing.Clear();
    }

    /// <summary>
    /// Removes the given item from the cache.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public void Remove(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        if (shellCache.TryGetValue(extension, out CacheItem? item))
        {
            item.Dispose();
            shellCache.Remove(extension);
        }
    }

    /// <summary>
    /// Adds the item to the cache queue.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public void Add(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        // Already cached?
        if (shellCache.TryGetValue(extension, out _))
            return;

        // Add to cache queue
        RunWorker(extension);
    }

    /// <summary>
    /// Gets the small shell icon for the given file extension from the cache.
    /// If the item is not cached, null will be returned.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public Image? GetSmallIcon(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        if (shellCache.TryGetValue(extension, out CacheItem? item))
        {
            return item.SmallIcon;
        }
        return null;
    }

    /// <summary>
    /// Gets the large shell icon for the given file extension from the cache.
    /// If the item is not cached, null will be returned.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public Image? GetLargeIcon(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        if (shellCache.TryGetValue(extension, out CacheItem? item))
        {
            return item.LargeIcon;
        }
        return null;
    }

    /// <summary>
    /// Gets the shell file type for the given file extension from the cache.
    /// If the item is not cached, null will be returned.
    /// </summary>
    /// <param name="extension">File extension.</param>
    public string GetFileType(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            throw new ArgumentException("extension cannot be null", nameof(extension));

        if (shellCache.TryGetValue(extension, out CacheItem? item))
        {
            return item.FileType;
        }
        return string.Empty;
    }

    #endregion


    #region RunWorker
    /// <summary>
    /// Pushes the given item to the worker queue.
    /// </summary>
    /// <param name="extension">File extension.</param>
    private void RunWorker(string extension)
    {
        // Get the current synchronization context
        if (context == null)
            context = SynchronizationContext.Current;

        // Already being processed?
        if (processing.ContainsKey(extension))
            return;
        else
            processing.Add(extension, false);

        // Raise the ShellInfoCaching event
        if (_imageGallery != null)
            _imageGallery.OnShellInfoCaching(new ShellInfoCachingEventArgs(extension));

        // Add the item to the queue for processing
        bw.RunWorkerAsync(extension);
    }

    #endregion


    #region Dispose
    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            bw.DoWork -= Bw_DoWork;
            bw.RunWorkerCompleted -= Bw_RunWorkerCompleted;

            Clear();
            bw.Dispose();

            disposed = true;

            GC.SuppressFinalize(this);
        }
    }

#if DEBUG
    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="ShellInfoCacheManager"/> is reclaimed by garbage collection.
    /// </summary>
    ~ShellInfoCacheManager()
    {
        System.Diagnostics.Debug.Print("Finalizer of {0} called.", GetType());
        Dispose();
    }

#endif
    #endregion
}

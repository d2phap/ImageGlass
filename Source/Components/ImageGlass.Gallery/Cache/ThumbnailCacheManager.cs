/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Base.Cache;
using ImageGlass.Base.QueuedWorker;

namespace ImageGlass.Gallery;

/// <summary>
/// Represents the cache manager responsible for asynchronously loading
/// item thumbnails.
/// </summary>
internal class ThumbnailCacheManager : IDisposable
{
    #region Member Variables
    private readonly QueuedWorker _bw = new()
    {
        ProcessingMode = ProcessingMode.LIFO,
        IsBackground = true,
        ThreadName = "Thumbnail cache worker thread",
    };
    private SynchronizationContext? _context = null;
    private readonly SendOrPostCallback _checkProcessingCallback;

    internal DiskCache _diskCache = new()
    {
        CacheSize = 100 * 1024 * 1024, // 100 MB disk cache
    };
    private readonly Dictionary<Guid, CacheItem> _thumbCache = new();
    private readonly Dictionary<Guid, bool> _processing = new();
    private readonly Dictionary<Guid, bool> _editCache = new();
    private readonly List<Guid> _removedItems = new();

    private Guid _processingRendererItem = Guid.Empty;
    private Guid _processingGalleryItem = Guid.Empty;
    private CacheItem? _rendererItem = null;
    private CacheItem? _galleryItem = null;

    private readonly ImageGallery _imageGallery;
    private bool _isDisposed = false;

    #endregion


    #region RequestType Enum
    private enum RequestType
    {
        /// <summary>
        /// This is a thumbnail request.
        /// </summary>
        Thumbnail,

        /// <summary>
        /// [TODO] This is a large image request for use in Gallery or Pane view modes.
        /// </summary>
        Gallery,

        /// <summary>
        /// This is a renderer request.
        /// </summary>
        Renderer
    }
    #endregion


    #region CacheRequest Class
    /// <summary>
    /// Represents a cache request.
    /// </summary>
    private class CacheRequest
    {
        /// <summary>
        /// Gets the guid of the item.
        /// </summary>
        public Guid Guid { get; private set; }
        /// <summary>
        /// Gets the adaptor of this item.
        /// </summary>
        public IAdaptor Adaptor { get; private set; }
        /// <summary>
        /// Gets the public key for the virtual item.
        /// </summary>
        public object VirtualItemKey { get; private set; }
        /// <summary>
        /// Gets the size of the requested thumbnail.
        /// </summary>
        public Size Size { get; private set; }
        /// <summary>
        /// Gets embedded thumbnail extraction behavior.
        /// </summary>
        public UseEmbeddedThumbnails UseEmbeddedThumbnails { get; private set; }
        /// <summary>
        /// Gets Exif rotation behavior.
        /// </summary>
        public bool AutoRotate { get; private set; }
        /// <summary>
        /// Gets the type of this request.
        /// </summary>
        public RequestType RequestType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheRequest"/> class
        /// for use with a virtual item.
        /// </summary>
        /// <param name="guid">The guid of the <see cref="ImageGalleryItem"/>.</param>
        /// <param name="adaptor">The adaptor of this item.</param>
        /// <param name="key">The public key for the virtual item.</param>
        /// <param name="size">The size of the requested thumbnail.</param>
        /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
        /// <param name="autoRotate">AutoRotate property of the owner control.</param>
        /// <param name="requestType">Type of this request.</param>
        public CacheRequest(Guid guid, IAdaptor adaptor, object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate, RequestType requestType)
        {
            Guid = guid;
            VirtualItemKey = key;
            Adaptor = adaptor;
            Size = size;
            UseEmbeddedThumbnails = useEmbeddedThumbnails;
            AutoRotate = autoRotate;
            RequestType = requestType;
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return "CacheRequest (" + VirtualItemKey.ToString() + ")";
        }
    }
    #endregion


    #region CacheItem Class
    /// <summary>
    /// Represents an item in the thumbnail cache.
    /// </summary>
    private class CacheItem : IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Gets the guid of the item.
        /// </summary>
        public Guid Guid { get; private set; }

        /// <summary>
        /// Gets the size of the requested thumbnail.
        /// </summary>
        public Size Size { get; private set; }

        /// <summary>
        /// Gets the cached image.
        /// </summary>
        public Image? Image { get; private set; }

        /// <summary>
        /// Gets or sets the state of the cache item.
        /// </summary>
        public CacheState State { get; set; }

        /// <summary>
        /// Gets embedded thumbnail extraction behavior.
        /// </summary>
        public UseEmbeddedThumbnails UseEmbeddedThumbnails { get; private set; }

        /// <summary>
        /// Gets Exif rotation behavior.
        /// </summary>
        public bool AutoRotate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem"/> class
        /// for use with a virtual item.
        /// </summary>
        /// <param name="guid">The guid of the <see cref="ImageGalleryItem"/>.</param>
        /// <param name="size">The size of the requested thumbnail.</param>
        /// <param name="image">The thumbnail image.</param>
        /// <param name="state">The cache state of the item.</param>
        /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
        /// <param name="autoRotate">AutoRotate property of the owner control.</param>
        public CacheItem(Guid guid, Size size, Image? image, CacheState state, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
        {
            Guid = guid;
            Size = size;
            Image = image;
            State = state;
            UseEmbeddedThumbnails = useEmbeddedThumbnails;
            AutoRotate = autoRotate;
            _isDisposed = false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                if (Image != null)
                {
                    Image.Dispose();
                    Image = null;
                }

                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

#if DEBUG
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// class is reclaimed by garbage collection.
        /// </summary>
        ~CacheItem()
        {
            if (Image != null)
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
        /// Gets the request.
        /// </summary>
        public CacheRequest Request { get; private set; }

        /// <summary>
        /// Gets whether this item should be processed.
        /// </summary>
        public bool ContinueProcessing { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CanContinueProcessingEventArgs"/> class.
        /// </summary>
        /// <param name="request">The cache request.</param>
        public CanContinueProcessingEventArgs(CacheRequest request)
        {
            Request = request;
            ContinueProcessing = true;
        }
    }

    #endregion


    #region Properties
    /// <summary>
    /// Determines whether the cache manager retries loading items on errors.
    /// </summary>
    public bool RetryOnError { get; internal set; } = false;

    /// <summary>
    /// Gets or sets the cache mode.
    /// </summary>
    public CacheMode CacheMode { get; internal set; } = CacheMode.OnDemand;

    /// <summary>
    /// Gets or sets the cache limit as count of items.
    /// </summary>
    public int CacheLimitAsItemCount { get; internal set; } = 0;

    /// <summary>
    /// Gets or sets the cache limit as allocated memory in MB.
    /// </summary>
    public long CacheLimitAsMemory { get; internal set; } = 20 * 1024 * 1024;

    /// <summary>
    /// Gets the approximate amount of memory used by the cache.
    /// </summary>
    public long MemoryUsed { get; private set; } = 0;

    /// <summary>
    /// Gets the approximate amount of memory used by removed items in the cache.
    /// This memory can be reclaimed by calling <see cref="Purge()"/>.
    /// </summary>
    public long MemoryUsedByRemoved { get; private set; } = 0;

    /// <summary>
    /// Returns the count of items in the cache.
    /// </summary>
    public long CacheSize => _thumbCache.Count;

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ThumbnailCacheManager"/> class.
    /// </summary>
    /// <param name="owner">The owner control.</param>
    public ThumbnailCacheManager(ImageGallery owner)
    {
        _bw.DoWork += Bw_DoWork;
        _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;

        _checkProcessingCallback = new SendOrPostCallback(CanContinueProcessing);
        _imageGallery = owner;
    }

    #endregion


    #region Context Callbacks

    /// <summary>
    /// Determines if the item should be processed.
    /// </summary>
    /// <param name="item">The <see cref="CacheItem"/> to check.</param>
    /// <returns>true if the item should be processed; otherwise false.</returns>
    private bool OnCanContinueProcessing(CacheRequest? item)
    {
        if (item is null) return false;

        var arg = new CanContinueProcessingEventArgs(item);
        _context?.Send(_checkProcessingCallback, arg);

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

        var request = arg.Request;
        if (request is null) return;

        var canProcess = true;

        // Is it in the edit cache?
        if (canProcess && _editCache.ContainsKey(request.Guid))
            canProcess = false;

        // Is it already cached?
        if (canProcess && (request?.RequestType == RequestType.Thumbnail))
        {
            _thumbCache.TryGetValue(request.Guid, out CacheItem? existing);
            if (existing != null
                && existing.Size == request.Size
                && existing.UseEmbeddedThumbnails == request.UseEmbeddedThumbnails
                && existing.AutoRotate == request.AutoRotate)
                canProcess = false;

            // Is it outside the visible area?
            if (canProcess
                && CacheMode == CacheMode.OnDemand
                && _imageGallery != null
                && !_imageGallery.IsItemVisible(request.Guid))
                canProcess = false;
        }
        else if (canProcess && (request?.RequestType == RequestType.Gallery))
        {
            var existing = _galleryItem;
            if (existing != null
                && existing.Guid == request.Guid
                && existing.Size == request.Size
                && existing.UseEmbeddedThumbnails == request.UseEmbeddedThumbnails
                && existing.AutoRotate == request.AutoRotate)
                canProcess = false;
        }
        else if (canProcess && (request?.RequestType == RequestType.Renderer))
        {
            var existing = _rendererItem;
            if (existing != null
                && existing.Guid == request.Guid
                && existing.Size == request.Size
                && existing.UseEmbeddedThumbnails == request.UseEmbeddedThumbnails
                && existing.AutoRotate == request.AutoRotate)
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
        var request = e.UserState as CacheRequest;
        if (request is null) return;

        var result = e.Result as CacheItem;

        // We are done processing
        if (request.RequestType == RequestType.Renderer)
            _processingRendererItem = Guid.Empty;
        else if (request.RequestType == RequestType.Gallery)
            _processingGalleryItem = Guid.Empty;
        else
            _processing.Remove(request.Guid);

        // Do not process the result if the cache operation was cancelled.
        if (e.Cancelled) return;

        // Dispose old item and add to cache
        if (request.RequestType == RequestType.Renderer)
        {
            if (_rendererItem != null)
                _rendererItem.Dispose();

            _rendererItem = result;
        }
        else if (request.RequestType == RequestType.Gallery)
        {
            if (_galleryItem != null)
                _galleryItem.Dispose();

            _galleryItem = result;
        }
        else if (result != null)
        {
            if (_thumbCache.TryGetValue(result.Guid, out CacheItem? existing))
            {
                existing.Dispose();
                _thumbCache.Remove(result.Guid);
            }
            _thumbCache.Add(result.Guid, result);

            if (result.Image != null)
            {
                // Did the thumbnail size change while we were
                // creating the thumbnail?
                if (result.Size != _imageGallery.ThumbnailSize)
                    result.State = CacheState.Unknown;

                // Purge invisible items if we exceeded the cache limit
                MemoryUsed += GetImageMemorySize(result.Image);
                if (IsCacheLimitExceeded())
                    PurgeInvisible(true);
            }
        }

        // Refresh the control
        if (_imageGallery != null)
        {
            if (request.RequestType != RequestType.Thumbnail || _imageGallery.IsItemVisible(request.Guid))
                _imageGallery.Refresh(false, true);
        }

        // Raise the ThumbnailCached event
        if (result != null && _imageGallery != null)
            _imageGallery.OnThumbnailCachedInternal(result.Guid, result.Image, result.Size, request.RequestType == RequestType.Thumbnail);

        // Raise the CacheError event
        if (e.Error != null && _imageGallery != null)
            _imageGallery.OnCacheErrorInternal(request.Guid, e.Error, CacheThread.Thumbnail);
    }

    /// <summary>
    /// Handles the DoWork event of the queued background worker.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="QueuedWorkerDoWorkEventArgs"/> instance 
    /// containing the event data.</param>
    private void Bw_DoWork(object? sender, QueuedWorkerDoWorkEventArgs e)
    {
        var request = e.Argument as CacheRequest;
        if (request is null) return;

        // Should we continue processing this item?
        // The callback checks the following and returns false if
        //   the item is already cached -OR-
        //   the item is in the edit cache -OR-
        //   the item is outside the visible area (only if the CacheMode is OnDemand).
        if (!OnCanContinueProcessing(request))
        {
            e.Cancel = true;
            return;
        }

        Image? thumb = null;
        var diskCacheKey = request.Adaptor.GetUniqueIdentifier(request.VirtualItemKey, request.Size, request.UseEmbeddedThumbnails, request.AutoRotate);

        // Check the disk cache
        using var cs = _diskCache.Read(diskCacheKey);
        if (cs.Length > 0)
        {
            thumb = new Bitmap(cs);
        }


        // Extract the thumbnail from the source image.
        if (thumb == null)
        {
            try
            {
                thumb = request.Adaptor.GetThumbnail(request.VirtualItemKey, request.Size, request.UseEmbeddedThumbnails, request.AutoRotate);
            }
            // fix infinite re-cache when throwing error
            catch { }

            // Save to disk cache
            if (thumb != null)
            {
                using var ms = new MemoryStream();
                thumb.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                _diskCache.Write(diskCacheKey, ms);
            }
        }

        // Return the thumbnail
        CacheItem? result;
        if (thumb == null && !RetryOnError)
        {
            result = new CacheItem(request.Guid, request.Size, null, CacheState.Error, request.UseEmbeddedThumbnails, request.AutoRotate);
        }
        else
        {
            result = new CacheItem(request.Guid, request.Size, thumb, CacheState.Cached, request.UseEmbeddedThumbnails, request.AutoRotate);
        }

        e.Result = result;
    }

    #endregion


    #region Instance Methods
    /// <summary>
    /// Pauses the cache threads. 
    /// </summary>
    public void Pause()
    {
        _bw.Pause();
    }

    /// <summary>
    /// Resumes the cache threads. 
    /// </summary>
    public void Resume()
    {
        _bw.Resume();
    }

    /// <summary>
    /// Starts editing an item. While items are edited,
    /// the cache thread will not work on them to prevent collisions.
    /// </summary>
    /// <param name="guid">The guid representing the item</param>
    public void BeginItemEdit(Guid guid)
    {
        try
        {
            _ = _editCache.TryAdd(guid, false);
        }
        catch { }
    }

    /// <summary>
    /// Ends editing an item. After this call, item
    /// image will be continued to be fetched by the thread.
    /// </summary>
    /// <param name="guid">The guid representing the item.</param>
    public void EndItemEdit(Guid guid)
    {
        _ = _editCache.Remove(guid);
    }

    /// <summary>
    /// Rebuilds the thumbnail cache.
    /// Old thumbnails will be kept until they are overwritten
    /// by new ones.
    /// </summary>
    public void Rebuild()
    {
        foreach (var item in _thumbCache.Values)
            item.State = CacheState.Unknown;

        if (_galleryItem != null)
            _galleryItem.State = CacheState.Unknown;

        _diskCache.Clear();
    }

    /// <summary>
    /// Clears the thumbnail cache.
    /// </summary>
    public void Clear()
    {
        foreach (var item in _thumbCache.Values)
        {
            item.Dispose();
        }
        _thumbCache.Clear();

        _galleryItem?.Dispose();
        _galleryItem = null;

        _rendererItem?.Dispose();
        _rendererItem = null;

        _bw.CancelAsync();

        MemoryUsed = 0;
        MemoryUsedByRemoved = 0;
        _removedItems.Clear();
        _processing.Clear();
        _processingRendererItem = Guid.Empty;


        // Empty persistent cache
        if (!_imageGallery.DoNotDeletePersistentCache)
        {
            _diskCache.Clear();
        }
    }

    /// <summary>
    /// Removes the given item from the cache.
    /// </summary>
    /// <param name="guid">The guid of the item to remove.</param>
    public void Remove(Guid guid)
    {
        Remove(guid, false);
    }

    /// <summary>
    /// Removes the given item from the cache.
    /// </summary>
    /// <param name="guid">The guid of the item to remove.</param>
    /// <param name="removeNow">true to remove the item now; false to remove the
    /// item later when the cache is purged.</param>
    public void Remove(Guid guid, bool removeNow)
    {
        if (!_thumbCache.TryGetValue(guid, out CacheItem? cacheItem))
            return;

        if (removeNow)
        {
            MemoryUsed -= GetImageMemorySize(cacheItem.Size.Width, cacheItem.Size.Height);
            cacheItem.Dispose();
            _thumbCache.Remove(guid);

            if (_galleryItem != null && _galleryItem.Guid == guid)
            {
                _galleryItem.Dispose();
                _galleryItem = null;
            }
        }
        else
        {
            MemoryUsedByRemoved += GetImageMemorySize(cacheItem.Size.Width, cacheItem.Size.Height);
            _removedItems.Add(guid);

            Purge();
        }

        // Remove from disk cache
        if (_imageGallery != null && _imageGallery.Items.TryGetValue(guid, out ImageGalleryItem? item))
        {
            if (item != null)
            {
                var diskCacheKey = item.Adaptor.GetUniqueIdentifier(
                    item.VirtualItemKey,
                    cacheItem.Size,
                    cacheItem.UseEmbeddedThumbnails,
                    cacheItem.AutoRotate);
                _diskCache.Remove(diskCacheKey);
            }
        }
    }

    /// <summary>
    /// Purges removed items from the cache.
    /// </summary>
    /// <param name="force">true to purge the cache now, regardless of
    /// memory usage; otherwise false to automatically purge the cache
    /// depending on memory usage.</param>
    public void Purge(bool force)
    {
        // Remove items now if we can free more than 25% of the cache limit
        if (force || IsPurgeNeeded())
        {
            foreach (var guid in _removedItems)
            {
                if (_thumbCache.TryGetValue(guid, out CacheItem? item))
                {
                    item?.Dispose();
                    _thumbCache.Remove(guid);
                }

                if (_galleryItem != null && _galleryItem.Guid == guid)
                {
                    _galleryItem.Dispose();
                    _galleryItem = null;
                }
            }

            _removedItems.Clear();
            MemoryUsed -= MemoryUsedByRemoved;
            MemoryUsedByRemoved = 0;
        }
    }

    /// <summary>
    /// Purges removed items from the cache automatically
    /// depending on memory usage.
    /// </summary>
    public void Purge()
    {
        Purge(false);
    }

    /// <summary>
    /// Purges invisible items from the cache.
    /// </summary>
    /// <param name="force">true to purge the cache now, regardless of
    /// memory usage; otherwise false to automatically purge the cache
    /// depending on memory usage.</param>
    public void PurgeInvisible(bool force)
    {
        if (_imageGallery == null) return;

        var visible = _imageGallery.GetVisibleItems();
        if (visible.Count == 0) return;

        foreach (var item in _thumbCache)
        {
            if (!visible.ContainsKey(item.Key))
            {
                _removedItems.Add(item.Key);
                MemoryUsedByRemoved += GetImageMemorySize(item.Value.Image);
            }
        }

        Purge(force);
    }

    /// <summary>
    /// Determines if removed items need to be purged. Removed items are purged
    /// if they take up more than 25% of the cache limit.
    /// </summary>
    /// <returns>true if removed items need to be purged; otherwise false.</returns>
    private bool IsPurgeNeeded()
    {
        return (CacheLimitAsMemory != 0 && MemoryUsedByRemoved > CacheLimitAsMemory / 4)
            || (CacheLimitAsItemCount != 0 && _removedItems.Count > CacheLimitAsItemCount / 4);
    }

    /// <summary>
    /// Determines if the cache limit is exceeded.
    /// </summary>
    /// <returns>true if the cache limit is exceeded; otherwise false.</returns>
    private bool IsCacheLimitExceeded()
    {
        return (CacheLimitAsMemory != 0 && MemoryUsedByRemoved > CacheLimitAsMemory)
            || (CacheLimitAsItemCount != 0 && _removedItems.Count > CacheLimitAsItemCount);
    }

    /// <summary>
    /// Returns the memory usage of an image.
    /// </summary>
    /// <param name="image">A image.</param>
    /// <returns>Memory size of the image.</returns>
    private static int GetImageMemorySize(Image? image)
    {
        if (image != null)
            return GetImageMemorySize(image.Width, image.Height);
        else
            return 0;
    }

    /// <summary>
    /// Returns the memory usage of an image in of given dimensions.
    /// The value is calculated aproximately as (Width * Height * BitsPerPixel / 8)
    /// </summary>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <returns>Memory size of the image.</returns>
    private static int GetImageMemorySize(int width, int height)
    {
        return width * height * 24 / 8;
    }

    /// <summary>
    /// Adds a virtual item to the cache queue.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="adaptor">he adaptor for this item.</param>
    /// <param name="key">The key of this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public void Add(Guid guid, IAdaptor adaptor, object key, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        // Already cached?
        if (_thumbCache.TryGetValue(guid, out CacheItem? item))
        {
            if (item.Size == thumbSize && item.UseEmbeddedThumbnails == useEmbeddedThumbnails)
                return;
        }

        // Add to cache queue
        RunWorker(new CacheRequest(guid, adaptor, key, thumbSize, useEmbeddedThumbnails, autoRotate, RequestType.Thumbnail));
    }

    /// <summary>
    /// Adds a virtual item to the cache.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="adaptor">The adaptor for this item.</param>
    /// <param name="key">The key of this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="thumb">Thumbnail image to add to cache.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public void Add(Guid guid, IAdaptor adaptor, object key, Size thumbSize, Image? thumb, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        // Already cached?
        if (_thumbCache.TryGetValue(guid, out CacheItem? item))
        {
            if (item.Size == thumbSize && item.UseEmbeddedThumbnails == useEmbeddedThumbnails)
                return;
        }

        // Resize
        thumb = Extractor.Current.GetThumbnail(thumb, thumbSize, useEmbeddedThumbnails, autoRotate);

        // Add to cache
        _thumbCache.Add(guid, new CacheItem(guid, thumbSize, thumb, CacheState.Cached, useEmbeddedThumbnails, autoRotate));

        // Add to disk cache
        using var stream = new MemoryStream();
        var diskCacheKey = adaptor.GetUniqueIdentifier(key, thumbSize, useEmbeddedThumbnails, autoRotate);
        thumb.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        _diskCache.Write(diskCacheKey, stream);


        // Raise the cache events
        if (_imageGallery != null)
        {
            _imageGallery.OnThumbnailCachedInternal(guid, thumb, thumbSize, true);
            _imageGallery.Refresh();
        }
    }

    /// <summary>
    /// Adds the virtual item image to the gallery cache queue.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="adaptor">The adaptor for this item.</param>
    /// <param name="key">The key of this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public void AddToGalleryCache(Guid guid, IAdaptor adaptor, object key, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        // Already cached?
        if (_galleryItem != null
            && _galleryItem.Guid == guid
            && _galleryItem.Image != null
            && _galleryItem.Size == thumbSize
            && _galleryItem.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && _galleryItem.AutoRotate == autoRotate)
            return;

        // Add to cache queue
        RunWorker(new CacheRequest(guid, adaptor, key, thumbSize, useEmbeddedThumbnails, autoRotate, RequestType.Gallery), 2);
    }

    /// <summary>
    /// Adds the virtual item image to the renderer cache queue.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="adaptor">The adaptor of this item.</param>
    /// <param name="key">The key of this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public void AddToRendererCache(Guid guid, IAdaptor adaptor, object key, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        // Already cached?
        if (_rendererItem != null
            && _rendererItem.Guid == guid
            && _rendererItem.Image != null
            && _rendererItem.Size == thumbSize
            && _rendererItem.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && _rendererItem.AutoRotate == autoRotate)
            return;

        // Add to cache queue
        RunWorker(new CacheRequest(guid, adaptor, key, thumbSize, useEmbeddedThumbnails, autoRotate, RequestType.Renderer), 1);
    }

    /// <summary>
    /// Gets the image from the renderer cache. If the image is not cached,
    /// null will be returned.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public Image? GetRendererImage(Guid guid, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        if (_rendererItem != null
            && _rendererItem.Guid == guid
            && _rendererItem.Image != null
            && _rendererItem.Size == thumbSize
            && _rendererItem.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && _rendererItem.AutoRotate == autoRotate)
            return _rendererItem.Image;
        else
            return null;
    }

    /// <summary>
    /// Gets the image from the gallery cache. If the image is not cached,
    /// null will be returned.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public Image? GetGalleryImage(Guid guid, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        if (_galleryItem != null
            && _galleryItem.Guid == guid
            && _galleryItem.Image != null
            && _galleryItem.Size == thumbSize
            && _galleryItem.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && _galleryItem.AutoRotate == autoRotate)
            return _galleryItem.Image;
        else
            return null;
    }

    /// <summary>
    /// Gets the image from the thumbnail cache. If the image is not cached,
    /// null will be returned.
    /// </summary>
    /// <param name="guid">The guid representing this item.</param>
    /// <param name="adaptor">The adaptor of this item.</param>
    /// <param name="key">The key of this item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    /// <param name="clone">true to return a clone of the cached image; otherwise false.</param>
    public Image? GetImage(Guid guid, IAdaptor adaptor, object key, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate, bool clone)
    {
        if (_thumbCache.TryGetValue(guid, out CacheItem? item)
            && item != null
            && item.Image != null
            && item.Size == thumbSize
            && item.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && item.AutoRotate == autoRotate)
        {
            if (clone)
            {
                lock (item.Image)
                {
                    return (Image?)item.Image.Clone();
                }
            }
            return item.Image;
        }
        else
        {
            var diskCacheKey = adaptor.GetUniqueIdentifier(key, thumbSize, useEmbeddedThumbnails, autoRotate);

            // Check the disk cache
            using var stream = _diskCache.Read(diskCacheKey);

            if (stream.Length > 0)
            {
                return new Bitmap(stream);
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the cache state of the specified item.
    /// </summary>
    /// <param name="guid">The guid representing the item.</param>
    /// <param name="thumbSize">Requested thumbnail size.</param>
    /// <param name="useEmbeddedThumbnails">UseEmbeddedThumbnails property of the owner control.</param>
    /// <param name="autoRotate">AutoRotate property of the owner control.</param>
    public CacheState GetCacheState(Guid guid, Size thumbSize, UseEmbeddedThumbnails useEmbeddedThumbnails, bool autoRotate)
    {
        if (_thumbCache.TryGetValue(guid, out CacheItem? item)
            && item != null
            && item.Size == thumbSize
            && item.UseEmbeddedThumbnails == useEmbeddedThumbnails
            && item.AutoRotate == autoRotate)
            return item.State;

        return CacheState.Unknown;
    }

    #endregion


    #region RunWorker
    /// <summary>
    /// Pushes the given item to the worker queue. Items with high priority are renderer 
    /// or gallery items, ie. large images in gallery and pane views and images requested 
    /// by custom renderers. Items with 0 priority are regular thumbnails.
    /// </summary>
    /// <param name="item">The item to add to the worker queue.</param>
    /// <param name="priority">Priority of the item in the queue.</param>
    private void RunWorker(CacheRequest item, int priority)
    {
        // Get the current synchronization context
        if (_context == null)
            _context = SynchronizationContext.Current;

        // Already being processed?
        if (item.RequestType == RequestType.Thumbnail)
        {
            if (_processing.ContainsKey(item.Guid))
                return;
            else
                _processing.Add(item.Guid, false);
        }
        else if (item.RequestType == RequestType.Renderer)
        {
            if (_processingRendererItem == item.Guid)
                return;
            else
            {
                _bw.CancelAsync(priority);
                _processingRendererItem = item.Guid;
            }
        }
        else if (item.RequestType == RequestType.Gallery)
        {
            if (_processingGalleryItem == item.Guid)
                return;
            else
            {
                _bw.CancelAsync(priority);
                _processingGalleryItem = item.Guid;
            }
        }

        // Raise the ThumbnailCaching event
        if (_imageGallery != null)
            _imageGallery.OnThumbnailCachingInternal(item.Guid, item.Size);

        // Add the item to the queue for processing
        _bw.RunWorkerAsync(item, priority, item.RequestType != RequestType.Thumbnail);
    }

    /// <summary>
    /// Pushes the given item to the worker queue.
    /// </summary>
    /// <param name="item">The item to add to the worker queue.</param>
    private void RunWorker(CacheRequest item)
    {
        RunWorker(item, 0);
    }

    #endregion


    #region Dispose
    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _bw.DoWork -= Bw_DoWork;
            _bw.RunWorkerCompleted -= Bw_RunWorkerCompleted;

            Clear();
            _bw.Dispose();

            _isDisposed = true;

            GC.SuppressFinalize(this);
        }
    }
#if DEBUG
    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="ThumbnailCacheManager"/> is reclaimed by garbage collection.
    /// </summary>
    ~ThumbnailCacheManager()
    {
        System.Diagnostics.Debug.Print("Finalizer of {0} called.", GetType());
        Dispose();
    }

#endif

    #endregion

}


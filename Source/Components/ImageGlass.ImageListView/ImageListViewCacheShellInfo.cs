// ImageListView - A listview control for image files
// Copyright (C) 2009 Ozgur Ozcitak
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Ozgur Ozcitak (ozcitak@yahoo.com)

// Dictionary<> is not thread safe if modified while being read.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ImageGlass.ImageListView {
    /// <summary>
    /// Represents the cache manager responsible for asynchronously loading
    /// shell info.
    /// </summary>
    internal class ImageListViewCacheShellInfo: IDisposable {
        #region Member Variables
        private QueuedBackgroundWorker bw;
        private SynchronizationContext context;
        private SendOrPostCallback checkProcessingCallback;

        private ImageListView mImageListView;

        private Dictionary<string, CacheItem> shellCache;
        private ConcurrentDictionary<string, bool> processing;

        private bool disposed;
        #endregion

        #region CacheItem Class
        /// <summary>
        /// Represents an item in the cache.
        /// </summary>
        private class CacheItem: IDisposable {
            private bool disposed;

            /// <summary>
            /// Gets the file extension.
            /// </summary>
            public string Extension { get; private set; }
            /// <summary>
            /// Gets the small shell icon.
            /// </summary>
            public Image SmallIcon { get; private set; }
            /// <summary>
            /// Gets the large shell icon.
            /// </summary>
            public Image LargeIcon { get; private set; }
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
            public CacheItem(string extension, Image smallIcon, Image largeIcon, string filetype, CacheState state) {
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
            public void Dispose() {
                if (!disposed) {
                    if (SmallIcon != null) {
                        SmallIcon.Dispose();
                        SmallIcon = null;
                    }
                    if (LargeIcon != null) {
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
            ~CacheItem() {
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
        private class CanContinueProcessingEventArgs: EventArgs {
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
            public CanContinueProcessingEventArgs(string extension) {
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
        /// Initializes a new instance of the <see cref="ImageListViewCacheShellInfo"/> class.
        /// </summary>
        /// <param name="owner">The owner control.</param>
        public ImageListViewCacheShellInfo(ImageListView owner) {
            context = null;
            bw = new QueuedBackgroundWorker();
            bw.Threads = 1;
            bw.IsBackground = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;

            checkProcessingCallback = new SendOrPostCallback(CanContinueProcessing);

            mImageListView = owner;
            RetryOnError = false;

            shellCache = new Dictionary<string, CacheItem>();
            processing = new ConcurrentDictionary<string, bool>();

            disposed = false;
        }
        #endregion

        #region Context Callbacks
        /// <summary>
        /// Determines if the item should be processed.
        /// </summary>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>true if the item should be processed; otherwise false.</returns>
        private bool OnCanContinueProcessing(string extension) {
            CanContinueProcessingEventArgs arg = new CanContinueProcessingEventArgs(extension);
            context.Send(checkProcessingCallback, arg);
            return arg.ContinueProcessing;
        }
        /// <summary>
        /// Determines if the item should be processed.
        /// </summary>
        /// <param name="argument">The event argument.</param>
        /// <returns>true if the item should be processed; otherwise false.</returns>
        private void CanContinueProcessing(object argument) {
            CanContinueProcessingEventArgs arg = argument as CanContinueProcessingEventArgs;
            bool canProcess = true;

            // Is it already cached?
            CacheItem existing;
            if (shellCache.TryGetValue(arg.Extension, out existing)) {
                if (existing.SmallIcon != null && existing.LargeIcon != null)
                    canProcess = false;
            }

            arg.ContinueProcessing = canProcess;
        }
        #endregion

        #region QueuedBackgroundWorker Events
        /// <summary>
        /// Handles the RunWorkerCompleted event of the queued background worker.
        /// [IG_CHANGE] Issue #359: thread collision? leftover context? failed to check null
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageGlass.ImageListView.QueuedWorkerCompletedEventArgs"/> 
        /// instance containing the event data.</param>
        void bw_RunWorkerCompleted(object sender, QueuedWorkerCompletedEventArgs e) {
            CacheItem result = e.Result as CacheItem;

            // Add to cache
            if (result != null) {
                // We are done processing
                bool removedValue;
                var removed = processing.TryRemove(result.Extension, out removedValue);
                if (!removed) {
                    throw new InvalidOperationException("processing.TryRemove failed");
                }

                CacheItem existing = null;
                if (shellCache.TryGetValue(result.Extension, out existing)) {
                    existing.Dispose();
                    shellCache.Remove(result.Extension);
                }
                shellCache.Add(result.Extension, result);
            }

            // Refresh the control lazily
            if (result != null && mImageListView != null)
                mImageListView.Refresh(false, true);
        }
        /// <summary>
        /// Handles the DoWork event of the queued background worker.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageGlass.ImageListView.QueuedWorkerDoWorkEventArgs"/> instance 
        /// containing the event data.</param>
        void bw_DoWork(object sender, QueuedWorkerDoWorkEventArgs e) {
            string extension = e.Argument as string;

            // Should we continue processing this item?
            // The callback checks if the item is already cached.
            if (!OnCanContinueProcessing(extension)) {
                e.Cancel = true;
                return;
            }

            // Read shell info
            ShellInfoExtractor info = ShellInfoExtractor.FromFile(extension);

            // Return the info
            CacheItem result = null;
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
        public void Pause() {
            bw.Pause();
        }
        /// <summary>
        /// Resumes the cache threads. 
        /// </summary>
        public void Resume() {
            bw.Resume();
        }
        /// <summary>
        /// Gets the cache state of the specified item.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public CacheState GetCacheState(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item))
                return item.State;

            return CacheState.Unknown;
        }
        /// <summary>
        /// Rebuilds the cache.
        /// Old items will be kept until they are overwritten
        /// by new ones.
        /// </summary>
        public void Rebuild() {
            foreach (CacheItem item in shellCache.Values)
                item.State = CacheState.Unknown;
        }
        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear() {
            foreach (CacheItem item in shellCache.Values)
                item.Dispose();
            shellCache.Clear();
            processing.Clear();
        }
        /// <summary>
        /// Removes the given item from the cache.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public void Remove(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item)) {
                item.Dispose();
                shellCache.Remove(extension);
            }
        }
        /// <summary>
        /// Adds the item to the cache queue.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public void Add(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            // Already cached?
            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item))
                return;

            // Add to cache queue
            RunWorker(extension);
        }
        /// <summary>
        /// Gets the small shell icon for the given file extension from the cache.
        /// If the item is not cached, null will be returned.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public Image GetSmallIcon(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item)) {
                return item.SmallIcon;
            }
            return null;
        }
        /// <summary>
        /// Gets the large shell icon for the given file extension from the cache.
        /// If the item is not cached, null will be returned.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public Image GetLargeIcon(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item)) {
                return item.LargeIcon;
            }
            return null;
        }
        /// <summary>
        /// Gets the shell file type for the given file extension from the cache.
        /// If the item is not cached, null will be returned.
        /// </summary>
        /// <param name="extension">File extension.</param>
        public string GetFileType(string extension) {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException("extension cannot be null", "extension");

            CacheItem item = null;
            if (shellCache.TryGetValue(extension, out item)) {
                return item.FileType;
            }
            return null;
        }
        #endregion

        #region RunWorker
        /// <summary>
        /// Pushes the given item to the worker queue.
        /// </summary>
        /// <param name="extension">File extension.</param>
        private void RunWorker(string extension) {
            // Get the current synchronization context
            if (context == null)
                context = SynchronizationContext.Current;

            // Already being processed?
            bool added = processing.TryAdd(extension, false);
            if (!added)
                return;

            // Add the item to the queue for processing
            bw.RunWorkerAsync(extension);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if (!disposed) {
                bw.DoWork -= bw_DoWork;
                bw.RunWorkerCompleted -= bw_RunWorkerCompleted;

                Clear();
                bw.Dispose();

                disposed = true;

                GC.SuppressFinalize(this);
            }
        }
#if DEBUG
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// ImageListViewCacheManager is reclaimed by garbage collection.
        /// </summary>
        ~ImageListViewCacheShellInfo() {
            System.Diagnostics.Debug.Print("Finalizer of {0} called.", GetType());
            Dispose();
        }
#endif
        #endregion
    }
}

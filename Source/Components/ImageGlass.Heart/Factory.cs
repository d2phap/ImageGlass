/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2021 DUONG DIEU PHAP
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
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Heart {
    public sealed class Factory: IDisposable {
        #region PRIVATE PROPERTIES

        /// <summary>
        /// The list of Imgs
        /// </summary>
        private List<Img> ImgList { get; } = new List<Img>();

        /// <summary>
        /// The list of image index that waiting for loading
        /// </summary>
        private List<int> QueuedList { get; } = new List<int>();

        /// <summary>
        /// The list of image index that waiting for releasing resource
        /// </summary>
        private List<int> FreeList { get; } = new List<int>();

        private bool IsRunWorker { get; set; } = false;

        #endregion

        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets length of Img list
        /// </summary>
        public int Length => ImgList.Count;

        /// <summary>
        /// Gets, sets image size
        /// </summary>
        public Size ImgSize { get; set; } = new Size();

        /// <summary>
        /// Gets sets ColorProfile name of path
        /// </summary>
        public string ColorProfileName { get; set; } = "sRGB";

        /// <summary>
        /// Gets, sets the value indicates if the ColorProfileName should apply to all image files
        /// </summary>
        public bool IsApplyColorProfileForAll { get; set; } = false;

        /// <summary>
        /// Gets, sets the value of ImageMagick.Channels to apply to the entire image list
        /// </summary>
        public int Channels { get; set; } = -1;

        /// <summary>
        /// Get filenames list
        /// </summary>
        public List<string> FileNames {
            get {
                var list = new List<string>();
                foreach (var item in ImgList) {
                    list.Add(item.Filename);
                }

                return list;
            }
        }

        /// <summary>
        /// Gets, sets the number of maximum items in queue list for 1 direction (Next or Back navigation).
        /// The maximum number of items in queue list is 2x + 1.
        /// </summary>
        public uint MaxQueue { get; set; } = 1;

        /// <summary>
        /// Gets, sests the value indicates that returns the embedded thumbnail if found.
        /// </summary>
        public bool UseEmbeddedThumbnail { get; set; } = false;

        public delegate void FinishLoadingImageHandler(object sender, EventArgs e);
        public event EventHandler<EventArgs> OnFinishLoadingImage;

        #endregion

        public Factory() { }

        /// <summary>
        /// The ImageBooster Factory
        /// </summary>
        /// <param name="filenames">List of filenames</param>
        public Factory(IList<string> filenames) {
            // import filenames to the list
            foreach (var filename in filenames) {
                ImgList.Add(new Img(filename));
            }

            // start background service worker
            IsRunWorker = true;
            var _bw = new BackgroundWorker();
            _bw.RunWorkerAsync(StartImageBoosterAsync());
        }

        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Add index of the image to queue list
        /// </summary>
        /// <param name="index">Current index of image list</param>
        private void UpdateQueueList(int index) {
            // check valid index
            if (index < 0 || index >= ImgList.Count) return;

            var list = new HashSet<int>
            {
                index
            };

            var maxCachedItems = (MaxQueue * 2) + 1;
            var iRight = index;
            var iLeft = index;

            // add index in the range in order: index -> right -> left -> ...
            for (var i = 0; list.Count < maxCachedItems && list.Count < ImgList.Count; i++) {
                // if i is even number
                if ((i & 1) == 0) {
                    // add right item: [index + 1; ...; to]
                    iRight++;

                    if (iRight < ImgList.Count) {
                        list.Add(iRight);
                    }
                    else {
                        list.Add(iRight - ImgList.Count);
                    }
                }
                // if i is odd number
                else {
                    // add left item: [index - 1; ...; from]
                    iLeft--;

                    if (iLeft >= 0) {
                        list.Add(iLeft);
                    }
                    else {
                        list.Add(ImgList.Count + iLeft);
                    }
                }
            }

            // release the resources
            foreach (var indexItem in FreeList) {
                if (!list.Contains(indexItem) && indexItem >= 0 && indexItem < ImgList.Count) {
                    ImgList[indexItem].Dispose();
                }
            }

            // update new index of free list
            FreeList.Clear();
            FreeList.AddRange(list);

            // update queue list
            QueuedList.Clear();
            QueuedList.AddRange(list);
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Start ImageBooster thread
        /// </summary>
        public async Task StartImageBoosterAsync() {
            while (IsRunWorker) {
                if (QueuedList.Count > 0) {
                    // pop out the first item
                    var index = QueuedList[0];
                    var img = ImgList[index];
                    QueuedList.RemoveAt(0);

                    if (!img.IsDone) {
                        // start loading image file
                        await img.LoadAsync(
                            size: ImgSize,
                            colorProfileName: ColorProfileName,
                            isApplyColorProfileForAll: IsApplyColorProfileForAll,
                            channel: Channels,
                            useEmbeddedThumbnail: UseEmbeddedThumbnail
                        ).ConfigureAwait(true);
                    }
                }

                await Task.Delay(10).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Releases all resources of the Factory and Stop ImageBooster thread
        /// </summary>
        public void Dispose() {
            // stop the worker
            IsRunWorker = false;

            // clear list and release resources
            Clear();
        }

        /// <summary>
        /// Add a filename to the list
        /// </summary>
        /// <param name="filename">Image filename</param>
        public void Add(string filename) {
            ImgList.Add(new Img(filename));
        }

        /// <summary>
        /// Get Img data
        /// </summary>
        /// <param name="index">image index</param>
        /// <param name="isSkipCache"></param>
        /// <param name="pageIndex">The index of image page to display (if it's multi-page)</param>
        /// <returns></returns>
        public async Task<Img> GetImgAsync(int index, bool isSkipCache = false, int pageIndex = 0) {
            // reload fresh new image data
            if (isSkipCache) {
                await ImgList[index].LoadAsync(
                    size: ImgSize,
                    colorProfileName: ColorProfileName,
                    isApplyColorProfileForAll: IsApplyColorProfileForAll,
                    channel: Channels,
                    useEmbeddedThumbnail: UseEmbeddedThumbnail
                ).ConfigureAwait(true);
            }
            // get image data from cache
            else {
                // update queue list according to index
                UpdateQueueList(index);
            }

            // wait until the image loading is done
            if (ImgList.Count > 0) {
                while (!ImgList[index].IsDone) {
                    await Task.Delay(1).ConfigureAwait(true);
                }
            }

            // Trigger event OnFinishLoadingImage
            OnFinishLoadingImage?.Invoke(this, EventArgs.Empty);

            // if there is no error
            if (ImgList.Count > 0) {
                if (ImgList[index].Error == null) {
                    ImgList[index].SetActivePage(pageIndex);
                }

                return ImgList[index];
            }

            return null;
        }

        /// <summary>
        /// Get filename with the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Returns filename or empty string</returns>
        public string GetFileName(int index) {
            try {
                if (ImgList.Count > 0 && ImgList[index] != null) {
                    return ImgList[index].Filename;
                }
            }
            catch (ArgumentOutOfRangeException) // force reload of empty list
            {
                return string.Empty;
            }

            return string.Empty;
        }

        /// <summary>
        /// Set filename
        /// </summary>
        /// <param name="index"></param>
        /// <param name="filename">Image filename</param>
        public void SetFileName(int index, string filename) {
            if (ImgList[index] != null) {
                ImgList[index].Filename = filename;
            }
        }

        /// <summary>
        /// Find index with the given filename
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns></returns>
        public int IndexOf(string filename) {
            // case sensitivity, esp. if filename passed on command line
            return ImgList.FindIndex(item => string.Equals(item.Filename, filename, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Unload and release resources of item with the given index
        /// </summary>
        /// <param name="index"></param>
        public void Unload(int index) {
            ImgList[index]?.Dispose();
        }

        /// <summary>
        /// Remove an item in the list with the given index
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index) {
            Unload(index);
            ImgList.RemoveAt(index);
        }

        /// <summary>
        /// Empty and release resource of the list
        /// </summary>
        public void Clear() {
            // release the resources of the img list
            ClearCache();
            ImgList.Clear();
        }

        /// <summary>
        /// Clear all cached images and release resource of the list
        /// </summary>
        public void ClearCache() {
            // release the resources of the img list
            foreach (var item in ImgList) {
                item.Dispose();
            }

            QueuedList.Clear();
        }

        /// <summary>
        /// Update cached images
        /// </summary>
        public void UpdateCache() {
            // clear current queue list
            QueuedList.Clear();

            var cachedIndexList = ImgList
                .Select((item, index) => new { ImgItem = item, Index = index })
                .Where(item => item.ImgItem.IsDone)
                .Select(item => item.Index)
                .ToList();

            // release the cachced images
            foreach (var index in cachedIndexList) {
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
        public bool ContainsDirPathOf(string filename) {
            var target = Path.GetDirectoryName(filename).ToUpperInvariant();

            var index = ImgList.FindIndex(item => Path.GetDirectoryName(item.Filename).ToUpperInvariant() == target);

            return index != -1;
        }

        #endregion

    }
}

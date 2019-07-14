/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2019 DUONG DIEU PHAP
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Heart
{
    public class Factory : IDisposable
    {

        #region PRIVATE PROPERTIES

        /// <summary>
        /// The list of Imgs
        /// </summary>
        private List<Img> ImgList { get; set; } = new List<Img>();


        /// <summary>
        /// The list of image index that waiting for loading
        /// </summary>
        private List<int> QueuedList { get; set; } = new List<int>();


        /// <summary>
        /// The list of image index that waiting for releasing resource
        /// </summary>
        private List<int> FreeList { get; set; } = new List<int>();

        private bool IsRunWorker { get; set; } = false;

        #endregion



        #region PUBLIC PROPERTIES

        /// <summary>
        /// Gets length of Img list
        /// </summary>
        public int Length => this.ImgList.Count;


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
        public List<string> FileNames
        {
            get
            {
                var list = new List<string>();
                foreach (var item in this.ImgList)
                {
                    list.Add(item.Filename);
                }

                return list;
            }
        }


        /// <summary>
        /// Gets, sets the number of maximum items in queue list for 1 direction (Next or Back navigation).
        /// The maximum number of items in queue list is 2x + 1.
        /// </summary>
        public int MaxQueue { get; set; } = 1;


        public delegate void FinishLoadingImageHandler(object sender, EventArgs e);
        public event FinishLoadingImageHandler OnFinishLoadingImage;

        #endregion



        public Factory() { }


        /// <summary>
        /// The ImageBooster Factory
        /// </summary>
        /// <param name="filenames">List of filenames</param>
        public Factory(IList<string> filenames)
        {
            // import filenames to the list
            foreach (var filename in filenames)
            {
                this.ImgList.Add(new Img(filename));
            }


            // start background service worker
            this.IsRunWorker = true;
            StartImageBooster();
        }



        #region PRIVATE FUNCTIONS

        /// <summary>
        /// Add index of the image to queue list
        /// </summary>
        /// <param name="index">Current index of image list</param>
        private void UpdateQueueList(int index)
        {
            // check valid index
            if (index < 0 || index >= this.ImgList.Count) return;

            var list = new HashSet<int>
            {
                index
            };


            var maxCachedItems = this.MaxQueue * 2 + 1;
            var iRight = index;
            var iLeft = index;
            

            // add index in the range in order: index -> right -> left -> ...
            for (int i = 0; list.Count < maxCachedItems && list.Count < this.ImgList.Count; i++)
            {
                // if i is even number
                if ((i & 1) == 0)
                {
                    // add right item: [index + 1; ...; to]
                    iRight += 1;

                    if (iRight < this.ImgList.Count)
                    {
                        list.Add(iRight);
                    }
                    else
                    {
                        list.Add(iRight - this.ImgList.Count);
                    }
                }
                // if i is odd number
                else
                {
                    // add left item: [index - 1; ...; from]
                    iLeft -= 1;

                    if (iLeft >= 0)
                    {
                        list.Add(iLeft);
                    }
                    else
                    {
                        list.Add(this.ImgList.Count + iLeft);
                    }
                }
            }


            // release the resources
            foreach (var indexItem in this.FreeList)
            {
                if (!list.Contains(indexItem) && indexItem >= 0 && indexItem < this.ImgList.Count)
                {
                    this.ImgList[indexItem].Dispose();
                }
            }

            // update new index of free list
            this.FreeList.Clear();
            this.FreeList.AddRange(list);

            // update queue list
            this.QueuedList.Clear();
            this.QueuedList.AddRange(list);
        }

        #endregion



        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Start ImageBooster thread
        /// </summary>
        public async void StartImageBooster()
        {
            while (this.IsRunWorker)
            {
                if (this.QueuedList.Count > 0)
                {
                    // pop out the first item
                    var index = this.QueuedList[0];
                    var img = this.ImgList[index];
                    QueuedList.RemoveAt(0);


                    if (!img.IsDone)
                    {
                        // start loading image file
                        _ = img.LoadAsync(
                            size: this.ImgSize,
                            colorProfileName: this.ColorProfileName,
                            isApplyColorProfileForAll: this.IsApplyColorProfileForAll,
                            channel: this.Channels
                        );
                    }
                }

                await Task.Delay(10);
            }
        }


        /// <summary>
        /// Releases all resources of the Factory and Stop ImageBooster thread
        /// </summary>
        public void Dispose()
        {
            // stop the worker
            this.IsRunWorker = false;

            // clear list and release resources
            this.Clear();
        }


        /// <summary>
        /// Add a filename to the list
        /// </summary>
        /// <param name="filename">Image filename</param>
        public void Add(string filename)
        {
            this.ImgList.Add(new Img(filename));
        }


        /// <summary>
        /// Get Img data
        /// </summary>
        /// <param name="index">image index</param>
        /// <param name="isSkipCache"></param>
        /// <returns></returns>
        public async Task<Img> GetImgAsync(int index, bool isSkipCache = false)
        {
            // reload fresh new image data
            if (isSkipCache)
            {
                await this.ImgList[index].LoadAsync(
                    size: this.ImgSize,
                    colorProfileName: this.ColorProfileName,
                    isApplyColorProfileForAll: this.IsApplyColorProfileForAll,
                    channel: this.Channels
                );
            }
            // get image data from cache
            else
            {
                // update queue list according to index
                UpdateQueueList(index);
            }


            // wait until the image loading is done
            while (!this.ImgList[index].IsDone)
            {
                await Task.Delay(1);
            }

            // Trigger event OnFinishLoadingImage
            OnFinishLoadingImage?.Invoke(this, new EventArgs());

            // if there is no error
            if (this.ImgList[index].Error == null)
            {
                return this.ImgList[index];
            }

            return null;
        }


        /// <summary>
        /// Get filename with the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Returns filename or empty string</returns>
        public string GetFileName(int index)
        {
            try
            {
                if (this.ImgList[index] != null)
                {
                    return this.ImgList[index].Filename;
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
        public void SetFileName(int index, string filename)
        {
            if (this.ImgList[index] != null)
            {
                this.ImgList[index].Filename = filename;
            }
        }


        /// <summary>
        /// Find index with the given filename
        /// </summary>
        /// <param name="filename">Image filename</param>
        /// <returns></returns>
        public int IndexOf(string filename)
        {
            // case sensitivity, esp. if filename passed on command line
            return this.ImgList.FindIndex(item => item.Filename.ToUpperInvariant() == filename.ToUpperInvariant());
        }


        /// <summary>
        /// Unload and release resources of item with the given index
        /// </summary>
        /// <param name="index"></param>
        public void Unload(int index)
        {
            if (this.ImgList[index] != null)
            {
                this.ImgList[index].Dispose();
            }
        }


        /// <summary>
        /// Remove an item in the list with the given index
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            this.Unload(index);
            this.ImgList.RemoveAt(index);
        }


        /// <summary>
        /// Empty and release resource of the list
        /// </summary>
        public void Clear()
        {
            // release the resources of the img list
            this.ClearCache();
            this.ImgList.Clear();
        }


        /// <summary>
        /// Clear all cached images and release resource of the list
        /// </summary>
        public void ClearCache()
        {
            // release the resources of the img list
            foreach (var item in this.ImgList)
            {
                item.Dispose();
            }

            this.QueuedList.Clear();
        }


        /// <summary>
        /// Update cached images
        /// </summary>
        public void UpdateCache()
        {
            // clear current queue list
            this.QueuedList.Clear();

            var cachedIndexList = this.ImgList
                .Select((item, index) => new { ImgItem = item, Index = index })
                .Where(item => item.ImgItem.IsDone)
                .Select(item => item.Index)
                .ToList();

            // release the cachced images
            foreach (var index in cachedIndexList)
            {
                this.ImgList[index].Dispose();
            }

            // add to queue list
            this.QueuedList.AddRange(cachedIndexList);
        }


        /// <summary>
        /// Check if the folder path of input filename exists in the list
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool ContainsDirPathOf(string filename)
        {
            var target = Path.GetDirectoryName(filename).ToUpperInvariant();

            var index = this.ImgList.FindIndex(item => Path.GetDirectoryName(item.Filename).ToUpperInvariant() == target);

            return index != -1;
        }

        #endregion



    }
}

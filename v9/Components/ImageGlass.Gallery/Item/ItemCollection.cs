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
using System.Collections;
using System.ComponentModel;

namespace ImageGlass.Gallery;


public partial class ImageGallery
{
    /// <summary>
    /// Represents the collection of items in the image list view.
    /// </summary>
    public class ItemCollection : IList<ImageGalleryItem>, ICollection, IList, IEnumerable
    {

        #region Member Variables
        private readonly List<ImageGalleryItem> mItems = new();
        private ImageGalleryItem? mFocused = null;
        private Dictionary<Guid, ImageGalleryItem> lookUp = new();

        internal bool collectionModified = true;
        internal ImageGallery _imageGallery;
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCollection"/>  class.
        /// </summary>
        /// <param name="owner">The <see cref="Gallery.ImageGallery"/> owning this collection.</param>
        internal ItemCollection(ImageGallery owner)
        {
            _imageGallery = owner;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the number of elements contained in
        /// the <see cref="ItemCollection"/>.
        /// </summary>
        public int Count => mItems.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ItemCollection"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the focused item.
        /// </summary>
        public ImageGalleryItem? FocusedItem
        {
            get => mFocused;
            set
            {
                var oldFocusedItem = mFocused;
                mFocused = value;

                // Refresh items
                if (oldFocusedItem != mFocused && _imageGallery != null)
                    _imageGallery.Refresh();
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageGallery"/> owning this collection.
        /// </summary>
        [Browsable(false)]
        public ImageGallery ImageGalleryOwner => _imageGallery;

        /// <summary>
        /// Gets or sets the <see cref="ImageGalleryItem"/> at the specified index.
        /// </summary>
        [Browsable(false)]
        public ImageGalleryItem this[int index]
        {
            get => mItems[index];
            set
            {
                var item = value;
                var oldItem = mItems[index];

                if (mItems[index] == mFocused)
                {
                    mFocused = item;
                }

                var oldSelected = mItems[index].Selected;
                item.mIndex = index;

                if (_imageGallery != null)
                {
                    item.ImageGalleryOwner = _imageGallery;
                }

                item.owner = this;
                mItems[index] = item;
                lookUp.Remove(oldItem.Guid);
                lookUp.Add(item.Guid, item);
                collectionModified = true;

                if (_imageGallery != null)
                {
                    _imageGallery.thumbnailCache.Remove(oldItem.Guid);
                    _imageGallery.metadataCache.Remove(oldItem.Guid);

                    if (_imageGallery.CacheMode == CacheMode.Continuous)
                    {
                        _imageGallery.thumbnailCache.Add(
                            item.Guid,
                            item.Adaptor,
                            item.VirtualItemKey,
                            _imageGallery.ThumbnailSize,
                            _imageGallery.UseEmbeddedThumbnails,
                            _imageGallery.AutoRotateThumbnails);
                    }

                    _imageGallery.metadataCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey);

                    if (item.Selected != oldSelected)
                    {
                        _imageGallery.OnSelectionChanged(new EventArgs());
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageGalleryItem"/> with the specified Guid.
        /// </summary>
        [Browsable(false)]
        internal ImageGalleryItem this[Guid guid] => lookUp[guid];

        #endregion


        #region Instance Methods

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add to the <see cref="ItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(ImageGalleryItem item, IAdaptor adaptor)
        {
            AddInternal(item, adaptor);

            if (_imageGallery != null)
            {
                if (item.Selected)
                    _imageGallery.OnSelectionChangedInternal();
                _imageGallery.Refresh();
            }
        }

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add to the <see cref="ItemCollection"/>.</param>
        public void Add(ImageGalleryItem item)
        {
            Add(item, _imageGallery.defaultAdaptor);
        }

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add to the <see cref="ItemCollection"/>.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(ImageGalleryItem item, Image initialThumbnail, IAdaptor adaptor)
        {
            item.clonedThumbnail = initialThumbnail;
            Add(item, adaptor);
        }

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add to the <see cref="ItemCollection"/>.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Add(ImageGalleryItem item, Image initialThumbnail)
        {
            Add(item, initialThumbnail, _imageGallery.defaultAdaptor);
        }

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        public void Add(string filename)
        {
            Add(filename, null);
        }

        /// <summary>
        /// Adds an item to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Add(string filename, Image? initialThumbnail)
        {
            var item = new ImageGalleryItem(filename)
            {
                mAdaptor = _imageGallery.defaultAdaptor,
                clonedThumbnail = initialThumbnail,
            };

            Add(item);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="items">An array of <see cref="ImageGalleryItem"/> 
        /// to add to the <see cref="ItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void AddRange(ImageGalleryItem[] items, IAdaptor adaptor)
        {
            if (_imageGallery != null)
            {
                _imageGallery.SuspendPaint();
            }

            foreach (ImageGalleryItem item in items)
            {
                Add(item, adaptor);
            }

            if (_imageGallery != null)
            {
                _imageGallery.Refresh();
                _imageGallery.ResumePaint();
            }
        }

        /// <summary>
        /// Adds a range of items to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="items">An array of <see cref="ImageGalleryItem"/> 
        /// to add to the <see cref="ItemCollection"/>.</param>
        public void AddRange(ImageGalleryItem[] items)
        {
            AddRange(items, _imageGallery.defaultAdaptor);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="filenames">The names or the image files.</param>
        public void AddRange(string[] filenames)
        {
            var items = new ImageGalleryItem[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                items[i] = new ImageGalleryItem(filenames[i]);
            }

            AddRange(items);
        }

        /// <summary>
        /// Removes all items from the <see cref="ItemCollection"/>.
        /// </summary>
        public void Clear()
        {
            mItems.Clear();
            mFocused = null;
            lookUp = new();
            collectionModified = true;

            if (_imageGallery != null)
            {
                _imageGallery.metadataCache.Clear();
                _imageGallery.thumbnailCache.Clear();
                _imageGallery.SelectedItems.Clear();

                _imageGallery.Refresh();

                // Raise the clear event
                _imageGallery.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Refresh, null));
            }
        }

        /// <summary>
        /// Determines whether the <see cref="ItemCollection"/> 
        /// contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the 
        /// <see cref="ItemCollection"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the 
        /// <see cref="ItemCollection"/>; otherwise, false.
        /// </returns>
        public bool Contains(ImageGalleryItem item)
        {
            return mItems.Contains(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> 
        /// that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ImageGalleryItem> GetEnumerator()
        {
            return mItems.GetEnumerator();
        }

        /// <summary>
        /// Inserts an item to the <see cref="ItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to 
        /// insert into the <see cref="ItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Insert(int index, ImageGalleryItem item, IAdaptor adaptor)
        {
            InsertInternal(index, item, adaptor);

            if (_imageGallery != null)
            {
                if (item.Selected)
                    _imageGallery.OnSelectionChangedInternal();
                _imageGallery.Refresh();
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="ItemCollection"/>
        /// at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which
        /// <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to 
        /// insert into the <see cref="ItemCollection"/>.</param>
        public void Insert(int index, ImageGalleryItem item)
        {
            Insert(index, item, _imageGallery.defaultAdaptor);
        }

        /// <summary>
        /// Inserts an item to the <see cref="ItemCollection"/>
        /// at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item
        /// should be inserted.</param>
        /// <param name="filename">The name of the image file.</param>
        public void Insert(int index, string filename)
        {
            Insert(index, new ImageGalleryItem(filename));
        }

        /// <summary>
        /// Inserts an item to the <see cref="ItemCollection"/> at
        /// the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should
        /// be inserted.</param>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Insert(int index, string filename, Image initialThumbnail)
        {
            var item = new ImageGalleryItem(filename)
            {
                clonedThumbnail = initialThumbnail
            };

            Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object 
        /// from the <see cref="ItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to remove 
        /// from the <see cref="ItemCollection"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the 
        /// <see cref="ItemCollection"/>; otherwise, false. This method also 
        /// returns false if <paramref name="item"/> is not found in the original 
        /// <see cref="ItemCollection"/>.
        /// </returns>
        public bool Remove(ImageGalleryItem item)
        {
            bool ret = RemoveInternal(item, true);

            if (_imageGallery != null)
            {
                if (item.Selected)
                {
                    _imageGallery.OnSelectionChangedInternal();
                }

                _imageGallery.Refresh();
            }

            return ret;
        }

        /// <summary>
        /// Removes the <see cref="ImageGalleryItem"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            Remove(mItems[index]);
        }

        #endregion


        #region Helper Methods
        /// <summary>
        /// Determines whether the collection contains the given key.
        /// </summary>
        /// <param name="guid">The key of the item.</param>
        /// <returns>true if the collection contains the given key; otherwise false.</returns>
        internal bool ContainsKey(Guid guid)
        {
            return lookUp.ContainsKey(guid);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="guid">The key of the item.</param>
        /// <param name="item">the value associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type 
        /// of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the collection contains the given key; otherwise false.</returns>
        internal bool TryGetValue(Guid guid, out ImageGalleryItem? item)
        {
            return lookUp.TryGetValue(guid, out item);
        }

        /// <summary>
        /// Adds the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        /// <returns>true if the item was added; otherwise false.</returns>
        internal bool AddInternal(ImageGalleryItem item, IAdaptor adaptor)
        {
            return InsertInternal(-1, item, adaptor);
        }

        /// <summary>
        /// Inserts the given item without raising a selection changed event.
        /// </summary>
        /// <param name="index">Insertion index. If index is -1 the item is added to the end of the list.</param>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to add.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        /// <returns>true if the item was added; otherwise false.</returns>
        internal bool InsertInternal(int index, ImageGalleryItem item, IAdaptor adaptor)
        {
            if (_imageGallery == null)
                return false;

            item.owner = this;
            item.mAdaptor = adaptor;

            if (index == -1)
            {
                item.mIndex = mItems.Count;
                mItems.Add(item);
            }
            else
            {
                item.mIndex = index;
                for (int i = index; i < mItems.Count; i++)
                    mItems[i].mIndex++;
                mItems.Insert(index, item);
            }

            lookUp.Add(item.Guid, item);
            collectionModified = true;

            item.ImageGalleryOwner = _imageGallery;

            // Add current thumbnail to cache
            if (item.clonedThumbnail != null)
            {
                _imageGallery.thumbnailCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey, _imageGallery.ThumbnailSize,
                    item.clonedThumbnail, _imageGallery.UseEmbeddedThumbnails, _imageGallery.AutoRotateThumbnails);
                item.clonedThumbnail = null;
            }

            // Add to thumbnail cache
            if (_imageGallery.CacheMode == CacheMode.Continuous)
            {
                _imageGallery.thumbnailCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey,
                    _imageGallery.ThumbnailSize, _imageGallery.UseEmbeddedThumbnails, _imageGallery.AutoRotateThumbnails);
            }

            // Add to details cache
            _imageGallery.metadataCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey);

            // Add to shell info cache
            var extension = item.extension;
            if (!string.IsNullOrEmpty(extension))
            {
                var state = _imageGallery.shellInfoCache.GetCacheState(extension);
                if (state == CacheState.Error && _imageGallery.RetryOnError == true)
                {
                    _imageGallery.shellInfoCache.Remove(extension);
                    _imageGallery.shellInfoCache.Add(extension);
                }
                else if (state == CacheState.Unknown)
                    _imageGallery.shellInfoCache.Add(extension);
            }

            // Raise the add event
            _imageGallery.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Add, item));

            return true;
        }

        /// <summary>
        /// Removes the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        internal void RemoveInternal(ImageGalleryItem item)
        {
            RemoveInternal(item, true);
        }

        /// <summary>
        /// Removes the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="removeFromCache">true to remove item image from cache; otherwise false.</param>
        internal bool RemoveInternal(ImageGalleryItem item, bool removeFromCache)
        {
            for (int i = item.mIndex + 1; i < mItems.Count; i++)
                mItems[i].mIndex--;
            if (item == mFocused) mFocused = null;
            if (removeFromCache && _imageGallery != null)
            {
                _imageGallery.thumbnailCache.Remove(item.Guid);
                _imageGallery.metadataCache.Remove(item.Guid);
            }

            var ret = mItems.Remove(item);
            lookUp.Remove(item.Guid);
            collectionModified = true;

            if (_imageGallery != null)
            {
                // Raise the remove event
                _imageGallery.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Remove, item));
            }

            return ret;
        }

        /// <summary>
        /// Returns the index of the specified item.
        /// </summary>
        internal static int IndexOf(ImageGalleryItem item)
        {
            return item.Index;
        }

        /// <summary>
        /// Returns the index of the item with the specified Guid.
        /// </summary>
        internal int IndexOf(Guid guid)
        {
            if (lookUp.TryGetValue(guid, out ImageGalleryItem? item))
                return item.Index;
            return -1;
        }

        #endregion


        #region Unsupported Interface
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        void ICollection<ImageGalleryItem>.CopyTo(ImageGalleryItem[] array, int arrayIndex)
        {
            mItems.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        [Obsolete("Use ImageGalleryItem.Index property instead.")]
        int IList<ImageGalleryItem>.IndexOf(ImageGalleryItem item)
        {
            return mItems.IndexOf(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is not ImageGalleryItem[])
                throw new ArgumentException("An array of ImageGalleryItem is required.", nameof(array));
            mItems.CopyTo((ImageGalleryItem[])array, index);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        int ICollection.Count
        {
            get { return mItems.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        object ICollection.SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        int IList.Add(object? value)
        {
            if (value is not ImageGalleryItem)
                throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));

            var item = (ImageGalleryItem)value;
            Add(item);

            return mItems.IndexOf(item);
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        bool IList.Contains(object? value)
        {
            if (value is not ImageGalleryItem)
                throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));
            return mItems.Contains((ImageGalleryItem)value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return mItems.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        int IList.IndexOf(object? value)
        {
            if (value is not ImageGalleryItem)
                throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));
            return IndexOf((ImageGalleryItem)value);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        void IList.Insert(int index, object? value)
        {
            if (value is not ImageGalleryItem)
                throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));
            Insert(index, (ImageGalleryItem)value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        bool IList.IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        void IList.Remove(object? value)
        {
            if (value is not ImageGalleryItem)
                throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));

            var item = (ImageGalleryItem)value;
            Remove(item);
        }

        /// <summary>
        /// Gets or sets the <see cref="object"/> at the specified index.
        /// </summary>
        object? IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value is not ImageGalleryItem)
                    throw new ArgumentException($"An object of type {nameof(ImageGalleryItem)} is required.", nameof(value));
                this[index] = (ImageGalleryItem)value;
            }
        }
        #endregion
    
    }
}

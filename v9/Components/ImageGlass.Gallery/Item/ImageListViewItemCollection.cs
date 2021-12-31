
using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ImageGlass.Gallery;


public partial class ImageListView
{
    /// <summary>
    /// Represents the collection of items in the image list view.
    /// </summary>
    public class ImageListViewItemCollection : IList<ImageListViewItem>, ICollection, IList, IEnumerable
    {
        #region Member Variables
        private List<ImageListViewItem> mItems;
        internal ImageListView mImageListView;
        private ImageListViewItem? mFocused;
        private Dictionary<Guid, ImageListViewItem> lookUp;
        internal bool collectionModified;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItemCollection"/>  class.
        /// </summary>
        /// <param name="owner">The <see cref="ImageListView"/> owning this collection.</param>
        internal ImageListViewItemCollection(ImageListView owner)
        {
            mItems = new List<ImageListViewItem>();
            lookUp = new Dictionary<Guid, ImageListViewItem>();
            mFocused = null;
            mImageListView = owner;
            collectionModified = true;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        public int Count => mItems.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ImageListViewItemCollection"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the focused item.
        /// </summary>
        public ImageListViewItem? FocusedItem
        {
            get
            {
                return mFocused;
            }
            set
            {
                var oldFocusedItem = mFocused;
                mFocused = value;

                // Refresh items
                if (oldFocusedItem != mFocused && mImageListView != null)
                    mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Gets the <see cref="ImageListView"/> owning this collection.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the ImageListView owning this collection.")]
        public ImageListView ImageListView { get { return mImageListView; } }
        /// <summary>
        /// Gets or sets the <see cref="ImageListViewItem"/> at the specified index.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets or sets the item at the specified index.")]
        public ImageListViewItem this[int index]
        {
            get
            {
                return mItems[index];
            }
            set
            {
                ImageListViewItem item = value;
                ImageListViewItem oldItem = mItems[index];

                if (mItems[index] == mFocused)
                    mFocused = item;
                bool oldSelected = mItems[index].Selected;
                item.mIndex = index;
                if (mImageListView != null)
                    item.mImageListView = mImageListView;
                item.owner = this;
                mItems[index] = item;
                lookUp.Remove(oldItem.Guid);
                lookUp.Add(item.Guid, item);
                collectionModified = true;

                if (mImageListView != null)
                {
                    mImageListView.thumbnailCache.Remove(oldItem.Guid);
                    mImageListView.metadataCache.Remove(oldItem.Guid);
                    if (mImageListView.CacheMode == CacheMode.Continuous)
                    {
                        mImageListView.thumbnailCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey,
                            mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                            mImageListView.AutoRotateThumbnails);
                    }
                    mImageListView.metadataCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey);
                    if (item.Selected != oldSelected)
                        mImageListView.OnSelectionChanged(new EventArgs());
                }
            }
        }
        /// <summary>
        /// Gets the <see cref="ImageListViewItem"/> with the specified Guid.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets or sets the item with the specified Guid.")]
        internal ImageListViewItem this[Guid guid]
        {
            get
            {
                return lookUp[guid];
            }
        }
        #endregion

        #region Instance Methods
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add to the <see cref="ImageListViewItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(ImageListViewItem item, ImageListViewItemAdaptor adaptor)
        {
            AddInternal(item, adaptor);

            if (mImageListView != null)
            {
                if (item.Selected)
                    mImageListView.OnSelectionChangedInternal();
                mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add to the <see cref="ImageListViewItemCollection"/>.</param>
        public void Add(ImageListViewItem item)
        {
            Add(item, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add to the <see cref="ImageListViewItemCollection"/>.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(ImageListViewItem item, Image initialThumbnail, ImageListViewItemAdaptor adaptor)
        {
            item.clonedThumbnail = initialThumbnail;
            Add(item, adaptor);
        }
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add to the <see cref="ImageListViewItemCollection"/>.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Add(ImageListViewItem item, Image initialThumbnail)
        {
            Add(item, initialThumbnail, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        public void Add(string filename)
        {
            Add(filename, null);
        }
        /// <summary>
        /// Adds an item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Add(string filename, Image? initialThumbnail)
        {
            var item = new ImageListViewItem(filename)
            {
                mAdaptor = mImageListView.defaultAdaptor,
                clonedThumbnail = initialThumbnail
            };

            Add(item);
        }

        /// <summary>
        /// Adds a virtual item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(object key, string text, ImageListViewItemAdaptor adaptor)
        {
            Add(key, text, null, adaptor);
        }

        /// <summary>
        /// Adds a virtual item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        public void Add(object key, string text)
        {
            Add(key, text, mImageListView.defaultAdaptor);
        }

        /// <summary>
        /// Adds a virtual item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Add(object key, string text, Image? initialThumbnail, ImageListViewItemAdaptor adaptor)
        {
            var item = new ImageListViewItem(key, text)
            {
                clonedThumbnail = initialThumbnail,
            };

            Add(item, adaptor);
        }

        /// <summary>
        /// Adds a virtual item to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Add(object key, string text, Image initialThumbnail)
        {
            Add(key, text, initialThumbnail, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Adds a range of items to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="items">An array of <see cref="ImageListViewItem"/> 
        /// to add to the <see cref="ImageListViewItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void AddRange(ImageListViewItem[] items, ImageListViewItemAdaptor adaptor)
        {
            if (mImageListView != null)
                mImageListView.SuspendPaint();

            foreach (ImageListViewItem item in items)
                Add(item, adaptor);

            if (mImageListView != null)
            {
                mImageListView.Refresh();
                mImageListView.ResumePaint();
            }
        }
        /// <summary>
        /// Adds a range of items to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="items">An array of <see cref="ImageListViewItem"/> 
        /// to add to the <see cref="ImageListViewItemCollection"/>.</param>
        public void AddRange(ImageListViewItem[] items)
        {
            AddRange(items, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Adds a range of items to the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="filenames">The names or the image files.</param>
        public void AddRange(string[] filenames)
        {
            ImageListViewItem[] items = new ImageListViewItem[filenames.Length];

            for (int i = 0; i < filenames.Length; i++)
            {
                items[i] = new ImageListViewItem(filenames[i]);
            }

            AddRange(items);
        }
        /// <summary>
        /// Removes all items from the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        public void Clear()
        {
            mItems.Clear();
            mFocused = null;

            // [IG_CHANGE] lookUp.Clear() does not deallocate memory
            lookUp = new();

            collectionModified = true;

            if (mImageListView != null)
            {
                mImageListView.metadataCache.Clear();
                mImageListView.thumbnailCache.Clear();
                mImageListView.SelectedItems.Clear();

                mImageListView.Refresh();

                // Raise the clear event
                mImageListView.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Refresh, null));
            }
        }
        /// <summary>
        /// Determines whether the <see cref="ImageListViewItemCollection"/> 
        /// contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the 
        /// <see cref="ImageListViewItemCollection"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the 
        /// <see cref="ImageListViewItemCollection"/>; otherwise, false.
        /// </returns>
        public bool Contains(ImageListViewItem item)
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
        public IEnumerator<ImageListViewItem> GetEnumerator()
        {
            return mItems.GetEnumerator();
        }
        /// <summary>
        /// Inserts an item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="ImageListViewItem"/> to 
        /// insert into the <see cref="ImageListViewItemCollection"/>.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Insert(int index, ImageListViewItem item, ImageListViewItemAdaptor adaptor)
        {
            InsertInternal(index, item, adaptor);

            if (mImageListView != null)
            {
                if (item.Selected)
                    mImageListView.OnSelectionChangedInternal();
                mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Inserts an item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="ImageListViewItem"/> to 
        /// insert into the <see cref="ImageListViewItemCollection"/>.</param>
        public void Insert(int index, ImageListViewItem item)
        {
            Insert(index, item, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Inserts an item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="filename">The name of the image file.</param>
        public void Insert(int index, string filename)
        {
            Insert(index, new ImageListViewItem(filename));
        }
        /// <summary>
        /// Inserts an item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="filename">The name of the image file.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Insert(int index, string filename, Image initialThumbnail)
        {
            ImageListViewItem item = new ImageListViewItem(filename);
            item.clonedThumbnail = initialThumbnail;
            Insert(index, item);
        }
        /// <summary>
        /// Inserts a virtual item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Insert(int index, object key, string text, ImageListViewItemAdaptor adaptor)
        {
            Insert(index, key, text, null, adaptor);
        }
        /// <summary>
        /// Inserts a virtual item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        public void Insert(int index, object key, string text)
        {
            Insert(index, key, text, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Inserts a virtual item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        public void Insert(int index, object key, string text, Image initialThumbnail, ImageListViewItemAdaptor adaptor)
        {
            ImageListViewItem item = new ImageListViewItem(key, text);
            item.clonedThumbnail = initialThumbnail;
            Insert(index, item, adaptor);
        }
        /// <summary>
        /// Inserts a virtual item to the <see cref="ImageListViewItemCollection"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new item should be inserted.</param>
        /// <param name="key">The key identifying the item.</param>
        /// <param name="text">Text of the item.</param>
        /// <param name="initialThumbnail">The initial thumbnail image for the item.</param>
        public void Insert(int index, object key, string text, Image initialThumbnail)
        {
            Insert(index, key, text, initialThumbnail, mImageListView.defaultAdaptor);
        }
        /// <summary>
        /// Removes the first occurrence of a specific object 
        /// from the <see cref="ImageListViewItemCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to remove 
        /// from the <see cref="ImageListViewItemCollection"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the 
        /// <see cref="ImageListViewItemCollection"/>; otherwise, false. This method also 
        /// returns false if <paramref name="item"/> is not found in the original 
        /// <see cref="ImageListViewItemCollection"/>.
        /// </returns>
        public bool Remove(ImageListViewItem item)
        {
            bool ret = RemoveInternal(item, true);

            if (mImageListView != null)
            {
                if (item.Selected)
                    mImageListView.OnSelectionChangedInternal();
                mImageListView.Refresh();
            }

            return ret;
        }
        /// <summary>
        /// Removes the <see cref="ImageListViewItem"/> at the specified index.
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
        internal bool TryGetValue(Guid guid, out ImageListViewItem item)
        {
            return lookUp.TryGetValue(guid, out item);
        }
        /// <summary>
        /// Adds the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        /// <returns>true if the item was added; otherwise false.</returns>
        internal bool AddInternal(ImageListViewItem item, ImageListViewItemAdaptor adaptor)
        {
            return InsertInternal(-1, item, adaptor);
        }
        /// <summary>
        /// Inserts the given item without raising a selection changed event.
        /// </summary>
        /// <param name="index">Insertion index. If index is -1 the item is added to the end of the list.</param>
        /// <param name="item">The <see cref="ImageListViewItem"/> to add.</param>
        /// <param name="adaptor">The adaptor associated with this item.</param>
        /// <returns>true if the item was added; otherwise false.</returns>
        internal bool InsertInternal(int index, ImageListViewItem item, ImageListViewItemAdaptor adaptor)
        {
            if (mImageListView == null)
                return false;

            // Check if the file already exists
            if (!string.IsNullOrEmpty(item.FileName) && !mImageListView.AllowDuplicateFileNames)
            {
                if (mItems.Exists(a => string.Compare(a.FileName, item.FileName, StringComparison.OrdinalIgnoreCase) == 0))
                    return false;
            }
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

            item.mImageListView = mImageListView;

            // Add current thumbnail to cache
            if (item.clonedThumbnail != null)
            {
                mImageListView.thumbnailCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey, mImageListView.ThumbnailSize,
                    item.clonedThumbnail, mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails);
                item.clonedThumbnail = null;
            }

            // Add to thumbnail cache
            if (mImageListView.CacheMode == CacheMode.Continuous)
            {
                mImageListView.thumbnailCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey,
                    mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails);
            }

            // Add to details cache
            mImageListView.metadataCache.Add(item.Guid, item.Adaptor, item.VirtualItemKey);

            // Add to shell info cache
            string extension = item.extension;
            if (!string.IsNullOrEmpty(extension))
            {
                CacheState state = mImageListView.shellInfoCache.GetCacheState(extension);
                if (state == CacheState.Error && mImageListView.RetryOnError == true)
                {
                    mImageListView.shellInfoCache.Remove(extension);
                    mImageListView.shellInfoCache.Add(extension);
                }
                else if (state == CacheState.Unknown)
                    mImageListView.shellInfoCache.Add(extension);
            }

            // Raise the add event
            mImageListView.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Add, item));

            return true;
        }
        /// <summary>
        /// Removes the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        internal void RemoveInternal(ImageListViewItem item)
        {
            RemoveInternal(item, true);
        }
        /// <summary>
        /// Removes the given item without raising a selection changed event.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="removeFromCache">true to remove item image from cache; otherwise false.</param>
        internal bool RemoveInternal(ImageListViewItem item, bool removeFromCache)
        {
            for (int i = item.mIndex + 1; i < mItems.Count; i++)
                mItems[i].mIndex--;
            if (item == mFocused) mFocused = null;
            if (removeFromCache && mImageListView != null)
            {
                mImageListView.thumbnailCache.Remove(item.Guid);
                mImageListView.metadataCache.Remove(item.Guid);
            }
            bool ret = mItems.Remove(item);
            lookUp.Remove(item.Guid);
            collectionModified = true;

            if (mImageListView != null)
            {
                // Raise the remove event
                mImageListView.OnItemCollectionChanged(new ItemCollectionChangedEventArgs(CollectionChangeAction.Remove, item));
            }

            return ret;
        }
        /// <summary>
        /// Returns the index of the specified item.
        /// </summary>
        internal int IndexOf(ImageListViewItem item)
        {
            return item.Index;
        }
        /// <summary>
        /// Returns the index of the item with the specified Guid.
        /// </summary>
        internal int IndexOf(Guid guid)
        {
            if (lookUp.TryGetValue(guid, out ImageListViewItem? item))
                return item.Index;
            return -1;
        }

        #endregion

        #region Unsupported Interface
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        void ICollection<ImageListViewItem>.CopyTo(ImageListViewItem[] array, int arrayIndex)
        {
            mItems.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        [Obsolete("Use ImageListViewItem.Index property instead.")]
        int IList<ImageListViewItem>.IndexOf(ImageListViewItem item)
        {
            return mItems.IndexOf(item);
        }
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array is not ImageListViewItem[])
                throw new ArgumentException("An array of ImageListViewItem is required.", nameof(array));
            mItems.CopyTo((ImageListViewItem[])array, index);
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
        int IList.Add(object value)
        {
            if (value is not ImageListViewItem)
                throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));

            var item = (ImageListViewItem)value;
            Add(item);

            return mItems.IndexOf(item);
        }
        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        bool IList.Contains(object value)
        {
            if (value is not ImageListViewItem)
                throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));
            return mItems.Contains((ImageListViewItem)value);
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
        int IList.IndexOf(object value)
        {
            if (value is not ImageListViewItem)
                throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));
            return IndexOf((ImageListViewItem)value);
        }
        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        void IList.Insert(int index, object value)
        {
            if (value is not ImageListViewItem)
                throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));
            Insert(index, (ImageListViewItem)value);
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
        void IList.Remove(object value)
        {
            if (value is not ImageListViewItem)
                throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));

            var item = (ImageListViewItem)value;
            Remove(item);
        }
        /// <summary>
        /// Gets or sets the <see cref="object"/> at the specified index.
        /// </summary>
        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                if (value is not ImageListViewItem)
                    throw new ArgumentException("An object of type ImageListViewItem is required.", nameof(value));
                this[index] = (ImageListViewItem)value;
            }
        }
        #endregion
    }

}

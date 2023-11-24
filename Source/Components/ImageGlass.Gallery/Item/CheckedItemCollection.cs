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
    /// Represents the collection of checked items in the image list view.
    /// </summary>
    public class CheckedItemCollection : IList<ImageGalleryItem>
    {
        #region Member Variables
        internal ImageGallery _imageGallery;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckedItemCollection"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="ImageGallery"/> owning this collection.</param>
        internal CheckedItemCollection(ImageGallery owner)
        {
            _imageGallery = owner;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="CheckedItemCollection"/>.
        /// </summary>
        [Category("Behavior"), Browsable(true), Description("Gets the number of elements contained in the collection.")]
        public int Count
        {
            get
            {
                int count = 0;
                foreach (ImageGalleryItem item in _imageGallery.Items)
                    if (item.Checked) count++;
                return count;
            }
        }            /// <summary>
                     /// Gets a value indicating whether the <see cref="CheckedItemCollection"/> is read-only.
                     /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets a value indicating whether the collection is read-only.")]
        public bool IsReadOnly => true;

        /// <summary>
        /// Gets the <see cref="ImageGallery"/> owning this collection.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the ImageGallery owning this collection.")]
        public ImageGallery ImageGalleryOwner => _imageGallery;

        /// <summary>
        /// Gets or sets the <see cref="ImageGalleryItem"/> at the specified index.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets or sets the item at the specified index")]
        public ImageGalleryItem this[int index]
        {
            get
            {
                int i = 0;
                foreach (ImageGalleryItem item in this)
                {
                    if (i == index)
                        return item;
                    i++;
                }
                throw new ArgumentException("No item with the given index exists.", nameof(index));
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Determines whether the <see cref="CheckedItemCollection"/> contains a specific value.
        /// </summary>
        /// <param name="item">The <see cref="ImageGalleryItem"/> to locate 
        /// in the <see cref="CheckedItemCollection"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> is found in the 
        /// <see cref="CheckedItemCollection"/>; otherwise, false.
        /// </returns>
        public bool Contains(ImageGalleryItem item)
        {
            return item.Checked && _imageGallery.Items.Contains(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ImageGalleryItem> GetEnumerator()
        {
            return new CheckedItemEnumerator(_imageGallery.Items);
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        internal void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        internal void Clear(bool raiseEvent)
        {
            foreach (ImageGalleryItem item in this)
            {
                item.mChecked = false;
                if (raiseEvent && _imageGallery != null)
                    _imageGallery.OnItemCheckBoxClickInternal(item);
            }
        }

        #endregion


        #region Unsupported Interface
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <exception cref="T:System.NotSupportedException">
        /// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </exception>
        void ICollection<ImageGalleryItem>.Add(ImageGalleryItem item)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        void ICollection<ImageGalleryItem>.Clear()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        void ICollection<ImageGalleryItem>.CopyTo(ImageGalleryItem[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        [Obsolete("Use ImageGallleryItem.Index property instead.")]
        int IList<ImageGalleryItem>.IndexOf(ImageGalleryItem item)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        void IList<ImageGalleryItem>.Insert(int index, ImageGalleryItem item)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        bool ICollection<ImageGalleryItem>.Remove(ImageGalleryItem item)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        void IList<ImageGalleryItem>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        ImageGalleryItem IList<ImageGalleryItem>.this[int index]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


        #region Internal Classes
        /// <summary>
        /// Represents an enumerator to walk though the checked items.
        /// </summary>
        internal class CheckedItemEnumerator : IEnumerator<ImageGalleryItem>
        {
            #region Member Variables
            private ItemCollection owner;
            private int current;
            private Guid lastItem;
            #endregion

            #region Constructor

            public CheckedItemEnumerator(ItemCollection collection)
            {
                owner = collection;
                current = -1;
                lastItem = Guid.Empty;
            }
            #endregion

            #region Properties

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            public ImageGalleryItem Current
            {
                get
                {
                    if (current == -1 || current > owner.Count - 1)
                        throw new InvalidOperationException();
                    return owner[current];
                }
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current
            {
                get { return Current; }
            }

            #endregion

            #region Instance Methods
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                ;
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            public bool MoveNext()
            {
                // Did we reach the end?
                if (current > owner.Count - 1)
                {
                    lastItem = Guid.Empty;
                    return false;
                }

                // Move to the next item if:
                // 1. We are before the first item. - OR -
                // 2. The current item is the same as the one we enumerated before. 
                //    The current item may have differed if the user for example 
                //    removed the current item between MoveNext calls. - OR -
                // 3. The current item is not checked.
                while (current == -1 ||
                    owner[current].Guid == lastItem ||
                    owner[current].Checked == false)
                {
                    current++;
                    if (current > owner.Count - 1)
                    {
                        lastItem = Guid.Empty;
                        return false;
                    }
                }

                // Cache the last item
                lastItem = owner[current].Guid;
                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                current = -1;
                lastItem = Guid.Empty;
            }

            #endregion
        }
        #endregion
    }

}

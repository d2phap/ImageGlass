using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    /// <summary>
    /// Represents available levels of zoom in an <see cref="ImageBox"/> control
    /// </summary>
    public class ImageBoxZoomLevelCollection: IList<int> {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomLevelCollection"/> class.
        /// </summary>
        public ImageBoxZoomLevelCollection() {
            List = new SortedList<int, int>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomLevelCollection"/> class.
        /// </summary>
        /// <param name="collection">The default values to populate the collection with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null</exception>
        public ImageBoxZoomLevelCollection(IEnumerable<int> collection)
          : this() {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            AddRange(collection);
        }

        #endregion

        #region Public Class Properties

        /// <summary>
        /// Returns the default zoom levels
        /// </summary>
        public static ImageBoxZoomLevelCollection Default {
            get {
                return new ImageBoxZoomLevelCollection(new[]
                                               {
                                         7, 10, 15, 20, 25, 30, 50, 70, 100, 150, 200, 300, 400, 500, 600, 700, 800, 1200, 1600, 2000, 2500, 3000, 3500
                                       });
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ImageBoxZoomLevelCollection" />.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ImageBoxZoomLevelCollection" />.
        /// </returns>
        public int Count {
            get { return List.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the zoom level at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public int this[int index] {
            get { return List.Values[index]; }
            set {
                List.RemoveAt(index);
                Add(value);
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the backing list.
        /// </summary>
        protected SortedList<int, int> List { get; set; }

        #endregion

        #region Public Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(int item) {
            List.Add(item, item);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="ImageBoxZoomLevelCollection"/>.
        /// </summary>
        /// <param name="collection">The items to add to the collection.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the <c>collection</c> parameter is null.</exception>
        public void AddRange(IEnumerable<int> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            foreach (int value in collection) {
                Add(value);
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear() {
            List.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.</returns>
        public bool Contains(int item) {
            return List.ContainsKey(item);
        }

        /// <summary>
        /// Copies a range of elements this collection into a destination <see cref="Array"/>.
        /// </summary>
        /// <param name="array">The <see cref="Array"/> that receives the data.</param>
        /// <param name="arrayIndex">A 64-bit integer that represents the index in the <see cref="Array"/> at which storing begins.</param>
        public void CopyTo(int[] array, int arrayIndex) {
            for (int i = 0; i < Count; i++) {
                array[arrayIndex + i] = List.Values[i];
            }
        }

        /// <summary>
        /// Finds the index of a zoom level matching or nearest to the specified value.
        /// </summary>
        /// <param name="zoomLevel">The zoom level.</param>
        public int FindNearest(int zoomLevel) {
            int nearestValue = List.Values[0];
            int nearestDifference = Math.Abs(nearestValue - zoomLevel);
            for (int i = 1; i < Count; i++) {
                int value = List.Values[i];
                int difference = Math.Abs(value - zoomLevel);
                if (difference < nearestDifference) {
                    nearestValue = value;
                    nearestDifference = difference;
                }
            }
            return nearestValue;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerator<int> GetEnumerator() {
            return List.Values.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        public int IndexOf(int item) {
            return List.IndexOfKey(item);
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.NotImplementedException">Not implemented</exception>
        public void Insert(int index, int item) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the next increased zoom level for the given current zoom.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level.</param>
        /// <returns>The next matching increased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
        public int NextZoom(int zoomLevel) {
            int index;

            index = IndexOf(FindNearest(zoomLevel));
            if (index < Count - 1) {
                index++;
            }

            return this[index];
        }

        /// <summary>
        /// Returns the next decreased zoom level for the given current zoom.
        /// </summary>
        /// <param name="zoomLevel">The current zoom level.</param>
        /// <returns>The next matching decreased zoom level for the given current zoom if applicable, otherwise the nearest zoom.</returns>
        public int PreviousZoom(int zoomLevel) {
            int index;

            index = IndexOf(FindNearest(zoomLevel));
            if (index > 0) {
                index--;
            }

            return this[index];
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public bool Remove(int item) {
            return List.Remove(item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="ImageBoxZoomLevelCollection"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public void RemoveAt(int index) {
            List.RemoveAt(index);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ImageBoxZoomLevelCollection"/> to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the <see cref="ImageBoxZoomLevelCollection"/>.</returns>
        public int[] ToArray() {
            int[] results;

            results = new int[Count];
            CopyTo(results, 0);

            return results;
        }

        #endregion

        #region IList<int> Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="ImageBoxZoomLevelCollection" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}

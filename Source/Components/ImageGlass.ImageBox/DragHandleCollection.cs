using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    public class DragHandleCollection: IEnumerable<DragHandle> {
        #region Instance Fields

        private readonly IDictionary<DragHandleAnchor, DragHandle> _items;

        #endregion

        #region Public Constructors

        public DragHandleCollection() {
            _items = new Dictionary<DragHandleAnchor, DragHandle>();
            _items.Add(DragHandleAnchor.TopLeft, new DragHandle(DragHandleAnchor.TopLeft));
            _items.Add(DragHandleAnchor.TopCenter, new DragHandle(DragHandleAnchor.TopCenter));
            _items.Add(DragHandleAnchor.TopRight, new DragHandle(DragHandleAnchor.TopRight));
            _items.Add(DragHandleAnchor.MiddleLeft, new DragHandle(DragHandleAnchor.MiddleLeft));
            _items.Add(DragHandleAnchor.MiddleRight, new DragHandle(DragHandleAnchor.MiddleRight));
            _items.Add(DragHandleAnchor.BottomLeft, new DragHandle(DragHandleAnchor.BottomLeft));
            _items.Add(DragHandleAnchor.BottomCenter, new DragHandle(DragHandleAnchor.BottomCenter));
            _items.Add(DragHandleAnchor.BottomRight, new DragHandle(DragHandleAnchor.BottomRight));
        }

        #endregion

        #region Public Properties

        public int Count {
            get { return _items.Count; }
        }

        public DragHandle this[DragHandleAnchor index] {
            get { return _items[index]; }
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<DragHandle> GetEnumerator() {
            return _items.Values.GetEnumerator();
        }

        public DragHandleAnchor HitTest(Point point) {
            DragHandleAnchor result;

            result = DragHandleAnchor.None;

            foreach (DragHandle handle in this) {
                if (handle.Visible && handle.Bounds.Contains(point)) {
                    result = handle.Anchor;
                    break;
                }
            }

            return result;
        }

        #endregion

        #region IEnumerable<DragHandle> Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion
    }
}

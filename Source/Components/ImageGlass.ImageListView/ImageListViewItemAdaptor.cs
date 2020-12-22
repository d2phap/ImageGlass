using System;
using System.Drawing;

namespace ImageGlass.ImageListView {
    public partial class ImageListView {
        /// <summary>
        /// Represents the abstract case class for adaptors.
        /// </summary>
        public abstract class ImageListViewItemAdaptor: IDisposable {
            #region Abstract Methods
            /// <summary>
            /// Returns the thumbnail image for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="size">Requested image size.</param>
            /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
            /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
            public abstract Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation, bool useWIC);
            /// <summary>
            /// Returns the path to the source image for use in drag operations.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <returns>The path to the source image.</returns>
            public abstract string GetSourceImage(object key);
            /// <summary>
            /// Returns the details for the given item.
            /// </summary>
            /// <param name="key">Item key.</param>
            /// <param name="useWIC">true to use Windows Imaging Component; otherwise false.</param>
            /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
            public abstract Utility.Tuple<ColumnType, string, object>[] GetDetails(object key, bool useWIC);
            /// <summary>
            /// Performs application-defined tasks associated with freeing,
            /// releasing, or resetting unmanaged resources.
            /// </summary>
            public abstract void Dispose();
            #endregion
        }
    }
}

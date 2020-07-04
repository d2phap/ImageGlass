using System;

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    /// <summary>
    /// Contains event data for the <see cref="ImageBox.ZoomChanged"/> event.
    /// </summary>
    public class ImageBoxZoomEventArgs: EventArgs {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomEventArgs"/> class.
        /// </summary>
        /// <param name="actions">The zoom operation being performed.</param>
        /// <param name="source">The source of the operation.</param>
        /// <param name="oldZoom">The old zoom level.</param>
        /// <param name="newZoom">The new zoom level.</param>
        public ImageBoxZoomEventArgs(ImageBoxZoomActions actions, ImageBoxActionSources source, double oldZoom, double newZoom)
          : this() {
            Actions = actions;
            Source = source;
            OldZoom = oldZoom;
            NewZoom = newZoom;
        }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxZoomEventArgs"/> class.
        /// </summary>
        protected ImageBoxZoomEventArgs() { }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the actions that occured.
        /// </summary>
        /// <value>The zoom operation.</value>
        public ImageBoxZoomActions Actions { get; protected set; }

        /// <summary>
        /// Gets or sets the new zoom level.
        /// </summary>
        /// <value>The new zoom level.</value>
        public double NewZoom { get; protected set; }

        /// <summary>
        /// Gets or sets the old zoom level.
        /// </summary>
        /// <value>The old zoom level.</value>
        public double OldZoom { get; protected set; }

        /// <summary>
        /// Gets or sets the source of the operation..
        /// </summary>
        /// <value>The source.</value>
        public ImageBoxActionSources Source { get; protected set; }

        #endregion
    }
}

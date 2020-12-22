using System.ComponentModel;
using System.Drawing;

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    /// <summary>
    /// Provides data for a cancelable event.
    /// </summary>
    public class ImageBoxCancelEventArgs: CancelEventArgs {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxCancelEventArgs"/> class.
        /// </summary>
        /// <param name="location">The location of the action being performed.</param>
        public ImageBoxCancelEventArgs(Point location)
          : this() {
            Location = location;
        }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageBoxCancelEventArgs"/> class.
        /// </summary>
        protected ImageBoxCancelEventArgs() { }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the location of the action being performed.
        /// </summary>
        /// <value>The location of the action being performed.</value>
        public Point Location { get; protected set; }

        #endregion
    }
}

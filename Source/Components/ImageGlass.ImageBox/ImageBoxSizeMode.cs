namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    /// <summary>
    /// Determines the sizing mode of an image hosted in an <see cref="ImageBox" /> control.
    /// </summary>
    public enum ImageBoxSizeMode {
        /// <summary>
        /// The image is disiplayed according to current zoom and scroll properties.
        /// </summary>
        Normal,

        /// <summary>
        /// The image is stretched to fill the client area of the control.
        /// </summary>
        Stretch,

        /// <summary>
        /// The image is stretched to fill as much of the client area of the control as possible, whilst retaining the same aspect ratio for the width and height.
        /// </summary>
        Fit
    }
}

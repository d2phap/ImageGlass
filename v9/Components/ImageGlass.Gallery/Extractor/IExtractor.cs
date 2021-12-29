namespace ImageGlass.Gallery;

/// <summary>
/// Interface for thumbnail, metadata and shell info extractors.
/// </summary>
public interface IExtractor
{
    /// <summary>
    /// Gets the name of this extractor.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Returns image metadata.
    /// </summary>
    /// <param name="path">Filepath of image</param>
    Metadata GetMetadata(string path);

    /// <summary>
    /// Creates a thumbnail from the given image file.
    /// </summary>
    /// <param name="filename">The filename pointing to an image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given file or null if an error occurs.</returns>
    Image GetThumbnail(string filename, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation);

    /// <summary>
    /// Creates a thumbnail from the given image.
    /// </summary>
    /// <param name="image">The source image.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given image or null if an error occurs.</returns>
    Image GetThumbnail(Image image, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation);

    /// <summary>
    /// Returns shell info.
    /// </summary>
    /// <param name="path">Filepath of image</param>
    ShellInfo GetShellInfo(string path);
}

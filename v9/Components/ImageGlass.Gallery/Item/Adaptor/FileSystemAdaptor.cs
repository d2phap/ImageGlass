using ImageGlass.Base;
using ImageGlass.Base.Cache;
using System.Text;

namespace ImageGlass.Gallery;



/// <summary>
/// Represents a file system adaptor.
/// </summary>
public class FileSystemAdaptor : IAdaptor
{
    // [IG_CHANGE] use a cache for commonly repeated strings
    private static StringCache _stringCache = new();

    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemAdaptor"/> class.
    /// </summary>
    public FileSystemAdaptor()
    {
        disposed = false;
    }

    /// <summary>
    /// Returns the thumbnail image for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>The thumbnail image from the given item or null if an error occurs.</returns>
    public override Image? GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        if (disposed)
            return null;

        // [IG_CHANGE] Issue #530: thumbnails not built if long file path
        string filename = Helpers.PrefixLongPath((string)key);

        if (File.Exists(filename))
            return Extractor.Instance.GetThumbnail(filename, size, useEmbeddedThumbnails, useExifOrientation);
        else
            return null;
    }

    /// <summary>
    /// Returns a unique identifier for this thumbnail to be used in persistent
    /// caching.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>A unique identifier string for the thumnail.</returns>
    public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        var sb = new StringBuilder();
        sb.Append((string)key);// Filename
        sb.Append(':');
        sb.Append(size.Width); // Thumbnail size
        sb.Append(',');
        sb.Append(size.Height);
        sb.Append(':');
        sb.Append(useEmbeddedThumbnails);
        sb.Append(':');
        sb.Append(useExifOrientation);

        return sb.ToString();
    }

    /// <summary>
    /// Returns the path to the source image for use in drag operations.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>The path to the source image.</returns>
    public override string GetSourceImage(object key)
    {
        if (disposed)
            return string.Empty;

        return (string)key;
    }

    /// <summary>
    /// Returns the details for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
    public override Tuple<ColumnType, string, object>[] GetDetails(object key)
    {
        if (disposed)
            return Array.Empty<Tuple<ColumnType, string, object>>();

        var filename = (string)key;
        var details = new List<Tuple<ColumnType, string, object>>();

        // Get file info
        if (File.Exists(filename))
        {
            var info = new FileInfo(filename);

            // [IG_CHANGE] use string cache
            details.Add(new(ColumnType.FilePath, string.Empty, _stringCache.GetFromCache(info.DirectoryName ?? "")));

            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, info.Directory?.Name ?? ""));
        }

        return details.ToArray();
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() => disposed = true;

}


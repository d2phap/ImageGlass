using System.Text;

namespace ImageGlass.Gallery;



/// <summary>
/// Represents a file system adaptor.
/// </summary>
public class FileSystemAdaptor : ImageListViewItemAdaptor
{
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
    public override Image GetThumbnail(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        if (disposed)
            return null;

        string filename = (string)key;
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
        StringBuilder sb = new StringBuilder();
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
            return null;

        string filename = (string)key;
        return filename;
    }
    /// <summary>
    /// Returns the details for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>An array of tuples containing item details or null if an error occurs.</returns>
    public override Tuple<ColumnType, string, object>[] GetDetails(object key)
    {
        if (disposed)
            return null;

        string filename = (string)key;
        List<Tuple<ColumnType, string, object>> details = new List<Tuple<ColumnType, string, object>>();

        // Get file info
        if (File.Exists(filename))
        {
            FileInfo info = new FileInfo(filename);
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.DateCreated, string.Empty, info.CreationTime));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.DateAccessed, string.Empty, info.LastAccessTime));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.DateModified, string.Empty, info.LastWriteTime));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FileSize, string.Empty, info.Length));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FilePath, string.Empty, info.DirectoryName ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FolderName, string.Empty, info.Directory.Name ?? ""));

            // Get metadata
            Metadata metadata = Extractor.Instance.GetMetadata(filename);
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Dimensions, string.Empty, new Size(metadata.Width, metadata.Height)));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Resolution, string.Empty, new SizeF((float)metadata.DPIX, (float)metadata.DPIY)));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.ImageDescription, string.Empty, metadata.ImageDescription ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.EquipmentModel, string.Empty, metadata.EquipmentModel ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.DateTaken, string.Empty, metadata.DateTaken));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Artist, string.Empty, metadata.Artist ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Copyright, string.Empty, metadata.Copyright ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.ExposureTime, string.Empty, (float)metadata.ExposureTime));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FNumber, string.Empty, (float)metadata.FNumber));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.ISOSpeed, string.Empty, (ushort)metadata.ISOSpeed));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.UserComment, string.Empty, metadata.Comment ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Rating, string.Empty, (ushort)metadata.Rating));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.Software, string.Empty, metadata.Software ?? ""));
            details.Add(new Tuple<ColumnType, string, object>(ColumnType.FocalLength, string.Empty, (float)metadata.FocalLength));
        }

        return details.ToArray();
    }
    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose()
    {
        disposed = true;
    }
}


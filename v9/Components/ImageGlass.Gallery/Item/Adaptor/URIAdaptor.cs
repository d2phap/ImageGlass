using System.Net;
using System.Text;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents a URI adaptor.
/// </summary>
public class UriAdaptor : IAdaptor
{
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UriAdaptor"/> class.
    /// </summary>
    public UriAdaptor()
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

        string uri = (string)key;
        try
        {
            using var client = new WebClient();
            byte[] imageData = client.DownloadData(uri);
            using var stream = new MemoryStream(imageData);
            using var sourceImage = Image.FromStream(stream);

            return Extractor.Instance.GetThumbnail(sourceImage, size, useEmbeddedThumbnails, useExifOrientation);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a unique identifier for this thumbnail to be used in persistent
    /// caching.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <param name="size">Requested image size.</param>
    /// <param name="useEmbeddedThumbnails">Embedded thumbnail usage.</param>
    /// <param name="useExifOrientation">true to automatically rotate images based on Exif orientation; otherwise false.</param>
    /// <returns>A unique identifier string for the thumbnail.</returns>
    public override string GetUniqueIdentifier(object key, Size size, UseEmbeddedThumbnails useEmbeddedThumbnails, bool useExifOrientation)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append((string)key);// Uri
        sb.Append(':');
        sb.Append(size.Width); // Thumbnail size
        sb.Append(',');
        sb.Append(size.Height);
        sb.Append(':');
        sb.Append(useEmbeddedThumbnails);
        sb.Append(':');
        sb.Append(useExifOrientation);
        sb.Append(':');
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

        string uri = (string)key;
        try
        {
            string filename = Path.GetTempFileName();
            using var client = new WebClient();
            client.DownloadFile(uri, filename);
            return filename;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns the details for the given item.
    /// </summary>
    /// <param name="key">Item key.</param>
    /// <returns>An array of 2-tuples containing item details or null if an error occurs.</returns>
    public override Tuple<ColumnType, string, object>[] GetDetails(object key)
    {
        if (disposed)
            return null;

        string uri = (string)key;
        List<Tuple<ColumnType, string, object>> details = new List<Tuple<ColumnType, string, object>>();

        details.Add(new Tuple<ColumnType, string, object>(ColumnType.Custom, "URL", uri));

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


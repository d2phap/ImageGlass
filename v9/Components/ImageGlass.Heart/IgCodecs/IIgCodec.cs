
namespace ImageGlass.Heart;


    /// <summary>
    /// Settings for loading image
    /// </summary>
public struct CodecReadSettings
{
    public int Width;
    public int Height;
    public bool IgnoreColorProfile;
}


/// <summary>
/// Provides interface of image codec
/// </summary>
public interface IIgCodec
{
    /// <summary>
    /// Full name of assembly type of this codec
    /// </summary>
    public string CodecId => GetType().FullName ?? string.Empty;

    /// <summary>
    /// Name of the codec
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Short description of the codec
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Author of the codec
    /// </summary>
    public string Author { get; }

    /// <summary>
    /// Get the supported extensions
    /// </summary>
    public List<string> SupportedExts { get; }

    /// <summary>
    /// Contact info of the codec author
    /// </summary>
    public string Contact { get; }


    /// <summary>
    /// Version of the codec
    /// </summary>
    public Version Version { get; }


    /// <summary>
    /// Version of the API used in writting the codec
    /// </summary>
    public Version ApiVersion { get; }


    /// <summary>
    /// Loads image file and returns BitmapSource
    /// </summary>
    /// <param name="filename">Full path of the file</param>
    /// <param name="settings">Loading settings</param>
    /// <returns></returns>
    public Bitmap Load(string filename, CodecReadSettings settings);


    /// <summary>
    /// Gets thumbnail from image
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public byte[] GetThumbnail(string filename, int width, int height);


    /// <summary>
    /// Gets base64 thumbnail from image
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public string GetThumbnailBase64(string filename, int width, int height);
}


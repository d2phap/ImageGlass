
namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Provides interface of image codec
/// </summary>
public interface IIgCodec : IDisposable
{
    #region Default properties
    /// <summary>
    /// Gets codec full file path. Example: <c>C:\my folder\MyCodec.dll</c>
    /// </summary>
    public string FullFilename => GetType().Assembly.Location;

    /// <summary>
    /// Gets codec filename. Example: <c>MyCodec.dll</c>
    /// </summary>
    public string Filename => Path.GetFileName(FullFilename);

    /// <summary>
    /// Full name of assembly type of this codec
    /// </summary>
    public virtual string Id => GetType().FullName ?? string.Empty;

    #endregion


    #region Public properties

    /// <summary>
    /// Name of the codec
    /// </summary>
    public string DisplayName { get; }

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

    #endregion



    /// <summary>
    /// Initialize the codec
    /// </summary>
    void Initialize() { }


    /// <summary>
    /// Loads metadata from file.
    /// </summary>
    /// <param name="filename">Full path of the file</param>
    /// <returns></returns>
    IgMetadata? LoadMetadata(string filename, CodecReadOptions? options = default);


    /// <summary>
    /// Loads image file async.
    /// </summary>
    /// <param name="filename">Full path of the file</param>
    /// <param name="options">Loading options</param>
    /// <param name="token">Cancellation token</param>
    Task<IgImgData> LoadAsync(string filename,
        CodecReadOptions? options = default,
        CancellationToken? token = null);


    /// <summary>
    /// Loads image file.
    /// </summary>
    /// <param name="filename">Full path of the file</param>
    /// <param name="options">Loading options</param>
    /// <returns></returns>
    IgImgData Load(string filename, CodecReadOptions? options = default);


    /// <summary>
    /// Gets thumbnail from image.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    Bitmap? GetThumbnail(string filename, int width, int height);


    /// <summary>
    /// Gets base64 thumbnail from image
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    string GetThumbnailBase64(string filename, int width, int height);

}


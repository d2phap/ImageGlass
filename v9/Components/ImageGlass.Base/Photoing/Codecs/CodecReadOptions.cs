
namespace ImageGlass.Base.Photoing.Codecs;

/// <summary>
/// Settings for loading image
/// </summary>
public record CodecReadOptions
{
    /// <summary>
    /// Gets, sets the requested width of the image
    /// </summary>
    public int Width = 0;

    /// <summary>
    /// Gets, sets the requested height of the image
    /// </summary>
    public int Height = 0;

    /// <summary>
    /// Gets, sets the value indicates whether the color profile should be ignored.
    /// </summary>
    public bool IgnoreColorProfile = false;

    /// <summary>
    /// Gets sets ColorProfile name of path
    /// </summary>
    public string ColorProfileName { get; set; } = "sRGB";

    /// <summary>
    /// Gets, sets the value indicates if the <see cref="ColorProfileName"/>
    /// should apply to all image files
    /// </summary>
    public bool IsApplyColorProfileForAll { get; set; } = false;

    /// <summary>
    /// Gets, sets the value of ImageMagick.Channels to apply to the entire image list
    /// </summary>
    public ColorChannels ImageChannel { get; set; } = ColorChannels.All;

    /// <summary>
    /// Gets, sests the value indicates that the embedded thumbnail should be return (if found).
    /// </summary>
    public bool UseEmbeddedThumbnail { get; set; } = false;

    /// <summary>
    /// Gets, sets the value indicates the RAW embedded thumbnail should be returned (if found).
    /// </summary>
    public bool UseRawThumbnail { get; set; } = true;

    /// <summary>
    /// Gets, sets the value indicates that the first frame of the image should be return.
    /// </summary>
    public bool FirstFrameOnly { get; set; } = false;

    /// <summary>
    /// Gets, sets metadata
    /// </summary>
    public IgMetadata? Metadata = null;


    /// <summary>
    /// Initializes <see cref="CodecReadOptions"/> instance.
    /// </summary>
    /// <param name="meta"></param>
    public CodecReadOptions(IgMetadata? meta = null)
    {
        Metadata = meta;
    }
}

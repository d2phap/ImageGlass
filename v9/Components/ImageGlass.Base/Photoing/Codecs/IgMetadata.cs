
namespace ImageGlass.Base.Photoing.Codecs;


public class IgMetadata
{
    public int Width { get; set; } = 0;

    public int Height { get; set; } = 0;

    public int FramesCount { get; set; } = 0;

    public int ExifRating { get; set; } = 0;
    public DateTime? ExifDateTimeOriginal { get; set; } = null;
    public DateTime? ExifDateTime { get; set; } = null;
}

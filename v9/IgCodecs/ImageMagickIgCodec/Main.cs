
using ImageGlass.Heart;

namespace ImageMagick.IgCodec;

public class Main : IIgCodec
{
    public string DisplayName => "Magick.NET codec";
    public string Description => "Use ImageMagick.NET to decode file formats";
    public string Author => "Duong Dieu Phap";
    public string Contact => "phap@imageglass.org";
    public Version Version => new(1, 0, 0, 0);
    public Version ApiVersion => new(0, 5);
    public List<string> SupportedExts => new()
    {
        ".3fr",
        ".ari",
        ".arw",
        ".avif",
        ".b64",
        ".bay",
        ".bmp",
        ".cap",
        ".cr2",
        ".cr3",
        ".crw",
        ".cur",
        ".cut",
        ".dcr",
        ".dcs",
        ".dds",
        ".dib",
        ".dng",
        ".drf",
        ".eip",
        ".emf",
        ".erf",
        ".exif",
        ".exr",
        ".fax",
        ".fff",
        ".gif",
        ".gpr",
        ".hdr",
        ".heic",
        ".heif",
        ".ico",
        ".iiq",
        ".jfif",
        ".jp2",
        ".jpe",
        ".jpeg",
        ".jpg",
        ".jxl",
        ".k25",
        ".kdc",
        ".mdc",
        ".mef",
        ".mos",
        ".mrw",
        ".nef",
        ".nrw",
        ".obm",
        ".orf",
        ".pbm",
        ".pcx",
        ".pef",
        "pdf",
        ".pgm",
        ".png",
        ".ppm",
        ".psb",
        ".psd",
        ".ptx",
        ".pxn",
        ".r3d",
        ".raf",
        ".raw",
        ".rw2",
        ".rwl",
        ".rwz",
        ".sr2",
        ".srf",
        ".srw",
        ".svg",
        ".tga",
        ".tif",
        ".tiff",
        ".webp",
        ".wmf",
        ".wpg",
        ".x3f",
        ".xbm",
        ".xpm",
    };


    public Bitmap Load(string filename, CodecReadSettings settings)
    {
        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var imgMSettings = new MagickReadSettings();


        if (ext == ".SVG")
        {
            imgMSettings.BackgroundColor = MagickColors.Transparent;
        }

        if (settings.Width > 0 && settings.Height > 0)
        {
            imgMSettings.Width = settings.Width;
            imgMSettings.Height = settings.Height;
        }


        using var imgM = new MagickImage(filename, imgMSettings);


        return imgM.ToBitmap();
    }


    public byte[] GetThumbnail(string filename, int width, int height)
    {
        byte[] result = Array.Empty<byte>();
        using var imgM = new MagickImage();


        try
        {
            imgM.Ping(filename);
        }
        // exit on invalid image
        catch { return result; }


        // try to get RAW thumbnail
        var rawProfile = imgM.GetProfile("dng:thumbnail");

        if (rawProfile is not null)
        {
            var imgBytes = rawProfile?.GetData();

            if (imgBytes is not null)
            {
                using var rawImgM = new MagickImage(imgBytes);

                if (imgM.BaseWidth > width || imgM.BaseHeight > height)
                {
                    rawImgM.Thumbnail(width, height);
                    result = rawImgM.ToByteArray(MagickFormat.WebP);
                }
            }
        }


        // cannot find raw thumbnail, try to create a thumbnail
        if (result.Length == 0)
        {
            // read entire file content
            imgM.Read(filename);

            try
            {
                var profile = imgM.GetExifProfile();
                using var thumbM = profile?.CreateThumbnail();

                if (thumbM is not null)
                {
                    result = thumbM.ToByteArray(MagickFormat.WebP);
                }
            }
            catch { }
        }


        // cannot create thumbnail, resize the image file
        if (result.Length == 0)
        {
            if (imgM.BaseWidth > width || imgM.BaseHeight > height)
            {
                imgM.Thumbnail(width, height);
            }

            result = imgM.ToByteArray(MagickFormat.WebP);
        }


        return result;
    }


    public string GetThumbnailBase64(string filename, int width, int height)
    {
        var bytes = GetThumbnail(filename, width, height);

        if (bytes.Length > 0)
        {
            using var imgM = new MagickImage(bytes);

            return "data:image/webp;base64," + imgM.ToBase64(MagickFormat.WebP);
        }

        return string.Empty;
    }
}

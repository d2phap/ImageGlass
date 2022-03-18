/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using ImageGlass.Base;
using ImageGlass.Base.Photoing.Codecs;
using ImageMagick.Formats;

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

    public void Dispose() { }

    private MagickReadSettings ParseSettings(CodecReadOptions? options, string filename = "")
    {
        options ??= new();

        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var settings = new MagickReadSettings();

        if (ext == ".SVG")
        {
            settings.BackgroundColor = MagickColors.Transparent;
            settings.SetDefine("svg:xml-parse-huge", "true");
        }

        if (options.Width > 0 && options.Height > 0)
        {
            settings.Width = options.Width;
            settings.Height = options.Height;
        }

        // Fixed #708: length and filesize do not match
        settings.SetDefines(new BmpReadDefines
        {
            IgnoreFileSize = true,
        });

        // Fix RAW color
        settings.SetDefines(new DngReadDefines()
        {
            UseCameraWhitebalance = true,
            OutputColor = DngOutputColor.AdobeRGB,
            ReadThumbnail = true,
        });


        return settings;
    }


    public IgMetadata? LoadMetadata(string filename, CodecReadOptions? options = null)
    {
        IgMetadata? meta = null;

        try
        {
            var settings = ParseSettings(options, filename);

            using var imgC = new MagickImageCollection();
            imgC.Ping(filename, settings);

            using var imgM = new MagickImage();
            imgM.Ping(filename, settings);

            meta = new()
            {
                Width = imgM.Width,
                Height = imgM.Height,
                FramesCount = imgC.Count,
            };
        }
        catch { }

        return meta;
    }


    // load image sync
    public IgImgData Load(string filename, CodecReadOptions? options = null)
    {
        return ImageGlass.Base.Helpers.RunSync(() => LoadAsync(filename, options));
    }


    // load image async
    public async Task<IgImgData> LoadAsync(string filename,
        CodecReadOptions? options = null,
        CancellationToken? token = null)
    {
        options ??= new();
        var cancelToken = token ?? default;

        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var settings = ParseSettings(options, filename);

        Bitmap? output;
        IColorProfile? colorProfile = null;
        IExifProfile? exifProfile = null;

        #region Read image data
        switch (ext)
        {
            case ".TXT": // base64 string
            case ".B64":
                var base64Content = string.Empty;
                using (var fs = new StreamReader(filename))
                {
                    base64Content = fs.ReadToEnd();
                }

                output = ConvertBase64ToBitmap(base64Content);
                break;

            case ".GIF":
            case ".FAX":
                try
                {
                    // Note: Using FileStream is much faster than using MagickImageCollection
                    output = ConvertFileToBitmap(filename);
                }
                catch
                {
                    (output, exifProfile, colorProfile) = await ReadWithMagickImage(filename, ext, settings, options, cancelToken);
                }
                break;


            default:
                (output, exifProfile, colorProfile) = await ReadWithMagickImage(filename, ext, settings, options, cancelToken);

                break;
        }
        #endregion


        return new IgImgData()
        {
            Image = output,
            ExifProfile = exifProfile,
            ColorProfile = colorProfile,
        };
    }


    // get thumbnail
    public Bitmap? GetThumbnail(string filename, int width, int height)
    {
        Bitmap? result = null;
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
                    result = rawImgM.ToBitmap();
                }
            }
        }


        // cannot find raw thumbnail, try to create a thumbnail
        if (result is null)
        {
            // read entire file content
            imgM.Read(filename);

            try
            {
                var profile = imgM.GetExifProfile();
                using var thumbM = profile?.CreateThumbnail();

                if (thumbM is not null)
                {
                    result = thumbM.ToBitmap();
                }
            }
            catch { }
        }


        // cannot create thumbnail, resize the image file
        if (result is null)
        {
            if (imgM.BaseWidth > width || imgM.BaseHeight > height)
            {
                imgM.Thumbnail(width, height);
            }

            result = imgM.ToBitmap();
        }

        return result;
    }


    // get thumbnail as base64 string
    public string GetThumbnailBase64(string filename, int width, int height)
    {
        var thumbnail = GetThumbnail(filename, width, height);

        if (thumbnail is not null)
        {
            using var imgM = new MagickImage();
            imgM.Read(thumbnail);

            return "data:image/png;base64," + imgM.ToBase64(MagickFormat.Png);
        }

        return string.Empty;
    }


    #region Private functions

    /// <summary>
    /// Read image file using Magick.NET
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="ext"></param>
    /// <param name="settings"></param>
    /// <param name="options"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    private static async Task<(Bitmap?, IExifProfile?, IColorProfile?)> ReadWithMagickImage(
        string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options,
        CancellationToken cancelToken)
    {
        Bitmap? bitmapOutput = null;
        IColorProfile? colorProfile = null;
        IExifProfile? exifProfile = null;

        var checkRotation = ext != ".HEIC";
        using var imgColl = new MagickImageCollection();

        // Issue #530: ImageMagick falls over if the file path is longer than the (old)
        // windows limit of 260 characters. Workaround is to read the file bytes,
        // but that requires using the "long path name" prefix to succeed.
        if (filename.Length > 260)
        {
            var newFilename = ImageGlass.Base.Helpers.PrefixLongPath(filename);
            var allBytes = File.ReadAllBytes(newFilename);

            imgColl.Ping(allBytes, settings);
        }
        else
        {
            imgColl.Ping(filename, settings);
        }


        // read all frames
        if (imgColl.Count > 1 && options.FirstFrameOnly is false)
        {
            await imgColl.ReadAsync(filename, settings, cancelToken);
            foreach (var imgPageM in imgColl)
            {
                (exifProfile, colorProfile) = ProcessMagickImage((MagickImage)imgPageM, options, ref bitmapOutput, checkRotation);
            }

            bitmapOutput = imgColl.ToBitmap();

            return (bitmapOutput, exifProfile, colorProfile);
        }


        // read a single frame only
        using var imgM = new MagickImage();
        if (options.UseRawThumbnail is true)
        {
            var profile = imgColl[0].GetProfile("dng:thumbnail");

            try
            {
                // try to get thumbnail
                var thumbnailData = profile?.GetData();
                if (thumbnailData != null)
                {
                    imgM.Read(thumbnailData, settings);
                }
                else
                {
                    await imgM.ReadAsync(filename, settings, cancelToken);
                }
            }
            catch
            {
                await imgM.ReadAsync(filename, settings, cancelToken);
            }
        }
        else
        {
            await imgM.ReadAsync(filename, settings, cancelToken);
        }


        // Issue #679: fix targa display with Magick.NET 7.15.x 
        if (ext == ".TGA") imgM.AutoOrient();


        // preprocess image
        imgM.Quality = options.Quality;
        (exifProfile, colorProfile) = ProcessMagickImage(imgM, options, ref bitmapOutput, checkRotation);

        // apply color channel
        using var channelImgM = FilterColorChannel(imgM, options.ImageChannel);
        bitmapOutput = channelImgM.ToBitmap();

        return (bitmapOutput, exifProfile, colorProfile);
    }

    /// <summary>
    /// Process Magick image
    /// </summary>
    /// <param name="imgM"></param>
    /// <param name="options"></param>
    /// <param name="bitmapOutput"></param>
    /// <param name="checkRotation"></param>
    /// <returns></returns>
    private static (IExifProfile?, IColorProfile?) ProcessMagickImage(
        MagickImage imgM, CodecReadOptions options,
        ref Bitmap? bitmapOutput,
        bool checkRotation = true)
    {
        imgM.Quality = options.Quality;

        IColorProfile? imgColorProfile = null;
        IExifProfile? exifProfile = null;
        try
        {
            // get the color profile of image
            imgColorProfile = imgM.GetColorProfile();

            // Get Exif information
            exifProfile = imgM.GetExifProfile();
        }
        catch { }

        // Use embedded thumbnails if specified
        if (exifProfile != null && options.UseEmbeddedThumbnail)
        {
            // Fetch the embedded thumbnail
            using var thumbM = exifProfile.CreateThumbnail();
            if (thumbM != null)
            {
                bitmapOutput = thumbM.ToBitmap();
            }
        }

        // Revert to source image if an embedded thumbnail with required size was not found.
        if (bitmapOutput == null)
        {
            if (exifProfile != null && checkRotation)
            {
                // Get Orientation Flag
                var exifRotationTag = exifProfile.GetValue(ExifTag.Orientation);

                if (exifRotationTag != null)
                {
                    if (int.TryParse(exifRotationTag.Value.ToString(), out var orientationFlag))
                    {
                        var orientationDegree = ImageGlass.Base.Helpers.GetOrientationDegree(orientationFlag);
                        if (orientationDegree != 0)
                        {
                            //Rotate image accordingly
                            imgM.Rotate(orientationDegree);
                        }
                    }
                }
            }

            // if always apply color profile
            // or only apply color profile if there is an embedded profile
            if (options.IsApplyColorProfileForAll || imgColorProfile != null)
            {
                var imgColor = Utils.GetColorProfile(options.ColorProfileName);

                if (imgColor != null)
                {
                    imgM.TransformColorSpace(
                        //set default color profile to sRGB
                        imgColorProfile ?? ColorProfile.SRGB,
                        imgColor);
                }
            }
        }

        return (exifProfile, imgColorProfile);
    }

    /// <summary>
    /// Filter color channel of Magick image
    /// </summary>
    /// <param name="imgM"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    private static MagickImage FilterColorChannel(MagickImage imgM, ColorChannel channel)
    {
        if (channel != ColorChannel.All)
        {
            var magickChannel = (Channels)channel;
            var channelImgM = (MagickImage)imgM.Separate(magickChannel).First();

            if (imgM.HasAlpha && magickChannel != Channels.Alpha)
            {
                using var alpha = imgM.Separate(Channels.Alpha).First();
                channelImgM.Composite(alpha, CompositeOperator.CopyAlpha);
            }

            return channelImgM;
        }

        return imgM;
    }

    /// <summary>
    /// Converts file to Bitmap
    /// </summary>
    /// <param name="filename">Full path of file</param>
    /// <returns></returns>
    private static Bitmap ConvertFileToBitmap(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        var ms = new MemoryStream();
        fs.CopyTo(ms);
        ms.Position = 0;

        return new Bitmap(ms, true);
    }

    /// <summary>
    /// Converts base64 string to Bitmap.
    /// </summary>
    /// <param name="content">Base64 string</param>
    /// <returns></returns>
    private static Bitmap? ConvertBase64ToBitmap(string content)
    {
        var (mimeType, rawData) = ImageGlass.Base.Helpers.ConvertBase64ToBytes(content);
        if (string.IsNullOrEmpty(mimeType)) return null;

        // supported MIME types:
        // https://www.iana.org/assignments/media-types/media-types.xhtml#image
        #region Settings
        var settings = new MagickReadSettings();

        switch (mimeType)
        {
            case "image/avif":
                settings.Format = MagickFormat.Avif;
                break;
            case "image/bmp":
                settings.Format = MagickFormat.Bmp;
                break;
            case "image/gif":
                settings.Format = MagickFormat.Gif;
                break;
            case "image/tiff":
                settings.Format = MagickFormat.Tiff;
                break;
            case "image/jpeg":
                settings.Format = MagickFormat.Jpeg;
                break;
            case "image/svg+xml":
                settings.BackgroundColor = MagickColors.Transparent;
                settings.Format = MagickFormat.Svg;
                break;
            case "image/x-icon":
                settings.Format = MagickFormat.Ico;
                break;
            case "image/x-portable-anymap":
                settings.Format = MagickFormat.Pnm;
                break;
            case "image/x-portable-bitmap":
                settings.Format = MagickFormat.Pbm;
                break;
            case "image/x-portable-graymap":
                settings.Format = MagickFormat.Pgm;
                break;
            case "image/x-portable-pixmap":
                settings.Format = MagickFormat.Ppm;
                break;
            case "image/x-xbitmap":
                settings.Format = MagickFormat.Xbm;
                break;
            case "image/x-xpixmap":
                settings.Format = MagickFormat.Xpm;
                break;
            case "image/x-cmu-raster":
                settings.Format = MagickFormat.Ras;
                break;
        }
        #endregion


        Bitmap? bmp = null;
        switch (settings.Format)
        {
            case MagickFormat.Gif:
            case MagickFormat.Gif87:
            case MagickFormat.Tif:
            case MagickFormat.Tiff64:
            case MagickFormat.Tiff:
            case MagickFormat.Ico:
            case MagickFormat.Icon:
                bmp = new Bitmap(new MemoryStream(rawData)
                {
                    Position = 0
                }, true);

                break;

            default:
                using (var imgM = new MagickImage(rawData, settings))
                {
                    bmp = imgM.ToBitmap();
                }
                break;
        }

        return bmp;
    }


    #endregion

}

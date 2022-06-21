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
using ImageMagick;
using ImageMagick.Formats;
using System.Runtime.CompilerServices;

namespace ImageGlass.Base.Photoing.Codecs;


/// <summary>
/// Handles reading and writing image file formats.
/// </summary>
public static class PhotoCodec
{

    #region Public functions

    /// <summary>
    /// Loads metadata from file.
    /// </summary>
    /// <param name="filePath">Full path of the file</param>
    public static IgMetadata? LoadMetadata(string filePath, CodecReadOptions? options = null)
    {
        IgMetadata? meta = null;

        try
        {
            var settings = ParseSettings(options, filePath);
            using var imgC = new MagickImageCollection();

            if (filePath.Length > 260)
            {
                var newFilename = Helpers.PrefixLongPath(filePath);
                var allBytes = File.ReadAllBytes(newFilename);

                imgC.Ping(allBytes, settings);
            }
            else
            {
                imgC.Ping(filePath, settings);
            }

            meta = new()
            {
                FramesCount = imgC.Count,
            };

            if (imgC.Count > 0)
            {
                using var imgM = imgC[0];
                var exifProfile = imgM.GetExifProfile();

                if (exifProfile != null)
                {
                    // ExifRatingPercent
                    meta.ExifRatingPercent = GetExifValue(exifProfile, ExifTag.RatingPercent);

                    // ExifDateTimeOriginal
                    var dt = GetExifValue(exifProfile, ExifTag.DateTimeOriginal);
                    meta.ExifDateTimeOriginal = Helpers.ConvertDateTime(dt);

                    // ExifDateTime
                    dt = GetExifValue(exifProfile, ExifTag.DateTime);
                    meta.ExifDateTime = Helpers.ConvertDateTime(dt);
                }

                meta.Width = imgM.Width;
                meta.Height = imgM.Height;
            }
        }
        catch { }

        return meta;
    }


    /// <summary>
    /// Loads image file.
    /// </summary>
    /// <param name="filePath">Full path of the file</param>
    /// <param name="options">Loading options</param>
    public static IgImgData Load(string filePath, CodecReadOptions? options = null)
    {
        options ??= new();

        var (loadSuccessful, result, ext, settings) = ReadWithStream(filePath, options);

        if (!loadSuccessful)
        {
            result = LoadWithMagickImage(filePath, ext, settings, options);
        }

        return result;
    }


    /// <summary>
    /// Loads image file async.
    /// </summary>
    /// <param name="filePath">Full path of the file</param>
    /// <param name="options">Loading options</param>
    /// <param name="token">Cancellation token</param>
    public static async Task<IgImgData> LoadAsync(string filePath,
        CodecReadOptions? options = null,
        CancellationToken? token = null)
    {
        options ??= new();
        var cancelToken = token ?? default;

        try
        {
            var (loadSuccessful, result, ext, settings) = ReadWithStream(filePath, options);

            if (!loadSuccessful)
            {
                result = await LoadWithMagickImageAsync(filePath, ext, settings, options, cancelToken);
            }

            return result;
        }
        catch (OperationCanceledException) { }

        return new IgImgData();
    }


    /// <summary>
    /// Gets thumbnail from image.
    /// </summary>
    public static Bitmap? GetThumbnail(string filePath, int width, int height)
    {
        var data = Load(filePath, new()
        {
            Width = width,
            Height = height,
            FirstFrameOnly = true,
            ImageChannel = ColorChannel.All,
            UseEmbeddedThumbnail = true,
            UseRawThumbnail = true,
            ApplyColorProfileForAll = false,
            Quality = 80,
        });

        return data.Image;
    }


    /// <summary>
    /// Gets base64 thumbnail from image
    /// </summary>
    public static string GetThumbnailBase64(string filePath, int width, int height)
    {
        var thumbnail = GetThumbnail(filePath, width, height);

        if (thumbnail != null)
        {
            using var imgM = new MagickImage();
            imgM.Read(thumbnail);

            return "data:image/png;base64," + imgM.ToBase64(MagickFormat.Png);
        }

        return string.Empty;
    }


    /// <summary>
    /// Converts base64 string to Bitmap.
    /// </summary>
    /// <param name="content">Base64 string</param>
    /// <returns></returns>
    public static Bitmap? Base64ToBitmap(string content)
    {
        var (MimeType, ByteData) = Helpers.ConvertBase64ToBytes(content);
        if (string.IsNullOrEmpty(MimeType)) return null;

        // supported MIME types:
        // https://www.iana.org/assignments/media-types/media-types.xhtml#image
        #region Settings
        var settings = new MagickReadSettings();

        switch (MimeType)
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
                bmp = new Bitmap(new MemoryStream(ByteData)
                {
                    Position = 0
                }, true);

                break;

            default:
                using (var imgM = new MagickImage(ByteData, settings))
                {
                    bmp = imgM.ToBitmap();
                }
                break;
        }

        return bmp;
    }


    /// <summary>
    /// Save as image file
    /// </summary>
    /// <param name="srcFileName">Source filename to save</param>
    /// <param name="destFilePath">Destination filename</param>
    /// <param name="format">New image format</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    public static async Task SaveAsync(string srcFileName, string destFilePath, MagickFormat format = MagickFormat.Unknown, int quality = 100, CancellationToken token = default)
    {
        using var imgM = new MagickImage(srcFileName)
        {
            Quality = quality,
        };

        try
        {
            if (format != MagickFormat.Unknown)
            {
                await imgM.WriteAsync(destFilePath, format, token);
            }
            else
            {
                await imgM.WriteAsync(destFilePath, token);
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Save as image file
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="format">New image format</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    /// <returns></returns>
    public static async Task SaveAsync(Bitmap? srcBitmap, string destFilePath, MagickFormat format = MagickFormat.Unknown, int quality = 100)
    {
        await Task.Run(() => Save(srcBitmap, destFilePath, format, quality));
    }


    /// <summary>
    /// Save as image file
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="format">New image format</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    public static void Save(Bitmap? srcBitmap, string destFilePath, MagickFormat format = MagickFormat.Unknown, int quality = 100)
    {
        if (srcBitmap == null) return;

        using var imgM = new MagickImage();
        imgM.Read(srcBitmap);
        imgM.Quality = quality;

        if (format != MagickFormat.Unknown)
        {
            imgM.Write(destFilePath, format);
        }
        else
        {
            imgM.Write(destFilePath);
        }
    }


    /// <summary>
    /// Save image pages to files
    /// </summary>
    /// <param name="filename">The full path of source file</param>
    /// <param name="destFolder">The destination folder to save to</param>
    public static async IAsyncEnumerable<int> SaveFramesAsync(string filename, string destFolder, [EnumeratorCancellation] CancellationToken token = default)
    {
        // create dirs unless it does not exist
        Directory.CreateDirectory(destFolder);

        using var imgColl = new MagickImageCollection(filename);
        var index = 0;

        foreach (var imgM in imgColl)
        {
            index++;
            imgM.Quality = 100;

            try
            {
                var newFilename = Path.GetFileNameWithoutExtension(filename)
                    + " - " + index.ToString($"D{imgColl.Count.ToString().Length}")
                    + ".png";
                var destFilePath = Path.Combine(destFolder, newFilename);

                await imgM.WriteAsync(destFilePath, MagickFormat.Png, token);
            }
            catch (OperationCanceledException) { break; }
            catch { }

            yield return index;
        }
    }

    #endregion



    #region Private functions

    /// <summary>
    /// Read image file using stream
    /// </summary>
    private static (bool loadSuccessful, IgImgData result, string ext, MagickReadSettings settings) ReadWithStream(string filename, CodecReadOptions? options = null)
    {
        options ??= new();
        var loadSuccessful = true;
        var result = new IgImgData();

        if (options.Metadata == null)
        {
            options.Metadata = LoadMetadata(filename, options);
        }

        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var settings = ParseSettings(options, filename);


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

                result.Image = Base64ToBitmap(base64Content);
                result.FrameCount = options.Metadata?.FramesCount ?? 0;

                break;

            case ".GIF":
            case ".FAX":
                try
                {
                    // Note: Using FileStream is much faster than using MagickImageCollection
                    result.Image = ConvertFileToBitmap(filename);
                    result.FrameCount = options.Metadata?.FramesCount ?? 0;
                }
                catch
                {
                    loadSuccessful = false;
                }
                break;

            default:
                loadSuccessful = false;

                break;
        }
        #endregion


        // apply size setting
        if (result.Image != null && options.Width > 0 && options.Height > 0)
        {
            if (result.Image.Width > options.Width || result.Image.Height > options.Height)
            {
                using var imgM = new MagickImage();
                imgM.Read(result.Image);

                ApplySizeSettings(imgM, options);

                result.Image = imgM.ToBitmap();
            }
        }


        return (loadSuccessful, result, ext, settings);
    }


    /// <summary>
    /// Loads image file with Magick.NET
    /// </summary>
    private static async Task<IgImgData> LoadWithMagickImageAsync(string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options, CancellationToken cancelToken)
    {
        var data = await ReadMagickImageAsync(filename, ext, settings, options, cancelToken);
        var result = new IgImgData(data);

        return result;
    }


    /// <summary>
    /// Loads image file with Magick.NET
    /// </summary>
    private static IgImgData LoadWithMagickImage(string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options)
    {
        var data = ReadMagickImage(filename, ext, settings, options);
        var result = new IgImgData(data);

        return result;
    }


    /// <summary>
    /// Reads and processes image file with Magick.NET
    /// </summary>
    private static async Task<IgMagickReadData> ReadMagickImageAsync(
        string filename, string ext, MagickReadSettings settings, CodecReadOptions options, CancellationToken cancelToken)
    {
        var result = new IgMagickReadData() { Extension = ext };
        var imgColl = new MagickImageCollection();

        // Issue #530: ImageMagick falls over if the file path is longer than the (old)
        // windows limit of 260 characters. Workaround is to read the file bytes,
        // but that requires using the "long path name" prefix to succeed.
        if (filename.Length > 260)
        {
            var newFilename = Helpers.PrefixLongPath(filename);
            var allBytes = File.ReadAllBytes(newFilename);

            imgColl.Ping(allBytes, settings);
        }
        else
        {
            imgColl.Ping(filename, settings);
        }

        // standardize first frame reading option
        result.FrameCount = imgColl.Count;
        bool readFirstFrameOnly;

        if (options.FirstFrameOnly == null)
        {
            readFirstFrameOnly = imgColl.Count < 2;
        }
        else
        {
            readFirstFrameOnly = options.FirstFrameOnly.Value;
        }


        // read all frames
        if (imgColl.Count > 1 && readFirstFrameOnly is false)
        {
            await imgColl.ReadAsync(filename, settings, cancelToken);

            foreach (var imgPageM in imgColl)
            {
                ProcessMagickImage((MagickImage)imgPageM, options, ext, false);
            }

            result.MultiFrameImage = imgColl;
            return result;
        }


        // read a single frame only
        var imgM = new MagickImage();
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


        // process image
        var processResult = ProcessMagickImage(imgM, options, ext, true);

        if (processResult.ThumbM != null)
        {
            imgM = processResult.ThumbM;
        }


        // apply color channel
        imgM = ApplyColorChannel(imgM, options);


        result.SingleFrameImage = imgM;
        result.ColorProfile = processResult.ColorProfile;
        result.ExifProfile = processResult.ExifProfile;


        imgColl.Dispose();

        return result;
    }


    /// <summary>
    /// Loads and processes image file with Magick.NET
    /// </summary>
    private static IgMagickReadData ReadMagickImage(string filename, string ext, MagickReadSettings settings, CodecReadOptions options)
    {
        var result = new IgMagickReadData() { Extension = ext };
        var imgColl = new MagickImageCollection();

        // Issue #530: ImageMagick falls over if the file path is longer than the (old)
        // windows limit of 260 characters. Workaround is to read the file bytes,
        // but that requires using the "long path name" prefix to succeed.
        if (filename.Length > 260)
        {
            var newFilename = Helpers.PrefixLongPath(filename);
            var allBytes = File.ReadAllBytes(newFilename);

            imgColl.Ping(allBytes, settings);
        }
        else
        {
            imgColl.Ping(filename, settings);
        }

        // standardize first frame reading option
        result.FrameCount = imgColl.Count;
        bool readFirstFrameOnly;

        if (options.FirstFrameOnly == null)
        {
            readFirstFrameOnly = imgColl.Count < 2;
        }
        else
        {
            readFirstFrameOnly = options.FirstFrameOnly.Value;
        }


        // read all frames
        if (imgColl.Count > 1 && readFirstFrameOnly is false)
        {
            imgColl.Read(filename, settings);

            foreach (var imgPageM in imgColl)
            {
                ProcessMagickImage((MagickImage)imgPageM, options, ext, false);
            }

            result.MultiFrameImage = imgColl;
            return result;
        }


        // read a single frame only
        var imgM = new MagickImage();
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
                    imgM.Read(filename, settings);
                }
            }
            catch
            {
                imgM.Read(filename, settings);
            }
        }
        else
        {
            imgM.Read(filename, settings);
        }


        // process image
        var processResult = ProcessMagickImage(imgM, options, ext, true);

        if (processResult.ThumbM != null)
        {
            imgM = processResult.ThumbM;
        }


        // apply color channel
        imgM = ApplyColorChannel(imgM, options);


        result.SingleFrameImage = imgM;
        result.ColorProfile = processResult.ColorProfile;
        result.ExifProfile = processResult.ExifProfile;

        imgColl.Dispose();

        return result;
    }


    /// <summary>
    /// Processes single-frame Magick image
    /// </summary>
    /// <param name="refImgM">Input Magick image to process</param>
    /// <returns></returns>
    private static (IColorProfile? ColorProfile, IExifProfile? ExifProfile, MagickImage? ThumbM) ProcessMagickImage(MagickImage refImgM, CodecReadOptions options, string ext, bool requestThumbnail)
    {
        IColorProfile? colorProfile = null;
        IExifProfile? exifProfile = null;
        IMagickImage? thumbM = null;

        // preprocess image, read embedded thumbnail if any
        refImgM.Quality = options.Quality;

        try
        {
            // get the color profile of image
            colorProfile = refImgM.GetColorProfile();

            // Get Exif information
            exifProfile = refImgM.GetExifProfile();
        }
        catch { }

        // Use embedded thumbnails if specified
        if (requestThumbnail && exifProfile != null && options.UseEmbeddedThumbnail)
        {
            // Fetch the embedded thumbnail
            thumbM = exifProfile.CreateThumbnail();
            if (thumbM != null)
            {
                ApplyRotation(thumbM, exifProfile, ext);
                ApplySizeSettings(thumbM, options);
            }
        }

        // Revert to source image if an embedded thumbnail with required size was not found.
        if (!requestThumbnail || thumbM == null)
        {
            // resize the image
            ApplySizeSettings(refImgM, options);

            ApplyRotation(refImgM, exifProfile, ext);

            // if always apply color profile
            // or only apply color profile if there is an embedded profile
            if (options.ApplyColorProfileForAll || colorProfile != null)
            {
                var imgColor = Helpers.GetColorProfile(options.ColorProfileName);

                if (imgColor != null)
                {
                    refImgM.TransformColorSpace(
                        //set default color profile to sRGB
                        colorProfile ?? ColorProfile.SRGB,
                        imgColor);
                }
            }
        }


        return (colorProfile, exifProfile, (MagickImage?)thumbM);
    }


    /// <summary>
    /// Applies color channel setting
    /// </summary>
    /// <returns></returns>
    private static MagickImage ApplyColorChannel(MagickImage imgM, CodecReadOptions options)
    {
        // apply color channel
        if (options.ImageChannel != ColorChannel.All)
        {
            return FilterColorChannel(imgM, options.ImageChannel);
        }

        return imgM;
    }


    /// <summary>
    /// Applies the size settings
    /// </summary>
    private static void ApplySizeSettings(IMagickImage imgM, CodecReadOptions options)
    {
        if (options.Width > 0 && options.Height > 0)
        {
            if (imgM.BaseWidth > options.Width || imgM.BaseHeight > options.Height)
            {
                imgM.Thumbnail(options.Width, options.Height);
            }
        }
    }


    /// <summary>
    /// Filter color channel of Magick image
    /// </summary>
    /// <param name="imgM"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    private static MagickImage FilterColorChannel(MagickImage imgM, ColorChannel channel)
    {
        if (channel == ColorChannel.All) return imgM;


        var magickChannel = (Channels)channel;
        var channelImgM = (MagickImage)imgM.Separate(magickChannel).First();

        if (imgM.HasAlpha && magickChannel != Channels.Alpha)
        {
            using var alpha = imgM.Separate(Channels.Alpha).First();
            channelImgM.Composite(alpha, CompositeOperator.CopyAlpha);
        }

        return channelImgM;
    }


    /// <summary>
    /// Applies rotation from EXIF profile
    /// </summary>
    /// <param name="imgM"></param>
    /// <param name="profile"></param>
    private static void ApplyRotation(IMagickImage imgM, IExifProfile? profile, string ext)
    {
        if (ext == ".HEIC" || ext == ".HEIF") return;

        if (ext == ".TGA")
        {
            imgM.AutoOrient();
            return;
        }

        if (profile == null) return;

        // Get Orientation Flag
        var exifRotationTag = profile.GetValue(ExifTag.Orientation);

        if (exifRotationTag != null)
        {
            if (int.TryParse(exifRotationTag.Value.ToString(), out var orientationFlag))
            {
                var orientationDegree = Helpers.GetOrientationDegree(orientationFlag);
                if (orientationDegree != 0)
                {
                    // Rotate image accordingly
                    imgM.Rotate(orientationDegree);
                }
            }
        }
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
    /// Parse <see cref="CodecReadOptions"/> to <see cref="MagickReadSettings"/>
    /// </summary>
    /// <param name="options"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    private static MagickReadSettings ParseSettings(CodecReadOptions? options, string filename = "")
    {
        options ??= new();

        var ext = Path.GetExtension(filename).ToUpperInvariant();
        var settings = new MagickReadSettings
        {
            // https://github.com/dlemstra/Magick.NET/issues/1077
            SyncImageWithExifProfile = true,
            SyncImageWithTiffProperties = true,
        };

        if (ext.Equals(".SVG", StringComparison.OrdinalIgnoreCase))
        {
            settings.BackgroundColor = MagickColors.Transparent;
            settings.SetDefine("svg:xml-parse-huge", "true");
        }
        else if (ext.Equals(".HEIC", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".HEIF", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new HeicReadDefines
            {
                PreserveOrientation = true,
                DepthImage = true,
            });
        }
        else if (ext.Equals(".JP2", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new Jp2ReadDefines
            {
                QualityLayers = options.Quality,
            });
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


    /// <summary>
    /// Get EXIF value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="profile"></param>
    /// <param name="tag"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private static T? GetExifValue<T>(IExifProfile? profile, ExifTag<T> tag, T? defaultValue = default)
    {
        if (profile == null) return default;

        var exifValue = profile.GetValue(tag);
        if (exifValue == null) return defaultValue;

        return exifValue.Value;
    }

    #endregion


}

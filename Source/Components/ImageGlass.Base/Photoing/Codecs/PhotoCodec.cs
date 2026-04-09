/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using DirectN;
using ImageGlass.WebP;
using ImageMagick;
using ImageMagick.Formats;
using PhotoSauce.MagicScaler;
using System.Runtime.CompilerServices;
using System.Text;
using WicNet;

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
    public static IgMetadata? LoadMetadata(string? filePath, CodecReadOptions? options = null)
    {
        FileInfo? fi = null;
        var meta = new IgMetadata() { FilePath = filePath ?? string.Empty };

        try
        {
            fi = new FileInfo(filePath);
        }
        catch { }
        if (fi == null) return meta;
        var ext = fi.Extension.ToUpperInvariant();

        meta.FileName = fi.Name;
        meta.FileExtension = ext;
        meta.FolderPath = fi.DirectoryName ?? string.Empty;
        meta.FolderName = Path.GetFileName(meta.FolderPath);

        meta.FileSize = fi.Length;
        meta.FileCreationTime = fi.CreationTime;
        meta.FileLastWriteTime = fi.LastWriteTime;
        meta.FileLastAccessTime = fi.LastAccessTime;

        try
        {
            var settings = ParseSettings(options, false, filePath);
            using var imgC = new MagickImageCollection();

            if (filePath.Length > 260)
            {
                var allBytes = File.ReadAllBytes(filePath);

                imgC.Ping(allBytes, settings);
            }
            else
            {
                imgC.Ping(filePath, settings);
            }

            meta.FrameIndex = 0;
            meta.FrameCount = imgC.Count;

            if (imgC.Count > 0)
            {
                var frameIndex = options?.FrameIndex ?? 0;

                // Check if frame index is greater than upper limit
                if (frameIndex >= imgC.Count)
                    frameIndex = 0;

                // Check if frame index is less than lower limit
                else if (frameIndex < 0)
                    frameIndex = imgC.Count - 1;

                meta.FrameIndex = (uint)frameIndex;
                using var imgM = imgC[frameIndex];


                // image size
                meta.OriginalWidth = imgM.BaseWidth;
                meta.OriginalHeight = imgM.BaseHeight;

                if (options?.AutoScaleDownLargeImage == true)
                {
                    var newSize = GetMaxImageRenderSize(imgM.BaseWidth, imgM.BaseHeight);

                    meta.RenderedWidth = (uint)newSize.Width;
                    meta.RenderedHeight = (uint)newSize.Height;
                }
                else
                {
                    meta.RenderedWidth = imgM.Width;
                    meta.RenderedHeight = imgM.Height;
                }


                // image color
                meta.HasAlpha = imgC.Any(i => i.HasAlpha);
                meta.ColorSpace = imgM.ColorSpace.ToString();
                meta.CanAnimate = CheckAnimatedFormat(imgC, ext);


                // EXIF profile
                if (imgM.GetExifProfile() is IExifProfile exifProfile)
                {
                    // ExifRatingPercent
                    meta.ExifRatingPercent = GetExifValue(exifProfile, ExifTag.RatingPercent);

                    // ExifDateTimeOriginal
                    var dt = GetExifValue(exifProfile, ExifTag.DateTimeOriginal);
                    meta.ExifDateTimeOriginal = BHelper.ConvertDateTime(dt);

                    // ExifDateTime
                    dt = GetExifValue(exifProfile, ExifTag.DateTime);
                    meta.ExifDateTime = BHelper.ConvertDateTime(dt);

                    meta.ExifArtist = GetExifValue(exifProfile, ExifTag.Artist);
                    meta.ExifCopyright = GetExifValue(exifProfile, ExifTag.Copyright);
                    meta.ExifSoftware = GetExifValue(exifProfile, ExifTag.Software);
                    meta.ExifImageDescription = GetExifValue(exifProfile, ExifTag.ImageDescription);
                    meta.ExifModel = GetExifValue(exifProfile, ExifTag.Model);
                    meta.ExifISOSpeed = (int?)GetExifValue(exifProfile, ExifTag.ISOSpeed);

                    var rational = GetExifValue(exifProfile, ExifTag.ExposureTime);
                    meta.ExifExposureTime = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;

                    rational = GetExifValue(exifProfile, ExifTag.FNumber);
                    meta.ExifFNumber = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;

                    rational = GetExifValue(exifProfile, ExifTag.FocalLength);
                    meta.ExifFocalLength = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;
                }
                else
                {
                    try
                    {
                        using var fs = File.OpenRead(filePath);
                        using var img = Image.FromStream(fs, false, false);
                        var enc = new ASCIIEncoding();

                        var EXIF_DateTimeOriginal = 0x9003; //36867
                        var EXIF_DateTime = 0x0132;

                        try
                        {
                            // get EXIF_DateTimeOriginal
                            var pi = img.GetPropertyItem(EXIF_DateTimeOriginal);
                            var dateTimeText = enc.GetString(pi.Value, 0, pi.Len - 1);

                            if (DateTime.TryParseExact(dateTimeText, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var exifDateTimeOriginal))
                            {
                                meta.ExifDateTimeOriginal = exifDateTimeOriginal;
                            }
                        }
                        catch { }


                        try
                        {
                            // get EXIF_DateTime
                            var pi = img.GetPropertyItem(EXIF_DateTime);
                            var dateTimeText = enc.GetString(pi.Value, 0, pi.Len - 1);

                            if (DateTime.TryParseExact(dateTimeText, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out var exifDateTime))
                            {
                                meta.ExifDateTime = exifDateTime;
                            }
                        }
                        catch { }
                    }
                    catch { }
                }


                // Color profile
                if (imgM.GetColorProfile() is IColorProfile colorProfile)
                {
                    meta.ColorProfile = colorProfile.ColorSpace.ToString();

                    if (!string.IsNullOrWhiteSpace(colorProfile.Description))
                    {
                        meta.ColorProfile = $"{colorProfile.Description} ({meta.ColorProfile})";
                    }
                }
            }
        }
        catch { }

        return meta;
    }


    /// <summary>
    /// Loads image file async.
    /// </summary>
    /// <param name="filePath">Full path of the file</param>
    /// <param name="options">Loading options</param>
    /// <param name="token">Cancellation token</param>
    public static async Task<IgImgData> LoadAsync(string filePath,
        CodecReadOptions? options = null, ImgTransform? transform = null,
        CancellationToken? token = null)
    {
        options ??= new();
        var cancelToken = token ?? default;

        try
        {
            var (loadSuccessful, result, ext, settings) = ReadWithStream(filePath, options, transform);

            if (!loadSuccessful)
            {
                result = await LoadWithMagickImageAsync(filePath, ext, settings, options, transform, cancelToken);
            }

            return result;
        }
        catch (OperationCanceledException) { }

        return new IgImgData();
    }


    /// <summary>
    /// Gets thumbnail from image.
    /// </summary>
    public static async Task<Bitmap?> GetThumbnailAsync(string filePath, uint width, uint height)
    {
        if (string.IsNullOrEmpty(filePath) || width == 0 || height == 0) return null;


        var options = new CodecReadOptions()
        {
            Width = width,
            Height = height,
            MinDimensionToUseWIC = 0,
            FirstFrameOnly = true,
            UseEmbeddedThumbnailRawFormats = true,
            UseEmbeddedThumbnailOtherFormats = true,
            ApplyColorProfileForAll = false,
        };
        var settings = ParseSettings(options, false, filePath);
        var ext = Path.GetExtension(filePath).ToLowerInvariant();


        var imgData = await ReadMagickImageAsync(filePath, ext, settings, options, null, new());

        if (imgData?.SingleFrameImage != null)
        {
            return imgData.SingleFrameImage.ToBitmap();
        }

        return null;
    }


    /// <summary>
    /// Gets thumbnail from image.
    /// </summary>
    public static Bitmap? GetThumbnail(string filePath, uint width, uint height)
    {
        return BHelper.RunSync(() => GetThumbnailAsync(filePath, width, height));
    }


    /// <summary>
    /// Gets embedded thumbnail.
    /// </summary>
    public static WicBitmapSource? GetEmbeddedThumbnail(string filePath, bool rawThumbnail = true, bool exifThumbnail = true, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(filePath)) return null;

        try
        {
            token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException) { return null; }

        var settings = ParseSettings(new() { FirstFrameOnly = true }, false, filePath);
        WicBitmapSource? result = null;

        using var imgM = new MagickImage();
        imgM.Ping(filePath, settings);


        // get RAW embedded thumbnail
        if (rawThumbnail)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // try to get thumbnail
                if (imgM.GetProfile("dng:thumbnail") is IImageProfile profile
                    && profile.ToReadOnlySpan() is ReadOnlySpan<byte> thumbnailData)
                {
                    imgM.Read(thumbnailData, settings);
                    imgM.AutoOrient();
                    result = BHelper.ToWicBitmapSource(imgM.ToBitmapSource(), imgM.HasAlpha);
                }
            }
            catch (OperationCanceledException) { return null; }
            catch { }
        }



        // Use JPEG embedded thumbnail
        if (exifThumbnail && result == null)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var exifProfile = imgM.GetExifProfile();

                // Fetch the embedded thumbnail
                using var thumbM = exifProfile?.CreateThumbnail();

                if (thumbM != null)
                {
                    thumbM.AutoOrient();
                    result = BHelper.ToWicBitmapSource(thumbM.ToBitmapSource(), thumbM.HasAlpha);
                }
            }
            catch (OperationCanceledException) { return null; }
            catch { }
        }

        return result;
    }


    /// <summary>
    /// Gets base64 thumbnail from image
    /// </summary>
    public static string GetThumbnailBase64(string filePath, uint width, uint height)
    {
        var thumbnail = GetThumbnail(filePath, width, height);

        if (thumbnail != null)
        {
            using var imgM = new MagickImage();
            imgM.Read(thumbnail);

            return "data:image/png;charset=utf-8;base64," + imgM.ToBase64(MagickFormat.Png);
        }

        return string.Empty;
    }


    /// <summary>
    /// Reads and processes the SVG file, replaces <c>#000</c> or <c>#fff</c>
    /// by the corresponding hex color value of the <paramref name="darkMode"/>.
    /// </summary>
    public static async Task<MagickImage?> ReadSvgWithMagickAsync(string svgFilePath, bool? darkMode, uint? width, uint? height, CancellationToken token = default)
    {
        // set up Magick settings
        var settings = ParseSettings(new CodecReadOptions()
        {
            Width = width ?? 0,
            Height = height ?? 0,
        }, false, svgFilePath);

        var imgM = new MagickImage();


        // change SVG icon color if requested
        if (darkMode != null)
        {
            // preprocess SVG content
            using var fs = new StreamReader(svgFilePath);
            var svg = await fs.ReadToEndAsync(token);

            if (darkMode.Value)
            {
                svg = svg.Replace("#000", "#fff");
            }
            else
            {
                svg = svg.Replace("#fff", "#000");
            }

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(svg));
            imgM.Read(ms, settings);
        }
        else
        {
            await imgM.ReadAsync(svgFilePath, settings, token);
        }


        return imgM;
    }


    /// <summary>
    /// Checks if the format can be written.
    /// </summary>
    public static bool CheckSupportFormatForSaving(string destFilePath)
    {
        return MagickFormatInfo.Create(destFilePath).SupportsWriting;
    }


    /// <summary>
    /// Save as image file, use Magick.NET.
    /// </summary>
    /// <param name="srcFileName">Source filename to save</param>
    /// <param name="destFilePath">Destination filename</param>
    /// <param name="readOptions">Options for reading image file</param>
    /// <param name="transform">Changes for writing image file</param>
    /// <param name="quality">Quality</param>
    /// <exception cref="FileFormatException"></exception>
    public static async Task SaveAsync(string srcFileName, string destFilePath, CodecReadOptions readOptions, ImgTransform? transform = null, uint quality = 100, CancellationToken token = default)
    {
        var ext = Path.GetExtension(destFilePath).ToUpperInvariant();

        try
        {
            if (!CheckSupportFormatForSaving(destFilePath))
            {
                throw new FileFormatException("IGE_001: Unsupported image format.");
            }

            var settings = ParseSettings(readOptions, true, srcFileName);


            using var imgData = await ReadMagickImageAsync(
                srcFileName,
                Path.GetExtension(srcFileName),
                settings,
                readOptions with
                {
                    // Magick.NET auto-corrects the rotation when saving,
                    // so we don't need to correct it manually.
                    CorrectRotation = false,
                }, transform, token);

            if (imgData.MultiFrameImage != null)
            {
                await imgData.MultiFrameImage.WriteAsync(destFilePath, token);
            }
            else if (imgData.SingleFrameImage != null)
            {
                imgData.SingleFrameImage.Quality = quality;

                // resize ICO file if it's larger than 256
                if (ext == ".ICO")
                {
                    var imgW = imgData.SingleFrameImage.Width;
                    var imgH = imgData.SingleFrameImage.Height;
                    const int MAX_ICON_SIZE = 256;

                    if (imgW > MAX_ICON_SIZE || imgH > MAX_ICON_SIZE)
                    {
                        var iconSize = GetMaxImageRenderSize(imgW, imgH, MAX_ICON_SIZE);
                        imgData.SingleFrameImage.Scale((uint)iconSize.Width, (uint)iconSize.Height);
                    }
                }

                await imgData.SingleFrameImage.WriteAsync(destFilePath, token);
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Save image file, use WIC if it supports, otherwise use Magick.NET.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="transform">Image transformation</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    /// <param name="format">New image format</param>
    public static async Task SaveAsync(WicBitmapSource? srcBitmap, string destFilePath, ImgTransform? transform = null, uint quality = 100, MagickFormat format = MagickFormat.Unknown, CancellationToken token = default)
    {
        if (srcBitmap == null) return;

        try
        {
            token.ThrowIfCancellationRequested();

            // transform image
            srcBitmap = TransformImage(srcBitmap, transform);

            // get WIC encoder for the dest format 
            var encoder = WicEncoder.FromFileExtension(Path.GetExtension(destFilePath));

            // if WIC supports this format
            if (encoder != null)
            {
                srcBitmap.Save(destFilePath, encoder.ContainerFormat);
            }
            // use Magick.NET to save
            else
            {
                // convert to Bitmap
                using var bitmap = BHelper.ToGdiPlusBitmap(srcBitmap);
                if (bitmap == null) return;

                // convert to MagickImage
                using var imgM = new MagickImage();
                await Task.Run(() =>
                {
                    imgM.Read(bitmap);
                    imgM.Quality = quality;
                }, token);


                // write image data to file
                token.ThrowIfCancellationRequested();
                if (format != MagickFormat.Unknown)
                {
                    await imgM.WriteAsync(destFilePath, format, token);
                }
                else
                {
                    await imgM.WriteAsync(destFilePath, token);
                }
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Exports image frames to files, using Magick.NET
    /// </summary>
    /// <param name="srcFilePath">The full path of source file</param>
    /// <param name="destFolder">The destination folder to save to</param>
    public static async IAsyncEnumerable<(int FrameNumber, string FileName)> SaveFramesAsync(string srcFilePath, string destFolder, [EnumeratorCancellation] CancellationToken token = default)
    {
        // create dirs unless it does not exist
        Directory.CreateDirectory(destFolder);

        using var imgColl = new MagickImageCollection(srcFilePath);
        var index = 0;

        foreach (var imgM in imgColl)
        {
            index++;
            imgM.Quality = 100;
            var newFilename = string.Empty;

            try
            {
                newFilename = Path.GetFileNameWithoutExtension(srcFilePath)
                    + " - " + index.ToString($"D{imgColl.Count.ToString().Length}")
                    + ".png";
                var destFilePath = Path.Combine(destFolder, newFilename);

                await imgM.WriteAsync(destFilePath, MagickFormat.Png, token);
            }
            catch (OperationCanceledException) { break; }
            catch { }

            yield return (index, newFilename);
        }
    }


    /// <summary>
    /// Saves source bitmap image as base64 using Stream.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap</param>
    /// <param name="srcExt">Source file extension, example: .png</param>
    /// <param name="destFilePath">Destination file</param>
    public static async Task SaveAsBase64Async(WicBitmapSource? srcBitmap, string srcExt, string destFilePath, ImgTransform? transform = null, CancellationToken token = default)
    {
        if (srcBitmap == null) return;

        var mimeType = BHelper.GetMIMEType(srcExt);
        var header = $"data:{mimeType};base64,";
        var srcFormat = BHelper.GetWicContainerFormatFromExtension(srcExt);

        try
        {
            token.ThrowIfCancellationRequested();
            srcBitmap = TransformImage(srcBitmap, transform);

            token.ThrowIfCancellationRequested();

            // convert bitmap to base64
            using var ms = new MemoryStream();
            srcBitmap.Save(ms, srcFormat);
            var base64 = Convert.ToBase64String(ms.ToArray());

            token.ThrowIfCancellationRequested();

            // write base64 file
            using var sw = new StreamWriter(destFilePath);
            await sw.WriteAsync(header + base64).ConfigureAwait(false);
            await sw.FlushAsync(token).ConfigureAwait(false);
            sw.Close();
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Saves image file as base64. Uses Magick.NET if <paramref name="transform"/>
    /// has changes. Uses Stream if the format is supported, else uses Magick.NET.
    /// </summary>
    /// <param name="srcFilePath">Source file path</param>
    /// <param name="destFilePath">Destination file path</param>
    public static async Task SaveAsBase64Async(string srcFilePath, string destFilePath, CodecReadOptions readOptions, ImgTransform? transform = null, CancellationToken token = default)
    {
        if (transform.HasChanges)
        {
            using var imgC = new MagickImageCollection();
            imgC.Ping(srcFilePath);

            if (imgC.Count == 1)
            {
                using var imgM = imgC[0];
                TransformImage(imgM, transform);

                using var wicSrc = BHelper.ToWicBitmapSource(imgM.ToBitmapSource(), imgM.HasAlpha);
                var ext = Path.GetExtension(srcFilePath);

                await SaveAsBase64Async(wicSrc, ext, destFilePath, null, token);
                return;
            }
        }


        var srcExt = Path.GetExtension(srcFilePath).ToLowerInvariant();
        var mimeType = BHelper.GetMIMEType(srcExt);

        try
        {
            token.ThrowIfCancellationRequested();

            // for basic MIME formats
            if (!string.IsNullOrEmpty(mimeType))
            {
                // read source file content
                using var fs = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read);
                var data = new byte[fs.Length];
                await fs.ReadExactlyAsync(data.AsMemory(0, (int)fs.Length), token);
                fs.Close();

                token.ThrowIfCancellationRequested();

                // convert bitmap to base64
                var header = $"data:{mimeType};base64,";
                var base64 = Convert.ToBase64String(data);

                token.ThrowIfCancellationRequested();

                // write base64 file
                using var sw = new StreamWriter(destFilePath);
                await sw.WriteAsync(header + base64);
                await sw.FlushAsync(token).ConfigureAwait(false);
                sw.Close();

                return;
            }


            // for not supported formats
            var bmp = await LoadAsync(srcFilePath, readOptions, transform, token);
            await SaveAsBase64Async(bmp.Image, srcExt, destFilePath, null, token);
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Applies changes from <see cref="ImgTransform"/>.
    /// </summary>
    public static WicBitmapSource? TransformImage(WicBitmapSource? bmpSrc, ImgTransform? transform)
    {
        if (bmpSrc == null || transform == null) return bmpSrc;

        // list of flips
        var flips = new List<WICBitmapTransformOptions>();
        if (transform.Flips.HasFlag(FlipOptions.Horizontal))
        {
            flips.Add(WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
        }
        if (transform.Flips.HasFlag(FlipOptions.Vertical))
        {
            flips.Add(WICBitmapTransformOptions.WICBitmapTransformFlipVertical);
        }


        // apply flips
        foreach (var flip in flips)
        {
            bmpSrc.FlipRotate(flip);
        }


        // rotate
        var rotate = transform.Rotation switch
        {
            90 => WICBitmapTransformOptions.WICBitmapTransformRotate90,
            -270 => WICBitmapTransformOptions.WICBitmapTransformRotate90,

            -90 => WICBitmapTransformOptions.WICBitmapTransformRotate270,
            270 => WICBitmapTransformOptions.WICBitmapTransformRotate270,

            180 => WICBitmapTransformOptions.WICBitmapTransformRotate180,
            -180 => WICBitmapTransformOptions.WICBitmapTransformRotate180,

            _ => WICBitmapTransformOptions.WICBitmapTransformRotate0,
        };

        if (rotate != WICBitmapTransformOptions.WICBitmapTransformRotate0)
        {
            bmpSrc.FlipRotate(rotate);
        }


        // invert color
        if (transform.IsColorInverted)
        {
            var newBmp = new WicBitmapSource(
                bmpSrc.Width, bmpSrc.Height,
                WicPixelFormat.GUID_WICPixelFormat32bppPRGBA);

            using var dc = newBmp.CreateDeviceContext();
            using var effect = dc.CreateEffect(Direct2DEffects.CLSID_D2D1Invert);

            using var cb = dc.CreateBitmapFromWicBitmap(bmpSrc.ComObject);
            {
                effect.SetInput(cb, 0);
                dc.BeginDraw();
                dc.DrawImage(effect);
                dc.EndDraw();
            }

            bmpSrc.Dispose();
            bmpSrc = newBmp;
        }

        return bmpSrc;
    }


    /// <summary>
    /// Initialize Magick.NET.
    /// </summary>
    public static void InitMagickNET()
    {
        OpenCL.IsEnabled = true;
        ResourceLimits.LimitMemory(new Percentage(75));
    }


    /// <summary>
    /// Checks if the supplied file name is supported for lossless compression using Magick.NET.
    /// </summary>
    public static bool IsLosslessCompressSupported(string? filePath)
    {
        var opt = new ImageOptimizer()
        {
            OptimalCompression = true,
        };

        return opt.IsSupported(filePath);
    }

    /// <summary>
    /// Performs lossless compression on the specified file using Magick.NET.
    /// If the new file size is not smaller, the file won't be overwritten.
    /// </summary>
    /// <returns>True when the image could be compressed otherwise false.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static bool LosslessCompress(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return false;

        var fi = new FileInfo(filePath);
        var opt = new ImageOptimizer()
        {
            OptimalCompression = true,
        };

        // check if the format is supported
        if (!opt.IsSupported(fi)) throw new NotSupportedException("IGE_002: Unsupported image format.");

        return opt.LosslessCompress(fi);
    }

    #endregion // Public functions



    #region Private functions

    /// <summary>
    /// Checks if the image data is animated format.
    /// </summary>
    /// <param name="imgC"></param>
    /// <param name="ext">File extension, e.g: <c>.gif</c></param>
    private static bool CheckAnimatedFormat(MagickImageCollection imgC, string? ext)
    {
        var isAnimatedExtension = ext == ".GIF" || ext == ".GIFV" || ext == ".WEBP" || ext == ".JXL";

        var canAnimate = imgC.Count > 1
            && (isAnimatedExtension || imgC.Any(i => i.GifDisposeMethod != GifDisposeMethod.Undefined));

        return canAnimate;
    }


    /// <summary>
    /// Read image file using stream
    /// </summary>
    private static (bool loadSuccessful, IgImgData result, string ext, MagickReadSettings settings) ReadWithStream(string filePath, CodecReadOptions? options = null, ImgTransform? transform = null, IgMetadata? metadata = null)
    {
        options ??= new();
        var loadSuccessful = true;

        metadata ??= LoadMetadata(filePath, options);
        var ext = Path.GetExtension(filePath).ToUpperInvariant();
        var settings = ParseSettings(options, false, filePath);

        var result = new IgImgData()
        {
            FrameCount = metadata?.FrameCount ?? 0,
            HasAlpha = metadata?.HasAlpha ?? false,
            CanAnimate = metadata?.CanAnimate ?? false,
        };


        #region Read image data
        switch (ext)
        {
            case ".TXT": // base64 string
            case ".B64":
                var base64Content = string.Empty;
                using (var fs = new StreamReader(filePath))
                {
                    base64Content = fs.ReadToEnd();
                }

                if (result.CanAnimate && options?.FirstFrameOnly != true)
                {
                    result.Source = BHelper.ToGdiPlusBitmapFromBase64(base64Content);
                }
                else
                {
                    result.Image = BHelper.ToWicBitmapSource(base64Content);

                    if (result.FrameCount == 1)
                    {
                        result.Image = TransformImage(result.Image, transform);
                    }
                }
                break;

            case ".GIF":
            case ".GIFV":
            case ".FAX":
                try
                {
                    // Note: Using WIC is much faster than using MagickImageCollection
                    if (result.CanAnimate && options?.FirstFrameOnly != true)
                    {
                        result.Source = BHelper.ToGdiPlusBitmap(filePath);
                    }
                    // multiple frame (but not animating or FirstFrameOnly requested)
                    else if (result.FrameCount > 1 && options?.FirstFrameOnly != true)
                    {
                        result.Source = WicBitmapDecoder.Load(filePath);
                    }
                    // single frame or FirstFrameOnly requested
                    else
                    {
                        result.Image = WicBitmapSource.Load(filePath);
                    }
                }
                catch
                {
                    loadSuccessful = false;
                }
                break;

            case ".WEBP":
                try
                {
                    using var webp = new WebPWrapper();

                    if (result.CanAnimate && options?.FirstFrameOnly != true)
                    {
                        var aniWebP = webp.AnimLoad(filePath);
                        var frames = aniWebP.Select(frame =>
                        {
                            var duration = frame.Duration > 0 ? frame.Duration : 100;
                            return new AnimatedImgFrame(frame.Bitmap, (uint)duration);
                        });

                        result.Source = new AnimatedImg(frames, result.FrameCount);
                    }
                    else
                    {
                        using var webpBmp = webp.Load(filePath);
                        result.Image = BHelper.ToWicBitmapSource(webpBmp);
                    }
                }
                catch
                {
                    loadSuccessful = false;
                }
                break;

            case ".JXR":
            case ".HDP":
            case ".WDP":
                try
                {
                    var wic = WicBitmapSource.Load(filePath);

                    if (options.IgnoreColorProfile)
                    {
                        result.Image = wic;
                    }
                    else
                    {
                        var ms = BHelper.ToMemoryStream(wic);

                        var imgM = new MagickImage(ms);
                        var profiles = GetProfiles(imgM);
                        var thumbM = ProcessMagickImageAndReturnThumbnail(imgM, options, ext, true, profiles.ColorProfile, profiles.ExifProfile);

                        if (thumbM != null)
                        {
                            imgM?.Dispose();
                            imgM = thumbM;
                        }


                        // apply final changes
                        TransformImage(imgM, transform);
                        result.Image = BHelper.ToWicBitmapSource(imgM.ToBitmapSource());
                    }
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
        if (options.Width > 0 && options.Height > 0)
        {
            using var imgM = new MagickImage();

            if (result.Image != null)
            {
                if (result.Image.Width > options.Width || result.Image.Height > options.Height)
                {
                    imgM.Read(result.Image.CopyPixels(0, 0, result.Image.Width, result.Image.Height));

                    ApplySizeSettings(imgM, options);

                    result.Image = BHelper.ToWicBitmapSource(imgM.ToBitmapSource(), imgM.HasAlpha);
                }
            }
            else if (result.Source is Bitmap bmp)
            {
                if (bmp.Width > options.Width || bmp.Height > options.Height)
                {
                    imgM.Read(bmp);

                    ApplySizeSettings(imgM, options);

                    result.Source = imgM.ToBitmap();
                }
            }
        }


        return (loadSuccessful, result, ext, settings);
    }


    /// <summary>
    /// Loads image file with Magick.NET
    /// </summary>
    private static async Task<IgImgData> LoadWithMagickImageAsync(string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options, ImgTransform? transform, CancellationToken cancelToken)
    {
        var data = await ReadMagickImageAsync(filename, ext, settings, options, transform, cancelToken);
        var result = new IgImgData(data);

        return result;
    }


    /// <summary>
    /// Reads and processes image file with Magick.NET.
    /// </summary>
    private static async Task<IgMagickReadData> ReadMagickImageAsync(
        string filePath, string ext, MagickReadSettings settings, CodecReadOptions options,
        ImgTransform? transform, CancellationToken cancelToken)
    {
        var result = new IgMagickReadData() { Extension = ext };
        var imgColl = new MagickImageCollection();
        imgColl.Ping(filePath, settings);

        // standardize first frame reading option
        result.FrameCount = imgColl.Count;
        result.CanAnimate = CheckAnimatedFormat(imgColl, ext);
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
            await imgColl.ReadAsync(filePath, settings, cancelToken);

            var i = 0;
            foreach (var imgFrameM in imgColl)
            {
                using var imgThumbFrameM = ProcessMagickImageAndReturnThumbnail((MagickImage)imgFrameM, options, ext, false, null, null);

                // apply transformation
                if (i == transform?.FrameIndex || transform?.FrameIndex == -1)
                {
                    TransformImage(imgFrameM, transform);
                }

                i++;
            }

            result.MultiFrameImage = imgColl;
            return result;
        }


        // read a single frame only
        var imgM = new MagickImage();
        var hasRequestedThumbnail = false;

        // get image profiles
        var profiles = GetProfiles(imgColl[0]);
        result.ColorProfile = profiles.ColorProfile;
        result.ExifProfile = profiles.ExifProfile;


        if (options.UseEmbeddedThumbnailRawFormats is true)
        {
            var profile = imgColl[0].GetProfile("dng:thumbnail");

            try
            {
                // try to get thumbnail
                if (profile != null
                    && profile.ToByteArray() is byte[] thumbnailData)
                {
                    imgM.Ping(thumbnailData);

                    // check min size
                    if (imgM.Width > options.EmbeddedThumbnailMinWidth
                        && imgM.Height > options.EmbeddedThumbnailMinHeight)
                    {
                        imgM.Read(thumbnailData, settings);
                        hasRequestedThumbnail = true;
                    }
                }
            }
            catch { }
        }

        if (!hasRequestedThumbnail)
        {
            imgM.Dispose();
            imgM = null;

            // check if the image size is not huge
            var imgWidth = imgColl[0].BaseWidth;
            var imgHeight = imgColl[0].BaseHeight;
            var isHuge = imgWidth >= options.MinDimensionToUseWIC
                || imgHeight >= options.MinDimensionToUseWIC;
            var canOpenWithWIC = WicDecoder.SupportsFileExtensionForDecoding(ext);

            // use WIC to load huge image
            if (isHuge && canOpenWithWIC)
            {
                // we will open with WIC later
            }
            else
            {
                imgM = (MagickImage)await InitializeSingleMagickImageAsync(filePath,
                    imgWidth, imgHeight, settings, options, cancelToken);
            }
        }

        if (imgM != null)
        {
            // process image
            var thumbM = ProcessMagickImageAndReturnThumbnail(imgM, options, ext, true, result.ColorProfile, result.ExifProfile);
            if (thumbM != null)
            {
                imgM.Dispose();
                imgM = thumbM;
            }

            // apply final changes
            TransformImage(imgM, transform);

            result.SingleFrameImage = imgM;
        }
        // use WIC to load huge image
        else
        {
            var bmpSrc = WicBitmapSource.Load(filePath);

            // apply final changes
            TransformImage(bmpSrc, transform);

            result.SingleFrameSource = bmpSrc;
        }


        imgColl.Dispose();

        return result;
    }


    /// <summary>
    /// Initialize the single-frame <paramref name="imgM"/>,
    /// quickly resize the image to fit the <see cref="Const.MAX_IMAGE_DIMENSION"/>
    /// according to the <see cref="CodecReadOptions.AutoScaleDownLargeImage"/>.
    /// </summary>
    public static async Task<IMagickImage> InitializeSingleMagickImageAsync(
        string srcFilePath, uint srcWidth, uint srcHeight,
        MagickReadSettings settings, CodecReadOptions options, CancellationToken cancelToken)
    {
        var imgM = new MagickImage();

        // the image is larger than the supported dimension
        var isSizeTooLarge = srcWidth > Const.MAX_IMAGE_DIMENSION
            || srcHeight > Const.MAX_IMAGE_DIMENSION;

        if (!isSizeTooLarge || !options.AutoScaleDownLargeImage)
        {
            await imgM.ReadAsync(srcFilePath, settings, cancelToken);
            return imgM;
        }


        // try to use MagicScaler for better performance
        #region Resize with MagicScaler

        FileStream? fs = null;
        var tempFilePath = Path.Combine(Path.GetTempPath(),
            $"temp_{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}{Path.GetExtension(srcFilePath)}");

        try
        {
            var resizerSettings = new ProcessImageSettings()
            {
                ColorProfileMode = ColorProfileMode.Preserve,
                OrientationMode = OrientationMode.Preserve,
                ResizeMode = CropScaleMode.Contain,
                Width = Const.MAX_IMAGE_DIMENSION,
                Height = Const.MAX_IMAGE_DIMENSION,
                HybridMode = HybridScaleMode.Turbo,
            };


            fs = File.Open(tempFilePath, FileMode.Create);


            // perform resizing
            await Task.Run(() =>
            {
                _ = MagicImageProcessor.ProcessImage(srcFilePath, fs, resizerSettings);
            }, cancelToken);


            // reset stream position
            fs.Position = 0;
        }
        catch { }


        // successfully resized with MagicScaler
        if (fs != null)
        {
            await imgM.ReadAsync(fs, settings, cancelToken);
            await fs.DisposeAsync();

            // delete the temp file
            File.Delete(tempFilePath);

            return imgM;
        }
        #endregion // Resize with MagicScaler


        // switch to Magick.NET if MagicScaler does not support
        #region Resize with Magick.NET

        // load full image data
        await imgM.ReadAsync(srcFilePath, settings, cancelToken);

        var newSize = GetMaxImageRenderSize(imgM.BaseWidth, imgM.BaseHeight);
        imgM.Scale((uint)newSize.Width, (uint)newSize.Height);

        return imgM;

        #endregion // Resize with Magick.NET

    }


    /// <summary>
    /// Gets maximum image dimention.
    /// </summary>
    private static Size GetMaxImageRenderSize(uint srcWidth, uint srcHeight, uint maxSize = Const.MAX_IMAGE_DIMENSION)
    {
        var widthScale = 1f;
        var heightScale = 1f;

        if (srcWidth > maxSize)
        {
            widthScale = 1f * maxSize / srcWidth;
        }

        if (srcHeight > maxSize)
        {
            heightScale = 1f * maxSize / srcHeight;
        }

        var scale = Math.Min(widthScale, heightScale);
        var newW = (int)(srcWidth * scale);
        var newH = (int)(srcHeight * scale);

        return new Size(newW, newH);
    }


    /// <summary>
    /// Gets Color profile & Exif profile.
    /// </summary>
    public static (IColorProfile? ColorProfile, IExifProfile? ExifProfile) GetProfiles(IMagickImage imgM)
    {
        IColorProfile? colorProfile = null;
        IExifProfile? exifProfile = null;

        try
        {
            // get the color profile of image
            colorProfile = imgM.GetColorProfile();

            // Get Exif information
            exifProfile = imgM.GetExifProfile();
        }
        catch { }

        return (colorProfile, exifProfile);
    }


    /// <summary>
    /// Processes single-frame Magick image
    /// </summary>
    /// <param name="refImgM">Input Magick image to process</param>
    private static MagickImage? ProcessMagickImageAndReturnThumbnail(MagickImage refImgM,
        CodecReadOptions options, string ext, bool requestThumbnail,
        IColorProfile? colorProfile, IExifProfile? exifProfile)
    {
        IMagickImage? thumbM = null;

        // preprocess image, read embedded thumbnail if any
        refImgM.Quality = 100;

        // Use embedded thumbnails if specified
        if (requestThumbnail && exifProfile != null && options.UseEmbeddedThumbnailOtherFormats)
        {
            // Fetch the embedded thumbnail
            thumbM = exifProfile.CreateThumbnail();
            if (thumbM != null
                && thumbM.Width > options.EmbeddedThumbnailMinWidth
                && thumbM.Height > options.EmbeddedThumbnailMinHeight)
            {
                if (options.CorrectRotation) thumbM.AutoOrient();

                ApplySizeSettings(thumbM, options);
            }
            else
            {
                thumbM?.Dispose();
                thumbM = null;
            }
        }

        // Revert to source image if an embedded thumbnail with required size was not found.
        if (!requestThumbnail || thumbM == null)
        {
            // resize the image
            ApplySizeSettings(refImgM, options);

            // for HEIC/HEIF, PreserveOrientation must be false
            // see https://github.com/d2phap/ImageGlass/issues/1928
            if (options.CorrectRotation) refImgM.AutoOrient();

            // if always apply color profile
            // or only apply color profile if there is an embedded profile
            if (options.ApplyColorProfileForAll || colorProfile != null)
            {
                var imgColor = BHelper.GetColorProfile(options.ColorProfileName);

                if (imgColor != null)
                {
                    refImgM.TransformColorSpace(
                        //set default color profile to sRGB
                        colorProfile ?? ColorProfiles.SRGB,
                        imgColor);
                }
            }


            // make sure the output color space is not CMYK
            if (refImgM.ColorSpace == ColorSpace.CMYK)
            {
                refImgM.ColorSpace = ColorSpace.sRGB;
            }
        }


        return (MagickImage?)thumbM;
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
    /// Applies changes from <see cref="ImgTransform"/>.
    /// </summary>
    private static void TransformImage(IMagickImage imgM, ImgTransform? transform = null)
    {
        if (transform == null) return;

        // rotate
        if (transform.Rotation != 0)
        {
            imgM.Rotate(transform.Rotation);
        }

        // flip
        if (transform.Flips.HasFlag(FlipOptions.Horizontal))
        {
            imgM.Flop();
        }
        if (transform.Flips.HasFlag(FlipOptions.Vertical))
        {
            imgM.Flip();
        }

        // invert color
        if (transform.IsColorInverted)
        {
            imgM.Negate(Channels.RGB);
        }
    }


    /// <summary>
    /// Parse <see cref="CodecReadOptions"/> to <see cref="MagickReadSettings"/>
    /// </summary>
    private static MagickReadSettings ParseSettings(CodecReadOptions? options, bool writePurpose, string filename = "")
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
            settings.SetDefine("svg:xml-parse-huge", "true");
            settings.Format = MagickFormat.Rsvg;
            settings.BackgroundColor = MagickColors.Transparent;
        }
        else if (ext.Equals(".SVGZ", StringComparison.OrdinalIgnoreCase))
        {
            settings.Format = MagickFormat.Svgz;
            settings.BackgroundColor = MagickColors.Transparent;
        }
        else if (ext.Equals(".HEIC", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new HeicReadDefines()
            {
                MaxChildrenPerBox = 500,
            });
        }
        else if (ext.Equals(".JP2", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new Jp2ReadDefines
            {
                QualityLayers = 100,
            });
        }
        else if (ext.Equals(".TIF", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".TIFF", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new TiffReadDefines
            {
                IgnoreTags = [
                    // Issue https://github.com/d2phap/ImageGlass/issues/1454
                    "34022", // ColorTable
                    "34025", // ImageColorValue
                    "34026", // BackgroundColorValue

                    // Issue https://github.com/d2phap/ImageGlass/issues/1181
                    "32928",

                    // Issue https://github.com/d2phap/ImageGlass/issues/1583
                    "32932", // Wang Annotation
                    // Issue https://github.com/d2phap/ImageGlass/issues/1617
                    "34031", // TrapIndicator
                ],
            });
        }
        else if (ext.Equals(".APNG", StringComparison.OrdinalIgnoreCase))
        {
            settings.Format = MagickFormat.APng;
        }

        if (options.Width > 0 && options.Height > 0)
        {
            settings.Width = options.Width;
            settings.Height = options.Height;

            if (ext == ".JPG" || ext == ".JPEG" || ext == ".JPE" || ext == ".JFIF")
            {
                settings.SetDefines(new JpegReadDefines()
                {
                    Size = new MagickGeometry(options.Width, options.Height),
                });
            }
        }

        // Fixed #708: length and filesize do not match
        settings.SetDefines(new BmpReadDefines
        {
            IgnoreFileSize = true,
        });

        // Fix RAW color
        settings.SetDefines(new DngReadDefines()
        {
            UseCameraWhiteBalance = true,
            OutputColor = DngOutputColor.SRGB,
            ReadThumbnail = true,
        });



        if (writePurpose)
        {
            if (ext == ".TIF" || ext == ".TIFF")
            {
                settings.SetDefines(new TiffWriteDefines
                {
                    WriteLayers = true,
                    PreserveCompression = true,
                });
            }
            else if (ext == ".WEBP")
            {
                settings.SetDefines(new WebPWriteDefines
                {
                    Lossless = true,
                    ThreadLevel = true,
                    AlphaQuality = 100,
                });
            }
        }

        return settings;
    }


    /// <summary>
    /// Get EXIF value
    /// </summary>
    private static T? GetExifValue<T>(IExifProfile? profile, ExifTag<T> tag, T? defaultValue = default)
    {
        if (profile == null) return default;

        var exifValue = profile.GetValue(tag);
        if (exifValue == null) return defaultValue;

        return exifValue.Value;
    }


    #endregion // Private functions


}

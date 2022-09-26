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
using DirectN;
using ImageMagick;
using ImageMagick.Formats;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media.Media3D;
using WicNet;
using ColorProfile = ImageMagick.ColorProfile;

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
        FileInfo? fi = null;
        var meta = new IgMetadata() { FilePath = filePath };

        try
        {
            fi = new FileInfo(filePath);
        }
        catch { }
        if (fi == null) return meta;

        meta.FileName = fi.Name;
        meta.FileExtension = fi.Extension.ToUpperInvariant();
        meta.FolderPath = fi.DirectoryName ?? string.Empty;
        meta.FolderName = Path.GetFileName(meta.FolderPath);

        meta.FileSize = fi.Length;
        meta.FileCreationTime = fi.CreationTimeUtc;
        meta.FileLastWriteTime = fi.LastWriteTimeUtc;
        meta.FileLastAccessTime = fi.LastAccessTimeUtc;

        try
        {
            var settings = ParseSettings(options, filePath);
            using var imgC = new MagickImageCollection();

            if (filePath.Length > 260)
            {
                var newFilename = BHelper.PrefixLongPath(filePath);
                var allBytes = File.ReadAllBytes(newFilename);

                imgC.Ping(allBytes, settings);
            }
            else
            {
                imgC.Ping(filePath, settings);
            }

            meta.FramesCount = imgC.Count;

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

                meta.Width = imgM.Width;
                meta.Height = imgM.Height;

                meta.OriginalWidth = imgM.BaseWidth;
                meta.OriginalHeight = imgM.BaseHeight;

                meta.HasAlpha = imgC.Any(i => i.HasAlpha);
                meta.CanAnimate = imgC.Count > 1 && imgC.Any(i => i.AnimationDelay > 0);
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
    public static IgImgData Load(string filePath, CodecReadOptions? options = null, IgImgChanges? changes = null)
    {
        options ??= new();

        var (loadSuccessful, result, ext, settings) = ReadWithStream(filePath, options, changes);

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
        CodecReadOptions? options = null, IgImgChanges? changes = null,
        CancellationToken? token = null)
    {
        options ??= new();
        var cancelToken = token ?? default;

        try
        {
            var (loadSuccessful, result, ext, settings) = ReadWithStream(filePath, options, changes);

            if (!loadSuccessful)
            {
                result = await LoadWithMagickImageAsync(filePath, ext, settings, options, changes, cancelToken);
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
        if (string.IsNullOrEmpty(filePath)) return null;

        var options = new CodecReadOptions()
        {
            Width = width,
            Height = height,
            FirstFrameOnly = true,
            ImageChannel = ColorChannel.All,
            UseEmbeddedThumbnailRawFormats = true,
            UseEmbeddedThumbnailOtherFormats = true,
            ApplyColorProfileForAll = false,
        };
        var settings = ParseSettings(options, filePath);
        var ext = Path.GetExtension(filePath).ToLower();


        var imgData = ReadMagickImage(filePath, ext, settings, options);

        if (imgData?.SingleFrameImage != null)
        {
            return imgData.SingleFrameImage.ToBitmap();
        }

        return null;
    }


    /// <summary>
    /// Gets embedded thumbnail.
    /// </summary>
    /// <param name="filePath">Full path of image file</param>
    public static WicBitmapSource? GetEmbeddedThumbnail(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return null;
        

        var settings = ParseSettings(new() { FirstFrameOnly = true }, filePath);
        WicBitmapSource? result = null;

        using var imgM = new MagickImage();
        imgM.Ping(filePath, settings);

        
        // get RAW embedded thumbnail
        try
        {
            var profile = imgM.GetProfile("dng:thumbnail");

            // try to get thumbnail
            var thumbnailData = profile?.GetData();
            if (thumbnailData != null)
            {
                imgM.Read(thumbnailData, settings);
                result = BHelper.ToWicBitmapSource(imgM.ToBitmapSource());
            }
        }
        catch { }


        // Use JPEG embedded thumbnail
        try
        {
            var exifProfile = imgM.GetExifProfile();

            // Fetch the embedded thumbnail
            using var thumbM = exifProfile?.CreateThumbnail();
            if (thumbM != null)
            {
                var ext = Path.GetExtension(filePath).ToLower();
                ApplyRotation(thumbM, exifProfile, ext);

                result = BHelper.ToWicBitmapSource(thumbM.ToBitmapSource());
            }
        }
        catch { }


        return result;
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
    /// Save as image file, use Magick.NET.
    /// </summary>
    /// <param name="srcFileName">Source filename to save</param>
    /// <param name="destFilePath">Destination filename</param>
    /// <param name="readOptions">Options for reading image file</param>
    /// <param name="changes">Changes for writing image file</param>
    /// <param name="quality">Quality</param>
    public static async Task SaveAsync(string srcFileName, string destFilePath, CodecReadOptions readOptions, IgImgChanges? changes = null, int quality = 100, CancellationToken token = default)
    {
        changes ??= new();

        try
        {
            var settings = ParseSettings(readOptions, srcFileName);
            using var imgData = await ReadMagickImageAsync(srcFileName, Path.GetExtension(srcFileName), settings, readOptions with
            {
                // Magick.NET auto-corrects the rotation when saving,
                // so we don't need to correct it manually.
                CorrectRotation = false,
            }, changes, token);

            if (imgData.MultiFrameImage != null)
            {
                await imgData.MultiFrameImage.WriteAsync(destFilePath, token);
            }
            else if (imgData.SingleFrameImage != null)
            {
                imgData.SingleFrameImage.Quality = quality;

                await imgData.SingleFrameImage.WriteAsync(destFilePath, token);
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Save as image file, use WIC if the extension is supported, else use Magick.NET.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    public static async Task SaveAsync(WicBitmapSource? srcBitmap, string destFilePath, int quality = 100, CancellationToken token = default)
    {
        if (srcBitmap == null) return;


        try
        {
            token.ThrowIfCancellationRequested();

            var wicEncoder = WicEncoder.FromFileExtension(Path.GetExtension(destFilePath));
            if (wicEncoder == null)
            {
                // use Magick.NET for saving
                await SaveAsync(srcBitmap, destFilePath, format: MagickFormat.Unknown, quality: quality, token);

                return;
            }


            // set saving options for WIC
            var encoderOptions = new List<KeyValuePair<string, object>>();
            if (quality == 100)
            {
                encoderOptions.Add(new("Lossless", true));
            }
            else
            {
                encoderOptions.Add(new("ImageQuality", quality / 100f));
            }


            using var stream = new FileStream(destFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            await Task.Run(() => srcBitmap.Save(stream, wicEncoder.ContainerFormat, encoderOptions: encoderOptions), token);
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Save as image file, use Magick.NET.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="format">New image format</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    public static async Task SaveAsync(WicBitmapSource? srcBitmap, string destFilePath, MagickFormat format = MagickFormat.Unknown, int quality = 100, CancellationToken token = default)
    {
        if (srcBitmap == null) return;


        try
        {
            token.ThrowIfCancellationRequested();

            using var bitmap = BHelper.ToGdiPlusBitmap(srcBitmap);
            if (bitmap == null) return;

            using var imgM = new MagickImage();

            await Task.Run(() =>
            {
                imgM.Read(bitmap);
                imgM.Quality = quality;
            }, token);

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
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Save as image file, use Magick.NET.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap to save</param>
    /// <param name="destFilePath">Destination file path</param>
    /// <param name="format">New image format</param>
    /// <param name="quality">JPEG/MIFF/PNG compression level</param>
    public static void Save(WicBitmapSource? srcBitmap, string destFilePath, MagickFormat format = MagickFormat.Unknown, int quality = 100)
    {
        if (srcBitmap == null) return;

        using var imgM = new MagickImage();
        imgM.Read(srcBitmap.CopyPixels(0, 0, srcBitmap.Width, srcBitmap.Height));
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
    /// Save image pages to files, using Magick.NET
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


    /// <summary>
    /// Saves source bitmap image as base64 using Stream.
    /// </summary>
    /// <param name="srcBitmap">Source bitmap</param>
    /// <param name="srcExt">Source file extension, example: .png</param>
    /// <param name="destFilePath">Destination file</param>
    public static async Task SaveAsBase64Async(WicBitmapSource? srcBitmap, string srcExt, string destFilePath, CancellationToken token = default)
    {
        if (srcBitmap == null) return;

        var mimeType = BHelper.GetMIMEType(srcExt);
        var header = $"data:{mimeType};base64,";
        var srcFormat = BHelper.GetWicContainerFormatFromExtension(srcExt);

        try
        {
            token.ThrowIfCancellationRequested();

            // convert bitmap to base64
            using var ms = new MemoryStream();
            srcBitmap.Save(ms, srcFormat);
            var base64 = Convert.ToBase64String(ms.ToArray());

            token.ThrowIfCancellationRequested();

            // write base64 file
            using var sw = new StreamWriter(destFilePath);
            await sw.WriteAsync(header + base64).ConfigureAwait(false);
            await sw.FlushAsync().ConfigureAwait(false);
            sw.Close();
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Saves image file as base64. Uses Magick.NET if <paramref name="changes"/>
    /// has changes. Uses Stream if the format is supported, else uses Magick.NET.
    /// </summary>
    /// <param name="srcFilePath">Source file path</param>
    /// <param name="destFilePath">Destination file path</param>
    public static async Task SaveAsBase64Async(string srcFilePath, string destFilePath, CodecReadOptions readOptions, IgImgChanges? changes = null, CancellationToken token = default)
    {
        if (changes.HasChanges)
        {
            using var imgC = new MagickImageCollection();
            imgC.Ping(srcFilePath);

            if (imgC.Count == 1)
            {
                using var imgM = imgC[0];
                ApplyIgImgChanges(imgM, changes);

                using var wicSrc = BHelper.ToWicBitmapSource(imgM.ToBitmapSource());
                var ext = Path.GetExtension(srcFilePath);

                await SaveAsBase64Async(wicSrc, ext, destFilePath, token);
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
                await fs.ReadAsync(data.AsMemory(0, (int)fs.Length), token);
                fs.Close();

                token.ThrowIfCancellationRequested();

                // convert bitmap to base64
                var header = $"data:{mimeType};base64,";
                var base64 = Convert.ToBase64String(data);

                token.ThrowIfCancellationRequested();

                // write base64 file
                using var sw = new StreamWriter(destFilePath);
                await sw.WriteAsync(header + base64);
                await sw.FlushAsync().ConfigureAwait(false);
                sw.Close();

                return;
            }


            // for not supported formats
            var bmp = await LoadAsync(srcFilePath, readOptions, changes, token);
            await SaveAsBase64Async(bmp.Image, srcExt, destFilePath, token);
        }
        catch (OperationCanceledException) { }
    }



    /// <summary>
    /// Applies changes from <see cref="IgImgChanges"/>.
    /// </summary>
    public static void ApplyIgImgChanges(WicBitmapSource? bmpSrc, IgImgChanges? changes)
    {
        if (bmpSrc == null || changes == null) return;

        // list of flips
        var flips = new List<WICBitmapTransformOptions>();
        if (changes.Flips.HasFlag(FlipOptions.Horizontal))
        {
            flips.Add(WICBitmapTransformOptions.WICBitmapTransformFlipHorizontal);
        }
        if (changes.Flips.HasFlag(FlipOptions.Vertical))
        {
            flips.Add(WICBitmapTransformOptions.WICBitmapTransformFlipVertical);
        }


        // apply flips
        flips.ForEach(flip => bmpSrc.Rotate(flip));

        // rotate
        if (changes.Rotation != 0)
        {
            // TODO:
        }
    }


    #endregion



    #region Private functions

    /// <summary>
    /// Read image file using stream
    /// </summary>
    private static (bool loadSuccessful, IgImgData result, string ext, MagickReadSettings settings) ReadWithStream(string filenPath, CodecReadOptions? options = null, IgImgChanges? changes = null)
    {
        options ??= new();
        var loadSuccessful = true;
        
        var metadata = LoadMetadata(filenPath, options);
        var ext = Path.GetExtension(filenPath).ToUpperInvariant();
        var settings = ParseSettings(options, filenPath);

        var result = new IgImgData();
        result.FrameCount = metadata?.FramesCount ?? 0;
        result.HasAlpha = metadata?.HasAlpha ?? false;
        result.CanAnimate = metadata?.CanAnimate ?? false;

        #region Read image data
        switch (ext)
        {
            case ".TXT": // base64 string
            case ".B64":
                var base64Content = string.Empty;
                using (var fs = new StreamReader(filenPath))
                {
                    base64Content = fs.ReadToEnd();
                }

                if (result.CanAnimate)
                {
                    result.Bitmap = BHelper.ToGdiPlusBitmapFromBase64(base64Content);
                }
                else
                {
                    result.Image = BHelper.ToWicBitmapSource(base64Content);

                    if (result.FrameCount == 1)
                    {
                        ApplyIgImgChanges(result.Image, changes);
                    }
                }
                break;

            case ".GIF":
            case ".FAX":
                try
                {
                    // Note: Using FileStream is much faster than using MagickImageCollection
                    if (result.CanAnimate)
                    {
                        result.Bitmap = BHelper.ToGdiPlusBitmap(filenPath);
                    }
                    else
                    {
                        result.Image = WicBitmapSource.Load(filenPath);
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

                    result.Image = BHelper.ToWicBitmapSource(imgM.ToBitmapSource());
                }
            }
            else if (result.Bitmap != null)
            {
                if (result.Bitmap.Width > options.Width || result.Bitmap.Height > options.Height)
                {
                    imgM.Read(result.Bitmap);

                    ApplySizeSettings(imgM, options);

                    result.Bitmap = imgM.ToBitmap();
                }
            }
        }
        

        return (loadSuccessful, result, ext, settings);
    }


    /// <summary>
    /// Loads image file with Magick.NET
    /// </summary>
    private static async Task<IgImgData> LoadWithMagickImageAsync(string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options, IgImgChanges? changes, CancellationToken cancelToken)
    {
        var data = await ReadMagickImageAsync(filename, ext, settings, options, changes, cancelToken);
        var result = new IgImgData(data);

        return result;
    }


    /// <summary>
    /// Loads image file with Magick.NET
    /// </summary>
    private static IgImgData LoadWithMagickImage(string filename, string ext,
        MagickReadSettings settings, CodecReadOptions options, IgImgChanges? changes = null)
    {
        var data = ReadMagickImage(filename, ext, settings, options, changes);
        var result = new IgImgData(data);

        return result;
    }


    /// <summary>
    /// Reads and processes image file with Magick.NET
    /// </summary>
    private static async Task<IgMagickReadData> ReadMagickImageAsync(
        string filename, string ext, MagickReadSettings settings, CodecReadOptions options, IgImgChanges? changes, CancellationToken cancelToken)
    {
        var result = new IgMagickReadData() { Extension = ext };
        var imgColl = new MagickImageCollection();

        // Issue #530: ImageMagick falls over if the file path is longer than the (old)
        // windows limit of 260 characters. Workaround is to read the file bytes,
        // but that requires using the "long path name" prefix to succeed.
        if (filename.Length > 260)
        {
            var newFilename = BHelper.PrefixLongPath(filename);
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
        if (options.UseEmbeddedThumbnailRawFormats is true)
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

        // apply final changes
        ApplyIgImgChanges(imgM, changes);

        result.SingleFrameImage = imgM;
        result.ColorProfile = processResult.ColorProfile;
        result.ExifProfile = processResult.ExifProfile;


        imgColl.Dispose();

        return result;
    }


    /// <summary>
    /// Loads and processes image file with Magick.NET
    /// </summary>
    private static IgMagickReadData ReadMagickImage(string filename, string ext, MagickReadSettings settings, CodecReadOptions options, IgImgChanges? changes = null)
    {
        var result = new IgMagickReadData() { Extension = ext };
        var imgColl = new MagickImageCollection();

        // Issue #530: ImageMagick falls over if the file path is longer than the (old)
        // windows limit of 260 characters. Workaround is to read the file bytes,
        // but that requires using the "long path name" prefix to succeed.
        if (filename.Length > 260)
        {
            var newFilename = BHelper.PrefixLongPath(filename);
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
        if (options.UseEmbeddedThumbnailRawFormats is true)
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

        // apply final changes
        ApplyIgImgChanges(imgM, changes);


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
        refImgM.Quality = 100;

        try
        {
            // get the color profile of image
            colorProfile = refImgM.GetColorProfile();

            // Get Exif information
            exifProfile = refImgM.GetExifProfile();
        }
        catch { }

        // Use embedded thumbnails if specified
        if (requestThumbnail && exifProfile != null && options.UseEmbeddedThumbnailOtherFormats)
        {
            // Fetch the embedded thumbnail
            thumbM = exifProfile.CreateThumbnail();
            if (thumbM != null)
            {
                if (options.CorrectRotation) ApplyRotation(thumbM, exifProfile, ext);

                ApplySizeSettings(thumbM, options);
            }
        }

        // Revert to source image if an embedded thumbnail with required size was not found.
        if (!requestThumbnail || thumbM == null)
        {
            // resize the image
            ApplySizeSettings(refImgM, options);

            if (options.CorrectRotation) ApplyRotation(refImgM, exifProfile, ext);

            // if always apply color profile
            // or only apply color profile if there is an embedded profile
            if (options.ApplyColorProfileForAll || colorProfile != null)
            {
                var imgColor = BHelper.GetColorProfile(options.ColorProfileName);

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
                imgM.InterpolativeResize(options.Width, options.Height, PixelInterpolateMethod.Nearest);
            }
        }


        // the image is larger than the supported dimension
        var isSizeTooLarge = imgM.BaseWidth > Constants.MAX_IMAGE_DIMENSION || imgM.BaseHeight > Constants.MAX_IMAGE_DIMENSION;
        if (options.AutoScaleDownLargeImage && isSizeTooLarge)
        {
            var widthScale = 1f;
            var heightScale = 1f;

            if (imgM.BaseWidth > Constants.MAX_IMAGE_DIMENSION)
            {
                widthScale = 1f * Constants.MAX_IMAGE_DIMENSION / imgM.BaseWidth;
            }

            if (imgM.BaseHeight > Constants.MAX_IMAGE_DIMENSION)
            {
                heightScale = 1f * Constants.MAX_IMAGE_DIMENSION / imgM.BaseHeight;
            }

            var scale = Math.Min(widthScale, heightScale);
            var newW = (int)(imgM.BaseWidth * scale);
            var newH = (int)(imgM.BaseHeight * scale);

            imgM.InterpolativeResize(newW, newH, PixelInterpolateMethod.Nearest);
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
                var orientationDegree = BHelper.GetOrientationDegree(orientationFlag);
                if (orientationDegree != 0)
                {
                    // Rotate image accordingly
                    imgM.Rotate(orientationDegree);
                }
            }
        }
    }


    /// <summary>
    /// Applies changes from <see cref="IgImgChanges"/>.
    /// </summary>
    private static void ApplyIgImgChanges(IMagickImage imgM, IgImgChanges? changes = null)
    {
        if (changes == null) return;

        // flip
        if (changes.Flips.HasFlag(FlipOptions.Horizontal))
        {
            imgM.Flip();
        }
        if (changes.Flips.HasFlag(FlipOptions.Vertical))
        {
            imgM.Flop();
        }

        // rotate
        if (changes.Rotation != 0)
        {
            imgM.Rotate(changes.Rotation);
        }
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
                QualityLayers = 100,
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
    private static T? GetExifValue<T>(IExifProfile? profile, ExifTag<T> tag, T? defaultValue = default)
    {
        if (profile == null) return default;

        var exifValue = profile.GetValue(tag);
        if (exifValue == null) return defaultValue;

        return exifValue.Value;
    }


    #endregion


}

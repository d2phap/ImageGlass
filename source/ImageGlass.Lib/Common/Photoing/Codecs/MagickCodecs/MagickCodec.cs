/*
ImageGlass - A Fast, Seamless Photo Viewer
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
using Cysharp.Collections;
using ImageMagick;
using ImageMagick.Formats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

public static partial class MagickCodec
{
    /// <summary>
    /// Indicates whether <see cref="MagickCodec"/> is initialized or not.
    /// </summary>
    public static bool IsInitialized { get; private set; } = false;


    /// <summary>
    /// Initializes ImageMagick decoder with OpenCL support.
    /// </summary>
    public static void Initialize()
    {
        if (IsInitialized) return;

#if DEBUG
        MagickNET.SetLogEvents(LogEventTypes.Exception);
#endif

        try
        {
            if (!ImageMagick.OpenCL.IsEnabled)
            {
                // this can throw exception
                ImageMagick.OpenCL.IsEnabled = true;
            }
        }
        catch { }

        IsInitialized = true;
    }


    /// <summary>
    /// Parse <see cref="PhotoReadOptions"/> to <see cref="MagickReadSettings"/>.
    /// </summary>
    public static MagickReadSettings ParseSettings(PhotoReadOptions? options,
        bool writePurpose, string filePath = "")
    {
        options ??= new();
        var ext = Path.GetExtension(filePath).ToUpperInvariant();


        // 1. create base settings
        var settings = new MagickReadSettings
        {
            // https://github.com/dlemstra/Magick.NET/issues/1077
            SyncImageWithExifProfile = true,
            SyncImageWithTiffProperties = true,
        };


        // 2. check the requested frame to decode
        if (options.FrameIndex >= 0)
        {
            settings.FrameIndex = (uint)options.FrameIndex;
            settings.FrameCount = 1;
        }


        // 3. add settings for specific format
        if (ext.Equals(".SVG", StringComparison.OrdinalIgnoreCase))
        {
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
                MaxItems = 2000, // Issue https://github.com/d2phap/ImageGlass/issues/2354
            });
        }
        else if (ext.Equals(".JP2", StringComparison.OrdinalIgnoreCase))
        {
            settings.SetDefines(new Jp2ReadDefines
            {
                QualityLayers = 100,
            });
        }
        else if (ext.Equals(".TIF", StringComparison.Ordinal)
            || ext.Equals(".TIFF", StringComparison.Ordinal))
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
        else if (ext.Equals(".APNG", StringComparison.Ordinal))
        {
            settings.Format = MagickFormat.APng;
        }


        // 4. update requested size for JPEG formats
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


        // 5. Edge case fixes
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


        // 6. add settings for writing
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
    /// Loads photo metadata from file path.
    /// </summary>
    public static async Task<PhotoMetadata> LoadMetadataAsync(string? filePath,
        PhotoReadOptions? options = null,
        MagickReadSettings? readSettings = null,
        CancellationToken token = default)
    {
        filePath ??= string.Empty;
        var meta = new PhotoMetadata(filePath);

        // 0. get file info
        if (string.IsNullOrWhiteSpace(filePath)) return meta;

        // SVG: delegate to SvgCodec for metadata
        if (SvgCodec.IsSvgFile(filePath))
        {
            return await SvgCodec.LoadMetadataAsync(filePath, token);
        }

        // 1. parse Magick settings
        var settings = readSettings ?? ParseSettings(options, false, filePath);

        // always get all frames metadata
        settings.FrameIndex = null;
        settings.FrameCount = null;
        using var imgC = new MagickImageCollection();


        // 2. ping data
        try
        {
            imgC.Ping(filePath, settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(LoadMetadataAsync)}: {ex.Message}");

            // try to load metadata with native codec
            meta = await SkiaCodec.LoadMetadataAsync(filePath, options, token);
            return meta;
        }

        // 3. load metadata
        meta = await LoadMetadataAsync(meta, imgC, options, token);

        return meta;
    }


    /// <summary>
    /// Loads photo metadata from byte array.
    /// </summary>
    public static async Task<PhotoMetadata> LoadMetadataAsync(byte[]? bytes,
        PhotoReadOptions? options = null,
        MagickReadSettings? readSettings = null,
        CancellationToken token = default)
    {
        var meta = new PhotoMetadata();
        if (bytes is null || bytes.Length == 0) return meta;

        // 1. parse Magick settings
        var settings = readSettings ?? ParseSettings(options, false);

        // always get all frames metadata
        settings.FrameIndex = null;
        settings.FrameCount = null;
        using var imgC = new MagickImageCollection();


        // 2. ping data
        try
        {
            imgC.Ping(bytes, settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(LoadMetadataAsync)}: {ex.Message}");
        }

        // 3. load metadata
        meta = await LoadMetadataAsync(meta, imgC, options, token);

        return meta;
    }


    /// <summary>
    /// Loads photo metadata from Magick instance.
    /// </summary>
    public static async Task<PhotoMetadata> LoadMetadataAsync(PhotoMetadata meta,
        MagickImageCollection imgC,
        PhotoReadOptions? options = null,
        CancellationToken token = default)
    {
        if (imgC.Count == 0) return meta;


        // 1. calculate the specific frame index
        var frameIndex = options?.FrameIndex ?? 0;

        // make sure frame index is within range
        if (frameIndex >= imgC.Count) frameIndex = 0;
        else if (frameIndex < 0) frameIndex = imgC.Count - 1;

        meta.FrameCount = (uint)imgC.Count;


        var readingTask = Task.Run(() =>
        {
            // 2. read metadata of all frames
            try
            {
                // get animation frames
                var skiaFramesInfo = SkiaCodec.GetFramesMetadata(meta.FilePath);
                meta.CanAnimate = skiaFramesInfo?.Count > 0;
                meta.AnimationLoop = imgC[0].AnimationIterations;

                // get frame metadata
                meta.Frames = imgC.Select((frame, frameIndex) => new FrameMetadata()
                {
                    BackgroundColor = (MagickColor?)frame.BackgroundColor ?? MagickColors.Transparent,
                    Width = frame.Width,
                    Height = frame.Height,
                    X = frame.Page.X,
                    Y = frame.Page.Y,

                    Animation = skiaFramesInfo?.ElementAtOrDefault(frameIndex),
                }).ToImmutableList();
            }
            catch { }


            // 3. read metadata of a specific frame
            try
            {
                // image size
                meta.OriginalWidth = meta.Width = imgC[frameIndex].Page.Width;
                meta.OriginalHeight = meta.Height = imgC[frameIndex].Page.Height;
                meta.Orientation = ToSkiaOrientation(imgC[frameIndex].Orientation);

                // determine if the result dimensions are swapped (90°/270° rotations)
                var swapDims = meta.Orientation is SKEncodedOrigin.LeftTop
                    or SKEncodedOrigin.RightTop
                    or SKEncodedOrigin.RightBottom
                    or SKEncodedOrigin.LeftBottom;
                if (swapDims)
                {
                    // swap width and height
                    meta.Width = meta.OriginalHeight;
                    meta.Height = meta.OriginalWidth;
                }

                // DPI: Convert units to inch
                var density = imgC[frameIndex].Density;
                meta.DpiX = density.X * 2.54d;
                meta.DpiY = density.Y * 2.54d;

                // image color
                meta.HasAlpha = imgC.Any(i => i.HasAlpha);
                meta.ColorSpace = imgC[frameIndex].ColorSpace;

                // get RAW thumbnail
                meta.RawThumbnail = imgC[frameIndex].GetProfile("dng:thumbnail");
            }
            catch { }


            // cancel if requested
            if (token.IsCancellationRequested) return;


            // 4. read frame exif profile
            try
            {
                if (imgC[frameIndex].GetExifProfile() is IExifProfile exifProfile)
                {
                    meta.ExifProfile = exifProfile;

                    // ExifRatingPercent
                    meta.ExifRatingPercent = GetExifValue__(exifProfile, ExifTag.RatingPercent);

                    // ExifDateTimeOriginal
                    var dt = GetExifValue__(exifProfile, ExifTag.DateTimeOriginal);
                    meta.ExifDateTimeOriginal = BHelper.ConvertDateTime(dt);

                    // ExifDateTime
                    dt = GetExifValue__(exifProfile, ExifTag.DateTime);
                    meta.ExifDateTime = BHelper.ConvertDateTime(dt);

                    meta.ExifArtist = GetExifValue__(exifProfile, ExifTag.Artist);
                    meta.ExifCopyright = GetExifValue__(exifProfile, ExifTag.Copyright);
                    meta.ExifSoftware = GetExifValue__(exifProfile, ExifTag.Software);
                    meta.ExifImageDescription = GetExifValue__(exifProfile, ExifTag.ImageDescription);
                    meta.ExifModel = GetExifValue__(exifProfile, ExifTag.Model);
                    meta.ExifISOSpeed = (int?)GetExifValue__(exifProfile, ExifTag.ISOSpeed);

                    var rational = GetExifValue__(exifProfile, ExifTag.ExposureTime);
                    meta.ExifExposureTime = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;

                    rational = GetExifValue__(exifProfile, ExifTag.FNumber);
                    meta.ExifFNumber = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;

                    rational = GetExifValue__(exifProfile, ExifTag.FocalLength);
                    meta.ExifFocalLength = rational.Denominator == 0
                        ? null
                        : rational.Numerator / rational.Denominator;
                }
            }
            catch { }


            // cancel if requested
            if (token.IsCancellationRequested) return;


            // 5. read color profile
            try
            {
                // Color profile
                if (imgC[frameIndex].GetColorProfile() is IColorProfile colorProfile)
                {
                    meta.ColorSpace = colorProfile.ColorSpace;
                    if (string.IsNullOrEmpty(colorProfile.Description))
                    {
                        meta.ColorProfileName = colorProfile.ColorSpace.ToString();
                    }
                    else
                    {
                        meta.ColorProfileName = $"{colorProfile.Description} ({colorProfile.ColorSpace})";
                    }

                    meta.MagickColorProfile = colorProfile;
                    meta.SkiaColorSpace = SKColorSpace.CreateIcc(colorProfile.ToByteArray());
                }
            }
            catch { }


            // detect HDR transfer function, wide gamut, and bit depth
            DetectHdrInfo(meta, imgC[frameIndex]);


            // 6. detect motion/live photo
            if (!token.IsCancellationRequested)
            {
                var liveInfo = LivePhotoDetector.Detect(meta.FilePath);
                meta.EmbeddedVideoOffsetFromEnd = liveInfo.IsLivePhoto
                    ? liveInfo.EmbeddedVideoOffsetFromEnd
                    : 0;
            }

        }, token).ConfigureAwait(false);


        await readingTask;

        return meta;
    }


    /// <summary>
    /// Reads and processes image file with Magick.NET.
    /// </summary>
    public static async Task<MagickDecoderOutput> DecodeImageAsync(
        PhotoMetadata meta,
        PhotoReadOptions options, MagickReadSettings? settings,
        PhotoTransform? transform, CancellationToken cancelToken)
    {
        var result = new MagickDecoderOutput();


        // 0. parse settings, make sure the frame index is correct
        settings ??= ParseSettings(options, false, meta.FilePath);
        if (options.FrameIndex >= 0)
        {
            settings.FrameIndex = (uint)options.FrameIndex;
            settings.FrameCount = 1;
        }


        // 1. read all frames if requested
        if (options.FrameIndex < 0)
        {
            var imgColl = new MagickImageCollection();
            await imgColl.ReadAsync(meta.FilePath, settings, cancelToken);

            var i = 0;
            foreach (var imgFrameM in imgColl)
            {
                ProcessMagickImage__((MagickImage)imgFrameM, options, meta, false);

                // apply transformation
                if (i == transform?.FrameIndex || transform?.FrameIndex == -1)
                {
                    TransformImage__(imgFrameM, transform);
                }

                i++;
            }

            result.MultiFrames = imgColl;
            return result;
        }


        // 2. read a single frame only
        var imgM = new MagickImage();
        var hasRequestedThumbnail = false;


        // 2.1 read embedded thumbnail only
        if (options.OnlyLoadRawPreview is true)
        {
            try
            {
                // try to get thumbnail
                if (meta.RawThumbnail != null)
                {
                    var thumbSpan = meta.RawThumbnail.ToReadOnlySpan();

                    imgM.Dispose();
                    imgM.Ping(thumbSpan);

                    // check min size
                    if (imgM.Width > options.PreviewMinWidth
                        && imgM.Height > options.PreviewMinHeight)
                    {
                        imgM.Read(thumbSpan, settings);
                        hasRequestedThumbnail = true;
                    }
                }
            }
            catch { }
        }


        // 2.2 read full image data
        if (!hasRequestedThumbnail)
        {
            imgM.Dispose();
            await imgM.ReadAsync(meta.FilePath, settings, cancelToken);
        }


        // 2.3 process image
        var thumbM = ProcessMagickImage__(imgM, options, meta, true);
        if (thumbM != null) imgM = thumbM;


        // 2.4 apply final changes
        TransformImage__(imgM, transform);
        result.SingleFrame = imgM;

        return result;
    }


    /// <summary>
    /// Gets thumbnail from the given image path.
    /// </summary>
    public static async Task<byte[]?> QuickDecodeAsync(string filePath, MagickFormat outputFormat,
        double desiredWidth, double desiredHeight,
        double minSize = 0, double maxSize = double.PositiveInfinity,
        CancellationToken token = default)
    {
        using var imgM = await QuickDecodeAsync(filePath, desiredWidth, desiredHeight, minSize, maxSize, token);
        if (imgM is null) return null;

        return imgM.ToByteArray(outputFormat);
    }


    /// <summary>
    /// Gets thumbnail from the given image path.
    /// </summary>
    public static async Task<MagickImage?> QuickDecodeAsync(string filePath,
        double desiredWidth, double desiredHeight,
        double minSize = 0, double maxSize = double.PositiveInfinity,
        CancellationToken token = default)
    {
        var options = new PhotoReadOptions()
        {
            Width = (uint)desiredWidth,
            Height = (uint)desiredHeight,
        };
        var settings = ParseSettings(options, false, filePath);

        // ping a throwaway image: reading into a pinged instance double-frees on teardown
        try
        {
            using var probeM = new MagickImage();
            probeM.Ping(filePath, settings);

            // check the dimention constraint
            if (probeM.Width < minSize
                || probeM.Height < minSize
                || probeM.Width > maxSize
                || probeM.Height > maxSize) return null;
        }
        catch { return null; }

        if (token.IsCancellationRequested) return null;


        var imgM = new MagickImage();
        try
        {
            await imgM.ReadAsync(filePath, settings, token);
            token.ThrowIfCancellationRequested();

            return imgM;
        }
        catch
        {
            imgM.Dispose();
            return null;
        }
    }


    /// <summary>
    /// Decodes photo from base64 string.
    /// </summary>
    public static async Task<Photo?> DecodeBase64Async(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;


        // 1. convert base64 string to bytes
        var (MimeType, ByteData) = ConvertBase64ToBytes(base64);
        if (string.IsNullOrEmpty(MimeType)) return null;


        // 2. convert Mime type to Magick format
        // supported MIME types:
        // https://www.iana.org/assignments/media-types/media-types.xhtml#image
        var format = ConvertMimeTypeToMagickFormat__(MimeType);


        // 3. create settings
        SKImage? bmpSrc = null;
        var readSettings = new MagickReadSettings { Format = format };
        if (readSettings.Format == MagickFormat.Rsvg)
        {
            readSettings.BackgroundColor = MagickColors.Transparent;
        }


        // 4. load bitmap from bytes
        using (var imgM = new MagickImage(ByteData, readSettings))
        {
            bmpSrc = SkiaCodec.FromMagick(imgM);
        }


        // 5. wrap the bitmap as photo object.
        var meta = await LoadMetadataAsync(ByteData, null, readSettings);
        var photo = new Photo(bmpSrc, meta);

        return photo;
    }


    /// <summary>
    /// Converts base64 string to byte array, returns MIME type and raw data in byte array.
    /// </summary>
    public static (string MimeType, byte[] ByteData) ConvertBase64ToBytes(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentNullException(nameof(content));
        }

        // data:image/svg-xml;base64,xxxxxxxx
        // type is optional
        var base64DataUriRegex = CreateBase64DataUriRegex__();
        var match = base64DataUriRegex.Match(content);

        if (!match.Success)
        {
            throw new FormatException("The base64 content is invalid.");
        }


        var base64Data = match.Groups["data"].Value;
        var byteData = Convert.FromBase64String(base64Data);
        var mimeType = match.Groups["type"].Value.ToLowerInvariant();

        if (mimeType.Length == 0)
        {
            // use default PNG MIME type
            mimeType = "image/png";
        }

        return (mimeType, byteData);
    }


    /// <summary>
    /// Gets pointer of Magick image pixels.
    /// </summary>
    public static nint GetPixelsPointer(MagickImage? imgM, MagickFormat format = MagickFormat.Bgra,
        int x = 0, int y = 0, int? width = null, int? height = null)
    {
        if (imgM is null) return IntPtr.Zero;

        // convert to common format
        imgM.Format = format;
        imgM.Depth = 8;

        // get pointer of imgM pixels
        using var pixels = imgM.GetPixelsUnsafe();
        var w = width ?? (int)imgM.Width;
        var h = height ?? (int)imgM.Height;
        var imgMPtr = pixels.GetAreaPointer(x, y, (uint)w, (uint)h);

        return imgMPtr;
    }


    /// <summary>
    /// Checks if the format can be read.
    /// </summary>
    public static bool CanRead(string srcFilePath)
    {
        return MagickFormatInfo.Create(srcFilePath)?.SupportsReading ?? false;
    }


    /// <summary>
    /// Checks if the format can be written.
    /// </summary>
    public static bool CanWrite(string destFilePath)
    {
        return MagickFormatInfo.Create(destFilePath)?.SupportsWriting ?? false;
    }


    /// <summary>
    /// Save the photo to file.
    /// </summary>
    /// <param name="meta">Source metadata</param>
    /// <param name="destFilePath">Destination filename</param>
    /// <param name="options">Options for reading image file</param>
    /// <param name="transform">Changes for writing image file</param>
    /// <param name="quality">Quality</param>
    /// <exception cref="Exception"></exception>
    public static async Task SaveAsync(PhotoMetadata meta, string destFilePath, PhotoReadOptions options,
        PhotoTransform? transform = null, uint quality = 100, CancellationToken token = default)
    {
        var destExt = Path.GetExtension(destFilePath);

        try
        {
            // 1. check if format is supported
            if (!CanWrite(destFilePath))
            {
                throw new FormatException("IGE: Unsupported image format.");
            }


            // 2. read the photo
            var settings = ParseSettings(options, true, meta.FilePath);
            using var result = await DecodeImageAsync(meta, options with
            {
                // Magick.NET auto-corrects the rotation when saving,
                // so we don't need to correct it manually.
                CorrectRotation = false,
            }, settings, transform, token);


            // 3. save the photo to file
            if (result.MultiFrames is not null)
            {
                // convert GIF to non-GIF formats, we need to coalesce all frames
                if (meta.FileExtension.Equals(".gif", StringComparison.OrdinalIgnoreCase)
                    && !destExt.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    result.MultiFrames.Coalesce();
                }

                await result.MultiFrames.WriteAsync(destFilePath, token);
            }
            else if (result.SingleFrame is not null)
            {
                result.SingleFrame.Quality = ResolveHdrSafeQuality__(meta, destExt, quality);

                // resize ICO file if it's larger than 256
                if (destExt.Equals(".ICO", StringComparison.OrdinalIgnoreCase))
                {
                    var imgW = result.SingleFrame.Width;
                    var imgH = result.SingleFrame.Height;
                    const int MAX_ICON_SIZE = 256;

                    if (imgW > MAX_ICON_SIZE || imgH > MAX_ICON_SIZE)
                    {
                        var iconSize = GetMaxImageRenderSize__(imgW, imgH, MAX_ICON_SIZE);
                        result.SingleFrame.Scale((uint)iconSize.Width, (uint)iconSize.Height);
                    }
                }

                await result.SingleFrame.WriteAsync(destFilePath, token);
            }
        }
        catch (OperationCanceledException) { }
    }


    /// <summary>
    /// Keeps an HDR image out of an encoder path that would discard its color encoding.
    /// </summary>
    private static uint ResolveHdrSafeQuality__(PhotoMetadata meta, string destExt, uint quality)
    {
        if (!meta.IsHdr || quality >= 100) return quality;

        // Measured: below 100 the JXL writer drops uses_original_profile, replacing the source
        // profile with a synthesized sRGB one, and no jxl: define overrides it. Other formats keep it.
        return destExt.Equals(".JXL", StringComparison.OrdinalIgnoreCase) ? 100 : quality;
    }


    /// <summary>
    /// Exports image frames to files, using Magick.NET
    /// </summary>
    /// <param name="srcFilePath">The full path of source file</param>
    /// <param name="destFolder">The destination folder to save to</param>
    public static async IAsyncEnumerable<(int FrameNumber, int FrameCount, string FileName)> SaveFramesAsync(string srcFilePath, string destFolder, [EnumeratorCancellation] CancellationToken token = default)
    {
        // create dirs unless it does not exist
        Directory.CreateDirectory(destFolder);

        // create settings to decode all frames
        var settings = ParseSettings(new PhotoReadOptions()
        {
            FrameIndex = -1,
        }, false, srcFilePath);

        using var imgColl = new MagickImageCollection(srcFilePath, settings);
        var frameCount = imgColl.Count;
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

            yield return (index, frameCount, newFilename);
        }
    }


    /// <summary>
    /// Detects HDR transfer function, wide gamut, and bit depth from all available signals,
    /// and populates the corresponding fields on <see cref="PhotoMetadata"/>.
    /// Uses a layered approach: Magick Depth -> SKColorSpace -> CICP box -> extension heuristics.
    /// </summary>
    /// <param name="meta">Metadata with <c>SkiaColorSpace</c>, <c>FileExtension</c>, and <c>FilePath</c> already set.</param>
    /// <param name="img">Optional Magick image for access to <c>Depth</c> and <c>ColorSpace</c>.</param>
    public static void DetectHdrInfo(PhotoMetadata meta, IMagickImage? img = null)
    {
        // 1. bit depth from Magick (most accurate source)
        if (img is not null)
        {
            meta.BitsPerChannel = (int)img.Depth;
        }

        // 2. Gain map detection — must run FIRST before ICC/CICP scanning.
        //    Gain-map images (JPEG Ultra HDR, HEIC/AVIF gain maps, ISO 21496-1)
        //    carry PQ metadata for the gain map, not for the base layer.
        //    If we let ICC scanning run first, the PQ profile text triggers a
        //    false PQ classification and the SDR base layer gets over-exposed.
        {
            var hasGainMap = false;

            if (img is not null)
            {
                // Apple HDR gain map (embedded as named profile in HEIC/JPEG)
                if (img.GetProfile("apple_hdrgainmap") is not null)
                {
                    hasGainMap = true;
                }
                // XMP-based gain maps (ISO 21496-1, Adobe, Android Ultra HDR)
                else if (img.GetProfile("xmp") is { } xmpProfile)
                {
                    ReadOnlySpan<byte> xmp = xmpProfile.ToReadOnlySpan();
                    if (xmp.IndexOf("hdrgm:"u8) >= 0
                        || xmp.IndexOf("HDRGainMap"u8) >= 0
                        || xmp.IndexOf("GContainer:"u8) >= 0)
                    {
                        hasGainMap = true;
                    }
                }
            }

            // Fallback: scan raw file bytes when Magick image is not available
            if (!hasGainMap && !string.IsNullOrEmpty(meta.FilePath))
            {
                hasGainMap = DetectGainMap(meta.FilePath);
            }

            if (hasGainMap)
            {
                meta.HdrTransferFn = HdrTransferFunction.GainMap;
                return; // base layer is SDR — no further HDR classification needed
            }
        }

        // 3. SKColorSpace-based detection (works for parametric ICC profiles)
        if (meta.SkiaColorSpace is not null)
        {
            if (meta.SkiaColorSpace.GetNumericalTransferFunction(out var transferFn))
            {
                if (transferFn.Equals(SKColorSpaceTransferFn.Pq))
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (transferFn.Equals(SKColorSpaceTransferFn.Hlg))
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }
            }

            if (meta.SkiaColorSpace.ToColorSpaceXyz(out var gamut))
            {
                meta.IsWideGamut = !gamut.Equals(SKColorSpaceXyz.Srgb);
            }
        }

        // 3. ICC profile text scan (covers LUT-based ICC that Skia can't parse)
        //    Works with or without img — falls back to meta.MagickColorProfile
        if (!meta.IsHdr)
        {
            ReadOnlySpan<byte> icc = default;
            if (img is not null && img.GetProfile("icc") is { } iccProfile)
            {
                icc = iccProfile.ToReadOnlySpan();
            }
            else if (meta.MagickColorProfile is { } metaIcc)
            {
                icc = metaIcc.ToReadOnlySpan();
            }

            // 3a. the profile's own cicp tag, which is exact where the text scan below only guesses
            if (icc.Length > 0 && ParseCicpFromIcc(icc) is (var iccPrimaries, var iccTransfer))
            {
                if (iccTransfer == 16)
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (iccTransfer == 18)
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }

                if (!meta.IsWideGamut && iccPrimaries is 9 or 11 or 12)
                {
                    meta.IsWideGamut = true;
                }
            }

            if (icc.Length > 0)
            {
                // Match canonical PQ identifiers only — avoid bare "PQ" (2 bytes)
                // which false-positives on random ICC LUT table data.
                if (icc.IndexOf("ST 2084"u8) >= 0
                    || icc.IndexOf("ST2084"u8) >= 0
                    || icc.IndexOf("Perceptual Quantizer"u8) >= 0)
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (icc.IndexOf("Hybrid Log"u8) >= 0
                    || icc.IndexOf("Hybrid Log-Gamma"u8) >= 0)
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }

                if (!meta.IsWideGamut)
                {
                    if (icc.IndexOf("BT.2020"u8) >= 0
                        || icc.IndexOf("Rec. 2020"u8) >= 0
                        || icc.IndexOf("Rec.2020"u8) >= 0
                        || icc.IndexOf("Display P3"u8) >= 0)
                    {
                        meta.IsWideGamut = true;
                    }
                }
            }
        }

        // 4. CICP-based detection for ISOBMFF containers (AVIF, HEIF, HEIC, HIF)
        //    when ICC is absent or uses a LUT-based profile that Skia can't parse
        if (!meta.IsHdr
            && meta.FileExtension is ".avif" or ".heif" or ".heic" or ".hif")
        {
            if (ParseCicpFromIsobmff(meta.FilePath) is (var primaries, var transfer))
            {
                // CICP transfer_characteristics: 16 = PQ (ST 2084), 18 = HLG
                if (transfer == 16)
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (transfer == 18)
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }

                // CICP color_primaries: 9 = BT.2020, 12 = Display P3
                if (!meta.IsWideGamut && primaries is 9 or 12)
                {
                    meta.IsWideGamut = true;
                }
            }
        }

        // 5. PNG cICP chunk detection (PNG equivalent of ISOBMFF CICP)
        if (!meta.IsHdr && meta.FileExtension is ".png")
        {
            if (ParseCicpFromPng(meta.FilePath) is (var pngPrimaries, var pngTransfer))
            {
                if (pngTransfer == 16)
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (pngTransfer == 18)
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }

                if (!meta.IsWideGamut && pngPrimaries is 9 or 11 or 12)
                {
                    meta.IsWideGamut = true;
                }
            }
        }

        // 6. JXL codestream color encoding
        if (!meta.IsHdr && meta.FileExtension is ".jxl")
        {
            if (ParseJxlColorEncoding(meta.FilePath) is (var jxlPrimaries, var jxlTransfer))
            {
                // JXL TransferFunction (CICP-compatible): 16=PQ, 18=HLG, 8=Linear
                if (jxlTransfer == 16)
                {
                    meta.HdrTransferFn = HdrTransferFunction.PQ;
                }
                else if (jxlTransfer == 18)
                {
                    meta.HdrTransferFn = HdrTransferFunction.HLG;
                }
                else if (jxlTransfer == 8) // Linear — HDR scene-referred
                {
                    meta.HdrTransferFn = HdrTransferFunction.Linear;
                }

                // JXL Primaries: 9=BT.2100/BT.2020, 11=P3
                if (!meta.IsWideGamut && jxlPrimaries is 9 or 11)
                {
                    meta.IsWideGamut = true;
                }
            }
            else
            {
                // Fallback only when parser couldn't determine TF (want_icc, all_default, etc.):
                // treat high bit-depth + wide gamut as HDR candidate.
                if (!meta.IsHdr
                    && meta.BitsPerChannel >= 10
                    && (meta.IsWideGamut
                        || img?.ColorSpace is ImageMagick.ColorSpace.DisplayP3))
                {
                    meta.HdrTransferFn = HdrTransferFunction.Linear;
                }
            }
        }

        // 7. Natively HDR formats: OpenEXR (.exr), Radiance HDR (.hdr),
        //    JPEG XR (.jxr/.wdp) are always HDR
        if (!meta.IsHdr
            && meta.FileExtension is ".exr" or ".hdr" or ".jxr" or ".wdp")
        {
            meta.HdrTransferFn = HdrTransferFunction.Linear;
            meta.IsWideGamut = true;
        }

        // 8. HDR10 EXIF metadata (ContentLightLevel / MasteringDisplayColorVolume)
        if (!meta.IsHdr && img is not null)
        {
            var cll = img.GetAttribute("exif:ContentLightLevel");
            var mdcv = img.GetAttribute("exif:MasteringDisplayColorVolume");
            if (cll is not null || mdcv is not null)
            {
                meta.HdrTransferFn = HdrTransferFunction.Linear;
            }
        }

        // 9. Magick ColorSpace-based wide gamut fallback
        if (!meta.IsWideGamut && img is not null
            && img.ColorSpace is ImageMagick.ColorSpace.DisplayP3)
        {
            meta.IsWideGamut = true;
        }

        // 10. HDR10 content peak, which the tone curve uses as its white level instead of guessing
        if (meta.IsHdr && meta.HdrTransferFn != HdrTransferFunction.GainMap)
        {
            meta.ContentPeakNits = DetectContentPeakNits(meta);
        }
    }


    /// <summary>
    /// Reads the declared peak content luminance in nits, preferring the container's own HDR10
    /// metadata over the EXIF copy. Returns <c>0</c> when the file declares none.
    /// </summary>
    private static double DetectContentPeakNits(PhotoMetadata meta)
    {
        var peak = meta.FileExtension switch
        {
            ".avif" or ".heif" or ".heic" or ".hif" => ParseContentPeakFromIsobmff(meta.FilePath),
            ".png" => ParseContentPeakFromPng(meta.FilePath),
            _ => null,
        };

        return peak ?? 0d;
    }


    /// <summary>
    /// Converts the Magick's <see cref="OrientationType"/> value to Skia's <see cref="SKEncodedOrigin"/> value.
    /// </summary>
    public static SKEncodedOrigin ToSkiaOrientation(OrientationType orientation)
    {
        return orientation switch
        {
            OrientationType.TopLeft => SKEncodedOrigin.TopLeft,
            OrientationType.TopRight => SKEncodedOrigin.TopRight,
            OrientationType.BottomRight => SKEncodedOrigin.BottomRight,
            OrientationType.BottomLeft => SKEncodedOrigin.BottomLeft,
            OrientationType.LeftTop => SKEncodedOrigin.LeftTop,
            OrientationType.RightTop => SKEncodedOrigin.RightTop,
            OrientationType.RightBottom => SKEncodedOrigin.RightBottom,
            OrientationType.LeftBottom => SKEncodedOrigin.LeftBottom,
            _ => SKEncodedOrigin.Default,
        };
    }



    /// <summary>
    /// Exports HDR float RGBA pixels from a Q16-HDRI Magick image.
    /// <para>
    /// Magick Q16-HDRI quantum range: 0 = black, 65535 = SDR white.
    /// Scene-referred HDR values above SDR white exceed 65535.
    /// This method normalizes to [0, 1+] for SkiaSharp <see cref="SKColorType.RgbaF32"/>.
    /// </para>
    /// <para>
    /// Channel order is discovered via <see cref="IUnsafePixelCollection{TQuantumType}.GetChannelIndex"/>
    /// because <see cref="IUnsafePixelCollection{TQuantumType}.GetArea"/> returns channels
    /// in Magick's internal order, which is NOT guaranteed to be RGB.
    /// </para>
    /// </summary>
    public static NativeMemoryArray<byte>? ExportHdrPixels(MagickImage imgM, IUnsafePixelCollection<float> pixels)
    {
        var hasAlpha = imgM.HasAlpha;
        var area = pixels.GetArea(0, 0, imgM.Width, imgM.Height);
        if (area is null || area.Length == 0) return null;

        var w = (int)imgM.Width;
        var h = (int)imgM.Height;
        var pixelCount = (long)w * h;
        var byteCount = pixelCount * 4 * sizeof(float);

        // callers shrink oversized images first; refuse rather than overflow if that was skipped
        if (pixelCount <= 0 || byteCount > int.MaxValue) return null;

        // Derive source channel count from the actual array size.
        // EXR files may have extra channels (depth, normals, etc.) beyond RGB(A).
        var srcChannels = (int)(area.Length / pixelCount);

        // Discover actual channel positions in Magick's internal order.
        var chR = (int)(pixels.GetChannelIndex(PixelChannel.Red) ?? 0);
        var chG = (int)(pixels.GetChannelIndex(PixelChannel.Green) ?? 1);
        var chB = (int)(pixels.GetChannelIndex(PixelChannel.Blue) ?? 2);
        var chA = hasAlpha ? (int)(pixels.GetChannelIndex(PixelChannel.Alpha) ?? 0u) : -1;

        var nativeBuffer = new NativeMemoryArray<byte>(byteCount, skipZeroClear: true, addMemoryPressure: true);
        var dst = MemoryMarshal.Cast<byte, float>(nativeBuffer.AsSpan());

        const float quantumScale = 1f / 65535f;
        var srcIdx = 0;

        for (var i = 0; i < pixelCount; i++)
        {
            var dstIdx = i * 4;

            if (srcChannels >= 3)
            {
                dst[dstIdx] = area[srcIdx + chR] * quantumScale;
                dst[dstIdx + 1] = area[srcIdx + chG] * quantumScale;
                dst[dstIdx + 2] = area[srcIdx + chB] * quantumScale;
                dst[dstIdx + 3] = chA >= 0 && srcIdx + chA < area.Length
                    ? area[srcIdx + chA] * quantumScale
                    : 1f;
            }
            else
            {
                // Grayscale (1 channel) or Gray+Alpha (2 channels)
                var gray = area[srcIdx] * quantumScale;
                dst[dstIdx] = gray;
                dst[dstIdx + 1] = gray;
                dst[dstIdx + 2] = gray;
                dst[dstIdx + 3] = chA >= 0 && srcChannels >= 2
                    ? area[srcIdx + chA] * quantumScale
                    : 1f;
            }

            srcIdx += srcChannels;
        }

        return nativeBuffer;
    }


    /// <summary>
    /// Get Magick color profile.
    /// </summary>
    /// <param name="name">Name or Full path of color profile</param>
    public static ColorProfile? GetColorProfileByName(string name)
    {
        // 1. don't use color profile
        if (name.Equals(nameof(ColorProfileOption.None))) return null;


        // 2. use built-in color profile
        var magickProfile = MagickCodec.GetBuiltinColorProfile(name);
        if (magickProfile is not null)
        {
            return magickProfile;
        }


        var profilePath = name;

        // 3. use current monitor profile
        if (name.Equals(nameof(ColorProfileOption.CurrentMonitorProfile)))
        {
            profilePath = Core.ColorProfileProvider?.ProfilePath;
        }


        // 4. use custom color profile
        if (Path.Exists(profilePath))
        {
            return new ColorProfile(profilePath);
        }

        return null;
    }


    /// <summary>
    /// Get built-in color profile by name.
    /// </summary>
    /// <param name="magickProfileName">Magick's color profile name</param>
    public static ColorProfile? GetBuiltinColorProfile(string magickProfileName)
    {
        // get magick's color profile from name
        var profile = magickProfileName switch
        {
            nameof(ColorProfiles.AdobeRGB1998) => ColorProfiles.AdobeRGB1998,
            nameof(ColorProfiles.AppleRGB) => ColorProfiles.AppleRGB,
            nameof(ColorProfiles.CoatedFOGRA39) => ColorProfiles.CoatedFOGRA39,
            nameof(ColorProfiles.ColorMatchRGB) => ColorProfiles.ColorMatchRGB,
            nameof(ColorProfiles.SRGB) => ColorProfiles.SRGB,
            nameof(ColorProfiles.USWebCoatedSWOP) => ColorProfiles.USWebCoatedSWOP,
            _ => null,
        };

        return profile;
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

        return opt.IsSupported(filePath ?? string.Empty);
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

}


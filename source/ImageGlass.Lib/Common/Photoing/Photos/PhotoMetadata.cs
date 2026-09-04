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
using ImageGlass.Common.Types;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using ImageMagick;
using SkiaSharp;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


public partial class PhotoMetadata : PhDisposable
{

    #region Public Properties

    #region File metadata
    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value) return;
            _filePath = value;

            SetFilePath__(value);
        }
    }

    public string FileName { get; private set; } = string.Empty;
    /// <summary>
    /// Getss file extension in lowercase. E.g. <c>.png</c>
    /// </summary>
    public string FileExtension { get; private set; } = string.Empty;
    public string FolderPath { get; private set; } = string.Empty;
    public string FolderName { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; } = 0;

    /// <summary>
    /// The formated file size. E.g. <c>32.09 MB</c>.
    /// </summary>
    public string FileSizeFormatted => BHelper.FormatSize(FileSizeInBytes);

    public DateTime FileCreationTimeUtc { get; private set; }
    public DateTime FileLastAccessTimeUtc { get; private set; }
    public DateTime FileLastWriteTimeUtc { get; private set; }
    public string FileCreationTimeFormatted => BHelper.FormatDateTime(FileCreationTimeUtc.ToLocalTime());
    public string FileLastAccessTimeFormatted => BHelper.FormatDateTime(FileLastAccessTimeUtc.ToLocalTime());
    public string FileLastWriteTimeFormatted => BHelper.FormatDateTime(FileLastWriteTimeUtc.ToLocalTime());

    #endregion // File metadata


    #region Bitmap information

    /// <summary>
    /// Gets the original width before processing orientation.
    /// </summary>
    public uint OriginalWidth { get; set; } = 0;

    /// <summary>
    /// Gets the original height before processing orientation.
    /// </summary>
    public uint OriginalHeight { get; set; } = 0;

    /// <summary>
    /// Gets the desired width after processing orientation.
    /// </summary>
    public uint Width { get; set; } = 0;

    /// <summary>
    /// Gets the desired height after processing orientation.
    /// </summary>
    public uint Height { get; set; } = 0;

    // DPI
    public double DpiX { get; set; } = 0;
    public double DpiY { get; set; } = 0;

    public uint FrameCount { get; set; } = 0;
    public string FrameCountFormatted => FrameCount > 1 ? FrameCount.ToString() : string.Empty;
    public uint AnimationLoop { get; set; } = 0;
    public IImmutableList<FrameMetadata> Frames { get; set; } = [];
    public bool HasAlpha { get; set; } = false;
    public bool CanAnimate { get; set; } = false;
    public SKEncodedOrigin Orientation { get; set; } = SKEncodedOrigin.Default;

    /// <summary>
    /// Gets, sets whether this image is a scalable vector format (SVG/SVGZ).
    /// </summary>
    public bool IsVector { get; set; } = false;

    /// <summary>The detected HDR transfer function type.</summary>
    public HdrTransferFunction HdrTransferFn { get; set; } = HdrTransferFunction.None;

    /// <summary>
    /// Whether the image is HDR (any transfer function other than
    /// <see cref="HdrTransferFunction.None"/>). Derived from
    /// <see cref="HdrTransferFn"/>; set <see cref="HdrTransferFn"/> to change.
    /// </summary>
    public bool IsHdr => HdrTransferFn != HdrTransferFunction.None;

    /// <summary>Whether the image has a wider-than-sRGB color gamut.</summary>
    public bool IsWideGamut { get; set; } = false;

    /// <summary>
    /// Peak content luminance in nits from the container's HDR10 metadata (MaxCLL, else the
    /// mastering display max luminance). <c>0</c> when the file declares none.
    /// </summary>
    public double ContentPeakNits { get; set; } = 0;

    /// <summary>The native bit depth per channel from the source codec.</summary>
    public int BitsPerChannel { get; set; } = 8;

    /// <summary>
    /// Gets the byte offset from end-of-file where the embedded video starts.
    /// For Google/Samsung motion photos. 0 means no embedded video.
    /// </summary>
    public long EmbeddedVideoOffsetFromEnd { get; set; } = 0;

    /// <summary>
    /// Whether this image contains an embedded motion/live photo video.
    /// Derived from <see cref="EmbeddedVideoOffsetFromEnd"/>; assign
    /// <see cref="EmbeddedVideoOffsetFromEnd"/> to change.
    /// </summary>
    public bool IsLivePhoto => EmbeddedVideoOffsetFromEnd > 0;

    #endregion // Bitmap information


    #region Color information

    public ColorSpace ColorSpace { get; set; } = ColorSpace.Undefined;
    public string ColorProfileName { get; set; } = string.Empty;


    /// <summary>
    /// Gets the Magick color profile.
    /// </summary>
    public IColorProfile? MagickColorProfile { get; set; } = null;

    /// <summary>
    /// Gets the Skia color space converted from <see cref="MagickColorProfile"/>.
    /// Returns <c>null</c> if unsupported color space.
    /// </summary>
    public SKColorSpace? SkiaColorSpace { get; set; } = null;

    public IImageProfile? RawThumbnail { get; set; } = null;

    #endregion // Color information


    #region EXIF metadata

    public IExifProfile? ExifProfile { get; set; } = null;
    public int ExifRatingPercent { get; set; } = 0;
    public string ExifRatingFormatted => BHelper.FormatStarRatingText(ExifRatingPercent);
    public DateTime? ExifDateTimeOriginal { get; set; } = null; // local time
    public DateTime? ExifDateTime { get; set; } = null; // local time

    public string ExifDateTimeOriginalFormatted => BHelper.FormatDateTime(ExifDateTimeOriginal?.ToLocalTime());
    public string ExifDateTimeFormatted => BHelper.FormatDateTime(ExifDateTime?.ToLocalTime());

    public string? ExifImageDescription { get; set; } = null;
    public string? ExifModel { get; set; } = null;
    public string? ExifArtist { get; set; } = null;
    public string? ExifCopyright { get; set; } = null;
    public string? ExifSoftware { get; set; } = null;
    public float? ExifExposureTime { get; set; } = null;
    public float? ExifFNumber { get; set; } = null;
    public int? ExifISOSpeed { get; set; } = null;
    public float? ExifFocalLength { get; set; } = null;

    #endregion // EXIF metadata

    #endregion // Public Properties



    public PhotoMetadata() { }

    public PhotoMetadata(string? filePath)
    {
        FilePath = filePath ?? string.Empty;
    }


    public PhotoMetadata(FileSearchEntry entry)
    {
        // assigned directly to skip SetFilePath__, which re-stats the file from disk
        _filePath = entry.FilePath;

        FileName = Path.GetFileName(entry.FilePath);
        FileExtension = Path.GetExtension(entry.FilePath).ToLowerInvariant();
        FolderPath = Path.GetDirectoryName(entry.FilePath) ?? string.Empty;
        FolderName = Path.GetFileName(FolderPath);
        FileSizeInBytes = entry.FileSizeInBytes;
        FileCreationTimeUtc = entry.FileCreationTimeUtc;
        FileLastWriteTimeUtc = entry.FileLastWriteTimeUtc;
        FileLastAccessTimeUtc = entry.FileLastAccessTimeUtc;
    }

    #region Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        SkiaColorSpace?.Dispose();
        SkiaColorSpace = null;
        MagickColorProfile = null;

        RawThumbnail = null;
        ExifProfile = null;
        FrameCount = 0;
        Frames.Clear();

        EmbeddedVideoOffsetFromEnd = 0;
        HdrTransferFn = HdrTransferFunction.None;
        ContentPeakNits = 0;
    }


    /// <summary>
    /// Sets the file path and extracts file-related metadata.
    /// </summary>
    private void SetFilePath__(string? filePath)
    {
        try
        {
            var fi = new FileInfo(filePath!);

            FileName = fi.Name;
            FileExtension = fi.Extension.ToLowerInvariant();
            FolderPath = fi.DirectoryName ?? string.Empty;
            FolderName = fi.Directory?.Name ?? string.Empty;

            FileSizeInBytes = fi.Length;
            FileCreationTimeUtc = fi.CreationTimeUtc;
            FileLastWriteTimeUtc = fi.LastWriteTimeUtc;
            FileLastAccessTimeUtc = fi.LastAccessTimeUtc;
        }
        catch
        {
            FileName = string.Empty;
            FileExtension = string.Empty;
            FolderPath = string.Empty;
            FolderName = string.Empty;

            FileSizeInBytes = 0;
            FileCreationTimeUtc = DateTime.MinValue;
            FileLastWriteTimeUtc = DateTime.MinValue;
            FileLastAccessTimeUtc = DateTime.MinValue;
        }
    }


    /// <summary>
    /// Checks if the metadata is outdated.
    /// </summary>
    public bool IsOutdated()
    {
        if (FrameCount == 0) return true;

        // check if the current Metadata is outdated or not
        var hasOutdatedCache = true;

        try
        {
            var fi = new FileInfo(FilePath);
            hasOutdatedCache = FileLastWriteTimeUtc < fi.LastWriteTimeUtc;
        }
        catch { }

        return hasOutdatedCache;
    }


    /// <summary>
    /// Retrieves an embedded thumbnail from either a RAW format or an EXIF profile if exists.
    /// </summary>
    public MagickImage? GetEmbeddedPreview()
    {
        if (RawThumbnail is null && ExifProfile is null) return null;

        MagickImage? thumbM = null;


        // 1. try get from RAW format
        if (RawThumbnail is not null)
        {
            thumbM = new MagickImage(RawThumbnail.ToReadOnlySpan());
        }


        // 2. try get from EXIF profile
        if (thumbM is null && ExifProfile is not null)
        {
            thumbM = (MagickImage?)ExifProfile.CreateThumbnail();
        }


        // 3. fix orientation
        if (thumbM is not null)
        {
            thumbM.Orientation = SkiaCodec.ToMagickOrientation(Orientation);
            thumbM?.AutoOrient();
        }

        return thumbM;
    }


    /// <summary>
    /// Extracts the embedded video from a motion/live photo and opens it
    /// in the system default video player.
    /// </summary>
    public async Task OpenLivePhotoVideoAsync()
    {
        if (!IsLivePhoto || EmbeddedVideoOffsetFromEnd == 0) return;

        var videoPath = await Task.Run(() => LivePhotoDetector.ExtractEmbeddedVideoAsync(FilePath, EmbeddedVideoOffsetFromEnd))
                .ConfigureAwait(false);

        if (string.IsNullOrEmpty(videoPath)) return;

        var psi = new ProcessStartInfo(videoPath)
        {
            UseShellExecute = true,
        };
        using var proc = Process.Start(psi);
    }


    #endregion // Methods



}


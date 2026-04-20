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
using System.Buffers.Binary;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WicNet;

namespace ImageGlass.Base.Photoing.Codecs;

internal static class IthmbDecoder
{
    private const string IthmbExtension = ".ithmb";
    private const string PhotoDatabaseFileName = "Photo Database";
    private static readonly byte[] JfifMarker = "JFIF\0"u8.ToArray();
    private static readonly byte[] ExifMarker = "Exif\0\0"u8.ToArray();
    private static readonly byte[] JpegEndMarker = [0xFF, 0xD9];

    private enum IthmbEncoding
    {
        Rgb565,
        Yuv422InterlacedSharedChrominance,
        Ycbcr420Padded,
    }

    private sealed record IthmbVariantProfile(
        int Prefix,
        int Width,
        int Height,
        IthmbEncoding Encoding,
        int FrameByteLength,
        int MinimumDecodeByteLength,
        RotateFlipType Rotation = RotateFlipType.RotateNoneFlipNone,
        bool LittleEndian = true)
    {
        public bool SwapsDimensions => Rotation is RotateFlipType.Rotate90FlipNone
            or RotateFlipType.Rotate270FlipNone
            or RotateFlipType.Rotate90FlipX
            or RotateFlipType.Rotate270FlipX;
    }

    private sealed record IthmbProbeResult(
        string FilePath,
        string FileName,
        long FileSize,
        int? Prefix,
        IthmbVariantProfile? Profile);

    private sealed record IthmbFrameLayout(
        long Offset,
        int ReadLength,
        int Width,
        int Height);

    private readonly record struct IthmbEmbeddedImageSlice(
        long Offset,
        int Length);

    private sealed record IthmbResolvedLayout(
        IthmbVariantProfile Profile,
        IReadOnlyList<IthmbFrameLayout> Frames,
        bool UsedCompanionMetadata,
        string? CompanionMetadataPath);

    private readonly record struct IthmbPhotoDatabaseEntry(
        int Offset,
        int ImageSizeBytes,
        int Width,
        int Height);

    private readonly record struct IthmbPhotoDatabaseGroupKey(
        int Width,
        int Height,
        int ImageSizeBytes);

    private static readonly IReadOnlyDictionary<int, IthmbVariantProfile> KnownProfiles =
        new Dictionary<int, IthmbVariantProfile>()
        {
            [1007] = new(1007, 480, 864, IthmbEncoding.Rgb565, 480 * 864 * 2, 480 * 864 * 2),
            [1009] = new(1009, 42, 30, IthmbEncoding.Rgb565, 42 * 30 * 2, 42 * 30 * 2),
            [1015] = new(1015, 130, 88, IthmbEncoding.Rgb565, 130 * 88 * 2, 130 * 88 * 2),
            [1019] = new(1019, 720, 480, IthmbEncoding.Yuv422InterlacedSharedChrominance, 720 * 480 * 2, 720 * 480 * 2),
            [1020] = new(1020, 176, 220, IthmbEncoding.Rgb565, 176 * 220 * 2, 176 * 220 * 2, RotateFlipType.Rotate270FlipNone),
            [1023] = new(1023, 176, 132, IthmbEncoding.Rgb565, 176 * 132 * 2, 176 * 132 * 2),
            [1024] = new(1024, 320, 240, IthmbEncoding.Rgb565, 320 * 240 * 2, 320 * 240 * 2),
            [1036] = new(1036, 50, 41, IthmbEncoding.Rgb565, 50 * 41 * 2, 50 * 41 * 2),
            [1067] = new(1067, 720, 480, IthmbEncoding.Ycbcr420Padded, 720 * 480 * 2, (720 * 480 * 3) / 2),
            [3008] = new(3008, 640, 480, IthmbEncoding.Rgb565, 640 * 480 * 2, 640 * 480 * 2),
        };


    public static bool IsIthmbFile(string? filePath)
    {
        return string.Equals(Path.GetExtension(filePath), IthmbExtension, StringComparison.OrdinalIgnoreCase);
    }


    public static IgMetadata LoadMetadata(string filePath, CodecReadOptions? options = null)
    {
        var meta = new IgMetadata();

        try
        {
            var fi = new FileInfo(filePath);

            meta.FilePath = filePath;
            meta.FileName = fi.Name;
            meta.FileExtension = fi.Extension.ToUpperInvariant();
            meta.FolderPath = fi.DirectoryName ?? string.Empty;
            meta.FolderName = Path.GetFileName(meta.FolderPath);
            meta.FileSize = fi.Length;
            meta.FileCreationTime = fi.CreationTime;
            meta.FileLastWriteTime = fi.LastWriteTime;
            meta.FileLastAccessTime = fi.LastAccessTime;

            if (TryResolveEmbeddedJpeg(filePath, out var embeddedJpeg)
                && TryLoadEmbeddedJpegMetadata(filePath, embeddedJpeg, out var width, out var height, out var dpiX, out var dpiY))
            {
                meta.OriginalWidth = width;
                meta.OriginalHeight = height;
                meta.RenderedWidth = width;
                meta.RenderedHeight = height;
                meta.DpiX = dpiX;
                meta.DpiY = dpiY;
                meta.FrameCount = 1;
                meta.FrameIndex = 0;
                meta.HasAlpha = false;
                meta.CanAnimate = false;
                meta.ColorSpace = "RGB";
                return meta;
            }

            if (!TryResolveLayout(filePath, out var layout))
            {
                return meta;
            }

            var frameIndex = NormalizeFrameIndex(options?.FrameIndex ?? 0, layout.Frames.Count);
            var frame = layout.Frames[frameIndex];

            meta.OriginalWidth = GetRenderedWidth(layout.Profile, frame.Width, frame.Height);
            meta.OriginalHeight = GetRenderedHeight(layout.Profile, frame.Width, frame.Height);
            meta.RenderedWidth = meta.OriginalWidth;
            meta.RenderedHeight = meta.OriginalHeight;
            meta.FrameCount = layout.Frames.Count;
            meta.FrameIndex = (uint)frameIndex;
            meta.HasAlpha = false;
            meta.CanAnimate = false;
            meta.ColorSpace = GetColorSpace(layout.Profile.Encoding);
        }
        catch { }

        return meta;
    }


    public static async Task<IgImgData> LoadAsync(string filePath, CodecReadOptions? options = null, ImgTransform? transform = null, CancellationToken token = default)
    {
        options ??= new();

        if (TryResolveEmbeddedJpeg(filePath, out var embeddedJpeg))
        {
            return await LoadEmbeddedJpegAsync(filePath, embeddedJpeg, options, transform, token);
        }

        if (!TryResolveLayout(filePath, out var layout))
        {
            return new();
        }

        if (layout.Frames.Count < 1)
        {
            return new();
        }

        var frameIndex = NormalizeFrameIndex(options.FrameIndex ?? 0, layout.Frames.Count);
        var frame = layout.Frames[frameIndex];
        var frameData = new byte[frame.ReadLength];

        await using var fs = File.OpenRead(filePath);
        fs.Seek(frame.Offset, SeekOrigin.Begin);
        await fs.ReadExactlyAsync(frameData, token);

        token.ThrowIfCancellationRequested();

        using var bmp = DecodeFrame(frameData, layout.Profile, frame.Width, frame.Height);
        var wicSrc = BHelper.ToWicBitmapSource(bmp);
        if (wicSrc == null)
        {
            return new();
        }

        if (options.Width > 0 && options.Height > 0
            && (wicSrc.Width > options.Width || wicSrc.Height > options.Height))
        {
            var resized = await BHelper.ResizeImageAsync(wicSrc, (int)options.Width, (int)options.Height);
            if (resized != null)
            {
                wicSrc.Dispose();
                wicSrc = resized;
            }
        }

        wicSrc = PhotoCodec.TransformImage(wicSrc, transform);

        return new IgImgData()
        {
            Image = wicSrc,
            FrameCount = layout.Frames.Count,
            HasAlpha = false,
            CanAnimate = false,
        };
    }


    public static async Task<Bitmap?> GetThumbnailAsync(string filePath, uint width, uint height, CancellationToken token = default)
    {
        using var imgData = await LoadAsync(filePath, new CodecReadOptions()
        {
            Width = width,
            Height = height,
            FirstFrameOnly = true,
            FrameIndex = 0,
        }, null, token);

        if (imgData.Image == null)
        {
            return null;
        }

        using var thumb = BHelper.ToGdiPlusBitmap(imgData.Image);
        return thumb == null ? null : new Bitmap(thumb);
    }


    public static async IAsyncEnumerable<(int FrameNumber, WicBitmapSource? Frame)> EnumerateFramesAsync(string filePath, [EnumeratorCancellation] CancellationToken token = default)
    {
        if (TryResolveEmbeddedJpeg(filePath, out var embeddedJpeg))
        {
            var frame = await LoadEmbeddedJpegAsync(filePath, embeddedJpeg, new CodecReadOptions()
            {
                FirstFrameOnly = true,
                FrameIndex = 0,
            }, null, token);

            yield return (1, frame.Image);
            frame.Image = null;
            frame.Dispose();
            yield break;
        }

        if (!TryResolveLayout(filePath, out var layout))
        {
            yield break;
        }

        for (var i = 0; i < layout.Frames.Count; i++)
        {
            token.ThrowIfCancellationRequested();

            var frame = await LoadAsync(filePath, new CodecReadOptions()
            {
                FirstFrameOnly = true,
                FrameIndex = i,
            }, null, token);

            yield return (i + 1, frame.Image);
            frame.Image = null;
            frame.Dispose();
        }
    }


    private static async Task<IgImgData> LoadEmbeddedJpegAsync(
        string filePath,
        IthmbEmbeddedImageSlice embeddedJpeg,
        CodecReadOptions options,
        ImgTransform? transform,
        CancellationToken token)
    {
        var bytes = await ReadEmbeddedSliceAsync(filePath, embeddedJpeg, token);
        using var ms = new MemoryStream(bytes, writable: false);
        using var image = Image.FromStream(ms, false, false);
        using var bmp = new Bitmap(image);
        var wicSrc = BHelper.ToWicBitmapSource(bmp);
        if (wicSrc == null)
        {
            return new();
        }

        if (options.Width > 0 && options.Height > 0
            && (wicSrc.Width > options.Width || wicSrc.Height > options.Height))
        {
            var resized = await BHelper.ResizeImageAsync(wicSrc, (int)options.Width, (int)options.Height);
            if (resized != null)
            {
                wicSrc.Dispose();
                wicSrc = resized;
            }
        }

        wicSrc = PhotoCodec.TransformImage(wicSrc, transform);

        return new IgImgData()
        {
            Image = wicSrc,
            FrameCount = 1,
            HasAlpha = false,
            CanAnimate = false,
        };
    }


    private static bool TryResolveLayout(string filePath, out IthmbResolvedLayout layout)
    {
        layout = default!;
        var probe = Probe(filePath);

        if (TryResolveLayoutFromPhotoDatabase(probe, out layout))
        {
            return true;
        }

        return TryResolveLayoutFromProfile(probe, out layout);
    }


    private static IthmbProbeResult Probe(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        IthmbVariantProfile? profile = null;
        int? prefix = null;

        if (TryExtractPrefix(fileInfo.Name, out var extractedPrefix))
        {
            prefix = extractedPrefix;
            KnownProfiles.TryGetValue(extractedPrefix, out profile);
        }

        return new IthmbProbeResult(
            filePath,
            fileInfo.Name,
            fileInfo.Exists ? fileInfo.Length : 0,
            prefix,
            profile);
    }


    private static bool TryResolveLayoutFromProfile(IthmbProbeResult probe, out IthmbResolvedLayout layout)
    {
        layout = default!;

        if (probe.Profile == null || probe.Profile.FrameByteLength < 1)
        {
            return false;
        }

        var frameCount = (int)Math.Min(probe.FileSize / probe.Profile.FrameByteLength, int.MaxValue);
        if (frameCount < 1)
        {
            return false;
        }

        var frames = new List<IthmbFrameLayout>(frameCount);
        for (var i = 0; i < frameCount; i++)
        {
            frames.Add(new IthmbFrameLayout(
                Offset: (long)i * probe.Profile.FrameByteLength,
                ReadLength: probe.Profile.FrameByteLength,
                Width: probe.Profile.Width,
                Height: probe.Profile.Height));
        }

        layout = new IthmbResolvedLayout(probe.Profile, frames, false, null);
        return true;
    }


    private static bool TryResolveLayoutFromPhotoDatabase(IthmbProbeResult probe, out IthmbResolvedLayout layout)
    {
        layout = default!;
        IthmbResolvedLayout? bestLayout = null;
        var bestScore = int.MinValue;

        foreach (var metadataPath in EnumerateCompanionMetadataPaths(probe.FilePath))
        {
            var entries = ParsePhotoDatabase(metadataPath);
            if (entries.Count < 1)
            {
                continue;
            }

            foreach (var group in entries.GroupBy(entry => new IthmbPhotoDatabaseGroupKey(entry.Width, entry.Height, entry.ImageSizeBytes)))
            {
                foreach (var profile in EnumerateCandidateProfiles(probe, group.Key))
                {
                    var frames = BuildFramesFromMetadata(group, profile, probe.FileSize);
                    if (frames.Count < 1)
                    {
                        continue;
                    }

                    var score = ScoreMetadataCandidate(probe, profile, group.Key, frames.Count);
                    if (score <= bestScore)
                    {
                        continue;
                    }

                    bestScore = score;
                    bestLayout = new IthmbResolvedLayout(profile, frames, true, metadataPath);
                }
            }
        }

        if (bestLayout == null)
        {
            return false;
        }

        layout = bestLayout;
        return true;
    }


    private static IEnumerable<IthmbVariantProfile> EnumerateCandidateProfiles(IthmbProbeResult probe, IthmbPhotoDatabaseGroupKey groupKey)
    {
        if (probe.Profile != null)
        {
            yield return probe.Profile;
        }

        foreach (var profile in KnownProfiles.Values)
        {
            if (probe.Profile?.Prefix == profile.Prefix)
            {
                continue;
            }

            if (probe.Prefix.HasValue && probe.Prefix.Value == profile.Prefix)
            {
                yield return profile;
                continue;
            }

            if (MatchesDimensions(profile, groupKey.Width, groupKey.Height)
                || MatchesStoredSize(profile, groupKey.ImageSizeBytes))
            {
                yield return profile;
            }
        }
    }


    private static IReadOnlyList<IthmbFrameLayout> BuildFramesFromMetadata(
        IGrouping<IthmbPhotoDatabaseGroupKey, IthmbPhotoDatabaseEntry> group,
        IthmbVariantProfile profile,
        long fileSize)
    {
        var readLength = DetermineReadLength(profile, group.Key.ImageSizeBytes);
        if (readLength < profile.MinimumDecodeByteLength)
        {
            return Array.Empty<IthmbFrameLayout>();
        }

        var width = group.Key.Width > 0 ? group.Key.Width : profile.Width;
        var height = group.Key.Height > 0 ? group.Key.Height : profile.Height;

        var frames = group
            .Select(entry => entry.Offset)
            .Where(offset => offset >= 0)
            .Distinct()
            .OrderBy(offset => offset)
            .Select(offset => CreateFrameLayout(offset, readLength, width, height, profile, fileSize))
            .Where(frame => frame != null)
            .Cast<IthmbFrameLayout>()
            .ToList();

        return frames;
    }


    private static IthmbFrameLayout? CreateFrameLayout(
        int offset,
        int readLength,
        int width,
        int height,
        IthmbVariantProfile profile,
        long fileSize)
    {
        if (offset >= fileSize)
        {
            return null;
        }

        var availableBytes = fileSize - offset;
        var actualReadLength = (int)Math.Min(availableBytes, readLength);
        if (actualReadLength < profile.MinimumDecodeByteLength)
        {
            return null;
        }

        return new IthmbFrameLayout(offset, actualReadLength, width, height);
    }


    private static IEnumerable<string> EnumerateCompanionMetadataPaths(string filePath)
    {
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var directory = Path.GetDirectoryName(filePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            yield break;
        }

        foreach (var baseDirectory in EnumerateCompanionDirectories(directory))
        {
            var candidatePath = Path.Combine(baseDirectory, PhotoDatabaseFileName);
            if (!seenPaths.Add(candidatePath) || !File.Exists(candidatePath))
            {
                continue;
            }

            yield return candidatePath;
        }
    }


    private static IEnumerable<string> EnumerateCompanionDirectories(string directory)
    {
        yield return directory;

        var parent = Directory.GetParent(directory)?.FullName;
        if (!string.IsNullOrWhiteSpace(parent))
        {
            yield return parent;
        }
    }


    private static IReadOnlyList<IthmbPhotoDatabaseEntry> ParsePhotoDatabase(string filePath)
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);
            var entries = new List<IthmbPhotoDatabaseEntry>();

            for (var index = 0; index <= bytes.Length - 44; index += 4)
            {
                if (!HasMarker(bytes, index, "mhni"))
                {
                    continue;
                }

                var offset = ReadInt32LittleEndian(bytes, index + 16);
                var imageSizeBytes = ReadInt32LittleEndian(bytes, index + 24);
                var height = ReadUInt16LittleEndian(bytes, index + 32);
                var width = ReadUInt16LittleEndian(bytes, index + 34);

                if (offset >= 0 && imageSizeBytes >= 0)
                {
                    entries.Add(new IthmbPhotoDatabaseEntry(offset, imageSizeBytes, width, height));
                }

                index += 40;
            }

            return entries;
        }
        catch
        {
            return Array.Empty<IthmbPhotoDatabaseEntry>();
        }
    }


    private static bool TryExtractPrefix(string fileName, out int prefix)
    {
        prefix = 0;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            return false;
        }

        var digits = new string(fileNameWithoutExtension
            .SkipWhile(c => !char.IsDigit(c))
            .TakeWhile(char.IsDigit)
            .ToArray());

        return int.TryParse(digits, out prefix);
    }


    private static bool TryResolveEmbeddedJpeg(string filePath, out IthmbEmbeddedImageSlice embeddedJpeg)
    {
        embeddedJpeg = default;

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtension)
            || !fileNameWithoutExtension.StartsWith("T", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var bytes = File.ReadAllBytes(filePath);
            if (!TryFindEmbeddedJpegOffset(bytes, out var jpegOffset))
            {
                return false;
            }

            var eoiRelative = bytes.AsSpan(jpegOffset).LastIndexOf(JpegEndMarker);
            if (eoiRelative < 0)
            {
                return false;
            }

            embeddedJpeg = new IthmbEmbeddedImageSlice(jpegOffset, eoiRelative + JpegEndMarker.Length);
            return embeddedJpeg.Length > 2;
        }
        catch
        {
            return false;
        }
    }


    private static bool TryFindEmbeddedJpegOffset(ReadOnlySpan<byte> bytes, out int jpegOffset)
    {
        jpegOffset = -1;

        foreach (var marker in new[] { JfifMarker, ExifMarker })
        {
            var markerOffset = bytes.IndexOf(marker);
            if (markerOffset < 6)
            {
                continue;
            }

            var candidateOffset = markerOffset - 6;
            if (candidateOffset + 4 > bytes.Length)
            {
                continue;
            }

            if (bytes[candidateOffset] != 0xFF
                || bytes[candidateOffset + 1] != 0xD8
                || bytes[candidateOffset + 2] != 0xFF
                || bytes[candidateOffset + 3] is not (0xE0 or 0xE1 or 0xE2))
            {
                continue;
            }

            jpegOffset = candidateOffset;
            return true;
        }

        return false;
    }


    private static bool TryLoadEmbeddedJpegMetadata(
        string filePath,
        IthmbEmbeddedImageSlice embeddedJpeg,
        out uint width,
        out uint height,
        out float dpiX,
        out float dpiY)
    {
        width = 0;
        height = 0;
        dpiX = 0;
        dpiY = 0;

        try
        {
            using var ms = new MemoryStream(ReadEmbeddedSlice(filePath, embeddedJpeg), writable: false);
            using var image = Image.FromStream(ms, false, false);
            width = (uint)image.Width;
            height = (uint)image.Height;
            dpiX = image.HorizontalResolution;
            dpiY = image.VerticalResolution;
            return width > 0 && height > 0;
        }
        catch
        {
            return false;
        }
    }


    private static byte[] ReadEmbeddedSlice(string filePath, IthmbEmbeddedImageSlice slice)
    {
        using var fs = File.OpenRead(filePath);
        fs.Seek(slice.Offset, SeekOrigin.Begin);

        var bytes = new byte[slice.Length];
        fs.ReadExactly(bytes);
        return bytes;
    }


    private static async Task<byte[]> ReadEmbeddedSliceAsync(string filePath, IthmbEmbeddedImageSlice slice, CancellationToken token)
    {
        await using var fs = File.OpenRead(filePath);
        fs.Seek(slice.Offset, SeekOrigin.Begin);

        var bytes = new byte[slice.Length];
        await fs.ReadExactlyAsync(bytes, token);
        return bytes;
    }


    private static int ScoreMetadataCandidate(
        IthmbProbeResult probe,
        IthmbVariantProfile profile,
        IthmbPhotoDatabaseGroupKey groupKey,
        int frameCount)
    {
        var score = 0;

        if (probe.Prefix == profile.Prefix)
        {
            score += 500;
        }

        if (probe.Profile?.Prefix == profile.Prefix)
        {
            score += 250;
        }

        if (MatchesDimensions(profile, groupKey.Width, groupKey.Height))
        {
            score += 200;
        }

        if (MatchesStoredSize(profile, groupKey.ImageSizeBytes))
        {
            score += 160;
        }

        if (groupKey.Width > 0 && groupKey.Height > 0)
        {
            score += 50;
        }

        if (groupKey.ImageSizeBytes > 0)
        {
            score += 30;
        }

        score += Math.Min(frameCount, 25);

        return score;
    }


    private static bool MatchesDimensions(IthmbVariantProfile profile, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        return (width == profile.Width && height == profile.Height)
            || (width == profile.Height && height == profile.Width);
    }


    private static bool MatchesStoredSize(IthmbVariantProfile profile, int storedSize)
    {
        if (storedSize <= 0)
        {
            return false;
        }

        return storedSize == profile.FrameByteLength
            || storedSize == profile.MinimumDecodeByteLength
            || (storedSize > profile.MinimumDecodeByteLength && storedSize < profile.FrameByteLength);
    }


    private static int DetermineReadLength(IthmbVariantProfile profile, int storedSize)
    {
        if (storedSize >= profile.MinimumDecodeByteLength && storedSize <= profile.FrameByteLength)
        {
            return storedSize;
        }

        return profile.FrameByteLength;
    }


    private static int NormalizeFrameIndex(int requestedIndex, int frameCount)
    {
        if (frameCount < 1)
        {
            return 0;
        }

        if (requestedIndex >= frameCount)
        {
            return 0;
        }

        if (requestedIndex < 0)
        {
            return frameCount - 1;
        }

        return requestedIndex;
    }


    private static uint GetRenderedWidth(IthmbVariantProfile profile, int width, int height)
    {
        return (uint)(profile.SwapsDimensions ? height : width);
    }


    private static uint GetRenderedHeight(IthmbVariantProfile profile, int width, int height)
    {
        return (uint)(profile.SwapsDimensions ? width : height);
    }


    private static string GetColorSpace(IthmbEncoding encoding)
    {
        return encoding == IthmbEncoding.Rgb565 ? "RGB" : "YCbCr";
    }


    private static bool HasMarker(ReadOnlySpan<byte> data, int offset, string marker)
    {
        if (offset < 0 || offset + marker.Length > data.Length)
        {
            return false;
        }

        for (var i = 0; i < marker.Length; i++)
        {
            if (data[offset + i] != marker[i])
            {
                return false;
            }
        }

        return true;
    }


    private static int ReadInt32LittleEndian(ReadOnlySpan<byte> data, int offset)
    {
        return BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, 4));
    }


    private static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> data, int offset)
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
    }


    private static Bitmap DecodeFrame(ReadOnlySpan<byte> frameData, IthmbVariantProfile profile, int width, int height)
    {
        using var source = profile.Encoding switch
        {
            IthmbEncoding.Rgb565 => DecodeRgb565(frameData, width, height, profile.LittleEndian),
            IthmbEncoding.Yuv422InterlacedSharedChrominance => DecodeYuv422Interlaced(frameData, width, height),
            IthmbEncoding.Ycbcr420Padded => DecodeYcbcr420Padded(frameData, width, height),
            _ => throw new NotSupportedException(),
        };

        if (profile.Rotation != RotateFlipType.RotateNoneFlipNone)
        {
            source.RotateFlip(profile.Rotation);
        }

        return new Bitmap(source);
    }


    private static Bitmap CreateEmptyBitmap(int width, int height)
    {
        return new Bitmap(Math.Max(width, 1), Math.Max(height, 1), PixelFormat.Format32bppArgb);
    }


    private static Bitmap DecodeRgb565(ReadOnlySpan<byte> frameData, int width, int height, bool littleEndian)
    {
        var requiredBytes = width * height * 2;
        if (frameData.Length < requiredBytes)
        {
            return CreateEmptyBitmap(width, height);
        }

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        var stride = Math.Abs(data.Stride);

        try
        {
            var buffer = new byte[stride * height];

            for (var y = 0; y < height; y++)
            {
                var rowOffset = y * stride;
                var srcOffset = y * width * 2;

                for (var x = 0; x < width; x++)
                {
                    var pixelOffset = srcOffset + x * 2;
                    var raw = littleEndian
                        ? frameData[pixelOffset] | (frameData[pixelOffset + 1] << 8)
                        : (frameData[pixelOffset] << 8) | frameData[pixelOffset + 1];

                    var r5 = (raw >> 11) & 0x1F;
                    var g6 = (raw >> 5) & 0x3F;
                    var b5 = raw & 0x1F;

                    var destOffset = rowOffset + x * 4;
                    buffer[destOffset] = (byte)((b5 << 3) | (b5 >> 2));
                    buffer[destOffset + 1] = (byte)((g6 << 2) | (g6 >> 4));
                    buffer[destOffset + 2] = (byte)((r5 << 3) | (r5 >> 2));
                    buffer[destOffset + 3] = 255;
                }
            }

            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
        }
        finally
        {
            bmp.UnlockBits(data);
        }

        return bmp;
    }


    private static Bitmap DecodeYuv422Interlaced(ReadOnlySpan<byte> frameData, int width, int height)
    {
        var requiredBytes = width * height * 2;
        if (frameData.Length < requiredBytes)
        {
            return CreateEmptyBitmap(width, height);
        }

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        var stride = Math.Abs(data.Stride);
        var buffer = new byte[stride * height];
        var numPixels = width * height;

        try
        {
            for (var y = 0; y < height; y++)
            {
                var rowOffset = y * stride;
                var halfRow = y / 2;

                for (var x = 0; x < width; x++)
                {
                    var pairOffset = x % 2 == 0 ? x * 2 : (x - 1) * 2;
                    var srcOffset = y % 2 == 0
                        ? halfRow * width * 2 + pairOffset
                        : numPixels + (halfRow * width * 2) + pairOffset;

                    if (srcOffset + 4 > frameData.Length)
                    {
                        continue;
                    }

                    var u = frameData[srcOffset];
                    var y1 = frameData[srcOffset + 1];
                    var v = frameData[srcOffset + 2];
                    var y2 = frameData[srcOffset + 3];

                    var (r, g, b) = YcbcrToRgb(x % 2 == 0 ? y1 : y2, u, v);

                    var destOffset = rowOffset + x * 4;
                    buffer[destOffset] = b;
                    buffer[destOffset + 1] = g;
                    buffer[destOffset + 2] = r;
                    buffer[destOffset + 3] = 255;
                }
            }

            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
        }
        finally
        {
            bmp.UnlockBits(data);
        }

        return bmp;
    }


    private static Bitmap DecodeYcbcr420Padded(ReadOnlySpan<byte> frameData, int width, int height)
    {
        var chromaWidth = (width + 1) / 2;
        var chromaHeight = (height + 1) / 2;
        var luminanceLength = width * height;
        var chromaPlaneLength = chromaWidth * chromaHeight;
        var minimumRequiredBytes = luminanceLength + (chromaPlaneLength * 2);
        if (frameData.Length < minimumRequiredBytes)
        {
            return CreateEmptyBitmap(width, height);
        }

        var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
        var stride = Math.Abs(data.Stride);
        var buffer = new byte[stride * height];

        try
        {
            for (var y = 0; y < height; y++)
            {
                var rowOffset = y * stride;

                for (var x = 0; x < width; x++)
                {
                    var yIndex = y * width + x;
                    var cIndex = (y / 2) * chromaWidth + (x / 2);

                    var luminance = frameData[yIndex];
                    var chromaBlue = frameData[luminanceLength + cIndex];
                    var chromaRed = frameData[luminanceLength + chromaPlaneLength + cIndex];
                    var (r, g, b) = YcbcrToRgb(luminance, chromaBlue, chromaRed);

                    var destOffset = rowOffset + x * 4;
                    buffer[destOffset] = b;
                    buffer[destOffset + 1] = g;
                    buffer[destOffset + 2] = r;
                    buffer[destOffset + 3] = 255;
                }
            }

            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
        }
        finally
        {
            bmp.UnlockBits(data);
        }

        return bmp;
    }


    private static (byte R, byte G, byte B) YcbcrToRgb(byte y, byte cb, byte cr)
    {
        var yVal = y;
        var cbVal = cb - 128d;
        var crVal = cr - 128d;

        var r = Math.Clamp((int)Math.Round(yVal + 1.402 * crVal), 0, 255);
        var g = Math.Clamp((int)Math.Round(yVal - 0.344136 * cbVal - 0.714136 * crVal), 0, 255);
        var b = Math.Clamp((int)Math.Round(yVal + 1.772 * cbVal), 0, 255);

        return ((byte)r, (byte)g, (byte)b);
    }
}

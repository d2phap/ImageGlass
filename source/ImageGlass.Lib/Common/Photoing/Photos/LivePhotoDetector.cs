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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Result of live/motion photo detection.
/// </summary>
public record LivePhotoInfo(bool IsLivePhoto, long EmbeddedVideoOffsetFromEnd);


/// <summary>
/// Detects and extracts embedded video from Google Pixel and Samsung Galaxy motion photos.
/// All methods are static, stateless, and AOT-safe (no reflection, no XML DOM).
/// </summary>
public static class LivePhotoDetector
{
    /// <summary>
    /// Max bytes to read from the head of a JPEG file for XMP detection.
    /// </summary>
    private const int HEAD_READ_SIZE_JPEG = 65536;

    /// <summary>
    /// Max bytes to read from the head of a HEIF/HEIC file for XMP detection.
    /// HEIF/HEIC use ISOBMFF boxes that can push XMP well past 64KB.
    /// </summary>
    private const int HEAD_READ_SIZE_HEIF = 2 * 1024 * 1024;

    /// <summary>
    /// Max bytes to read from the tail of the file for MP4 ftyp scanning.
    /// </summary>
    private const int TAIL_READ_SIZE = 65536;

    /// <summary>
    /// Chunk size for scanning files backwards for Samsung markers.
    /// </summary>
    private const int SCAN_CHUNK_SIZE = 256 * 1024;

    /// <summary>
    /// HEIF/HEIC file extensions.
    /// </summary>
    private static readonly string[] _heifExtensions =
        [".heif", ".heic", ".hif", ".avif"];

    /// <summary>
    /// MP4 ftyp box signature bytes: "ftyp".
    /// </summary>
    private static ReadOnlySpan<byte> FtypSignature => "ftyp"u8;

    /// <summary>
    /// Samsung MotionPhoto_Data marker.
    /// </summary>
    private static ReadOnlySpan<byte> SamsungMotionPhotoDataMarker => "MotionPhoto_Data"u8;

    /// <summary>
    /// Samsung mpv2 version tag.
    /// </summary>
    private static ReadOnlySpan<byte> SamsungMpv2Tag => "mpv2"u8;

    /// <summary>
    /// Tracks temp video files created during the session for cleanup on exit.
    /// </summary>
    private static readonly List<string> _tempFiles = [];
    private static readonly Lock _tempFilesLock = new();
    private static bool _exitHandlerRegistered;


    #region Public Methods

    /// <summary>
    /// Detects whether the given image file is a motion/live photo.
    /// Thread-safe and stateless.
    /// </summary>
    public static LivePhotoInfo Detect(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return new(false, 0);

        try
        {
            var fileLength = new FileInfo(filePath).Length;
            if (fileLength < 100) return new(false, 0);

            // 1. Try XMP-based detection from the file head
            var xmpResult = DetectFromXmp(filePath, fileLength);
            if (xmpResult.IsLivePhoto)
            {
                // Validate the offset points to a real ftyp box.
                // Some Samsung HEIF files report a tiny Container:Directory
                // Item:Length that points to the SEF trailer, not the video.
                if (ValidateFtypAtOffset(filePath, fileLength, xmpResult.EmbeddedVideoOffsetFromEnd))
                {
                    return xmpResult;
                }
                // XMP says live photo but offset is bad — fall through
            }

            // 2. Fallback: scan tail for Samsung marker or MP4 ftyp after JPEG EOI
            var tailResult = DetectFromTail(filePath, fileLength);
            if (tailResult.IsLivePhoto) return tailResult;

            // 3. Samsung deep scan: search for MotionPhoto_Data marker
            // in the file (handles HEIF/HEIC without XMP)
            var samsungResult = DetectFromSamsungMarkerScan(filePath, fileLength);
            return samsungResult;
        }
        catch { }

        return new(false, 0);
    }


    /// <summary>
    /// Extracts the embedded video to a temp .mp4 file.
    /// Returns its real platform path for handing to an external player, or <c>null</c> on failure.
    /// </summary>
    public static async Task<string?> ExtractEmbeddedVideoAsync(
        string imagePath, long offsetFromEnd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || offsetFromEnd <= 0)
            return null;

        try
        {
            var fileLength = new FileInfo(imagePath).Length;
            var videoStart = fileLength - offsetFromEnd;
            if (videoStart < 0) return null;

            // Read and validate MP4 signature
            using var fs = new FileStream(imagePath, FileMode.Open,
                FileAccess.Read, FileShare.Read, 8192,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            fs.Seek(videoStart, SeekOrigin.Begin);

            // Validate: bytes 4..7 should be "ftyp"
            var header = new byte[8];
            var bytesRead = await fs.ReadAsync(header.AsMemory(0, 8), ct).ConfigureAwait(false);
            if (bytesRead < 8) return null;

            if (!header.AsSpan(4, 4).SequenceEqual(FtypSignature))
                return null;

            // Seek back and extract full video
            fs.Seek(videoStart, SeekOrigin.Begin);
            var videoLength = (int)offsetFromEnd;
            var tempPath = BHelper.ConfigDir(Dir.Temporary, $"ig_livephoto_{Guid.NewGuid():N}.mp4");

            using (var outFs = new FileStream(tempPath, FileMode.Create,
                FileAccess.Write, FileShare.None, 8192,
                FileOptions.Asynchronous))
            {
                var buffer = new byte[81920];
                var remaining = videoLength;

                while (remaining > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    var toRead = Math.Min(buffer.Length, remaining);
                    bytesRead = await fs.ReadAsync(buffer.AsMemory(0, toRead), ct).ConfigureAwait(false);
                    if (bytesRead == 0) break;

                    await outFs.WriteAsync(buffer.AsMemory(0, bytesRead), ct).ConfigureAwait(false);
                    remaining -= bytesRead;
                }
            }

            // cleanup deletes through the path we wrote; the caller gets the form an external player sees
            RegisterTempFile(tempPath);
            return BHelper.GetRealPlatformPath(tempPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }

        return null;
    }


    /// <summary>
    /// Cleans up all temp video files created during the session.
    /// </summary>
    public static void CleanupTempFiles()
    {
        string[] files;
        lock (_tempFilesLock)
        {
            files = [.. _tempFiles];
            _tempFiles.Clear();
        }

        foreach (var file in files)
        {
            try { File.Delete(file); }
            catch { }
        }
    }

    #endregion // Public Methods



    #region Private methods

    /// <summary>
    /// Reads the file head and searches for XMP motion photo markers
    /// via raw string matching (no XML DOM). Uses adaptive read sizes:
    /// 64KB for JPEG, 2MB for HEIF/HEIC.
    /// </summary>
    private static LivePhotoInfo DetectFromXmp(string filePath, long fileLength)
    {
        var ext = Path.GetExtension(filePath);
        var maxHead = _heifExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase)
            ? HEAD_READ_SIZE_HEIF
            : HEAD_READ_SIZE_JPEG;
        var readSize = (int)Math.Min(maxHead, fileLength);
        var headBuffer = new byte[readSize];

        using (var fs = new FileStream(filePath, FileMode.Open,
            FileAccess.Read, FileShare.Read,
            Math.Min(readSize, 81920), FileOptions.SequentialScan))
        {
            _ = fs.Read(headBuffer);
        }

        // XMP is typically ASCII/UTF-8 embedded in JPEG APP1 or HEIF meta box
        var headText = Encoding.UTF8.GetString(headBuffer);

        // Google Motion Photo: GCamera:MotionPhoto="1" or Camera:MotionPhoto="1"
        // Older Google format: GCamera:MicroVideo="1"
        var hasMotionPhotoMarker =
            headText.Contains("MotionPhoto=\"1\"", StringComparison.Ordinal)
            || headText.Contains("MotionPhoto='1'", StringComparison.Ordinal)
            || headText.Contains("MicroVideo=\"1\"", StringComparison.Ordinal)
            || headText.Contains("MicroVideo='1'", StringComparison.Ordinal);

        if (hasMotionPhotoMarker)
        {
            var offset = ParseVideoOffset(headText, fileLength);
            if (offset > 0)
            {
                return new LivePhotoInfo(true, offset);
            }

            // Have the marker but no valid offset — try tail scan
            var tailResult = DetectFromTail(filePath, fileLength);
            if (tailResult.IsLivePhoto) return tailResult;

            return new(false, 0);
        }

        return new(false, 0);
    }


    /// <summary>
    /// Validates that the file contains a valid MP4 ftyp box
    /// at the position indicated by <paramref name="offsetFromEnd"/>.
    /// </summary>
    private static bool ValidateFtypAtOffset(string filePath, long fileLength, long offsetFromEnd)
    {
        var videoStart = fileLength - offsetFromEnd;
        if (videoStart < 0 || videoStart + 8 > fileLength) return false;

        var header = new byte[8];
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);
        fs.Seek(videoStart, SeekOrigin.Begin);

        if (fs.Read(header) < 8) return false;

        return header.AsSpan(4, 4).SequenceEqual(FtypSignature);
    }


    /// <summary>
    /// Parses the video offset from XMP text. Looks for <c>GCamera:MotionPhotoVideoOffset="N"</c>,
    /// <c>Camera:MicroVideoOffset="N"</c>, or
    /// <c>Container:Directory</c> with <c>Item:Length</c> for the video item.
    /// </summary>
    private static long ParseVideoOffset(string xmpText, long fileLength)
    {
        // Try GCamera:MotionPhotoVideoOffset="N"
        var offset = ExtractNumericAttribute(xmpText, "MotionPhotoVideoOffset=\"");
        if (offset <= 0)
        {
            // Fallback: older Google format MicroVideoOffset
            offset = ExtractNumericAttribute(xmpText, "MicroVideoOffset=\"");
        }

        if (offset > 0 && offset < fileLength)
        {
            return offset;
        }

        // Fallback: Container:Directory format (Samsung / newer Google).
        // The video item has Item:Semantic="MotionPhoto" and Item:Length="N"
        // on the same <Container:Item .../> element. N is the video byte length
        // (= offset from end of file).
        offset = ParseVideoOffsetFromContainerDirectory(xmpText);
        if (offset > 0 && offset < fileLength)
        {
            return offset;
        }

        return 0;
    }


    /// <summary>
    /// Parses the video byte length from a <c>Container:Directory</c> XMP structure.
    /// Searches for the <c>Container:Item</c> element that contains
    /// <c>Item:Semantic="MotionPhoto"</c> and reads its <c>Item:Length</c> attribute.
    /// </summary>
    private static long ParseVideoOffsetFromContainerDirectory(string xmpText)
    {
        // Find the video item marker
        const string semanticMarker = "Item:Semantic=\"MotionPhoto\"";
        var semanticIdx = xmpText.IndexOf(semanticMarker, StringComparison.Ordinal);
        if (semanticIdx < 0) return 0;

        // Narrow to the enclosing XML element: find '<' before and '/>' or '>' after.
        // This ensures we only read Item:Length from the same Container:Item element.
        var elemStart = xmpText.LastIndexOf('<', semanticIdx);
        if (elemStart < 0) elemStart = Math.Max(0, semanticIdx - 500);

        var elemEnd = xmpText.IndexOf("/>", semanticIdx, StringComparison.Ordinal);
        if (elemEnd < 0) elemEnd = xmpText.IndexOf('>', semanticIdx);
        if (elemEnd < 0) elemEnd = Math.Min(xmpText.Length, semanticIdx + 500);
        else elemEnd += 2; // include the "/>" or ">"

        var element = xmpText.AsSpan(elemStart, elemEnd - elemStart);

        const string lengthPrefix = "Item:Length=\"";
        var lengthIdx = element.IndexOf(lengthPrefix.AsSpan(), StringComparison.Ordinal);
        if (lengthIdx < 0) return 0;

        var valueStart = lengthIdx + lengthPrefix.Length;
        var remaining = element[valueStart..];
        var quoteIdx = remaining.IndexOf('"');
        if (quoteIdx <= 0) return 0;

        var valueSpan = remaining[..quoteIdx];
        if (long.TryParse(valueSpan, NumberStyles.None, CultureInfo.InvariantCulture, out var length)
            && length > 0)
        {
            return length;
        }

        return 0;
    }


    /// <summary>
    /// Extracts a numeric value from an attribute like <c>SomeAttr="12345"</c>.
    /// </summary>
    private static long ExtractNumericAttribute(string text, string prefix)
    {
        var idx = text.IndexOf(prefix, StringComparison.Ordinal);
        if (idx < 0) return 0;

        idx += prefix.Length;
        var endIdx = text.IndexOf('"', idx);
        if (endIdx < 0 || endIdx == idx) return 0;

        var valueSpan = text.AsSpan(idx, endIdx - idx);
        if (long.TryParse(valueSpan, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return 0;
    }


    /// <summary>
    /// Scans the tail of the file for Samsung <c>MotionPhoto_Data</c> marker
    /// or an MP4 <c>ftyp</c> box after the JPEG EOI marker.
    /// </summary>
    private static LivePhotoInfo DetectFromTail(string filePath, long fileLength)
    {
        var readSize = (int)Math.Min(TAIL_READ_SIZE, fileLength);
        var tailBuffer = new byte[readSize];
        var tailStart = fileLength - readSize;

        using (var fs = new FileStream(filePath, FileMode.Open,
            FileAccess.Read, FileShare.Read, readSize, FileOptions.SequentialScan))
        {
            fs.Seek(tailStart, SeekOrigin.Begin);
            _ = fs.Read(tailBuffer.AsSpan());
        }

        // 1. Look for Samsung MotionPhoto_Data marker
        var samsungIdx = tailBuffer.AsSpan().IndexOf(SamsungMotionPhotoDataMarker);
        if (samsungIdx >= 0)
        {
            var result = TryParseSamsungMotionPhotoData(tailBuffer, samsungIdx, tailStart, fileLength);
            if (result.IsLivePhoto) return result;
        }

        // 2. Look for MP4 ftyp box in the tail
        var ftypIdx = FindFtypBox(tailBuffer);
        if (ftypIdx >= 0)
        {
            // The ftyp box starts 4 bytes before the "ftyp" text (box size field)
            var boxStart = ftypIdx - 4;
            if (boxStart >= 0)
            {
                var offsetFromEnd = readSize - boxStart;
                if (offsetFromEnd > 0 && offsetFromEnd < fileLength)
                {
                    return new LivePhotoInfo(true, offsetFromEnd);
                }
            }
        }

        return new(false, 0);
    }


    /// <summary>
    /// Finds the first ftyp box in the buffer.
    /// Returns the index of the "ftyp" text (not the box start), or -1.
    /// </summary>
    private static int FindFtypBox(ReadOnlySpan<byte> buffer)
    {
        var idx = 0;
        while (idx + 4 <= buffer.Length)
        {
            var pos = buffer[idx..].IndexOf(FtypSignature);
            if (pos < 0) return -1;

            var absPos = idx + pos;

            // Validate: the 4 bytes before "ftyp" should be the box size (>= 8, < 1024)
            if (absPos >= 4)
            {
                var boxSize = (buffer[absPos - 4] << 24)
                    | (buffer[absPos - 3] << 16)
                    | (buffer[absPos - 2] << 8)
                    | buffer[absPos - 1];

                if (boxSize >= 8 && boxSize < 1024)
                {
                    return absPos;
                }
            }

            idx = absPos + 1;
        }

        return -1;
    }


    /// <summary>
    /// Scans the file backwards for Samsung <c>MotionPhoto_Data</c> marker
    /// followed by an ftyp box. Handles Samsung HEIF/HEIC motion photos
    /// that lack XMP in the file head.
    /// </summary>
    private static LivePhotoInfo DetectFromSamsungMarkerScan(string filePath, long fileLength)
    {
        // Only scan files up to 50MB to avoid excessive I/O
        if (fileLength > 50 * 1024 * 1024) return new(false, 0);

        // Scan backwards in chunks, with overlap for markers spanning boundaries
        const int overlap = 32;
        var buffer = new byte[SCAN_CHUNK_SIZE + overlap];

        using var fs = new FileStream(filePath, FileMode.Open,
            FileAccess.Read, FileShare.Read, 81920, FileOptions.SequentialScan);

        // Start scanning from the end (skip the last TAIL_READ_SIZE already checked)
        var scanEnd = fileLength - TAIL_READ_SIZE;
        if (scanEnd < 0) return new(false, 0);

        var pos = scanEnd;

        while (pos > 0)
        {
            var chunkStart = Math.Max(0, pos - SCAN_CHUNK_SIZE);
            var chunkSize = (int)(pos + overlap - chunkStart);
            chunkSize = (int)Math.Min(chunkSize, fileLength - chunkStart);

            fs.Seek(chunkStart, SeekOrigin.Begin);
            var bytesRead = fs.Read(buffer, 0, chunkSize);
            if (bytesRead == 0) break;

            var span = buffer.AsSpan(0, bytesRead);
            var markerIdx = span.IndexOf(SamsungMotionPhotoDataMarker);

            if (markerIdx >= 0)
            {
                var result = TryParseSamsungMotionPhotoData(
                    buffer.AsSpan(0, bytesRead), markerIdx, chunkStart, fileLength);
                if (result.IsLivePhoto) return result;
            }

            pos = chunkStart;
            if (chunkStart == 0) break;
        }

        return new(false, 0);
    }


    /// <summary>
    /// Tries to parse video offset from a Samsung <c>MotionPhoto_Data</c> marker.
    /// Handles two formats:
    /// <list type="bullet">
    /// <item>Versionless: <c>MotionPhoto_Data</c> followed directly by ftyp box</item>
    /// <item>mpv2: <c>MotionPhoto_Data</c> + "mpv2" + 4-byte BE video start + 4-byte BE video length</item>
    /// </list>
    /// </summary>
    /// <param name="buffer">Buffer containing the marker.</param>
    /// <param name="markerIdx">Index of <c>MotionPhoto_Data</c> in the buffer.</param>
    /// <param name="bufferStartInFile">Absolute file offset where the buffer starts.</param>
    /// <param name="fileLength">Total file length.</param>
    private static LivePhotoInfo TryParseSamsungMotionPhotoData(
        ReadOnlySpan<byte> buffer, int markerIdx, long bufferStartInFile, long fileLength)
    {
        var afterMarker = markerIdx + SamsungMotionPhotoDataMarker.Length;
        if (afterMarker >= buffer.Length) return new(false, 0);

        // Format 1 (versionless): ftyp box directly after marker
        if (afterMarker + 32 <= buffer.Length)
        {
            var searchWindow = buffer.Slice(afterMarker, Math.Min(32, buffer.Length - afterMarker));
            var ftypPos = searchWindow.IndexOf(FtypSignature);

            if (ftypPos >= 0 && ftypPos >= 4)
            {
                var videoStartInFile = bufferStartInFile + afterMarker + ftypPos - 4;
                var offsetFromEnd = fileLength - videoStartInFile;

                if (offsetFromEnd > 0)
                {
                    return new LivePhotoInfo(true, offsetFromEnd);
                }
            }
        }

        // Format 2 (mpv2): "mpv2" tag + 4-byte BE video start + 4-byte BE video length
        if (afterMarker + 12 <= buffer.Length
            && buffer.Slice(afterMarker, 4).SequenceEqual(SamsungMpv2Tag))
        {
            var videoStartFromBof =
                ((long)buffer[afterMarker + 4] << 24)
                | ((long)buffer[afterMarker + 5] << 16)
                | ((long)buffer[afterMarker + 6] << 8)
                | buffer[afterMarker + 7];

            var videoLength =
                ((long)buffer[afterMarker + 8] << 24)
                | ((long)buffer[afterMarker + 9] << 16)
                | ((long)buffer[afterMarker + 10] << 8)
                | buffer[afterMarker + 11];

            if (videoStartFromBof > 0 && videoLength > 0
                && videoStartFromBof + videoLength <= fileLength)
            {
                var offsetFromEnd = fileLength - videoStartFromBof;
                return new LivePhotoInfo(true, offsetFromEnd);
            }
        }

        return new(false, 0);
    }


    /// <summary>
    /// Registers a temp file for cleanup on process exit.
    /// </summary>
    private static void RegisterTempFile(string path)
    {
        lock (_tempFilesLock)
        {
            _tempFiles.Add(path);

            if (!_exitHandlerRegistered)
            {
                _exitHandlerRegistered = true;
                AppDomain.CurrentDomain.ProcessExit += (_, _) => CleanupTempFiles();
            }
        }
    }

    #endregion // Private methods
}

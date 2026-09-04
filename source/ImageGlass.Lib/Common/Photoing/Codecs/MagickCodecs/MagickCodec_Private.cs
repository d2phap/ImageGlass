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
using Avalonia;
using ImageMagick;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text.RegularExpressions;

namespace ImageGlass.Common.Photoing;

public static partial class MagickCodec
{
    [GeneratedRegex(@"(^data\:(?<type>image\/[a-z\+\-]*);base64,)?(?<data>[a-zA-Z0-9\+\/\=]+)$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled, "en-US")]
    private static partial Regex CreateBase64DataUriRegex__();



    /// <summary>
    /// Processes single-frame Magick image.
    /// Returns thumbnail image if requested.
    /// </summary>
    /// <param name="refImgM">Input Magick image to process</param>
    private static MagickImage? ProcessMagickImage__(MagickImage refImgM,
        PhotoReadOptions options, PhotoMetadata meta, bool requestThumbnail)
    {
        IMagickImage? thumbM = null;

        // Ping exposes no ICC for JXL, so metadata can arrive marked SDR for a PQ image; correct
        // it from this fully-read copy before the transform below, which keys off meta.IsHdr.
        if (!meta.IsHdr) ApplyIccHdrInfo__(meta, refImgM);


        // Use embedded thumbnails if specified
        if (requestThumbnail && meta.ExifProfile != null && options.OnlyLoadNonRawPreview)
        {
            // Fetch the embedded thumbnail
            thumbM = meta.ExifProfile.CreateThumbnail();
            if (thumbM != null
                && thumbM.Width > options.PreviewMinWidth
                && thumbM.Height > options.PreviewMinHeight)
            {
                if (options.CorrectRotation) thumbM.AutoOrient();

                ApplySizeSettings__(thumbM, options);
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
            ApplySizeSettings__(refImgM, options);

            // for HEIC/HEIF, PreserveOrientation must be false
            // see https://github.com/d2phap/ImageGlass/issues/1928
            if (options.CorrectRotation) refImgM.AutoOrient();


            // if always apply color profile
            // or only apply color profile if there is an embedded profile.
            // Skip for HDR images: tone mapping handles color space conversion;
            // applying the monitor profile here would cause a double transform.
            if (!meta.IsHdr
                && (Core.Config.EnableAlwaysApplyColorProfile || meta.MagickColorProfile is not null))
            {
                if (GetColorProfileByName(Core.Config.ColorProfile) is { } destIccProfile)
                {
                    refImgM.TransformColorSpace(
                        //set default color profile to sRGB
                        meta.MagickColorProfile ?? ColorProfiles.SRGB,
                        destIccProfile);
                }
            }


            // Make sure the output isn't CMYK. A CMYK dest profile (soft-proof) or a CMYK
            // source would otherwise render as an inverted image when read back as RGBA.
            // The built-in (non-ICC) cast keeps the expected print-like tint.
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
    private static void ApplySizeSettings__(IMagickImage imgM, PhotoReadOptions options)
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
    /// Applies changes from <paramref name="transform"/>.
    /// </summary>
    private static void TransformImage__(IMagickImage imgM, PhotoTransform? transform = null)
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
    /// Gets maximum image dimention.
    /// </summary>
    private static Size GetMaxImageRenderSize__(uint srcWidth, uint srcHeight, uint maxSize)
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
        var newW = srcWidth * scale;
        var newH = srcHeight * scale;

        return new Size(newW, newH);
    }


    /// <summary>
    /// Gets <see cref="MagickFormat"/> from mime type.
    /// </summary>
    private static MagickFormat ConvertMimeTypeToMagickFormat__(string? mimeType)
    {
        return mimeType switch
        {
            "image/avif" => MagickFormat.Avif,
            "image/bmp" => MagickFormat.Bmp,
            "image/gif" => MagickFormat.Gif,
            "image/tiff" => MagickFormat.Tiff,
            "image/jpeg" => MagickFormat.Jpeg,
            "image/svg+xml" => MagickFormat.Rsvg,
            "image/x-icon" => MagickFormat.Ico,
            "image/x-portable-anymap" => MagickFormat.Pnm,
            "image/x-portable-bitmap" => MagickFormat.Pbm,
            "image/x-portable-graymap" => MagickFormat.Pgm,
            "image/x-portable-pixmap" => MagickFormat.Ppm,
            "image/x-xbitmap" => MagickFormat.Xbm,
            "image/x-xpixmap" => MagickFormat.Xpm,
            "image/x-cmu-raster" => MagickFormat.Ras,
            _ => MagickFormat.Png,
        };
    }


    /// <summary>
    /// Get EXIF value.
    /// </summary>
    private static T? GetExifValue__<T>(IExifProfile? profile, ExifTag<T> tag, T? defaultValue = default)
    {
        if (profile == null) return default;

        var exifValue = profile.GetValue(tag);
        if (exifValue == null) return defaultValue;

        return exifValue.Value;
    }


    /// <summary>
    /// Parses the color encoding from a JPEG XL codestream header.
    /// Handles both bare codestream (starts with 0xFF0A) and ISOBMFF container format.
    /// Returns JXL (Primaries, TransferFunction) enum values (CICP-compatible sparse), or <c>null</c>.
    /// JXL Primaries: 1=sRGB, 2=Custom, 9=BT.2100, 11=P3 (matches JXL_PRIMARIES_*).
    /// JXL TransferFunction: 1=BT.709, 2=Unknown, 8=Linear, 13=sRGB, 16=PQ, 17=DCI, 18=HLG.
    /// </summary>
    private static (int Primaries, int TransferCharacteristics)? ParseJxlColorEncoding(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        const int HEADER_SIZE = 64 * 1024;

        try
        {
            using var fs = File.OpenRead(filePath);
            var bufSize = (int)Math.Min(fs.Length, HEADER_SIZE);
            var buf = new byte[bufSize];
            fs.ReadExactly(buf);
            if (buf.Length < 4) return null;

            int csOffset; // codestream start offset

            // bare codestream: starts with 0xFF 0x0A
            if (buf[0] == 0xFF && buf[1] == 0x0A)
            {
                csOffset = 0;
            }
            // ISOBMFF container: detect "ftypjxl " brand signature
            else if (buf.AsSpan().IndexOf("ftypjxl "u8) >= 0
                || buf.AsSpan().IndexOf("ftypJXL "u8) >= 0)
            {
                // scan for 'jxlc' (complete codestream) or 'jxlp' (partial codestream) box
                csOffset = -1;
                ReadOnlySpan<byte> jxlcSig = "jxlc"u8;
                ReadOnlySpan<byte> jxlpSig = "jxlp"u8;
                for (int i = 0; i <= buf.Length - 8; i++)
                {
                    if (buf.AsSpan(i, 4).SequenceEqual(jxlcSig))
                    {
                        csOffset = i + 4; // data starts after box type
                        break;
                    }
                    if (buf.AsSpan(i, 4).SequenceEqual(jxlpSig))
                    {
                        // jxlp data starts with 4-byte sequence number before codestream bytes
                        csOffset = i + 8;
                        break;
                    }
                }
                if (csOffset < 0) return null;
            }
            else
            {
                return null;
            }

            // verify codestream signature
            if (csOffset + 2 > buf.Length) return null;
            if (buf[csOffset] != 0xFF || buf[csOffset + 1] != 0x0A) return null;

            // --- JXL bit reader (LSB-first within bytes) ---
            int bitPos = (csOffset + 2) * 8;

            uint ReadBits(int n)
            {
                uint result = 0;
                for (int i = 0; i < n; i++)
                {
                    int byteIdx = bitPos >> 3;
                    int bitIdx = bitPos & 7;
                    if (byteIdx >= buf.Length) return result;
                    result |= (uint)((buf[byteIdx] >> bitIdx) & 1) << i;
                    bitPos++;
                }
                return result;
            }

            bool ReadBool() => ReadBits(1) == 1;

            // U32 with distribution: 4 entries of (base, extraBits)
            uint ReadU32(int b0, int n0, int b1, int n1, int b2, int n2, int b3, int n3)
            {
                return (int)ReadBits(2) switch
                {
                    0 => (uint)b0 + ReadBits(n0),
                    1 => (uint)b1 + ReadBits(n1),
                    2 => (uint)b2 + ReadBits(n2),
                    _ => (uint)b3 + ReadBits(n3),
                };
            }

            // Enum = U32(0, 1, 2+u(4), 18+u(6))
            uint ReadEnum() => ReadU32(0, 0, 1, 0, 2, 4, 18, 6);

            void SkipSizeHeader()
            {
                if (ReadBool()) // small
                {
                    ReadBits(5); // height_m1
                    if (ReadBits(3) == 0) ReadBits(5); // width_m1 when ratio==0
                }
                else
                {
                    ReadU32(1, 9, 1, 13, 1, 18, 1, 30); // height
                    if (ReadBits(3) == 0) ReadU32(1, 9, 1, 13, 1, 18, 1, 30); // width
                }
            }

            void SkipCustomXy()
            {
                // U32(Bits(19), BitsOffset(19,524288), BitsOffset(20,1048576), BitsOffset(21,2097152))
                ReadU32(0, 19, 524288, 19, 1048576, 20, 2097152, 21); // x
                ReadU32(0, 19, 524288, 19, 1048576, 20, 2097152, 21); // y
            }

            void SkipPreviewHeader()
            {
                var div8 = ReadBool();
                if (div8)
                {
                    // U32(Val(16), Val(32), BitsOffset(5,1), BitsOffset(9,33))
                    ReadU32(16, 0, 32, 0, 1, 5, 33, 9); // ysize_div8
                    var ratio = ReadBits(3);
                    if (ratio == 0) ReadU32(16, 0, 32, 0, 1, 5, 33, 9); // xsize_div8
                }
                else
                {
                    // U32(BitsOffset(6,1), BitsOffset(8,65), BitsOffset(10,321), BitsOffset(12,1345))
                    ReadU32(1, 6, 65, 8, 321, 10, 1345, 12); // ysize
                    var ratio = ReadBits(3);
                    if (ratio == 0) ReadU32(1, 6, 65, 8, 321, 10, 1345, 12); // xsize
                }
            }

            void SkipBitDepth()
            {
                if (ReadBool()) // floating_point_sample
                {
                    ReadU32(32, 0, 16, 0, 24, 0, 1, 6); // bits_per_sample
                    ReadBits(4);                          // exponent_bits (Bits(4))
                }
                else
                {
                    ReadU32(8, 0, 10, 0, 12, 0, 1, 6);   // bits_per_sample
                }
            }

            void SkipNameString()
            {
                // U32(Val(0), Bits(4), BitsOffset(5,16), BitsOffset(10,48))
                var nameLen = ReadU32(0, 0, 0, 4, 16, 5, 48, 10);
                ReadBits((int)(nameLen * 8)); // each char is 8 bits
            }

            // ExtraChannelInfo: type, bit_depth, dim_shift, name, conditionals
            // ExtraChannel: 0=Alpha, 1=Depth, 2=SpotColor, 3=SelectionMask,
            //   4=Black, 5=CFA, 6=Thermal
            void SkipExtraChannelInfo()
            {
                var type = ReadEnum();
                SkipBitDepth();
                ReadU32(0, 0, 3, 0, 4, 0, 1, 3); // dim_shift
                SkipNameString();
                if (type == 0) ReadBool();              // alpha_associated
                if (type == 2) ReadBits(16 * 4);        // spot_color: 4× F16
                if (type == 5) ReadU32(1, 0, 0, 2, 3, 4, 19, 8); // cfa_channel
            }

            // --- Parse SizeHeader ---
            SkipSizeHeader();

            // --- Parse ImageMetadata ---
            if (ReadBool()) return null; // all_default -> sRGB

            var extraFields = ReadBool();
            if (extraFields)
            {
                ReadBits(3); // orientation_minus_1
                if (ReadBool()) SkipSizeHeader();  // have_intrinsic_size
                if (ReadBool()) SkipPreviewHeader(); // have_preview
                if (ReadBool())                    // have_animation
                {
                    ReadU32(100, 0, 1000, 0, 1, 10, 1, 30); // tps_numerator
                    ReadU32(1, 0, 1001, 0, 1, 8, 1, 10);     // tps_denominator
                    ReadU32(0, 0, 0, 3, 0, 16, 0, 32);       // num_loops
                    ReadBool();                                // have_timecodes
                }
            }

            // BitDepth (no AllDefault — always starts with floating_point_sample Bool)
            SkipBitDepth();

            // modular_16_bit_buffer_sufficient (always present)
            ReadBool();

            // num_extra_channels (always present)
            var numExtraChannels = ReadU32(0, 0, 1, 0, 2, 4, 1, 12);
            if (numExtraChannels != 0)
            {
                for (uint i = 0; i < numExtraChannels; i++)
                {
                    if (ReadBool()) continue; // ExtraChannelInfo all_default
                    SkipExtraChannelInfo();    // parse non-default extra channel
                }
            }

            // xyb_encoded
            ReadBool();

            // --- Parse ColorEncoding ---
            if (ReadBool()) return null; // all_default -> sRGB
            if (ReadBool()) return null; // want_icc -> can't parse structure

            var colorSpace = (int)ReadEnum(); // 0=RGB, 1=Gray, 2=XYB, 3=Unknown

            // XYB has implicit transfer function (gamma 1/3), not serialized
            if (colorSpace == 2) return null;

            bool hasPrimaries = colorSpace is 0 or 3; // RGB or Unknown

            int primaries = -1;
            // White point (always present for non-XYB)
            var whitePoint = (int)ReadEnum();
            if (whitePoint == 2) SkipCustomXy(); // custom white point

            if (hasPrimaries)
            {
                primaries = (int)ReadEnum();
                if (primaries == 2) // custom primaries
                {
                    SkipCustomXy(); // red
                    SkipCustomXy(); // green
                    SkipCustomXy(); // blue
                }
            }

            // CustomTransferFunction (not implicit since we excluded XYB above)
            if (ReadBool()) return null; // have_gamma -> not a named HDR TF
            var transferFunction = (int)ReadEnum();

            return (primaries, transferFunction);
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Detects HDR gain map by scanning the file for XMP metadata markers.
    /// Covers JPEG Ultra HDR, HEIC/AVIF gain maps, and ISO 21496-1 standard gain maps.
    /// </summary>
    private static bool DetectGainMap(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

        // XMP metadata with gain map info is typically in the first 256 KB
        const int SCAN_SIZE = 256 * 1024;

        try
        {
            using var fs = File.OpenRead(filePath);
            var bufSize = (int)Math.Min(fs.Length, SCAN_SIZE);
            var buf = new byte[bufSize];
            fs.ReadExactly(buf);

            var span = buf.AsSpan();

            // Adobe/Google/ISO 21496-1 gain map namespace
            if (span.IndexOf("hdrgm:Version"u8) >= 0) return true;

            // Apple HDR gain map
            if (span.IndexOf("HDRGainMap"u8) >= 0) return true;

            return false;
        }
        catch { }

        return false;
    }


    /// <summary>
    /// Upgrades <paramref name="meta"/> to HDR when the image's ICC profile says so.
    /// </summary>
    private static void ApplyIccHdrInfo__(PhotoMetadata meta, IMagickImage img)
    {
        if (img.GetProfile("icc") is not { } profile) return;
        if (ParseCicpFromIcc(profile.ToReadOnlySpan()) is not (var primaries, var transfer)) return;

        meta.HdrTransferFn = transfer switch
        {
            16 => HdrTransferFunction.PQ,
            18 => HdrTransferFunction.HLG,
            _ => meta.HdrTransferFn,
        };

        if (!meta.IsWideGamut && primaries is 9 or 11 or 12)
        {
            meta.IsWideGamut = true;
        }
    }


    /// <summary>
    /// Reads the CICP values out of an ICC profile's <c>cicp</c> tag (ICC.1:2022, 10.4).
    /// </summary>
    private static (int ColorPrimaries, int TransferCharacteristics)? ParseCicpFromIcc(ReadOnlySpan<byte> icc)
    {
        // 128-byte header, then a uint32 tag count, then 12 bytes per tag table entry
        const int TAG_COUNT_OFFSET = 128;
        const int TAG_TABLE_OFFSET = 132;
        const int TAG_ENTRY_SIZE = 12;
        const int CICP_TAG_SIZE = 12;

        if (icc.Length < TAG_TABLE_OFFSET) return null;

        var tagCount = BinaryPrimitives.ReadUInt32BigEndian(icc[TAG_COUNT_OFFSET..]);

        // a sane profile has tens of tags; anything wilder means the bytes are not an ICC profile
        if (tagCount == 0 || tagCount > 256) return null;

        for (var i = 0; i < (int)tagCount; i++)
        {
            var entry = TAG_TABLE_OFFSET + (i * TAG_ENTRY_SIZE);
            if (entry + TAG_ENTRY_SIZE > icc.Length) return null;

            if (!icc.Slice(entry, 4).SequenceEqual("cicp"u8)) continue;

            var dataOffset = BinaryPrimitives.ReadUInt32BigEndian(icc[(entry + 4)..]);
            var dataSize = BinaryPrimitives.ReadUInt32BigEndian(icc[(entry + 8)..]);
            if (dataSize < CICP_TAG_SIZE || dataOffset + CICP_TAG_SIZE > (uint)icc.Length) return null;

            var data = icc.Slice((int)dataOffset, CICP_TAG_SIZE);

            // the tag data repeats its own signature, then 4 reserved bytes
            if (!data[..4].SequenceEqual("cicp"u8)) return null;

            return (data[8], data[9]);
        }

        return null;
    }


    /// <summary>
    /// Parses CICP (Coding-Independent Code Points) from ISOBMFF containers (AVIF, HEIF)
    /// by searching for the 'colr' box with 'nclx' colour type.
    /// </summary>
    /// <returns>
    /// A tuple of (ColorPrimaries, TransferCharacteristics) CICP values, or <c>null</c>.
    /// </returns>
    private static (int ColorPrimaries, int TransferCharacteristics)? ParseCicpFromIsobmff(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        // ISOBMFF 'colr' box with 'nclx' type layout:
        //   [4 bytes size][4 bytes 'colr'][4 bytes 'nclx']
        //   [2 bytes color_primaries][2 bytes transfer_char][2 bytes matrix_coeff][1 byte range]
        // We search for the 8-byte signature 'colrnclx' in the first 256KB.
        const int SEARCH_SIZE = 256 * 1024;
        ReadOnlySpan<byte> signature = "colrnclx"u8;

        try
        {
            using var fs = File.OpenRead(filePath);
            var bufSize = (int)Math.Min(fs.Length, SEARCH_SIZE);
            var buf = new byte[bufSize];
            fs.ReadExactly(buf);

            var span = buf.AsSpan();
            // Need at least 8 (signature) + 4 (primaries + transfer) = 12 bytes after match start
            for (int i = 0; i <= span.Length - 12; i++)
            {
                if (span.Slice(i, 8).SequenceEqual(signature))
                {
                    var primaries = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(i + 8));
                    var transfer = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(i + 10));
                    return (primaries, transfer);
                }
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Parses CICP from a PNG file's cICP chunk.
    /// PNG cICP chunk layout: [4 bytes length][4 bytes "cICP"][1 byte primaries][1 byte transfer][1 byte matrix][1 byte range][4 bytes CRC]
    /// </summary>
    private static (int ColorPrimaries, int TransferCharacteristics)? ParseCicpFromPng(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        const int SEARCH_SIZE = 64 * 1024;
        ReadOnlySpan<byte> chunkType = "cICP"u8;

        try
        {
            using var fs = File.OpenRead(filePath);

            // verify PNG signature (8 bytes)
            Span<byte> sig = stackalloc byte[8];
            if (fs.Read(sig) < 8) return null;
            ReadOnlySpan<byte> pngSig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
            if (!sig.SequenceEqual(pngSig)) return null;

            var bufSize = (int)Math.Min(fs.Length, SEARCH_SIZE);
            var buf = new byte[bufSize];
            fs.Position = 0;
            fs.ReadExactly(buf);

            var span = buf.AsSpan();
            // Walk PNG chunks: [4 bytes length][4 bytes type][data...][4 bytes CRC]
            int pos = 8; // after PNG signature
            while (pos + 12 <= span.Length)
            {
                int chunkLen = BinaryPrimitives.ReadInt32BigEndian(span.Slice(pos));
                if (chunkLen < 0) break;

                if (span.Slice(pos + 4, 4).SequenceEqual(chunkType))
                {
                    // cICP data: [primaries:1][transfer:1][matrix:1][range:1]
                    if (pos + 10 <= span.Length)
                    {
                        return ((int)span[pos + 8], (int)span[pos + 9]);
                    }
                }

                // advance to next chunk: length(4) + type(4) + data + crc(4)
                pos += 12 + chunkLen;
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Whether a parsed peak is usable: rejects a bad box match and the flat 10000 encoders emit
    /// when they do not know the real peak (the PQ container ceiling, not a grade).
    /// </summary>
    private static bool IsPlausiblePeakNits(double nits) => nits is >= 100d and < 10_000d;


    /// <summary>
    /// Parses the HDR10 content peak from ISOBMFF (AVIF, HEIF): the 'clli' box (MaxCLL), falling
    /// back to 'mdcv' mastering display max luminance as HDR tone mappers conventionally do.
    /// </summary>
    /// <returns>Peak luminance in nits, or <c>null</c> when the file declares none.</returns>
    private static double? ParseContentPeakFromIsobmff(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        const int SEARCH_SIZE = 256 * 1024;

        try
        {
            using var fs = File.OpenRead(filePath);
            var bufSize = (int)Math.Min(fs.Length, SEARCH_SIZE);
            var buf = new byte[bufSize];
            fs.ReadExactly(buf);

            var span = buf.AsSpan();

            // 'clli' body: [2 max_content_light_level][2 max_pic_average_light_level], already in nits
            var clliPos = FindSizedBox(span, "clli"u8, 4);
            if (clliPos >= 0)
            {
                var maxCll = (double)BinaryPrimitives.ReadUInt16BigEndian(span.Slice(clliPos));
                if (IsPlausiblePeakNits(maxCll)) return maxCll;
            }

            // 'mdcv' body: 12 bytes primaries + 4 white point, then max/min luminance in 0.0001 nits
            var mdcvPos = FindSizedBox(span, "mdcv"u8, 24);
            if (mdcvPos >= 0)
            {
                var maxMastering = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(mdcvPos + 16)) / 10_000d;
                if (IsPlausiblePeakNits(maxMastering)) return maxMastering;
            }
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Finds an ISOBMFF box by type, validating the size field in front of the 4CC so a bare
    /// 4-byte scan cannot match compressed pixel data.
    /// </summary>
    /// <returns>Offset of the box body, or <c>-1</c> when not found.</returns>
    private static int FindSizedBox(ReadOnlySpan<byte> span, ReadOnlySpan<byte> boxType, int bodySize)
    {
        var expectedSize = 8 + bodySize;
        var searchFrom = 0;

        while (searchFrom <= span.Length - 4)
        {
            var found = span.Slice(searchFrom).IndexOf(boxType);
            if (found < 0) return -1;

            var typePos = searchFrom + found;
            var bodyPos = typePos + 4;

            if (typePos >= 4 && bodyPos + bodySize <= span.Length
                && BinaryPrimitives.ReadUInt32BigEndian(span.Slice(typePos - 4)) == expectedSize)
            {
                return bodyPos;
            }

            searchFrom = typePos + 1;
        }

        return -1;
    }


    /// <summary>
    /// Parses the HDR10 content peak from a PNG 'cLLi' chunk (encoders use that spelling, not the
    /// spec's 'cLLI'), falling back to 'mDCV'. Both carry luminance in units of 0.0001 nits.
    /// </summary>
    /// <returns>Peak luminance in nits, or <c>null</c> when the file declares none.</returns>
    private static double? ParseContentPeakFromPng(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return null;

        const int SEARCH_SIZE = 64 * 1024;

        try
        {
            using var fs = File.OpenRead(filePath);

            Span<byte> sig = stackalloc byte[8];
            if (fs.Read(sig) < 8) return null;
            ReadOnlySpan<byte> pngSig = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
            if (!sig.SequenceEqual(pngSig)) return null;

            var bufSize = (int)Math.Min(fs.Length, SEARCH_SIZE);
            var buf = new byte[bufSize];
            fs.Position = 0;
            fs.ReadExactly(buf);

            var span = buf.AsSpan();
            double? masteringPeak = null;

            // walk chunks: [4 length][4 type][data...][4 CRC]
            var pos = 8;
            while (pos + 12 <= span.Length)
            {
                var chunkLen = BinaryPrimitives.ReadInt32BigEndian(span.Slice(pos));
                if (chunkLen < 0) break;

                var dataPos = pos + 8;
                var chunkType = span.Slice(pos + 4, 4);

                // content light level data: [4 MaxCLL][4 MaxFALL]
                if ((chunkType.SequenceEqual("cLLi"u8) || chunkType.SequenceEqual("cLLI"u8))
                    && dataPos + 4 <= span.Length)
                {
                    var maxCll = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(dataPos)) / 10_000d;
                    if (IsPlausiblePeakNits(maxCll)) return maxCll;
                }
                // mDCV data: 12 bytes primaries + 4 white point, then max/min luminance
                else if (chunkType.SequenceEqual("mDCV"u8) && dataPos + 20 <= span.Length)
                {
                    var maxMastering = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(dataPos + 16)) / 10_000d;
                    if (IsPlausiblePeakNits(maxMastering)) masteringPeak = maxMastering;
                }

                pos += 12 + chunkLen;
            }

            return masteringPeak;
        }
        catch { }

        return null;
    }

}

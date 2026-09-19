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
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides native Skia-based raster metadata loading and decoding.
/// </summary>
public sealed class SkiaCodecAdapter : PhDisposable, ICodec
{
    /// <summary>
    /// Built-in extensions handled by the Skia codec. Plugins can override
    /// individual extensions at runtime by claiming the same extension with a
    /// higher <c>DecodePriority</c>; this list is the safe default that ships
    /// with the host.
    /// </summary>
    private static readonly string[] _supportedExtensions =
        [".bmp", ".gif", ".gifv", ".jpg", ".jpeg", ".png", ".webp"];


    /// <inheritdoc/>
    public string CodecId { get; } = "skiasharp";

    /// <inheritdoc/>
    public string CodecName { get; } = "SkiaSharp";

    /// <inheritdoc/>
    public int MetadataPriority { get; } = 10;

    /// <inheritdoc/>
    public int DecodePriority { get; } = 100;

    /// <inheritdoc/>
    public int EncodePriority { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> DecodingExtensions => _supportedExtensions;

    /// <summary>
    /// Decode-only. Writing goes through <see cref="MagickCodecAdapter"/>, which is also what
    /// <c>SkiaCodec.SaveAsync</c> delegates to.
    /// </summary>
    public bool SupportsEncoding { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> EncodingExtensions => [];


    /// <inheritdoc/>
    public bool CanLoadMetadata(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || SvgCodec.IsSvgFile(filePath)) return false;

        try
        {
            return SkiaCodec.CanPing(filePath);
        }
        catch
        {
            return false;
        }
    }


    /// <inheritdoc/>
    public Task<PhotoMetadata> LoadMetadataAsync(string filePath,
        PhotoReadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return SkiaCodec.LoadMetadataAsync(filePath, options, cancellationToken);
    }


    /// <inheritdoc/>
    public bool CanDecode(PhotoMetadata metadata, CodecSelectionContext context)
    {
        if (metadata is null || metadata.IsVector) return false;
        // An unsupported dest profile (e.g. CMYK) routes single-frame decode to Magick so it
        // can bake the profile. But Magick decode is single-frame only, so animated images must
        // stay on Skia (which builds the animator) or they lose animation.
        if (!context.IsDestColorProfileSupported && metadata.FrameCount <= 1) return false;
        // no Skia equivalent of ExifProfile.CreateThumbnail(), so the non-RAW preview stays on Magick
        if (context.LoadOtherThumbnailOnly) return false;

        // Skia decodes a RAW embedded preview an order of magnitude faster than Magick
        if (context.LoadRawThumbnailOnly)
        {
            try
            {
                return SkiaCodec.CanReadRawPreview(metadata,
                    context.PreviewMinWidth, context.PreviewMinHeight);
            }
            catch
            {
                return false;
            }
        }

        if (Array.IndexOf(_supportedExtensions, metadata.FileExtension) < 0) return false;

        try
        {
            return SkiaCodec.CanRead(metadata);
        }
        catch
        {
            return false;
        }
    }


    /// <inheritdoc/>
    public async Task<CodecDecodeResult> DecodeAsync(PhotoMetadata metadata,
        PhotoReadOptions options,
        CodecSelectionContext context,
        CancellationToken cancellationToken = default)
    {
        using var output = await SkiaCodec.LoadAsync(metadata, options, cancellationToken).ConfigureAwait(false);

        return CodecDecodeResultFactory.FromSkiaOutput(CodecId, output, metadata);
    }


    /// <inheritdoc/>
    public bool CanEncode(string destFilePath, CodecEncodeContext context) => false;

    /// <inheritdoc/>
    public Task<CodecEncodeResult> EncodeAsync(CodecEncodeRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new CodecEncodeResult(false, true));

    /// <inheritdoc/>
    public bool CanEncodeMultiFrame(string destFilePath, CodecEncodeContext context) => false;

    /// <inheritdoc/>
    public Task<CodecEncodeResult> EncodeMultiFrameAsync(CodecMultiFrameEncodeRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new CodecEncodeResult(false, true));
}
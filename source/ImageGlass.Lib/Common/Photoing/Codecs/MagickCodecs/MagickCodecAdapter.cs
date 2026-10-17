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
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides Magick-based metadata loading and decode fallback.
/// </summary>
public sealed class MagickCodecAdapter : PhDisposable, ICodec
{
    private static readonly string[] _supportedExtensions = [];


    /// <inheritdoc/>
    public string CodecId { get; } = "magick.net";

    /// <inheritdoc/>
    public string CodecName { get; } = "Magick.NET";

    /// <inheritdoc/>
    public int MetadataPriority { get; } = 100;

    /// <inheritdoc/>
    public int DecodePriority { get; } = 10;

    /// <inheritdoc/>
    public int EncodePriority { get; } = 10;

    /// <inheritdoc/>
    public IReadOnlyList<string> DecodingExtensions => _supportedExtensions;

    /// <inheritdoc/>
    public bool SupportsEncoding { get; } = true;

    /// <inheritdoc/>
    public IReadOnlyList<string> EncodingExtensions => _supportedExtensions;


    /// <inheritdoc/>
    public bool CanLoadMetadata(string filePath)
    {
        return !string.IsNullOrWhiteSpace(filePath)
            && MagickCodec.CanRead(filePath);
    }


    /// <inheritdoc/>
    public Task<PhotoMetadata> LoadMetadataAsync(string filePath,
        PhotoReadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return MagickCodec.LoadMetadataAsync(filePath, options, null, cancellationToken);
    }


    /// <inheritdoc/>
    public bool CanDecode(PhotoMetadata metadata, CodecSelectionContext context)
    {
        return !string.IsNullOrWhiteSpace(metadata?.FilePath)
            && MagickCodec.CanRead(metadata.FilePath);
    }


    /// <inheritdoc/>
    public async Task<CodecDecodeResult> DecodeAsync(PhotoMetadata metadata,
        PhotoReadOptions options,
        CodecSelectionContext context,
        CancellationToken cancellationToken = default)
    {
        using var output = await MagickCodec.DecodeImageAsync(
            metadata,
            options,
            null,
            null,
            cancellationToken).ConfigureAwait(false);

        // tag what the decode converted to, so the viewer never converts the same pixels twice
        var image = SkiaCodec.FromMagick(output.SingleFrame, MagickCodec.GetDecodedColorSpace(output, metadata),
            metadata.IsHdr, out var decodeScale);

        return new CodecDecodeResult
        {
            CodecId = CodecId,
            ContentKind = image is not null ? CodecContentKind.StaticRaster : CodecContentKind.None,
            Size = new Size(image?.Width ?? 0, image?.Height ?? 0),
            DecodeScale = decodeScale,
            SingleFrame = image,
            IsHdr = metadata.IsHdr,
            HasEmbeddedColorProfile = metadata.SkiaColorSpace is not null || metadata.MagickColorProfile is not null,
            HasBakedDisplayProfile = MagickCodec.HasBakedDisplayProfile(output, metadata),
        };
    }


    /// <inheritdoc/>
    public bool CanEncode(string destFilePath, CodecEncodeContext context)
    {
        return !string.IsNullOrWhiteSpace(destFilePath)
            && MagickCodec.CanWrite(destFilePath);
    }


    /// <inheritdoc/>
    public async Task<CodecEncodeResult> EncodeAsync(CodecEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!CanEncode(request.DestFilePath, CodecEncodeContext.Default))
        {
            return new CodecEncodeResult(false, true);
        }

        try
        {
            // transform is already applied by the caller, so pass null
            await SkiaCodec.SaveAsync(request.Source, request.DestFilePath, null,
                request.Quality, cancellationToken).ConfigureAwait(false);
            return new CodecEncodeResult(true, false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            return new CodecEncodeResult(false, false, ex.Message);
        }
    }


    /// <summary>
    /// Always <c>false</c>: the built-in multi-frame writer is <c>MagickCodec.SaveAsync</c>, which
    /// re-reads the source collection and so preserves palettes, disposal and the GIF coalesce
    /// path far better than a frame-by-frame pixel round trip.
    /// </summary>
    public bool CanEncodeMultiFrame(string destFilePath, CodecEncodeContext context) => false;


    /// <inheritdoc/>
    public Task<CodecEncodeResult> EncodeMultiFrameAsync(CodecMultiFrameEncodeRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new CodecEncodeResult(false, true));
}
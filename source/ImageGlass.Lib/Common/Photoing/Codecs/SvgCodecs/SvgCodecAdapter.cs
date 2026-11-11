/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides SVG metadata loading and vector decoding when the vector renderer is enabled.
/// </summary>
public sealed class SvgCodecAdapter : PhDisposable, ICodec
{
    private static readonly string[] _supportedExtensions = [".svg", ".svgz"];


    /// <inheritdoc/>
    public string CodecId { get; } = "svg.skia";

    /// <inheritdoc/>
    public string CodecName { get; } = "Svg.Skia";

    /// <inheritdoc/>
    public int MetadataPriority { get; } = 200;

    /// <inheritdoc/>
    public int DecodePriority { get; } = 200;

    /// <inheritdoc/>
    public int EncodePriority { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> DecodingExtensions => _supportedExtensions;

    /// <summary>
    /// Decode-only: there is no SVG writer.
    /// </summary>
    public bool SupportsEncoding { get; }

    /// <inheritdoc/>
    public IReadOnlyList<string> EncodingExtensions => [];


    /// <inheritdoc/>
    public bool CanLoadMetadata(string filePath)
    {
        return SvgCodec.IsSvgFile(filePath);
    }


    /// <inheritdoc/>
    public Task<PhotoMetadata> LoadMetadataAsync(string filePath,
        PhotoReadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return SvgCodec.LoadMetadataAsync(filePath, cancellationToken);
    }


    /// <inheritdoc/>
    public bool CanDecode(PhotoMetadata metadata, CodecSelectionContext context)
    {
        return context.EnableVectorRenderer
            && metadata.IsVector
            && SvgCodec.IsSvgFile(metadata.FilePath);
    }


    /// <inheritdoc/>
    public async Task<CodecDecodeResult> DecodeAsync(PhotoMetadata metadata,
        PhotoReadOptions options, CodecSelectionContext context, CancellationToken cancellationToken = default)
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
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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Provides a common contract for built-in and future external photo codecs.
/// </summary>
public interface ICodec : IDisposable
{
    /// <summary>
    /// Gets the stable codec identifier.
    /// </summary>
    string CodecId { get; }

    /// <summary>
    /// Gets the friendly name of codec.
    /// </summary>
    string CodecName { get; }

    /// <summary>
    /// Gets the ordering priority when selecting a metadata codec.
    /// Higher values are evaluated first.
    /// </summary>
    int MetadataPriority { get; }

    /// <summary>
    /// Gets the ordering priority when selecting a decode codec.
    /// Higher values are evaluated first.
    /// </summary>
    int DecodePriority { get; }

    /// <summary>
    /// Gets the ordering priority when selecting a codec to write a destination extension.
    /// Higher values are evaluated first.
    /// </summary>
    int EncodePriority { get; }

    /// <summary>
    /// Gets the extensions this codec can read.
    /// </summary>
    IReadOnlyList<string> DecodingExtensions { get; }

    /// <summary>
    /// Gets whether this codec can write anything at all (static raster or multi-frame).
    /// Used to keep non-encoders out of encode selection entirely.
    /// </summary>
    bool SupportsEncoding { get; }

    /// <summary>
    /// Gets the extensions this codec can write. Empty plus <see cref="SupportsEncoding"/>
    /// means catch-all, which only a built-in codec may claim.
    /// </summary>
    IReadOnlyList<string> EncodingExtensions { get; }

    /// <summary>
    /// Returns <c>true</c> if this codec can load metadata for the specified file.
    /// </summary>
    bool CanLoadMetadata(string filePath);

    /// <summary>
    /// Loads metadata for the specified file.
    /// </summary>
    Task<PhotoMetadata> LoadMetadataAsync(string filePath,
        PhotoReadOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if this codec can decode the supplied metadata under the current runtime context.
    /// </summary>
    bool CanDecode(PhotoMetadata metadata, CodecSelectionContext context);

    /// <summary>
    /// Decodes the supplied metadata into a viewer-compatible result.
    /// </summary>
    Task<CodecDecodeResult> DecodeAsync(PhotoMetadata metadata,
        PhotoReadOptions options,
        CodecSelectionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if this codec can write the given destination path.
    /// </summary>
    bool CanEncode(string destFilePath, CodecEncodeContext context);

    /// <summary>
    /// Writes one image to <see cref="CodecEncodeRequest.DestFilePath"/>.
    /// </summary>
    Task<CodecEncodeResult> EncodeAsync(CodecEncodeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> if this codec can write multiple frames to the given destination path.
    /// </summary>
    bool CanEncodeMultiFrame(string destFilePath, CodecEncodeContext context);

    /// <summary>
    /// Writes every frame of the request to <see cref="CodecMultiFrameEncodeRequest.DestFilePath"/>.
    /// </summary>
    Task<CodecEncodeResult> EncodeMultiFrameAsync(CodecMultiFrameEncodeRequest request,
        CancellationToken cancellationToken = default);
}
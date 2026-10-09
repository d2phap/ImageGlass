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
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Reads the frames of a photo file on demand with the codec the viewer uses, apart from the viewer; every frame is a new image the caller owns.
/// </summary>
public sealed class PhotoFrameReader : PhDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ICodec? _codec;
    private readonly CodecSelectionContext _context;
    private readonly PhotoReadOptions _readOptions;

    // an animated codec renders any frame through its animator; the others decode one frame per call
    private AnimatorImpl? _animator;
    private SKImage? _firstFrame;


    /// <summary>
    /// Gets the metadata the reader loaded for the file.
    /// </summary>
    public PhotoMetadata Metadata { get; }


    /// <summary>
    /// Gets the number of frames or pages.
    /// </summary>
    public int FrameCount => _animator?.Frames.Length ?? (int)Math.Max(1, Metadata.FrameCount);


    private PhotoFrameReader(PhotoMetadata meta, ICodec? codec, CodecSelectionContext context, PhotoReadOptions readOptions)
    {
        Metadata = meta;
        _codec = codec;
        _context = context;
        _readOptions = readOptions;
    }


    /// <summary>
    /// Opens a file for reading its frames with <paramref name="readOptions"/>; <c>null</c> when no codec reads it.
    /// </summary>
    public static async Task<PhotoFrameReader?> OpenAsync(string filePath, PhotoReadOptions readOptions, CancellationToken token)
    {
        var meta = await LoadMetadataAsync(filePath, token).ConfigureAwait(false);
        if (meta is null) return null;

        // a vector file is read as raster frames here; vector drawing is the caller's own path
        var context = new CodecSelectionContext
        {
            EnableVectorRenderer = false,
            IsDestColorProfileSupported = Core.IsDestColorProfileSupported,
        };

        var reader = new PhotoFrameReader(meta, Core.CodecRegistry.SelectDecodeCodec(meta, context), context, readOptions);

        try
        {
            await reader.ReadFirstFrameAsync(token).ConfigureAwait(false);
            return reader;
        }
        catch
        {
            reader.Dispose();
            throw;
        }
    }


    /// <summary>
    /// Reads a frame, edits not applied; the caller owns the returned image.
    /// </summary>
    public async Task<SKImage?> ReadFrameAsync(int frameIndex, CancellationToken token)
    {
        await _gate.WaitAsync(token).ConfigureAwait(false);

        try
        {
            if (IsDisposed) return null;

            // an animator frame stays the animator's, and the next request may evict it
            if (_animator is not null)
            {
                return SkiaCodec.CopyImage(_animator.GetRenderedFrameBitmap((uint)frameIndex));
            }

            if (frameIndex == 0 && _firstFrame is not null)
            {
                var first = _firstFrame;
                _firstFrame = null;
                return first;
            }

            return await DecodeFrameAsync(frameIndex, token).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }


    /// <summary>
    /// Decodes frame 0, which also tells whether the codec renders frames through an animator.
    /// </summary>
    private async Task ReadFirstFrameAsync(CancellationToken token)
    {
        if (_codec is null) return;

        CodecDecodeResult? result = null;
        try
        {
            result = await _codec.DecodeAsync(Metadata, _readOptions with { FrameIndex = 0 }, _context, token).ConfigureAwait(false);

            // detach both, so disposing the result does not free what we keep
            _animator = result.Animator;
            _firstFrame = result.SingleFrame;
            result.Animator = null;
            result.SingleFrame = null;

            if (_animator is not null)
            {
                _firstFrame?.Dispose();
                _firstFrame = null;
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            // the per-frame Magick decode below still gets its turn
            Debug.WriteLine($"❌❌❌ {nameof(PhotoFrameReader)}.{nameof(ReadFirstFrameAsync)}: {ex.Message}");
        }
        finally
        {
            result?.Dispose();
        }
    }


    /// <summary>
    /// Decodes one frame with the codec, else with Magick, which sniffs the content itself.
    /// </summary>
    private async Task<SKImage?> DecodeFrameAsync(int frameIndex, CancellationToken token)
    {
        var options = _readOptions with { FrameIndex = frameIndex };

        if (_codec is not null)
        {
            try
            {
                using var result = await _codec.DecodeAsync(Metadata, options, _context, token).ConfigureAwait(false);
                var frame = result.SingleFrame;
                result.SingleFrame = null;

                if (!frame.IsDisposed()) return frame;
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {nameof(PhotoFrameReader)}.{nameof(DecodeFrameAsync)} ({_codec.CodecId}, frame {frameIndex}): {ex.Message}");
            }
        }

        using var data = await MagickCodec.DecodeImageAsync(Metadata, options, null, null, token).ConfigureAwait(false);
        return SkiaCodec.FromMagick(data.SingleFrame, MagickCodec.GetDecodedColorSpace(data, Metadata), Metadata.IsHdr);
    }


    /// <summary>
    /// Loads the metadata through the codec registry; <c>null</c> when no codec reads the file.
    /// </summary>
    private static async Task<PhotoMetadata?> LoadMetadataAsync(string filePath, CancellationToken token)
    {
        try
        {
            var codec = Core.CodecRegistry.SelectMetadataCodec(filePath);
            if (codec is null) return null;

            return await codec.LoadMetadataAsync(filePath, null, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(PhotoFrameReader)}.{nameof(LoadMetadataAsync)}: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        // the animator holds the metadata's color space, so it goes first
        _animator?.Dispose();
        _animator = null;
        _firstFrame?.Dispose();
        _firstFrame = null;
        Metadata.Dispose();
    }
}

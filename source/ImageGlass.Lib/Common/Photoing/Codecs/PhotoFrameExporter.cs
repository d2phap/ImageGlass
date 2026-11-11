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
using ImageGlass.Common.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Exports the frames of a multi-frame photo to PNG files.
/// </summary>
public static class PhotoFrameExporter
{
    /// <summary>
    /// Exports every frame of <paramref name="srcFilePath"/> into <paramref name="destFolder"/> as PNG,
    /// decoding with the codec the app uses to view the file. Plugin-backed formats (e.g. APNG) are
    /// therefore exported by their own codec instead of being handed to Magick, which cannot read them.
    /// </summary>
    /// <param name="srcFilePath">The full path of source file.</param>
    /// <param name="destFolder">The destination folder to save to.</param>
    public static async IAsyncEnumerable<(int FrameNumber, int FrameCount, string FileName)> SaveFramesAsync(
        string srcFilePath, string destFolder,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        // create dirs unless it does not exist
        Directory.CreateDirectory(destFolder);

        // 1. resolve the codec that decodes this file in the viewer
        var meta = await LoadMetadataAsync(srcFilePath, token).ConfigureAwait(false);
        var context = CreateSelectionContext();
        var codec = meta is null ? null : Core.CodecRegistry.SelectDecodeCodec(meta, context);

        // Magick gains nothing from per-frame decoding: its collection path reads every
        // frame in a single pass, so let it keep the files it already owns.
        var isMagickDecoder = codec is null
            || ReferenceEquals(codec, Core.CodecRegistry.FallbackDecodeCodec);


        // 2. the first decode tells us how the codec exposes frames: animated codecs return an
        //    animator that renders any frame, the others return one frame per decode call
        AnimatorImpl? animator = null;
        SKImage? pendingFrame = null;

        if (meta is not null && codec is not null && !isMagickDecoder)
        {
            var firstResult = await TryDecodeAsync(codec, meta, context, 0, token).ConfigureAwait(false);

            if (firstResult is not null)
            {
                animator = firstResult.Animator;
                pendingFrame = firstResult.SingleFrame;

                // detach both, so disposing the result does not free what we still need
                firstResult.Animator = null;
                firstResult.SingleFrame = null;
                firstResult.Dispose();

                if (animator is not null)
                {
                    pendingFrame?.Dispose();
                    pendingFrame = null;
                }
            }
        }


        // 3. no codec frame available: fall back to Magick, which sniffs the file content
        if (animator is null && pendingFrame is null)
        {
            meta?.Dispose();

            await foreach (var info in MagickCodec.SaveFramesAsync(srcFilePath, destFolder, token)
                .ConfigureAwait(false))
            {
                yield return info;
            }
            yield break;
        }


        // 4. write every frame decoded by the codec
        try
        {
            var frameCount = animator?.Frames.Length ?? (int)Math.Max(1, meta!.FrameCount);
            var srcFileName = Path.GetFileNameWithoutExtension(srcFilePath);
            var numberFormat = $"D{frameCount.ToString().Length}";

            for (var i = 0; i < frameCount; i++)
            {
                if (token.IsCancellationRequested) break;

                var frameNumber = i + 1;
                var fileName = $"{srcFileName} - {frameNumber.ToString(numberFormat)}.png";
                var destFilePath = Path.Combine(destFolder, fileName);

                // an animator frame stays owned by the animator; anything decoded here is ours
                var animatorFrame = animator?.GetRenderedFrameBitmap((uint)i);
                SKImage? ownedFrame = null;

                if (animatorFrame is null)
                {
                    ownedFrame = pendingFrame;
                    pendingFrame = null;

                    ownedFrame ??= await TryDecodeFrameAsync(codec!, meta!, context, i, token)
                        .ConfigureAwait(false);
                }

                var frame = animatorFrame ?? ownedFrame;

                try
                {
                    await WriteFrameAsync(frame, destFilePath, token).ConfigureAwait(false);
                }
                finally
                {
                    ownedFrame?.Dispose();
                }

                yield return (frameNumber, frameCount, fileName);
            }
        }
        finally
        {
            // the animator holds the metadata's color space, so it must go first
            animator?.Dispose();
            pendingFrame?.Dispose();
            meta?.Dispose();
        }
    }


    /// <summary>
    /// Loads the photo metadata through the codec registry. Returns <c>null</c> when no codec
    /// can read the file, which routes the export to the Magick fallback.
    /// </summary>
    private static async Task<PhotoMetadata?> LoadMetadataAsync(string srcFilePath, CancellationToken token)
    {
        try
        {
            var codec = Core.CodecRegistry.SelectMetadataCodec(srcFilePath);
            if (codec is null) return null;

            return await codec.LoadMetadataAsync(srcFilePath, null, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return null; }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(LoadMetadataAsync)}: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Creates the codec-selection context for a full-size decode, never an embedded preview.
    /// </summary>
    private static CodecSelectionContext CreateSelectionContext()
    {
        return new CodecSelectionContext
        {
            EnableVectorRenderer = Core.Config.EnableVectorRenderer,
            IsDestColorProfileSupported = Core.IsDestColorProfileSupported,
        };
    }


    /// <summary>
    /// Decodes the requested frame through the codec. Returns <c>null</c> when the decode fails.
    /// </summary>
    private static async Task<CodecDecodeResult?> TryDecodeAsync(ICodec codec, PhotoMetadata meta,
        CodecSelectionContext context, int frameIndex, CancellationToken token)
    {
        try
        {
            var options = new PhotoReadOptions() { FrameIndex = frameIndex };
            return await codec.DecodeAsync(meta, options, context, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return null; }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(TryDecodeAsync)} ({codec.CodecId}, frame {frameIndex}): {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Decodes the requested frame and hands its image over to the caller, which owns it.
    /// </summary>
    private static async Task<SKImage?> TryDecodeFrameAsync(ICodec codec, PhotoMetadata meta,
        CodecSelectionContext context, int frameIndex, CancellationToken token)
    {
        var result = await TryDecodeAsync(codec, meta, context, frameIndex, token).ConfigureAwait(false);
        if (result is null) return null;

        var frame = result.SingleFrame;

        // detach, so disposing the result does not free the returned image
        result.SingleFrame = null;
        result.Dispose();

        return frame;
    }


    /// <summary>
    /// Writes a decoded frame to a PNG file. An encoding failure is logged and skipped so it does
    /// not abort the remaining frames, while a file error is thrown because it hits every frame.
    /// </summary>
    private static async Task WriteFrameAsync(SKImage? frame, string destFilePath, CancellationToken token)
    {
        if (frame.IsDisposed()) return;

        try
        {
            using var encoded = frame.Encode(SKEncodedImageFormat.Png, 100);

            // Skia refused the pixel layout: let Magick write the file instead
            if (encoded is null || encoded.Size == 0)
            {
                await SkiaCodec.SaveAsync(frame, destFilePath, null, 100, token).ConfigureAwait(false);
                return;
            }

            await using var fileStream = File.Create(destFilePath);
            encoded.SaveTo(fileStream);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is not IOException and not UnauthorizedAccessException)
        {
            Debug.WriteLine($"❌❌❌ {nameof(WriteFrameAsync)} ({destFilePath}): {ex.Message}");
        }
    }

}

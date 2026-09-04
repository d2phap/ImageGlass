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
using SkiaSharp;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Routes a save through a plugin encoder when one claims the destination extension, leaving the
/// built-in Magick path untouched otherwise. Keeps <see cref="Photo.SaveAsAsync"/> thin.
/// </summary>
internal static class CodecEncodePipeline
{
    /// <summary>
    /// Tries to write <paramref name="destFilePath"/> with a plugin encoder. Returns <c>false</c>
    /// when no plugin claims it, so the caller falls back to the built-in path. Throws when a
    /// plugin was selected and genuinely failed.
    /// </summary>
    public static async Task<bool> TryEncodeAsync(Photo photo, string destFilePath,
        PhotoTransform? transform, uint quality, CancellationToken token)
    {
        if (photo is null || string.IsNullOrEmpty(destFilePath)) return false;

        var codec = Core.CodecRegistry.SelectEncodeCodec(destFilePath);

        // No encoder, or the built-in one: keep today's behavior verbatim, which also preserves
        // the multi-frame re-decode and the ICO downscale that live in MagickCodec.SaveAsync.
        if (codec is null || codec is MagickCodecAdapter) return false;

        var isMultiFrame = !photo.IsClipboard
            && photo.Metadata.FrameCount > 1
            && ReferenceEquals(Core.CodecRegistry.SelectMultiFrameEncodeCodec(destFilePath), codec);

        var tempPath = SaveStaging.BuildTempPath(destFilePath);
        SaveStaging.SweepStaleTempFiles(destFilePath);

        try
        {
            var result = isMultiFrame
                ? await EncodeMultiFrameAsync(codec, photo, tempPath, transform, quality, token).ConfigureAwait(false)
                : await EncodeSingleFrameAsync(codec, photo, tempPath, transform, quality, token).ConfigureAwait(false);

            if (result.Unsupported)
            {
                SaveStaging.TryDelete(tempPath);
                return false;
            }
            if (!result.Succeeded)
            {
                SaveStaging.TryDelete(tempPath);
                throw new InvalidDataException(
                    $"IGE: '{codec.CodecName}' could not write the image. {result.Error}".TrimEnd());
            }
        }
        catch
        {
            SaveStaging.TryDelete(tempPath);
            throw;
        }

        // a failed promote deliberately keeps the staged file: it holds the finished image
        var (promoted, promoteError) = await SaveStaging.PromoteAsync(tempPath, destFilePath, token).ConfigureAwait(false);
        if (!promoted)
        {
            throw new IOException($"IGE: could not finalize '{destFilePath}'. {promoteError}".TrimEnd());
        }

        return true;
    }


    /// <summary>
    /// Encodes the single image the user is saving.
    /// </summary>
    private static async Task<CodecEncodeResult> EncodeSingleFrameAsync(ICodec codec, Photo photo,
        string tempPath, PhotoTransform? transform, uint quality, CancellationToken token)
    {
        var (source, ownsSource) = await GetSingleFrameAsync(photo, transform, token).ConfigureAwait(false);
        if (source is null) return new CodecEncodeResult(false, false, "no pixels to encode");

        try
        {
            return await codec.EncodeAsync(new CodecEncodeRequest
            {
                DestFilePath = tempPath,
                Source = source,
                Quality = quality,
                SourceFilePath = photo.IsClipboard ? null : photo.FilePath,
                SourceIccProfile = GetIccProfile(photo),
            }, token).ConfigureAwait(false);
        }
        finally
        {
            if (ownsSource) source.Dispose();
        }
    }


    /// <summary>
    /// Resolves the pixels to write. Clipboard and selection content is already in memory; a file
    /// is re-decoded rather than reusing the cached bitmap, which may be a preview or a tone-mapped
    /// HDR frame that would silently downgrade the save.
    /// </summary>
    private static async Task<(SKImage? Image, bool Owned)> GetSingleFrameAsync(Photo photo,
        PhotoTransform? transform, CancellationToken token)
    {
        if (photo.IsClipboard)
        {
            if (photo.Bitmap is not SKImage img || img.IsDisposed()) return (null, false);

            var transformed = SkiaCodec.TransformImage(img, transform);
            return transformed is not null ? (transformed, true) : (img, false);
        }

        var frameIndex = (uint)Math.Max(0, photo.ReadOptions.FrameIndex);
        var decoded = await photo.DecodeStaticFrameAsync(frameIndex, token).ConfigureAwait(false);
        if (decoded is null) return (null, false);

        var withTransform = SkiaCodec.TransformImage(decoded, transform);
        if (withTransform is null) return (decoded, true);

        decoded.Dispose();
        return (withTransform, true);
    }


    /// <summary>
    /// Encodes every frame of a multi-frame photo, pulling one frame at a time.
    /// </summary>
    private static async Task<CodecEncodeResult> EncodeMultiFrameAsync(ICodec codec, Photo photo,
        string tempPath, PhotoTransform? transform, uint quality, CancellationToken token)
    {
        var frameCount = (int)Math.Max(1, photo.Metadata.FrameCount);
        var animator = photo.Bitmap as AnimatorImpl;
        var wasPlaying = animator?.IsPlaying == true;

        // The animator's frame cache is small and evicts by disposing, and it keeps advancing on
        // the UI thread. Pause it so frames stay valid long enough to be copied.
        if (wasPlaying) animator!.Pause();

        try
        {
            return await codec.EncodeMultiFrameAsync(new CodecMultiFrameEncodeRequest
            {
                DestFilePath = tempPath,
                FrameCount = frameCount,
                IsAnimated = animator is not null,
                LoopCount = (int)(animator?.LoopCount ?? photo.Metadata.AnimationLoop),
                Quality = quality,
                SourceFilePath = photo.FilePath,
                SourceIccProfile = GetIccProfile(photo),
                GetFrameAsync = (index, ct) => GetMultiFrameAsync(photo, animator, index, transform, ct),
            }, token).ConfigureAwait(false);
        }
        finally
        {
            if (wasPlaying) animator!.Play();
        }
    }


    /// <summary>
    /// Supplies one frame of a multi-frame save. An animator frame stays owned by the animator and
    /// is only valid until the next request, so the encoder copies it before asking for another.
    /// </summary>
    private static async Task<CodecEncodeFrame?> GetMultiFrameAsync(Photo photo, AnimatorImpl? animator,
        int index, PhotoTransform? transform, CancellationToken token)
    {
        if (animator is not null)
        {
            var frame = animator.GetRenderedFrameBitmap((uint)index);
            if (frame.IsDisposed()) return null;

            var duration = index < animator.Frames.Length ? animator.Frames[index].Duration : 0;
            var shaped = ApplyFrameTransform(frame, transform, index);

            return shaped is not null
                ? new CodecEncodeFrame(shaped, duration, true)
                : new CodecEncodeFrame(frame, duration, false);
        }

        // Page container (multi-page TIFF, PDF): no timing, decode each page on demand.
        var decoded = await photo.DecodeStaticFrameAsync((uint)index, token).ConfigureAwait(false);
        if (decoded is null) return null;

        var transformed = ApplyFrameTransform(decoded, transform, index);
        if (transformed is null) return new CodecEncodeFrame(decoded, 0, true);

        decoded.Dispose();
        return new CodecEncodeFrame(transformed, 0, true);
    }


    /// <summary>
    /// Applies the transform to one frame, matching Magick's rule: only the targeted frame, or all
    /// frames when the transform targets -1. Returns <c>null</c> when nothing changed.
    /// </summary>
    private static SKImage? ApplyFrameTransform(SKImage frame, PhotoTransform? transform, int index)
    {
        if (transform is null) return null;
        if (transform.FrameIndex != -1 && transform.FrameIndex != index) return null;

        return SkiaCodec.TransformImage(frame, transform);
    }


    /// <summary>
    /// The raw ICC bytes of the photo, so an encoder can tag its output; <c>null</c> means sRGB.
    /// </summary>
    private static byte[]? GetIccProfile(Photo photo)
    {
        try
        {
            return photo.Metadata.MagickColorProfile?.ToByteArray();
        }
        catch
        {
            return null;
        }
    }


}

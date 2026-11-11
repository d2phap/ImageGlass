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
using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Runtime context for encode-codec selection. Empty for now; mirrors <see cref="CodecSelectionContext"/>.
/// </summary>
public sealed class CodecEncodeContext
{
    /// <summary>
    /// The shared default instance.
    /// </summary>
    public static readonly CodecEncodeContext Default = new();
}


/// <summary>
/// Outcome of an encode attempt. <paramref name="Unsupported"/> means the codec declined, so the
/// caller should fall back rather than surface an error.
/// </summary>
public sealed record CodecEncodeResult(bool Succeeded, bool Unsupported, string? Error = null);


/// <summary>
/// One frame handed to a multi-frame encode. <paramref name="OwnsImage"/> is <c>false</c> for an
/// animator-owned frame (the animator disposes it) and <c>true</c> for a freshly decoded one.
/// </summary>
public sealed record CodecEncodeFrame(SKImage Image, int DurationMs, bool OwnsImage);


/// <summary>
/// A single-image encode request. <see cref="DestFilePath"/> is a host-owned temp path carrying
/// the real target extension; the host moves it into place on success.
/// </summary>
public sealed record CodecEncodeRequest
{
    /// <summary>
    /// Destination path to write, completely, closing any handle before returning.
    /// </summary>
    public required string DestFilePath { get; init; }

    /// <summary>
    /// Pixels to encode. Host-owned and already transformed; do not dispose.
    /// </summary>
    public required SKImage Source { get; init; }

    /// <summary>
    /// Requested output quality, 1..100.
    /// </summary>
    public uint Quality { get; init; } = 100;

    /// <summary>
    /// The image the pixels came from, or <c>null</c> for clipboard/selection content.
    /// Lets an encoder carry metadata across the save.
    /// </summary>
    public string? SourceFilePath { get; init; }

    /// <summary>
    /// Raw ICC profile of the pixels, straight from the source file; <c>null</c> means sRGB.
    /// Passed through rather than derived, because Skia exposes no color-space-to-ICC writer.
    /// </summary>
    public byte[]? SourceIccProfile { get; init; }
}


/// <summary>
/// A multi-frame encode request, covering animated formats and page containers alike
/// (see <see cref="IsAnimated"/>). Frames are pulled one at a time via <see cref="GetFrameAsync"/>.
/// </summary>
public sealed record CodecMultiFrameEncodeRequest
{
    /// <summary>
    /// Destination path to write; same contract as <see cref="CodecEncodeRequest.DestFilePath"/>.
    /// </summary>
    public required string DestFilePath { get; init; }

    /// <summary>
    /// Number of frames that will be requested, always >= 1.
    /// </summary>
    public required int FrameCount { get; init; }

    /// <summary>
    /// Supplies frame <c>i</c>. Called in order; only one frame is alive at a time.
    /// </summary>
    public required Func<int, CancellationToken, Task<CodecEncodeFrame?>> GetFrameAsync { get; init; }

    /// <summary>
    /// <c>true</c> when the frames form a timeline, so durations and <see cref="LoopCount"/> matter.
    /// <c>false</c> for a page container (multi-page TIFF, PDF).
    /// </summary>
    public bool IsAnimated { get; init; }

    /// <summary>
    /// Playback loops when <see cref="IsAnimated"/>; 0 = infinite.
    /// </summary>
    public int LoopCount { get; init; }

    /// <summary>
    /// Requested output quality, 1..100.
    /// </summary>
    public uint Quality { get; init; } = 100;

    /// <summary>
    /// The image the frames came from, or <c>null</c> for clipboard/selection content.
    /// </summary>
    public string? SourceFilePath { get; init; }

    /// <summary>
    /// Raw ICC profile of the frames; <c>null</c> means sRGB.
    /// </summary>
    public byte[]? SourceIccProfile { get; init; }
}

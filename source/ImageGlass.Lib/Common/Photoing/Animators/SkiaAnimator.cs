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
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// A SkiaSharp-based animator that decodes and composites frames on demand.
/// Optimized for low memory usage by maintaining a single composite buffer
/// instead of caching all frames.
/// </summary>
public class SkiaAnimator : AnimatorImpl
{
    private const int MAX_CACHE_COUNT = 5;

    /// <summary>
    /// <c>SkCodec::kNoFrame</c>: no usable prior frame, so the codec rebuilds the chain itself.
    /// </summary>
    private const int NO_FRAME = -1;

    private readonly SKCodec _codec;
    private readonly SKImage?[] _frameCache;
    private readonly Queue<uint> _cachedFramesQueue = new();
    private readonly Lock _syncLock = new();


    /// <summary>
    /// The canvas where frames are composed. 
    /// Keeps the current visual state of the animation.
    /// </summary>
    private SKBitmap? _compositeBitmap;

    private int _lastRenderedFrameIndex = -1;
    private DispatcherTimer _timer;


    /// <summary>
    /// Initializes a new instance of the <see cref="SkiaAnimator"/> class.
    /// </summary>
    public SkiaAnimator(SKCodec? srcCodec, SKCodecFrameInfo[] frames) : base(frames)
    {
        _codec = srcCodec ?? throw new ArgumentNullException(nameof(srcCodec));
        _frameCache = new SKImage[frames.Length];


        // Use DispatcherTimer to integrate with Avalonia's loop.
        // We set a high resolution (16ms ~ 60fps) to poll the stopwatch in the base class.
        _timer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher.UIThread);
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += Timer_Tick;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        lock (_syncLock)
        {
            StopTimer();
            _timer.Tick -= Timer_Tick;

            _compositeBitmap?.Dispose();
            _compositeBitmap = null;

            // Dispose caches
            for (int i = 0; i < _frameCache.Length; i++)
            {
                _frameCache[i]?.Dispose();
                _frameCache[i] = null;
            }
            _cachedFramesQueue.Clear();
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void StartTimer()
    {
        _timer.Start();
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void StopTimer()
    {
        _timer.Stop();
    }


    private void Timer_Tick(object? sender, EventArgs e)
    {
        OnTimerTicked();
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override SKImage? GetRenderedFrameBitmap(uint frameIndex)
    {
        lock (_syncLock)
        {
            if (IsDisposed || _codec is null) return null;

            // 1. Return cached result if available
            var cached = _frameCache[frameIndex];
            if (!cached.IsDisposed()) return cached;

            // Clear stale entry (disposed externally by SKImageRef)
            _frameCache[frameIndex] = null;


            // 2. Initialize Composite Bitmap if needed (Lazy init)
            if (_compositeBitmap is null)
            {
                var info = _codec.Info;
                // For high-bit-depth sources (HDR AVIF, animated WebP with wide gamut),
                // preserve the native color type. Otherwise use Bgra8888 for best
                // compatibility with Avalonia/Windows. Premul is faster for composition.
                var compositeType = SkiaCodec.IsHighBitDepthColorType(info.ColorType)
                    ? info.ColorType
                    : SKColorType.Bgra8888;
                _compositeBitmap = new SKBitmap(info.Width, info.Height, compositeType, SKAlphaType.Premul);
                _lastRenderedFrameIndex = -1;
            }


            // 3. Compose the requested frame. A seek needs no replay: the codec rebuilds
            // any non-sequential frame from its own required-frame chain.
            RenderFrame((int)frameIndex);


            // 4. Return Snapshot
            // SKImage.FromBitmap creates a copy if the bitmap is mutable.
            // This copy is ESSENTIAL for thread safety, preventing the UI from reading 
            // the bitmap while the animator modifies it for the next frame.
            var frameImage = SKImage.FromBitmap(_compositeBitmap);
            _frameCache[frameIndex] = frameImage;


            // 5. Manage Cache: FIFO Eviction
            if (!_cachedFramesQueue.Contains(frameIndex))
            {
                _cachedFramesQueue.Enqueue(frameIndex);
            }

            while (_cachedFramesQueue.Count > MAX_CACHE_COUNT)
            {
                var evictedIndex = _cachedFramesQueue.Dequeue();

                // Ensure we don't dispose the one we just created (safety check)
                if (evictedIndex != frameIndex)
                {
                    _frameCache[evictedIndex]?.Dispose();
                    _frameCache[evictedIndex] = null;
                }
            }

            return frameImage;
        }
    }


    /// <summary>
    /// Composes <paramref name="frameIndex"/> into <c>_compositeBitmap</c>.
    /// </summary>
    /// <remarks>
    /// The codec owns disposal and blending. Given a prior frame it applies that frame's disposal
    /// method and the current frame's blend itself, so doing either here corrupts the result.
    /// </remarks>
    private void RenderFrame(int frameIndex)
    {
        if (_compositeBitmap is null) return;
        if (frameIndex < 0 || frameIndex >= _frames.Length) return;

        var info = _compositeBitmap.Info;

        // ask the DECODING codec, not _frames: that array is built by a separate SKCodec whose
        // builder skips indexes it cannot read, which would shift every later entry
        var requiredFrame = _codec.GetFrameInfo(frameIndex, out var frameMeta)
            ? frameMeta.RequiredFrame
            : NO_FRAME;

        // chaining is only legal while the buffer really holds the frame the codec is told about,
        // and only priorFrame == requiredFrame makes the codec apply that frame's disposal
        var canChainFromPrevious = frameIndex > 0
            && _lastRenderedFrameIndex == frameIndex - 1
            && requiredFrame == frameIndex - 1;

        var options = canChainFromPrevious
            ? new SKCodecOptions(frameIndex, frameIndex - 1)
            : new SKCodecOptions(frameIndex);

        // an independent decode paints only its own sub-rect, so the rest must start clear
        if (!canChainFromPrevious) _compositeBitmap.Erase(SKColors.Transparent);

        var frameInfo = new SKImageInfo(info.Width, info.Height, info.ColorType, info.AlphaType);
        var result = _codec.GetPixels(frameInfo, _compositeBitmap.GetPixels(), _compositeBitmap.RowBytes, options);

        // a failed decode leaves the buffer in an unknown state, so the next frame must not chain onto it
        _lastRenderedFrameIndex = result == SKCodecResult.Success ? frameIndex : NO_FRAME;
    }


}

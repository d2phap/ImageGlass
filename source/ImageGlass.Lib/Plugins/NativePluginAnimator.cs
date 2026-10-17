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
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.SDK.Plugins;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ImageGlass.Plugins;

/// <summary>
/// <see cref="AnimatorImpl"/> backed by a native plugin codec. Pulls per-frame
/// pixels through the plugin's <c>DecodeAnimationFrame</c> entry point.
/// <para>
/// The plugin contract requires every frame returned by <c>DecodeAnimationFrame</c>
/// to be a fully composed RGBA frame at the full canvas size, so this animator
/// performs no sub-rect composition or disposal replay. Per-frame plugin calls
/// are serialized by <see cref="_syncLock"/> because they are not assumed to be
/// re-entrant.
/// </para>
/// </summary>
internal sealed unsafe class NativePluginAnimator : AnimatorImpl
{
    private const int MAX_CACHE_COUNT = 5;

    private readonly NativeCodecProxy _proxy;

    // written by a rename on the file watcher thread while frames still decode by path
    private volatile string _filePath;
    private readonly SKColorSpace? _colorSpace;

    private readonly SKImage?[] _frameCache;
    private readonly Queue<uint> _cachedFramesQueue = new();
    private readonly Lock _syncLock = new();

    private DispatcherTimer _timer;


    /// <summary>
    /// Builds an animator for the current photo by crossing the ABI once to pull
    /// animation traits and per-frame timing. Releases the native
    /// <see cref="IGAnimationInfo"/> back to the plugin before returning.
    /// </summary>
    public static NativePluginAnimator Create(NativeCodecProxy proxy,
        PhotoMetadata metadata, CancellationToken token)
    {
        var codecApi = proxy.CodecApi;
        if (!PluginAbi.HasEntryPoint(codecApi, &codecApi->GetAnimationInfo)
            || !PluginAbi.HasEntryPoint(codecApi, &codecApi->FreeAnimationInfo)
            || !PluginAbi.HasEntryPoint(codecApi, &codecApi->DecodeAnimationFrame))
        {
            throw new NotSupportedException(
                $"IGE: Native codec '{proxy.CodecId}' is missing one of the animation entry points.");
        }

        // PHASE 1: register cancellation, allocate a stack slot for the info struct.
        var cancelHandle = PluginHostApiTable.RegisterCancellation(token);
        IGAnimationInfo info = default;
        var infoOwned = false;
        try
        {
            IGStatus status;
            try
            {
                fixed (char* pPath = metadata.FilePath)
                {
                    var pathRef = new IGStringRef { Data = pPath, Length = metadata.FilePath.Length };
                    status = codecApi->GetAnimationInfo(pathRef, &info, (void*)cancelHandle);
                }
            }
            catch (Exception ex)
            {
                proxy.FailureManager.RecordSoftFailure(proxy.Plugin.PluginId,
                    $"managed exception during GetAnimationInfo: {ex.Message}");
                throw new InvalidDataException(
                    $"IGE: Native codec '{proxy.CodecId}' threw during GetAnimationInfo of '{metadata.FilePath}'.", ex);
            }

            if (status == IGStatus.Canceled)
            {
                token.ThrowIfCancellationRequested();
            }
            if (status != IGStatus.OK)
            {
                throw new InvalidDataException(
                    $"IGE: Native codec '{proxy.CodecId}' returned status {status} for GetAnimationInfo of '{metadata.FilePath}'.");
            }
            infoOwned = true;

            if (info.FrameCount <= 0 || info.Frames == null)
            {
                throw new InvalidDataException(
                    $"IGE: Native codec '{proxy.CodecId}' reported zero frames for '{metadata.FilePath}'.");
            }

            // PHASE 2: copy per-frame timing into the SKCodecFrameInfo[] expected by AnimatorImpl.
            // Only Duration + AlphaType are meaningful here -- the host does not composite.
            var frames = new SKCodecFrameInfo[info.FrameCount];
            for (var i = 0; i < info.FrameCount; i++)
            {
                var f = info.Frames[i];
                frames[i] = new SKCodecFrameInfo
                {
                    Duration = f.DurationMs,
                    AlphaType = f.HasAlpha != 0 ? SKAlphaType.Unpremul : SKAlphaType.Opaque,
                };
            }

            var animator = new NativePluginAnimator(proxy, metadata, frames)
            {
                _loopCount = Math.Max(0, info.LoopCount),
            };

            // Persist the loop count on the photo metadata so the UI knows.
            metadata.AnimationLoop = (uint)Math.Max(0, info.LoopCount);

            return animator;
        }
        finally
        {
            // PHASE 3: always return the IGAnimationInfo block to the plugin.
            if (infoOwned)
            {
                try
                {
                    codecApi->FreeAnimationInfo(&info);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NativePluginAnimator] FreeAnimationInfo threw: {ex.Message}");
                }
            }
            PluginHostApiTable.ReleaseCancellation(cancelHandle);
        }
    }


    private NativePluginAnimator(NativeCodecProxy proxy, PhotoMetadata metadata,
        SKCodecFrameInfo[] frames) : base(frames)
    {
        _proxy = proxy;
        _filePath = metadata.FilePath;
        _colorSpace = metadata.SkiaColorSpace;
        _frameCache = new SKImage[frames.Length];

        // Drive the animator on the UI dispatcher so frame change events stay on UI thread.
        _timer = new DispatcherTimer(DispatcherPriority.Render, Dispatcher.UIThread)
        {
            Interval = TimeSpan.FromMilliseconds(16),
        };
        _timer.Tick += Timer_Tick;
    }


    /// <inheritdoc/>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        lock (_syncLock)
        {
            StopTimer();
            _timer.Tick -= Timer_Tick;

            for (var i = 0; i < _frameCache.Length; i++)
            {
                _frameCache[i]?.Dispose();
                _frameCache[i] = null;
            }
            _cachedFramesQueue.Clear();
        }
    }


    /// <inheritdoc/>
    protected override void StartTimer() => _timer.Start();

    /// <inheritdoc/>
    protected override void StopTimer() => _timer.Stop();

    private void Timer_Tick(object? sender, EventArgs e) => OnTimerTicked();


    /// <inheritdoc/>
    public override void UpdateFilePath(string filePath) => _filePath = filePath;


    /// <inheritdoc/>
    public override SKImage? GetRenderedFrameBitmap(uint frameIndex)
    {
        lock (_syncLock)
        {
            if (IsDisposed) return null;
            if (frameIndex >= _frames.Length) return null;

            // 1. Fast path: cached image still alive.
            var cached = _frameCache[frameIndex];
            if (!cached.IsDisposed()) return cached;
            _frameCache[frameIndex] = null;

            // 2. Plugin returns the fully composed frame; just decode it.
            var result = DecodePluginFrame((int)frameIndex);
            if (result is null) return null;

            // 3. LRU cache (FIFO eviction with safety against re-evicting the freshest frame).
            _frameCache[frameIndex] = result;
            if (!_cachedFramesQueue.Contains(frameIndex))
            {
                _cachedFramesQueue.Enqueue(frameIndex);
            }
            while (_cachedFramesQueue.Count > MAX_CACHE_COUNT)
            {
                var evicted = _cachedFramesQueue.Dequeue();
                if (evicted != frameIndex)
                {
                    _frameCache[evicted]?.Dispose();
                    _frameCache[evicted] = null;
                }
            }

            return result;
        }
    }


    /// <summary>
    /// Calls the plugin's DecodeAnimationFrame and wraps the returned buffer in an
    /// <see cref="SKImage"/> (zero-copy via <see cref="PluginPixelBufferRelease"/>).
    /// The wrapped image IS the rendered frame.
    /// </summary>
    private SKImage? DecodePluginFrame(int frameIndex)
    {
        var codecApi = _proxy.CodecApi;
        IGPixelBuffer buffer = default;
        var bufferOwned = false;
        var ownershipTransferred = false;

        // read once, so the pinned chars and the length come from the same path
        var filePath = _filePath;

        try
        {
            IGStatus status;
            try
            {
                fixed (char* pPath = filePath)
                {
                    var pathRef = new IGStringRef { Data = pPath, Length = filePath.Length };
                    status = codecApi->DecodeAnimationFrame(pathRef, frameIndex, &buffer, null);
                }
            }
            catch (Exception ex)
            {
                _proxy.FailureManager.RecordSoftFailure(_proxy.Plugin.PluginId,
                    $"managed exception during DecodeAnimationFrame: {ex.Message}");
                return null;
            }

            if (status != IGStatus.OK) return null;
            bufferOwned = true;

            var frameHasAlpha = _frames[frameIndex].AlphaType != SKAlphaType.Opaque;
            var image = _proxy.WrapPluginBufferAsImage(in buffer, _colorSpace, frameHasAlpha);
            ownershipTransferred = true;
            return image;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NativePluginAnimator] DecodePluginFrame[{frameIndex}] failed: {ex.Message}");
            return null;
        }
        finally
        {
            if (bufferOwned && !ownershipTransferred)
            {
                try
                {
                    var localBuf = buffer;
                    codecApi->FreePixelBuffer(&localBuf);
                }
                catch { }
            }
        }
    }
}

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
using ImageGlass.SDK.Plugins;
using SkiaSharp;
using System;
using System.Diagnostics;

namespace ImageGlass.Plugins;

/// <summary>
/// Managed carrier handed to <see cref="SKImage.FromPixels(SKImageInfo, IntPtr, int, SKImageRasterReleaseDelegate, object)"/>
/// so that when SkiaSharp disposes the resulting image, the underlying plugin
/// pixel buffer is returned to the plugin via its <c>FreePixelBuffer</c> entry point.
/// <para>
/// SkiaSharp may invoke the release delegate on any thread; the SDK contract requires
/// <c>FreePixelBuffer</c> to be thread-safe.
/// </para>
/// </summary>
internal sealed class PluginPixelBufferRelease
{
    /// <summary>
    /// Pointer to the codec API table that produced the buffer. Stored as <see cref="nint"/>
    /// because this object is referenced by managed code; the underlying memory is plugin-owned
    /// and lives for the plugin's lifetime, well past any single SKImage.
    /// </summary>
    public nint CodecApiPtr;

    /// <summary>
    /// Snapshot of the pixel buffer descriptor that the plugin returned. The plugin uses
    /// the data pointer (and possibly <see cref="IGPixelBuffer.ReleaseContext"/>) inside this
    /// struct to identify which buffer to free.
    /// </summary>
    public IGPixelBuffer Buffer;

    /// <summary>
    /// Plugin id used when logging release failures.
    /// </summary>
    public string PluginId = string.Empty;

    /// <summary>
    /// Liveness gate of the owning plugin; the release no-ops once its library is unloaded.
    /// </summary>
    public PluginLiveToken? LiveToken;

    /// <summary>
    /// Tracks whether the buffer has already been released so manual release paths
    /// (host failure cleanup) and Skia disposal cannot double-free.
    /// </summary>
    private int _released;

    /// <summary>
    /// Buffer size reported to the GC via <see cref="GC.AddMemoryPressure"/>, 0 when untracked.
    /// </summary>
    private long _pressureBytes;


    /// <summary>
    /// Cached delegate handed to <c>SKImage.FromPixels</c> so we don't re-allocate per call.
    /// </summary>
    public static readonly SKImageRasterReleaseDelegate Release = OnRelease;

    /// <summary>
    /// Cached delegate handed to <c>SKData.Create</c> when wrapping the plugin pointer
    /// in an <see cref="SKData"/>.
    /// </summary>
    public static readonly SKDataReleaseDelegate ReleaseData = OnRelease;


    private static unsafe void OnRelease(IntPtr pixels, object context)
    {
        if (context is PluginPixelBufferRelease carrier)
        {
            carrier.ReleaseFromHost();
        }
    }


    /// <summary>
    /// Accounts this buffer to the GC, which cannot otherwise see plugin-owned native memory.
    /// </summary>
    public void TrackNativeMemory(long byteCount)
    {
        if (byteCount <= 0 || _pressureBytes != 0) return;

        _pressureBytes = byteCount;
        GC.AddMemoryPressure(byteCount);
    }


    /// <summary>
    /// Releases the plugin buffer immediately. Safe to call multiple times.
    /// </summary>
    public unsafe void ReleaseFromHost()
    {
        if (System.Threading.Interlocked.Exchange(ref _released, 1) != 0) return;

        // Skip if the library was unloaded; calling into unmapped memory crashes with an
        // uncatchable AccessViolationException. Memory is reclaimed by the OS on unload.
        if (LiveToken is not null) LiveToken.RunIfAlive(FreeBuffer);
        else FreeBuffer();

        // paired with TrackNativeMemory; the interlock above makes it exactly once
        if (_pressureBytes > 0) GC.RemoveMemoryPressure(_pressureBytes);
    }


    /// <summary>
    /// Calls the plugin's <c>FreePixelBuffer</c>; only valid while the library is loaded.
    /// </summary>
    private unsafe void FreeBuffer()
    {
        var apiPtr = CodecApiPtr;
        if (apiPtr == 0) return;

        try
        {
            var codecApi = (IGCodecApi*)apiPtr;
            if (!PluginAbi.HasEntryPoint(codecApi, &codecApi->FreePixelBuffer)) return;

            var localBuf = Buffer;
            codecApi->FreePixelBuffer(&localBuf);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PluginPixelBufferRelease] '{PluginId}' FreePixelBuffer threw: {ex.Message}");
        }
    }
}

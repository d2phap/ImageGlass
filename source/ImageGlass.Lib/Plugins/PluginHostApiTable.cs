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
using ImageGlass.Common;
using ImageGlass.SDK.Plugins;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ImageGlass.Plugins;


/// <summary>
/// Owns the singleton <see cref="IGHostApi"/> table the host hands to native plugins.
/// Allocates the host-side function-pointer tables once and pins them for the process lifetime.
/// </summary>
internal static unsafe class PluginHostApiTable
{
    private static IGHostApi* _hostApi;
    private static IGHostCoreApi* _coreApi;

    // Tracks live cancellation tokens by an opaque integer handle that crosses the ABI as void*.
    // This avoids passing GC handles directly to native code.
    private static readonly ConcurrentDictionary<nint, CancellationToken> _cancellationTokens = new();
    private static long _nextCancellationHandle;


    /// <summary>
    /// Returns a pointer to the host API table, lazily initializing it on first call.
    /// </summary>
    public static IGHostApi* Get()
    {
        if (_hostApi != null) return _hostApi;
        Initialize();
        return _hostApi!;
    }


    /// <summary>
    /// Registers a cancellation token and returns an opaque handle to pass into the plugin
    /// as the <c>cancellation</c> parameter of decode/metadata calls.
    /// </summary>
    public static nint RegisterCancellation(CancellationToken token)
    {
        if (token == default || !token.CanBeCanceled) return 0;
        var handle = (nint)Interlocked.Increment(ref _nextCancellationHandle);
        _cancellationTokens[handle] = token;
        return handle;
    }


    /// <summary>
    /// Releases an opaque cancellation handle previously created by <see cref="RegisterCancellation"/>.
    /// </summary>
    public static void ReleaseCancellation(nint handle)
    {
        if (handle == 0) return;
        _cancellationTokens.TryRemove(handle, out _);
    }


    /// <summary>
    /// Allocates and initializes the host API tables exposed to native plugins.
    /// </summary>
    private static void Initialize()
    {
        // Build the nested core API first so the top-level table can point to it.
        _coreApi = (IGHostCoreApi*)NativeMemory.AllocZeroed((nuint)sizeof(IGHostCoreApi));
        _coreApi->Log = &HostLog;
        _coreApi->Alloc = &HostAlloc;
        _coreApi->Free = &HostFree;
        _coreApi->IsCancellationRequested = &HostIsCancellationRequested;
        _coreApi->GetConfigDirectory = &HostGetConfigDirectory;

        // Expose the finalized core table through the top-level host API.
        _hostApi = (IGHostApi*)NativeMemory.AllocZeroed((nuint)sizeof(IGHostApi));
        _hostApi->StructSize = sizeof(IGHostApi);
        _hostApi->AbiVersion = IGNativeAbi.IG_PLUGIN_ABI_VERSION;
        _hostApi->Core = _coreApi;
    }


    #region Host callbacks (called by native plugins)

    /// <summary>
    /// Writes a plugin log message to the host debug output.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
    private static void HostLog(int level, IGStringRef message)
    {
        try
        {
            var text = message.ToManaged();
            Debug.WriteLine($"[NativePlugin][{level}] {text}");
        }
        catch
        {
            // never throw across the ABI
        }
    }


    /// <summary>
    /// Allocates unmanaged memory on behalf of a plugin.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void* HostAlloc(nuint size)
    {
        try
        {
            return NativeMemory.Alloc(size);
        }
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Frees unmanaged memory previously returned by <see cref="HostAlloc(nuint)"/>.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void HostFree(void* ptr)
    {
        try
        {
            if (ptr != null) NativeMemory.Free(ptr);
        }
        catch
        {
            // never throw across the ABI
        }
    }


    /// <summary>
    /// Checks whether the host-side cancellation token associated with a plugin call has been canceled.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int HostIsCancellationRequested(void* cancellation)
    {
        try
        {
            var handle = (nint)cancellation;
            if (handle == 0) return 0;
            if (_cancellationTokens.TryGetValue(handle, out var token))
            {
                return token.IsCancellationRequested ? 1 : 0;
            }
        }
        catch { }
        return 0;
    }


    /// <summary>
    /// Returns the configured ImageGlass config directory, optionally copying it into a caller buffer.
    /// </summary>
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int HostGetConfigDirectory(char* buffer, int bufferLength)
    {
        try
        {
            var path = BHelper.ConfigPath;
            if (buffer == null || bufferLength <= 0) return path.Length;
            var n = Math.Min(path.Length, bufferLength);
            for (var i = 0; i < n; i++) buffer[i] = path[i];
            return n;
        }
        catch
        {
            return 0;
        }
    }

    #endregion

}

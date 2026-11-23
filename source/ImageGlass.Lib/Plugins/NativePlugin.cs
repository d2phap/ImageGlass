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
using ImageGlass.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ImageGlass.Plugins;


/// <summary>
/// Wraps a single loaded native plugin shared library and the function pointers it returned.
/// </summary>
internal sealed unsafe class NativePlugin : PhDisposable
{
    /// <summary>
    /// Gets the manifest ID of the loaded plugin.
    /// </summary>
    public string PluginId { get; }

    /// <summary>
    /// Gets the absolute path to the loaded shared library.
    /// </summary>
    public string LibraryPath { get; }

    /// <summary>
    /// Gets the handle the library is mapped under, from <see cref="PluginLibraryLoader"/>.
    /// </summary>
    public nint LibraryHandle { get; }

    /// <summary>
    /// Gets the root API table exported by the plugin.
    /// </summary>
    public IGPluginApi* PluginApi { get; }

    /// <summary>
    /// Gets the codec entries advertised by the plugin during probing.
    /// </summary>
    public List<NativeCodecEntry> Codecs { get; } = [];

    /// <summary>
    /// Shared liveness gate; marked dead before the plugin is shut down so late buffer
    /// releases no-op instead of calling into a plugin that has stopped.
    /// </summary>
    public PluginLiveToken LiveToken { get; } = new();


    /// <summary>
    /// Creates a wrapper around one loaded native plugin instance.
    /// </summary>
    public NativePlugin(string pluginId, string libraryPath, nint libraryHandle, IGPluginApi* pluginApi)
    {
        PluginId = pluginId;
        LibraryPath = libraryPath;
        LibraryHandle = libraryHandle;
        PluginApi = pluginApi;
    }


    /// <summary>
    /// Shuts the plugin down and stops every call into it. The library stays mapped on purpose,
    /// see PluginLibraryLoader.
    /// </summary>
    protected override void OnDisposing()
    {
        // Mark dead first: nothing may call into the plugin once it has been shut down.
        LiveToken.MarkDead();

        // Give the plugin a chance to release its own process-lifetime state.
        try
        {
            if (PluginApi != null && PluginApi->Shutdown != null)
            {
                PluginApi->Shutdown();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NativePlugin] Shutdown threw for '{PluginId}': {ex.Message}");
        }

        base.OnDisposing();
    }
}


/// <summary>
/// Holds a single codec API pointer alongside its capability descriptor.
/// Wraps the pointer in <see cref="nint"/> so it can be used as a generic type argument.
/// </summary>
internal readonly struct NativeCodecEntry
{
    /// <summary>
    /// Gets the raw pointer to the codec-specific API table.
    /// </summary>
    public readonly nint CodecApiPtr;

    /// <summary>
    /// Gets the managed capability snapshot gathered during probing.
    /// </summary>
    public readonly CodecPluginCapability Capability;

    /// <summary>
    /// Creates one plugin-codec entry pairing the API pointer with its capability data.
    /// </summary>
    public NativeCodecEntry(nint codecApiPtr, CodecPluginCapability capability)
    {
        CodecApiPtr = codecApiPtr;
        Capability = capability;
    }
}


/// <summary>
/// Thread-safe gate tying use of a plugin to its enabled lifetime. Calling a plugin that has been
/// shut down is undefined, so every crossing goes through here and <see cref="MarkDead"/> drains.
/// </summary>
internal sealed class PluginLiveToken
{
    private readonly Lock _gate = new();
    private bool _isAlive = true;
    private int _entered;

    /// <summary>
    /// Runs <paramref name="release"/> under the gate only while the plugin is loaded.
    /// </summary>
    public void RunIfAlive(Action release)
    {
        lock (_gate)
        {
            if (!_isAlive) return;
            release();
        }
    }

    /// <summary>
    /// Registers an in-flight native call; <c>false</c> means the plugin is already dead.
    /// Pair every <c>true</c> with <see cref="Exit"/> in a <c>finally</c>.
    /// </summary>
    public bool TryEnter()
    {
        lock (_gate)
        {
            if (!_isAlive) return false;
            _entered++;
            return true;
        }
    }

    /// <summary>
    /// Releases an entry taken by <see cref="TryEnter"/>.
    /// </summary>
    public void Exit()
    {
        lock (_gate)
        {
            if (_entered > 0) _entered--;
        }
    }

    /// <summary>
    /// Marks the plugin dead and blocks until in-flight calls finish, so the caller can shut it
    /// down. Never call from inside a <see cref="TryEnter"/> scope.
    /// </summary>
    public void MarkDead()
    {
        // flag first, so no new entry can start
        lock (_gate) { _isAlive = false; }

        var spin = new SpinWait();
        while (true)
        {
            lock (_gate)
            {
                if (_entered == 0) return;
            }
            spin.SpinOnce();
        }
    }
}


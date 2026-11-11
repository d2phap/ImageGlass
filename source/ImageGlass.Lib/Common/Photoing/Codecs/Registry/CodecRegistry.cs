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
using ImageGlass.Plugins;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// A read-only snapshot of a registered codec (for diagnostics / settings UI).
/// <paramref name="PluginId"/> is the owning plugin's id when the codec comes from a native
/// plugin, or <c>null</c> for a built-in codec.
/// </summary>
public sealed record CodecInfo(string CodecId, string CodecName, int DecodePriority,
    IReadOnlyList<string> DecodingExtensions, bool IsPlugin, string? PluginId = null)
{
    /// <summary>
    /// Priority used when several codecs can write the same extension.
    /// </summary>
    public int EncodePriority { get; init; }

    /// <summary>
    /// Extensions this codec can write; empty for a decode-only codec.
    /// </summary>
    public IReadOnlyList<string> EncodingExtensions { get; init; } = [];

    /// <summary>
    /// Whether this codec can write anything.
    /// </summary>
    public bool SupportsEncoding { get; init; }

    /// <summary>
    /// Whether this is the last-resort decoder that claims files no other codec does.
    /// </summary>
    public bool IsFallback { get; init; }
}


/// <summary>
/// Provides deterministic selection and lifetime management for registered photo codecs.
/// </summary>
public sealed class CodecRegistry : PhDisposable
{
    private readonly Lock _lock = new();
    private readonly List<ICodec> _codecs = [];
    private readonly List<ICodec> _metadataCodecs = [];
    private readonly List<ICodec> _decodeCodecs = [];

    // Per-extension fast-path caches. The first lookup for an extension walks the full
    // priority-sorted list; subsequent lookups try the remembered winner first and only
    // fall back to a full scan if it can no longer handle the file (e.g. context changed,
    // file content differs). Caches are cleared whenever a new codec is registered.
    private readonly ConcurrentDictionary<string, ICodec> _metadataCodecByExt = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ICodec> _decodeCodecByExt = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ICodec> _encodeCodecByExt = new(StringComparer.OrdinalIgnoreCase);

    // Only codecs that can write; keeps decode-only codecs out of encode selection.
    private readonly List<ICodec> _encodeCodecs = [];

    // Extensions declared for decoding by plugin codecs; answers "plugin-owned?" without a
    // SelectDecodeCodec scan and the file probing it costs.
    private readonly HashSet<string> _pluginDecodingExts = new(StringComparer.OrdinalIgnoreCase);

    // Registration order, used to break priority ties deterministically.
    private readonly Dictionary<string, long> _regSeqById = new(StringComparer.Ordinal);
    private long _regSeq;

    private readonly MagickCodecAdapter _fallbackDecodeCodec;


    /// <summary>
    /// Gets the built-in Magick.NET codec used as the last-resort decoder when no registered
    /// codec claims the file. Magick sniffs the file content, so it can still succeed where the
    /// extension-based <see cref="ICodec.CanDecode"/> checks all say no.
    /// </summary>
    public ICodec FallbackDecodeCodec => _fallbackDecodeCodec;


    /// <summary>
    /// Initializes a new instance of <see cref="CodecRegistry"/> with the built-in codecs.
    /// </summary>
    public CodecRegistry()
    {
        _fallbackDecodeCodec = new MagickCodecAdapter();

        Register(new SvgCodecAdapter());
        Register(new SkiaCodecAdapter());
        Register(_fallbackDecodeCodec);
    }


    /// <summary>
    /// Registers a codec in the registry. All codecs (built-in or plugin) are treated equally and
    /// ordered by <see cref="ICodec.MetadataPriority"/> / <see cref="ICodec.DecodePriority"/>;
    /// a higher-priority plugin codec can therefore override a built-in for the same file.
    /// Ties go to the earlier registration, and built-ins register first.
    /// </summary>
    public void Register(ICodec codec)
    {
        ArgumentNullException.ThrowIfNull(codec);

        lock (_lock)
        {
            if (_codecs.Exists(c => c.CodecId.Equals(codec.CodecId, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException($"Codec '{codec.CodecId}' is already registered.");
            }

            _regSeqById[codec.CodecId] = _regSeq++;

            _codecs.Add(codec);
            _metadataCodecs.Add(codec);
            _decodeCodecs.Add(codec);
            if (codec.SupportsEncoding) _encodeCodecs.Add(codec);

            _metadataCodecs.Sort((left, right) => Compare(right.MetadataPriority, left.MetadataPriority, left, right));
            _decodeCodecs.Sort((left, right) => Compare(right.DecodePriority, left.DecodePriority, left, right));
            _encodeCodecs.Sort((left, right) => Compare(right.EncodePriority, left.EncodePriority, left, right));

            // the new codec may outrank a cached winner
            ClearSelectionCaches();
            RebuildPluginDecodingExts();
        }
    }


    /// <summary>
    /// Orders by descending priority, then by ascending registration order. Both arguments are
    /// pre-swapped by the caller so <paramref name="rightPriority"/> comes first.
    /// </summary>
    private int Compare(int rightPriority, int leftPriority, ICodec left, ICodec right)
    {
        var byPriority = rightPriority.CompareTo(leftPriority);
        if (byPriority != 0) return byPriority;

        var leftSeq = _regSeqById.GetValueOrDefault(left.CodecId, long.MaxValue);
        var rightSeq = _regSeqById.GetValueOrDefault(right.CodecId, long.MaxValue);
        return leftSeq.CompareTo(rightSeq);
    }


    /// <summary>
    /// Removes a codec from selection and clears the caches (does not dispose it); returns
    /// <c>true</c> if present. Used to hot-unregister a disabled plugin's codec.
    /// </summary>
    public bool Unregister(ICodec codec)
    {
        ArgumentNullException.ThrowIfNull(codec);

        lock (_lock)
        {
            var removed = _codecs.RemoveAll(c => ReferenceEquals(c, codec)) > 0;
            if (!removed) return false;

            _metadataCodecs.RemoveAll(c => ReferenceEquals(c, codec));
            _decodeCodecs.RemoveAll(c => ReferenceEquals(c, codec));
            _encodeCodecs.RemoveAll(c => ReferenceEquals(c, codec));
            _regSeqById.Remove(codec.CodecId);

            // the removed codec may be a cached winner
            ClearSelectionCaches();
            RebuildPluginDecodingExts();
            return true;
        }
    }


    /// <summary>
    /// Selects the first registered codec that can load metadata for the specified file.
    /// </summary>
    public ICodec? SelectMetadataCodec(string filePath)
    {
        var ext = string.IsNullOrEmpty(filePath) ? string.Empty : Path.GetExtension(filePath);

        lock (_lock)
        {
            return SelectWithCache(_metadataCodecByExt, _metadataCodecs, ext,
                c => c.CanLoadMetadata(filePath), nameof(SelectMetadataCodec));
        }
    }


    /// <summary>
    /// Selects the first registered codec that can decode the specified metadata under the current runtime context.
    /// </summary>
    public ICodec? SelectDecodeCodec(PhotoMetadata metadata, CodecSelectionContext context)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(context);

        var ext = metadata.FileExtension ?? string.Empty;

        lock (_lock)
        {
            return SelectWithCache(_decodeCodecByExt, _decodeCodecs, ext,
                c => c.CanDecode(metadata, context), nameof(SelectDecodeCodec));
        }
    }


    /// <summary>
    /// Whether a plugin codec declares <paramref name="extension"/> for decoding. Callers use it to
    /// keep a content-sniffing built-in decoder from pre-empting the codec that owns the format.
    /// </summary>
    public bool IsDecodingExtensionOwnedByPlugin(string? extension)
    {
        if (string.IsNullOrEmpty(extension)) return false;

        lock (_lock)
        {
            return _pluginDecodingExts.Contains(extension);
        }
    }


    /// <summary>
    /// Rebuilds the plugin-owned decoding extensions. Caller holds <c>_lock</c>.
    /// </summary>
    private void RebuildPluginDecodingExts()
    {
        _pluginDecodingExts.Clear();

        foreach (var codec in _decodeCodecs)
        {
            if (!IsPluginCodec(codec)) continue;

            foreach (var ext in codec.DecodingExtensions)
            {
                _pluginDecodingExts.Add(ext);
            }
        }
    }


    /// <summary>
    /// Clears the per-extension codec-selection caches so the next lookup re-scans by
    /// priority. Call when selection context changes (e.g. color-profile support toggles),
    /// else a codec chosen while a higher-priority one was ineligible stays stuck.
    /// </summary>
    public void InvalidateSelectionCaches()
    {
        lock (_lock)
        {
            ClearSelectionCaches();
        }
    }


    /// <summary>
    /// Clears every per-extension cache. One place, so a future cache cannot be forgotten.
    /// Caller holds <c>_lock</c>.
    /// </summary>
    private void ClearSelectionCaches()
    {
        _metadataCodecByExt.Clear();
        _decodeCodecByExt.Clear();
        _encodeCodecByExt.Clear();
    }


    /// <summary>
    /// Selects the codec that should write <paramref name="destFilePath"/>, or <c>null</c> if none can.
    /// </summary>
    public ICodec? SelectEncodeCodec(string destFilePath, CodecEncodeContext? context = null)
    {
        var ext = string.IsNullOrEmpty(destFilePath) ? string.Empty : Path.GetExtension(destFilePath);
        var ctx = context ?? CodecEncodeContext.Default;

        lock (_lock)
        {
            return SelectWithCache(_encodeCodecByExt, _encodeCodecs, ext,
                c => c.CanEncode(destFilePath, ctx), nameof(SelectEncodeCodec));
        }
    }


    /// <summary>
    /// Selects the codec that should write multiple frames to <paramref name="destFilePath"/>.
    /// Deliberately uncached: a codec can be static-only for the same extension, and saving is rare.
    /// </summary>
    public ICodec? SelectMultiFrameEncodeCodec(string destFilePath, CodecEncodeContext? context = null)
    {
        var ctx = context ?? CodecEncodeContext.Default;

        lock (_lock)
        {
            return SelectFirst(_encodeCodecs, c => c.CanEncodeMultiFrame(destFilePath, ctx),
                nameof(SelectMultiFrameEncodeCodec));
        }
    }


    /// <summary>
    /// Returns a snapshot of the codec that would write <paramref name="extension"/>, or <c>null</c>.
    /// </summary>
    public CodecInfo? GetEncodeCodecInfo(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return null;

        // CanEncode wants a path, so give it a synthetic file name carrying the extension.
        var probePath = "_" + (extension.StartsWith('.') ? extension : "." + extension);
        var codec = SelectEncodeCodec(probePath);

        return codec is null ? null : ToCodecInfo(codec);
    }


    /// <summary>
    /// Returns every extension a registered codec explicitly advertises for writing, with its owner.
    /// A catch-all encoder contributes nothing, since it enumerates no extensions.
    /// </summary>
    public IReadOnlyList<(string Ext, string CodecName, int EncodePriority, bool IsPlugin)> GetEncodingExtensions()
    {
        lock (_lock)
        {
            var list = new List<(string, string, int, bool)>();
            foreach (var codec in _encodeCodecs)
            {
                foreach (var ext in codec.EncodingExtensions)
                {
                    list.Add((ext, codec.CodecName, codec.EncodePriority, !IsBuiltIn(codec)));
                }
            }
            return list;
        }
    }


    /// <summary>
    /// Returns a read-only snapshot of the registered codecs, ordered by decode priority
    /// (highest first). Informational only (for diagnostics / settings UI).
    /// </summary>
    public IReadOnlyList<CodecInfo> GetCodecInfos()
    {
        lock (_lock)
        {
            var list = new List<CodecInfo>(_decodeCodecs.Count);
            foreach (var c in _decodeCodecs)
            {
                list.Add(ToCodecInfo(c));
            }
            return list;
        }
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        lock (_lock)
        {
            foreach (var codec in _codecs)
            {
                codec.Dispose();
            }

            _codecs.Clear();
            _metadataCodecs.Clear();
            _decodeCodecs.Clear();
            _encodeCodecs.Clear();
            _regSeqById.Clear();
            ClearSelectionCaches();
        }

        base.OnDisposing();
    }



    /// <summary>
    /// Whether the codec ships with the host rather than coming from a plugin.
    /// </summary>
    private static bool IsBuiltIn(ICodec codec)
        => codec is SvgCodecAdapter or SkiaCodecAdapter or MagickCodecAdapter;


    /// <summary>
    /// Whether the codec comes from a plugin instead of shipping with the host.
    /// </summary>
    public static bool IsPluginCodec(ICodec? codec)
        => codec is not null && !IsBuiltIn(codec);


    /// <summary>
    /// Builds the read-only snapshot of one registered codec.
    /// </summary>
    private CodecInfo ToCodecInfo(ICodec codec)
    {
        return new CodecInfo(codec.CodecId, codec.CodecName, codec.DecodePriority,
            codec.DecodingExtensions, !IsBuiltIn(codec), (codec as NativeCodecProxy)?.Plugin.PluginId)
        {
            EncodePriority = codec.EncodePriority,
            EncodingExtensions = codec.EncodingExtensions,
            SupportsEncoding = codec.SupportsEncoding,
            IsFallback = ReferenceEquals(codec, _fallbackDecodeCodec),
        };
    }


    private static ICodec? SelectFirst(List<ICodec> orderedCodecs, Func<ICodec, bool> predicate, string opName)
    {
        foreach (var codec in orderedCodecs)
        {
            try { if (predicate(codec)) return codec; }
            catch (Exception ex) { Debug.WriteLine($"❌❌❌ {opName} ({codec.CodecId}): {ex.Message}"); }
        }
        return null;
    }


    /// <summary>
    /// Tries the cached codec for <paramref name="ext"/> first; on miss or stale entry,
    /// falls back to a full priority-ordered scan and updates the cache with the winner.
    /// </summary>
    private static ICodec? SelectWithCache(
        ConcurrentDictionary<string, ICodec> cache,
        List<ICodec> orderedCodecs,
        string ext,
        Func<ICodec, bool> predicate,
        string opName)
    {
        if (!string.IsNullOrEmpty(ext) && cache.TryGetValue(ext, out var cached))
        {
            try
            {
                if (predicate(cached)) return cached;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {opName} ({cached.CodecId}) [cached]: {ex.Message}");
            }
        }

        var codec = SelectFirst(orderedCodecs, predicate, opName);
        if (codec != null && !string.IsNullOrEmpty(ext))
        {
            cache[ext] = codec;
        }

        return codec;
    }

}
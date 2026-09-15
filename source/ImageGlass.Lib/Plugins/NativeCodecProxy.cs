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
using Avalonia;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.SDK.Plugins;
using ImageMagick;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Plugins;


/// <summary>
/// Bridges a single native codec (one entry of an <see cref="IGPluginApi"/>) into the
/// host's <see cref="ICodec"/> contract so the rest of the codec system can treat it
/// identically to built-in codecs.
/// </summary>
internal sealed unsafe class NativeCodecProxy : PhDisposable, ICodec
{
    private readonly NativePlugin _plugin;
    private readonly IGCodecApi* _codecApi;
    private readonly PluginFailureManager _failureManager;
    private readonly HashSet<string> _supportedExtensionsSet;
    private readonly HashSet<string> _encodingExtensionsSet;

    /// <summary>
    /// Gets the plugin handle that owns this codec entry.
    /// </summary>
    internal NativePlugin Plugin => _plugin;

    /// <summary>
    /// Gets the native codec API table this proxy bridges into the host.
    /// </summary>
    internal IGCodecApi* CodecApi => _codecApi;

    /// <summary>
    /// Gets the failure manager used to record soft failures and quarantine the plugin.
    /// </summary>
    internal PluginFailureManager FailureManager => _failureManager;


    /// <summary>
    /// Gets the stable codec identifier reported by the plugin.
    /// </summary>
    public string CodecId { get; }

    /// <summary>
    /// Gets the human-readable codec name used in diagnostics and selection output.
    /// </summary>
    public string CodecName { get; }

    /// <summary>
    /// Gets the priority used during metadata codec selection.
    /// </summary>
    public int MetadataPriority { get; }

    /// <summary>
    /// Gets the priority used during full decode selection.
    /// </summary>
    public int DecodePriority { get; }

    /// <summary>
    /// Gets the priority used when choosing a codec to write a destination extension.
    /// </summary>
    public int EncodePriority { get; }

    /// <summary>
    /// Gets the extensions this codec decodes, after removing the ones the user switched off.
    /// </summary>
    public IReadOnlyList<string> DecodingExtensions { get; }

    /// <inheritdoc/>
    public bool SupportsEncoding => SupportsStaticRasterEncoding || SupportsMultiFrameEncoding;

    /// <summary>
    /// Gets the extensions this codec encodes, after removing the ones the user switched off.
    /// </summary>
    public IReadOnlyList<string> EncodingExtensions { get; }

    /// <summary>
    /// Gets every extension the codec declares for decoding, ignoring the user's choices.
    /// </summary>
    public IReadOnlyList<string> DeclaredDecodingExtensions { get; }

    /// <summary>
    /// Gets every extension the codec declares for encoding, ignoring the user's choices.
    /// </summary>
    public IReadOnlyList<string> DeclaredEncodingExtensions { get; }

    /// <summary>
    /// Gets whether the plugin codec supports metadata probing.
    /// </summary>
    public bool SupportsMetadata { get; }

    /// <summary>
    /// Gets whether the plugin codec supports static-raster decoding.
    /// </summary>
    public bool SupportsStaticRasterDecoding { get; }

    /// <summary>
    /// Gets whether the plugin codec can report embedded color profiles.
    /// </summary>
    public bool SupportsColorProfiles { get; }

    /// <summary>
    /// Gets whether the plugin codec implements the animation decode entry points.
    /// </summary>
    public bool SupportsAnimationDecoding { get; }

    /// <summary>
    /// Gets whether the plugin codec implements static-raster encoding.
    /// </summary>
    public bool SupportsStaticRasterEncoding { get; }

    /// <summary>
    /// Gets whether the plugin codec implements the multi-frame encode session.
    /// </summary>
    public bool SupportsMultiFrameEncoding { get; }


    /// <summary>
    /// Creates a managed proxy around one native codec entry.
    /// </summary>
    public NativeCodecProxy(
        NativePlugin plugin,
        IGCodecApi* codecApi,
        CodecPluginCapability capability,
        PluginFailureManager quarantine)
    {
        _plugin = plugin;
        _codecApi = codecApi;
        _failureManager = quarantine;

        CodecId = capability.CodecId;
        CodecName = string.IsNullOrEmpty(capability.CodecName)
            ? $"{plugin.PluginId}/{capability.CodecId}" : capability.CodecName;

        // Trust is the gate, so reported priorities are honored as-is; ties go to built-ins.
        MetadataPriority = capability.MetadataPriority;
        DecodePriority = capability.DecodePriority;
        EncodePriority = capability.EncodePriority;

        SupportsMetadata = capability.SupportsMetadata;
        SupportsColorProfiles = capability.SupportsColorProfiles;
        SupportsStaticRasterDecoding = capability.SupportsStaticRasterDecoding;
        SupportsAnimationDecoding = capability.SupportsAnimationDecoding;
        SupportsStaticRasterEncoding = capability.SupportsStaticRasterEncoding;
        SupportsMultiFrameEncoding = capability.SupportsMultiFrameEncoding;

        // Evaluated once, so changing the choices needs Core.ReloadPluginCodecs.
        var (disabledDecode, disabledEncode) = PluginTrustPolicy.GetExtensionExclusions(plugin.PluginId);

        DeclaredDecodingExtensions = Normalize(capability.DecodingExtensions);
        DeclaredEncodingExtensions = Normalize(capability.EncodingExtensions);
        DecodingExtensions = Filter(DeclaredDecodingExtensions, disabledDecode);
        EncodingExtensions = Filter(DeclaredEncodingExtensions, disabledEncode);

        _supportedExtensionsSet = new HashSet<string>(DecodingExtensions, StringComparer.OrdinalIgnoreCase);
        _encodingExtensionsSet = new HashSet<string>(EncodingExtensions, StringComparer.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Normalizes a capability extension list (lowercase, leading dot, no blanks).
    /// </summary>
    private static List<string> Normalize(string[] extensions)
    {
        var list = new List<string>(extensions.Length);
        foreach (var ext in extensions)
        {
            var normalized = PluginTrustPolicy.NormalizeExtension(ext);
            if (normalized.Length > 1) list.Add(normalized);
        }
        return list;
    }


    /// <summary>
    /// Removes the extensions the user switched off for this direction.
    /// </summary>
    private static List<string> Filter(IReadOnlyList<string> declared, IReadOnlySet<string> disabled)
    {
        if (disabled.Count == 0) return [.. declared];

        var list = new List<string>(declared.Count);
        foreach (var ext in declared)
        {
            if (!disabled.Contains(ext)) list.Add(ext);
        }
        return list;
    }


    /// <summary>
    /// Checks whether this codec should participate in metadata loading for the given file.
    /// </summary>
    public bool CanLoadMetadata(string filePath)
    {
        if (!SupportsMetadata) return false;
        if (_failureManager.IsQuarantined(_plugin.PluginId)) return false;
        if (string.IsNullOrEmpty(filePath)) return false;
        var ext = Path.GetExtension(filePath);
        return _supportedExtensionsSet.Contains(ext);
    }


    /// <summary>
    /// Checks whether this codec can decode the given photo metadata.
    /// </summary>
    public bool CanDecode(PhotoMetadata metadata, CodecSelectionContext context)
    {
        if (!SupportsStaticRasterDecoding) return false;
        if (_failureManager.IsQuarantined(_plugin.PluginId)) return false;
        if (metadata is null || string.IsNullOrEmpty(metadata.FilePath)) return false;
        return _supportedExtensionsSet.Contains(metadata.FileExtension);
    }


    /// <summary>
    /// Loads plugin-provided metadata on a background thread.
    /// </summary>
    public Task<PhotoMetadata> LoadMetadataAsync(string filePath,
        PhotoReadOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => LoadMetadataCore(filePath, cancellationToken),
            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Decodes one raster frame through the native plugin on a background thread.
    /// </summary>
    public Task<CodecDecodeResult> DecodeAsync(PhotoMetadata metadata,
        PhotoReadOptions options,
        CodecSelectionContext context,
        CancellationToken cancellationToken = default)
    {
        var frameIndex = Math.Max(0, options?.FrameIndex ?? 0);

        // 0 means full size; only the preview/thumbnail callers ask for a box
        var maxWidth = (int)Math.Min(int.MaxValue, options?.Width ?? 0);
        var maxHeight = (int)Math.Min(int.MaxValue, options?.Height ?? 0);

        // A size box only ever comes from the thumbnail/preview path, which wants one still;
        // building an animator to take frame 0 from it decodes the whole canvas for nothing.
        var wantsScaledStill = maxWidth > 0 && maxHeight > 0
            && PluginAbi.HasEntryPoint(_codecApi, &_codecApi->DecodeStaticRasterScaled);

        // Animation branch: the plugin advertises animation AND metadata says
        // the file plays as a timeline. Build the animator on a background
        // thread; per-frame decode is lazy inside the animator.
        if (SupportsAnimationDecoding && metadata.CanAnimate && !wantsScaledStill)
        {
            return Task.Factory.StartNew(() => DecodeAnimationCore(metadata, cancellationToken),
                cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        return Task.Factory.StartNew(
            () => DecodeCore(metadata, frameIndex, maxWidth, maxHeight, cancellationToken),
            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Builds a <see cref="NativePluginAnimator"/> for the current photo and wraps
    /// it in a <see cref="CodecDecodeResult"/>. Pixels are decoded lazily by the
    /// animator on every frame change.
    /// </summary>
    private CodecDecodeResult DecodeAnimationCore(PhotoMetadata metadata, CancellationToken token)
    {
        // PHASE 1: cross the ABI once to pull animation traits and per-frame timing.
        var animator = NativePluginAnimator.Create(this, metadata, token);

        // PHASE 2: hand the animator back to the host. Photo.LoadAsync moves it
        // into Photo.Bitmap, ViewerControl wires FrameChanged + StartAnimator.
        return new CodecDecodeResult
        {
            CodecId = $"plugin:{_plugin.PluginId}:{CodecId}",
            ContentKind = CodecContentKind.Animation,
            Size = new Size(metadata.Width, metadata.Height),
            Animator = animator,
            IsHdr = metadata.IsHdr,
            HasEmbeddedColorProfile = SupportsColorProfiles && metadata.SkiaColorSpace is not null,
        };
    }


    /// <summary>
    /// Invokes the plugin metadata entry point and maps the result into <see cref="PhotoMetadata"/>.
    /// </summary>
    private PhotoMetadata LoadMetadataCore(string filePath, CancellationToken token)
    {
        var meta = new PhotoMetadata(filePath);
        if (!PluginAbi.HasEntryPoint(_codecApi, &_codecApi->LoadMetadata)) return meta;

        // hold the plugin alive: a hot-disable could otherwise unmap it mid-call
        if (!_plugin.LiveToken.TryEnter()) return meta;

        // Register a host-side cancellation handle before crossing the ABI.
        var cancelHandle = PluginHostApiTable.RegisterCancellation(token);
        try
        {
            IGImageInfo info = default;
            IGStatus status;
            try
            {
                // Call the native metadata entry point with a fixed UTF-16 file path.
                fixed (char* pPath = filePath)
                {
                    var pathRef = new IGStringRef { Data = pPath, Length = filePath.Length };
                    status = _codecApi->LoadMetadata(pathRef, &info, (void*)cancelHandle);
                }
            }
            catch (Exception ex)
            {
                _failureManager.RecordSoftFailure(_plugin.PluginId,
                    $"managed exception during LoadMetadata: {ex.Message}");
                return meta;
            }

            if (status == IGStatus.Canceled)
            {
                token.ThrowIfCancellationRequested();
            }
            if (status == IGStatus.OK)
            {
                // Copy the native metadata fields into the managed photo model.
                if (info.Width > 0) meta.Width = (uint)info.Width;
                if (info.Height > 0) meta.Height = (uint)info.Height;
                if (info.Width > 0) meta.OriginalWidth = (uint)info.Width;
                if (info.Height > 0) meta.OriginalHeight = (uint)info.Height;
                meta.HasAlpha = info.HasAlpha != 0;
                meta.FrameCount = (uint)Math.Max(1, info.FrameCount);
                meta.HdrTransferFn = MapHdrTransferFn(info.HdrTransferFn);
                meta.BitsPerChannel = MapBitsPerChannel((IGPixelFormat)info.PixelFormat);

                // Animation gate: only flip CanAnimate when the codec advertises animation
                // AND the file genuinely has multiple frames. Multi-page documents (e.g. TIFF)
                // report FrameCount > 1 but must NOT trigger the animation pipeline.
                meta.CanAnimate = SupportsAnimationDecoding && info.FrameCount > 1;

                ApplyColorProfile(meta, in info);
            }
        }
        finally
        {
            PluginHostApiTable.ReleaseCancellation(cancelHandle);
            _plugin.LiveToken.Exit();
        }
        return meta;
    }


    /// <summary>
    /// Invokes the plugin decode entry point and wraps the returned pixel buffer in a codec result.
    /// </summary>
    private CodecDecodeResult DecodeCore(PhotoMetadata metadata, int frameIndex,
        int maxWidth, int maxHeight, CancellationToken token)
    {
        if (!PluginAbi.HasEntryPoint(_codecApi, &_codecApi->DecodeStaticRaster)
            || !PluginAbi.HasEntryPoint(_codecApi, &_codecApi->FreePixelBuffer))
        {
            throw new NotSupportedException($"Native codec '{CodecId}' does not support static-raster decode.");
        }

        // hold the plugin alive (see LoadMetadataCore)
        if (!_plugin.LiveToken.TryEnter())
        {
            throw new InvalidDataException($"IGE: Native codec '{CodecId}' was unloaded.");
        }

        // Register cancellation before entering native code and keep track of buffer ownership.
        var cancelHandle = PluginHostApiTable.RegisterCancellation(token);
        var filePath = metadata.FilePath;
        IGPixelBuffer buffer = default;
        var bufferOwned = false;
        var ownershipTransferred = false;

        try
        {
            // A scaled decode is worth asking for only when the caller wants a box AND the
            // plugin has the entry point; Unsupported from it just means "use the full path".
            var wantsScaled = maxWidth > 0 && maxHeight > 0
                && PluginAbi.HasEntryPoint(_codecApi, &_codecApi->DecodeStaticRasterScaled);
            var isScaled = false;

            IGStatus status;
            try
            {
                // Ask the plugin to decode the requested frame into its ABI buffer struct.
                fixed (char* pPath = filePath)
                {
                    var pathRef = new IGStringRef { Data = pPath, Length = filePath.Length };

                    if (wantsScaled)
                    {
                        status = _codecApi->DecodeStaticRasterScaled(pathRef, frameIndex,
                            maxWidth, maxHeight, &buffer, (void*)cancelHandle);
                        isScaled = status == IGStatus.OK;
                    }
                    else
                    {
                        status = IGStatus.Unsupported;
                    }

                    if (!isScaled && status is IGStatus.Unsupported or IGStatus.NotImplemented)
                    {
                        status = _codecApi->DecodeStaticRaster(pathRef, frameIndex, &buffer,
                            (void*)cancelHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                _failureManager.RecordSoftFailure(_plugin.PluginId,
                    $"managed exception during DecodeStaticRaster: {ex.Message}");
                throw new InvalidDataException(
                    $"IGE: Native codec '{CodecId}' threw during decode of '{filePath}'.", ex);
            }

            if (status == IGStatus.Canceled)
            {
                token.ThrowIfCancellationRequested();
            }
            if (status != IGStatus.OK)
            {
                throw new InvalidDataException(
                    $"IGE: Native codec '{CodecId}' returned status {status} for '{filePath}'.");
            }
            bufferOwned = true;

            // Wrap zero-copy: the SKImage takes ownership of the plugin buffer; when
            // SkiaSharp disposes it, the release delegate calls back into the plugin's
            // FreePixelBuffer (which the SDK contract requires to be thread-safe).
            // FrameCount is 0 until metadata is really loaded, so HasAlpha means nothing before that
            var knownAlpha = metadata.FrameCount > 0 ? metadata.HasAlpha : (bool?)null;
            var image = WrapPluginBufferAsImage(in buffer, metadata.SkiaColorSpace, knownAlpha);
            ownershipTransferred = true;

            // Synchronize the managed metadata dimensions with the decoded image; a scaled
            // buffer is smaller than the file, so it must never stand in for the real size.
            if (!isScaled)
            {
                if (metadata.Width == 0) metadata.Width = (uint)buffer.Width;
                if (metadata.Height == 0) metadata.Height = (uint)buffer.Height;
                if (metadata.OriginalWidth == 0) metadata.OriginalWidth = (uint)buffer.Width;
                if (metadata.OriginalHeight == 0) metadata.OriginalHeight = (uint)buffer.Height;
            }

            // Build the final decode result the registry expects.
            return new CodecDecodeResult
            {
                CodecId = $"plugin:{_plugin.PluginId}:{CodecId}",
                ContentKind = CodecContentKind.StaticRaster,
                Size = new Size(buffer.Width, buffer.Height),
                SingleFrame = image,
                IsHdr = metadata.IsHdr,
                HasEmbeddedColorProfile = SupportsColorProfiles && metadata.SkiaColorSpace is not null,
            };
        }
        finally
        {
            // If we never handed buffer ownership to Skia, return it to the plugin now.
            if (bufferOwned && !ownershipTransferred)
            {
                try
                {
                    IGPixelBuffer localBuf = buffer;
                    _codecApi->FreePixelBuffer(&localBuf);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[NativeCodecProxy] FreePixelBuffer threw: {ex.Message}");
                }
            }
            PluginHostApiTable.ReleaseCancellation(cancelHandle);
            _plugin.LiveToken.Exit();
        }
    }


    /// <summary>
    /// Checks whether this codec should write the given destination path.
    /// </summary>
    public bool CanEncode(string destFilePath, CodecEncodeContext context)
    {
        if (!SupportsStaticRasterEncoding) return false;
        if (_failureManager.IsQuarantined(_plugin.PluginId)) return false;
        if (string.IsNullOrEmpty(destFilePath)) return false;
        return _encodingExtensionsSet.Contains(Path.GetExtension(destFilePath));
    }


    /// <summary>
    /// Checks whether this codec should write multiple frames to the given destination path.
    /// </summary>
    public bool CanEncodeMultiFrame(string destFilePath, CodecEncodeContext context)
        => SupportsMultiFrameEncoding && CanEncode(destFilePath, context);


    /// <summary>
    /// Encodes one image through the plugin on a background thread.
    /// </summary>
    public Task<CodecEncodeResult> EncodeAsync(CodecEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => EncodeCore(request, cancellationToken),
            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Encodes every frame through the plugin's multi-frame session on a background thread.
    /// </summary>
    public Task<CodecEncodeResult> EncodeMultiFrameAsync(CodecMultiFrameEncodeRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.Factory.StartNew(() => EncodeMultiFrameCore(request, cancellationToken),
            cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    /// <summary>
    /// Copies an <see cref="SKImage"/> into a freshly allocated host-owned BGRA8 unpremultiplied
    /// buffer, reusing <paramref name="pixels"/> when it is already big enough.
    /// </summary>
    private static void ReadPixelsInto(SKImage image, ref byte* pixels, ref nuint capacity,
        out IGPixelBuffer buffer, bool keepPrecision)
    {
        // Unpremul because that is what every IGPixelFormat means to a plugin. Do NOT copy
        // SkiaCodec.ToMagick, which uses Premul: handing premultiplied bytes over is a silent
        // dark-halo bug on any image with alpha.
        var (colorType, pixelFormat, bytesPerPixel) = keepPrecision
            ? ResolveEncodeFormat(image.ColorType)
            : (SKColorType.Bgra8888, IGPixelFormat.Bgra8Unorm, 4);

        // The source color space rides along so ReadPixels does not convert; an 8-bit hand-over
        // stays untagged, exactly as it was before high-precision formats were offered.
        var info = keepPrecision
            ? new SKImageInfo(image.Width, image.Height, colorType, SKAlphaType.Unpremul, image.ColorSpace)
            : new SKImageInfo(image.Width, image.Height, colorType, SKAlphaType.Unpremul);

        var stride = checked(info.Width * bytesPerPixel);
        var byteCount = (long)stride * info.Height;

        if (byteCount > int.MaxValue) throw new InvalidDataException("IGE: frame too large to encode.");

        if (capacity < (nuint)byteCount)
        {
            if (pixels != null) System.Runtime.InteropServices.NativeMemory.Free(pixels);
            pixels = (byte*)System.Runtime.InteropServices.NativeMemory.Alloc((nuint)byteCount);
            capacity = (nuint)byteCount;
        }

        // Always copy: PeekPixels returns null for a GPU-backed image, its layout is whatever the
        // image happens to be, and a plugin-decoded image is backed by ANOTHER plugin's memory.
        if (!image.ReadPixels(info, (nint)pixels, stride, 0, 0))
        {
            throw new InvalidDataException("IGE: could not read the source pixels.");
        }

        buffer = new IGPixelBuffer
        {
            Data = pixels,
            Width = info.Width,
            Height = info.Height,
            Stride = stride,
            PixelFormat = (int)pixelFormat,
            ReleaseContext = HOST_OWNED_BUFFER,
        };
    }


    /// <summary>
    /// The hand-over format that keeps a high-bit-depth source intact, and its byte width.
    /// </summary>
    private static (SKColorType ColorType, IGPixelFormat PixelFormat, int BytesPerPixel)
        ResolveEncodeFormat(SKColorType sourceColorType) => sourceColorType switch
        {
            SKColorType.RgbaF16 or SKColorType.RgbaF16Clamped or SKColorType.RgbaF32
                => (SKColorType.RgbaF16, IGPixelFormat.RgbaFloat16, 8),

            SKColorType.Rgba16161616 or SKColorType.Rgba1010102 or SKColorType.Bgra1010102
                or SKColorType.Rgb101010x or SKColorType.Bgr101010x
                => (SKColorType.Rgba16161616, IGPixelFormat.Rgba16Unorm, 8),

            _ => (SKColorType.Bgra8888, IGPixelFormat.Bgra8Unorm, 4),
        };


    /// <summary>
    /// <c>true</c> when flattening to 8 bits would throw away real precision, so the encode should
    /// be offered the wider format first.
    /// </summary>
    private static bool NeedsHighPrecision(SKImage image)
    {
        return ResolveEncodeFormat(image.ColorType).BytesPerPixel > 4;
    }


    /// <summary>
    /// Sentinel the host puts in an encode input's <c>ReleaseContext</c> so a plugin that
    /// symmetrically frees its buffers can detect and refuse host memory.
    /// </summary>
    private static readonly void* HOST_OWNED_BUFFER = (void*)1;


    /// <summary>
    /// Builds the encode options for a call; <paramref name="pSrc"/> and <paramref name="pIcc"/>
    /// must stay fixed for the whole native call.
    /// </summary>
    private static IGEncodeOptions BuildOptions(uint quality, SKImage source, string srcPath,
        char* pSrc, byte[]? icc, byte* pIcc)
    {
        return new IGEncodeOptions
        {
            StructSize = sizeof(IGEncodeOptions),
            Quality = (int)Math.Clamp(quality, 1u, 100u),
            Lossless = quality >= 100 ? 1 : 0,
            PreserveAlpha = source.AlphaType != SKAlphaType.Opaque ? 1 : 0,
            SourceFilePath = new IGStringRef { Data = pSrc, Length = srcPath.Length },
            IccProfileData = pIcc,
            IccProfileSize = icc?.Length ?? 0,
        };
    }


    /// <summary>
    /// Invokes the plugin's single-image encode entry point.
    /// </summary>
    private CodecEncodeResult EncodeCore(CodecEncodeRequest request, CancellationToken token)
    {
        // Re-check the extension: a plugin that ignores it would otherwise write its own format
        // into whatever path a direct caller passed.
        if (!CanEncode(request.DestFilePath, CodecEncodeContext.Default)
            || !PluginAbi.HasEntryPoint(_codecApi, &_codecApi->EncodeStaticRaster))
        {
            return new CodecEncodeResult(false, true);
        }
        if (!_plugin.LiveToken.TryEnter()) return new CodecEncodeResult(false, true);

        var cancelHandle = PluginHostApiTable.RegisterCancellation(token);
        var srcPath = request.SourceFilePath ?? string.Empty;
        var icc = request.SourceIccProfile;
        byte* pixels = null;
        nuint capacity = 0;

        try
        {
            // Offer a high-bit-depth source in its own format so HDR is not flattened to 8 bits;
            // a plugin that takes only Bgra8Unorm answers Unsupported and the retry obliges it.
            var keepPrecision = NeedsHighPrecision(request.Source);

            while (true)
            {
                ReadPixelsInto(request.Source, ref pixels, ref capacity, out var buffer, keepPrecision);
                token.ThrowIfCancellationRequested();

                IGStatus status;
                fixed (char* pDest = request.DestFilePath)
                fixed (char* pSrc = srcPath)
                fixed (byte* pIcc = icc)
                {
                    var opts = BuildOptions(request.Quality, request.Source, srcPath, pSrc, icc, pIcc);
                    var destRef = new IGStringRef { Data = pDest, Length = request.DestFilePath.Length };

                    try
                    {
                        status = _codecApi->EncodeStaticRaster(destRef, &buffer, &opts, (void*)cancelHandle);
                    }
                    catch (Exception ex)
                    {
                        _failureManager.RecordSoftFailure(_plugin.PluginId,
                            $"managed exception during EncodeStaticRaster: {ex.Message}");
                        return new CodecEncodeResult(false, false, ex.Message);
                    }
                }

                if (keepPrecision && status is IGStatus.Unsupported or IGStatus.NotImplemented)
                {
                    keepPrecision = false;
                    continue;
                }

                return MapEncodeStatus(status, token);
            }
        }
        finally
        {
            if (pixels != null) System.Runtime.InteropServices.NativeMemory.Free(pixels);
            PluginHostApiTable.ReleaseCancellation(cancelHandle);
            _plugin.LiveToken.Exit();
        }
    }


    /// <summary>
    /// Drives the plugin's multi-frame session, holding exactly one frame at a time.
    /// </summary>
    private CodecEncodeResult EncodeMultiFrameCore(CodecMultiFrameEncodeRequest request, CancellationToken token)
    {
        if (!CanEncodeMultiFrame(request.DestFilePath, CodecEncodeContext.Default)
            || !PluginAbi.HasEntryPoint(_codecApi, &_codecApi->BeginEncodeMultiFrame)
            || !PluginAbi.HasEntryPoint(_codecApi, &_codecApi->EncodeFrame)
            || !PluginAbi.HasEntryPoint(_codecApi, &_codecApi->EndEncodeMultiFrame))
        {
            return new CodecEncodeResult(false, true);
        }
        if (!_plugin.LiveToken.TryEnter()) return new CodecEncodeResult(false, true);

        var cancelHandle = PluginHostApiTable.RegisterCancellation(token);
        var srcPath = request.SourceFilePath ?? string.Empty;
        var icc = request.SourceIccProfile;
        void* session = null;
        var committed = false;
        byte* pixels = null;
        nuint capacity = 0;

        try
        {
            IGStatus status;
            using (var firstFrame = FetchFrame(request, 0, token))
            {
                if (firstFrame is null) return new CodecEncodeResult(false, false, "frame 0 unavailable");

                fixed (char* pDest = request.DestFilePath)
                fixed (char* pSrc = srcPath)
                fixed (byte* pIcc = icc)
                {
                    var info = new IGMultiFrameEncodeInfo
                    {
                        StructSize = sizeof(IGMultiFrameEncodeInfo),
                        FrameCount = request.FrameCount,
                        IsAnimated = request.IsAnimated ? 1 : 0,
                        LoopCount = request.LoopCount,
                        CanvasWidth = 0,
                        CanvasHeight = 0,
                    };
                    var opts = BuildOptions(request.Quality, firstFrame.Frame.Image, srcPath, pSrc, icc, pIcc);
                    var destRef = new IGStringRef { Data = pDest, Length = request.DestFilePath.Length };

                    status = _codecApi->BeginEncodeMultiFrame(destRef, &info, &opts, &session, (void*)cancelHandle);
                }

                if (status is IGStatus.Unsupported or IGStatus.NotImplemented)
                {
                    session = null;
                    return new CodecEncodeResult(false, true);
                }
                if (status != IGStatus.OK || session == null)
                {
                    session = null;
                    return new CodecEncodeResult(false, false, $"BeginEncodeMultiFrame returned {status}");
                }

                status = WriteFrame(firstFrame.Frame, 0, ref pixels, ref capacity, session, cancelHandle);
            }

            if (status != IGStatus.OK) return MapEncodeStatus(status, token);

            for (var i = 1; i < request.FrameCount; i++)
            {
                token.ThrowIfCancellationRequested();

                using var frame = FetchFrame(request, i, token);
                if (frame is null) return new CodecEncodeResult(false, false, $"frame {i} unavailable");

                status = WriteFrame(frame.Frame, i, ref pixels, ref capacity, session, cancelHandle);
                if (status != IGStatus.OK) return MapEncodeStatus(status, token);
            }

            status = _codecApi->EndEncodeMultiFrame(session, 1, (void*)cancelHandle);
            committed = true;
            return MapEncodeStatus(status, token);
        }
        finally
        {
            // abort so the plugin closes its handle; the host discards the temp file
            if (session != null && !committed)
            {
                try { _codecApi->EndEncodeMultiFrame(session, 0, null); }
                catch (Exception ex) { Debug.WriteLine($"[NativeCodecProxy] abort threw: {ex.Message}"); }
            }
            if (pixels != null) System.Runtime.InteropServices.NativeMemory.Free(pixels);
            PluginHostApiTable.ReleaseCancellation(cancelHandle);
            _plugin.LiveToken.Exit();
        }
    }


    /// <summary>
    /// Copies one frame across the ABI. The frame's pixels are copied first, so the caller may
    /// release it as soon as this returns.
    /// </summary>
    private IGStatus WriteFrame(CodecEncodeFrame frame, int index, ref byte* pixels, ref nuint capacity,
        void* session, nint cancelHandle)
    {
        // 8-bit: the session is already open, so a per-frame format retry would mean restarting it
        ReadPixelsInto(frame.Image, ref pixels, ref capacity, out var buffer, keepPrecision: false);

        var frameInfo = new IGEncodeFrameInfo
        {
            StructSize = sizeof(IGEncodeFrameInfo),
            FrameIndex = index,
            DurationMs = frame.DurationMs,
            HasAlpha = frame.Image.AlphaType != SKAlphaType.Opaque ? 1 : 0,
        };

        try
        {
            return _codecApi->EncodeFrame(session, &buffer, &frameInfo, (void*)cancelHandle);
        }
        catch (Exception ex)
        {
            _failureManager.RecordSoftFailure(_plugin.PluginId,
                $"managed exception during EncodeFrame: {ex.Message}");
            return IGStatus.Internal;
        }
    }


    /// <summary>
    /// Pulls frame <paramref name="index"/> and wraps it so only owned frames get disposed.
    /// </summary>
    private static OwnedFrame? FetchFrame(CodecMultiFrameEncodeRequest request, int index, CancellationToken token)
    {
        var frame = request.GetFrameAsync(index, token).GetAwaiter().GetResult();
        return frame is null || frame.Image.IsDisposed() ? null : new OwnedFrame(frame);
    }


    /// <summary>
    /// Translates a plugin encode status into a host result.
    /// </summary>
    private static CodecEncodeResult MapEncodeStatus(IGStatus status, CancellationToken token)
    {
        if (status == IGStatus.Canceled) token.ThrowIfCancellationRequested();
        if (status is IGStatus.Unsupported or IGStatus.NotImplemented) return new CodecEncodeResult(false, true);
        if (status != IGStatus.OK) return new CodecEncodeResult(false, false, $"codec returned {status}");
        return new CodecEncodeResult(true, false);
    }


    /// <summary>
    /// Disposes a pulled frame only when the producer handed over ownership.
    /// </summary>
    private sealed class OwnedFrame(CodecEncodeFrame frame) : IDisposable
    {
        public CodecEncodeFrame Frame { get; } = frame;

        public void Dispose()
        {
            if (Frame.OwnsImage) Frame.Image.Dispose();
        }
    }


    /// <summary>
    /// Wraps a plugin-owned <see cref="IGPixelBuffer"/> in an <see cref="SKImage"/>
    /// without copying the pixels. The returned image owns the plugin buffer:
    /// disposing the image calls back into the plugin's <c>FreePixelBuffer</c>
    /// via the release delegate (which the SDK contract requires to be thread-safe).
    /// </summary>
    /// <param name="hasAlpha">The plugin's alpha answer; only a definite <c>false</c> marks it opaque.</param>
    internal SKImage WrapPluginBufferAsImage(in IGPixelBuffer buffer, SKColorSpace? srcColorSpace,
        bool? hasAlpha = null)
    {
        // Validate the buffer before we hand it to Skia.
        if (buffer.Data == null || buffer.Width <= 0 || buffer.Height <= 0)
        {
            throw new InvalidDataException(
                $"IGE: Native codec '{CodecId}' returned an invalid pixel buffer.");
        }

        var (colorType, alphaType) = MapPixelFormat((IGPixelFormat)buffer.PixelFormat, hasAlpha);
        if (colorType == SKColorType.Unknown)
        {
            throw new InvalidDataException(
                $"IGE: Native codec '{CodecId}' returned an unsupported pixel format ({buffer.PixelFormat}).");
        }

        var info = new SKImageInfo(buffer.Width, buffer.Height, colorType, alphaType);
        if (srcColorSpace is not null)
        {
            info = info.WithColorSpace(srcColorSpace);
        }

        // Carrier holds the plugin codec API + a copy of the buffer descriptor so
        // SkiaSharp can hand the pointer back to the plugin on dispose.
        var carrier = new PluginPixelBufferRelease
        {
            CodecApiPtr = (nint)_codecApi,
            Buffer = buffer,
            PluginId = _plugin.PluginId,
            LiveToken = _plugin.LiveToken,
        };

        // native memory the GC cannot see; without this, decoded frames pile up unnoticed
        var byteCount = checked(buffer.Stride * buffer.Height);
        carrier.TrackNativeMemory(byteCount);

        // Wrap the plugin pointer in SKData with a release callback, then build the SKImage.
        SKData? data = null;
        SKImage? image;
        try
        {
            data = SKData.Create((nint)buffer.Data, byteCount,
                PluginPixelBufferRelease.ReleaseData, carrier);

            image = SKImage.FromPixels(info, data, buffer.Stride);
        }
        catch
        {
            data?.Dispose();
            carrier.ReleaseFromHost();
            throw;
        }

        if (image is null)
        {
            // Skia rejected the layout; release the plugin buffer ourselves.
            data?.Dispose();
            carrier.ReleaseFromHost();
            throw new InvalidDataException(
                $"IGE: Native codec '{CodecId}' returned an unsupported pixel buffer layout.");
        }

        return image;
    }


    /// <summary>
    /// Bits per channel the plugin's declared pixel format decodes to; drives the cache budget.
    /// </summary>
    internal static int MapBitsPerChannel(IGPixelFormat format) => format switch
    {
        IGPixelFormat.Rgba16Unorm or IGPixelFormat.RgbaFloat16 => 16,
        _ => 8,
    };


    /// <summary>
    /// Maps an <see cref="IGPixelFormat"/> to the corresponding Skia color/alpha types.
    /// Returns (<see cref="SKColorType.Unknown"/>, <see cref="SKAlphaType.Unknown"/>) when the
    /// host has no compatible Skia format for the buffer.
    /// </summary>
    /// <param name="hasAlpha">A definite <c>false</c> marks it opaque so Skia skips the unpremultiply pass.</param>
    internal static (SKColorType ColorType, SKAlphaType AlphaType) MapPixelFormat(IGPixelFormat format,
        bool? hasAlpha = null)
    {
        var alphaType = hasAlpha == false ? SKAlphaType.Opaque : SKAlphaType.Unpremul;

        return format switch
        {
            IGPixelFormat.Bgra8Unorm => (SKColorType.Bgra8888, alphaType),
            IGPixelFormat.Rgba8Unorm => (SKColorType.Rgba8888, alphaType),
            IGPixelFormat.Rgba16Unorm => (SKColorType.Rgba16161616, alphaType),
            IGPixelFormat.RgbaFloat16 => (SKColorType.RgbaF16, alphaType),
            _ => (SKColorType.Unknown, SKAlphaType.Unknown),
        };
    }


    /// <summary>
    /// Maps an <see cref="IGHdrTransferFn"/> integer (as carried in <see cref="IGImageInfo.HdrTransferFn"/>)
    /// to the host's <see cref="HdrTransferFunction"/> enum.
    /// </summary>
    private static HdrTransferFunction MapHdrTransferFn(int value)
    {
        return (IGHdrTransferFn)value switch
        {
            IGHdrTransferFn.PQ => HdrTransferFunction.PQ,
            IGHdrTransferFn.HLG => HdrTransferFunction.HLG,
            IGHdrTransferFn.GainMap => HdrTransferFunction.GainMap,
            IGHdrTransferFn.Linear => HdrTransferFunction.Linear,
            IGHdrTransferFn.ScRgb => HdrTransferFunction.ScRgb,
            _ => HdrTransferFunction.None,
        };
    }


    /// <summary>
    /// Resolves the source color profile from the plugin-supplied
    /// <see cref="IGImageInfo"/> and writes it into <paramref name="meta"/>.
    /// Prefers the raw ICC bytes (handles arbitrary profiles like ProPhoto)
    /// and falls back to the <see cref="IGColorSpace"/> enum hint.
    /// </summary>
    private static void ApplyColorProfile(PhotoMetadata meta, in IGImageInfo info)
    {
        // Prefer raw ICC bytes when supplied. The plugin owns the buffer and
        // both SKColorSpace.CreateIcc and PhotoColorProfile copy what they need
        // synchronously, so we don't retain the pointer past this call.
        SKColorSpace? skiaCs = null;
        if (info.IccProfileData != null && info.IccProfileSize > 0)
        {
            try
            {
                // Build Skia/Magick profile objects and the human-readable profile name.
                var iccSpan = new ReadOnlySpan<byte>(info.IccProfileData, info.IccProfileSize);
                skiaCs = SkiaCodec.CreateIccColorSpace(iccSpan);

                var bytes = iccSpan.ToArray();
                var photoProfile = new PhotoColorProfile(bytes);
                meta.ColorProfileName = photoProfile.GetIccDescription();
                if (photoProfile.ProfileData is not null)
                {
                    meta.MagickColorProfile = new ColorProfile(photoProfile.ProfileData);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NativeCodecProxy] ICC profile parse failed: {ex.Message}");
            }
        }

        // Fall back to the named-enum hint if no ICC was provided.
        skiaCs ??= MapColorSpace((IGColorSpace)info.ColorSpace);

        // Store the resolved color space and the coarse Magick color-space classification.
        if (skiaCs is not null)
        {
            meta.SkiaColorSpace?.Dispose();
            meta.SkiaColorSpace = skiaCs;
        }

        meta.ColorSpace = skiaCs is not null && skiaCs.IsSrgb
            ? ImageMagick.ColorSpace.sRGB
            : ImageMagick.ColorSpace.Undefined;
    }


    /// <summary>
    /// Maps an <see cref="IGColorSpace"/> identifier to a fresh <see cref="SKColorSpace"/>.
    /// Returns <c>null</c> when the plugin reported <see cref="IGColorSpace.Unknown"/>;
    /// the caller should leave the existing <see cref="PhotoMetadata.SkiaColorSpace"/> alone
    /// (or assume sRGB) in that case.
    /// </summary>
    private static SKColorSpace? MapColorSpace(IGColorSpace cs)
    {
        return cs switch
        {
            IGColorSpace.Srgb => SKColorSpace.CreateSrgb(),
            IGColorSpace.LinearSrgb => SKColorSpace.CreateSrgbLinear(),
            IGColorSpace.DisplayP3 => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Srgb, SKColorSpaceXyz.DisplayP3),
            IGColorSpace.AdobeRgb => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.TwoDotTwo, SKColorSpaceXyz.AdobeRgb),
            IGColorSpace.Rec2020 => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Rec2020, SKColorSpaceXyz.Rec2020),
            IGColorSpace.Rec2020Linear => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Rec2020),
            _ => null,
        };
    }


    /// <summary>
    /// Releases any resources owned directly by the proxy.
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();
    }
}

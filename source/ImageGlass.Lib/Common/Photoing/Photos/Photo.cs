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
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Loggers;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.ServiceProviders.FileSearchService;
using ImageGlass.Common.Types;
using ImageMagick;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;

public partial class Photo : PhDisposable
{
    // private properties
    private uint _width = 0;
    private uint _height = 0;
    private int _frameIndex = -1;

    private Task<PhotoMetadata>? _taskMetadata;
    private CancellationTokenSource? _cancelPhotoLoading;
    private readonly Lock _lock = new();
    private int _loadGeneration;

    // serializes LoadAsync on this photo: a second loader joins the load in flight, never cancels it
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private int _cancelEpoch;

    // track pending tasks
    private ConcurrentDictionary<Guid, bool> _taskRefs = new();

    // in-flight file writes across all photos; shutdown waits on this
    private static int _pendingSaveCount;

    private CancellationTokenSource? _cancelThumbnailLoading;
    private double _galleryThumbnailRequestSize;

    /// <summary>
    /// Prevents duplicate concurrent loads of the same photo's thumbnail.
    /// </summary>
    private readonly SemaphoreSlim _thumbnailLock = new(1, 1);

    /// <summary>
    /// Rough transient cost of one thumbnail slot: a cache miss decodes the full image first.
    /// </summary>
    private const uint MB_PER_THUMBNAIL_SLOT = 128;

    /// <summary>
    /// Limits how many different Photo instances load thumbnails concurrently across the entire
    /// app. Every in-flight slot can transiently hold one full-resolution decode, so the ceiling
    /// follows the cache budget and only then the CPU. Lazy so it reads the loaded config.
    /// </summary>
    private static readonly Lazy<SemaphoreSlim> _thumbnailThrottleLock = new(() =>
    {
        var byCpu = Math.Clamp(Environment.ProcessorCount - 1, 4, 16);
        var byBudget = (int)(Core.Config.CacheMaxMemoryInMb / MB_PER_THUMBNAIL_SLOT);
        var slots = Math.Clamp(byBudget, 4, byCpu);

        return new SemaphoreSlim(slots, slots);
    });



    #region Public Propterties

    /// <summary>
    /// Gets the native bitmap,
    /// either <see cref="SKImage"/>, <see cref="AnimatorImpl"/>,
    /// or <see cref="SkiaVectorSource"/>.
    /// </summary>
    public IDisposable? Bitmap { get; private set; } = null;

    /// <summary>
    /// Gets the size of the photo.
    /// </summary>
    public Size Size => new Size(_width, _height);

    /// <summary>
    /// Gets the width of the photo.
    /// </summary>
    public uint Width => (uint)Size.Width;

    /// <summary>
    /// Gets the height of the photo.
    /// </summary>
    public uint Height => (uint)Size.Height;

    /// <summary>
    /// Gets the bytes one decoded pixel occupies. Codec plugins can hand back 16-bit or
    /// half-float frames, so this is not always 4; falls back to 4 before the decode.
    /// </summary>
    public int BytesPerPixel => Bitmap is SKImage img && !img.IsDisposed()
        ? Math.Max(1, img.ColorType.GetBytesPerPixel())
        : 4;

    /// <summary>
    /// Gets the linear scale this photo was decoded at. Below 1 when the full resolution
    /// exceeded what the renderer can hold, so <see cref="Size"/> is smaller than the file.
    /// </summary>
    public double DecodeScale { get; private set; } = 1;

    /// <summary>
    /// Gets the current frame index of this photo.
    /// </summary>
    public int FrameIndex => _frameIndex;

    /// <summary>
    /// Gets the loading state of the photo.
    /// </summary>
    public PhotoState State { get; set; } = PhotoState.None;

    /// <summary>
    /// Gets the codec ID used to decode the photo.
    /// </summary>
    public string CodecId
    {
        get; private set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    } = string.Empty;


    /// <summary>
    /// Gets, sets value indicating if the photo is current index.
    /// </summary>
    public bool IsCurrent
    {
        get; set
        {
            if (field == value) return;
            field = value;
            _ = OnPropertyChanged();
        }
    }


    /// <summary>
    /// Checks if this photo is a clipboard photo.
    /// </summary>
    public bool IsClipboard => string.IsNullOrEmpty(FilePath);


    /// <summary>
    /// Gets file path of the photo. E.g. <c>"C:\Album\My photo.png"</c>.
    /// </summary>
    public string FilePath
    {
        get => Metadata.FilePath; set
        {
            if (!Metadata.FilePath.Equals(value, StringComparison.Ordinal))
            {
                Metadata.FilePath = value;

                OnPropertyChanged(nameof(FilePath));
                OnPropertyChanged(nameof(DirPath));
                OnPropertyChanged(nameof(Metadata));

                OnPropertyChanged(nameof(IsClipboard));
                OnPropertyChanged(nameof(Extension));
                OnPropertyChanged(nameof(FileTitle));
                OnPropertyChanged(nameof(GalleryFileTitle));
                OnPropertyChanged(nameof(GalleryFileExtension));
            }
        }
    }

    /// <summary>
    /// Gets original dir path. E.g: <c>"C:\Album"</c>.
    /// </summary>
    public string DirPath => Path.GetDirectoryName(FilePath) ?? string.Empty;

    /// <summary>
    /// Gets original file extension. E.g: <c>".png"</c>.
    /// </summary>
    public string Extension => Path.GetExtension(FilePath);

    /// <summary>
    /// Gets original file name without extension. E.g. <c>"My photo"</c>.
    /// </summary>
    public string FileTitle => Path.GetFileNameWithoutExtension(FilePath);

    /// <summary>
    /// Gets the file name without extension and including a trailing dot. E.g. <c>"My photo."</c>.
    /// </summary>
    public string GalleryFileTitle => $"{FileTitle}.";

    /// <summary>
    /// Gets file extension without dot. E.g. <c>"png"</c>.
    /// </summary>
    public string GalleryFileExtension => Extension.Length > 1 ? Extension.Substring(1) : string.Empty;




    /// <summary>
    /// Gets the error details.
    /// </summary>
    public Exception? Error { get; set; } = null;

    /// <summary>
    /// Gets, sets options for reading photo.
    /// </summary>
    public PhotoReadOptions ReadOptions { get; set; } = new();

    /// <summary>
    /// Gets, sets the settings for reading Metadata and photo with <see cref="MagickCodec"/>.
    /// </summary>
    public MagickReadSettings? ReadSettings { get; set; } = null;

    /// <summary>
    /// Gets image metadata.
    /// </summary>
    public PhotoMetadata Metadata
    {
        get; private set
        {
            if (field == value) return;
            try
            {
                field.Dispose();
                field = value;
                _ = OnPropertyChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌: Metadata setter: {ex.Message}");
            }
        }
    } = new();

    /// <summary>
    /// Gets photo loading cancellation token source.
    /// </summary>
    public CancellationToken? CancelToken => _cancelPhotoLoading?.Token;

    /// <summary>
    /// Gets, sets the image source for gallery thumbnail.
    /// </summary>
    public Bitmap? GalleryThumbnail
    {
        get; set
        {
            if (field == value) return;

            try
            {
                var old = field;
                field = value;
                _ = OnPropertyChanged();

                // Disposal is the UI thread's job: this usually runs on a worker, where freeing
                // the old bitmap races the gallery still measuring it as its Image.Source.
                if (old is not null) Dispatcher.UIThread.Post(old.Dispose, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌: GalleryThumbnail setter: {ex.Message}");
            }
        }
    }

    #endregion // Public Propterties



    #region Instance Initilization

    /// <summary>
    /// Initializes new instance of <see cref="Photo"/>
    /// </summary>
    public Photo() { }


    /// <summary>
    /// Initializes new instance of <see cref="Photo"/>
    /// </summary>
    public Photo(string filePath = "", PhotoReadOptions? options = null)
    {
        FilePath = filePath;
        ReadOptions = options ?? new();
    }

    /// <summary>
    /// Initializes a photo with filesystem data captured by folder enumeration.
    /// </summary>
    public Photo(FileSearchEntry entry, PhotoReadOptions? options = null)
    {
        Metadata = new PhotoMetadata(entry);
        ReadOptions = options ?? new();
    }

    /// <summary>
    /// Initializes a new single-frame photo using a bitmap source for rendering.
    /// </summary>
    public Photo(SKBitmap? bmp, PhotoState state = PhotoState.Loaded)
    {
        InitializePhoto(bmp, bmp?.Width ?? 0, bmp?.Height ?? 0, null, state);
    }


    /// <summary>
    /// Initializes a new single-frame photo using a image source for rendering.
    /// </summary>
    public Photo(SKImage? img, PhotoState state = PhotoState.Loaded)
    {
        InitializePhoto(img, img?.Width ?? 0, img?.Height ?? 0, null, state);
    }


    /// <summary>
    /// Initializes a new single-frame using a image source for rendering.
    /// </summary>
    public Photo(SKImage? img, PhotoMetadata? meta, PhotoState state = PhotoState.Loaded)
    {
        InitializePhoto(img, 0, 0, meta, state);
    }


    #endregion // Instance Initilization



    #region Override Functions

    /// <summary>
    /// <inheritdoc/>
    /// Calling this function also disposes Metadata object.
    /// </summary>
    protected override async void OnDisposing()
    {
        base.OnDisposing();

        // async void: anything escaping here lands on a worker thread as an unhandled
        // exception, which FailFasts the process with no dialog and nothing logged
        try
        {
            await OnDisposing(true).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(OnDisposing)}: {ex.Message}");
        }
    }


    /// <summary>
    /// Handles the disposal of resources when an object is being disposed.
    /// </summary>
    /// <param name="disposeEverything">
    /// Option to dispose everything or only the Bitmap object.
    /// </param>
    private async Task OnDisposing(bool disposeEverything)
    {
        CancelLoading();

        // let pinned background readers finish, else they read a freed bitmap
        while (!_taskRefs.IsEmpty)
        {
            await Task.Delay(10).ConfigureAwait(false);
        }

        UnloadBitmap();

        // dispose everything
        if (disposeEverything)
        {
            await UnloadThumbnailAsync().ConfigureAwait(false);

            // Do NOT dispose _thumbnailLock: a concurrent LoadThumbnailAsync may
            // still be parked on its WaitAsync (e.g. behind the throttle lock), and
            // disposing it makes that await throw ObjectDisposedException on a
            // background task — surfacing as an unobserved-exception crash. We only
            // use WaitAsync/Release (never AvailableWaitHandle), so the SemaphoreSlim
            // holds no unmanaged handle and is reclaimed safely by the GC.

            // only wait for it to settle; a faulted metadata load (e.g. a plugin codec
            // throwing) must not turn disposal into an unhandled exception
            if (_taskMetadata is not null)
            {
                try
                {
                    await _taskMetadata.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌❌❌ {nameof(OnDisposing)}/metadata: {ex.Message}");
                }
            }

            Metadata.Dispose();
            Metadata = new();

            // same lock as CancelLoading, else it cancels a CTS we just disposed
            lock (_lock)
            {
                _cancelPhotoLoading?.Dispose();
                _cancelPhotoLoading = null;
            }
        }
    }


    #endregion // Override Functions



    #region Private Functions

    private void InitializePhoto(IDisposable? src, int width, int height, PhotoMetadata? meta, PhotoState state = PhotoState.Loaded)
    {
        // set Bitmap
        if (src is null) Bitmap = null;
        else if (src is SKImage img) Bitmap = img;
        else if (src is SKBitmap bmp) Bitmap = SkiaCodec.ToSKImage(bmp);
        else if (src is AnimatorImpl animator) Bitmap = animator;
        else throw new ArgumentException("IGE: Unsupported bitmap source", nameof(src));


        Metadata.Dispose();
        Metadata = meta ?? new()
        {
            Width = (uint)width,
            Height = (uint)height,
            FrameCount = 1,
        };

        _width = (uint)Metadata.Width;
        _height = (uint)Metadata.Height;
        DecodeScale = 1;

        State = state;
    }


    private CodecSelectionContext CreateCodecSelectionContext(PhotoMetadata meta)
    {
        return new CodecSelectionContext
        {
            EnableVectorRenderer = Core.Config.EnableVectorRenderer,
            IsDestColorProfileSupported = Core.IsDestColorProfileSupported,
            LoadRawThumbnailOnly = ReadOptions.OnlyLoadRawPreview && meta.RawThumbnail is not null,
            LoadOtherThumbnailOnly = ReadOptions.OnlyLoadNonRawPreview && (meta.ExifProfile?.ThumbnailLength ?? 0) > 0,
        };
    }


    private void ApplyDecodeResult(CodecDecodeResult result)
    {
        // set decoder codec
        CodecId = result.CodecId;

        _width = (uint)result.Size.Width;
        _height = (uint)result.Size.Height;
        DecodeScale = result.DecodeScale;

        if (result.VectorSource is not null)
        {
            Bitmap = result.VectorSource;
            result.VectorSource = null;
            return;
        }

        if (result.Animator is not null)
        {
            result.Animator.FrameChanged -= OnAnimatorFrameChanged;
            result.Animator.FrameChanged += OnAnimatorFrameChanged;
            Bitmap = result.Animator;
            result.Animator = null;
            return;
        }

        if (result.SingleFrame is not null)
        {
            Bitmap = result.SingleFrame;
            result.SingleFrame = null;
            return;
        }

        Bitmap = null;
    }


    private MagickReadSettings GetOrCreateMagickReadSettings()
    {
        ReadSettings ??= MagickCodec.ParseSettings(ReadOptions, false, FilePath);
        return ReadSettings;
    }


    /// <summary>
    /// Handles the decoding of image files based on their metadata.
    /// </summary>
    private async Task OnDecodingAsync(PhotoMetadata meta, CancellationToken token)
    {
        var context = CreateCodecSelectionContext(meta);
        var selectedCodec = Core.CodecRegistry.SelectDecodeCodec(meta, context);

        // no registered codec claims the file -> let Magick try anyway, it sniffs the content
        var isFallback = selectedCodec is null;
        var codec = selectedCodec ?? Core.CodecRegistry.FallbackDecodeCodec;

        // codec-agnostic trace: covers built-in and plugin codecs uniformly
        var isPlugin = codec is not (SvgCodecAdapter or SkiaCodecAdapter or MagickCodecAdapter);
        PhotoTrace.Mark("decode:codec", FilePath,
            $"{codec.CodecId} (decodePriority={codec.DecodePriority}, plugin={isPlugin}, "
            + $"fallback={isFallback}, frameIndex={ReadOptions.FrameIndex})");

        using var result = await codec.DecodeAsync(meta, ReadOptions, context, token).ConfigureAwait(false);

        // describe before ApplyDecodeResult nulls out the result fields (moves them to Bitmap)
        if (PhotoTrace.Enabled) PhotoTrace.Mark("decode:done", FilePath, $"kind={DescribeDecodeResult(result)}");

        ApplyDecodeResult(result);
    }


    /// <summary>
    /// Formats a decode result for <see cref="PhotoTrace"/> (source kind + dimensions/color type).
    /// </summary>
    private static string DescribeDecodeResult(CodecDecodeResult result)
    {
        if (result.VectorSource is not null) return "vector";
        if (result.Animator is not null) return $"animator frames={result.Animator.Frames.Length}";
        if (result.SingleFrame is SKImage img)
            return $"single-frame {img.Width}x{img.Height} colorType={img.ColorType}";
        return "none";
    }


    /// <summary>
    /// Formats key metadata for <see cref="PhotoTrace"/> (dimensions, frames, HDR, color profile).
    /// </summary>
    private static string DescribeMetadata(PhotoMetadata meta)
    {
        var hdr = meta.IsHdr ? $"HDR({meta.HdrTransferFn})" : "SDR";
        var profile = string.IsNullOrEmpty(meta.ColorProfileName) ? "none" : meta.ColorProfileName;
        return $"{meta.Width}x{meta.Height} frames={meta.FrameCount} ext={meta.FileExtension} "
            + $"vector={meta.IsVector} {hdr} wideGamut={meta.IsWideGamut} bpc={meta.BitsPerChannel} profile={profile}";
    }


    /// <summary>
    /// Keeps <see cref="_frameIndex"/> in sync during animation playback.
    /// </summary>
    private void OnAnimatorFrameChanged(AnimatorImpl sender, AnimatorFrameChangedEventArgs e)
    {
        _frameIndex = (int)e.CurrentFrame;
    }


    #endregion // Private Functions



    #region Public Functions

    /// <summary>
    /// Disposes the Bitmap and resets the relevant data.
    /// This method keeps the Metadata and neccessary resources.
    /// </summary>
    public async void Unload()
    {
        // async void: an escaping exception here would FailFast the process from a worker thread
        try
        {
            // wait for all pending tasks are done
            while (!_taskRefs.IsEmpty)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }

            // reset info
            State = PhotoState.None;
            Error = null;

            // unload image
            await OnDisposing(false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(Unload)}: {ex.Message}");
        }
    }


    /// <summary>
    /// Pins <see cref="Bitmap"/> for a background operation so <see cref="Unload"/> waits for it
    /// instead of disposing the image mid-use. Dispose the returned scope when done.
    /// </summary>
    public IDisposable PinBitmap()
    {
        var taskId = Guid.NewGuid();
        _ = _taskRefs.TryAdd(taskId, true);

        return new BitmapPin(this, taskId);
    }


    private sealed class BitmapPin(Photo photo, Guid taskId) : IDisposable
    {
        public void Dispose() => photo._taskRefs.TryRemove(taskId, out _);
    }


    /// <summary>
    /// Disposes the bitmap only and keeps other relevent data.
    /// </summary>
    public void UnloadBitmap()
    {
        if (Bitmap is AnimatorImpl animator)
        {
            animator.FrameChanged -= OnAnimatorFrameChanged;
        }

        Bitmap?.Dispose();
        Bitmap = null;

        _frameIndex = -1;
    }


    /// <summary>
    /// Stops any ongoing photo loading process.
    /// </summary>
    [MemberNotNull(nameof(_cancelPhotoLoading))]
    public virtual CancellationToken CancelLoading()
    {
        // a cancel must also drop a load still queued behind another one, which holds no token yet
        _ = Interlocked.Increment(ref _cancelEpoch);

        return ResetCancelToken();
    }


    /// <summary>
    /// Replaces the loading token without counting as a cancel of a queued load.
    /// </summary>
    [MemberNotNull(nameof(_cancelPhotoLoading))]
    private CancellationToken ResetCancelToken()
    {
        lock (_lock)
        {
            _cancelPhotoLoading?.Cancel();
            _cancelPhotoLoading?.Dispose();
            _cancelPhotoLoading = new();

            return _cancelPhotoLoading.Token;
        }
    }


    /// <summary>
    /// Loads photo from file.
    /// </summary>
    /// <remarks>
    /// Serialized per photo: cancelling a load in flight instead left its caller with no Loaded event.
    /// </remarks>
    public virtual async Task LoadAsync(bool useCache,
        Func<PhotoLoadingEventArgs, Task>? handleProgressFn = null,
        bool skipLoadingEvent = false)
    {
        // use cached data
        if (useCache && State != PhotoState.None)
        {
            PhotoTrace.Mark("load:cache-hit", FilePath, $"state={State}");
            await DispatchLoadedAsync(handleProgressFn);
            return;
        }

        // no ConfigureAwait(false) below: handleProgressFn renders, so it resumes on the UI thread
        var epoch = Volatile.Read(ref _cancelEpoch);
        await _loadGate.WaitAsync();
        try
        {
            // cancelled while queued, i.e. the caller navigated away before we got our turn
            if (Volatile.Read(ref _cancelEpoch) != epoch)
            {
                PhotoTrace.Mark("load:queued-cancel", FilePath, $"state={State}");
                return;
            }

            // the load we queued behind may have decoded the photo already
            if (useCache && State != PhotoState.None)
            {
                PhotoTrace.Mark("load:cache-hit", FilePath, $"state={State}, afterGate=True");
                await DispatchLoadedAsync(handleProgressFn);
                return;
            }

            await LoadAsync__(useCache, handleProgressFn, skipLoadingEvent);
        }
        finally
        {
            _ = _loadGate.Release();
        }
    }


    /// <summary>
    /// Raises Loaded for an already-decoded photo, so a cache hit still tells its caller to render.
    /// </summary>
    private async Task DispatchLoadedAsync(Func<PhotoLoadingEventArgs, Task>? handleProgressFn)
    {
        if (handleProgressFn is null || State != PhotoState.Loaded) return;

        // CancellationToken.None: the decode is done, there is nothing left to abort
        await handleProgressFn(new(PhotoState.Loaded, this, CancellationToken.None));
    }


    /// <summary>
    /// The load itself; runs one at a time per photo.
    /// </summary>
    private async Task LoadAsync__(bool useCache,
        Func<PhotoLoadingEventArgs, Task>? handleProgressFn,
        bool skipLoadingEvent)
    {
        var token = ResetCancelToken();
        var myGeneration = Interlocked.Increment(ref _loadGeneration);

        PhotoTrace.Begin(FilePath, $"useCache={useCache}, skipLoadingEvent={skipLoadingEvent}");
        try
        {
            // reset dispose status
            _isDisposed.SetFalse();
            State = PhotoState.None;
            Error = null;


            // 1. load metadata ===================
            // cancel if requested
            if (token.IsCancellationRequested) return;

            // load metadata
            await LoadMetadataAsync(useCache, token: token);
            PhotoTrace.Mark("metadata:loaded", FilePath, DescribeMetadata(Metadata));

            if (!skipLoadingEvent)
            {
                if (handleProgressFn is not null)
                {
                    PhotoTrace.Mark("preview:dispatch", FilePath);
                    await handleProgressFn(new(PhotoState.Preview, this, token));
                }
            }


            // 2. load image data ===================
            // cancel if requested
            if (token.IsCancellationRequested) return;

            // decode the photo on a dedicated thread to avoid thread pool starvation
            // (thumbnail loading can saturate the thread pool when OS thumbnail cache is disabled)
            Error = await Task.Factory.StartNew(async () =>
            {
                try
                {
                    await OnDecodingAsync(Metadata, token);
                    return null;
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            if (Error is not null) PhotoTrace.Mark("decode:error", FilePath, Error.Message);

            // cancel if requested
            if (token.IsCancellationRequested) return;


            // done loading
            State = PhotoState.Loaded;
            PhotoTrace.Mark("loaded:dispatch", FilePath, $"hasError={Error is not null}");
            if (handleProgressFn is not null)
            {
                await handleProgressFn(new(PhotoState.Loaded, this, token));
            }
        }
        catch (TaskCanceledException)
        {
            State = PhotoState.None;
        }
        catch (Exception ex)
        {
            Error = ex;
            State = PhotoState.Loaded;
            PhotoTrace.Mark("load:exception", FilePath, ex.Message);
            if (handleProgressFn is not null)
            {
                await handleProgressFn(new(PhotoState.Loaded, this, token));
            }
        }
        finally
        {
            PhotoTrace.End(FilePath, $"state={State}, cancelled={token.IsCancellationRequested}");

            // only unload if no newer load has started on this Photo;
            // a newer LoadAsync increments _loadGeneration, so if ours
            // is stale, calling Unload would cancel the newer load
            if (token.IsCancellationRequested
                && Volatile.Read(ref _loadGeneration) == myGeneration)
            {
                Unload();
            }
        }
    }


    /// <summary>
    /// Loads <c><see cref="Metadata"/></c> for the photo.
    /// Returns the cached metadata if it's not null and up-to-date.
    /// </summary>
    public async Task LoadMetadataAsync(bool useCache, PhotoReadOptions? newOptions = null, CancellationToken token = default)
    {
        var task = Task.Run(() => LoadMetadataAsync__(useCache, newOptions));
        var meta = await task.WaitAsync(token).ConfigureAwait(false);

        if (meta is not null)
        {
            Metadata = meta;
        }
    }
    private async Task<PhotoMetadata?> LoadMetadataAsync__(bool useCache, PhotoReadOptions? newOptions = null)
    {
        try
        {
            ReadOptions = newOptions ?? ReadOptions;

            // if already started loading, wait for the task completes
            if (useCache)
            {
                if (_taskMetadata is not null
                    && _taskMetadata.Status != TaskStatus.Canceled
                    && _taskMetadata.Status != TaskStatus.Faulted)
                {
                    return await _taskMetadata;
                }
            }
            else
            {
                _taskMetadata = null;
            }


            // check if the current Metadata is outdated or not
            var hasOutdatedCache = !useCache || Metadata.IsOutdated();

            // load the metadata if it's outdated
            if (hasOutdatedCache)
            {
                var metadataCodec = Core.CodecRegistry.SelectMetadataCodec(FilePath);
                PhotoTrace.Mark("metadata:codec", FilePath,
                    metadataCodec is null ? "none (fallback)" : $"{metadataCodec.CodecId} (metaPriority={metadataCodec.MetadataPriority})");

                // load metadata off-thread
                if (metadataCodec is not null)
                {
                    _taskMetadata = Task.Run(() => metadataCodec.LoadMetadataAsync(
                        FilePath,
                        ReadOptions,
                        CancellationToken.None));
                }
                else
                {
                    _taskMetadata = Task.Run(() => new PhotoMetadata(FilePath));
                }

                // must assign on UI thread
                return await _taskMetadata;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌❌❌ {nameof(LoadMetadataAsync__)}: {ex.Message}");
        }

        return null;
    }


    /// <summary>
    /// Gets an image frame from the photo.
    /// </summary>
    public async Task<SKImage?> GetFrameAsync(uint frameIndex)
    {
        var newFrameIndex = (int)frameIndex;

        // 1. animated formats: delegate to animator's own frame cache
        if (Bitmap is AnimatorImpl animator)
        {
            _frameIndex = newFrameIndex;
            return animator.GetRenderedFrameBitmap(frameIndex);
        }

        // 2. cache hit: requested frame is already loaded in Bitmap
        if (frameIndex == _frameIndex && Bitmap is SKImage cachedImg)
        {
            return cachedImg;
        }


        // 3. single-frame image: nothing else to decode
        if (Metadata.FrameCount <= 1)
        {
            _frameIndex = newFrameIndex;
            return Bitmap as SKImage;
        }


        // 4. multi-frame: decode the requested frame via the registered codec
        // (Magick built-in or any plugin codec that supports the format).
        var newFrame = await Task.Factory.StartNew(async () =>
        {
            var options = ReadOptions with { FrameIndex = newFrameIndex };
            var context = CreateCodecSelectionContext(Metadata);
            var codec = Core.CodecRegistry.SelectDecodeCodec(Metadata, context);
            if (codec is not null)
            {
                using var result = await codec.DecodeAsync(Metadata, options, context, CancellationToken.None).ConfigureAwait(false);
                if (result.SingleFrame is SKImage sf)
                {
                    // update decoder codec
                    CodecId = result.CodecId;

                    // Detach the frame from the result so its dispose doesn't free our image.
                    var detached = sf;
                    result.SingleFrame = null;
                    return detached;
                }
            }

            // Fallback: legacy direct-Magick path (e.g. SVG vector codec returned no raster).
            using var data = await MagickCodec.DecodeImageAsync(Metadata,
                options, GetOrCreateMagickReadSettings(), null, CancellationToken.None);
            return SkiaCodec.FromMagick(data.SingleFrame, Metadata.SkiaColorSpace, Metadata.IsHdr);
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();


        lock (_lock)
        {
            if (IsDisposed)
            {
                newFrame?.Dispose();
                return null;
            }

            Bitmap?.Dispose();
            Bitmap = newFrame;
            _frameIndex = newFrameIndex;
            _width = (uint)(newFrame?.Width ?? 0);
            _height = (uint)(newFrame?.Height ?? 0);
        }

        return newFrame;
    }


    /// <summary>
    /// Decodes a fresh copy of the given frame directly from the source, WITHOUT touching the cached
    /// <see cref="Bitmap"/> or the current frame index. The caller owns the returned image. Used by
    /// the viewer to (re)capture the pre-tone-map HDR frame for live re-tone-mapping without a full,
    /// display-disrupting reload.
    /// </summary>
    public async Task<SKImage?> DecodeStaticFrameAsync(uint frameIndex, CancellationToken token = default)
    {
        var newFrameIndex = (int)frameIndex;

        return await Task.Factory.StartNew(async () =>
        {
            var options = ReadOptions with { FrameIndex = newFrameIndex };
            var context = CreateCodecSelectionContext(Metadata);
            var codec = Core.CodecRegistry.SelectDecodeCodec(Metadata, context);
            if (codec is not null)
            {
                using var result = await codec.DecodeAsync(Metadata, options, context, token).ConfigureAwait(false);
                if (result.SingleFrame is SKImage sf)
                {
                    // detach so the result's dispose doesn't free the returned image
                    var detached = sf;
                    result.SingleFrame = null;
                    return detached;
                }
            }

            // fallback: direct Magick decode
            using var data = await MagickCodec.DecodeImageAsync(Metadata, options,
                GetOrCreateMagickReadSettings(), null, token);
            return SkiaCodec.FromMagick(data.SingleFrame, Metadata.SkiaColorSpace, Metadata.IsHdr);
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }


    /// <summary>
    /// Saves the photo to file.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public async Task SaveAsAsync(string destFilePath,
        PhotoTransform transforms, uint quality,
        bool preserveModifiedDate = false, CancellationToken token = default)
    {
        var taskId = Guid.NewGuid();
        _ = _taskRefs.TryAdd(taskId, true);
        _ = Interlocked.Increment(ref _pendingSaveCount);

        try
        {
            var lastWriteTime = File.GetLastWriteTime(destFilePath);

            // 0. a plugin encoder claiming this extension wins; otherwise fall through unchanged
            var handled = await CodecEncodePipeline.TryEncodeAsync(this, destFilePath, transforms, quality, token);

            // 1. save clipboard photo to file
            if (!handled && IsClipboard && Bitmap is SKImage img)
            {
                await SaveStaging.WriteThenPromoteAsync(destFilePath, stagePath =>
                    Task.Factory.StartNew(async () =>
                    {
                        await SkiaCodec.SaveAsync(img, stagePath, transforms, quality, token);
                    }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap(), token);
            }

            // 2. save photo to file
            else if (!handled)
            {
                // update read options
                var readOptions = ReadOptions with
                {
                    FrameIndex = Metadata.FrameCount > 1
                        ? -1 // save all frame
                        : ReadOptions.FrameIndex, // save only current frame
                };

                await SaveStaging.WriteThenPromoteAsync(destFilePath, stagePath =>
                    Task.Factory.StartNew(async () =>
                    {
                        await MagickCodec.SaveAsync(Metadata, stagePath, readOptions, transforms, quality, token);
                    }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap(), token);
            }


            // Issue #307: option to preserve the modified date/time
            if (preserveModifiedDate)
            {
                File.SetLastWriteTime(destFilePath, lastWriteTime);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _ = Interlocked.Decrement(ref _pendingSaveCount);
            _ = _taskRefs.TryRemove(taskId, out _);
        }
    }


    /// <summary>
    /// Number of file writes currently in flight across all photos.
    /// </summary>
    public static int PendingSaveCount => Volatile.Read(ref _pendingSaveCount);


    /// <summary>
    /// Waits until every in-flight save has finished, so the app cannot tear down mid-write.
    /// </summary>
    public static async Task WaitForPendingSavesAsync(TimeSpan timeout)
    {
        var startedAt = Stopwatch.StartNew();

        while (Volatile.Read(ref _pendingSaveCount) > 0 && startedAt.Elapsed < timeout)
        {
            await Task.Delay(20).ConfigureAwait(false);
        }
    }


    /// <summary>
    /// Signals cancellation to any in-progress thumbnail loading so it exits early.
    /// </summary>
    public void CancelThumbnailLoading()
    {
        // _thumbnailLock is deliberately not held here so the holder sees the cancel and
        // releases sooner, which means it may be disposing this CTS right now
        try
        {
            _cancelThumbnailLoading?.Cancel();
        }
        catch (ObjectDisposedException) { }
    }


    /// <summary>
    /// Loads a thumbnail image at the specified DPI scaling factor.
    /// </summary>
    public async Task LoadThumbnailAsync(double dpi)
    {
        var thumbSize = Core.Config.ThumbnailSize * dpi * 2; // 2x bigger
        await LoadThumbnailAsync(thumbSize, useCache: true);
    }


    /// <summary>
    /// Loads the gallery thumbnail asynchronously.
    /// Uses a semaphore to ensure only one load per photo at a time,
    /// and caches the result on <see cref="GalleryThumbnail"/>.
    /// </summary>
    public async Task LoadThumbnailAsync(double thumbSize, bool useCache, CancellationToken cancellationToken = default)
    {
        // 1. fast path: use cached thumbnail
        if (useCache && GalleryThumbnail is not null && _galleryThumbnailRequestSize == thumbSize) return;
        if (IsDisposed || string.IsNullOrEmpty(FilePath)) return;

        // 2. acquire global throttle to avoid saturating the thread pool
        //    (prevents blocking the main image loading when many thumbnails load at once)
        await _thumbnailThrottleLock.Value.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _thumbnailLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // 3. double-check after acquiring the lock
                if (useCache && GalleryThumbnail is not null && _galleryThumbnailRequestSize == thumbSize) return;
                if (IsDisposed) return;

                // reset cancellation for this load
                _cancelThumbnailLoading?.Dispose();
                _cancelThumbnailLoading = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var token = _cancelThumbnailLoading.Token;


                // 4. try caches without reading the source file
                using var diskThumb = useCache
                    ? await ThumbnailDiskCache.TryGetAsync(
                        FilePath, (int)thumbSize, Metadata.FileLastWriteTimeUtc, token)
                        .ConfigureAwait(false)
                    : null;
                var didProbePlatformCache = useCache && diskThumb.IsDisposed();
                using var platformThumb = didProbePlatformCache
                    ? await Core.PreviewProvider.TryGetCachedThumbnailAsync(
                        FilePath, (int)thumbSize, token).ConfigureAwait(false)
                    : null;
                var cachedThumb = platformThumb ?? diskThumb;

                if (!cachedThumb.IsDisposed())
                {
                    // the Shell hands back whole cache tiers; retaining one costs MBs per photo
                    using var scaledThumb = thumbSize > 0
                        && (cachedThumb.Width > thumbSize || cachedThumb.Height > thumbSize)
                        ? await Task.Run(() => SkiaCodec.ScaleDown(cachedThumb, thumbSize), token)
                            .ConfigureAwait(false)
                        : null;

                    var avBitmapCached = await Task.Run(
                        () => SkiaCodec.ToWritableBitmap(scaledThumb ?? cachedThumb), token)
                        .ConfigureAwait(false);

                    if (avBitmapCached is null) return;
                    if (!await ShowGalleryThumbnailAsync(avBitmapCached, token)) return;

                    // The requested size includes a supersampling margin.
                    // If the cached image is smaller than the displayed size,
                    // show it now but keep looking for a sharper replacement.
                    var cachedThumbMayBeTooSmall = thumbSize > 0
                        && Math.Max(cachedThumb.Width, cachedThumb.Height)
                            < thumbSize * PhotoPreviewProvider.MIN_PREVIEW_SIZE_RATIO;
                    if (!cachedThumbMayBeTooSmall)
                    {
                        _galleryThumbnailRequestSize = thumbSize;
                        return;
                    }
                }

                // 5. load metadata if needed
                await LoadMetadataAsync(true, token: token).ConfigureAwait(false);
                if (token.IsCancellationRequested) return;

                // a small source cannot produce a larger thumbnail.
                if (PhotoPreviewProvider.IsPreviewLargeEnough(cachedThumb, Metadata, thumbSize))
                {
                    _galleryThumbnailRequestSize = thumbSize;
                    return;
                }


                // 6. get thumbnail from platform provider
                PhotoTrace.Mark("thumb:provider", null, $"{FilePath} @ {(int)thumbSize}px");
                var swThumb = PhotoTrace.Enabled ? Stopwatch.StartNew() : null;
                using var skThumb = await Task.Run(
                    () => Core.PreviewProvider.GetThumbnailAsync(
                        Metadata, thumbSize, token, didProbePlatformCache), token)
                    .ConfigureAwait(false);
                if (token.IsCancellationRequested || skThumb.IsDisposed()) return;

                if (swThumb is not null)
                {
                    PhotoTrace.Mark("thumb:provider-done", null,
                        $"{FilePath} -> {skThumb.Width}x{skThumb.Height} in {swThumb.ElapsedMilliseconds}ms");
                }


                // 7. convert SKImage to Avalonia Bitmap
                var avBitmap = await Task.Run(
                    () => SkiaCodec.ToWritableBitmap(skThumb), token)
                    .ConfigureAwait(false);

                // 8. update the gallery thumbnail (triggers UI binding update)
                if (avBitmap is null) return;
                if (!await ShowGalleryThumbnailAsync(avBitmap, token)) return;
                _galleryThumbnailRequestSize = thumbSize;


                // 9. write to disk cache last, so encoding never delays the thumbnail appearing.
                // Awaited (not fire-and-forget) to keep skThumb alive until the encode is done.
                await ThumbnailDiskCache.PutAsync(FilePath, (int)thumbSize, skThumb, token)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌❌❌ {nameof(LoadThumbnailAsync)}: {ex.Message}");
            }
            finally
            {
                _thumbnailLock.Release();
            }
        }
        finally
        {
            _thumbnailThrottleLock.Value.Release();
        }
    }


    /// <summary>
    /// Cancels pending thumbnail loading and disposes the cached thumbnail.
    /// </summary>
    public async Task UnloadThumbnailAsync()
    {
        // Signal cancellation first so any in-progress load
        // can exit early and release the lock sooner.
        CancelThumbnailLoading();

        await _thumbnailLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cancelThumbnailLoading?.Dispose();
            _cancelThumbnailLoading = null;

            await ClearGalleryThumbnailAsync();
        }
        finally
        {
            _thumbnailLock.Release();
        }
    }


    private async Task<bool> ShowGalleryThumbnailAsync(Bitmap thumbnail, CancellationToken token)
    {
        if (token.IsCancellationRequested || IsDisposed)
        {
            thumbnail.Dispose();
            return false;
        }

        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested || IsDisposed)
            {
                thumbnail.Dispose();
                return false;
            }

            GalleryThumbnail = thumbnail;
            return true;
        });
    }


    private async Task ClearGalleryThumbnailAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            GalleryThumbnail = null;
            _galleryThumbnailRequestSize = 0;
        });
    }


    #endregion // Public Functions


}



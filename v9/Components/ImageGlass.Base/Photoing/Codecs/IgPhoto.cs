namespace ImageGlass.Base.Photoing.Codecs;

/// <summary>
/// Initialize <see cref="IgPhoto"/> instance
/// </summary>
public class IgPhoto : IDisposable
{
    #region IDisposable Disposing

    public bool IsDisposed { get; private set; } = false;

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            Unload();
        }

        // Free any unmanaged objects here.
        IsDisposed = true;
    }

    public virtual void Dispose()
    {
        CancelLoading();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~IgPhoto()
    {
        Dispose(false);
    }

    #endregion


    private CancellationTokenSource? _tokenSrc;


    #region Public properties

    /// <summary>
    /// Gets, sets original filename.
    /// </summary>
    public string OriginalFilename { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets working filename.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets file extension. E.g: <c>.png</c>.
    /// </summary>
    public string Extension => Path.GetExtension(Filename);

    /// <summary>
    /// Gets the error details
    /// </summary>
    public Exception? Error { get; private set; } = null;

    /// <summary>
    /// Gets the value indicates that image loading is done.
    /// </summary>
    public bool IsDone { get; private set; } = false;

    /// <summary>
    /// Gets, sets number of image frames.
    /// </summary>
    public int FramesCount { get; set; } = 0;

    /// <summary>
    /// Gets the first frame of image.
    /// </summary>
    public Bitmap? Image { get; internal set; }

    #endregion


    /// <summary>
    /// Initializes <see cref="IgPhoto"/> instance.
    /// </summary>
    /// <param name="filename"></param>
    public IgPhoto(string filename)
    {
        Filename = filename;
    }


    #region Public functions

    /// <summary>
    /// Load the photo.
    /// </summary>
    /// <param name="codec"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    private async Task LoadImageAsync(
        IIgCodec? codec,
        CodecReadOptions? options = null)
    {
        // reset dispose status
        IsDisposed = false;

        // reset done status
        IsDone = false;

        // reset error
        Error = null;

        options ??= new();

        try
        {
            if (codec == null)
                throw new NullReferenceException(nameof(codec));

            // load image data
            var metadata = codec.LoadMetadata(Filename, options);
            FramesCount = metadata?.FramesCount ?? 0;

            options = options with
            {
                FirstFrameOnly = FramesCount < 2,
            };

            // cancel if requested
            if (_tokenSrc is not null && _tokenSrc.IsCancellationRequested)
            {
                _tokenSrc.Token.ThrowIfCancellationRequested();
            }

            // load image
            Image = await codec.LoadAsync(Filename, options, _tokenSrc?.Token);

            // cancel if requested
            if (_tokenSrc is not null && _tokenSrc.IsCancellationRequested)
            {
                _tokenSrc.Token.ThrowIfCancellationRequested();
            }

            // done loading
            IsDone = true;
        }
        catch (OperationCanceledException) {
            Unload();
            Dispose();
        }
        catch (Exception ex)
        {
            // save the error
            Error = ex;

            // done loading
            IsDone = true;
        }
    }


    /// <summary>
    /// Read and load image into memory.
    /// </summary>
    /// <param name="codec"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task LoadAsync(
        IIgCodec? codec,
        CodecReadOptions? options = null,
        CancellationTokenSource? tokenSrc = null)
    {
        _tokenSrc = tokenSrc ?? new();

        await LoadImageAsync(codec, options);
    }

    /// <summary>
    /// Unload the image and reset the relevant info
    /// </summary>
    public void Unload()
    {
        // reset info
        IsDone = false;
        Error = null;
        FramesCount = 0;

        // unload image
        Image?.Dispose();
        Image = null;
    }


    /// <summary>
    /// Cancels image loading.
    /// </summary>
    public void CancelLoading()
    {
        _tokenSrc?.Cancel();
    }

    #endregion

}

namespace ImageGlass.Base.Photoing.Codecs;

/// <summary>
/// Initialize <see cref="IgPhoto"/> instance
/// </summary>
public class IgPhoto : IDisposable
{
    #region IDisposable Disposing

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Free any other managed objects here.
            Image?.Dispose();
        }

        // Free any unmanaged objects here.
        _disposed = true;
    }

    public virtual void Dispose()
    {
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
    /// Gets, sets image codec.
    /// </summary>
    public IIgCodec? Codec { get; set; }

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
    public IgPhoto() { }


    /// <summary>
    /// Initializes <see cref="IgPhoto"/> instance.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="codec"></param>
    public IgPhoto(string filename)
    {
        Filename = filename;
    }


    #region Public functions

    /// <summary>
    /// Load the photo.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task LoadAsync(CodecReadOptions? options = null)
    {
        _tokenSrc?.Cancel();
        _tokenSrc = new();

        // reset done status
        IsDone = false;

        // reset error
        Error = null;

        options ??= new();

        try
        {
            if (Codec == null)
                throw new NullReferenceException(nameof(Codec));

            // load image data
            var metadata = Codec.LoadMetadata(Filename, options);
            FramesCount = metadata?.FramesCount ?? 0;

            options = options with
            {
                FirstFrameOnly = FramesCount < 2,
            };

            // load image
            Image = await Codec.LoadAsync(Filename, options, _tokenSrc.Token);

        }
        catch (TaskCanceledException) { }
        catch (Exception ex)
        {
            // save the error
            Error = ex;
        }

        // done loading
        IsDone = true;
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

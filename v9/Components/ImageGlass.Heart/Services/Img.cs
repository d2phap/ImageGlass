using HapplaBox.Heart;

namespace ImageGlass.Heart;

public class Img : IDisposable
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
            IsDone = false;
            Error = null;
            Image = null;
        }

        // Free any unmanaged objects here.
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Img()
    {
        Dispose(false);
    }

    #endregion



    #region PUBLIC PROPERTIES

    /// <summary>
    /// Gets, sets image codec
    /// </summary>
    private IIgCodec Codec { get; set; }


    /// <summary>
    /// Gets the error details
    /// </summary>
    public Exception? Error { get; private set; } = null;


    /// <summary>
    /// Gets the value indicates that image loading is done
    /// </summary>
    public bool IsDone { get; private set; } = false;


    /// <summary>
    /// Gets, sets original filename of Img
    /// </summary>
    public string OriginalFilename { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets working filename of Img
    /// </summary>
    public string Filename { get; set; } = string.Empty;


    /// <summary>
    /// Gets, sets Bitmap data
    /// </summary>
    public Bitmap? Image { get; set; } = null;


    #endregion



    /// <summary>
    /// The Img class contain image data
    /// </summary>
    /// <param name="filename">Image filename</param>
    /// <param name="codec">The codec to decode image file</param>
    public Img(string filename, IIgCodec codec)
    {
        Filename = filename;
        Codec = codec;
    }


    /// <summary>
    /// Load image
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public async Task LoadAsync(int width = -1, int height = -1)
    {
        // reset done status
        IsDone = false;

        // reset error
        Error = null;

        using var photo = new Photo(Codec);

        try
        {
            // load image data
            Image = await photo.LoadAsync(Filename, new CodecReadSettings()
            {
                Width = width,
                Height = height,
            });
        }
        catch (Exception ex)
        {
            // save the error
            Error = ex;
        }


        // done loading
        IsDone = true;
    }
}


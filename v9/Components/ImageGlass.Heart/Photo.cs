
using ImageGlass.Heart;

namespace HapplaBox.Heart;


/// <summary>
/// Handles image file
/// </summary>
public class Photo : IDisposable
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
        }

        // Free any unmanaged objects here.
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Photo()
    {
        Dispose(false);
    }

    #endregion


    /// <summary>
    /// Gets codec to decode image
    /// </summary>
    private IIgCodec Codec { get; }


    /// <summary>
    /// Initilizes Photo instance
    /// </summary>
    /// <param name="codec"></param>
    public Photo(IIgCodec codec)
    {
        Codec = codec;
    }


    /// <summary>
    /// Load an image file
    /// </summary>
    /// <param name="filename">Image file name</param>
    /// <param name="settings">Image loading settings</param>
    /// <returns></returns>
    public Bitmap? Load(string filename, CodecReadSettings settings = default)
    {
        var bmp = Codec.Load(filename, settings);

        return bmp;
    }


    /// <summary>
    /// Load an image file
    /// </summary>
    /// <param name="filename">Image file name</param>
    /// <param name="settings">Image loading settings</param>
    /// <returns></returns>
    public Task<Bitmap?> LoadAsync(string filename, CodecReadSettings settings = default)
    {
        return Task.Run(() =>
        {
            var bmp = Load(filename, settings);

            return bmp;
        });
    }

}


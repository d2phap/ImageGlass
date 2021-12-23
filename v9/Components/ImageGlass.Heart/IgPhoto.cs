namespace ImageGlass.Heart;

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
            FirstFrame?.Dispose();

            foreach (var bmp in AllFrames)
            {
                bmp.Dispose();
            }
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


    public IgMetadata? Metadata { get; set; }

    public Bitmap? FirstFrame { get; set; }

    public List<Bitmap> AllFrames { get; set; } = new(0);

}

public class IgMetadata
{
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;

    public int FrameCount { get; set; } = 0;
}

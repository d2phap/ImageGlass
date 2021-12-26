
namespace ImageGlass.PhotoBox;

/// <summary>
/// Panning event arguments
/// </summary>
public class PanningEventArgs : EventArgs
{
    /// <summary>
    /// Gets current mouse pointer location on host control
    /// </summary>
    public PointF HostLocation { get; private set; } = new(0, 0);

    /// <summary>
    /// Gets panning start mouse pointer location on host control
    /// </summary>
    public PointF HostStartLocation { get; private set; } = new(0, 0);


    public PanningEventArgs(PointF loc, PointF startLoc)
    {
        HostLocation = loc;
        HostStartLocation = startLoc;
    }
}


/// <summary>
/// Zoom event arguments
/// </summary>
public class ZoomEventArgs : EventArgs
{
    /// <summary>
    /// Gets zoom factor
    /// </summary>
    public float ZoomFactor { get; private set; } = 0f;


    public ZoomEventArgs(float zoomFactor)
    {
        ZoomFactor = zoomFactor;
    }
}


/// <summary>
/// MouseMouse event arguments
/// </summary>
public class ImageMouseMoveEventArgs : EventArgs
{
    /// <summary>
    /// Gets the x-coordinate of the image
    /// </summary>
    public float ImageX { get; private set; } = 0;

    /// <summary>
    /// Gets the y-coordinate of the image
    /// </summary>
    public float ImageY { get; private set; } = 0;

    /// <summary>
    /// Gets which mouse button was pressed
    /// </summary>
    public MouseButtons Button { get; private set; } = MouseButtons.Left;


    public ImageMouseMoveEventArgs(float imgX, float imgY, MouseButtons button)
    {
        Button = button;
        ImageX = imgX;
        ImageY = imgY;
    }
}
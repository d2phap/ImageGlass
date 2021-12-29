
namespace ImageGlass.Base.QueuedWorker;

/// <summary>
/// Represents the mode in which the work items of <see cref="QueuedBackgroundWorker"/> are processed.
/// </summary>
public enum ProcessingMode
{
    /// <summary>
    /// Items are processed in the order they are received.
    /// </summary>
    FIFO,
    /// <summary>
    /// Items are processed in reverse order.
    /// </summary>
    LIFO,
}

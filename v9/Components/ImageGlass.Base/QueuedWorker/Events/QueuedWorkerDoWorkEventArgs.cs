using System.ComponentModel;

namespace ImageGlass.Base.QueuedWorker;



/// <summary>
/// Represents the method that will handle the DoWork event.
/// </summary>
/// <param name="sender">The object that is the source of the event.</param>
/// <param name="e">An <see cref="QueuedWorkerDoWorkEventArgs"/> that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void QueuedWorkerDoWorkEventHandler(object sender, QueuedWorkerDoWorkEventArgs e);


/// <summary>
/// Represents the event arguments of the DoWork event.
/// </summary>
public class QueuedWorkerDoWorkEventArgs : DoWorkEventArgs
{
    /// <summary>
    /// Gets the priority of this item.
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Initializes a new instance of the QueuedWorkerDoWorkEventArgs class.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    /// <param name="priority">A value between 0 and 5 indicating the priority of this item.</param>
    public QueuedWorkerDoWorkEventArgs(object argument, int priority)
        : base(argument)
    {
        Priority = priority;
    }
}

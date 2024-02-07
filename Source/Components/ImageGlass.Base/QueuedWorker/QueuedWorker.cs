/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using System.ComponentModel;

namespace ImageGlass.Base.QueuedWorker;


/// <summary>
/// A background worker with a work queue.
/// </summary>
public class QueuedWorker : Component
{

    #region Member Variables

    private bool _isDisposed = false;
    private readonly object _lockObject = new();

    private Thread[] _threads = [];
    private ProcessingMode _processingMode = ProcessingMode.FIFO;
    private int _threadCount = 5;
    private string _threadName = string.Empty;

    private bool _isStopping = false;
    private bool _isStarted = false;
    private bool _isPaused = false;

    private int _priorityQueues = 5;
    private LinkedList<AsyncOperation>[] _items = [];
    private AsyncOperation?[] _singleItems = [];
    private readonly Dictionary<object, bool> _cancelledItems = [];

    private readonly SendOrPostCallback _workCompletedCallback;

    #endregion


    #region Properties

    /// <summary>
    /// Represents the mode in which the work items are processed.
    /// Processing mode cannot be changed after any work is added to the work queue.
    /// </summary>
    [Browsable(true), Category("Behaviour"), DefaultValue(typeof(ProcessingMode), "FIFO")]
    public ProcessingMode ProcessingMode
    {
        get => _processingMode;
        set
        {
            if (_isStarted)
                throw new ThreadStateException("The thread has already been started.");

            _processingMode = value;
            BuildWorkQueue();
        }
    }

    /// <summary>
    /// Gets or sets the number of priority queues. Number of queues
    /// cannot be changed after any work is added to the work queue.
    /// </summary>
    [Browsable(true), Category("Behaviour"), DefaultValue(5)]
    public int PriorityQueues
    {
        get => _priorityQueues;
        set
        {
            if (_isStarted)
                throw new ThreadStateException("The thread has already been started.");

            _priorityQueues = value;
            BuildWorkQueue();
        }
    }

    /// <summary>
    /// Determines whether the <see cref="QueuedWorker"/> started working.
    /// </summary>
    [Browsable(false), Category("Behavior")]
    public bool Started => _isStarted;

    /// <summary>
    /// Gets or sets a value indicating whether or not the worker thread is a background thread.
    /// </summary>
    [Browsable(true), Category("Behavior")]
    public bool IsBackground
    {
        get => _threads[0].IsBackground;
        set
        {
            for (int i = 0; i < _threadCount; i++)
                _threads[i].IsBackground = value;
        }
    }

    /// <summary>
    /// Determines whether the <see cref="QueuedWorker"/> is paused.
    /// </summary>
    private bool Paused
    {
        get
        {
            lock (_lockObject)
            {
                return _isPaused;
            }
        }
    }

    /// <summary>
    /// Determines whether the <see cref="QueuedWorker"/> is being stopped.
    /// </summary>
    private bool Stopping
    {
        get
        {
            lock (_lockObject)
            {
                return _isStopping;
            }
        }
    }

    /// <summary>
    /// Gets or sets the number of worker threads. Number of threads
    /// cannot be changed after any work is added to the work queue.
    /// </summary>
    [Browsable(true), Category("Behaviour"), DefaultValue(5)]
    public int Threads
    {
        get => _threadCount;
        set
        {
            if (_isStarted)
                throw new ThreadStateException("The thread has already been started.");

            _threadCount = value;
            CreateThreads();
        }
    }

    /// <summary>
    /// Represents the name of the worker threads.
    /// </summary>
    [Browsable(true), Category("Behaviour"), DefaultValue("")]
    public string ThreadName
    {
        get => _threadName;
        set
        {
            if (_isStarted)
                throw new ThreadStateException("The thread has already been started.");

            _threadName = value;
            CreateThreads();
        }
    }

    #endregion


    #region Public Events

    /// <summary>
    /// Occurs when the background operation of an item has completed,
    /// has been canceled, or has raised an exception.
    /// </summary>
    public event RunQueuedWorkerCompletedEventHandler? RunWorkerCompleted;

    /// <summary>
    /// Occurs when <see cref="RunWorkerAsync(object, int)" /> is called.
    /// </summary>
    public event QueuedWorkerDoWorkEventHandler? DoWork;

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedWorker"/> class.
    /// </summary>
    public QueuedWorker()
    {
        // Threads
        CreateThreads();

        // Work items
        BuildWorkQueue();

        // The loader complete callback
        _workCompletedCallback = new SendOrPostCallback(RunWorkerCompletedCallback);
    }

    #endregion


    #region Public Methods

    /// <summary>
    /// Starts processing a new background operation.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    /// <param name="priority">A value between 0 and <see cref="PriorityQueues"/> indicating the priority of this item.
    /// An item with a higher priority will be processed before items with lower priority.</param>
    /// <param name="single">true to run this operation without waiting for queued items; otherwise
    /// false to add this operatino to th queue.</param>
    public void RunWorkerAsync(object? argument, int priority, bool single)
    {
        if (priority < 0 || priority >= _priorityQueues)
            throw new ArgumentException("priority must be between 0 and " + (_priorityQueues - 1).ToString() + "  inclusive.", nameof(priority));

        // Start the worker threads
        if (!_isStarted)
        {
            // Start the thread
            for (int i = 0; i < _threadCount; i++)
            {
                _threads[i].Start();
                while (!_threads[i].IsAlive) { }
            }

            _isStarted = true;
        }

        lock (_lockObject)
        {
            AddWork(argument, priority, single);
            Monitor.Pulse(_lockObject);
        }
    }

    /// <summary>
    /// Starts processing a new background operation.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    /// <param name="priority">A value between 0 and <see cref="PriorityQueues"/> indicating the priority of this item.
    /// An item with a higher priority will be processed before items with lower priority.</param>
    public void RunWorkerAsync(object argument, int priority)
    {
        RunWorkerAsync(argument, priority, false);
    }

    /// <summary>
    /// Starts processing a new background operation.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    public void RunWorkerAsync(object argument)
    {
        RunWorkerAsync(argument, 0, false);
    }

    /// <summary>
    /// Starts processing a new background operation.
    /// </summary>
    public void RunWorkerAsync()
    {
        RunWorkerAsync(null, 0, false);
    }

    /// <summary>
    /// Pauses the worker.
    /// </summary>
    public void Pause()
    {
        lock (_lockObject)
        {
            _isPaused = true;
            Monitor.Pulse(_lockObject);
        }
    }

    /// <summary>
    /// Resumes processing pending operations in the work queue.
    /// </summary>
    public void Resume()
    {
        lock (_lockObject)
        {
            _isPaused = false;
            Monitor.Pulse(_lockObject);
        }
    }

    /// <summary>
    /// Cancels all pending operations in all queues.
    /// </summary>
    public void CancelAsync()
    {
        lock (_lockObject)
        {
            ClearWorkQueue();
            Monitor.Pulse(_lockObject);
        }
    }

    /// <summary>
    /// Cancels all pending operations in the given queue.
    /// </summary>
    /// <param name="priority">A value between 0 and <see cref="PriorityQueues"/> 
    /// indicating the priority queue to cancel.</param>
    public void CancelAsync(int priority)
    {
        if (priority < 0 || priority >= _priorityQueues)
            throw new ArgumentException("priority must be between 0 and " + (_priorityQueues - 1).ToString() + "  inclusive.", nameof(priority));

        lock (_lockObject)
        {
            ClearWorkQueue(priority);
            Monitor.Pulse(_lockObject);
        }
    }

    /// <summary>
    /// Cancels processing the item with the given key.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    public void CancelAsync(object argument)
    {
        lock (_lockObject)
        {
            if (_cancelledItems.TryAdd(argument, false))
            {
                Monitor.Pulse(_lockObject);
            }
        }
    }

    /// <summary>
    /// Gets the apartment state of worker threads.
    /// </summary>
    /// <returns>The apartment state of worker threads.</returns>
    public ApartmentState GetApartmentState()
    {
        return _threads[0].GetApartmentState();
    }

    /// <summary>
    /// Sets the apartment state of worker threads. The apartment state
    /// cannot be changed after any work is added to the work queue.
    /// </summary>
    /// <param name="state">The new state of worker threads.</param>
    public void SetApartmentState(ApartmentState state)
    {
        for (int i = 0; i < _threadCount; i++)
            _threads[i].SetApartmentState(state);
    }

    #endregion


    #region Private Methods

    /// <summary>
    /// Determines if the work queue is empty.
    /// This method must be called from inside a lock.
    /// </summary>
    /// <returns>true if the work queue is empty; otherwise false.</returns>
    private bool IsWorkQueueEmpty()
    {
        foreach (var asyncOp in _singleItems)
        {
            if (asyncOp != null) return false;
        }

        foreach (LinkedList<AsyncOperation> queue in _items)
        {
            if (queue.Count > 0) return false;
        }

        return true;
    }

    /// <summary>
    /// Adds the operation to the work queue.
    /// This method must be called from inside a lock.
    /// </summary>
    /// <param name="argument">The argument of an asynchronous operation.</param>
    /// <param name="priority">A value between 0 and <see cref="PriorityQueues"/> indicating the priority of this item.
    /// An item with a higher priority will be processed before items with lower priority.</param>
    /// <param name="single">true to run this operation without waiting for queued items; otherwise
    /// false to add this operatino to th queue.</param>
    private void AddWork(object? argument, int priority, bool single)
    {
        // Create an async operation for this work item
        var asyncOp = AsyncOperationManager.CreateOperation(argument);

        if (single)
        {
            var currentOp = _singleItems[priority];
            currentOp?.OperationCompleted();

            _singleItems[priority] = asyncOp;
        }
        else if (_processingMode == ProcessingMode.FIFO)
        {
            _items[priority].AddLast(asyncOp);
        }
        else
        {
            _items[priority].AddFirst(asyncOp);
        }
    }

    /// <summary>
    /// Gets a pending operation from the work queue.
    /// This method must be called from inside a lock.
    /// </summary>
    /// <returns>A 2-tuple whose first component is the the pending operation with 
    /// the highest priority from the work queue and the second component is the
    /// priority.</returns>
    private Tuple<AsyncOperation?, int> GetWork()
    {
        AsyncOperation? request = null;
        var priority = 0;

        for (int i = _priorityQueues - 1; i >= 0; i--)
        {
            request = _singleItems[i];
            if (request != null)
            {
                _singleItems[i] = null;
                priority = i;
                break;
            }
        }

        if (request == null)
        {
            for (int i = _priorityQueues - 1; i >= 0; i--)
            {
                if (_items[i].Count > 0)
                {
                    priority = i;
                    request = _items[i]?.First?.Value;
                    _items[i].RemoveFirst();
                    break;
                }
            }
        }

        return Tuple.Create(request, priority);
    }

    /// <summary>
    /// Rebuilds the work queue.
    /// This method must be called from inside a lock.
    /// </summary>
    private void BuildWorkQueue()
    {
        _singleItems = new AsyncOperation[_priorityQueues];
        _items = new LinkedList<AsyncOperation>[_priorityQueues];

        for (int i = 0; i < _priorityQueues; i++)
        {
            _items[i] = new LinkedList<AsyncOperation>();
        }
    }

    /// <summary>
    /// Clears all work queues.
    /// This method must be called from inside a lock.
    /// </summary>
    private void ClearWorkQueue()
    {
        for (int i = 0; i < _priorityQueues; i++)
            ClearWorkQueue(i);
    }

    /// <summary>
    /// Clears the work queue with the given priority.
    /// This method must be called from inside a lock.
    /// </summary>
    /// <param name="priority">A value between 0 and <see cref="PriorityQueues"/> 
    /// indicating the priority queue to cancel.</param>
    private void ClearWorkQueue(int priority)
    {
        var singleOp = _singleItems[priority];

        if (singleOp != null)
        {
            singleOp.OperationCompleted();
            _singleItems[priority] = null;
        }

        while (_items[priority].Count > 0)
        {
            var asyncOp = _items[priority]?.First?.Value;

            asyncOp?.OperationCompleted();
            _items[priority].RemoveFirst();
        }
    }

    /// <summary>
    /// Creates the thread array.
    /// </summary>
    private void CreateThreads()
    {
        _threads = new Thread[_threadCount];

        for (int i = 0; i < _threadCount; i++)
        {
            _threads[i] = new Thread(new ThreadStart(Run))
            {
                Name = _threadName + " " + (i + 1).ToString(),
                IsBackground = true
            };
        }
    }

    /// <summary>
    /// Used to call <see cref="OnRunWorkerCompleted"/> by the synchronization context.
    /// </summary>
    /// <param name="arg">The argument.</param>
    private void RunWorkerCompletedCallback(object? arg)
    {
        if (arg is null) return;

        OnRunWorkerCompleted((QueuedWorkerCompletedEventArgs)arg);
    }

    /// <summary>
    /// Used by the worker thread to process items.
    /// </summary>
    private void Run()
    {
        while (!Stopping)
        {
            lock (_lockObject)
            {
                // Wait until we have pending work items
                if (_isPaused || IsWorkQueueEmpty())
                    Monitor.Wait(_lockObject);
            }

            // Loop until we exhaust the queue
            var queueFull = true;
            while (queueFull && !Stopping && !Paused)
            {
                // Get an item from the queue
                AsyncOperation? asyncOp = null;
                object? request = null;
                var priority = 0;

                lock (_lockObject)
                {
                    // Check queues
                    var work = GetWork();
                    asyncOp = work.Item1;
                    priority = work.Item2;

                    if (asyncOp != null)
                        request = asyncOp.UserSuppliedState;

                    // Check if the item was removed
                    if (request != null && _cancelledItems.ContainsKey(request))
                        request = null;
                }

                if (request != null)
                {
                    Exception? error = null;
                    object? result = null;
                    var cancel = false;

                    // Start the work
                    try
                    {
                        // Raise the do work event
                        var doWorkArg = new QueuedWorkerDoWorkEventArgs(request, priority);
                        OnDoWork(doWorkArg);

                        result = doWorkArg.Result;
                        cancel = doWorkArg.Cancel;
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }

                    // Raise the work complete event
                    var workCompletedArg = new QueuedWorkerCompletedEventArgs(request, result, priority, error, cancel);

                    if (!Stopping)
                        asyncOp?.PostOperationCompleted(_workCompletedCallback, workCompletedArg);
                }
                else
                {
                    asyncOp?.OperationCompleted();
                }

                // Check if the cache is exhausted
                lock (_lockObject)
                {
                    queueFull = !IsWorkQueueEmpty();
                }
            }
        }
    }

    #endregion


    #region Virtual Methods

    /// <summary>
    /// Raises the RunWorkerCompleted event.
    /// </summary>
    /// <param name="e">A <see cref="QueuedWorkerCompletedEventArgs"/> that contains event data.</param>
    protected virtual void OnRunWorkerCompleted(QueuedWorkerCompletedEventArgs e)
    {
        try
        {
            RunWorkerCompleted?.Invoke(this, e);
        }
        catch { }
    }

    /// <summary>
    /// Raises the DoWork event.
    /// </summary>
    /// <param name="e">A <see cref="QueuedWorkerDoWorkEventArgs"/> that contains event data.</param>
    protected virtual void OnDoWork(QueuedWorkerDoWorkEventArgs e)
    {
        DoWork?.Invoke(this, e);
    }

    #endregion


    #region Dispose

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component"/> 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; 
    /// false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_isDisposed)
            return;

        lock (_lockObject)
        {
            if (!_isStopping)
            {
                _isStopping = true;
                ClearWorkQueue();

                _cancelledItems.Clear();
                Monitor.PulseAll(_lockObject);
            }
        }

        _isDisposed = true;
    }

    #endregion

}


/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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

using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common;

public partial class BHelper
{
    private static CancellationTokenSource? _cancelGcCollect;


    #region Debouncer

    // Key: The specific Action/Delegate instance.
    private static readonly ConcurrentDictionary<object, CancellationTokenSource> _tokens = new();


    /// <summary>
    /// Debounces a parameterless action.
    /// <b>Note</b>: 'action' delegate must be the same instance (cached) to work correctly.
    /// </summary>
    public static void Debounce(int delayMs, Action action, bool runUIThread = false)
    {
        // Pass 'action' as both the dictionary key and the workload
        DebounceInternal(action, delayMs, action, runUIThread);
    }


    /// <summary>
    /// Debounces an action with a parameter. 
    /// <b>Note</b>: 'action' delegate must be the same instance (cached) to work correctly.
    /// </summary>
    public static void Debounce<T>(int delayMs, Action<T?> action, T? param = default, bool runUIThread = false)
    {
        // Pass 'action' as the key to ensure we cancel previous calls to THIS method.
        // Create a lightweight closure for the workload to capture the new 'param'.
        DebounceInternal(action, delayMs, () => action(param), runUIThread);
    }


    /// <summary>
    /// Core logic to handle cancellation, delay, and execution.
    /// </summary>
    private static void DebounceInternal(object key, int delayMs, Action workload, bool runUIThread)
    {
        // 1. Cancel and dispose the previous timer for this delegate
        if (_tokens.TryRemove(key, out var oldCts))
        {
            oldCts.Cancel();
            oldCts.Dispose();
        }

        // 2. Create new token (capture the token struct so the continuation never touches the
        // CTS getter, which throws ObjectDisposedException once a newer call disposes this cts)
        var cts = new CancellationTokenSource();
        var token = cts.Token;
        _tokens[key] = cts;

        // 3. Start delay
        Task.Delay(delayMs, token).ContinueWith(t =>
        {
            // Check cancellation immediately upon waking
            if (token.IsCancellationRequested) return;

            // Cleanup dictionary: only remove+dispose if we are still the active entry
            // (never dispose a newer call's cts)
            if (_tokens.TryRemove(new KeyValuePair<object, CancellationTokenSource>(key, cts)))
            {
                cts.Dispose();
            }

            // 4. Execute
            if (runUIThread)
            {
                // 'Post' is fire-and-forget and slightly faster than InvokeAsync for this case
                Dispatcher.UIThread.Post(workload, DispatcherPriority.Background);
            }
            else
            {
                workload();
            }
        },
        CancellationToken.None,
        TaskContinuationOptions.OnlyOnRanToCompletion,
        TaskScheduler.Default);
    }

    #endregion // Debouncer



    /// <summary>
    /// Delay calling <see cref="GC.Collect"/> for <paramref name="delayMs"/> milliseconds.
    /// </summary>
    public static void GcCollect(int delayMs = 500)
    {
        var newCts = new CancellationTokenSource();
        var oldCts = Interlocked.Exchange(ref _cancelGcCollect, newCts);

        oldCts?.Cancel();
        oldCts?.Dispose();

        _ = GCCollectAsync(delayMs, newCts.Token);
    }


    private static async Task GCCollectAsync(int delayMs, CancellationToken token)
    {
        // check if task is cancelled
        if (token.IsCancellationRequested) return;

        await Task.Delay(delayMs, token);

        // check if task is cancelled
        if (token.IsCancellationRequested) return;

        // Collect system garbage
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }


}

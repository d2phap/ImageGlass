/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;

namespace ImageGlass.UI.Windowing;

/// <summary>
/// Hides the mouse cursor of a window after a period of pointer inactivity,
/// and shows it again on the next pointer input. Used by the slideshow.
/// </summary>
public sealed class IdleCursorHider : PhDisposable
{
    /// <summary>
    /// Pointer idle time before the cursor is hidden.
    /// </summary>
    public static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromSeconds(3);

    // the Win32, X11 and macOS backends all map None to a hidden cursor
    private static readonly Cursor _hiddenCursor = new(StandardCursorType.None);

    private readonly TopLevel _topLevel;
    private readonly InputElement[] _targets;
    private readonly List<(InputElement Target, Cursor? Cursor, bool WasSet)> _savedCursors = [];
    private readonly DispatcherTimer _timer;
    private bool _isRunning;
    private bool _isHidden;


    /// <summary>
    /// Initializes a new instance of <see cref="IdleCursorHider"/>.
    /// </summary>
    /// <param name="topLevel">The window to listen to pointer input on.</param>
    /// <param name="cursorOwners">Controls whose own <see cref="InputElement.Cursor"/> shadows the inherited one.</param>
    public IdleCursorHider(TopLevel topLevel, params InputElement[] cursorOwners)
        : this(topLevel, DefaultIdleTimeout, cursorOwners)
    { }


    /// <summary>
    /// Initializes a new instance of <see cref="IdleCursorHider"/>.
    /// </summary>
    public IdleCursorHider(TopLevel topLevel, TimeSpan idleTimeout, params InputElement[] cursorOwners)
    {
        _topLevel = topLevel;

        _targets = new InputElement[cursorOwners.Length + 1];
        _targets[0] = topLevel;
        cursorOwners.CopyTo(_targets, 1);

        _timer = new DispatcherTimer(idleTimeout, DispatcherPriority.Background, (_, _) => HideCursor());
    }


    #region Public Methods

    /// <summary>
    /// Starts watching for pointer inactivity.
    /// </summary>
    public void Start()
    {
        if (IsDisposed || _isRunning) return;
        _isRunning = true;

        _topLevel.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel, true);
        _topLevel.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel, true);
        _topLevel.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel, true);
        _topLevel.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel, true);

        RestartTimer();
    }


    /// <summary>
    /// Stops watching for pointer inactivity and shows the cursor again.
    /// </summary>
    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;

        _timer.Stop();

        _topLevel.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
        _topLevel.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        _topLevel.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        _topLevel.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);

        ShowCursor();
    }


    /// <summary>
    /// Hides the cursor immediately.
    /// </summary>
    public void HideCursor()
    {
        _timer.Stop(); // one-shot: the next pointer input restarts it
        if (_isHidden) return;
        _isHidden = true;

        // read every target before writing any, else a target that inherits
        // from an already-hidden ancestor saves the hidden cursor as its own
        _savedCursors.Clear();
        foreach (var target in _targets)
        {
            var wasSet = target.IsSet(InputElement.CursorProperty);
            _savedCursors.Add((target, wasSet ? target.Cursor : null, wasSet));
        }

        foreach (var item in _savedCursors)
        {
            item.Target.Cursor = _hiddenCursor;
        }
    }


    /// <summary>
    /// Shows the cursor immediately.
    /// </summary>
    public void ShowCursor()
    {
        if (!_isHidden) return;
        _isHidden = false;

        foreach (var item in _savedCursors)
        {
            if (item.WasSet)
            {
                item.Target.Cursor = item.Cursor;
            }
            else
            {
                item.Target.ClearValue(InputElement.CursorProperty);
            }
        }

        _savedCursors.Clear();
    }

    #endregion // Public Methods



    #region Private Methods

    protected override void OnDisposing()
    {
        base.OnDisposing();
        Stop();
    }


    private void OnPointerMoved(object? sender, PointerEventArgs e) => HandlePointerActivity(e);

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e) => HandlePointerActivity(e);

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) => HandlePointerActivity(e);

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e) => HandlePointerActivity(e);


    private void HandlePointerActivity(PointerEventArgs e)
    {
        // a touch contact draws no cursor, so its events must not keep one visible
        if (e.Pointer.Type == PointerType.Touch) return;

        ShowCursor();
        RestartTimer();
    }


    private void RestartTimer()
    {
        if (!_isRunning) return;

        _timer.Stop();
        _timer.Start();
    }

    #endregion // Private Methods

}

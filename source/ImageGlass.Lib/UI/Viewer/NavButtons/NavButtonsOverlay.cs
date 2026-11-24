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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Avalonia.VisualTree;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using System;
using System.IO;

namespace ImageGlass.UI.Viewer;

/// <summary>
/// An overlay that draws left/right navigation buttons on the viewer.
/// </summary>
public class NavButtonsOverlay : PhControl
{
    private const double ANIM_DURATION_MS = 100;
    private const double ANIM_SLIDE_DISTANCE = 10; // pixels to slide in/out

    private NavButtonsInfo _state = new();
    private ViewerControl? _parentViewer;
    private Window? _parentWindow;

    // an inactive window is only activated by the next click, which must not also navigate
    private bool _swallowActivatingPress = false;
    private Point? _lastPointerPoint = null;

    // animation state per button (0 = hidden, 1 = fully visible)
    private double _leftAnimProgress = 0;
    private double _rightAnimProgress = 0;
    private bool _leftAnimTarget = false;
    private bool _rightAnimTarget = false;
    private bool _animRunning = false;
    private TimeSpan _lastFrameTime = TimeSpan.Zero;

    // a backdrop blur belongs to one visual and its clip, so each button is a visual of its own
    private readonly NavButton _leftButton = new()
    {
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new Thickness(NavButtonsInfo.NAV_BTN_MARGIN, 0, 0, 0),
    };
    private readonly NavButton _rightButton = new()
    {
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new Thickness(0, 0, NavButtonsInfo.NAV_BTN_MARGIN, 0),
    };


    public NavButtonsOverlay()
    {
        // without a transparent background, pointer events pass through and hover/press never trigger
        Background = Brushes.Transparent;
        Content = new Panel { Children = { _leftButton, _rightButton } };
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _parentViewer = this.FindAncestorOfType<ViewerControl>();
        if (_parentViewer is not null)
        {
            _state = _parentViewer._navButtons;
        }

        _parentWindow = TopLevel.GetTopLevel(this) as Window;
        if (_parentWindow is not null)
        {
            _parentWindow.Activated += ParentWindow_Activated;
            _parentWindow.Deactivated += ParentWindow_Deactivated;
        }

        // suppress context menu on nav button areas
        AddHandler(ContextRequestedEvent, OnContextRequested, RoutingStrategies.Tunnel);

        LoadIcons();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        RemoveHandler(ContextRequestedEvent, OnContextRequested);

        if (_parentWindow is not null)
        {
            _parentWindow.Activated -= ParentWindow_Activated;
            _parentWindow.Deactivated -= ParentWindow_Deactivated;
            _parentWindow = null;
        }

        _animRunning = false;
    }


    /// <summary>
    /// Restores the hover state under the pointer once the window is active again.
    /// </summary>
    private void ParentWindow_Activated(object? sender, EventArgs e)
    {
        if (!IsPointerOver || _lastPointerPoint is not Point pos) return;

        _state.IsLeftHovered = GetLeftHitArea().Contains(pos);
        _state.IsRightHovered = GetRightHitArea().Contains(pos);
        UpdateAnimationTargets();
    }


    /// <summary>
    /// Hides the buttons and arms the activating-click guard when the window loses focus.
    /// </summary>
    private void ParentWindow_Deactivated(object? sender, EventArgs e)
    {
        _swallowActivatingPress = true;
        _state.ResetState();
        UpdateAnimationTargets();
        UpdateButtons();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        LoadIcons();
        UpdateButtons();
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (_parentViewer is null || !_state.IsEnabled || _parentViewer.EnableSelection) return;

        var p = e.GetCurrentPoint(this);
        if (!p.Properties.IsLeftButtonPressed) return;

        // the click that re-activates the window must not reach a nav button
        if (!IsWindowActive() || _swallowActivatingPress)
        {
            _swallowActivatingPress = false;
            return;
        }

        var pos = p.Position;
        _state.PointerDownPoint = pos;
        _state.IsDragging = false;

        // reset opposite side to avoid both buttons showing (touch scenario)
        if (GetLeftHitArea().Contains(pos))
        {
            _state.IsRightHovered = false;
            _state.IsRightPressed = false;
            _state.IsLeftPressed = true;
            UpdateAnimationTargets();
            UpdateButtons();
        }
        else if (GetRightHitArea().Contains(pos))
        {
            _state.IsLeftHovered = false;
            _state.IsLeftPressed = false;
            _state.IsRightPressed = true;
            UpdateAnimationTargets();
            UpdateButtons();
        }

        // Don't set e.Handled — let the event bubble to parent ViewerControl
    }


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_parentViewer is null || !_state.IsEnabled || _parentViewer.EnableSelection) return;

        var pos = e.GetPosition(this);
        var p = e.GetCurrentPoint(this);

        // an inactive window must not light up the buttons on hover
        if (!IsWindowActive())
        {
            _lastPointerPoint = pos;
            _state.ResetState();
            UpdateAnimationTargets();
            return;
        }

        // a real cursor move means this is no longer the click that activated the window
        if (_lastPointerPoint != pos) _swallowActivatingPress = false;
        _lastPointerPoint = pos;

        // drag threshold check
        if ((_state.IsLeftPressed || _state.IsRightPressed) && !_state.IsDragging)
        {
            if (_state.PointerDownPoint is Point downPt)
            {
                var dx = pos.X - downPt.X;
                var dy = pos.Y - downPt.Y;
                if (Math.Sqrt(dx * dx + dy * dy) > 5)
                {
                    _state.IsDragging = true;
                    UpdateAnimationTargets();
                }
            }
        }

        // only update hover when no mouse button is held (not panning)
        if (!p.Properties.IsLeftButtonPressed)
        {
            _state.IsLeftHovered = GetLeftHitArea().Contains(pos);
            _state.IsRightHovered = GetRightHitArea().Contains(pos);
            UpdateAnimationTargets();
        }
    }


    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        var wasLeftPressed = _state.IsLeftPressed;
        var wasRightPressed = _state.IsRightPressed;
        var wasDragging = _state.IsDragging;

        _state.IsLeftPressed = false;
        _state.IsRightPressed = false;
        _state.IsDragging = false;
        _state.PointerDownPoint = null;

        if (_parentViewer is null || !_state.IsEnabled || _parentViewer.EnableSelection)
            return;
        if (!IsWindowActive()) return;

        var pos = e.GetPosition(this);

        // click detection
        if (!wasDragging)
        {
            if (wasLeftPressed && GetLeftHitArea().Contains(pos))
            {
                _parentViewer.OnNavButtonClicked(NavButtonDirection.Left);
            }
            else if (wasRightPressed && GetRightHitArea().Contains(pos))
            {
                _parentViewer.OnNavButtonClicked(NavButtonDirection.Right);
            }
        }

        // update hover
        _state.IsLeftHovered = GetLeftHitArea().Contains(pos);
        _state.IsRightHovered = GetRightHitArea().Contains(pos);
        UpdateAnimationTargets();
        UpdateButtons();
    }


    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _state.ResetState();
        _lastPointerPoint = null;
        UpdateAnimationTargets();
    }

    #endregion // Control Events



    #region Private Methods

    /// <summary>
    /// Checks if the parent window is active; the buttons ignore input while it is not.
    /// </summary>
    private bool IsWindowActive() => _parentWindow?.IsActive ?? true;


    /// <summary>
    /// Suppresses context menu when the pointer is in a nav button hit area.
    /// </summary>
    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (_parentViewer is null || !_state.IsEnabled || _parentViewer.EnableSelection) return;

        // suppress context menu if any nav button is hovered or pressed
        if (_state.IsLeftHovered || _state.IsRightHovered
            || _state.IsLeftPressed || _state.IsRightPressed)
        {
            e.Handled = true;
        }
    }


    /// <summary>
    /// Brings both button visuals to their animation progress, press state and icon.
    /// </summary>
    private void UpdateButtons()
    {
        var isActive = _parentViewer is not null && _state.IsEnabled && !_parentViewer.EnableSelection;
        var leftProgress = isActive ? _leftAnimProgress : 0;
        var rightProgress = isActive ? _rightAnimProgress : 0;
        var isLeftPressed = _state.IsLeftPressed && !_state.IsDragging;
        var isRightPressed = _state.IsRightPressed && !_state.IsDragging;

        // the left button slides in from the right, the right one from the left
        _leftButton.Apply(leftProgress, ANIM_SLIDE_DISTANCE * (1 - leftProgress), isLeftPressed, _state.LeftIcon);
        _rightButton.Apply(rightProgress, -ANIM_SLIDE_DISTANCE * (1 - rightProgress), isRightPressed, _state.RightIcon);
    }


    /// <summary>
    /// Computes whether each button should be visible, and starts/stops
    /// the animation timer accordingly.
    /// </summary>
    private void UpdateAnimationTargets()
    {
        var leftVisible = (_state.IsLeftHovered || _state.IsLeftPressed) && !_state.IsDragging;
        var rightVisible = (_state.IsRightHovered || _state.IsRightPressed) && !_state.IsDragging;

        _leftAnimTarget = leftVisible;
        _rightAnimTarget = rightVisible;

        EnsureAnimationRunning();
    }


    private void EnsureAnimationRunning()
    {
        // check if animation is already at target
        var leftDone = _leftAnimTarget ? _leftAnimProgress >= 1.0 : _leftAnimProgress <= 0.0;
        var rightDone = _rightAnimTarget ? _rightAnimProgress >= 1.0 : _rightAnimProgress <= 0.0;

        if (leftDone && rightDone)
        {
            _animRunning = false;
            return;
        }

        if (!_animRunning)
        {
            _animRunning = true;
            _lastFrameTime = TimeSpan.Zero;
            TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
        }
    }


    private void OnAnimationFrame(TimeSpan ts)
    {
        if (!_animRunning) return;

        if (_lastFrameTime != TimeSpan.Zero)
        {
            var elapsedMs = (ts - _lastFrameTime).TotalMilliseconds;
            var step = elapsedMs / ANIM_DURATION_MS;

            _leftAnimProgress = _leftAnimTarget
                ? Math.Min(1.0, _leftAnimProgress + step)
                : Math.Max(0.0, _leftAnimProgress - step);

            _rightAnimProgress = _rightAnimTarget
                ? Math.Min(1.0, _rightAnimProgress + step)
                : Math.Max(0.0, _rightAnimProgress - step);

            UpdateButtons();
        }
        _lastFrameTime = ts;

        // stop when both animations are done
        var leftDone = _leftAnimTarget ? _leftAnimProgress >= 1.0 : _leftAnimProgress <= 0.0;
        var rightDone = _rightAnimTarget ? _rightAnimProgress >= 1.0 : _rightAnimProgress <= 0.0;

        if (leftDone && rightDone)
        {
            _animRunning = false;
            return;
        }

        // request next frame
        TopLevel.GetTopLevel(this)?.RequestAnimationFrame(OnAnimationFrame);
    }


    private Rect GetLeftHitArea()
    {
        var hitWidth = NavButtonsInfo.NAV_BTN_SIZE.Width + NavButtonsInfo.NAV_BTN_MARGIN;
        return new Rect(0, 0, hitWidth, Bounds.Height);
    }


    private Rect GetRightHitArea()
    {
        var hitWidth = NavButtonsInfo.NAV_BTN_SIZE.Width + NavButtonsInfo.NAV_BTN_MARGIN;
        return new Rect(Bounds.Width - hitWidth, 0, hitWidth, Bounds.Height);
    }


    private void LoadIcons()
    {
        _state.LeftArrowSvgPath = Core.Theme.GetIconPath(IgThemeIcon.ViewPreviousImage);
        _state.RightArrowSvgPath = Core.Theme.GetIconPath(IgThemeIcon.ViewNextImage);
        _state.LeftIcon = LoadSvgIcon(_state.LeftArrowSvgPath);
        _state.RightIcon = LoadSvgIcon(_state.RightArrowSvgPath);
    }


    private static SvgImage? LoadSvgIcon(string svgPath)
    {
        if (string.IsNullOrEmpty(svgPath) || !File.Exists(svgPath)) return null;

        try
        {
            var svgSource = SvgSource.Load(svgPath);
            return new SvgImage { Source = svgSource };
        }
        catch { }

        return null;
    }

    #endregion // Private Methods


}

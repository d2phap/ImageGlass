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
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace ImageGlass.UI.Windowing;


public partial class PhWindow : Window
{
    protected bool _canUseBackdrop = false;

    // the state to return to when the window is restored from minimized
    private WindowState _stateBeforeMinimized = WindowState.Normal;

    // the bounds to return to when the window is restored from maximized/full screen
    private Rect? _windowedBounds;

    // whether a windowed-bounds capture is queued, waiting for the window state to settle
    private bool _isWindowedBoundsQueued;

    // how much of a restored window must land on a screen to count as reachable (DIP)
    private const double MIN_VISIBLE_SIZE = 64;

    protected static Color DefaultActivateBg => Core.Theme.Settings.IsDarkMode
        ? AppThemeColors.BackgroundActivateDark
        : AppThemeColors.BackgroundActivateLight;

    protected static Color DefaultInactivateBg => Core.Theme.Settings.IsDarkMode
        ? AppThemeColors.BackgroundInactivateDark
        : AppThemeColors.BackgroundInactivateLight;



    #region Public Properties

    /// <summary>
    /// Gets the handle of this window.
    /// </summary>
    public nint Handle => GetTopLevel(this)?.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;


    /// <summary>
    /// Gets the DPI scale of this window.
    /// </summary>
    public double Dpi => TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;


    /// <summary>
    /// Gets the state to restore this window to, i.e. the pre-minimize state while minimized.
    /// </summary>
    public WindowState RestorableWindowState => WindowState == WindowState.Minimized
        ? _stateBeforeMinimized
        : WindowState;


    /// <summary>
    /// Gets the last size &amp; position this window had while windowed, i.e. the bounds it
    /// returns to from maximized or full screen; <see langword="null"/> until it has been windowed.
    /// </summary>
    public Rect? WindowedBounds
    {
        get
        {
            // while windowed the live values are the answer, and a queued capture may not have run
            CommitWindowedBounds();
            return _windowedBounds;
        }
    }


    /// <summary>
    /// Gets, sets the value indicates that if this window uses a custom backdrop.
    /// </summary>
    public virtual bool UseCustomBackdrop { get; set; } = false;


    /// <summary>
    /// Gets, sets the value indicates that the window icon won't be loaded by default.
    /// </summary>
    public virtual bool UseCustomWindowIcon { get; set; } = false;


    /// <summary>
    /// Gets, sets the window backdrop style.
    /// </summary>
    public BackdropStyle BackdropStyle
    {
        get => GetValue(BackdropStyleProperty);
        set => SetValue(BackdropStyleProperty, value);
    }
    public static readonly StyledProperty<BackdropStyle> BackdropStyleProperty =
        AvaloniaProperty.Register<Window, BackdropStyle>(nameof(BackdropStyle), BackdropStyle.None);



    /// <summary>
    /// Gets, sets the hotkey to close the window with.
    /// </summary>
    public Hotkey[] CloseWindowHotkeys
    {
        get => GetValue(CloseWindowHotkeysProperty);
        set => SetValue(CloseWindowHotkeysProperty, value);
    }
    public static readonly StyledProperty<Hotkey[]> CloseWindowHotkeysProperty =
        AvaloniaProperty.Register<Window, Hotkey[]>(nameof(CloseWindowHotkeys), []);



    /// <summary>
    /// Gets, sets the value indicates that the app icon is shown on the title bar.
    /// The taskbar icon is not affected.
    /// </summary>
    public bool ShowTitleBarIcon
    {
        get => GetValue(ShowTitleBarIconProperty);
        set => SetValue(ShowTitleBarIconProperty, value);
    }
    public static readonly StyledProperty<bool> ShowTitleBarIconProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(ShowTitleBarIcon), true);



    /// <summary>
    /// Gets, sets the frameless mode.
    /// </summary>
    public bool IsFrameless
    {
        get => GetValue(IsFramelessProperty);
        set => SetValue(IsFramelessProperty, value);
    }
    public static readonly StyledProperty<bool> IsFramelessProperty =
        AvaloniaProperty.Register<Window, bool>(nameof(IsFrameless), false);


    #endregion // Public Properties



    public PhWindow()
    {
        OnIgFramelessModeChanged(IsFrameless);
        if (BackdropStyle == BackdropStyle.None)
        {
            UpdateBackground(true);
        }

        Topmost = Core.Config.EnableWindowTopMost;

        Core.ThemeChanged += Core_ThemeChanged;
        Core.LanguageChanged += Core_LanguageChanged;
        Core.Config.PropertyChanged += Config_PropertyChanged;
    }



    #region Events & Override methods

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        OnIgLanguageChanged();

        // needs a live window handle, so it cannot be applied when the property is set
        OnIgTitleBarIconVisibilityChanged(ShowTitleBarIcon);

        // still inside Show(), and the theme pack is loaded by now, unlike in the ctor
        UpdateTitleBarDarkMode();

        DetachImeWhenNotEditingText();

        // seed the windowed bounds, in case the window is never moved nor resized
        TrackWindowedBounds();
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ActualThemeVariantChanged += PhWindow_ActualThemeVariantChanged;
        Activated += PhWindow_Activated;
        Deactivated += PhWindow_Deactivated;
        PositionChanged += PhWindow_PositionChanged;
    }


    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        ActualThemeVariantChanged -= PhWindow_ActualThemeVariantChanged;
        Activated -= PhWindow_Activated;
        Deactivated -= PhWindow_Deactivated;
        PositionChanged -= PhWindow_PositionChanged;

        Core.ThemeChanged -= Core_ThemeChanged;
        Core.LanguageChanged -= Core_LanguageChanged;
        Core.Config.PropertyChanged -= Config_PropertyChanged;
    }


    private void PhWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        TrackWindowedBounds();
    }


    protected override void OnResized(WindowResizedEventArgs e)
    {
        base.OnResized(e);
        TrackWindowedBounds();
    }


    private void PhWindow_ActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (UseCustomBackdrop) return;

        if (IsActive) PhWindow_Activated(sender, e);
        else PhWindow_Deactivated(sender, e);
    }


    private async void PhWindow_Activated(object? sender, EventArgs e)
    {
        OnIgActivated(e);

        // another window may have re-attached the IME while we were inactive
        DetachImeWhenNotEditingText();


        // handle built-in backdrop style
        if (UseCustomBackdrop) return;
        if (_canUseBackdrop)
        {
            await AnimateBackgroundColorAsync(DefaultActivateBg.A(0));
        }
    }


    private async void PhWindow_Deactivated(object? sender, EventArgs e)
    {
        OnIgDeactivated(e);


        // handle built-in backdrop style
        if (UseCustomBackdrop) return;
        if (_canUseBackdrop)
        {
            await AnimateBackgroundColorAsync(DefaultInactivateBg);
        }
    }


    private void Core_ThemeChanged(object? sender, ThemePackChangedEventArgs e)
    {
        // update app icon
        if (!UseCustomWindowIcon)
        {
            _ = UpdateWindowIconAsync();
        }

        UpdateBackground(IsActive);
        UpdateTitleBarDarkMode();
        OnIgThemeChanged(e);
    }


    private void Core_LanguageChanged(object? sender, EventArgs e)
    {
        OnIgLanguageChanged();
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // the setting can be toggled while this window is open (main menu, or the Settings window itself)
        if (e.PropertyName == nameof(Config.EnableWindowTopMost))
        {
            Topmost = Core.Config.EnableWindowTopMost;
        }
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // BackdropStyle
        if (e.Property == BackdropStyleProperty)
        {
            OnIgBackdropStyleChanged((BackdropStyle)e.NewValue!);
        }

        // IsFrameless
        else if (e.Property == IsFramelessProperty)
        {
            OnIgFramelessModeChanged((bool)e.NewValue!);
        }

        // ShowTitleBarIcon
        else if (e.Property == ShowTitleBarIconProperty)
        {
            OnIgTitleBarIconVisibilityChanged((bool)e.NewValue!);
        }

        // a new window icon (theme change) resets the title bar icon, so re-apply
        else if (e.Property == IconProperty)
        {
            OnIgTitleBarIconVisibilityChanged(ShowTitleBarIcon);
        }

        // WindowState
        else if (e.Property == WindowStateProperty)
        {
            var newState = (WindowState)e.NewValue!;
            var oldState = (WindowState)e.OldValue!;

            // capture the pre-minimize state, so the window can be restored to it later
            if (newState == WindowState.Minimized && oldState != WindowState.Minimized)
            {
                _stateBeforeMinimized = oldState;
            }
        }
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        // check if the hotkey for closing window is pressed
        foreach (var hk in CloseWindowHotkeys)
        {
            if (hk.IsSame(e.Key, e.KeyModifiers))
            {
                OnIgCloseWindowHotkeyPressed(e);
                if (!e.Handled)
                {
                    e.Handled = true;
                    Close();
                    return;
                }

                break;
            }
        }


        base.OnKeyDown(e);
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        // Frameless mode: enable window dragging at top border
        if (IsFrameless)
        {
            var p = e.GetCurrentPoint(this);
            if (p.Properties.IsLeftButtonPressed && p.Position.Y < 15) BeginMoveDrag(e);
        }
    }


    #endregion // Events & Override methods



    #region Virtual methods

    /// <summary>
    /// Occurs when the window is activated.
    /// </summary>
    protected virtual void OnIgActivated(EventArgs e) { }


    /// <summary>
    /// Occurs when the window is deactivated.
    /// </summary>
    protected virtual void OnIgDeactivated(EventArgs e) { }


    /// <summary>
    /// Occurs when one of the hotkey for closing window is pressed.
    /// </summary>
    protected virtual void OnIgCloseWindowHotkeyPressed(KeyEventArgs e) { }


    /// <summary>
    /// Occurs when the frameless mode is changed.
    /// </summary>
    protected virtual void OnIgFramelessModeChanged(bool enable)
    {
        if (enable)
        {
            ExtendClientAreaToDecorationsHint = true;
            WindowDecorations = WindowDecorations.BorderOnly;
        }
        else
        {
            ExtendClientAreaToDecorationsHint = false;
            WindowDecorations = WindowDecorations.Full;
        }

        // the restored title bar comes back without the app icon; re-assert it once the
        // platform has rebuilt the frame
        Dispatcher.Post(
            () => OnIgTitleBarIconVisibilityChanged(ShowTitleBarIcon),
            DispatcherPriority.Background);
    }


    /// <summary>
    /// Occurs when the visibility of the title bar icon is changed.
    /// Does nothing by default; platforms that draw an app icon on the title bar override this.
    /// </summary>
    protected virtual void OnIgTitleBarIconVisibilityChanged(bool show) { }


    /// <summary>
    /// Occurs when the app theme is changed.
    /// </summary>
    protected virtual void OnIgThemeChanged(ThemePackChangedEventArgs e) { }


    /// <summary>
    /// Occurs when the app language is changed.
    /// </summary>
    protected virtual void OnIgLanguageChanged() { }


    /// <summary>
    /// Occurs whenthe backdrop style is changed.
    /// </summary>
    protected virtual void OnIgBackdropStyleChanged(BackdropStyle style)
    {
        if (style != BackdropStyle.None)
        {
            // map the built-in backdrop styles
            if (!UseCustomBackdrop)
            {
                WindowTransparencyLevel[] levels = style switch
                {
                    BackdropStyle.Mica => [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
                    BackdropStyle.MicaAlt => [WindowTransparencyLevel.Mica, WindowTransparencyLevel.None],
                    BackdropStyle.Acrylic => [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.None],
                    _ => [WindowTransparencyLevel.None],
                };

                TransparencyLevelHint = levels;
            }
        }


        // check if we can apply window backdrop
        _canUseBackdrop = !BHelper.IsWindows10
            && !ActualTransparencyLevel.Equals(WindowTransparencyLevel.None)
            && !ActualTransparencyLevel.Equals(WindowTransparencyLevel.Transparent);


        // update background according to the backdrop
        UpdateBackground(true);
    }


    /// <summary>
    /// Updates the background color to reflect the current transparency and activation state.
    /// </summary>
    protected virtual void UpdateBackground(bool isActive)
    {
        var windowBg = isActive ? DefaultActivateBg : DefaultInactivateBg;

        // update background color for transparency
        if (_canUseBackdrop)
        {
            Background = windowBg.A(0).ToBrush();
        }
        else
        {
            Background = windowBg.ToBrush();
        }
    }

    #endregion // Virtual methods



    #region Internal Methods

    /// <summary>
    /// Detaches the OS IME while focus is outside a text field, so a CJK IME cannot swallow
    /// single keys. Avalonia only detaches it after a text field has been focused once.
    /// </summary>
    protected void DetachImeWhenNotEditingText()
    {
        if (FocusManager?.GetFocusedElement() is TextBox
            or NumericUpDown
            or MaskedTextBox
            or AutoCompleteBox) return;

        Core.ShellProvider?.DetachIme(Handle);
    }


    /// <summary>
    /// Draws the title bar in the colors of the current theme. Avalonia only does this on
    /// Windows 11, so a Windows 10 title bar would stay light whatever the theme.
    /// </summary>
    protected void UpdateTitleBarDarkMode()
    {
        Core.ShellProvider?.SetTitleBarDarkMode(Handle, Core.Theme.Settings.IsDarkMode);
    }


    /// <summary>
    /// Updates icon for window and taskbar.
    /// </summary>
    protected async Task UpdateWindowIconAsync(string? customIconPath = null)
    {
        // 1. get full path of icon
        var iconPath = Core.Theme.GetIconPath(IgThemeIcon.AppLogo);
        var useDefaultIcon = !File.Exists(iconPath);


        // 2. use default icon as logo
        if (string.IsNullOrWhiteSpace(customIconPath))
        {
            if (useDefaultIcon)
            {
                // get default logo icon if theme's app logo does not exist
                Icon = Resx.GetDefaultWindowIcon();

                return;
            }
        }
        // 3. use custom icon path
        else
        {
            iconPath = customIconPath;
        }


        // 4. use theme icon as logo
        // decode the logo
        var size = DpiScale(64);
        var bytes = await MagickCodec.QuickDecodeAsync(iconPath, ImageMagick.MagickFormat.Ico, size, size);
        if (bytes is null) return;

        // update icon
        using var ms = new MemoryStream(bytes);
        Icon = new WindowIcon(ms);
    }


    /// <summary>
    /// Animates the window background color.
    /// </summary>
    protected async Task AnimateBackgroundColorAsync(Color toColor)
    {
        if (Background is not SolidColorBrush fromBrush) return;

        var fromColor = fromBrush.Color;
        var toBrush = toColor.ToBrush();
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new LinearEasing(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters = { new Setter(SolidColorBrush.ColorProperty, fromColor) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters = { new Setter(SolidColorBrush.ColorProperty, toColor) }
                }
            },
        };


        Background = toBrush;
        await animation.RunAsync(toBrush);
    }


    /// <summary>
    /// Queues a capture of the current bounds, so the layout to restore to survives a session that
    /// ends maximized, in full screen or minimized.
    /// </summary>
    private void TrackWindowedBounds()
    {
        // maximizing, full screen and minimizing all move & resize the window BEFORE WindowState
        // catches up, so capturing here would record that layout; let the state settle first
        if (_isWindowedBoundsQueued || !IsVisible || WindowState != WindowState.Normal) return;
        _isWindowedBoundsQueued = true;

        Dispatcher.UIThread.Post(() =>
        {
            _isWindowedBoundsQueued = false;
            CommitWindowedBounds();
        }, DispatcherPriority.Background);
    }


    /// <summary>
    /// Captures the current bounds as the windowed ones, if the window is windowed right now.
    /// </summary>
    private void CommitWindowedBounds()
    {
        // a hidden or non-windowed window reports the bounds of another layout, or none at all
        if (!IsVisible || WindowState != WindowState.Normal) return;

        var size = ClientSize;
        if (size.Width <= 0 || size.Height <= 0) return;

        _windowedBounds = new Rect(Position.X, Position.Y, (int)size.Width, (int)size.Height);
    }


    /// <summary>
    /// Restores the window size &amp; position from the saved bounds: a size under
    /// <paramref name="minSize"/> falls back to <paramref name="defaultSize"/>, and a position that
    /// no longer lands on a connected screen falls back to the center of the nearest live screen.
    /// </summary>
    protected void RestoreWindowBounds(Rect savedBounds, Size defaultSize, Size minSize)
    {
        // 1. size: a degenerate saved size (a transient 0×0 on close, a hand-edited config) would
        // open the window too small to be seen
        var size = savedBounds.Width >= minSize.Width && savedBounds.Height >= minSize.Height
            ? savedBounds.Size
            : defaultSize;
        var pos = new PixelPoint((int)savedBounds.X, (int)savedBounds.Y);

        // position it ourselves either way: WindowStartupLocation.CenterScreen is a no-op when the
        // saved position lies outside every screen, which is the case we have to recover from
        WindowStartupLocation = WindowStartupLocation.Manual;

        // 2. position: keep it while enough of the window still lands on a connected screen;
        // with no screen info to validate against, trust it rather than move the window
        var screens = Screens;
        if (screens is null || screens.All.Count == 0 || IsVisibleOnAnyScreen(screens, pos, size))
        {
            Width = size.Width;
            Height = size.Height;
            Position = pos;
            return;
        }

        // 3. off-screen (unplugged monitor, hand-edited config): center on the nearest live screen,
        // shrinking to its work area since the saved size may come from a bigger monitor
        var screen = screens.ScreenFromPoint(pos) ?? screens.Primary ?? screens.All[0];
        var workArea = screen.GetActualWorkingArea();
        var maxSize = workArea.Size.ToSize(screen.Scaling);

        size = new Size(Math.Min(size.Width, maxSize.Width), Math.Min(size.Height, maxSize.Height));
        Width = size.Width;
        Height = size.Height;
        Position = workArea.CenterRect(new PixelRect(PixelSize.FromSize(size, screen.Scaling))).Position;
    }


    /// <summary>
    /// Checks whether a window of the given size at the given position lands on a connected screen
    /// with enough of it inside the work area to be seen and dragged.
    /// </summary>
    private static bool IsVisibleOnAnyScreen(Screens screens, PixelPoint pos, Size size)
    {
        foreach (var screen in screens.All)
        {
            // Position shares the screen coordinate space while the size is in DIP, so scale it
            var winRect = new PixelRect(pos, PixelSize.FromSize(size, screen.Scaling));
            var visible = screen.GetActualWorkingArea().Intersect(winRect);
            var minVisible = PixelSize.FromSize(new Size(
                Math.Min(MIN_VISIBLE_SIZE, size.Width),
                Math.Min(MIN_VISIBLE_SIZE, size.Height)), screen.Scaling);

            if (visible.Width >= minVisible.Width && visible.Height >= minVisible.Height) return true;
        }

        return false;
    }

    #endregion // Internal Methods



    #region Public Methods

    /// <summary>
    /// Restores the window from the minimized state and brings it to the foreground.
    /// </summary>
    public void RestoreAndActivate()
    {
        // Activate() cannot un-minimize a window, and a plain Normal would drop the pre-minimize state
        if (WindowState == WindowState.Minimized)
        {
            WindowState = _stateBeforeMinimized;
        }

        Activate();
    }


    /// <summary>
    /// Scales the given number on the DPI scaling factor.
    /// </summary>
    public double DpiScale(double value, double? scaleFactor = null) => (scaleFactor ?? Dpi) * value;


    /// <summary>
    /// Scales the given size based on the DPI scaling factor.
    /// </summary>
    public Size DpiScale(Size value, double? scaleFactor = null) => new Size(DpiScale(value.Width, scaleFactor), DpiScale(value.Height, scaleFactor));


    /// <summary>
    /// Scales the given point on the DPI scaling factor.
    /// </summary>
    public Point DpiScale(Point value, double? scaleFactor = null) => new Point(DpiScale(value.X, scaleFactor), DpiScale(value.Y, scaleFactor));

    #endregion // Public Methods


}

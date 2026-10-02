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
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using ImageGlass.UI.Windowing;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// A display-only window that mirrors the main viewer, e.g. onto a projector or another screen.
/// </summary>
public sealed class ProjectorWindow : PhWindow
{
    // share of the work area a normal projector window takes
    private const double NORMAL_SIZE_RATIO = 0.6;

    // a move or a state change lands asynchronously, and a state applies to the screen the window is on
    private const int MOVE_SETTLE_MS = 50;

    // macOS animates out of its full screen space, and ignores moves until done
    private const int MAC_FULL_SCREEN_EXIT_MS = 800;

    private readonly IdleCursorHider _cursorHider;
    private readonly Border _host;


    /// <summary>
    /// Occurs when the window moves, or covers its screen differently.
    /// </summary>
    public event EventHandler? LayoutChanged;



    #region Public Properties

    /// <summary>
    /// Gets the number the user knows the projector by.
    /// </summary>
    public int Number { get; }


    /// <summary>
    /// Gets the display-only viewer that mirrors the main viewer.
    /// </summary>
    public ViewerControl Viewer { get; }


    /// <summary>
    /// Gets how the window covers its screen.
    /// </summary>
    public ProjectorWindowMode Mode { get; private set; } = ProjectorWindowMode.Normal;


    /// <summary>
    /// Gets, sets how the window covers a screen it is moved onto, unless the main window is there.
    /// </summary>
    public ProjectorWindowMode PreferredMode { get; set; } = ProjectorWindowMode.FullScreen;


    /// <summary>
    /// Gets the background color of a projector that has none of its own.
    /// </summary>
    public static Color DefaultBackgroundColor
    {
        get
        {
            var brush = Resx.Get<IBrush?>(ResxId.IG_ProjectorBackgroundBrush);
            if (brush is ISolidColorBrush solidBrush) return solidBrush.Color;

            return Colors.Black;
        }
    }


    /// <summary>
    /// Gets, sets the background color; <c>null</c> follows the slideshow background color.
    /// </summary>
    public Color? BackgroundColor
    {
        get;
        set
        {
            field = value;
            ApplyBackground();
        }
    }

    #endregion // Public Properties



    public ProjectorWindow(int number, ViewerControl source)
    {
        Number = number;
        WindowStartupLocation = WindowStartupLocation.Manual;
        ShowActivated = false;
        MinWidth = 160;
        MinHeight = 90;

        Viewer = new ViewerControl
        {
            MirrorSource = source,
            IsInteractive = false,
            EnableNavButtons = false,
            PanMargin = 0,
            CheckerboardMode = Core.Config.CheckerboardMode,
            InterpolationScaleDown = Core.Config.ImageInterpolationScaleDown,
            InterpolationScaleUp = Core.Config.ImageInterpolationScaleUp,
        };

        _host = new Border { Child = Viewer };
        ApplyBackground();
        Content = _host;

        // nothing to point at on an audience screen
        _cursorHider = new IdleCursorHider(this, Viewer);

        PositionChanged += ProjectorWindow_PositionChanged;
        Core.Config.PropertyChanged += Config_PropertyChanged;
        _ = UpdateWindowIconAsync();
    }



    #region Window Events

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        _cursorHider.Start();
    }


    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        PositionChanged -= ProjectorWindow_PositionChanged;
        Core.Config.PropertyChanged -= Config_PropertyChanged;
        _cursorHider.Dispose();
        Viewer.MirrorSource = null;
    }


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property != WindowStateProperty) return;

        // the user can maximize or restore the window too; a minimized one keeps its mode
        var state = (WindowState)e.NewValue!;
        if (state == WindowState.FullScreen) Mode = ProjectorWindowMode.FullScreen;
        else if (state == WindowState.Maximized) Mode = ProjectorWindowMode.Maximized;
        else if (state == WindowState.Normal) Mode = ProjectorWindowMode.Normal;

        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }


    private void ProjectorWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        Title = $"{Core.Lang[LangId.Tool_Projector_WindowTitle, Number]} - {BHelper.AppDisplayName}";
    }


    protected override void OnIgActivated(EventArgs e)
    {
        base.OnIgActivated(e);

        // a projector that covers its screen takes no input, so the keyboard goes back to the main window
        if (Mode == ProjectorWindowMode.Normal) return;

        Dispatcher.UIThread.Post(() =>
        {
            var mainWindow = App.MainWindow;
            var isMainMinimized = mainWindow.WindowState == WindowState.Minimized;
            if (isMainMinimized || !mainWindow.IsVisible) return;

            mainWindow.Activate();
        }, DispatcherPriority.Background);
    }


    private void Config_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Config.ImageInterpolationScaleDown))
        {
            Viewer.InterpolationScaleDown = Core.Config.ImageInterpolationScaleDown;
        }
        else if (e.PropertyName == nameof(Config.ImageInterpolationScaleUp))
        {
            Viewer.InterpolationScaleUp = Core.Config.ImageInterpolationScaleUp;
        }
        else if (e.PropertyName == nameof(Config.CheckerboardMode))
        {
            Viewer.CheckerboardMode = Core.Config.CheckerboardMode;
        }
    }

    #endregion // Window Events



    #region Public Methods

    /// <summary>
    /// Shows the window on <paramref name="screen"/>, covering it as <paramref name="mode"/> says.
    /// </summary>
    public async Task ShowOnScreenAsync(Screen screen, ProjectorWindowMode mode)
    {
        PlaceOnScreen(screen);
        Show();

        await Task.Delay(MOVE_SETTLE_MS);
        ApplyMode(mode);
    }


    /// <summary>
    /// Moves the window onto <paramref name="screen"/>, covering it as <paramref name="mode"/> says.
    /// </summary>
    public async Task MoveToScreenAsync(Screen screen, ProjectorWindowMode mode)
    {
        // 1. only a normal window moves between screens
        if (WindowState != WindowState.Normal)
        {
            var wasFullScreen = WindowState == WindowState.FullScreen;
            WindowState = WindowState.Normal;

            var isMacFullScreenExit = wasFullScreen && BHelper.OS == OSType.Mac;
            await Task.Delay(isMacFullScreenExit ? MAC_FULL_SCREEN_EXIT_MS : MOVE_SETTLE_MS);
        }


        // 2. place it inside the target screen, again if the OS put it elsewhere
        PlaceOnScreen(screen);
        await Task.Delay(MOVE_SETTLE_MS);

        var currentScreen = Screens.ScreenFromWindow(this);
        if (currentScreen != screen)
        {
            PlaceOnScreen(screen);
            await Task.Delay(MOVE_SETTLE_MS);
        }


        // 3. cover the screen it is on now
        ApplyMode(mode);
    }


    /// <summary>
    /// Gets the screen the window is on, if any.
    /// </summary>
    public Screen? GetScreen() => Screens.ScreenFromWindow(this);

    #endregion // Public Methods



    #region Private Methods

    /// <summary>
    /// Centers a normal window in the work area of <paramref name="screen"/>.
    /// </summary>
    private void PlaceOnScreen(Screen screen)
    {
        var workArea = screen.WorkingArea;
        var pixelWidth = (int)(workArea.Width * NORMAL_SIZE_RATIO);
        var pixelHeight = (int)(workArea.Height * NORMAL_SIZE_RATIO);

        // the position is in desktop pixels, the size in DIP of the target screen
        Position = new PixelPoint(
            workArea.X + (workArea.Width - pixelWidth) / 2,
            workArea.Y + (workArea.Height - pixelHeight) / 2);
        Width = pixelWidth / screen.Scaling;
        Height = pixelHeight / screen.Scaling;
    }


    /// <summary>
    /// Paints the window in <see cref="BackgroundColor"/>, else in the projector color of the theme.
    /// </summary>
    private void ApplyBackground()
    {
        // the theme binding and a picked color take the same slot, so drop the old one first
        _host.ClearValue(Border.BackgroundProperty);

        if (BackgroundColor is { } color)
        {
            _host.Background = color.ToBrush();
            return;
        }

        _host[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_ProjectorBackgroundBrush);
    }


    /// <summary>
    /// Applies the window state of <paramref name="mode"/>.
    /// </summary>
    private void ApplyMode(ProjectorWindowMode mode)
    {
        Mode = mode;
        WindowState = mode switch
        {
            ProjectorWindowMode.FullScreen => WindowState.FullScreen,
            ProjectorWindowMode.Maximized => WindowState.Maximized,
            _ => WindowState.Normal,
        };
    }

    #endregion // Private Methods

}

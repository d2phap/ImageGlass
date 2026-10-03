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
/// How a projector window covers its screen.
/// </summary>
public enum ProjectorWindowMode
{
    FullScreen,
    Maximized,
    Normal,

    // frameless in one cell of a layout on its screen; never a preferred mode
    Tiled,
}


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
    private readonly ProjectorPointerControl _pointer = new();

    // the point the pointer marks, where 0 to 1 spans the photo on each axis; null while it is hidden
    private Point? _pointerPoint;


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
    /// Gets the grid cell the window fills while <see cref="Mode"/> is <see cref="ProjectorWindowMode.Tiled"/>.
    /// </summary>
    public ProjectorTile? Tile { get; private set; }


    /// <summary>
    /// Gets the screen the window was tiled on, while <see cref="Tile"/> is set.
    /// </summary>
    public Screen? TileScreen { get; private set; }


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

        // activated once it covers its screen, see ShowOnScreenAsync
        ShowActivated = false;
        MinWidth = 160;
        MinHeight = 90;

        Viewer = new ViewerControl
        {
            MirrorSource = source,
            IsInteractive = false,
            EnableNavButtons = false,
            PanMargin = 0,
            ZoomMode = GetStartZoomMode(),
            CheckerboardMode = Core.Config.CheckerboardMode,
            InterpolationScaleDown = Core.Config.ImageInterpolationScaleDown,
            InterpolationScaleUp = Core.Config.ImageInterpolationScaleUp,
        };

        // the pointer layer takes no input, so the viewer under it stays display-only
        var pointerLayer = new Canvas
        {
            IsHitTestVisible = false,
            ClipToBounds = true,
            Children = { _pointer },
        };
        _host = new Border
        {
            Child = new Panel { Children = { Viewer, pointerLayer } },
        };
        ApplyBackground();
        Content = _host;

        // nothing to point at on an audience screen
        _cursorHider = new IdleCursorHider(this, Viewer);

        PositionChanged += ProjectorWindow_PositionChanged;
        Viewer.RenderStateChanged += Viewer_RenderStateChanged;
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
        Viewer.RenderStateChanged -= Viewer_RenderStateChanged;
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
        if (state is WindowState.FullScreen or WindowState.Maximized) LeaveTile();

        if (state == WindowState.FullScreen) Mode = ProjectorWindowMode.FullScreen;
        else if (state == WindowState.Maximized) Mode = ProjectorWindowMode.Maximized;
        else if (state == WindowState.Normal) Mode = Tile is null ? ProjectorWindowMode.Normal : ProjectorWindowMode.Tiled;

        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }


    private void ProjectorWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }


    private void Viewer_RenderStateChanged(ViewerControl sender, EventArgs e)
    {
        // the photo moves under a still cursor while the main viewer zooms; placed after the draw, which a moved visual would upset
        if (_pointerPoint is null) return;

        Dispatcher.UIThread.Post(PlacePointer, DispatcherPriority.Background);
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

        // only now, so a projector covering its screen hands the keyboard back to the main window
        Activate();
    }


    /// <summary>
    /// Moves the window onto <paramref name="screen"/>, covering it as <paramref name="mode"/> says.
    /// </summary>
    public async Task MoveToScreenAsync(Screen screen, ProjectorWindowMode mode)
    {
        // 1. only a normal window with its frame moves between screens
        LeaveTile();
        await RestoreNormalStateAsync();


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
    /// Shows the window frameless in <paramref name="tile"/> of a grid on <paramref name="screen"/>, moving it there if shown.
    /// </summary>
    public async Task ShowInTileAsync(Screen screen, ProjectorTile tile)
    {
        // 1. only a normal window takes an exact size
        await RestoreNormalStateAsync();


        // 2. drop the frame, so the window fills its cell edge to edge
        Tile = tile;
        TileScreen = screen;
        Mode = ProjectorWindowMode.Tiled;
        WindowDecorations = WindowDecorations.None;
        PlaceInTile(screen, tile);
        if (!IsVisible) Show();


        // 3. again once it is on that screen, as moving onto another scale resizes the window
        await Task.Delay(MOVE_SETTLE_MS);
        PlaceInTile(screen, tile);
        LayoutChanged?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Gets the screen the window is on, if any.
    /// </summary>
    public Screen? GetScreen() => Screens.ScreenFromWindow(this);


    /// <summary>
    /// Shows the pointer at <paramref name="relativePoint"/>, where 0 to 1 spans the photo on each axis.
    /// </summary>
    public void ShowPointer(Point relativePoint)
    {
        _pointerPoint = relativePoint;
        PlacePointer();
    }


    /// <summary>
    /// Hides the pointer.
    /// </summary>
    public void HidePointer()
    {
        _pointerPoint = null;
        _pointer.Hide();
    }


    /// <summary>
    /// Plays a click on the pointer.
    /// </summary>
    public void PlayPointerClick() => _pointer.PlayClick();

    #endregion // Public Methods



    #region Private Methods

    /// <summary>
    /// Puts the pointer over its point on the photo as the viewer shows the photo now; hidden where the viewer does not show that point.
    /// </summary>
    private void PlacePointer()
    {
        if (_pointerPoint is not { } relativePoint) return;

        // 1. no photo to point at yet
        var bitmapSize = Viewer.BitmapSize;
        if (bitmapSize.IsEmpty)
        {
            _pointer.Hide();
            return;
        }

        // 2. a point this projector crops away, zoomed in further than the main viewer
        var sourcePoint = new Point(relativePoint.X * bitmapSize.Width, relativePoint.Y * bitmapSize.Height);
        var clientPoint = Viewer.PointSourceToClient(sourcePoint);
        var isInView = Viewer.DrawingArea.Contains(clientPoint);
        if (!isInView)
        {
            _pointer.Hide();
            return;
        }

        _pointer.MoveTo(clientPoint, Viewer.DrawingArea.Size);
    }


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
    /// Gives a frameless window the exact pixel bounds of <paramref name="tile"/> in the work area of <paramref name="screen"/>.
    /// </summary>
    private void PlaceInTile(Screen screen, ProjectorTile tile)
    {
        var bounds = tile.Layout.GetCellBounds(screen.WorkingArea, tile.Cell);
        Position = bounds.Position;

        // the exact quotient, as Avalonia rounds a DIP size up to whole pixels, so any extra adds one
        var scaling = RenderScaling;
        Width = bounds.Width / scaling;
        Height = bounds.Height / scaling;
    }


    /// <summary>
    /// Brings back the frame of a tiled window, which then covers its screen as a normal window.
    /// </summary>
    private void LeaveTile()
    {
        if (Tile is null) return;

        Tile = null;
        TileScreen = null;
        WindowDecorations = WindowDecorations.Full;
    }


    /// <summary>
    /// Takes the window out of full screen or maximized, as only a normal window moves and sizes freely.
    /// </summary>
    private async Task RestoreNormalStateAsync()
    {
        if (WindowState == WindowState.Normal) return;

        var wasFullScreen = WindowState == WindowState.FullScreen;
        WindowState = WindowState.Normal;

        var isMacFullScreenExit = wasFullScreen && BHelper.OS == OSType.Mac;
        await Task.Delay(isMacFullScreenExit ? MAC_FULL_SCREEN_EXIT_MS : MOVE_SETTLE_MS);
    }


    /// <summary>
    /// Gets the zoom mode a projector starts with, used while it does not follow the main viewer: that of the main window.
    /// </summary>
    private static ZoomMode GetStartZoomMode()
    {
        // a locked zoom keeps the zoom factor it has, which means nothing on a projector no one zooms
        var mode = Core.Config.ZoomMode;
        if (mode == ZoomMode.LockZoom) return ZoomMode.AutoZoom;

        return mode;
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

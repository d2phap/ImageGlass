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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI.Viewer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Owns the projector windows: opens and closes them, places them on screens, and keeps their settings.
/// </summary>
public sealed class ProjectorManager : PhDisposable
{
    /// <summary>
    /// The tool id the settings are kept under in <see cref="Config.ToolSettings"/>.
    /// </summary>
    public const string TOOL_ID = "Tool_Projector";

    /// <summary>
    /// The most projectors that can be open at once without ImageGlass Pro.
    /// </summary>
    public const int MAX_CLASSIC_PROJECTORS = 1;

    // owner key of the projectors' request in SleepGuard
    private const string SLEEP_GUARD_OWNER = "projector";

    private readonly List<ProjectorWindow> _windows = [];
    private readonly Screens _screens;
    private bool _isTrackingPointer;

    // where the cursor is on the main viewer, in its coordinates, while it is over it
    private Point? _mainPointerPoint;

    // set while a refresh of the pointers waits, so a burst of viewport changes runs one
    private InterlockedBool _isPointerRefreshPosted;


    /// <summary>
    /// Occurs when a projector opens, closes, moves, or a setting changes.
    /// </summary>
    public event EventHandler? Changed;



    #region Public Properties

    /// <summary>
    /// Gets the settings of the projectors.
    /// </summary>
    public ProjectorConfig Config { get; private set; } = new();


    /// <summary>
    /// Gets the open projectors, by number.
    /// </summary>
    public IReadOnlyList<ProjectorWindow> Windows => _windows;


    /// <summary>
    /// Gets the screens projectors can be placed on.
    /// </summary>
    public IReadOnlyList<Screen> AllScreens => _screens.All;


    /// <summary>
    /// Gets whether another projector can be opened: always with ImageGlass Pro, else up to <see cref="MAX_CLASSIC_PROJECTORS"/>.
    /// </summary>
    public bool CanAddProjector
    {
        get
        {
            if (Core.IsProEnabled) return true;

            return _windows.Count < MAX_CLASSIC_PROJECTORS;
        }
    }


    /// <summary>
    /// Gets whether only ImageGlass Pro could open another projector.
    /// </summary>
    public bool IsLimitedByLicense => !CanAddProjector;

    #endregion // Public Properties



    public ProjectorManager()
    {
        LoadConfig();

        _screens = App.MainWindow.Screens;
        _screens.Changed += Screens_Changed;

        if (Config.ShowPointer) AttachPointerTracking();
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        _screens.Changed -= Screens_Changed;
        DetachPointerTracking();
        CloseAll();
    }



    #region Public Methods

    /// <summary>
    /// Opens a projector on <paramref name="screen"/>, else on a free screen; <c>null</c> when none can open.
    /// </summary>
    public async Task<ProjectorWindow?> AddProjectorAsync(Screen? screen = null)
    {
        if (IsDisposed) return null;
        if (!CanAddProjector) return null;

        // 1. pick the number and screen; a new projector covers its screen in full, unless the main window is there
        var number = GetFreeNumber();
        var target = screen ?? PickFreeScreen();
        if (target is null) return null;

        var mode = GetModeForScreen(target, ProjectorWindowMode.FullScreen);


        // 2. open it, in the background color the projector of that number had
        var window = new ProjectorWindow(number, GetMainViewer())
        {
            BackgroundColor = GetSavedBackgroundColor(number),
        };
        window.Viewer.EnableMirrorSync = Config.EnableViewSync;
        window.Closed += Window_Closed;
        window.LayoutChanged += Window_LayoutChanged;

        _windows.Add(window);
        _windows.Sort((a, b) => a.Number.CompareTo(b.Number));

        // an audience watches a projector hands-off, so the screens must stay on
        SleepGuard.Acquire(SLEEP_GUARD_OWNER, $"{BHelper.AppDisplayName} projector");

        await window.ShowOnScreenAsync(target, mode);
        OnChanged();

        return window;
    }


    /// <summary>
    /// Moves <paramref name="window"/> onto <paramref name="screen"/>, covering it as the window prefers.
    /// </summary>
    public async Task MoveProjectorAsync(ProjectorWindow window, Screen screen)
    {
        var mode = GetModeForScreen(screen, window.PreferredMode);

        await PlaceProjectorAsync(window, screen, mode);
    }


    /// <summary>
    /// Changes how <paramref name="window"/> covers the screen it is on, and wherever it moves next.
    /// </summary>
    public async Task SetWindowModeAsync(ProjectorWindow window, ProjectorWindowMode mode)
    {
        // a cell is part of a layout of the whole screen
        if (mode == ProjectorWindowMode.Tiled)
        {
            await TileProjectorAsync(window);
            return;
        }

        var screen = window.GetScreen() ?? PickFreeScreen();
        if (screen is null) return;

        // an explicit choice applies even over the main window
        window.PreferredMode = mode;
        await PlaceProjectorAsync(window, screen, mode);
    }


    /// <summary>
    /// Tiles the projectors of <see cref="GetTileSet"/> on <paramref name="screen"/> into the used cells of <paramref name="layout"/>, in number order.
    /// </summary>
    public async Task ArrangeAsync(Screen screen, ProjectorLayout layout, ProjectorWindow? window = null)
    {
        if (IsDisposed) return;

        var tileSet = GetTileSet(screen, window);
        if (tileSet.Count == 0) return;
        if (tileSet.Count > layout.UsedCells.Count) return;

        // 1. the windows are independent, so they move into their cells together
        var tileTasks = tileSet.Select((w, i) => w.ShowInTileAsync(screen, new ProjectorTile(layout, layout.UsedCells[i])));

        // 2. the tiled ones left out would overlap the new cells, so they cover the screen as they prefer
        var untileTasks = GetWindowsOnScreen(screen)
            .Where(w => w.Tile is not null)
            .Where(w => !tileSet.Contains(w))
            .Select(w => PlaceProjectorAsync(w, screen, GetModeForScreen(screen, w.PreferredMode)));

        await Task.WhenAll(tileTasks.Concat(untileTasks));
        OnChanged();
    }


    /// <summary>
    /// Puts each tiled projector on <paramref name="screen"/> back to covering it as the projector prefers.
    /// </summary>
    public async Task RestoreDefaultAsync(Screen screen)
    {
        if (IsDisposed) return;

        var restoreTasks = GetWindowsOnScreen(screen)
            .Where(w => w.Tile is not null)
            .Select(w => PlaceProjectorAsync(w, screen, GetModeForScreen(screen, w.PreferredMode)));

        await Task.WhenAll(restoreTasks);
    }


    /// <summary>
    /// Gets the projectors on <paramref name="screen"/>, by number.
    /// </summary>
    public IReadOnlyList<ProjectorWindow> GetWindowsOnScreen(Screen screen)
    {
        return _windows
            .Where(w => w.GetScreen() == screen)
            .ToArray();
    }


    /// <summary>
    /// Gets the projectors on <paramref name="screen"/> a layout tiles, by number: <paramref name="window"/>, those tiled already, then the others, up to <see cref="ProjectorLayout.MAX_TILED_PROJECTORS"/>.
    /// </summary>
    public IReadOnlyList<ProjectorWindow> GetTileSet(Screen screen, ProjectorWindow? window = null)
    {
        // the tiled ones go first, so a new layout keeps the projectors the user put in cells
        var picked = GetWindowsOnScreen(screen)
            .OrderBy(w => w != window)
            .ThenBy(w => w.Tile is null)
            .ThenBy(w => w.Number)
            .Take(ProjectorLayout.MAX_TILED_PROJECTORS);

        return picked
            .OrderBy(w => w.Number)
            .ToArray();
    }


    /// <summary>
    /// Gets the grid the tiled projectors on <paramref name="screen"/> share; <c>null</c> when none is tiled, or they are in different grids.
    /// </summary>
    public ProjectorLayout? GetActiveLayout(Screen screen)
    {
        var tiledWindows = GetWindowsOnScreen(screen)
            .Where(w => w.Tile is not null)
            .ToArray();

        var layout = tiledWindows.FirstOrDefault()?.Tile?.Layout;
        if (layout is null) return null;

        var isSameGrid = tiledWindows.All(w => w.Tile?.Layout == layout);
        if (!isSameGrid) return null;

        return layout;
    }


    /// <summary>
    /// Sets how <paramref name="window"/> fits the photo while it does not follow the main viewer.
    /// </summary>
    public void SetZoomMode(ProjectorWindow window, ZoomMode mode)
    {
        if (IsDisposed) return;

        window.Viewer.ZoomMode = mode;
        OnChanged();
    }


    /// <summary>
    /// Sets the background color of <paramref name="window"/>; <c>null</c> follows the slideshow background color.
    /// </summary>
    public void SetBackgroundColor(ProjectorWindow window, Color? color)
    {
        if (IsDisposed) return;

        window.BackgroundColor = color;

        // kept by projector number, so a projector opened later with that number has it again
        var colors = Config.BackgroundColors;
        var index = window.Number - 1;
        while (colors.Count <= index)
        {
            colors.Add(string.Empty);
        }
        colors[index] = color?.ToHex() ?? string.Empty;

        SaveConfig();
        OnChanged();
    }


    /// <summary>
    /// Closes a projector.
    /// </summary>
    public static void CloseProjector(ProjectorWindow window)
    {
        window.Close();
    }


    /// <summary>
    /// Closes every projector.
    /// </summary>
    public void CloseAll()
    {
        // closing removes the window from the list
        foreach (var window in _windows.ToArray())
        {
            window.Close();
        }
    }


    /// <summary>
    /// Sets whether projectors follow the zoom, pan and transforms of the main viewer.
    /// </summary>
    public void SetViewSync(bool enabled)
    {
        Config.EnableViewSync = enabled;
        foreach (var window in _windows)
        {
            window.Viewer.EnableMirrorSync = enabled;
        }

        SaveConfig();
        OnChanged();
    }


    /// <summary>
    /// Sets whether projectors highlight where the cursor is on the main viewer.
    /// </summary>
    public void SetShowPointer(bool enabled)
    {
        Config.ShowPointer = enabled;
        if (enabled) AttachPointerTracking();
        else DetachPointerTracking();

        SaveConfig();
        OnChanged();
    }


    /// <summary>
    /// Gets the screens ordered left to right, then top to bottom, the order the user numbers them by.
    /// </summary>
    public IReadOnlyList<Screen> GetOrderedScreens()
    {
        return _screens.All
            .OrderBy(s => s.Bounds.X)
            .ThenBy(s => s.Bounds.Y)
            .ToArray();
    }


    /// <summary>
    /// Gets the screen the main window is on.
    /// </summary>
    public Screen? GetMainWindowScreen() => _screens.ScreenFromWindow(App.MainWindow);


    /// <summary>
    /// Gets the settings as JSON for <see cref="Config.ToolSettings"/>.
    /// </summary>
    public JsonElement SaveSettings()
    {
        return JsonSerializer.SerializeToElement(Config, ProjectorConfigJsonContext.Default.ProjectorConfig);
    }

    #endregion // Public Methods



    #region Private Methods

    private void Window_Closed(object? sender, EventArgs e)
    {
        if (sender is not ProjectorWindow window) return;

        window.Closed -= Window_Closed;
        window.LayoutChanged -= Window_LayoutChanged;
        _windows.Remove(window);

        if (_windows.Count == 0)
        {
            SleepGuard.Release(SLEEP_GUARD_OWNER);
        }

        OnChanged();
    }


    private void Window_LayoutChanged(object? sender, EventArgs e)
    {
        // the user may have dragged it onto another screen, or maximized it by hand
        OnChanged();
    }


    private void Screens_Changed(object? sender, EventArgs e)
    {
        // a new resolution, scale or work area leaves the tiled projectors off their cells
        _ = RetileAsync();
        OnChanged();
    }


    private void MainViewer_PointerMoved(object? sender, PointerEventArgs e)
    {
        var viewer = GetMainViewer();
        var clientPoint = e.GetPosition(viewer);
        _mainPointerPoint = clientPoint;

        UpdatePointers(viewer, clientPoint);
    }


    private void MainViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var viewer = GetMainViewer();
        var isLeftButton = e.GetCurrentPoint(viewer).Properties.IsLeftButtonPressed;
        if (!isLeftButton) return;

        foreach (var window in _windows)
        {
            window.PlayPointerClick();
        }
    }


    private void MainViewer_PointerExited(object? sender, PointerEventArgs e)
    {
        _mainPointerPoint = null;
        HidePointers();
    }


    private void MainViewer_RenderStateChanged(ViewerControl sender, EventArgs e)
    {
        // the photo moves under a still cursor as the main viewer zooms by key or shows another photo; raised on any thread
        var isFirstRequest = _isPointerRefreshPosted.SetTrue();
        if (!isFirstRequest) return;

        Dispatcher.UIThread.Post(RefreshPointers, DispatcherPriority.Background);
    }


    /// <summary>
    /// Starts following the cursor on the main viewer, for the pointers of the projectors.
    /// </summary>
    private void AttachPointerTracking()
    {
        if (_isTrackingPointer) return;
        _isTrackingPointer = true;

        // handled ones too, as the viewer takes presses and moves for panning
        var viewer = GetMainViewer();
        viewer.AddHandler(InputElement.PointerMovedEvent, MainViewer_PointerMoved, RoutingStrategies.Bubble, true);
        viewer.AddHandler(InputElement.PointerPressedEvent, MainViewer_PointerPressed, RoutingStrategies.Bubble, true);
        viewer.PointerExited += MainViewer_PointerExited;
        viewer.RenderStateChanged += MainViewer_RenderStateChanged;
    }


    /// <summary>
    /// Stops following the cursor, and hides the pointers.
    /// </summary>
    private void DetachPointerTracking()
    {
        if (!_isTrackingPointer) return;
        _isTrackingPointer = false;

        var viewer = GetMainViewer();
        viewer.RemoveHandler(InputElement.PointerMovedEvent, MainViewer_PointerMoved);
        viewer.RemoveHandler(InputElement.PointerPressedEvent, MainViewer_PointerPressed);
        viewer.PointerExited -= MainViewer_PointerExited;
        viewer.RenderStateChanged -= MainViewer_RenderStateChanged;

        _mainPointerPoint = null;
        HidePointers();
    }


    /// <summary>
    /// Shows the pointer of each projector at the point under <paramref name="clientPoint"/> of <paramref name="viewer"/>, placed relative to the photo.
    /// </summary>
    private void UpdatePointers(ViewerControl viewer, Point clientPoint)
    {
        // 1. the point where 0 to 1 spans the photo on each axis, so off the photo it keeps its place beside it
        var bitmapSize = viewer.BitmapSize;
        if (bitmapSize.IsEmpty)
        {
            HidePointers();
            return;
        }

        var sourcePoint = viewer.PointClientToSource(clientPoint);
        var relativePoint = new Point(sourcePoint.X / bitmapSize.Width, sourcePoint.Y / bitmapSize.Height);


        // 2. a projector off sync shows the photo neither turned nor flipped, so the point is elsewhere on it
        var transform = Core.ImageTransform;
        var hasFlips = transform.Flips != FlipOptions.None;
        var hasRotation = transform.Rotation % 360 != 0;
        var isTurned = hasFlips || hasRotation;

        foreach (var window in _windows)
        {
            var canPoint = CanPointOn(window, isTurned);
            if (canPoint) window.ShowPointer(relativePoint);
            else window.HidePointer();
        }
    }


    /// <summary>
    /// Places the pointers again for where the cursor last was on the main viewer, as the photo may have moved under it.
    /// </summary>
    private void RefreshPointers()
    {
        _ = _isPointerRefreshPosted.SetFalse();
        if (_mainPointerPoint is not { } clientPoint) return;

        UpdatePointers(GetMainViewer(), clientPoint);
    }


    /// <summary>
    /// Checks whether <paramref name="window"/> can show the pointer: in the same view of the photo as the main viewer.
    /// </summary>
    private static bool CanPointOn(ProjectorWindow window, bool isTurned)
    {
        if (window.Viewer.EnableMirrorSync) return true;

        return !isTurned;
    }


    /// <summary>
    /// Hides the pointer of every projector.
    /// </summary>
    private void HidePointers()
    {
        foreach (var window in _windows)
        {
            window.HidePointer();
        }
    }


    /// <summary>
    /// Puts each tiled projector still on its screen back into its cell.
    /// </summary>
    private async Task RetileAsync()
    {
        foreach (var window in _windows.ToArray())
        {
            if (IsDisposed) return;
            if (window.Tile is not { } tile) continue;

            // a projector the OS moved off an unplugged screen would cover another one
            var isOpen = _windows.Contains(window);
            var screen = window.GetScreen();
            var isOnTileScreen = screen is not null && screen == window.TileScreen;
            if (!isOpen || !isOnTileScreen) continue;

            await window.ShowInTileAsync(screen!, tile);
        }
    }


    private void OnChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Gets the viewer of the main window.
    /// </summary>
    private static ViewerControl GetMainViewer() => App.MainWindow.PART_MainView.PART_Viewer;


    /// <summary>
    /// Gets the smallest projector number not in use.
    /// </summary>
    private int GetFreeNumber()
    {
        var number = 1;
        while (_windows.Any(w => w.Number == number))
        {
            number++;
        }

        return number;
    }


    /// <summary>
    /// Picks a screen showing neither a projector nor the main window, else one without a projector.
    /// </summary>
    private Screen? PickFreeScreen()
    {
        var mainScreen = GetMainWindowScreen();
        var usedScreens = _windows
            .Select(w => w.GetScreen())
            .OfType<Screen>()
            .ToArray();

        var orderedScreens = GetOrderedScreens();
        var freeScreens = orderedScreens.Where(s => !usedScreens.Contains(s)).ToArray();

        return freeScreens.FirstOrDefault(s => s != mainScreen)
            ?? freeScreens.FirstOrDefault()
            ?? mainScreen
            ?? orderedScreens.FirstOrDefault();
    }


    /// <summary>
    /// Shows <paramref name="window"/> on <paramref name="screen"/> as <paramref name="mode"/> says.
    /// </summary>
    private async Task PlaceProjectorAsync(ProjectorWindow window, Screen screen, ProjectorWindowMode mode)
    {
        if (IsDisposed) return;

        var isOpen = _windows.Contains(window);
        if (!isOpen) return;

        await window.MoveToScreenAsync(screen, mode);
        OnChanged();
    }


    /// <summary>
    /// Gets how a projector covers <paramref name="screen"/>: as a normal window over the main window, else as preferred.
    /// </summary>
    private ProjectorWindowMode GetModeForScreen(Screen screen, ProjectorWindowMode preferredMode)
    {
        var isMainScreen = screen == GetMainWindowScreen();

        return isMainScreen ? ProjectorWindowMode.Normal : preferredMode;
    }


    /// <summary>
    /// Tiles <paramref name="window"/> with the other projectors of <see cref="GetTileSet"/>: in the layout the tiled ones share if it has room, else a fitting one.
    /// </summary>
    private async Task TileProjectorAsync(ProjectorWindow window)
    {
        var screen = window.GetScreen();
        if (screen is null) return;

        var tileSet = GetTileSet(screen, window);
        var tiledLayouts = tileSet
            .Select(w => w.Tile?.Layout)
            .OfType<ProjectorLayout>()
            .Distinct()
            .ToArray();

        var sharedLayout = tiledLayouts.Length == 1 ? tiledLayouts[0] : null;
        var hasRoom = sharedLayout?.UsedCells.Count >= tileSet.Count;
        var aspectRatio = ProjectorLayout.GetAspectRatio(screen.WorkingArea);
        var layout = hasRoom ? sharedLayout! : ProjectorLayout.GetDefault(tileSet.Count, aspectRatio);

        await ArrangeAsync(screen, layout, window);
    }


    /// <summary>
    /// Gets the background color saved for the projector of <paramref name="number"/>; <c>null</c> when it follows the slideshow background color.
    /// </summary>
    private Color? GetSavedBackgroundColor(int number)
    {
        var index = number - 1;
        var colors = Config.BackgroundColors;
        if (index < 0 || index >= colors.Count) return null;

        var hex = colors[index];
        if (string.IsNullOrWhiteSpace(hex)) return null;

        // an unreadable value parses as transparent, which no projector could show anyway
        var color = BHelper.ColorFromHex(hex, Core.AccentColor);
        if (color.A == 0) return null;

        return color;
    }


    /// <summary>
    /// Loads the settings from <see cref="Config.ToolSettings"/>.
    /// </summary>
    private void LoadConfig()
    {
        var hasSettings = Core.Config.ToolSettings.TryGetValue(TOOL_ID, out var jsonEl);
        if (!hasSettings) return;

        try
        {
            var config = jsonEl.Deserialize(ProjectorConfigJsonContext.Default.ProjectorConfig);
            if (config is not null) Config = config;
        }
        catch (Exception ex)
        {
            // a broken value keeps the defaults rather than failing the tool
            Debug.WriteLine($"❌❌❌ {nameof(ProjectorManager)}.{nameof(LoadConfig)}: {ex.Message}");
        }
    }


    /// <summary>
    /// Writes the settings to <see cref="Config.ToolSettings"/>, saved to disk with the app config.
    /// </summary>
    private void SaveConfig()
    {
        ToolRegistry.SetToolSettings(TOOL_ID, SaveSettings());
    }

    #endregion // Private Methods

}

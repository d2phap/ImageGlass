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
using Avalonia.Media;
using Avalonia.Platform;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
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
    /// The most projectors that can be open at once with ImageGlass Pro.
    /// </summary>
    public const int MAX_PRO_PROJECTORS = 4;

    /// <summary>
    /// The most projectors that can be open at once without ImageGlass Pro.
    /// </summary>
    public const int MAX_CLASSIC_PROJECTORS = 1;

    // owner key of the projectors' request in SleepGuard
    private const string SLEEP_GUARD_OWNER = "projector";

    private readonly List<ProjectorWindow> _windows = [];
    private readonly Screens _screens;


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
    /// Gets the most projectors that can be open at once.
    /// </summary>
    public static int MaxProjectors => Core.IsProEnabled ? MAX_PRO_PROJECTORS : MAX_CLASSIC_PROJECTORS;


    /// <summary>
    /// Gets whether another projector can be opened.
    /// </summary>
    public bool CanAddProjector => _windows.Count < MaxProjectors;


    /// <summary>
    /// Gets whether only ImageGlass Pro could open another projector.
    /// </summary>
    public bool IsLimitedByLicense => !CanAddProjector && _windows.Count < MAX_PRO_PROJECTORS;

    #endregion // Public Properties



    public ProjectorManager()
    {
        LoadConfig();

        _screens = App.MainWindow.Screens;
        _screens.Changed += Screens_Changed;
    }


    protected override void OnDisposing()
    {
        base.OnDisposing();

        _screens.Changed -= Screens_Changed;
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
    /// Tiles the projectors on <paramref name="screen"/> into the used cells of <paramref name="layout"/>, in number order.
    /// </summary>
    public async Task ArrangeAsync(Screen screen, ProjectorLayout layout)
    {
        if (IsDisposed) return;

        var windows = GetWindowsOnScreen(screen);
        if (windows.Count == 0) return;
        if (windows.Count > layout.UsedCells.Count) return;

        // the windows are independent, so they move into their cells together
        var tileTasks = windows.Select((window, i) => window.ShowInTileAsync(screen, new ProjectorTile(layout, layout.UsedCells[i])));
        await Task.WhenAll(tileTasks);

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
    /// Gets the grid all projectors on <paramref name="screen"/> are tiled into; <c>null</c> when they are not.
    /// </summary>
    public ProjectorLayout? GetActiveLayout(Screen screen)
    {
        var windows = GetWindowsOnScreen(screen);
        if (windows.Count == 0) return null;

        var layout = windows[0].Tile?.Layout;
        if (layout is null) return null;

        var isSameGrid = windows.All(w => w.Tile?.Layout == layout);
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
    /// Tiles <paramref name="window"/> with the other projectors on its screen: in the layout the tiled ones share if it has room, else a fitting one.
    /// </summary>
    private async Task TileProjectorAsync(ProjectorWindow window)
    {
        var screen = window.GetScreen();
        if (screen is null) return;

        var windows = GetWindowsOnScreen(screen);
        var tiledLayouts = windows
            .Select(w => w.Tile?.Layout)
            .OfType<ProjectorLayout>()
            .Distinct()
            .ToArray();

        var sharedLayout = tiledLayouts.Length == 1 ? tiledLayouts[0] : null;
        var hasRoom = sharedLayout is not null && sharedLayout.UsedCells.Count >= windows.Count;
        var aspectRatio = ProjectorLayout.GetAspectRatio(screen.WorkingArea);
        var layout = hasRoom ? sharedLayout! : ProjectorLayout.GetDefault(windows.Count, aspectRatio);

        await ArrangeAsync(screen, layout);
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

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
using Avalonia.Platform;
using ImageGlass.Common;
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
    /// Opens a projector on <paramref name="screen"/>, else where it was last shown, else on a free screen; <c>null</c> when none can open.
    /// </summary>
    public async Task<ProjectorWindow?> AddProjectorAsync(Screen? screen = null)
    {
        if (IsDisposed) return null;
        if (!CanAddProjector) return null;

        // 1. pick the number, screen and mode
        var number = GetFreeNumber();
        var placement = GetPlacement(number);
        var savedScreen = FindScreen(placement);
        var target = screen ?? savedScreen ?? PickFreeScreen();
        if (target is null) return null;

        // a saved screen comes with its saved mode
        var isSavedScreen = target == savedScreen;
        var mode = isSavedScreen ? placement!.WindowMode : GetDefaultMode(target);


        // 2. open it
        var window = new ProjectorWindow(number, GetMainViewer());
        window.Viewer.EnableMirrorSync = Config.EnableViewSync;
        window.Viewer.ZoomMode = Config.ZoomMode;
        window.Closed += Window_Closed;
        window.LayoutChanged += Window_LayoutChanged;

        _windows.Add(window);
        _windows.Sort((a, b) => a.Number.CompareTo(b.Number));

        // an audience watches a projector hands-off, so the screens must stay on
        SleepGuard.Acquire(SLEEP_GUARD_OWNER, $"{BHelper.AppDisplayName} projector");

        await window.ShowOnScreenAsync(target, mode);


        // 3. remember where it is
        SavePlacement(window, target, mode);
        OnChanged();

        return window;
    }


    /// <summary>
    /// Moves <paramref name="window"/> onto <paramref name="screen"/>, covering it as <paramref name="mode"/> says, or as it does now.
    /// </summary>
    public async Task MoveProjectorAsync(ProjectorWindow window, Screen screen, ProjectorWindowMode? mode = null)
    {
        if (IsDisposed) return;

        var isOpen = _windows.Contains(window);
        if (!isOpen) return;

        var newMode = mode ?? window.Mode;
        await window.MoveToScreenAsync(screen, newMode);

        SavePlacement(window, screen, newMode);
        OnChanged();
    }


    /// <summary>
    /// Changes how <paramref name="window"/> covers the screen it is on.
    /// </summary>
    public async Task SetWindowModeAsync(ProjectorWindow window, ProjectorWindowMode mode)
    {
        var screen = window.GetScreen() ?? PickFreeScreen();
        if (screen is null) return;

        await MoveProjectorAsync(window, screen, mode);
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
    /// Sets how projectors fit the photo while they do not follow the main viewer.
    /// </summary>
    public void SetZoomMode(ZoomMode mode)
    {
        Config.ZoomMode = mode;
        foreach (var window in _windows)
        {
            window.Viewer.ZoomMode = mode;
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
        if (sender is ProjectorWindow window) RememberPlacement(window);

        OnChanged();
    }


    /// <summary>
    /// Remembers the screen and mode a shown projector has now.
    /// </summary>
    private void RememberPlacement(ProjectorWindow window)
    {
        if (!window.IsVisible) return;

        var screen = window.GetScreen();
        if (screen is null) return;

        SavePlacement(window, screen, window.Mode);
    }


    private void Screens_Changed(object? sender, EventArgs e)
    {
        OnChanged();
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
    /// Gets where the projector of <paramref name="number"/> was last shown.
    /// </summary>
    private ProjectorPlacement? GetPlacement(int number)
    {
        var index = number - 1;
        if (index < 0) return null;
        if (index >= Config.Placements.Count) return null;

        return Config.Placements[index];
    }


    /// <summary>
    /// Finds the connected screen a placement was saved for.
    /// </summary>
    private Screen? FindScreen(ProjectorPlacement? placement)
    {
        if (placement is null) return null;

        var screens = _screens.All;
        var exactMatch = screens.FirstOrDefault(s => s.DisplayName == placement.ScreenName
            && s.Bounds.X == placement.ScreenX
            && s.Bounds.Y == placement.ScreenY);
        if (exactMatch is not null) return exactMatch;

        // the screens may have been rearranged, or another screen may have the name now
        var nameMatch = screens.FirstOrDefault(s => s.DisplayName == placement.ScreenName);
        if (nameMatch is not null) return nameMatch;

        return screens.FirstOrDefault(s => s.Bounds.X == placement.ScreenX
            && s.Bounds.Y == placement.ScreenY);
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
    /// Gets how a new projector covers <paramref name="screen"/>: never over the main window by default.
    /// </summary>
    private ProjectorWindowMode GetDefaultMode(Screen screen)
    {
        var isMainScreen = screen == GetMainWindowScreen();

        return isMainScreen ? ProjectorWindowMode.Normal : ProjectorWindowMode.FullScreen;
    }


    /// <summary>
    /// Remembers where a projector is shown, so it opens there again.
    /// </summary>
    private void SavePlacement(ProjectorWindow window, Screen screen, ProjectorWindowMode mode)
    {
        var index = window.Number - 1;
        while (Config.Placements.Count <= index)
        {
            Config.Placements.Add(new ProjectorPlacement());
        }

        Config.Placements[index] = new ProjectorPlacement
        {
            ScreenName = screen.DisplayName ?? string.Empty,
            ScreenX = screen.Bounds.X,
            ScreenY = screen.Bounds.Y,
            WindowMode = mode,
        };

        SaveConfig();
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
        Core.Config.ToolSettings[TOOL_ID] = SaveSettings();
    }

    #endregion // Private Methods

}

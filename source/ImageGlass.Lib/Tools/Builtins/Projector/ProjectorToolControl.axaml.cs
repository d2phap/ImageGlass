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
using Avalonia.Layout;
using Avalonia.Platform;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageGlass.Tools;


/// <summary>
/// Hosted tool to open projector windows, place them on screens, and sync them with the main viewer.
/// </summary>
public partial class ProjectorToolControl : PhControl, IToolControl
{
    // the ComboBox items follow these orders, so an index maps to a value
    private static readonly ProjectorWindowMode[] _windowModes =
    [
        ProjectorWindowMode.FullScreen,
        ProjectorWindowMode.Maximized,
        ProjectorWindowMode.Normal,
        ProjectorWindowMode.Tiled,
    ];
    private static readonly ZoomMode[] _zoomModes =
    [
        ZoomMode.AutoZoom,
        ZoomMode.ScaleToFit,
        ZoomMode.ScaleToFill,
        ZoomMode.ScaleToWidth,
        ZoomMode.ScaleToHeight,
    ];

    // a press on a projector name turns into a drag once the pointer moves this far
    private const double DRAG_THRESHOLD = 4;

    // where the dragged name sits from the pointer, clear of the cursor
    private const double DRAG_GHOST_OFFSET = 14;

    // the most layouts in a row of the layout menu
    private const int MAX_LAYOUT_COLUMNS = 3;

    private readonly Dictionary<ProjectorWindow, ProjectorRow> _rows = [];
    private readonly List<ProjectorWindow> _rowOrder = [];
    private readonly List<LayoutOption> _layoutOptions = [];
    private ProjectorManager? _manager;

    // the projector whose name is being dragged onto a monitor
    private ProjectorWindow? _dragWindow;
    private Point _dragStart;
    private bool _isDragging;

    // the monitor the layout menu is open for, and the projectors it shows
    private Screen? _layoutScreen;
    private int[] _layoutNumbers = [];

    // prevents feedback loops while the controls are filled from the projectors
    private bool _isUpdatingUI;


    public static string TOOL_ID => ProjectorManager.TOOL_ID;
    public string ToolId => TOOL_ID;
    public bool HasSettingsUI => false;
    public object? Settings => Core.Projectors?.Config;
    public ViewerControl Viewer { get; set; } = null!;


    public ProjectorToolControl()
    {
        InitializeComponent();
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _manager = Core.GetProjectors();
        _manager.Changed += Manager_Changed;
        PART_ScreenMap.Manager = _manager;

        PopulateZoomModeItems();
        RefreshUI();

        PART_ScreenMap.ScreenClicked += PART_ScreenMap_ScreenClicked;
        PART_LayoutPopup.Closed += PART_LayoutPopup_Closed;
        PART_BtnAddFirst.Click += PART_BtnAdd_Click;
        PART_BtnAdd.Click += PART_BtnAdd_Click;
        PART_ChkSync.IsCheckedChanged += PART_ChkSync_IsCheckedChanged;
        PART_CmbZoomMode.SelectionChanged += PART_CmbZoomMode_SelectionChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _manager?.Changed -= Manager_Changed;
        PART_ScreenMap.Manager = null;
        PART_LayoutPopup.IsOpen = false;
        EndDrag();

        PART_ScreenMap.ScreenClicked -= PART_ScreenMap_ScreenClicked;
        PART_LayoutPopup.Closed -= PART_LayoutPopup_Closed;
        PART_BtnAddFirst.Click -= PART_BtnAdd_Click;
        PART_BtnAdd.Click -= PART_BtnAdd_Click;
        PART_ChkSync.IsCheckedChanged -= PART_ChkSync_IsCheckedChanged;
        PART_CmbZoomMode.SelectionChanged -= PART_CmbZoomMode_SelectionChanged;

        base.OnUnloaded(e);
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        var addText = Core.Lang[LangId.Tool_Projector_BtnAdd];
        var addTooltip = AppAPIProvider.GetMenuTooltipText(LangId.Tool_Projector_BtnAdd);
        PART_BtnAddFirst.Text = addText;
        PART_BtnAdd.Text = addText;
        ToolTip.SetTip(PART_BtnAddFirst, addTooltip);
        ToolTip.SetTip(PART_BtnAdd, addTooltip);

        // the rows and items carry text set in code
        RelocalizeZoomModeItems();
        foreach (var row in _rows.Values)
        {
            RelocalizeRow(row);
        }
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // the default background color comes from the theme
        RefreshUI();
    }


    private void Manager_Changed(object? sender, EventArgs e)
    {
        RefreshUI();
    }


    private void PART_ScreenMap_ScreenClicked(ScreenMapControl sender, Screen screen)
    {
        OpenLayoutMenu(screen);
    }


    private void PART_LayoutPopup_Closed(object? sender, EventArgs e)
    {
        _layoutScreen = null;
        _layoutNumbers = [];
    }


    private async void PART_BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        _ = await Core.API.RunApiAsync(API.IG_AddProjector);
    }


    private async void PART_ChkSync_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingUI) return;

        var isChecked = PART_ChkSync.IsChecked == true;
        _ = await Core.API.RunApiAsync(API.IG_ToggleProjectorSync, isChecked.ToString());
    }


    private void PART_CmbZoomMode_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingUI) return;

        var index = PART_CmbZoomMode.SelectedIndex;
        if (index < 0) return;

        _manager?.SetZoomMode(_zoomModes[index]);
    }

    #endregion // Control Events



    #region Control Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void LoadSettings(JsonElement? jsonEl)
    {
        // the projectors read their own settings when first used, and stay open without this panel
        _ = Core.GetProjectors();
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public JsonElement? SaveSettings()
    {
        return Core.GetProjectors().SaveSettings();
    }


    /// <summary>
    /// Fills the controls from the open projectors and their settings.
    /// </summary>
    private void RefreshUI()
    {
        if (_manager is null) return;

        _isUpdatingUI = true;
        try
        {
            // 1. with nothing open, only the button to open one
            var windows = _manager.Windows;
            var hasProjectors = windows.Count > 0;
            PART_BtnAddFirst.IsVisible = !hasProjectors;
            PART_Projectors.IsVisible = hasProjectors;


            // 2. a row per projector: its monitor, how it covers it, and its color
            SyncRows();

            var screens = _manager.GetOrderedScreens();
            var defaultColor = ProjectorWindow.DefaultBackgroundColor;
            foreach (var row in _rows.Values)
            {
                SyncMonitorItems(row.CmbMonitor, screens.Count);
                row.CmbMonitor.SelectedIndex = IndexOfScreen(screens, row.Window.GetScreen());
                row.CmbMode.SelectedIndex = Array.IndexOf(_windowModes, row.Window.Mode);
                row.ColorPicker.DefaultColor = defaultColor;
                row.ColorPicker.SelectedColor = row.Window.BackgroundColor ?? defaultColor;
            }


            // 3. the actions; Classic still gets the button, which explains the Pro limit
            var isAtProLimit = windows.Count >= ProjectorManager.MAX_PRO_PROJECTORS;
            PART_BtnAdd.IsEnabled = !isAtProLimit;
            PART_ProBadge.IsVisible = _manager.IsLimitedByLicense;


            // 4. the options; the zoom mode only applies while not syncing
            var isSynced = _manager.Config.EnableViewSync;
            PART_ChkSync.IsChecked = isSynced;
            PART_ZoomModeGroup.IsVisible = !isSynced;
            PART_CmbZoomMode.SelectedIndex = Array.IndexOf(_zoomModes, _manager.Config.ZoomMode);


            // 5. an open layout menu follows the projectors on its monitor
            SyncLayoutMenu();
        }
        finally
        {
            _isUpdatingUI = false;
        }
    }


    /// <summary>
    /// Adds and removes rows so the list matches the open projectors, in number order.
    /// </summary>
    private void SyncRows()
    {
        var windows = _manager!.Windows;

        // 1. drop the rows of closed projectors
        var closedWindows = _rows.Keys.Where(w => !windows.Contains(w)).ToArray();
        foreach (var window in closedWindows)
        {
            _ = _rows.Remove(window);
        }

        // 2. add rows for new ones
        foreach (var window in windows)
        {
            var hasRow = _rows.ContainsKey(window);
            if (!hasRow) _rows[window] = CreateRow(window);
        }

        // 3. lay them out in order, leaving rows in place when nothing changed, as an open dropdown would close
        var isInOrder = _rowOrder.SequenceEqual(windows);
        if (isInOrder) return;

        _rowOrder.Clear();
        _rowOrder.AddRange(windows);
        PART_ProjectorList.Children.Clear();
        PART_ProjectorList.RowDefinitions.Clear();

        for (var rowIndex = 0; rowIndex < windows.Count; rowIndex++)
        {
            var row = _rows[windows[rowIndex]];
            Control[] cells = [row.Chip, row.CmbMonitor, row.CmbMode, row.ColorPicker, row.BtnClose];

            PART_ProjectorList.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            for (var column = 0; column < cells.Length; column++)
            {
                Grid.SetRow(cells[column], rowIndex);
                Grid.SetColumn(cells[column], column);
                PART_ProjectorList.Children.Add(cells[column]);
            }
        }
    }


    /// <summary>
    /// Creates the controls of one projector: its name to drag onto a monitor, its monitor, how it covers it, its color, and closing it.
    /// </summary>
    private ProjectorRow CreateRow(ProjectorWindow window)
    {
        // 1. the name: hovering shows its monitor on the map, dragging moves it to another one
        var chip = new PhToolButton
        {
            Padding = new Thickness(10, 5),
            Focusable = false,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        chip.PointerEntered += (_, _) => PART_ScreenMap.SelectedProjectorNumber = window.Number;
        chip.PointerExited += (_, _) =>
        {
            if (_dragWindow is null) PART_ScreenMap.SelectedProjectorNumber = 0;
        };
        chip.AddHandler(PointerPressedEvent, (_, e) => StartDrag(window, chip, e), RoutingStrategies.Bubble, true);
        chip.AddHandler(PointerMovedEvent, (_, e) => ContinueDrag(e), RoutingStrategies.Bubble, true);
        chip.AddHandler(PointerReleasedEvent, (_, _) => DropDraggedProjector(), RoutingStrategies.Bubble, true);
        chip.PointerCaptureLost += (_, _) => EndDrag();


        // 2. the monitor it is on
        var cmbMonitor = new ComboBox
        {
            MinWidth = 120,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        cmbMonitor.SelectionChanged += async (_, _) => await MoveToMonitorAsync(window, cmbMonitor.SelectedIndex);


        // 3. how it covers the monitor
        var cmbMode = new ComboBox
        {
            MinWidth = 120,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        foreach (var _ in _windowModes)
        {
            cmbMode.Items.Add(new ComboBoxItem());
        }
        cmbMode.SelectionChanged += async (_, _) =>
        {
            if (_isUpdatingUI) return;
            if (_manager is null) return;

            var index = cmbMode.SelectedIndex;
            if (index < 0) return;

            await _manager.SetWindowModeAsync(window, _windowModes[index]);
        };


        // 4. its background color
        var colorPicker = new PhColorPickerControl
        {
            ShowHexLabel = false,
            ShowResetButton = false,
            SwatchWidth = 24,
            VerticalAlignment = VerticalAlignment.Center,
        };
        colorPicker.ColorChanged += (_, _) =>
        {
            if (_isUpdatingUI) return;
            if (_manager is null) return;

            // the default color is kept as no color, so it follows the slideshow background color
            var color = colorPicker.SelectedColor;
            var isDefaultColor = color == colorPicker.DefaultColor;

            _manager.SetBackgroundColor(window, isDefaultColor ? null : color);
        };


        // 5. closing it
        var btnClose = new PhToolButton
        {
            Padding = new Thickness(6),
            Focusable = false,
            VerticalAlignment = VerticalAlignment.Center,
            Content = new PathIcon
            {
                Width = 12,
                Height = 12,
                Data = Resx.GetIcon(ResxIconId.IconClose),
            },
        };
        btnClose.Click += (_, _) => ProjectorManager.CloseProjector(window);

        var row = new ProjectorRow(window, chip, cmbMonitor, cmbMode, colorPicker, btnClose);
        RelocalizeRow(row);

        return row;
    }


    /// <summary>
    /// Moves <paramref name="window"/> to the monitor picked in its row.
    /// </summary>
    private async Task MoveToMonitorAsync(ProjectorWindow window, int monitorIndex)
    {
        if (_isUpdatingUI) return;
        if (_manager is null) return;

        var screens = _manager.GetOrderedScreens();
        if (monitorIndex < 0 || monitorIndex >= screens.Count) return;

        var screen = screens[monitorIndex];
        if (screen == window.GetScreen()) return;

        await _manager.MoveProjectorAsync(window, screen);
    }


    /// <summary>
    /// Gives <paramref name="combo"/> an item per monitor, numbered as on the screen map.
    /// </summary>
    private static void SyncMonitorItems(ComboBox combo, int monitorCount)
    {
        while (combo.ItemCount > monitorCount)
        {
            combo.Items.RemoveAt(combo.ItemCount - 1);
        }

        while (combo.ItemCount < monitorCount)
        {
            var monitorNumber = combo.ItemCount + 1;
            combo.Items.Add(new ComboBoxItem { Content = Core.Lang[LangId.Tool_Projector_Monitor, monitorNumber] });
        }
    }


    /// <summary>
    /// Gets where <paramref name="screen"/> is in <paramref name="screens"/>; <c>-1</c> when it is not there.
    /// </summary>
    private static int IndexOfScreen(IReadOnlyList<Screen> screens, Screen? screen)
    {
        if (screen is null) return -1;

        for (var i = 0; i < screens.Count; i++)
        {
            if (screens[i] == screen) return i;
        }

        return -1;
    }


    /// <summary>
    /// Fills the zoom mode ComboBox, in the order of <see cref="_zoomModes"/>.
    /// </summary>
    private void PopulateZoomModeItems()
    {
        if (PART_CmbZoomMode.ItemCount > 0) return;

        foreach (var _ in _zoomModes)
        {
            PART_CmbZoomMode.Items.Add(new ComboBoxItem());
        }

        RelocalizeZoomModeItems();
    }


    private void RelocalizeZoomModeItems()
    {
        for (var i = 0; i < PART_CmbZoomMode.ItemCount; i++)
        {
            if (PART_CmbZoomMode.Items[i] is not ComboBoxItem item) continue;
            item.Content = GetZoomModeText(_zoomModes[i]);
        }
    }


    private static void RelocalizeRow(ProjectorRow row)
    {
        var projectorName = Core.Lang[LangId.Tool_Projector_WindowTitle, row.Window.Number];
        var colorText = Core.Lang[LangId._BackgroundColor];

        row.Chip.Content = projectorName;
        row.ColorPicker.Title = $"{projectorName} - {colorText}";
        ToolTip.SetTip(row.Chip, Core.Lang[LangId.Tool_Projector_DragHint]);
        ToolTip.SetTip(row.ColorPicker, colorText);
        ToolTip.SetTip(row.BtnClose, Core.Lang[LangId._Close]);

        for (var i = 0; i < row.CmbMonitor.ItemCount; i++)
        {
            if (row.CmbMonitor.Items[i] is not ComboBoxItem item) continue;
            item.Content = Core.Lang[LangId.Tool_Projector_Monitor, i + 1];
        }

        for (var i = 0; i < row.CmbMode.ItemCount; i++)
        {
            if (row.CmbMode.Items[i] is not ComboBoxItem item) continue;
            item.Content = GetWindowModeText(_windowModes[i]);
        }
    }


    private static string GetWindowModeText(ProjectorWindowMode mode) => mode switch
    {
        ProjectorWindowMode.FullScreen => Core.Lang[LangId.Tool_Projector_ModeFullScreen],
        ProjectorWindowMode.Maximized => Core.Lang[LangId.Tool_Projector_ModeMaximized],
        ProjectorWindowMode.Tiled => Core.Lang[LangId.Tool_Projector_ModeTiled],
        _ => Core.Lang[LangId.Tool_Projector_ModeNormal],
    };


    private static string GetZoomModeText(ZoomMode mode) => mode switch
    {
        ZoomMode.ScaleToFit => Core.Lang[LangId.Menu_MnuScaleToFit],
        ZoomMode.ScaleToFill => Core.Lang[LangId.Menu_MnuScaleToFill],
        ZoomMode.ScaleToWidth => Core.Lang[LangId.Menu_MnuScaleToWidth],
        ZoomMode.ScaleToHeight => Core.Lang[LangId.Menu_MnuScaleToHeight],
        _ => Core.Lang[LangId.Menu_MnuAutoZoom],
    };

    #endregion // Control Methods



    #region Dragging A Projector

    /// <summary>
    /// Starts tracking a press on the name of <paramref name="window"/>, which turns into a drag once the pointer moves.
    /// </summary>
    private void StartDrag(ProjectorWindow window, Control chip, PointerPressedEventArgs e)
    {
        var isLeftButton = e.GetCurrentPoint(chip).Properties.IsLeftButtonPressed;
        if (!isLeftButton) return;

        _dragWindow = window;
        _dragStart = e.GetPosition(this);
        _isDragging = false;

        // the moves keep coming once the pointer leaves the name
        e.Pointer.Capture(chip);
    }


    /// <summary>
    /// Moves the dragged name with the pointer, and marks the monitor it would drop on.
    /// </summary>
    private void ContinueDrag(PointerEventArgs e)
    {
        if (_dragWindow is null) return;

        // 1. a small slip of the pointer is still a click
        if (!_isDragging)
        {
            var moved = e.GetPosition(this) - _dragStart;
            var isFarEnough = Math.Abs(moved.X) > DRAG_THRESHOLD || Math.Abs(moved.Y) > DRAG_THRESHOLD;
            if (!isFarEnough) return;

            _isDragging = true;
            ShowDragGhost(_dragWindow);
        }

        // 2. follow the pointer
        var ghostPosition = e.GetPosition(PART_DragLayer);
        Canvas.SetLeft(PART_DragGhost, ghostPosition.X + DRAG_GHOST_OFFSET);
        Canvas.SetTop(PART_DragGhost, ghostPosition.Y + DRAG_GHOST_OFFSET);

        var mapPosition = e.GetPosition(PART_ScreenMap);
        PART_ScreenMap.DropTargetScreen = PART_ScreenMap.GetScreenAt(mapPosition);
    }


    /// <summary>
    /// Moves the dragged projector to the monitor it was dropped on.
    /// </summary>
    private async void DropDraggedProjector()
    {
        var window = _dragWindow;
        var target = _isDragging ? PART_ScreenMap.DropTargetScreen : null;
        EndDrag();

        if (_manager is null) return;
        if (window is null || target is null) return;
        if (target == window.GetScreen()) return;

        await _manager.MoveProjectorAsync(window, target);
    }


    /// <summary>
    /// Shows the name of <paramref name="window"/> following the pointer, colored like its badge on the map.
    /// </summary>
    private void ShowDragGhost(ProjectorWindow window)
    {
        var accent = Core.AccentColor;

        PART_DragGhost.Background = accent.ToBrush();
        PART_DragGhostText.Foreground = accent.InvertBlackOrWhite().ToBrush();
        PART_DragGhostText.Text = Core.Lang[LangId.Tool_Projector_WindowTitle, window.Number];
        PART_DragGhost.IsVisible = true;
    }


    /// <summary>
    /// Ends a drag, dropped or not.
    /// </summary>
    private void EndDrag()
    {
        if (_isDragging) PART_ScreenMap.SelectedProjectorNumber = 0;

        _dragWindow = null;
        _isDragging = false;
        PART_DragGhost.IsVisible = false;
        PART_ScreenMap.DropTargetScreen = null;
    }

    #endregion // Dragging A Projector



    #region Layout Menu

    /// <summary>
    /// Opens the layouts of <paramref name="screen"/>, dropping down from it on the screen map.
    /// </summary>
    private void OpenLayoutMenu(Screen screen)
    {
        if (_manager is null) return;

        var windows = _manager.GetWindowsOnScreen(screen);
        if (windows.Count == 0) return;

        _layoutScreen = screen;
        BuildLayoutOptions(screen, windows);

        PART_LayoutPopup.PlacementRect = PART_ScreenMap.GetScreenBounds(screen);
        PART_LayoutPopup.IsOpen = true;
    }


    /// <summary>
    /// Fills the menu with the default, then each layout the projectors on <paramref name="screen"/> fit in.
    /// </summary>
    private void BuildLayoutOptions(Screen screen, IReadOnlyList<ProjectorWindow> windows)
    {
        PART_LayoutOptions.Children.Clear();
        _layoutOptions.Clear();

        var screens = _manager!.GetOrderedScreens();
        PART_LblLayout.LangParams = IndexOfScreen(screens, screen) + 1;

        _layoutNumbers = windows.Select(w => w.Number).ToArray();
        var aspectRatio = ProjectorLayout.GetAspectRatio(screen.WorkingArea);

        AddLayoutOption(null, aspectRatio);
        foreach (var layout in ProjectorLayout.GetLayouts(_layoutNumbers.Length, aspectRatio))
        {
            AddLayoutOption(layout, aspectRatio);
        }

        PART_LayoutOptions.Columns = Math.Min(MAX_LAYOUT_COLUMNS, _layoutOptions.Count);
        UpdateLayoutOptionStates();
    }


    /// <summary>
    /// Adds a button that lays the projectors out as <paramref name="layout"/>; <c>null</c> for the default.
    /// </summary>
    private void AddLayoutOption(ProjectorLayout? layout, double aspectRatio)
    {
        var thumbnail = new LayoutThumbnailControl(layout, _layoutNumbers, aspectRatio);
        var button = new PhToolButton
        {
            Padding = new Thickness(6),
            Focusable = false,
            Content = thumbnail,
        };
        button.Click += async (_, _) => await ApplyLayoutAsync(layout);

        PART_LayoutOptions.Children.Add(button);
        _layoutOptions.Add(new LayoutOption(layout, thumbnail));
    }


    /// <summary>
    /// Lays out the projectors of the menu's monitor as <paramref name="layout"/>; <c>null</c> restores the default.
    /// </summary>
    private async Task ApplyLayoutAsync(ProjectorLayout? layout)
    {
        var screen = _layoutScreen;
        PART_LayoutPopup.IsOpen = false;

        if (_manager is null) return;
        if (screen is null) return;

        if (layout is null) await _manager.RestoreDefaultAsync(screen);
        else await _manager.ArrangeAsync(screen, layout);
    }


    /// <summary>
    /// Keeps an open menu in step with the projectors on its monitor: closed when none is left, rebuilt when they change.
    /// </summary>
    private void SyncLayoutMenu()
    {
        if (!PART_LayoutPopup.IsOpen) return;
        if (_layoutScreen is not { } screen) return;

        var windows = _manager!.GetWindowsOnScreen(screen);
        if (windows.Count == 0)
        {
            PART_LayoutPopup.IsOpen = false;
            return;
        }

        var projectorNumbers = windows.Select(w => w.Number).ToArray();
        var isSameProjectors = projectorNumbers.SequenceEqual(_layoutNumbers);
        if (isSameProjectors) UpdateLayoutOptionStates();
        else BuildLayoutOptions(screen, windows);
    }


    /// <summary>
    /// Marks the option the projectors on the menu's monitor are laid out in now.
    /// </summary>
    private void UpdateLayoutOptionStates()
    {
        if (_manager is null) return;
        if (_layoutScreen is not { } screen) return;

        var windows = _manager.GetWindowsOnScreen(screen);
        var activeLayout = _manager.GetActiveLayout(screen);
        var isDefault = windows.All(w => w.Tile is null);

        foreach (var option in _layoutOptions)
        {
            option.Thumbnail.IsActive = option.Layout is null ? isDefault : option.Layout == activeLayout;
        }
    }

    #endregion // Layout Menu



    /// <summary>
    /// The controls of one open projector in the list.
    /// </summary>
    private sealed record ProjectorRow(ProjectorWindow Window, PhToolButton Chip, ComboBox CmbMonitor, ComboBox CmbMode,
        PhColorPickerControl ColorPicker, PhToolButton BtnClose);


    /// <summary>
    /// A layout in the menu, by its thumbnail; <c>null</c> for the default.
    /// </summary>
    private sealed record LayoutOption(ProjectorLayout? Layout, LayoutThumbnailControl Thumbnail);

}

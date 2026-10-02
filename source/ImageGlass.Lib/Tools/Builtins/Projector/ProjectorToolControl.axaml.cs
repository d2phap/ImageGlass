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
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Viewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
    private static readonly int TILED_MODE_INDEX = Array.IndexOf(_windowModes, ProjectorWindowMode.Tiled);
    private static readonly ZoomMode[] _zoomModes =
    [
        ZoomMode.AutoZoom,
        ZoomMode.ScaleToFit,
        ZoomMode.ScaleToFill,
        ZoomMode.ScaleToWidth,
        ZoomMode.ScaleToHeight,
    ];

    private readonly Dictionary<ProjectorWindow, ProjectorRow> _rows = [];
    private readonly List<LayoutOption> _layoutOptions = [];
    private ProjectorManager? _manager;

    // what the layout groups show, so they are rebuilt only when it changes
    private string _layoutGroupsKey = string.Empty;

    // the projector a click on the screen map moves
    private ProjectorWindow? _selectedWindow;

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
        RefreshUI(selectNewProjectors: false);

        PART_ScreenMap.ScreenClicked += PART_ScreenMap_ScreenClicked;
        PART_BtnAdd.Click += PART_BtnAdd_Click;
        PART_BtnCloseAll.Click += PART_BtnCloseAll_Click;
        PART_ChkSync.IsCheckedChanged += PART_ChkSync_IsCheckedChanged;
        PART_CmbZoomMode.SelectionChanged += PART_CmbZoomMode_SelectionChanged;
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _manager?.Changed -= Manager_Changed;
        PART_ScreenMap.Manager = null;

        PART_ScreenMap.ScreenClicked -= PART_ScreenMap_ScreenClicked;
        PART_BtnAdd.Click -= PART_BtnAdd_Click;
        PART_BtnCloseAll.Click -= PART_BtnCloseAll_Click;
        PART_ChkSync.IsCheckedChanged -= PART_ChkSync_IsCheckedChanged;
        PART_CmbZoomMode.SelectionChanged -= PART_CmbZoomMode_SelectionChanged;

        base.OnUnloaded(e);
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        PART_BtnAdd.Text = Core.Lang[LangId.Tool_Projector_BtnAdd];
        PART_BtnCloseAll.Text = Core.Lang[LangId.Tool_Projector_BtnCloseAll];
        ToolTip.SetTip(PART_BtnAdd, AppAPIProvider.GetMenuTooltipText(LangId.Tool_Projector_BtnAdd));
        ToolTip.SetTip(PART_BtnCloseAll, AppAPIProvider.GetMenuTooltipText(LangId.Tool_Projector_BtnCloseAll));

        // the rows and items carry text set in code
        RelocalizeZoomModeItems();
        foreach (var row in _rows.Values)
        {
            RelocalizeRow(row);
        }

        UpdateHint();

        // the layout buttons carry tooltips set in code too, so build them again
        _layoutGroupsKey = string.Empty;
        RefreshUI(selectNewProjectors: false);
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // the default background color comes from the theme
        RefreshUI(selectNewProjectors: false);
    }


    private void Manager_Changed(object? sender, EventArgs e)
    {
        RefreshUI(selectNewProjectors: true);
    }


    private async void PART_ScreenMap_ScreenClicked(ScreenMapControl sender, Screen screen)
    {
        if (_manager is null) return;

        // nothing to move yet: open a projector on that screen
        if (_selectedWindow is null)
        {
            var isLocked = FeatureManager.IsLocked(API.IG_AddProjector);
            if (isLocked) return;

            _ = await _manager.AddProjectorAsync(screen);
            return;
        }

        await _manager.MoveProjectorAsync(_selectedWindow, screen);
    }


    private async void PART_BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        _ = await Core.API.RunApiAsync(API.IG_AddProjector);
    }


    private async void PART_BtnCloseAll_Click(object? sender, RoutedEventArgs e)
    {
        _ = await Core.API.RunApiAsync(API.IG_CloseAllProjectors);
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
    private void RefreshUI(bool selectNewProjectors)
    {
        if (_manager is null) return;

        _isUpdatingUI = true;
        try
        {
            // 1. the projector list, and which one the map moves
            SyncRows(selectNewProjectors);

            var isSelectionOpen = IsOpen(_selectedWindow);
            if (!isSelectionOpen) _selectedWindow = _manager.Windows.FirstOrDefault();

            var defaultColor = ProjectorWindow.DefaultBackgroundColor;
            foreach (var row in _rows.Values)
            {
                // a window is tiled through a layout, so the mode only shows while it is
                var isTiled = row.Window.Mode == ProjectorWindowMode.Tiled;
                if (row.CmbMode.Items[TILED_MODE_INDEX] is ComboBoxItem tiledItem) tiledItem.IsVisible = isTiled;

                row.BtnSelect.IsChecked = ReferenceEquals(row.Window, _selectedWindow);
                row.CmbMode.SelectedIndex = Array.IndexOf(_windowModes, row.Window.Mode);
                row.ColorPicker.DefaultColor = defaultColor;
                row.ColorPicker.SelectedColor = row.Window.BackgroundColor ?? defaultColor;
            }

            PART_ScreenMap.SelectedProjectorNumber = _selectedWindow?.Number ?? 0;
            UpdateHint();
            SyncLayoutGroups();


            // 2. the actions; Classic still gets the button, which explains the Pro limit
            var isAtProLimit = _manager.Windows.Count >= ProjectorManager.MAX_PRO_PROJECTORS;
            PART_BtnAdd.IsEnabled = !isAtProLimit;
            PART_ProBadge.IsVisible = _manager.IsLimitedByLicense;
            PART_BtnCloseAll.IsEnabled = _manager.Windows.Count > 0;


            // 3. the options; the zoom mode only applies while not syncing
            var isSynced = _manager.Config.EnableViewSync;
            PART_ChkSync.IsChecked = isSynced;
            PART_ZoomModeGroup.IsVisible = !isSynced;
            PART_CmbZoomMode.SelectedIndex = Array.IndexOf(_zoomModes, _manager.Config.ZoomMode);
        }
        finally
        {
            _isUpdatingUI = false;
        }
    }


    /// <summary>
    /// Adds and removes rows so the list matches the open projectors, in number order.
    /// </summary>
    private void SyncRows(bool selectNewProjectors)
    {
        var windows = _manager!.Windows;

        // 1. drop the rows of closed projectors
        var closedWindows = _rows.Keys.Where(w => !windows.Contains(w)).ToArray();
        foreach (var window in closedWindows)
        {
            _ = _rows.Remove(window);
        }

        // 2. add rows for new ones; a new one is selected, so a click on the map places it
        foreach (var window in windows)
        {
            var hasRow = _rows.ContainsKey(window);
            if (hasRow) continue;

            _rows[window] = CreateRow(window);
            if (selectNewProjectors) _selectedWindow = window;
        }

        // 3. lay them out in order
        PART_ProjectorList.Children.Clear();
        foreach (var window in windows)
        {
            PART_ProjectorList.Children.Add(_rows[window].Root);
        }

        PART_ProjectorList.IsVisible = windows.Count > 0;
    }


    /// <summary>
    /// Shows the layouts of each screen two or more projectors share, marking the one they are tiled in.
    /// </summary>
    private void SyncLayoutGroups()
    {
        // 1. the screens to tile, numbered as on the screen map
        var screens = _manager!.GetOrderedScreens();
        var groups = new List<LayoutGroup>();
        for (var i = 0; i < screens.Count; i++)
        {
            var projectorNumbers = _manager.GetWindowsOnScreen(screens[i])
                .Select(w => w.Number)
                .ToArray();

            if (projectorNumbers.Length >= 2) groups.Add(new LayoutGroup(screens[i], i + 1, projectorNumbers));
        }


        // 2. rebuild the groups only when that changes, so a hovered button keeps its state
        var groupsKey = string.Join(';', groups.Select(g => $"{g.ScreenNumber}:{string.Join(',', g.ProjectorNumbers)}:{g.Screen.WorkingArea}"));
        if (groupsKey != _layoutGroupsKey)
        {
            _layoutGroupsKey = groupsKey;
            RebuildLayoutGroups(groups);
        }


        // 3. mark the layout each screen is tiled in
        foreach (var option in _layoutOptions)
        {
            var activeLayout = _manager.GetActiveLayout(option.Screen);
            option.Thumbnail.IsActive = activeLayout == option.Thumbnail.Layout;
        }
    }


    /// <summary>
    /// Creates a group of layout buttons for each screen in <paramref name="groups"/>.
    /// </summary>
    private void RebuildLayoutGroups(IReadOnlyList<LayoutGroup> groups)
    {
        PART_LayoutGroups.Children.Clear();
        _layoutOptions.Clear();

        foreach (var group in groups)
        {
            var label = new PhTextBlock
            {
                LangKey = LangId.Tool_Projector_LblLayout,
                LangParams = group.ScreenNumber,
                FontSize = Const.FONT_SIZE_SMALL,
                Opacity = 0.6,
            };

            // the thumbnails take the shape of the area the projectors are tiled in
            var workArea = group.Screen.WorkingArea;
            var aspectRatio = (double)workArea.Width / workArea.Height;
            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4,
            };

            foreach (var layout in ProjectorLayout.GetLayouts(group.ProjectorNumbers.Length))
            {
                var thumbnail = new LayoutThumbnailControl(layout, group.ProjectorNumbers, aspectRatio);
                var button = new PhToolButton
                {
                    Padding = new Thickness(5),
                    Focusable = false,
                    Content = thumbnail,
                };
                ToolTip.SetTip(button, Core.Lang[LangId.Tool_Projector_LayoutGrid, layout.Rows, layout.Columns]);

                var screen = group.Screen;
                button.Click += async (_, _) =>
                {
                    if (_manager is null) return;
                    await _manager.ArrangeAsync(screen, layout);
                };

                buttons.Children.Add(button);
                _layoutOptions.Add(new LayoutOption(screen, thumbnail));
            }

            PART_LayoutGroups.Children.Add(new StackPanel
            {
                Spacing = 4,
                Children = { label, buttons },
            });
        }

        PART_LayoutGroups.IsVisible = groups.Count > 0;
    }


    /// <summary>
    /// Creates the controls of one projector: select it, change how it covers its screen, close it.
    /// </summary>
    private ProjectorRow CreateRow(ProjectorWindow window)
    {
        var btnSelect = new PhToolButton
        {
            Padding = new Thickness(10, 5),
            Focusable = false,
            VerticalAlignment = VerticalAlignment.Center,
        };
        btnSelect.Click += (_, _) => SelectProjector(window);

        var cmbMode = new ComboBox
        {
            MinWidth = 120,
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
            if (index == TILED_MODE_INDEX) return;

            await _manager.SetWindowModeAsync(window, _windowModes[index]);
        };

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

        var root = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { btnSelect, cmbMode, colorPicker, btnClose },
        };

        var row = new ProjectorRow(window, root, btnSelect, cmbMode, colorPicker, btnClose);
        RelocalizeRow(row);

        return row;
    }


    /// <summary>
    /// Checks whether <paramref name="window"/> is an open projector.
    /// </summary>
    private bool IsOpen(ProjectorWindow? window)
    {
        if (window is null) return false;
        if (_manager is null) return false;

        return _manager.Windows.Contains(window);
    }


    /// <summary>
    /// Makes <paramref name="window"/> the projector a click on the screen map moves.
    /// </summary>
    private void SelectProjector(ProjectorWindow window)
    {
        _selectedWindow = window;
        RefreshUI(selectNewProjectors: false);
    }


    /// <summary>
    /// Tells what a click on the screen map does.
    /// </summary>
    private void UpdateHint()
    {
        if (_selectedWindow is null)
        {
            PART_LblHint.LangKey = LangId.Tool_Projector_HintOpen;
            PART_LblHint.LangParams = null;
            return;
        }

        PART_LblHint.LangKey = LangId.Tool_Projector_HintMove;
        PART_LblHint.LangParams = Core.Lang[LangId.Tool_Projector_WindowTitle, _selectedWindow.Number];
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

        row.BtnSelect.Content = projectorName;
        row.ColorPicker.Title = $"{projectorName} - {colorText}";
        ToolTip.SetTip(row.ColorPicker, colorText);
        ToolTip.SetTip(row.BtnClose, Core.Lang[LangId._Close]);

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



    /// <summary>
    /// The controls of one open projector in the list.
    /// </summary>
    private sealed record ProjectorRow(ProjectorWindow Window, StackPanel Root, PhToolButton BtnSelect, ComboBox CmbMode,
        PhColorPickerControl ColorPicker, PhToolButton BtnClose);


    /// <summary>
    /// A screen two or more projectors share, numbered as on the screen map.
    /// </summary>
    private sealed record LayoutGroup(Screen Screen, int ScreenNumber, int[] ProjectorNumbers);


    /// <summary>
    /// A layout button of a screen, by its thumbnail.
    /// </summary>
    private sealed record LayoutOption(Screen Screen, LayoutThumbnailControl Thumbnail);

}

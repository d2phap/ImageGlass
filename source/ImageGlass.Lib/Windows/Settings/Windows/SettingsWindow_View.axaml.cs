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
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Common.Windows;

public partial class SettingsWindowView : PhControl
{
    private SettingsViewModel _vm = null!;
    private List<SettingsNavItem> _navItems = [];
    private readonly Dictionary<SettingsNavId, SettingsPage> _pages = [];

    // hotkey to active the search box
    private static readonly Hotkey _searchHotkey = new(Hotkey.Ctrl, Key.K);
    private TopLevel? _topLevel;

    // sidebar mouse-click gating: a ListBox commits selection on pointer-press, so we
    // defer the page swap to pointer-release (Tapped) to get click semantics. These two
    // flags distinguish a real click from a press-then-drag-away.
    private bool _sidebarPressing;
    private bool _sidebarTapped;


    /// <summary>
    /// Gets the nav id of the currently shown page.
    /// </summary>
    public SettingsNavId CurrentNavId { get; private set; }



    /// <summary>
    /// Parameterless constructor for the XAML loader / designer.
    /// </summary>
    public SettingsWindowView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the settings view bound to the given working-copy view model.
    /// </summary>
    public SettingsWindowView(SettingsViewModel vm) : this()
    {
        InitSettingsPage(vm);
    }



    #region Override Methods

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // listen on the top level so the search hotkey works no matter what has focus
        _topLevel = TopLevel.GetTopLevel(this);
        _topLevel?.AddHandler(KeyDownEvent, TopLevel_KeyDown, RoutingStrategies.Tunnel);
    }


    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _topLevel?.RemoveHandler(KeyDownEvent, TopLevel_KeyDown);
        _topLevel = null;

        base.OnDetachedFromVisualTree(e);
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        if (PART_Search is not null)
        {
            PART_Search.PlaceholderText = GetSearchPlaceholder();
        }

        if (PART_License is not null)
        {
            PART_License.Text = "✦ " + Core.Lang[Core.IsProEnabled
                ? LangId.Menu_MnuManageLicense
                : LangId.Menu_MnuUpgradeLicense];
        }

        // re-template the sidebar so the localized labels refresh
        if (PART_Sidebar is not null)
        {
            PART_Sidebar.ItemsSource = null;
            PART_Sidebar.ItemsSource = _navItems;
            NavigateTo(CurrentNavId);
        }
    }

    #endregion // Override Methods



    #region Control Events

    private void Sidebar_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // a mouse-driven selection swaps the page on release (Tapped), not here on press;
        // keyboard and programmatic selection changes swap immediately
        if (_sidebarPressing) return;
        if (PART_Sidebar.SelectedItem is SettingsNavItem nav) ShowPage(nav);
    }


    // tunnel handler so the flag is set before the ListBoxItem selects on press
    private void Sidebar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _sidebarPressing = true;
        _sidebarTapped = false;
    }


    private void Sidebar_Tapped(object? sender, TappedEventArgs e)
    {
        _sidebarTapped = true;
        if (PART_Sidebar.SelectedItem is SettingsNavItem nav) ShowPage(nav);
    }


    private void Sidebar_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_sidebarPressing) return;
        _sidebarPressing = false;

        // run after Tapped (if any) has fired; when the press was released away from any
        // item (drag-away, no Tapped), restore the highlight to the page actually shown
        Dispatcher.UIThread.Post(() =>
        {
            if (!_sidebarTapped
                && PART_Sidebar.SelectedItem is SettingsNavItem nav
                && nav.NavId != CurrentNavId)
            {
                NavigateTo(CurrentNavId);
            }
        }, DispatcherPriority.Input);
    }


    private async void License_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _ = await new ManageLicenseWindow().ShowAsync(TopLevel.GetTopLevel(this) as PhWindow);
        }
        catch
        {
            // an async void handler must never reach the unhandled-error dialog
        }
    }


    // Ctrl+K / Cmd+K: focus the search box and select any existing query
    private void TopLevel_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != _searchHotkey.Key || e.KeyModifiers != _searchHotkey.Modifiers) return;

        FocusSearch();
        PART_Search.TextBox.SelectAll();
        e.Handled = true;
    }


    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        UpdateSearchResults();
    }


    private void TxtSearch_GotFocus(object? sender, FocusChangedEventArgs e)
    {
        TryReopenSearchPopup();
    }


    private void TxtSearch_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        TryReopenSearchPopup();
    }

    private void TxtSearch_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!PART_SearchPopup.IsOpen) return;

        var focused = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() as Visual;
        if (focused is not null && focused.GetSelfAndVisualAncestors().Contains(PART_SearchResults))
            return;

        PART_SearchPopup.IsOpen = false;
    }


    /// <summary>
    /// Re-opens the results dropdown for the current query, unless the user is interacting
    /// with the inner clear (X) button (which manages the popup itself by clearing the text).
    /// </summary>
    private void TryReopenSearchPopup()
    {
        if (PART_Search.IsClearButtonPointerOver) return;

        if (!PART_SearchPopup.IsOpen && !string.IsNullOrEmpty(PART_Search.Text))
        {
            UpdateSearchResults();
        }
    }


    /// <summary>
    /// Runs the search for the current query text and shows/hides the results popup.
    /// </summary>
    private void UpdateSearchResults()
    {
        var results = _vm.Registry.Search(PART_Search.Text).Take(25).ToList();
        PART_SearchResults.ItemsSource = results;
        PART_SearchPopup.IsOpen = results.Count > 0;

        // pre-select the first result so Enter works immediately and gives a default highlight
        PART_SearchResults.SelectedIndex = results.Count > 0 ? 0 : -1;
    }


    private void TxtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (!PART_SearchPopup.IsOpen) return;

        switch (e.Key)
        {
            // move the highlighted result while keeping focus in the search box
            case Key.Down:
                MoveSearchSelection(1);
                e.Handled = true;
                break;
            case Key.Up:
                MoveSearchSelection(-1);
                e.Handled = true;
                break;

            // navigate to the highlighted result (the first is pre-selected on each search)
            case Key.Enter:
                if (PART_SearchResults.SelectedItem is SettingItem item)
                {
                    e.Handled = true;
                    JumpToSetting(item);
                }
                break;

            case Key.Escape:
                PART_SearchPopup.IsOpen = false;
                e.Handled = true;
                break;
        }
    }


    /// <summary>
    /// Moves the search-result selection by <paramref name="delta"/>, wrapping around.
    /// </summary>
    private void MoveSearchSelection(int delta)
    {
        var count = PART_SearchResults.ItemCount;
        if (count == 0) return;

        var index = PART_SearchResults.SelectedIndex + delta;
        if (index < 0) index = count - 1;
        else if (index >= count) index = 0;

        PART_SearchResults.SelectedIndex = index;
        PART_SearchResults.ScrollIntoView(index);
    }


    private void SearchResults_Tapped(object? sender, TappedEventArgs e)
    {
        if (PART_SearchResults.SelectedItem is SettingItem item)
        {
            e.Handled = true;
            // defer so the ListBox finishes processing this pointer event before we close
            // the popup / clear its selection (otherwise it throws ArgumentOutOfRangeException)
            Dispatcher.UIThread.Post(() => JumpToSetting(item));
        }
    }

    #endregion // Control Events



    #region Methods

    private void InitSettingsPage(SettingsViewModel vm)
    {
        _vm = vm;
        _navItems = SettingsNavItem.CreateDefaultList();

        // Build every page up front so the search index and navigate-by-config see all
        // settings (not just visited pages). Pages are lightweight.
        foreach (var navItem in _navItems)
        {
            var page = navItem.CreatePage(_vm);
            page.NavLabel = navItem.Label; // for search breadcrumbs ("General > Startup")
            page.EnsureBuilt();
            _pages[navItem.NavId] = page;
        }

        // disable controls the admin has locked, and show the managed-settings warning bar
        _vm.Registry.DisableLockedControls();
        PART_LockWarning.IsVisible = Config.AdminLockedConfigs.Count > 0;

        // search box
        PART_Search.PlaceholderText = GetSearchPlaceholder();
        PART_Search.TextChanged += TxtSearch_TextChanged;
        PART_Search.TextBox.KeyDown += TxtSearch_KeyDown;
        PART_Search.TextBox.GotFocus += TxtSearch_GotFocus;
        PART_Search.TextBox.LostFocus += TxtSearch_LostFocus;
        PART_Search.TextBox.AddHandler(PointerReleasedEvent, TxtSearch_PointerReleased, RoutingStrategies.Tunnel);
        PART_SearchResults.Tapped += SearchResults_Tapped;

        // sidebar
        PART_Sidebar.ItemsSource = _navItems;
        PART_Sidebar.SelectionChanged += Sidebar_SelectionChanged;
        PART_Sidebar.AddHandler(PointerPressedEvent, Sidebar_PointerPressed, RoutingStrategies.Tunnel);
        PART_Sidebar.AddHandler(PointerReleasedEvent, Sidebar_PointerReleased, RoutingStrategies.Tunnel);
        PART_Sidebar.Tapped += Sidebar_Tapped;

        // baseline selection: the first item; a specific page/config is restored
        // afterwards via NavigateToConfig (IG_OpenSettings supplies the target)
        NavigateTo(_navItems[0].NavId);
    }


    #region Navigation

    /// <summary>
    /// Moves keyboard focus to the search box.
    /// </summary>
    public void FocusSearch() => PART_Search?.FocusSearch();


    /// <summary>
    /// Builds the search box placeholder, appending the focus hotkey (e.g. "Search settings… (Ctrl+K)").
    /// </summary>
    private static string GetSearchPlaceholder() => $"{Core.Lang[LangId.Settings_SearchPlaceholder]} ({_searchHotkey.KeyString})";


    /// <summary>
    /// Selects the sidebar item with the given nav id (shows its page).
    /// </summary>
    public void NavigateTo(SettingsNavId navId)
    {
        var item = _navItems.FirstOrDefault(i => i.NavId == navId);
        if (item is null) return;

        PART_Sidebar.SelectedItem = item; // raises SelectionChanged -> ShowPage
    }


    /// <summary>
    /// Navigates to the Plugins page and highlights the plugin with the given id (used by the File
    /// type associations page's plugin-codec link).
    /// </summary>
    public void NavigateToPlugin(string pluginId)
    {
        NavigateTo(SettingsNavId.Plugins);

        if (_pages.TryGetValue(SettingsNavId.Plugins, out var page) && page.Content is PluginsSettingsView view)
        {
            view.FocusPlugin(pluginId);
        }
    }


    /// <summary>
    /// Navigates to the Tools page and opens the add/edit dialog for the given tool id.
    /// </summary>
    public void NavigateToTool(string? toolId)
    {
        if (string.IsNullOrEmpty(toolId)) return;

        NavigateTo(SettingsNavId.Tools);

        if (_pages.TryGetValue(SettingsNavId.Tools, out var page) && page.Content is ToolsSettingsView view)
        {
            // defer so the freshly shown page is attached before opening the child edit dialog
            Dispatcher.UIThread.Post(() => _ = view.EditToolAsync(toolId));
        }
    }


    /// <summary>
    /// Refreshes the File type associations page's codec/plugin rows after a plugin change.
    /// </summary>
    public void NotifyPluginsChanged()
    {
        if (_pages.TryGetValue(SettingsNavId.FileAssociations, out var page)
            && page.Content is FileTypeAssociationsSettingsView view)
        {
            view.RefreshCodecFormats();
        }
    }


    /// <summary>
    /// Navigates to the given target. A registered config id jumps to the setting on its
    /// page; otherwise the value is treated as a page nav id (e.g. the restored last opened
    /// page). No-op when the target matches neither.
    /// </summary>
    public void NavigateToConfig(string? configId)
    {
        // a config id -> jump to the setting on its page
        if (_vm.Registry.FindByConfigId(configId) is { } item)
        {
            JumpToSetting(item);
            return;
        }

        // otherwise a page nav id -> just show the page
        if (Enum.TryParse<SettingsNavId>(configId, true, out var navId)
            && _navItems.Any(i => i.NavId == navId))
        {
            NavigateTo(navId);
        }
    }


    private void ShowPage(SettingsNavItem nav)
    {
        if (!_pages.TryGetValue(nav.NavId, out var page)) return;

        PART_ContentHost.Content = page;
        PART_Title.Text = nav.LabelText;
        CurrentNavId = nav.NavId;

        // remember the last viewed page (persisted to disk on OK/Apply/app exit)
        Core.Config.LastOpenedSetting = nav.NavId.ToString();
    }


    private void JumpToSetting(SettingItem item)
    {
        PART_Search.FocusSearch();

        PART_SearchResults.SelectedItem = null;
        PART_SearchPopup.IsOpen = false;

        NavigateTo(item.PageNavId);
        if (_pages.TryGetValue(item.PageNavId, out var page))
        {
            SettingsPage.ScrollToItem(item);
        }
    }

    #endregion // Navigation


    #endregion // Methods


}

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
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ImageGlass.Common;
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.UI;


public partial class ToolbarControl
{
    // window-mode toggles that live at the top level of the in-app menu; on a macOS menu bar
    // they cannot be bar titles, so they are folded into the Layout menu
    private static readonly LangId[] _windowModeKeys =
    [
        LangId.Menu_MnuWindowFit,
        LangId.Menu_MnuFrameless,
        LangId.Menu_MnuFullScreen,
        LangId.Menu_MnuSlideshow,
    ];

    // maps each native menu item back to its source PhMenuItem so state can be re-synced on open
    private readonly List<(NativeMenuItem Native, PhMenuItem Source)> _nativeMenuPairs = [];


    /// <summary>
    /// Builds the macOS menu-bar menu (the window-level <see cref="NativeMenu"/>) that mirrors the
    /// in-app main menu defined in XAML (<c>PART_MainMenu</c>).
    /// <para>
    /// The native menu is only a <i>view</i> of the existing menu: every leaf routes its click back
    /// to the source <see cref="PhMenuItem"/> so all command logic and external-tool handlers are
    /// reused without duplication. The tree is built once; item state (checked/enabled/text) is
    /// re-synced in place when a submenu opens — never restructured — because AppKit's Help-menu
    /// search indexer opens every submenu and mutating the collection mid-iteration crashes.
    /// </para>
    /// </summary>
    public NativeMenu BuildNativeWindowMenu()
    {
        PrepareSourceMenu();

        var root = new NativeMenu();

        foreach (var src in PART_MainMenu.Items)
        {
            // only submenus can be menu-bar titles; loose top-level items/separators are handled
            // elsewhere (window modes -> Layout; Settings/Exit -> app menu)
            if (src is not PhMenuItem item || item.Items.Count == 0) continue;
            if (!item.IsVisible) continue;

            var topItem = new NativeMenuItem { Header = GetNativeHeader(item) };
            var submenu = new NativeMenu();
            BuildNativeItems(submenu, item.Items);

            // fold the loose window-mode toggles into the Layout menu
            if (item.LangKey == LangId.Menu_MnuLayout)
            {
                submenu.Add(new NativeMenuItemSeparator());
                foreach (var key in _windowModeKeys)
                {
                    var wm = FindSourceItem(key);
                    if (wm is not null && wm.IsVisible) submenu.Add(CreateNativeLeaf(wm));
                }
            }

            topItem.Menu = submenu;
            submenu.Opening += (_, _) => SyncNativeStates();

            _nativeMenuPairs.Add((topItem, item));
            root.Add(topItem);
        }

        return root;
    }


    /// <summary>
    /// Ensures the source menu's bindings (IsChecked, etc.) and dynamic state are current, even
    /// though the in-app ContextMenu popup may never have been opened on this platform.
    /// </summary>
    private void PrepareSourceMenu()
    {
        _nativeMenuPairs.Clear();
        if (DataContext is not ToolbarControlModel vm) return;

        PART_MainMenu.DataContext = vm.ButtonMenuVM;
        RefreshMainMenuState();
        _ = vm.ButtonMenuVM.OnPropertyChanged(string.Empty); // evaluate IsChecked bindings
    }


    /// <summary>
    /// Recursively builds the native items of <paramref name="target"/> from the source items.
    /// </summary>
    private void BuildNativeItems(NativeMenu target, ItemCollection sourceItems)
    {
        foreach (var src in sourceItems)
        {
            if (src is not PhMenuItem item) continue;
            if (!item.IsVisible) continue;

            // separator: the menu uses the "-" header convention
            if (item.Header is "-")
            {
                target.Add(new NativeMenuItemSeparator());
                continue;
            }

            // submenu
            if (item.Items.Count > 0)
            {
                var nativeParent = new NativeMenuItem { Header = GetNativeHeader(item) };
                var submenu = new NativeMenu();
                BuildNativeItems(submenu, item.Items);
                nativeParent.Menu = submenu;
                submenu.Opening += (_, _) => SyncNativeStates();

                _nativeMenuPairs.Add((nativeParent, item));
                target.Add(nativeParent);
            }
            // leaf
            else
            {
                target.Add(CreateNativeLeaf(item));
            }
        }
    }


    /// <summary>
    /// Creates a native leaf item mirroring <paramref name="source"/> and routing its click back to it.
    /// </summary>
    private NativeMenuItem CreateNativeLeaf(PhMenuItem source, KeyGesture? gesture = null)
    {
        var nativeItem = new NativeMenuItem
        {
            Header = GetNativeHeader(source),
            IsEnabled = source.IsEnabled,
            ToggleType = source.ToggleType,
            IsChecked = source.IsChecked,
            Gesture = gesture ?? GetNativeGesture(source),
        };

        var clickTarget = source;
        nativeItem.Click += (_, _) =>
            clickTarget.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

        _nativeMenuPairs.Add((nativeItem, source));
        return nativeItem;
    }


    /// <summary>
    /// Re-syncs scalar state (enabled/checked/text) from the source items onto the existing native
    /// items. Property-only updates — no add/remove — so it is safe to run while AppKit iterates the
    /// menu (e.g. the Help-menu search indexer opens every submenu).
    /// </summary>
    private void SyncNativeStates()
    {
        UpdateMenuItemEnableStates();
        EditingApp.UpdateAppNameForMenuEdit(PART_MnuEdit);
        if (DataContext is ToolbarControlModel vm)
            _ = vm.ButtonMenuVM.OnPropertyChanged(string.Empty); // re-evaluate IsChecked bindings

        foreach (var (native, source) in _nativeMenuPairs)
        {
            native.IsEnabled = source.IsEnabled;
            native.IsChecked = source.IsChecked;
            native.Header = GetNativeHeader(source);
        }
    }


    /// <summary>
    /// Resolves the display text for a native menu item, preferring the source item's already
    /// localized header and falling back to localizing via <see cref="PhMenuItem.LangKey"/>
    /// (the source may not have been localized yet because its popup never opened).
    /// </summary>
    private static string GetNativeHeader(PhMenuItem item)
    {
        if (item.Header is string h && !string.IsNullOrWhiteSpace(h) && h != "-") return h;

        if (item.LangKey is not null)
        {
            var text = Core.Lang[item.LangKey, item.LangParams];
            if (!string.IsNullOrWhiteSpace(text)) return text;
        }

        return item.Header?.ToString() ?? string.Empty;
    }


    /// <summary>
    /// Resolves the keyboard shortcut for a menu item, honoring user-configured hotkeys first then
    /// the built-in action default. macOS routes the key equivalent to the menu, so it fires once.
    /// </summary>
    private static KeyGesture? GetNativeGesture(PhMenuItem item)
    {
        if (item.LangKey is not LangId key) return null;

        var hotkeys = Core.Config.MenuHotkeys.GetValueOrDefault(key);
        if (hotkeys is null || hotkeys.Length == 0)
        {
            hotkeys = AppAPIProvider.GetMenuAction(key)?.Hotkeys;
        }

        var hk = hotkeys?.FirstOrDefault();
        if (hk is null || hk.Key == Key.None) return null;

        return hk.ToGesture();
    }


    /// <summary>
    /// Finds the first source <see cref="PhMenuItem"/> with the given language key (searches submenus).
    /// </summary>
    private PhMenuItem? FindSourceItem(LangId langKey) => FindSourceItem(PART_MainMenu.Items, langKey);

    private static PhMenuItem? FindSourceItem(ItemCollection items, LangId langKey)
    {
        foreach (var it in items)
        {
            if (it is not PhMenuItem item) continue;
            if (item.LangKey == langKey) return item;

            if (item.Items.Count > 0)
            {
                var found = FindSourceItem(item.Items, langKey);
                if (found is not null) return found;
            }
        }

        return null;
    }
}

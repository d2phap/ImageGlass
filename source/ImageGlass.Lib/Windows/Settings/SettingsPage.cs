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
using Avalonia.Threading;
using ImageGlass.Common.Localization;
using ImageGlass.UI;
using System;

namespace ImageGlass.Common.Windows;

/// <summary>
/// A single settings page (tab) in the navigation.
/// </summary>
public sealed class SettingsPage : PhControl
{
    private bool _isBuilt;
    private readonly SettingsViewModel _vm;
    private readonly Func<SettingsViewModel, SettingsNavId, LangId?, Control> _createView;


    /// <summary>
    /// Gets the unique nav id of this page (matches the sidebar item / <see cref="Config.LastOpenedSetting"/>).
    /// </summary>
    public SettingsNavId NavId { get; }

    /// <summary>
    /// Gets, sets the localization key of this page's sidebar label (used for search breadcrumbs).
    /// Assigned by the host before <see cref="EnsureBuilt"/>.
    /// </summary>
    public LangId? NavLabel { get; set; }


    /// <param name="createView">
    /// Factory that builds the page content from the view model, nav id and (resolved) nav label.
    /// </param>
    public SettingsPage(SettingsViewModel vm, SettingsNavId navId,
        Func<SettingsViewModel, SettingsNavId, LangId?, Control> createView)
    {
        _vm = vm;
        NavId = navId;
        _createView = createView;
    }


    /// <summary>
    /// Builds the page content once; the view registers its setting items into the shared
    /// <see cref="SettingsRegistry"/> as it builds.
    /// </summary>
    public void EnsureBuilt()
    {
        if (_isBuilt) return;
        _isBuilt = true;

        Content = _createView(_vm, NavId, NavLabel);
    }


    /// <summary>
    /// Scrolls the given setting into view and focuses it (themed focus ring) so the user
    /// can spot where the search/config navigation landed.
    /// </summary>
    public static void ScrollToItem(SettingItem item)
    {
        var target = item.Target;
        if (target is null) return;

        // defer until the freshly shown page has completed a layout pass
        Dispatcher.UIThread.Post(() =>
        {
            target.BringIntoView();
            target.Focus(NavigationMethod.Tab); // shows the themed focus ring
        }, DispatcherPriority.Loaded);
    }
}

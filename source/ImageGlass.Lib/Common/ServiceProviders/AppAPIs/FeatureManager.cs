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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Collections.Frozen;
using System.Threading;

namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// Manages the locked features.
/// </summary>
internal static class FeatureManager
{
    private static FrozenSet<string> _locked = FrozenSet<string>.Empty;
    private static FrozenSet<LangId> _proGated = FrozenSet<LangId>.Empty;
    private static readonly Lock _lock = new();

    // Menu features that require Pro; gated (badged) until a valid license is active.
    private static readonly FrozenSet<LangId> _proFeatureKeys =
        FrozenSet.ToFrozenSet(new[] { LangId.Menu_MnuHdrToneMapper });

    // Gated features Classic can still open as a read-only preview.
    private static readonly FrozenSet<LangId> _proPreviewKeys =
        FrozenSet.ToFrozenSet(new[] { LangId.Menu_MnuHdrToneMapper });


    /// <summary>
    /// Rebuilds the lock and Pro-gate snapshots from the current license + config.
    /// </summary>
    public static void Refresh()
    {
        // admin-only, already Pro-gated at capture
        var newLocked = Config.LockedFeatures;

        // consumer Pro features stay gated until a license is active
        var newProGated = Core.IsProEnabled ? FrozenSet<LangId>.Empty : _proFeatureKeys;

        lock (_lock)
        {
            _locked = newLocked;
            _proGated = newProGated;
        }
    }


    /// <summary>
    /// Checks if a menu key is a Pro feature that is not yet unlocked.
    /// </summary>
    public static bool IsProGated(LangId? langKey)
        => langKey is LangId key && _proGated.Contains(key);


    /// <summary>
    /// Checks if a menu key is a Pro feature Classic must not run at all: gated and not previewable.
    /// </summary>
    public static bool IsProBlocked(LangId? langKey)
        => IsProGated(langKey) && !_proPreviewKeys.Contains(langKey!.Value);


    /// <summary>
    /// Checks if an API is locked.
    /// </summary>
    public static bool IsLocked(API api) => _locked.Contains(api.ToString("G"));


    /// <summary>
    /// Checks if an API name is locked.
    /// </summary>
    public static bool IsLocked(string? apiName) => !string.IsNullOrEmpty(apiName) && _locked.Contains(apiName);


    /// <summary>
    /// Whether interactive zoom (mouse-wheel / touch / touchpad) is locked because a zoom API is
    /// locked. Conservative: any zoom direction being locked disables interactive zoom entirely.
    /// </summary>
    public static bool IsZoomLocked() => IsLocked(API.IG_ZoomIn) || IsLocked(API.IG_ZoomOut);


    /// <summary>
    /// Whether interactive pan (mouse-wheel / touch / touchpad) is locked because a pan API is
    /// locked. Conservative: any pan direction being locked disables interactive pan entirely.
    /// </summary>
    public static bool IsPanLocked() => IsLocked(API.IG_PanLeft) || IsLocked(API.IG_PanRight)
        || IsLocked(API.IG_PanUp) || IsLocked(API.IG_PanDown);


    /// <summary>
    /// Checks if a menu item with the given language key is locked.
    /// </summary>
    public static bool IsLocked(LangId? langKey)
    {
        var action = AppAPIProvider.GetMenuAction(langKey);
        return IsLocked(action?.Executable);
    }


    /// <summary>
    /// Hides locked menu items from the given items control.
    /// </summary>
    public static void HideLockedMenuItems(ItemCollection items)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i] is not PhMenuItem mnu) continue;

            // Check if this menu item is locked via LangKey
            if (IsLocked(mnu.LangKey))
            {
                items.RemoveAt(i);
                continue;
            }

            // Recursively process submenus
            if (mnu.Items.Count > 0)
            {
                HideLockedMenuItems(mnu.Items);

                // Hide parent if all children were removed
                if (mnu.Items.Count == 0)
                {
                    items.RemoveAt(i);
                }
            }
        }

        // Clean up orphaned separators
        CleanupSeparators(items);
    }


    /// <summary>
    /// Removes orphaned separators from menu items.
    /// </summary>
    private static void CleanupSeparators(ItemCollection items)
    {
        // Remove leading separators
        while (items.Count > 0 && items[0] is PhMenuItem { Header: "-" })
        {
            items.RemoveAt(0);
        }

        // Remove trailing separators
        while (items.Count > 0 && items[^1] is PhMenuItem { Header: "-" })
        {
            items.RemoveAt(items.Count - 1);
        }

        // Remove duplicate separators
        for (int i = items.Count - 2; i >= 0; i--)
        {
            if (items[i] is PhMenuItem { Header: "-" } &&
                items[i + 1] is PhMenuItem { Header: "-" })
            {
                items.RemoveAt(i + 1);
            }
        }
    }
}

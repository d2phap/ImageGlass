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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Common.Windows;

/// <summary>
/// A registry of all <see cref="SettingItem"/>s across every settings page.
/// Used to power the search box and navigate-by-<see cref="ConfigId"/>.
/// </summary>
public sealed class SettingsRegistry
{
    private readonly List<SettingItem> _items = [];


    /// <summary>
    /// Gets all registered setting items.
    /// </summary>
    public IReadOnlyList<SettingItem> Items => _items;


    /// <summary>
    /// Registers a setting item.
    /// </summary>
    public void Register(SettingItem item) => _items.Add(item);


    /// <summary>
    /// Clears all registered items.
    /// </summary>
    public void Clear() => _items.Clear();


    /// <summary>
    /// Disables every registered control whose config id is admin-locked, so managed settings
    /// cannot be edited. Controls sharing a locked id are all disabled.
    /// </summary>
    public void DisableLockedControls()
    {
        foreach (var item in _items)
        {
            if (item.Id is { } id && item.Target is { } target && Config.IsConfigLocked(id))
            {
                target.IsEnabled = false;
            }
        }
    }


    /// <summary>
    /// Finds the first setting item matching the given config id string.
    /// Returns <c>null</c> if the id is invalid or not registered.
    /// </summary>
    public SettingItem? FindByConfigId(string? configId)
    {
        if (string.IsNullOrWhiteSpace(configId)) return null;
        if (!Enum.TryParse<ConfigId>(configId, out var id)) return null;

        return _items.FirstOrDefault(i => i.Id == id);
    }


    /// <summary>
    /// Returns the setting items whose localized label or config id contains the query.
    /// Empty query yields no results.
    /// </summary>
    public IEnumerable<SettingItem> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) yield break;
        var q = query.Trim();

        foreach (var item in _items)
        {
            var matchLabel = item.LabelText.Contains(q, StringComparison.OrdinalIgnoreCase);
            var matchId = item.Id is { } id
                && id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase);

            if (matchLabel || matchId) yield return item;
        }
    }
}

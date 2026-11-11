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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.Common.Windows;

/// <summary>
/// One row in the Keyboard settings table.
/// </summary>
public sealed class MenuHotkeyRowModel : PhReactive
{

    #region // Public Properties

    /// <summary>
    /// Gets the language key of the menu action this row represents.
    /// </summary>
    public LangId MenuKey { get; }

    /// <summary>
    /// Gets, sets the hierarchical display path of the action (e.g. <c>File / Open…</c>);
    /// re-localized in place on language change.
    /// </summary>
    public string ActionPath
    {
        get; set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    /// Gets the built-in default hotkeys.
    /// </summary>
    public Hotkey[] DefaultHotkeys { get; }


    /// <summary>
    /// Gets, sets the currently effective hotkeys for this action.
    /// </summary>
    public Hotkey[] Hotkeys
    {
        get; set
        {
            field = value ?? [];
            OnPropertyChanged();
            OnPropertyChanged(nameof(HotkeysText));
            OnPropertyChanged(nameof(HotkeysTooltip));
            OnPropertyChanged(nameof(IsChanged));
            OnPropertyChanged(nameof(IsDefault));
            OnPropertyChanged(nameof(HotkeysShowAccent));
            OnPropertyChanged(nameof(HotkeysShowNormal));
        }
    } = [];


    /// <summary>
    /// Gets the display text of the effective hotkeys (comma-separated).
    /// </summary>
    public string HotkeysText => string.Join(", ", Hotkeys.Select(h => h.KeyString));

    /// <summary>
    /// Gets the display text of the default hotkeys (comma-separated).
    /// </summary>
    public string DefaultHotkeysText => string.Join(", ", DefaultHotkeys.Select(h => h.KeyString));

    /// <summary>
    /// Gets whether the effective hotkeys differ from the defaults.
    /// </summary>
    public bool IsChanged => !HotkeysSetEqual(Hotkeys, DefaultHotkeys);

    /// <summary>
    /// Gets whether the effective hotkeys match the defaults.
    /// </summary>
    public bool IsDefault => !IsChanged;

    /// <summary>
    /// Gets, sets whether this row shares a hotkey with another action (computed by the editor).
    /// </summary>
    public bool IsConflict
    {
        get; set
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HotkeysTooltip));
            OnPropertyChanged(nameof(HotkeysShowDanger));
            OnPropertyChanged(nameof(HotkeysShowAccent));
            OnPropertyChanged(nameof(HotkeysShowNormal));
        }
    }


    // hotkey-cell color states (mutually exclusive): conflict (danger) wins over customized (accent)
    public bool HotkeysShowDanger => IsConflict;
    public bool HotkeysShowAccent => IsChanged && !IsConflict;
    public bool HotkeysShowNormal => !IsChanged && !IsConflict;

    /// <summary>
    /// Gets the localized tooltip for the row's edit button.
    /// </summary>
    public string EditTooltip => Core.Lang[LangId._Edit];

    /// <summary>
    /// Gets the tooltip shown on the hotkeys cell: the conflict warning when the hotkey is duplicated,
    /// otherwise the full hotkey text (useful when the cell is truncated), or <c>null</c> when empty.
    /// </summary>
    public string? HotkeysTooltip => IsConflict
        ? Core.Lang[LangId.Settings_Keyboard_Conflict]
        : HotkeysText.Length > 0 ? HotkeysText : null;

    #endregion // Public Properties


    public MenuHotkeyRowModel(LangId menuKey, string actionPath, Hotkey[] defaultHotkeys, Hotkey[] hotkeys)
    {
        MenuKey = menuKey;
        ActionPath = actionPath;
        DefaultHotkeys = defaultHotkeys ?? [];
        Hotkeys = hotkeys ?? [];
    }


    /// <summary>
    /// Compares two hotkey lists as unordered sets of their display strings.
    /// </summary>
    public static bool HotkeysSetEqual(IReadOnlyList<Hotkey> a, IReadOnlyList<Hotkey> b)
    {
        if (a.Count != b.Count) return false;

        var sa = a.Select(h => h.KeyString).OrderBy(s => s, StringComparer.Ordinal);
        var sb = b.Select(h => h.KeyString).OrderBy(s => s, StringComparer.Ordinal);
        return sa.SequenceEqual(sb);
    }

}

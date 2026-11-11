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
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.UI;

/// <summary>
/// Records and lists keyboard shortcuts.
/// </summary>
public class PhHotkeyPicker : PhControl
{
    private readonly List<Hotkey> _hotkeys = [];
    private readonly WrapPanel _chips;
    private readonly PhTextBox _recorder;


    /// <summary>
    /// Gets a copy of the recorded hotkeys, or replaces the whole set.
    /// </summary>
    public IReadOnlyList<Hotkey> Hotkeys
    {
        get => [.. _hotkeys];
        set
        {
            _hotkeys.Clear();
            if (value is not null) _hotkeys.AddRange(value);
            RenderChips();
        }
    }


    /// <summary>
    /// Gets, sets the placeholder shown in the recorder box.
    /// </summary>
    public string? PlaceholderText
    {
        get => _recorder.PlaceholderText;
        set => _recorder.PlaceholderText = value;
    }


    public PhHotkeyPicker()
    {
        _recorder = new PhTextBox
        {
            IsReadOnly = true,
            ValidateByPressingEnter = false,
        };

        // tunnel + handledEventsToo so we capture the chord before the (read-only) TextBox does
        _recorder.AddHandler(KeyDownEvent, OnRecorderKeyDown,
            RoutingStrategies.Tunnel, handledEventsToo: true);

        _chips = new WrapPanel
        {
            IsVisible = false,
            Margin = new Thickness(0, 8, 0, 0),
        };

        Content = new StackPanel
        {
            Children = { _recorder, _chips },
        };
    }


    #region Recording

    /// <summary>
    /// Records a shortcut from a key press: ignores lone modifiers (and leaves Tab/Escape/Enter for
    /// normal navigation/close/submit), otherwise adds the chord.
    /// </summary>
    private void OnRecorderKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Tab or Key.Escape or Key.Enter) return;

        if (IsModifierKey(e.Key)) { e.Handled = true; return; }

        AddHotkey(new Hotkey(e.KeyModifiers, e.Key));
        e.Handled = true;
    }


    private static bool IsModifierKey(Key key) => key
        is Key.LeftCtrl or Key.RightCtrl
        or Key.LeftShift or Key.RightShift
        or Key.LeftAlt or Key.RightAlt
        or Key.LWin or Key.RWin;


    private void AddHotkey(Hotkey hk)
    {
        if (hk.Key == Key.None) return;
        if (_hotkeys.Any(h => h.Key == hk.Key && h.Modifiers == hk.Modifiers)) return;

        _hotkeys.Add(hk);
        RenderChips();
    }

    #endregion // Recording


    #region Chips

    private void RenderChips()
    {
        _chips.Children.Clear();
        foreach (var hk in _hotkeys)
        {
            _chips.Children.Add(BuildChip(hk));
        }
        _chips.IsVisible = _hotkeys.Count > 0;
    }


    private PhHotkeyChip BuildChip(Hotkey hk)
    {
        var chip = new PhHotkeyChip(hk.KeyString, showDelete: true)
        {
            Margin = new Thickness(0, 0, 6, 6),
        };
        chip.Deleted += (_, _) =>
        {
            _hotkeys.Remove(hk);
            RenderChips();
        };
        return chip;
    }

    #endregion // Chips

}

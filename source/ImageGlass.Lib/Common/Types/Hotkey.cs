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
using Avalonia.Input;
using ImageGlass.Common.Types.JsonTypeConverters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Types;


[JsonConverter(typeof(JsonStringToHotkeyConverter))]
public class Hotkey
{
    /// <summary>
    /// The default modifier key for "Control" action, which is "Control" key on Windows and "Command (Meta)" key on macOS.
    /// </summary>
    public static readonly KeyModifiers Ctrl = BHelper.OS == OSType.Mac ? KeyModifiers.Meta : KeyModifiers.Control;

    /// <summary>
    /// The default hotkey for "Delete" action, which is "Delete" key on Windows and "Backspace" key on macOS.
    /// </summary>
    public static readonly Key Delete = BHelper.OS == OSType.Mac ? Key.Back : Key.Delete;


    /// <summary>
    /// Gets, sets the virtual key.
    /// </summary>
    public Key Key { get; set; } = Key.None;

    /// <summary>
    /// Gets, sets the key modifiers.
    /// </summary>
    public KeyModifiers Modifiers { get; set; } = KeyModifiers.None;
    public bool Control => Modifiers.HasFlag(KeyModifiers.Control);
    public bool Shift => Modifiers.HasFlag(KeyModifiers.Shift);
    public bool Alt => Modifiers.HasFlag(KeyModifiers.Alt);

    /// <summary>
    /// Gets the hotkey text formatted for the current platform, for display only.
    /// </summary>
    public string KeyString => ToString(this);

    /// <summary>
    /// Gets the platform-independent hotkey text used to persist the hotkey.
    /// </summary>
    public string InvariantKeyString => ToInvariantString(this);



    public Hotkey() { }


    public Hotkey(Key key)
    {
        Key = key;
    }


    public Hotkey(KeyModifiers modifiers, Key key)
    {
        Modifiers = modifiers;
        Key = key;
    }


    public Hotkey(KeyGesture kg)
    {
        Key = kg.Key;
        Modifiers = kg.KeyModifiers;
    }


    public Hotkey(KeyEventArgs e)
    {
        Key = e.Key;
        Modifiers = e.KeyModifiers;
    }


    #region Key name tables

    // key names written by ToInvariantString(); any key missing here uses its enum name.
    // The numpad operators are deliberately absent, their symbols collide with the OEM keys.
    private static readonly Dictionary<Key, string> _invariantKeyNames = new()
    {
        [Key.D0] = "0",
        [Key.D1] = "1",
        [Key.D2] = "2",
        [Key.D3] = "3",
        [Key.D4] = "4",
        [Key.D5] = "5",
        [Key.D6] = "6",
        [Key.D7] = "7",
        [Key.D8] = "8",
        [Key.D9] = "9",
        [Key.Back] = "Backspace",
        [Key.OemPlus] = "+",
        [Key.OemMinus] = "-",
        [Key.OemComma] = ",",
        [Key.OemPeriod] = ".",
        [Key.OemQuestion] = "/",
        [Key.OemSemicolon] = ";",
        [Key.OemQuotes] = "'",
        [Key.OemOpenBrackets] = "[",
        [Key.OemCloseBrackets] = "]",
        [Key.OemPipe] = "|",
        [Key.OemBackslash] = "\\",
        [Key.OemTilde] = "`",
    };

    // every spelling ParseFrom() accepts on top of the enum names: the invariant names above,
    // the platform display names (incl. the macOS keycaps), and hand-written aliases
    private static readonly Dictionary<string, Key> _keyAliases = BuildKeyAliases();

    private static readonly Dictionary<string, KeyModifiers> _modifierAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Ctrl"] = KeyModifiers.Control,
        ["Control"] = KeyModifiers.Control,
        ["⌃"] = KeyModifiers.Control,
        ["Shift"] = KeyModifiers.Shift,
        ["⇧"] = KeyModifiers.Shift,
        ["Alt"] = KeyModifiers.Alt,
        ["Option"] = KeyModifiers.Alt,
        ["⌥"] = KeyModifiers.Alt,
        ["Cmd"] = KeyModifiers.Meta,
        ["Command"] = KeyModifiers.Meta,
        ["Meta"] = KeyModifiers.Meta,
        ["Win"] = KeyModifiers.Meta,
        ["Super"] = KeyModifiers.Meta,
        ["⌘"] = KeyModifiers.Meta,
    };


    private static Dictionary<string, Key> BuildKeyAliases()
    {
        var aliases = new Dictionary<string, Key>(StringComparer.OrdinalIgnoreCase)
        {
            ["\""] = Key.OemQuotes,
            ["*"] = Key.Multiply,
            ["Esc"] = Key.Escape,
            ["Del"] = Key.Delete,
            ["Ins"] = Key.Insert,
            ["PgUp"] = Key.PageUp,
            ["PgDn"] = Key.PageDown,
            ["Up Arrow"] = Key.Up,
            ["Down Arrow"] = Key.Down,
            ["Left Arrow"] = Key.Left,
            ["Right Arrow"] = Key.Right,

            // macOS keycaps, which its display text uses and its older configs stored
            ["←"] = Key.Left,
            ["↑"] = Key.Up,
            ["→"] = Key.Right,
            ["↓"] = Key.Down,
            ["↖"] = Key.Home,
            ["↘"] = Key.End,
            ["↩"] = Key.Return,
            ["⇞"] = Key.PageUp,
            ["⇟"] = Key.PageDown,
            ["⇥"] = Key.Tab,
            ["⌫"] = Key.Back,
            ["⎋"] = Key.Escape,
            ["␣"] = Key.Space,
        };

        foreach (var (key, name) in _invariantKeyNames)
        {
            aliases[name] = key;
        }

        return aliases;
    }

    #endregion // Key name tables


    #region Methods

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString() => KeyString;


    /// <summary>
    /// Checks if the provided hotkey is same.
    /// </summary>
    public bool IsSame(Hotkey? hk)
    {
        if (hk is null) return false;

        return IsSame(hk.Key, hk.Modifiers);
    }


    /// <summary>
    /// Checks if the provided hotkey is same.
    /// </summary>
    public bool IsSame(Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        if (Key != key) return false;
        if (Control && !modifiers.HasFlag(KeyModifiers.Control)) return false;
        if (Shift && !modifiers.HasFlag(KeyModifiers.Shift)) return false;
        if (Alt && !modifiers.HasFlag(KeyModifiers.Alt)) return false;

        return true;
    }


    /// <summary>
    /// Converts to <see cref="KeyGesture"/>.
    /// </summary>
    public KeyGesture ToGesture()
    {
        return new KeyGesture(Key, Modifiers);
    }


    /// <summary>
    /// Parses a hotkey text such as <c>Ctrl+Shift+1</c>, accepting both the invariant and the
    /// platform display names. Returns <c>null</c> for a text that is not a usable hotkey.
    /// </summary>
    public static Hotkey? ParseFrom(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        var modifiers = KeyModifiers.None;
        var key = Key.None;
        var partStart = 0;

        // '+' separates the parts, except where it is the part itself (e.g. "Ctrl++")
        for (var i = 0; i <= s.Length; i++)
        {
            var isLastPart = i == s.Length;
            if (!isLastPart && (s[i] != '+' || i == partStart)) continue;

            var part = s[partStart..i].Trim();
            partStart = i + 1;

            // the trailing part is the key, everything before it is a modifier
            if (isLastPart)
            {
                if (!TryParseKey(part, out key)) return null;
            }
            else
            {
                if (!_modifierAliases.TryGetValue(part, out var modifier)) return null;
                modifiers |= modifier;
            }
        }

        return new Hotkey(modifiers, key);
    }


    /// <summary>
    /// Parse <see cref="Hotkey"/> to the platform display string.
    /// </summary>
    public static string ToString(Hotkey hotkey)
    {
        var kg = new KeyGesture(hotkey.Key, hotkey.Modifiers);
        return kg.ToString("p", null);
    }


    /// <summary>
    /// Parse <see cref="Hotkey"/> to the platform-independent string used to persist it;
    /// <see cref="ParseFrom(string?)"/> reads back exactly what this writes.
    /// </summary>
    public static string ToInvariantString(Hotkey hotkey)
    {
        var parts = new List<string>(5);
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (hotkey.Modifiers.HasFlag(KeyModifiers.Meta)) parts.Add("Cmd");

        parts.Add(_invariantKeyNames.GetValueOrDefault(hotkey.Key) ?? hotkey.Key.ToString());

        return string.Join('+', parts);
    }


    /// <summary>
    /// Resolves a single key name; an unknown name, or one that resolves to
    /// <see cref="Key.None"/>, is rejected.
    /// </summary>
    private static bool TryParseKey(string name, out Key key)
    {
        key = Key.None;
        if (string.IsNullOrEmpty(name)) return false;

        if (_keyAliases.TryGetValue(name, out key)) return true;

        // Enum.TryParse() also reads the numeric value, which would silently turn a typed digit
        // ("1") into an unrelated key (Key.Cancel), so only names get through
        if (!char.IsAsciiLetter(name[0])) return false;

        return Enum.TryParse(name, true, out key) && key != Key.None;
    }


    #endregion // Methods


}

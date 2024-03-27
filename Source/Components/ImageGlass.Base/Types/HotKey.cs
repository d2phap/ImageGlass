/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.Base.WinApi;

namespace ImageGlass.Base;

public class Hotkey
{
    public bool Control { get; set; } = false;
    public bool Shift { get; set; } = false;
    public bool Alt { get; set; } = false;
    public Keys KeyData { get; set; } = Keys.None;
    public Keys Modifiers { get; set; } = Keys.None;
    public Keys KeyCode { get; set; } = Keys.None;
    public int KeyValue { get; set; } = -1;
    public string KeyStr
    {
        get
        {
            var str = KeyCode == Keys.None ? string.Empty : KeyCode.ToString();

            if (str.Equals(nameof(Keys.Next)))
            {
                str = "PageDown";
            }
            else if (str.StartsWith("Oem")
                || (str.StartsWith('D') && str.Length == 2) // D0 -> D9
                )
            {
                var unicode = KeyboardApi.KeyCodeToChar(KeyCode, false).ToString();

                if (!string.IsNullOrEmpty(unicode))
                {
                    str = unicode;
                }
            }

            return str;
        }
    }


    public Hotkey()
    {
        ParseFrom(Keys.None);
    }

    public Hotkey(string s)
    {
        ParseFrom(s);
    }

    public Hotkey(Keys keys)
    {
        ParseFrom(keys);
    }


    /// <summary>
    /// Parses hotkey from string
    /// </summary>
    /// <param name="s"></param>
    public void ParseFrom(string s)
    {
        var hotkey = Keys.None;

        try
        {

            var keyStrings = s.ToLowerInvariant().Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var str in keyStrings)
            {
                if (str.Equals("ctrl"))
                {
                    hotkey |= Keys.Control;
                }
                else if (str.Equals("shift"))
                {
                    hotkey |= Keys.Shift;
                }
                else if (str.Equals("alt"))
                {
                    hotkey |= Keys.Alt;
                }
                else
                {
                    if (str.Length == 1)
                    {
                        hotkey |= KeyboardApi.CharToKeyCode(str[0]);
                    }
                    else
                    {
                        Keys? key = null;
                        try
                        {
                            var kc = new KeysConverter();
                            key = (Keys?)kc.ConvertFromInvariantString(str);
                        }
                        catch (ArgumentException)
                        {
                            key = BHelper.ParseEnum<Keys>(str);
                        }

                        if (key is not null)
                        {
                            hotkey |= key.Value;
                        }
                    }
                }
            }
        }
        catch { }


        if (hotkey != Keys.None)
        {
            ParseFrom(hotkey);
        }
    }


    /// <summary>
    /// Parses hotkey from keys
    /// </summary>
    public void ParseFrom(Keys keys)
    {
        var ka = new KeyEventArgs(keys);

        Control = ka.Control;
        Shift = ka.Shift;
        Alt = ka.Alt;
        Modifiers = ka.Modifiers;
        KeyCode = ka.KeyCode;
        KeyData = ka.KeyData;
        KeyValue = ka.KeyValue;
    }


    /// <summary>
    /// Converts hotkey to string
    /// </summary>
    public override string ToString()
    {
        var modifiers = new List<string>(4);

        if (Control) modifiers.Add("Ctrl");
        if (Shift) modifiers.Add("Shift");
        if (Alt) modifiers.Add("Alt");


        var ignoredKeys = new List<string>() {
            nameof(Keys.ControlKey),
            nameof(Keys.ShiftKey),
            nameof(Keys.Menu),
            nameof(Keys.LWin),
            nameof(Keys.RWin),
            nameof(Keys.Capital),
        };


        if (KeyStr.Length == 1)
        {
            modifiers.Add(KeyStr.ToUpperInvariant());
        }
        else if (!ignoredKeys.Contains(KeyStr))
        {
            modifiers.Add(KeyStr);
        }


        return string.Join('+', modifiers); // do not use ZString here
    }

}

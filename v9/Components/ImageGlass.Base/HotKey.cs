/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
namespace ImageGlass.Base;

public class HotKey
{
    public bool Control { get; set; } = false;
    public bool Shift { get; set; } = false;
    public bool Alt { get; set; } = false;
    public Keys Modifiers { get; set; } = Keys.None;
    public Keys KeyCode { get; set; } = Keys.None;
    public Keys KeyData { get; set; } = Keys.None;
    public int KeyValue { get; set; } = -1;


    public HotKey()
    {
        ParseFrom(Keys.None);
    }

    public HotKey(string s)
    {
        ParseFrom(s);
    }

    public HotKey(Keys keys)
    {
        ParseFrom(keys);
    }


    /// <summary>
    /// Parses hotkey from string
    /// </summary>
    /// <param name="s"></param>
    public void ParseFrom(string s)
    {
        var kc = new KeysConverter();

        try
        {
            var keys = (Keys?)kc.ConvertFromString(s);

            if (keys != null)
            {
                ParseFrom(keys.Value);
            }
        }
        catch { }
    }


    /// <summary>
    /// Parses hotkey from keys
    /// </summary>
    /// <param name="keys"></param>
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
    /// <returns></returns>
    public override string ToString()
    {
        var str = string.Empty;

        try
        {
            var kc = new KeysConverter();
            str = kc.ConvertToInvariantString(KeyData) ?? "";
        }
        catch { }

        return str;
    }
}

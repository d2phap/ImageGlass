/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using System.Text;

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
            var str = KeyCode.ToString();

            if (str.StartsWith("Oem")
                || (str.StartsWith("D") && str.Length == 2) // D0 -> D9
                )
            {
                var unicode = KeyboardApi.KeyCodeToUnicode(KeyData);

                if (!string.IsNullOrEmpty(unicode))
                {
                    str = unicode;
                }
            }

            return str;
        }
    }

    public static Dictionary<string, Keys> CharToKeyMapping => new()
    {
        { "`", Keys.Oemtilde },
        { "-", Keys.OemMinus },
        { "=", Keys.Oemplus },
        { "[", Keys.OemOpenBrackets },
        { "]", Keys.OemCloseBrackets },
        { "\\", Keys.Oem5 },
        { ";", Keys.Oem1 },
        { "'", Keys.Oem7 },
        { ",", Keys.Oemcomma },
        { ".", Keys.OemPeriod },
        { "/", Keys.OemQuestion },

        { "0", Keys.D0 },
        { "1", Keys.D1 },
        { "2", Keys.D2 },
        { "3", Keys.D3 },
        { "4", Keys.D4 },
        { "5", Keys.D5 },
        { "6", Keys.D6 },
        { "7", Keys.D7 },
        { "8", Keys.D8 },
        { "9", Keys.D9 },
    };


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

        var chars = s.Split("+", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var c in chars)
        {
            if (c.Equals("ctrl", StringComparison.OrdinalIgnoreCase))
            {
                hotkey |= Keys.Control;
            }
            else if (c.Equals("shift", StringComparison.OrdinalIgnoreCase))
            {
                hotkey |= Keys.Shift;
            }
            else if (c.Equals("alt", StringComparison.OrdinalIgnoreCase))
            {
                hotkey |= Keys.Alt;
            }
            else
            {
                if (CharToKeyMapping.ContainsKey(c))
                {
                    hotkey |= CharToKeyMapping[c];
                }
                else
                {
                    var kc = new KeysConverter();
                    var key = (Keys?)kc.ConvertFromString(c);

                    if (key is not null)
                    {
                        hotkey |= key.Value;
                    }
                }
            }
        }


        if (hotkey != Keys.None)
        {
            ParseFrom(hotkey);
        }
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
        var strB = new StringBuilder();

        if (Control)
        {
            strB.Append("Ctrl+");
        }

        if (Shift)
        {
            strB.Append("Shift+");
        }

        if (Alt)
        {
            strB.Append("Alt+");
        }

        if (KeyStr.Length == 1)
        {
            strB.Append(KeyStr.ToUpperInvariant());
        }
        else
        {
            strB.Append(KeyStr);
        }


        return strB.ToString();
    }
}

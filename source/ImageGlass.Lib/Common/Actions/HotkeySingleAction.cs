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
using ImageGlass.Common.Localization;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System.Text.Json.Serialization;


namespace ImageGlass.Common.Actions;


[JsonSerializable(typeof(HotkeySingleAction))]
public partial class HotkeySingleActionJsonContext : JsonSerializerContext { }



public partial class HotkeySingleAction : SingleAction
{
    /// <summary>
    /// Gets, sets hotkeys.
    /// </summary>
    public Hotkey[] Hotkeys { get; set; } = [];


    public HotkeySingleAction() : base(null) { }


    public HotkeySingleAction(LangId langKey, API api, KeyModifiers modifiers, Key key) : base(api)
    {
        LangKey = Lang.KeysMap[langKey];
        Hotkeys = [new Hotkey(modifiers, key)];
    }


    public HotkeySingleAction(LangId langKey, API api, Key key) : base(api)
    {
        LangKey = Lang.KeysMap[langKey];
        Hotkeys = [new Hotkey(key)];
    }


    public HotkeySingleAction(LangId langKey, API api, Hotkey[]? hotkeys = null) : base(api)
    {
        LangKey = Lang.KeysMap[langKey];
        if (hotkeys is not null) Hotkeys = hotkeys;
    }


    public HotkeySingleAction(LangId langKey, API api, string? argument, Hotkey[]? hotkeys = null)
        : base(api, argument)
    {
        LangKey = Lang.KeysMap[langKey];
        if (hotkeys is not null) Hotkeys = hotkeys;
    }


}


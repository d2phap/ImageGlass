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
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Actions;


[JsonSerializable(typeof(SingleAction))]
public partial class SingleActionJsonContext : JsonSerializerContext { }


public partial class SingleAction : PhReactive
{
    /// <summary>
    /// Executable action, its value can be:
    /// <list type="bullet">
    ///   <item>
    ///   Name of a menu item in Main Menu. For example: <c>MnuPrint</c>
    ///   </item>
    ///   <item>
    ///   Name of an <c>IG_</c> method. For example: <c>IG_Print</c>
    ///   </item>
    ///   <item>
    ///   Path of executable file/command. For example: <c>cmd.exe</c>
    ///   </item>
    /// </list>
    /// </summary>
    public string Executable { get; set; } = "";


    /// <summary>
    /// Arguments to pass to the <see cref="Executable"/> in JSON format.
    /// </summary>
    public string Argument { get; set; } = "";


    /// <summary>
    /// The next command to execute after running the <see cref="Executable"/>.
    /// </summary>
    public SingleAction? NextAction { get; set; }


    /// <summary>
    /// Gets, sets the language key of this action.
    /// </summary>
    [JsonIgnore]
    public string? LangKey { get; set; }


    // for JSON deserialization
    public SingleAction() { }


    public SingleAction(string executable = "", string argument = "", SingleAction? nextAction = null)
    {
        Executable = executable;
        Argument = argument;
        NextAction = nextAction;
    }


    public SingleAction(API api, string? argument = null, string langKey = "")
    {
        Executable = Enum.GetName(api) ?? "";
        Argument = argument ?? "";
        LangKey = langKey;
    }


    public SingleAction(SingleAction? action = null)
    {
        Executable = action?.Executable ?? "";
        Argument = action?.Argument ?? "";
        NextAction = action?.NextAction;
        LangKey = action?.LangKey;
    }


    public override string ToString()
    {
        return Executable;
    }

}




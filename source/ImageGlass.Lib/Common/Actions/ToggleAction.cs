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
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Actions;


[JsonSerializable(typeof(ToggleAction))]
public partial class ToggleActionJsonContext : JsonSerializerContext { }


public partial class ToggleAction : PhReactive
{
    /// <summary>
    /// Gets the manager to check whether the <see cref="ToggleAction"/>
    /// value is on (<c>true</c>) or off (<c>false</c>).
    /// </summary>
    private static readonly Dictionary<Guid, bool> _manager = [];


    /// <summary>
    /// Gets the id of the action for toggling.
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; init; } = Guid.NewGuid();


    /// <summary>
    /// Action to run when toggling on.
    /// </summary>
    public SingleAction? ToggleOn { get; set; } = null;


    /// <summary>
    /// Action to run when toggling off.
    /// </summary>
    public SingleAction? ToggleOff { get; set; } = null;


    public ToggleAction(SingleAction? toggleOn = null, SingleAction? toggleOff = null)
    {
        ToggleOn = toggleOn;
        ToggleOff = toggleOff;
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString()
    {
        return $"{ToggleOn?.ToString() ?? "<empty>"} | {ToggleOff?.ToString() ?? "<empty>"}";
    }


    #region Static Methods

    /// <summary>
    /// Checks if the given command is off.
    /// </summary>
    public static bool IsToggleOff(Guid cmdId)
    {
        if (_manager.TryGetValue(cmdId, out var isToggled))
        {
            return isToggled;
        }

        return false;
    }


    /// <summary>
    /// Sets the toggling value of the given command.
    /// </summary>
    public static void SetToggleValue(Guid cmdId, bool isToggled)
    {
        if (!_manager.TryAdd(cmdId, isToggled))
        {
            _manager[cmdId] = isToggled;
        }
    }

    #endregion // Static Methods

}




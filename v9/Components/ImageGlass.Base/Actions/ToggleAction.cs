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
using System.Text.Json.Serialization;

namespace ImageGlass.Base.Actions;


/// <summary>
/// Defines user toggling action
/// </summary>
public class ToggleAction
{
    /// <summary>
    /// Gets the ToggleAction manager to check whether the <see cref="ToggleAction"/>
    /// toggling value is on (<c>true</c>) or off (<c>false</c>).
    /// </summary>
    private static readonly Dictionary<Guid, bool> _manager = new();


    /// <summary>
    /// Gets the id of the action for toggling.
    /// </summary>
    [JsonIgnore]
    public Guid Id { get; init; }


    /// <summary>
    /// Action to run when toggling on.
    /// </summary>
    public SingleAction? ToggleOn { get; set; } = null;


    /// <summary>
    /// Action to run when toggling off.
    /// </summary>
    public SingleAction? ToggleOff { get; set; } = null;


    /// <summary>
    /// Initialize the empty <see cref="ToggleAction"/> instance.
    /// </summary>
    public ToggleAction()
    {
        Id = Guid.NewGuid();
    }


    /// <summary>
    /// Checks if the given action is toggled
    /// </summary>
    public static bool IsToggled(Guid actionId)
    {
        if (_manager.TryGetValue(actionId, out var isToggled))
        {
            return isToggled;
        }

        return false;
    }


    /// <summary>
    /// Sets the toggling value of the given action
    /// </summary>
    public static void SetToggleValue(Guid actionId, bool isToggled)
    {
        if (_manager.ContainsKey(actionId))
        {
            _manager[actionId] = isToggled;
        }
        else
        {
            _manager.Add(actionId, isToggled);
        }
    }

}


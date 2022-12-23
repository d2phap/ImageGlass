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
namespace ImageGlass.UI;

public enum PopupExitResult
{
    /// <summary>
    /// Nothing is returned from the popup. This means that the modal dialog continues running.
    /// </summary>
    None = 0,

    /// <summary>
    /// The popup return value is OK.
    /// </summary>
    OK = 1,

    /// <summary>
    /// The popup return value is Cancel.
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// The popup return value is Abort when user closes by ESC key.
    /// </summary>
    Abort = 3,
}


/// <summary>
/// Specifies identifiers to indicate the return data of a popup.
/// </summary>
public class PopupResult
{
    /// <summary>
    /// Gets the exit result of the popup
    /// </summary>
    public PopupExitResult ExitResult { get; internal set; } = PopupExitResult.None;

    /// <summary>
    /// Gets <see cref="Popup.Value"/>
    /// </summary>
    public string Value { get; internal set; } = "";

    /// <summary>
    /// Gets the check state of the <see cref="Popup.ChkOption"/>
    /// </summary>
    public bool IsOptionChecked { get; internal set; } = false;
}

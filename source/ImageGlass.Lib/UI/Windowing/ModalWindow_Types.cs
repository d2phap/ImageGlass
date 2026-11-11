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
using Avalonia.Media;
using ImageGlass.Common.Types;

namespace ImageGlass.UI.Windowing;


/// <summary>
/// Defines constants that indicate the criticality of the InfoBar that is shown.
/// </summary>
public enum InfoBarSeverity
{
    Info,
    Success,
    Warning,
    Danger,
}



/// <summary>
/// The built-in buttons for <see cref="ModalWindow"/>.
/// </summary>
public enum ModalWindowButton
{
    OK,
    Close,
    Yes_No,
    OK_Cancel,
    OK_Close,
    LearnMore_Close,
    Continue_Quit,
}


/// <summary>
/// Specifies identifiers to indicate the return data of a dialog.
/// </summary>
public class ModalWindowResult
{
    /// <summary>
    /// Gets the exit result of the dialog.
    /// </summary>
    public DialogExitCode ExitCode { get; internal set; } = DialogExitCode.None;

    /// <summary>
    /// Gets the value of input.
    /// </summary>
    public string InputValue { get; internal set; } = "";

    /// <summary>
    /// Gets the check state of the Remember checkbox option.
    /// </summary>
    public bool IsRememberOptionChecked { get; internal set; } = false;
}


public record ModalWindowOptions
{
    public string? Title { get; set; }
    public string? Heading { get; set; }
    public string? Description { get; set; }
    public string? Details { get; set; }
    public string? Note { get; set; }
    public InfoBarSeverity? NoteStyle { get; set; }
    public IImage? Thumbnail { get; set; }
    public StockIconId? ThumbnailIcon { get; set; }
    public bool IsRememberOptionVisible { get; set; }
    public bool? IsInputVisible { get; set; }
    public string InputValue { get; set; } = string.Empty;
    public string? InputPlaceholder { get; set; }
    public TextBoxAcceptValue AcceptValue { get; set; } = TextBoxAcceptValue.Any;
    public bool? ShowInTaskbar { get; set; }
}
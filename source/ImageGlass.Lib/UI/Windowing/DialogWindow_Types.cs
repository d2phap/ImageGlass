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
using System;

namespace ImageGlass.UI.Windowing;


public enum DialogExitCode
{
    /// <summary>
    /// Nothing is returned from the dialog box. This means that the modal dialog continues running.
    /// </summary>
    None = 0,
    /// <summary>
    /// The dialog box return value is OK (usually sent from a button labeled OK).
    /// </summary>
    OK = 1,
    /// <summary>
    /// The dialog box return value is Cancel (usually sent from a button labeled Cancel).
    /// </summary>
    Cancel = 2,
    /// <summary>
    /// The dialog box return value is Abort (usually sent from a button labeled Abort).
    /// </summary>
    Abort = 3,
}


public enum DialogButton
{
    Button1,
    Button2,
    Button3,

    // no default button: SetDefaultButton applies no accent (e.g. an info dialog whose only
    // footer button is a neutral Close)
    None,
}


public enum DialogAction
{
    Submit,
    Cancel,
    Apply,
}


public enum DialogFocus
{
    Default,
    Button1,
    Button2,
    Button3,
}


public class DialogEventArgs(DialogAction actionType) : EventArgs
{
    public DialogAction ActionType => actionType;
    public bool CanProceed { get; set; } = true;
}

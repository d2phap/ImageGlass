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
using System.ComponentModel;

namespace ImageGlass.Settings;


/// <summary>
///  Provides data for the <see cref='ToolForm.OnToolFormClosing'/> event.
/// </summary>
public class ToolFormClosingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Gets the name of the tool form.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///  Provides the reason for the Form Close.
    /// </summary>
    public CloseReason CloseReason { get; }


    public ToolFormClosingEventArgs(string name, CloseReason closeReason, bool cancel) : base(cancel)
    {
        Name = name;
        CloseReason = closeReason;
    }

}


/// <summary>
/// Provides data for the <see cref='ToolForm.OnToolFormClosed'/> event.
/// </summary>
public class ToolFormClosedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the tool form.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///  Provides the reason for the Form Close.
    /// </summary>
    public CloseReason CloseReason { get; }


    public ToolFormClosedEventArgs(string name, CloseReason closeReason)
    {
        Name = name;
        CloseReason = closeReason;
    }

}


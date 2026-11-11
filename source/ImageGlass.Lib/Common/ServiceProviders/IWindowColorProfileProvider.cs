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

using Avalonia.Controls;
using ImageGlass.Common.Types;
using System;
using System.Threading.Tasks;

namespace ImageGlass.Common.ServiceProviders;


/// <summary>
/// Manages the color profile for a window.
/// </summary>
public interface IWindowColorProfileProvider : IDisposable
{
    /// <summary>
    /// Occurs when a physical display's color profile is changed.
    /// </summary>
    event TEventHandler<IWindowColorProfileProvider, ColorProfileChangedEventArgs>? Changed;


    /// <summary>
    /// Gets the color profile path.
    /// </summary>
    string ProfilePath { get; }


    /// <summary>
    /// Indicates whether the HDR is enabled.
    /// </summary>
    bool IsHdr { get; }


    /// <summary>
    /// Indicates whether the instance has been initialized.
    /// </summary>
    bool IsInitialized { get; }


    /// <summary>
    /// Initializes the color profile setting instance for the given window.
    /// </summary>
    void Initialize(Window window);


    /// <summary>
    /// Reads the color profile data.
    /// </summary>
    Task<byte[]?> ReadColorProfileDataAsync();

}


public class ColorProfileChangedEventArgs(string profilePath, bool isHdr) : EventArgs
{
    /// <summary>
    /// Gets the color profile path.
    /// </summary>
    public string ProfilePath { get; set; } = profilePath;


    /// <summary>
    /// Indicates whether the HDR is enabled.
    /// </summary>
    public bool IsHdr { get; set; } = isHdr;
}

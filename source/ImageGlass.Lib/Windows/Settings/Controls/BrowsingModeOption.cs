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
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;

namespace ImageGlass.Common.Windows;

/// <summary>
/// One item of the Browsing mode dropdown: the localized mode name plus a short
/// description shown beneath it. Both are re-localized in place on language change.
/// </summary>
public sealed class BrowsingModeOption(BrowsingMode value, LangId nameKey, LangId descriptionKey)
    : PhReactive
{
    /// <summary>
    /// Gets the mode this option represents.
    /// </summary>
    public BrowsingMode Value => value;


    /// <summary>
    /// Gets the localized mode name.
    /// </summary>
    public string Name => Core.Lang[nameKey];


    /// <summary>
    /// Gets the localized one-line description of the mode.
    /// </summary>
    public string Description => Core.Lang[descriptionKey];


    /// <summary>
    /// Re-raises change notifications so the bound texts pick up a new language.
    /// </summary>
    public void RefreshLang()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
    }
}

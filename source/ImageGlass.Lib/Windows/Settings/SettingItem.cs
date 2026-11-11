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
using ImageGlass.Common.Localization;
using System.Collections.Generic;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Describes a single searchable/navigable setting row rendered on a settings page.
/// </summary>
public sealed class SettingItem
{
    /// <summary>
    /// Gets the associated config id, or <c>null</c> for non-config widgets (links, info rows).
    /// </summary>
    public ConfigId? Id { get; init; }

    /// <summary>
    /// Gets the localization key of the setting label.
    /// </summary>
    public LangId Label { get; init; }

    /// <summary>
    /// Gets the nav id of the page that hosts this setting.
    /// </summary>
    public SettingsNavId PageNavId { get; init; }

    /// <summary>
    /// Gets the localization key of the page (sidebar tab) that hosts this setting.
    /// </summary>
    public LangId? Page { get; init; }

    /// <summary>
    /// Gets the localization key of the section heading this setting belongs to.
    /// </summary>
    public LangId? Section { get; init; }

    /// <summary>
    /// Gets, sets the materialized control to scroll to / highlight on navigation.
    /// </summary>
    public Control? Target { get; set; }


    /// <summary>
    /// Gets the localized label text.
    /// </summary>
    public string LabelText => Core.Lang[Label];

    /// <summary>
    /// Gets the localized breadcrumb path of this setting, e.g. "General &gt; Startup".
    /// Includes the page (tab) name and the section heading when present.
    /// </summary>
    public string SectionText
    {
        get
        {
            var parts = new List<string>(2);
            if (Page is { } p) parts.Add(Core.Lang[p]);
            if (Section is { } s) parts.Add(Core.Lang[s]);
            return string.Join(" > ", parts);
        }
    }
}

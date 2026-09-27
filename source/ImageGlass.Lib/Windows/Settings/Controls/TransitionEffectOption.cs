/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2026 DUONG DIEU PHAP
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
using ImageGlass.UI.Viewer.Transitions;

namespace ImageGlass.Common.Windows;

/// <summary>
/// One item of a transition effect dropdown: None / Random, an effect file, or the divider between them.
/// </summary>
public sealed class TransitionEffectOption : PhReactive
{
    private readonly LangId? _nameKey;


    /// <summary>
    /// Gets the effect id stored in the config; empty for the divider.
    /// </summary>
    public string Id { get; }


    /// <summary>
    /// Gets the file of an effect file, or <c>null</c> for None / Random.
    /// </summary>
    public string? FilePath { get; }


    /// <summary>
    /// Gets whether this item is the divider between None / Random and the effect files.
    /// </summary>
    public bool IsDivider { get; }


    /// <summary>
    /// Gets the localized name of None / Random, or the id of an effect file (its name without the extension).
    /// </summary>
    public string Name => _nameKey is { } key ? Core.Lang[key] : Id;


    /// <summary>
    /// Gets whether the item shows a file path.
    /// </summary>
    public bool HasFilePath => FilePath is not null;


    /// <summary>
    /// Gets the file path as the user sees it, or <c>null</c> for None / Random.
    /// </summary>
    public string? DisplayFilePath => FilePath is null ? null : BHelper.GetRealPlatformPath(FilePath);


    private TransitionEffectOption(string id, string? filePath, bool isDivider)
    {
        Id = id;
        FilePath = filePath;
        IsDivider = isDivider;
        _nameKey = filePath is null && !isDivider ? Lang.GetKey($"{nameof(TransitionEffect)}_{id}") : null;
    }


    /// <summary>
    /// Creates the item of <c>None</c> / <c>Random</c>, or of a saved effect whose file is gone.
    /// </summary>
    public static TransitionEffectOption FromId(string id) => new(id, null, false);


    /// <summary>
    /// Creates the item of an effect file.
    /// </summary>
    public static TransitionEffectOption FromFile(string id, string filePath) => new(id, filePath, false);


    /// <summary>
    /// Creates the divider item, which the dropdown keeps disabled.
    /// </summary>
    public static TransitionEffectOption CreateDivider() => new(string.Empty, null, true);


    /// <summary>
    /// Re-raises change notifications so the bound name picks up a new language.
    /// </summary>
    public void RefreshLang() => OnPropertyChanged(nameof(Name));

}

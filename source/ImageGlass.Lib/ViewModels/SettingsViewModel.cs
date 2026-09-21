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
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

/// <summary>
/// Working copy of the settings being edited, decoupled from <see cref="Core.Config"/>.
/// <para>
/// Edits are staged in <see cref="_pending"/> and only written to <see cref="Core.Config"/>
/// (and persisted to disk) when the user presses OK or Apply via <see cref="CommitAsync"/>.
/// Switching pages keeps pending edits; Cancel calls <see cref="Discard"/>.
/// </para>
/// </summary>
public sealed class SettingsViewModel : PhReactive
{
    // only changed values live here; everything else reads through to Core.Config
    private readonly Dictionary<ConfigId, object?> _pending = [];


    /// <summary>
    /// Gets the shared index of all setting rows (for search + navigate-by-config).
    /// </summary>
    public SettingsRegistry Registry { get; } = new();


    /// <summary>
    /// Gets whether there are uncommitted edits.
    /// </summary>
    public bool IsDirty => _pending.Count > 0;


    /// <summary>
    /// Gets the staged value for <paramref name="id"/>, falling back to the current
    /// <see cref="Core.Config"/> value (or <paramref name="defaultValue"/>) when not staged.
    /// </summary>
    public T GetValue<T>(ConfigId id, T defaultValue)
    {
        if (_pending.TryGetValue(id, out var v) && v is T staged) return staged;
        return Core.Config.Get(id, defaultValue);
    }


    /// <summary>
    /// Stages a value for <paramref name="id"/> and raises a change notification
    /// (so live UI inside the dialog can react). Does NOT touch <see cref="Core.Config"/>.
    /// </summary>
    public void SetValue(ConfigId id, object? value)
    {
        _pending[id] = value;
        _ = OnPropertyChanged(value, null, id.ToString());
    }


    /// <summary>
    /// Writes all staged edits into <see cref="Core.Config"/>, persists to disk,
    /// then clears the staging store and runs any post-apply actions.
    /// </summary>
    /// <returns><c>false</c> if the values were applied but not persisted to disk.</returns>
    public async Task<bool> CommitAsync()
    {
        if (_pending.Count == 0) return true;

        // 1. push staged values into the live config (raises PropertyChanged -> live UI updates).
        //    admin-locked ids are refused here: skipping Set keeps their admin-merged value, and
        //    excluding them from changedIds stops their post-apply side effects from running
        var changedIds = new List<ConfigId>(_pending.Count);
        foreach (var (id, value) in _pending)
        {
            if (value is null || Config.IsConfigLocked(id)) continue;
            Core.Config.Set(id, value);
            changedIds.Add(id);
        }
        _pending.Clear();

        // 2. persist to disk
        var isSaved = await Core.Config.SaveAsync();

        // 3. run settings that need an explicit apply step (theme/language/list reload)
        RunApplyActions(changedIds);

        return isSaved;
    }


    /// <summary>
    /// Drops all staged edits.
    /// </summary>
    public void Discard() => _pending.Clear();


    /// <summary>
    /// Runs explicit follow-up actions for committed settings that don't propagate on their own via
    /// <see cref="Core.Config"/> bindings. Shared with <see cref="AppAPIProvider.IG_ApplySettingsAsync"/>.
    /// </summary>
    internal static void RunApplyActions(IReadOnlyList<ConfigId> changedIds)
    {
        // changes require reloading photo list
        var reloadList = changedIds.Any(static id => id
            is ConfigId.ImageLoadingOrder
            or ConfigId.ImageLoadingOrderType
            or ConfigId.EnableExplorerSortOrder
            or ConfigId.EnableSubfoldersLoading
            or ConfigId.EnableImageFolderGrouping
            or ConfigId.EnableHiddenImagesLoading);

        // changes require reloading current photo (IG_Reload keeps the current zoom + pan)
        var reloadPhoto = changedIds.Any(static id => id
            is ConfigId.EnableOnlyLoadRawPreview
            or ConfigId.EnableOnlyLoadNonRawPreview
            or ConfigId.EnableHdrToneMapping
            or ConfigId.EnableAlwaysApplyColorProfile
            or ConfigId.EnableVectorRenderer);

        // ColorProfile isn't in reloadPhoto: UpdateDestColorProfile also invalidates the codec
        // selection cache, then re-applies via the ColorProfileChanged event (also keeping zoom).
        if (changedIds.Contains(ConfigId.ColorProfile))
        {
            Core.UpdateDestColorProfile();
        }

        // these flip which codec decodes an extension; drop the sticky per-extension selection cache
        if (changedIds.Any(static id => id
            is ConfigId.EnableVectorRenderer
            or ConfigId.EnableOnlyLoadRawPreview
            or ConfigId.EnableOnlyLoadNonRawPreview))
        {
            Core.CodecRegistry.InvalidateSelectionCaches();
        }

        // language pack changed -> reload it live (setting Core.Lang raises Core.LanguageChanged)
        if (changedIds.Contains(ConfigId.Language))
        {
            _ = Core.Config.LoadCurrentLanguageAsync();
        }

        // external tools edited -> rebuild the registry so the Tools menu / IG_OpenTool use the new values
        if (changedIds.Contains(ConfigId.Tools))
        {
            Core.ReloadExternalTools();
        }

        // updated hotkeys after toolbar buttons, menu hotkeys, or external tools edited
        if (changedIds.Contains(ConfigId.ToolbarButtons)
            || changedIds.Contains(ConfigId.MenuHotkeys)
            || changedIds.Contains(ConfigId.Tools))
        {
            Core.API.RegisterHotkeys();
        }

        // theme pack changed -> re-apply live. omit the accent so ApplyThemePackAsync reads the live
        // OS accent itself; passing the derived Core.AccentColor would make a system-accent pack
        // inherit the previous pack's accent (and darken it again each time)
        if (changedIds.Any(static id => id is ConfigId.DarkTheme or ConfigId.LightTheme)
            && Avalonia.Application.Current is App app)
        {
            _ = app.ApplyThemePackAsync(Core.IsSystemDarkMode);
        }


        if (reloadList) AppAPIProvider.IG_ReloadList();
        if (reloadPhoto) AppAPIProvider.IG_Reload(false);
    }
}

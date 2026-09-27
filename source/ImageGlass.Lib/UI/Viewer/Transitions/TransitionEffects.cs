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
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// Registry of the photo transition effects, which are the files in the <see cref="Dir.Transitions"/> folders.
/// </summary>
public static class TransitionEffects
{
    /// <summary>
    /// Effect id that disables the transition.
    /// </summary>
    public const string NONE = "None";

    /// <summary>
    /// Effect id that picks a different effect for every transition.
    /// </summary>
    public const string RANDOM = "Random";

    /// <summary>
    /// File extension of an effect, written in the gl-transitions GLSL format; other files in the folders are ignored.
    /// </summary>
    public const string FILE_EXT = ".igtransition.glsl";

    public const uint MIN_DURATION_MS = 50;
    public const uint MAX_DURATION_MS = 10_000;

    // null until first read, so startup never touches the folders
    private static volatile TransitionEffect[]? _effects;


    /// <summary>
    /// Gets the effects read from the <see cref="Dir.Transitions"/> folders, loading them on first use.
    /// </summary>
    public static IReadOnlyList<TransitionEffect> All => _effects ??= LoadEffects();


    /// <summary>
    /// Re-reads the effect folders, picking up added, edited and removed files.
    /// </summary>
    public static void Reload()
    {
        _effects = LoadEffects();
    }


    /// <summary>
    /// Finds an effect by id; <see cref="RANDOM"/> picks one of the effects that compile.
    /// </summary>
    public static TransitionEffect? Find(string? effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId)) return null;
        if (effectId.Equals(NONE, StringComparison.OrdinalIgnoreCase)) return null;

        if (effectId.Equals(RANDOM, StringComparison.OrdinalIgnoreCase))
        {
            var usable = All.Where(e => e.GetEffect() is not null).ToArray();
            return usable.Length > 0 ? usable[Random.Shared.Next(usable.Length)] : null;
        }

        return All.FirstOrDefault(e => e.Id.Equals(effectId, StringComparison.OrdinalIgnoreCase));
    }


    /// <summary>
    /// Creates a transition request (<paramref name="direction"/> below 0 is backward); <c>null</c> if the effect is unusable.
    /// </summary>
    public static TransitionRequest? CreateRequest(string? effectId, uint durationMs, int direction)
    {
        var effect = Find(effectId);
        if (effect?.GetEffect() is null) return null;

        return new TransitionRequest(effect,
            (int)Math.Clamp(durationMs, MIN_DURATION_MS, MAX_DURATION_MS),
            direction < 0 ? -1 : 1);
    }


    /// <summary>
    /// Reads the effect files of the config folder, then of the app folder; the first file of an id wins.
    /// </summary>
    private static TransitionEffect[] LoadEffects()
    {
        var takenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NONE, RANDOM };

        var effects = new List<TransitionEffect>();
        var configDir = BHelper.ConfigDir(Dir.Transitions);
        var baseDir = BHelper.BaseDir(Dir.Transitions);

        LoadEffectsFromDir(configDir, takenIds, effects);

        // portable mode keeps both in the same folder
        var isSameDir = string.Equals(Path.GetFullPath(configDir), Path.GetFullPath(baseDir), StringComparison.OrdinalIgnoreCase);
        if (!isSameDir) LoadEffectsFromDir(baseDir, takenIds, effects);

        return [.. effects];
    }


    /// <summary>
    /// Adds the effects of one folder whose id is not taken yet; the files compile on first use.
    /// </summary>
    private static void LoadEffectsFromDir(string dir, HashSet<string> takenIds, List<TransitionEffect> effects)
    {
        string[] files;
        try
        {
            if (!Directory.Exists(dir)) return;
            files = Directory.GetFiles(dir);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Unable to list transition effects in '{dir}': {ex.Message}");
            return;
        }
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var isEffect = fileName.EndsWith(FILE_EXT, StringComparison.OrdinalIgnoreCase);
            if (!isEffect || fileName.Length == FILE_EXT.Length) continue;

            var id = fileName[..^FILE_EXT.Length];
            if (!takenIds.Add(id)) continue;

            try
            {
                var glsl = File.ReadAllText(file);
                effects.Add(TransitionEffect.FromGlsl(id, glsl, file));
            }
            catch (Exception ex)
            {
                // an unreadable file only loses its own effect
                Debug.WriteLine($"❌ Unable to read transition effect '{file}': {ex.Message}");
            }
        }
    }

}

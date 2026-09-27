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
/// Registry of the photo transition effects: the built-in ones plus the custom ones in <see cref="Dir.Transitions"/>.
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
    /// File extension of a custom effect written in SkSL against <see cref="TransitionEffect.SHADER_HEADER"/>.
    /// </summary>
    public const string SKSL_EXT = ".sksl";

    /// <summary>
    /// File extension of a custom effect written for gl-transitions.
    /// </summary>
    public const string GLSL_EXT = ".glsl";

    public const uint MIN_DURATION_MS = 50;
    public const uint MAX_DURATION_MS = 10_000;

    // null until first read, so startup never touches the folder
    private static volatile TransitionEffect[]? _customEffects;


    /// <summary>
    /// Gets the custom effects, loading them from <see cref="Dir.Transitions"/> on first use.
    /// </summary>
    public static IReadOnlyList<TransitionEffect> CustomEffects => _customEffects ??= LoadCustomEffects();


    /// <summary>
    /// Gets the built-in effects followed by the custom ones.
    /// </summary>
    public static IEnumerable<TransitionEffect> AllEffects => BuiltinTransitionEffects.All.Concat(CustomEffects);


    /// <summary>
    /// Gets the folder of the custom effects.
    /// </summary>
    public static string CustomEffectsDir => BHelper.ConfigDir(Dir.Transitions);


    /// <summary>
    /// Re-reads the custom effects folder, picking up added, edited and removed files.
    /// </summary>
    public static void ReloadCustomEffects()
    {
        _customEffects = LoadCustomEffects();
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
            var usable = AllEffects.Where(e => e.GetEffect() is not null).ToArray();
            return usable.Length > 0 ? usable[Random.Shared.Next(usable.Length)] : null;
        }

        return AllEffects.FirstOrDefault(e => e.Id.Equals(effectId, StringComparison.OrdinalIgnoreCase));
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
    /// Reads the custom effects; a file whose id is already taken is skipped, and the files compile on first use.
    /// </summary>
    private static TransitionEffect[] LoadCustomEffects()
    {
        var dir = CustomEffectsDir;
        string[] files;
        try
        {
            if (!Directory.Exists(dir)) return [];
            files = Directory.GetFiles(dir);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Unable to list transition effects in '{dir}': {ex.Message}");
            return [];
        }
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var takenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { NONE, RANDOM };
        takenIds.UnionWith(BuiltinTransitionEffects.All.Select(e => e.Id));

        var effects = new List<TransitionEffect>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);
            var isSkSL = ext.Equals(SKSL_EXT, StringComparison.OrdinalIgnoreCase);
            var isGlsl = ext.Equals(GLSL_EXT, StringComparison.OrdinalIgnoreCase);
            if (!isSkSL && !isGlsl) continue;

            var id = Path.GetFileNameWithoutExtension(file);
            if (!takenIds.Add(id)) continue;

            try
            {
                var code = File.ReadAllText(file);
                var source = isSkSL
                    ? TransitionEffect.SHADER_HEADER + code
                    : GlTransitionsAdapter.ToSkSL(code);
                var lineOffset = isSkSL
                    ? TransitionEffect.CountLines(TransitionEffect.SHADER_HEADER)
                    : GlTransitionsAdapter.LineOffset;

                effects.Add(new TransitionEffect(id, source, file, lineOffset));
            }
            catch (Exception ex)
            {
                // an unreadable file only loses its own effect
                Debug.WriteLine($"❌ Unable to read transition effect '{file}': {ex.Message}");
            }
        }

        return [.. effects];
    }

}

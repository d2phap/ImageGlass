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
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// Registry of the photo transition effects.
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

    public const uint MIN_DURATION_MS = 50;
    public const uint MAX_DURATION_MS = 10_000;


    /// <summary>
    /// Gets the built-in effects.
    /// </summary>
    public static IReadOnlyList<TransitionEffect> BuiltIns { get; } =
    [
        new("Fade", """
            half4 main(float2 p) {
                return mix(fromImage.eval(p), toImage.eval(p), progress);
            }
            """),

        // direction 1 pushes the old photo out to the left, -1 to the right
        new("Push", """
            half4 main(float2 p) {
                float offset = resolution.x * progress * direction;
                half4 oldColor = fromImage.eval(p + float2(offset, 0.0));
                half4 newColor = toImage.eval(p + float2(offset - resolution.x * direction, 0.0));
                return newColor + oldColor * (1.0 - newColor.a);
            }
            """),

        // direction 1 reveals the new photo from the right edge, -1 from the left edge
        new("Wipe", """
            half4 main(float2 p) {
                const float edge = 0.08;
                float x = p.x / resolution.x;
                if (direction < 0.0) x = 1.0 - x;

                float t = progress * (1.0 + edge);
                float amount = smoothstep(1.0 - t, 1.0 - t + edge, x);
                return mix(fromImage.eval(p), toImage.eval(p), amount);
            }
            """),
    ];


    /// <summary>
    /// Gets the ids of all selectable effects, including <see cref="NONE"/> and <see cref="RANDOM"/>.
    /// </summary>
    public static IEnumerable<string> GetEffectIds()
    {
        return BuiltIns.Select(e => e.Id).Prepend(RANDOM).Prepend(NONE);
    }


    /// <summary>
    /// Finds an effect by id; <see cref="RANDOM"/> picks one of the built-in effects.
    /// </summary>
    public static TransitionEffect? Find(string? effectId)
    {
        if (string.IsNullOrWhiteSpace(effectId)) return null;
        if (effectId.Equals(NONE, StringComparison.OrdinalIgnoreCase)) return null;

        if (effectId.Equals(RANDOM, StringComparison.OrdinalIgnoreCase))
        {
            return BuiltIns[Random.Shared.Next(BuiltIns.Count)];
        }

        return BuiltIns.FirstOrDefault(e => e.Id.Equals(effectId, StringComparison.OrdinalIgnoreCase));
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

}

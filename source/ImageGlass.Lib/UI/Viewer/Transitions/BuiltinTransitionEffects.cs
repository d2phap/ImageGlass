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
using System.Collections.Generic;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// The transition effects compiled into the app; the others ship as files in <see cref="Common.Types.Dir.Transitions"/>.
/// </summary>
public static class BuiltinTransitionEffects
{
    /// <summary>
    /// Gets the built-in effects, in the order they are listed.
    /// </summary>
    public static IReadOnlyList<TransitionEffect> All { get; } =
    [
        TransitionEffect.FromGlsl("Fade", """
            vec4 transition(vec2 uv) {
              return mix(getFromColor(uv), getToColor(uv), progress);
            }
            """),

        // blocks of 4 px keep the grain visible on high-DPI screens
        TransitionEffect.FromGlsl("Dissolve", """
            float hash(vec2 p) {
              vec3 p3 = fract(vec3(p.xyx) * 0.1031);
              p3 += dot(p3, p3.yzx + 33.33);
              return fract((p3.x + p3.y) * p3.z);
            }

            vec4 transition(vec2 uv) {
              float threshold = hash(floor(uv * resolution / 4.0));
              return threshold < progress ? getToColor(uv) : getFromColor(uv);
            }
            """),

        // navigationDirection 1 reveals the new photo from the right edge, -1 from the left edge
        TransitionEffect.FromGlsl("Wipe", """
            uniform float smoothness; // = 0.08

            vec4 transition(vec2 uv) {
              float x = navigationDirection < 0.0 ? 1.0 - uv.x : uv.x;
              float t = progress * (1.0 + smoothness);
              float amount = smoothstep(1.0 - t, 1.0 - t + smoothness, x);
              return mix(getFromColor(uv), getToColor(uv), amount);
            }
            """),
    ];

}

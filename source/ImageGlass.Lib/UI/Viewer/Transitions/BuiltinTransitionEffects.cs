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
/// The transition effects shipped with the app.
/// </summary>
public static class BuiltinTransitionEffects
{
    /// <summary>
    /// Gets the built-in effects, in the order they are listed.
    /// </summary>
    public static IReadOnlyList<TransitionEffect> All { get; } =
    [
        Create("Fade", """
            half4 main(float2 p) {
                return mix(fromImage.eval(p), toImage.eval(p), progress);
            }
            """),

        Create("FadeThroughBlack", """
            half4 main(float2 p) {
                const half4 black = half4(0.0, 0.0, 0.0, 1.0);
                if (progress < 0.5) return mix(fromImage.eval(p), black, progress * 2.0);

                return mix(black, toImage.eval(p), progress * 2.0 - 1.0);
            }
            """),

        // blocks of 4 px keep the grain visible on high-DPI screens
        Create("Dissolve", """
            float hash(float2 p) {
                float3 p3 = fract(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return fract((p3.x + p3.y) * p3.z);
            }

            half4 main(float2 p) {
                float threshold = hash(floor(p / 4.0));
                return threshold < progress ? toImage.eval(p) : fromImage.eval(p);
            }
            """),

        // the old photo grows past the viewer while the new one settles in from slightly smaller
        Create("Zoom", """
            half4 main(float2 p) {
                float2 center = resolution * 0.5;
                half4 oldColor = fromImage.eval(center + (p - center) / (1.0 + progress * 0.5));
                half4 newColor = toImage.eval(center + (p - center) / (0.8 + progress * 0.2));
                return mix(oldColor, newColor, progress);
            }
            """),

        // direction 1 pushes the old photo out to the left, -1 to the right
        Create("Push", """
            half4 main(float2 p) {
                float offset = resolution.x * progress * direction;
                half4 oldColor = fromImage.eval(p + float2(offset, 0.0));
                half4 newColor = toImage.eval(p + float2(offset - resolution.x * direction, 0.0));
                return newColor + oldColor * (1.0 - newColor.a);
            }
            """),

        // the new photo slides in over the old one, which fades so it does not linger around a smaller photo
        Create("Cover", """
            half4 main(float2 p) {
                half4 newColor = toImage.eval(p + float2(resolution.x * (1.0 - progress) * direction, 0.0));
                return newColor + fromImage.eval(p) * (1.0 - newColor.a) * (1.0 - progress);
            }
            """),

        // direction 1 reveals the new photo from the right edge, -1 from the left edge
        Create("Wipe", """
            half4 main(float2 p) {
                const float edge = 0.08;
                float x = p.x / resolution.x;
                if (direction < 0.0) x = 1.0 - x;

                float t = progress * (1.0 + edge);
                float amount = smoothstep(1.0 - t, 1.0 - t + edge, x);
                return mix(fromImage.eval(p), toImage.eval(p), amount);
            }
            """),

        // the radius grows from the center until it reaches the corners
        Create("Circle", """
            half4 main(float2 p) {
                const float edge = 0.02;
                float2 center = resolution * 0.5;
                float distance = length(p - center) / length(center);

                float t = progress * (1.0 + edge);
                float amount = 1.0 - smoothstep(t - edge, t, distance);
                return mix(fromImage.eval(p), toImage.eval(p), amount);
            }
            """),

        Create("Blinds", """
            half4 main(float2 p) {
                const float count = 10.0;
                float slat = fract(p.y / resolution.y * count);
                return slat < progress ? toImage.eval(p) : fromImage.eval(p);
            }
            """),
    ];


    private static TransitionEffect Create(string id, string body)
    {
        return new TransitionEffect(id, TransitionEffect.SHADER_HEADER + body);
    }

}

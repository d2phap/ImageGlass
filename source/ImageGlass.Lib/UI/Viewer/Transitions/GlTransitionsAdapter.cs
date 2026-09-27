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
using System.Text.RegularExpressions;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// Translates an effect in the gl-transitions GLSL format (https://gl-transitions.com), the only format effects are written in, to SkSL.
/// </summary>
public static partial class GlTransitionsAdapter
{
    // the authoring contract: never rename or remove an input here, only add, or existing effect files break
    private const string HEADER = """
        uniform shader fromImage;
        uniform shader toImage;
        uniform float progress;
        uniform float ratio;
        uniform float2 resolution;
        uniform float navigationDirection;

        // gl-transitions samples in 0-1 coordinates with the y axis pointing up
        half4 getFromColor(float2 uv) { return fromImage.eval(float2(uv.x, 1.0 - uv.y) * resolution); }
        half4 getToColor(float2 uv) { return toImage.eval(float2(uv.x, 1.0 - uv.y) * resolution); }

        """;

    private const string FOOTER = """

        half4 main(float2 p) {
            float2 uv = p / resolution;
            return half4(transition(float2(uv.x, 1.0 - uv.y)));
        }
        """;


    /// <summary>
    /// Wraps the <c>vec4 transition(vec2 uv)</c> source of a gl-transitions effect into a complete SkSL shader.
    /// </summary>
    public static string ToSkSL(string glsl)
    {
        // SkSL uniforms take no initializer, so a parameter with a "// = default" comment becomes a constant
        var body = UniformWithDefaultRegex().Replace(glsl, "const $1 $2 = $3;");

        return HEADER + body + FOOTER;
    }


    /// <summary>
    /// Gets the number of lines <see cref="ToSkSL"/> puts before the author's code.
    /// </summary>
    public static int LineOffset { get; } = TransitionEffect.CountLines(HEADER);


    [GeneratedRegex(@"^[ \t]*uniform[ \t]+(\w+)[ \t]+(\w+)[ \t]*;[ \t]*//[ \t]*=[ \t]*([^;\r\n]+?)[ \t]*;?[ \t]*$", RegexOptions.Multiline)]
    private static partial Regex UniformWithDefaultRegex();

}

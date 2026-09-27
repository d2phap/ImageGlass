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
using SkiaSharp;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// A photo transition effect, drawn by an SkSL runtime shader.
/// </summary>
public sealed partial class TransitionEffect
{
    private readonly Lock _lock = new();
    private SKRuntimeEffect? _effect;
    private bool _isCompiled;


    /// <summary>
    /// Gets the effect id, stored in the config.
    /// </summary>
    public string Id { get; }


    /// <summary>
    /// Gets the complete SkSL source, translated from the effect's GLSL.
    /// </summary>
    public string Source { get; }


    /// <summary>
    /// Gets the file of a custom effect, or <c>null</c> for a built-in one.
    /// </summary>
    public string? FilePath { get; }


    /// <summary>
    /// Gets the number of lines wrapped around the author's code, so errors report the line of their file.
    /// </summary>
    public int LineOffset { get; }


    /// <summary>
    /// Gets the shader compiler errors, or <c>null</c> if the effect compiled (or has not compiled yet).
    /// </summary>
    public string? CompileError { get; private set; }


    private TransitionEffect(string id, string source, string? filePath, int lineOffset)
    {
        Id = id;
        Source = source;
        FilePath = filePath;
        LineOffset = lineOffset;
    }


    /// <summary>
    /// Creates an effect from its gl-transitions GLSL source.
    /// </summary>
    public static TransitionEffect FromGlsl(string id, string glsl, string? filePath = null)
    {
        return new TransitionEffect(id, GlTransitionsAdapter.ToSkSL(glsl), filePath, GlTransitionsAdapter.LineOffset);
    }


    /// <summary>
    /// Gets the compiled shader, compiling it once on first use; <c>null</c> if the source is invalid.
    /// </summary>
    public SKRuntimeEffect? GetEffect()
    {
        lock (_lock)
        {
            if (_isCompiled) return _effect;
            _isCompiled = true;

            var effect = SKRuntimeEffect.CreateShader(Source, out var errors);
            if (effect is null)
            {
                CompileError = ErrorLineRegex().Replace(errors, m =>
                {
                    var line = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) - LineOffset;
                    return $"error: {line}:";
                });
                return null;
            }

            _effect = effect;
            return _effect;
        }
    }


    /// <summary>
    /// Counts the lines of a code prefix, for <see cref="LineOffset"/>.
    /// </summary>
    public static int CountLines(string prefix) => prefix.Split('\n').Length - 1;


    [GeneratedRegex(@"^error: (\d+):", RegexOptions.Multiline)]
    private static partial Regex ErrorLineRegex();

}

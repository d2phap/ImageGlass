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
using System.Threading;

namespace ImageGlass.UI.Viewer.Transitions;


/// <summary>
/// A photo transition effect, drawn by an SkSL runtime shader.
/// </summary>
public sealed class TransitionEffect
{
    /// <summary>
    /// The inputs every effect receives; its source only has to define <c>half4 main(float2 p)</c>.
    /// </summary>
    public const string SHADER_HEADER = """
        uniform shader fromImage;
        uniform shader toImage;
        uniform float progress;
        uniform float2 resolution;
        uniform float direction;

        """;

    private readonly Lock _lock = new();
    private SKRuntimeEffect? _effect;
    private bool _isCompiled;


    /// <summary>
    /// Gets the effect id, stored in the config.
    /// </summary>
    public string Id { get; }


    /// <summary>
    /// Gets the SkSL source, without <see cref="SHADER_HEADER"/>.
    /// </summary>
    public string Source { get; }


    /// <summary>
    /// Gets the shader compiler errors, or <c>null</c> if the effect compiled (or has not compiled yet).
    /// </summary>
    public string? CompileError { get; private set; }


    public TransitionEffect(string id, string source)
    {
        Id = id;
        Source = source;
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

            var effect = SKRuntimeEffect.CreateShader(SHADER_HEADER + Source, out var errors);
            if (effect is null)
            {
                CompileError = errors;
                return null;
            }

            _effect = effect;
            return _effect;
        }
    }

}

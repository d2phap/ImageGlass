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
using Avalonia;
using ImageGlass.Common.Types;
using SkiaSharp;

namespace ImageGlass.Common.Photoing;

/// <summary>
/// Represents the result of decoding a photo through the codec registry.
/// </summary>
public sealed class CodecDecodeResult : PhDisposable
{
    /// <summary>
    /// Gets or sets the stable codec identifier.
    /// </summary>
    public string CodecId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the decoded content kind.
    /// </summary>
    public CodecContentKind ContentKind { get; set; } = CodecContentKind.None;

    /// <summary>
    /// Gets or sets the logical size of the decoded result.
    /// </summary>
    public Size Size { get; set; } = new();

    /// <summary>
    /// Gets or sets the linear scale the image was decoded at; below 1 when it was too large to decode fully.
    /// </summary>
    public double DecodeScale { get; set; } = 1;

    /// <summary>
    /// Gets or sets the decoded single-frame raster image.
    /// </summary>
    public SKImage? SingleFrame { get; set; } = null;

    /// <summary>
    /// Gets or sets the decoded animator.
    /// </summary>
    public AnimatorImpl? Animator { get; set; } = null;

    /// <summary>
    /// Gets or sets the decoded vector source.
    /// </summary>
    public SkiaVectorSource? VectorSource { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether the decoded content is HDR.
    /// </summary>
    public bool IsHdr { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the decoded content carries an embedded color profile.
    /// </summary>
    public bool HasEmbeddedColorProfile { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the pixels carry a display profile their tag cannot describe (a CMYK soft-proof).
    /// </summary>
    public bool HasBakedDisplayProfile { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the viewer should prefer direct rendering for the result.
    /// </summary>
    public bool PreferDirectRender { get; set; } = false;


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnDisposing()
    {
        base.OnDisposing();

        SingleFrame?.Dispose();
        SingleFrame = null;

        Animator?.Dispose();
        Animator = null;

        VectorSource?.Dispose();
        VectorSource = null;
    }
}
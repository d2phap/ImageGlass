/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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

public partial class SkiaDecoderOutput : PhDisposable
{
    public SKImage? SingleFrame { get; set; } = null;
    public SkiaAnimator? Animator { get; set; } = null;
    public SkiaVectorSource? VectorSource { get; set; } = null;

    public Size Size { get; set; } = new();

    /// <summary>
    /// Linear scale the image was decoded at; below 1 when it was too large to decode fully.
    /// </summary>
    public double DecodeScale { get; set; } = 1;


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
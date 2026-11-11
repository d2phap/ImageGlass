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
namespace ImageGlass.Common.Photoing;

internal static class CodecDecodeResultFactory
{
    public static CodecDecodeResult FromSkiaOutput(string codecId,
        SkiaDecoderOutput output, PhotoMetadata metadata)
    {
        var result = new CodecDecodeResult
        {
            CodecId = codecId,
            Size = output.Size,
            DecodeScale = output.DecodeScale,
            IsHdr = metadata.IsHdr,
            HasEmbeddedColorProfile = metadata.SkiaColorSpace is not null || metadata.MagickColorProfile is not null,
        };

        if (output.VectorSource is not null)
        {
            result.ContentKind = CodecContentKind.Vector;
            result.PreferDirectRender = true;
            result.VectorSource = output.VectorSource;
            output.VectorSource = null;
            return result;
        }

        if (output.Animator is not null)
        {
            result.ContentKind = CodecContentKind.Animation;
            result.Animator = output.Animator;
            output.Animator = null;
            return result;
        }

        if (output.SingleFrame is not null)
        {
            result.ContentKind = CodecContentKind.StaticRaster;
            result.SingleFrame = output.SingleFrame;
            output.SingleFrame = null;
        }

        return result;
    }
}
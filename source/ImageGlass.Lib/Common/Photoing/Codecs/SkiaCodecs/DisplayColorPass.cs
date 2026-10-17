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
using ImageGlass.Common.Extensions;
using SkiaSharp;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// The color management a frame gets before display (HDR tone mapping, then the monitor profile), comparable by value.
/// </summary>
public sealed record DisplayColorPass(bool IsHdr, HdrTransferFunction TransferFn, double ContentPeakNits,
    HdrToneMappingOptions? Options, SKColorSpace? DestProfile)
{
    /// <summary>
    /// Whether the pass leaves every frame as it is.
    /// </summary>
    public bool IsNoOp => !IsHdr && DestProfile is null;


    /// <summary>
    /// The pass a viewer rendering HDR when <paramref name="enableHdr"/> gives <paramref name="meta"/>'s photo now.
    /// </summary>
    public static DisplayColorPass For(PhotoMetadata? meta, bool enableHdr)
    {
        var canApplyProfile = CanApplyDestProfile(meta);
        var destProfile = canApplyProfile ? Core.DestColorProfile : null;

        var isHdr = enableHdr && meta?.IsHdr == true;
        if (!isHdr) return new DisplayColorPass(false, HdrTransferFunction.None, 0, null, destProfile);

        // tuning the options is Pro-only; copied, so a later edit cannot change what this pass means
        var options = Core.IsProEnabled ? Core.HdrToneMappingConfig with { } : new HdrToneMappingOptions();

        return new DisplayColorPass(true, meta!.HdrTransferFn, meta.ContentPeakNits, options, destProfile);
    }


    /// <summary>
    /// Whether the monitor profile applies to a photo with <paramref name="meta"/>.
    /// </summary>
    public static bool CanApplyDestProfile(PhotoMetadata? meta)
    {
        if (!Core.IsDestColorProfileSupported) return false;
        if (Core.Config.EnableAlwaysApplyColorProfile) return true;

        return meta?.SkiaColorSpace is not null;
    }


    /// <summary>
    /// Applies the pass to <paramref name="srcImage"/>; <c>null</c> when it changes nothing.
    /// </summary>
    public SKImage? Apply(SKImage srcImage)
    {
        // 1. HDR: tone-map to standard sRGB first, then the monitor profile below (same as SDR)
        if (IsHdr)
        {
            var toneMapped = HdrToneMapper.ToneMapToSdr(srcImage, TransferFn, Options!, ContentPeakNits);

            if (!toneMapped.IsDisposed())
            {
                if (DestProfile is null) return toneMapped;

                var isProfiled = SkiaCodec.TryApplyColorSpace(toneMapped, DestProfile, out var profiled);
                if (!isProfiled) return toneMapped;

                toneMapped.Dispose();
                return profiled;
            }
        }

        // 2. monitor color profile only; a Magick decode that already converted to it tags the pixels so
        if (DestProfile is null) return null;

        var isInDestSpace = srcImage.ColorSpace is { } srcSpace && SKColorSpace.Equal(srcSpace, DestProfile);
        if (isInDestSpace) return null;

        var isColored = SkiaCodec.TryApplyColorSpace(srcImage, DestProfile, out var colored);
        return isColored ? colored : null;
    }
}

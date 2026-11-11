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
using System;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Color profile options.
/// </summary>
public enum ColorProfileOption
{
    None,
    Custom,
    CurrentMonitorProfile,

    // ImageMagick's profiles
    AdobeRGB1998,
    AppleRGB,
    CoatedFOGRA39,
    ColorMatchRGB,
    sRGB,
    USWebCoatedSWOP,
}


/// <summary>
/// Flip options.
/// </summary>
[Flags]
public enum FlipOptions
{
    None = 0,
    Horizontal = 1 << 1,
    Vertical = 1 << 2,
}


/// <summary>
/// Rotate option.
/// </summary>
public enum RotateOption
{
    Left = 0,
    Right = 1,
}


/// <summary>
/// Color channels
/// </summary>
[Flags]
public enum ColorChannels
{
    R = 1 << 1,
    G = 1 << 2,
    B = 1 << 3,
    A = 1 << 4,

    RGB = R | G | B,
    RGBA = RGB | A,
    RA = R | A,
    GA = G | A,
    BA = B | A,
}


/// <summary>
/// HDR tone mapping mode for SDR displays.
/// </summary>
public enum HdrToneMappingMode
{
    /// <summary>
    /// Pass through raw HDR values (for HDR monitors). Clips on SDR displays.
    /// </summary>
    None,

    /// <summary>
    /// BT.2408 reference white with a smooth roll-off across the whole range (default).
    /// Reference white lands near half SDR peak, matching how HDR photos are rendered to SDR.
    /// </summary>
    BT2408,

    /// <summary>
    /// Extended Reinhard with wide shoulder.
    /// Trades SDR brightness for significantly more highlight detail.
    /// Best for recovering specular highlights in HDR photos.
    /// </summary>
    Reinhard,

    /// <summary>
    /// ACES-style filmic curve with moderate shoulder.
    /// Cinematic rolloff — punchier than Reinhard, more highlight
    /// headroom than BT.2408.
    /// </summary>
    ACES,
}


/// <summary>
/// HDR transfer function type.
/// </summary>
public enum HdrTransferFunction
{
    /// <summary>The image is SDR; no HDR transfer function applies.</summary>
    None = 0,

    /// <summary>
    /// Perceptual Quantizer (SMPTE ST 2084), used in HDR10 and Dolby Vision.
    /// </summary>
    PQ = 1,

    /// <summary>
    /// Hybrid Log-Gamma, used in broadcast HDR.
    /// </summary>
    HLG = 2,

    /// <summary>
    /// HDR via gain map (Ultra HDR / ISO 21496-1).
    /// </summary>
    GainMap = 3,

    /// <summary>
    /// Scene-referred linear HDR (e.g. OpenEXR, Radiance HDR).
    /// No PQ/HLG transfer; pixels are linear, may exceed 1.0, and 1.0 IS diffuse white.
    /// </summary>
    Linear = 4,

    /// <summary>
    /// scRGB extended-range linear (IEC 61966-2-2), what WIC infers for every float or
    /// fixed-point format. Linear like <see cref="Linear"/>, but 1.0 is 80 nits, NOT white.
    /// </summary>
    ScRgb = 5,
}


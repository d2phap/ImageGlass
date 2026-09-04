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

using ImageGlass.Common.Types.JsonTypeConverters;
using System.Text.Json.Serialization;

namespace ImageGlass.Common.Photoing;


[JsonSerializable(typeof(HdrToneMappingOptions))]
public partial class HdrToneMappingOptionsJsonContext : JsonSerializerContext { }


/// <summary>
/// Options for HDR-to-SDR tone mapping in <see cref="HdrToneMapper"/>.
/// </summary>
public sealed record HdrToneMappingOptions
{
    /// <summary>
    /// Tone mapping algorithm (BT.2408, Reinhard, ACES, or None for pass-through).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<HdrToneMappingMode>))]
    public HdrToneMappingMode Mode { get; set; } = HdrToneMappingMode.BT2408;

    /// <summary>
    /// Exposure adjustment in EV stops.
    /// <c>0</c> = no change, <c>+1</c> = 2× brighter, <c>-1</c> = 0.5×.
    /// Typical range: <c>-3</c> to <c>+3</c>.
    /// </summary>
    public double Exposure { get; set; } = 0d;

    /// <summary>
    /// Override for the luminance (in nits) treated as HDR reference white. It is the
    /// normalization divisor, so lower brightens and higher darkens; the curve's peak is separate.
    /// Default: <c>203</c> (ITU-R BT.2408 reference white).
    /// Typical range: <c>100</c> to <c>400</c>.
    /// </summary>
    public double ReferenceWhiteNits { get; set; } = 203d;

    /// <summary>
    /// Extra tone-curve headroom above the peak the file declares, for a grade brighter than its
    /// own metadata claims. <c>0</c> = trust the declared peak, <c>1</c> = 4x it.
    /// Typical range: <c>0</c> to <c>1</c>.
    /// </summary>
    public double HighlightCompression { get; set; } = 0d;

    /// <summary>
    /// Post-tone-map saturation multiplier.
    /// <c>1</c> = no change, <c>&lt;1</c> = desaturate, <c>&gt;1</c> = boost.
    /// Typical range: <c>0</c> to <c>2</c>.
    /// </summary>
    public double Saturation { get; set; } = 1d;
}

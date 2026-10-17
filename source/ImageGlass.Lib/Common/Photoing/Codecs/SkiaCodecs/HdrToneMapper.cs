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
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Threading;
using System.Threading.Tasks;

namespace ImageGlass.Common.Photoing;


/// <summary>
/// Tone-maps HDR images to SDR sRGB for display on standard monitors.
/// <para>
/// Two tone-mapping strategies are used depending on content type:
/// <list type="bullet">
/// <item><b>Per-channel</b> for linear sRGB-primaries content (EXR, Radiance HDR, scRGB):
/// avoids overflow artifacts (skyblue tint, blue->white wash-out) in sRGB space.</item>
/// <item><b>Luminance-based</b> for wide-gamut PQ/HLG content (JXL, AVIF, HEIF)
/// — preserves channel ratios needed by the Rec.2020->sRGB gamut matrix.</item>
/// </list>
/// </para>
/// Monitor color profile is applied by the caller after tone mapping.
/// </summary>
public static class HdrToneMapper
{
    /// <summary>
    /// Input level, in reference-white units, mapped to SDR white when the file declares no
    /// content peak. Covers a 1200-nit grade, so brighter grades clip.
    /// </summary>
    private const float FallbackToneCurveWhiteLevel = 6f;

    /// <summary>
    /// Bounds for a white level derived from a declared content peak: below 1 the curve would clip
    /// reference white itself, and past 24 the extra headroom no longer changes the midtones.
    /// </summary>
    private const float MinToneCurveWhiteLevel = 1f;
    private const float MaxToneCurveWhiteLevel = 24f;

    /// <summary>
    /// ITU-R BT.2408 HDR reference white, and the default <see cref="HdrToneMappingOptions.ReferenceWhiteNits"/>.
    /// </summary>
    private const float Bt2408ReferenceWhiteNits = 203f;

    /// <summary>
    /// scRGB reference white (IEC 61966-2-2): the luminance its 1.0 stands for.
    /// </summary>
    private const float ScRgbWhiteNits = 80f;

    /// <summary>
    /// Above this, a linearized sample carries real HDR range; the slack absorbs rounding from the
    /// color-space conversion.
    /// </summary>
    private const float DiffuseWhiteThreshold = 1.001f;

    /// <summary>
    /// Peak histogram shape: buckets per reference-white unit, spanning [0, 32).
    /// </summary>
    private const int PeakBucketsPerUnit = 32;
    private const int PeakBuckets = 32 * PeakBucketsPerUnit;

    /// <summary>
    /// Share of samples allowed above the measured peak, so a few blown pixels cannot darken the
    /// whole image the way a true maximum would.
    /// </summary>
    private const double PeakPercentile = 0.9999d;

    // Rec.2020 luminance coefficients (ITU-R BT.2020) — used by luminance-based path
    private const float Lum2020R = 0.2627f;
    private const float Lum2020G = 0.6780f;
    private const float Lum2020B = 0.0593f;

    // Rec.2020 -> sRGB  3x3 gamut mapping matrix (linear)
    // M = sRGB_from_XYZ x XYZ_from_Rec2020
    private const float M00 = 1.6605f, M01 = -0.5876f, M02 = -0.0729f;
    private const float M10 = -0.1245f, M11 = 1.1329f, M12 = -0.0083f;
    private const float M20 = -0.0182f, M21 = -0.1006f, M22 = 1.1188f;

    // sRGB/Rec.709 luminance coefficients — used for saturation in per-channel path
    private const float Lum709R = 0.2126f;
    private const float Lum709G = 0.7152f;
    private const float Lum709B = 0.0722f;



    #region Public Methods

    /// <summary>
    /// Tone-maps an HDR image to SDR sRGB for display on standard monitors.
    /// Returns <c>null</c> if the source is invalid, when <see cref="HdrToneMappingOptions.Mode"/>
    /// is <see cref="HdrToneMappingMode.None"/> (pass-through), or when the
    /// decoded image is not actually HDR-encoded.
    /// </summary>
    /// <param name="contentPeakNits">Declared peak from the file's HDR10 metadata; <c>0</c> if none.</param>
    public static SKImage? ToneMapToSdr(SKImage? source,
        HdrTransferFunction transferFn, HdrToneMappingOptions options, double contentPeakNits = 0d)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (source.IsDisposed()) return null;
        if (options.Mode == HdrToneMappingMode.None) return null;

        // Gain-map images: the decoded base layer is already SDR.
        if (transferFn == HdrTransferFunction.GainMap) return null;

        SKImage? retaggedSource = null;
        try
        {
            var effectiveSource = source;

            if (!IsHdrColorSpace(source.ColorSpace))
            {
                if (transferFn is HdrTransferFunction.None or HdrTransferFunction.Linear
                    or HdrTransferFunction.ScRgb)
                {
                    // Linear sRGB-primaries HDR (EXR, Radiance HDR, scRGB):
                    // pixels are already linear but the source may have no color
                    // space tag (or sRGB). Tag as linear-sRGB so that Skia's
                    // DrawImage in ToneMapManual doesn't apply an unwanted sRGB
                    // gamma -> linear conversion (which would darken the image).
                    var linearCs = SKColorSpace.CreateSrgbLinear();

                    retaggedSource = ReinterpretColorSpace(source, linearCs);
                    if (retaggedSource is null) return null;

                    effectiveSource = retaggedSource;
                }
                else
                {
                    // PQ/HLG: metadata says PQ or HLG but the decoded color space
                    // doesn't reflect it — re-tag with the correct transfer function.
                    var hdrCs = BuildHdrColorSpace(transferFn, source.ColorSpace);
                    if (hdrCs is null) return null;

                    retaggedSource = ReinterpretColorSpace(source, hdrCs);
                    if (retaggedSource is null) return null;

                    effectiveSource = retaggedSource;
                }
            }

            return ToneMapManual(effectiveSource, transferFn, options, contentPeakNits);
        }
        finally
        {
            retaggedSource?.Dispose();
        }
    }


    /// <summary>
    /// Returns <c>true</c> when the color space uses a PQ or HLG transfer function.
    /// </summary>
    public static bool IsHdrColorSpace(SKColorSpace? cs)
    {
        if (cs is null || cs.IsSrgb || cs.GammaIsLinear) return false;
        if (cs.GammaIsCloseToSrgb) return false;
        if (cs.GetNumericalTransferFunction(out _)) return false;
        return true;
    }

    #endregion // Public Methods



    #region Private Methods

    /// <summary>
    /// Tone mapping pipeline with two strategies:
    /// <para><b>PQ/HLG path</b> (wide-gamut Rec.2020):</para>
    /// 1. Linearize via Skia color-space conversion to linear Rec.2020.
    /// 2. Normalize so reference white = 1.0 (PQ) or keep 1.0 (HLG).
    /// 3. Apply tone curve on <b>luminance</b>, scale RGB proportionally.
    /// 4. Gamut-map Rec.2020 -> sRGB via 3×3 matrix.
    /// 5. Encode to sRGB gamma.
    /// <para><b>Linear sRGB path</b> (EXR, Radiance HDR, scRGB):</para>
    /// 1. Read float pixels directly (already linear sRGB).
    /// 2. Apply tone curve <b>per-channel</b> independently.
    /// 3. Encode to sRGB gamma.
    /// <para>
    /// The two strategies exist because:
    /// <list type="bullet">
    /// <item>Per-channel avoids overflow artifacts in sRGB space (no skyblue
    /// tint on near-white, no blue->white wash-out).</item>
    /// <item>Luminance-based preserves channel ratios that the Rec.2020->sRGB
    /// gamut matrix needs; per-channel would compress all channels toward 1.0,
    /// making the matrix output near-white.</item>
    /// </list>
    /// </para>
    /// </summary>
    private static unsafe SKImage? ToneMapManual(SKImage source,
        HdrTransferFunction transferFn, HdrToneMappingOptions options, double contentPeakNits)
    {
        var isLinearSrgb = transferFn is HdrTransferFunction.None or HdrTransferFunction.Linear
            or HdrTransferFunction.ScRgb;

        // ── Step 1: linearize source into float pixels ──
        using var linearBmp = LinearizeToFloat(source, isLinearSrgb);
        if (linearBmp is null) return null;

        // one pass covers both the SDR check and the peak, for a source that declares none
        var (maxSample, measuredPeak) = MeasureLinearPeak(linearBmp);

        // nothing above diffuse white means no HDR range, so mapping it would only darken an SDR image
        if (isLinearSrgb && maxSample <= DiffuseWhiteThreshold) return null;

        // ── Step 1b: build the tone curve around the peak this image actually has ──
        var compression = Math.Clamp((float)options.HighlightCompression, 0f, 1f);
        var saturation = Math.Clamp((float)options.Saturation, 0f, 2f);
        var whiteLevel = ComputeToneCurveWhiteLevel(transferFn, options,
            contentPeakNits, measuredPeak, compression);

        var toneCurve = ToneCurve.For(options.Mode, whiteLevel, compression);
        if (toneCurve is null) return null;

        var width = linearBmp.Width;
        var height = linearBmp.Height;

        // ── Step 2: allocate output (linear sRGB, RgbaF32) ──
        var outputInfo = new SKImageInfo(width, height,
            SKColorType.RgbaF32, SKAlphaType.Unpremul, SKColorSpace.CreateSrgbLinear());
        using var outputBmp = new SKBitmap(outputInfo);

        var srcPtr = (byte*)linearBmp.GetPixels();
        var dstPtr = (byte*)outputBmp.GetPixels();
        var srcRowBytes = linearBmp.RowBytes;
        var dstRowBytes = outputBmp.RowBytes;

        // ── Step 3: normalization and exposure ──
        var normScale = ComputeNormScale(transferFn, options);

        // ── Step 4: per-pixel tone mapping ──
        if (isLinearSrgb)
        {
            ToneMapPerChannel(srcPtr, srcRowBytes, dstPtr, dstRowBytes,
                width, height, normScale, saturation, toneCurve.Value);
        }
        else
        {
            ToneMapLuminanceBased(srcPtr, srcRowBytes, dstPtr, dstRowBytes,
                width, height, normScale, saturation, toneCurve.Value);
        }

        // ── Step 5: convert linear sRGB float -> final sRGB Rgba8888 ──
        return ConvertToFinalSrgb(outputBmp);
    }


    /// <summary>
    /// Linearizes the source image into an <see cref="SKColorType.RgbaF32"/> bitmap.
    /// For sRGB-primaries content (EXR/HDR/scRGB), the target is linear sRGB.
    /// For PQ/HLG content, the target is linear Rec.2020.
    /// </summary>
    /// <returns>An <see cref="SKBitmap"/> owning the linearized pixels,
    /// or <c>null</c> on failure. Caller owns disposal.</returns>
    private static SKBitmap? LinearizeToFloat(SKImage source, bool isLinearSrgb)
    {
        var targetCs = isLinearSrgb
            ? SKColorSpace.CreateSrgbLinear()
            : SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Linear, SKColorSpaceXyz.Rec2020);

        var linearInfo = new SKImageInfo(source.Width, source.Height,
            SKColorType.RgbaF32, SKAlphaType.Unpremul, targetCs);

        var bitmap = new SKBitmap();
        var isAllocated = bitmap.TryAllocPixels(linearInfo);
        if (!isAllocated)
        {
            bitmap.Dispose();
            return null;
        }

        var isDrawn = SkiaCodec.TryDrawInBands(source, bitmap);
        if (!isDrawn)
        {
            bitmap.Dispose();
            return null;
        }

        return bitmap;
    }


    /// <summary>
    /// Per-thread accumulator for <see cref="MeasureLinearPeak"/>.
    /// </summary>
    private sealed class PeakScan
    {
        public readonly int[] Histogram = new int[PeakBuckets + 1];
        public float Max;
    }


    /// <summary>
    /// Measures the linearized image in one parallel pass: the true maximum sample (which tells real
    /// HDR range from linear SDR) and the <see cref="PeakPercentile"/> sample used as the peak.
    /// </summary>
    private static unsafe (float Max, float Peak) MeasureLinearPeak(SKBitmap linearBmp)
    {
        var basePtr = (nint)linearBmp.GetPixels();
        var rowBytes = linearBmp.RowBytes;
        var width = linearBmp.Width;

        var merged = new int[PeakBuckets + 1];
        var maxSample = 0f;
        var mergeLock = new Lock();

        Parallel.For(0, linearBmp.Height, () => new PeakScan(), (y, _, scan) =>
        {
            var row = (float*)((byte*)basePtr + (long)y * rowBytes);

            for (var x = 0; x < width; x++)
            {
                var i = x * 4;
                var v = MathF.Max(row[i], MathF.Max(row[i + 1], row[i + 2]));
                if (v > scan.Max) scan.Max = v;

                var bucket = v <= 0f ? 0 : (int)(v * PeakBucketsPerUnit);
                scan.Histogram[Math.Min(bucket, PeakBuckets)]++;
            }

            return scan;
        },
        scan =>
        {
            lock (mergeLock)
            {
                if (scan.Max > maxSample) maxSample = scan.Max;
                for (var b = 0; b <= PeakBuckets; b++) merged[b] += scan.Histogram[b];
            }
        });

        long total = 0;
        for (var b = 0; b <= PeakBuckets; b++) total += merged[b];

        // walk down from the brightest bucket until more than the allowed share sits above it
        var allowedAbove = (long)(total * (1d - PeakPercentile));
        long above = 0;
        for (var b = PeakBuckets; b > 0; b--)
        {
            above += merged[b];
            if (above <= allowedAbove) continue;

            var peak = (b + 1) / (float)PeakBucketsPerUnit;
            return (maxSample, MathF.Min(peak, maxSample));
        }

        return (maxSample, maxSample);
    }


    /// <summary>
    /// Computes the combined normalization scale from white point + exposure EV.
    /// </summary>
    private static float ComputeNormScale(HdrTransferFunction transferFn, HdrToneMappingOptions options)
    {
        var whiteNits = ClampReferenceWhiteNits(options);
        var normScale = EncodingWhiteNits(transferFn) / whiteNits;

        // Exposure in EV stops: 0 = no change, +1 = 2×, -1 = 0.5×.
        var exposure = (float)options.Exposure;
        if (exposure != 0f)
        {
            normScale *= MathF.Pow(2f, exposure);
        }

        return normScale;
    }


    /// <summary>
    /// Per-channel tone mapping for linear sRGB-primaries content.
    /// Each channel is independently compressed — avoids overflow artifacts.
    /// </summary>
    private static unsafe void ToneMapPerChannel(
        byte* srcPtr, int srcRowBytes, byte* dstPtr, int dstRowBytes,
        int width, int height, float normScale, float saturation,
        ToneCurve toneCurve)
    {
        var applySaturation = MathF.Abs(saturation - 1f) > 1e-4f;

        // parallelize per row (pointers captured as nint; lambdas can't capture pointer types)
        var srcBase = (nint)srcPtr;
        var dstBase = (nint)dstPtr;
        Parallel.For(0, height, y =>
        {
            var srcRow = (float*)((byte*)srcBase + (long)y * srcRowBytes);
            var dstRow = (float*)((byte*)dstBase + (long)y * dstRowBytes);

            // four pixels per step, then the rest one at a time
            var x = 0;
            if (Vector128.IsHardwareAccelerated)
            {
                for (; x <= width - 4; x += 4)
                {
                    ToneMapPerChannel4(srcRow + x * 4, dstRow + x * 4,
                        normScale, saturation, applySaturation, toneCurve);
                }
            }

            for (; x < width; x++)
            {
                var i = x * 4;
                var r = srcRow[i] * normScale;
                var g = srcRow[i + 1] * normScale;
                var b = srcRow[i + 2] * normScale;
                var a = srcRow[i + 3];

                if (r > 0f || g > 0f || b > 0f)
                {
                    r = toneCurve.Map(r);
                    g = toneCurve.Map(g);
                    b = toneCurve.Map(b);
                }

                // Saturation: lerp toward luminance gray in linear sRGB
                if (applySaturation)
                {
                    var lum = Lum709R * r + Lum709G * g + Lum709B * b;
                    r = lum + saturation * (r - lum);
                    g = lum + saturation * (g - lum);
                    b = lum + saturation * (b - lum);
                }

                dstRow[i] = Math.Clamp(r, 0f, 1f);
                dstRow[i + 1] = Math.Clamp(g, 0f, 1f);
                dstRow[i + 2] = Math.Clamp(b, 0f, 1f);
                dstRow[i + 3] = Math.Clamp(a, 0f, 1f);
            }
        });
    }


    /// <summary>
    /// Luminance-based tone mapping for wide-gamut Rec.2020 content (PQ/HLG).
    /// Preserves channel ratios for correct Rec.2020 -> sRGB gamut mapping.
    /// </summary>
    private static unsafe void ToneMapLuminanceBased(
        byte* srcPtr, int srcRowBytes, byte* dstPtr, int dstRowBytes,
        int width, int height, float normScale, float saturation,
        ToneCurve toneCurve)
    {
        var applySaturation = MathF.Abs(saturation - 1f) > 1e-4f;

        // parallelize per row (pointers captured as nint; lambdas can't capture pointer types)
        var srcBase = (nint)srcPtr;
        var dstBase = (nint)dstPtr;
        Parallel.For(0, height, y =>
        {
            var srcRow = (float*)((byte*)srcBase + (long)y * srcRowBytes);
            var dstRow = (float*)((byte*)dstBase + (long)y * dstRowBytes);

            // four pixels per step, then the rest one at a time
            var x = 0;
            if (Vector128.IsHardwareAccelerated)
            {
                for (; x <= width - 4; x += 4)
                {
                    ToneMapLuminanceBased4(srcRow + x * 4, dstRow + x * 4,
                        normScale, saturation, applySaturation, toneCurve);
                }
            }

            for (; x < width; x++)
            {
                var i = x * 4;
                var r = srcRow[i] * normScale;
                var g = srcRow[i + 1] * normScale;
                var b = srcRow[i + 2] * normScale;
                var a = srcRow[i + 3];

                var lum = Lum2020R * r + Lum2020G * g + Lum2020B * b;

                if (lum > 0f)
                {
                    var scale = toneCurve.Map(lum) / lum;
                    r *= scale;
                    g *= scale;
                    b *= scale;
                }
                else
                {
                    r = 0f; g = 0f; b = 0f;
                }

                // Gamut map: Rec.2020 linear -> sRGB linear
                var sr = M00 * r + M01 * g + M02 * b;
                var sg = M10 * r + M11 * g + M12 * b;
                var sb = M20 * r + M21 * g + M22 * b;

                // Saturation: lerp toward luminance gray in sRGB linear
                if (applySaturation)
                {
                    var srgbLum = Lum709R * sr + Lum709G * sg + Lum709B * sb;
                    sr = srgbLum + saturation * (sr - srgbLum);
                    sg = srgbLum + saturation * (sg - srgbLum);
                    sb = srgbLum + saturation * (sb - srgbLum);
                }

                dstRow[i] = Math.Clamp(sr, 0f, 1f);
                dstRow[i + 1] = Math.Clamp(sg, 0f, 1f);
                dstRow[i + 2] = Math.Clamp(sb, 0f, 1f);
                dstRow[i + 3] = Math.Clamp(a, 0f, 1f);
            }
        });
    }


    /// <summary>
    /// <see cref="ToneMapPerChannel"/>'s pixel body on four pixels, op for op, so it writes the same bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ToneMapPerChannel4(float* src, float* dst,
        float normScale, float saturation, bool applySaturation, in ToneCurve toneCurve)
    {
        LoadPlanes(src, out var r, out var g, out var b, out var a);
        r *= normScale;
        g *= normScale;
        b *= normScale;

        // a pixel with any channel above zero maps all three
        var isRedLit = Vector128.GreaterThan(r, Vector128<float>.Zero);
        var isGreenLit = Vector128.GreaterThan(g, Vector128<float>.Zero);
        var isBlueLit = Vector128.GreaterThan(b, Vector128<float>.Zero);
        var isLit = isRedLit | isGreenLit | isBlueLit;

        var mappedR = toneCurve.Map(r);
        var mappedG = toneCurve.Map(g);
        var mappedB = toneCurve.Map(b);
        r = Vector128.ConditionalSelect(isLit, mappedR, r);
        g = Vector128.ConditionalSelect(isLit, mappedG, g);
        b = Vector128.ConditionalSelect(isLit, mappedB, b);

        if (applySaturation)
        {
            var lum = Lum709R * r + Lum709G * g + Lum709B * b;
            r = lum + saturation * (r - lum);
            g = lum + saturation * (g - lum);
            b = lum + saturation * (b - lum);
        }

        var clampedR = Clamp01(r);
        var clampedG = Clamp01(g);
        var clampedB = Clamp01(b);
        var clampedA = Clamp01(a);
        StorePlanes(dst, clampedR, clampedG, clampedB, clampedA);
    }


    /// <summary>
    /// <see cref="ToneMapLuminanceBased"/>'s pixel body on four pixels, op for op, so it writes the same bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ToneMapLuminanceBased4(float* src, float* dst,
        float normScale, float saturation, bool applySaturation, in ToneCurve toneCurve)
    {
        LoadPlanes(src, out var r, out var g, out var b, out var a);
        r *= normScale;
        g *= normScale;
        b *= normScale;

        var lum = Lum2020R * r + Lum2020G * g + Lum2020B * b;

        // an unlit pixel goes black, so its lanes of the division are dropped
        var isLit = Vector128.GreaterThan(lum, Vector128<float>.Zero);
        var mapped = toneCurve.Map(lum);
        var scale = mapped / lum;
        r = Vector128.ConditionalSelect(isLit, r * scale, Vector128<float>.Zero);
        g = Vector128.ConditionalSelect(isLit, g * scale, Vector128<float>.Zero);
        b = Vector128.ConditionalSelect(isLit, b * scale, Vector128<float>.Zero);

        var sr = M00 * r + M01 * g + M02 * b;
        var sg = M10 * r + M11 * g + M12 * b;
        var sb = M20 * r + M21 * g + M22 * b;

        if (applySaturation)
        {
            var srgbLum = Lum709R * sr + Lum709G * sg + Lum709B * sb;
            sr = srgbLum + saturation * (sr - srgbLum);
            sg = srgbLum + saturation * (sg - srgbLum);
            sb = srgbLum + saturation * (sb - srgbLum);
        }

        var clampedR = Clamp01(sr);
        var clampedG = Clamp01(sg);
        var clampedB = Clamp01(sb);
        var clampedA = Clamp01(a);
        StorePlanes(dst, clampedR, clampedG, clampedB, clampedA);
    }


    /// <summary>
    /// Splits four RGBA float pixels at <paramref name="src"/> into one vector per channel.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void LoadPlanes(float* src, out Vector128<float> r, out Vector128<float> g,
        out Vector128<float> b, out Vector128<float> a)
    {
        var p0 = Vector128.Load(src);
        var p1 = Vector128.Load(src + 4);
        var p2 = Vector128.Load(src + 8);
        var p3 = Vector128.Load(src + 12);

        // the even lanes of two RGBA pixels are R and B, the odd lanes G and A
        var rb01 = Vector128.UnzipEven(p0, p1);
        var ga01 = Vector128.UnzipOdd(p0, p1);
        var rb23 = Vector128.UnzipEven(p2, p3);
        var ga23 = Vector128.UnzipOdd(p2, p3);

        r = Vector128.UnzipEven(rb01, rb23);
        b = Vector128.UnzipOdd(rb01, rb23);
        g = Vector128.UnzipEven(ga01, ga23);
        a = Vector128.UnzipOdd(ga01, ga23);
    }


    /// <summary>
    /// Interleaves one vector per channel back into four RGBA float pixels at <paramref name="dst"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void StorePlanes(float* dst, Vector128<float> r, Vector128<float> g,
        Vector128<float> b, Vector128<float> a)
    {
        var rb01 = Vector128.ZipLower(r, b);
        var rb23 = Vector128.ZipUpper(r, b);
        var ga01 = Vector128.ZipLower(g, a);
        var ga23 = Vector128.ZipUpper(g, a);

        var p0 = Vector128.ZipLower(rb01, ga01);
        var p1 = Vector128.ZipUpper(rb01, ga01);
        var p2 = Vector128.ZipLower(rb23, ga23);
        var p3 = Vector128.ZipUpper(rb23, ga23);

        p0.Store(dst);
        p1.Store(dst + 4);
        p2.Store(dst + 8);
        p3.Store(dst + 12);
    }


    /// <summary>
    /// <see cref="Math.Clamp(float, float, float)"/> to [0, 1] on each lane, NaN and -0 kept as it keeps them.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> Clamp01(Vector128<float> v)
    {
        var isBelow = Vector128.LessThan(v, Vector128<float>.Zero);
        var isAbove = Vector128.GreaterThan(v, Vector128<float>.One);
        var raised = Vector128.ConditionalSelect(isBelow, Vector128<float>.Zero, v);

        return Vector128.ConditionalSelect(isAbove, Vector128<float>.One, raised);
    }


    /// <summary>
    /// Converts a linear sRGB <see cref="SKColorType.RgbaF32"/> bitmap to a final
    /// sRGB <see cref="SKColorType.Rgba8888"/> image. Monitor profile is applied
    /// by the caller.
    /// </summary>
    private static SKImage? ConvertToFinalSrgb(SKBitmap linearBmp)
    {
        // the tone-mapped bitmap is finished, so the image shares its pixels instead of copying them
        using var linearImg = SkiaCodec.ToSKImageNoCopy(linearBmp);
        if (linearImg is null) return null;

        var finalInfo = new SKImageInfo(linearBmp.Width, linearBmp.Height,
            SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        using var finalBmp = new SKBitmap();
        var isAllocated = finalBmp.TryAllocPixels(finalInfo);
        if (!isAllocated) return null;

        var isDrawn = SkiaCodec.TryDrawInBands(linearImg, finalBmp);
        if (!isDrawn) return null;

        return SkiaCodec.ToSKImageNoCopy(finalBmp);
    }


    /// <summary>
    /// Re-wraps an existing image with a different color space without modifying pixel data.
    /// </summary>
    private static SKImage? ReinterpretColorSpace(SKImage source, SKColorSpace newColorSpace)
    {
        using var pixmap = source.PeekPixels();
        if (pixmap is null) return null;

        var newInfo = pixmap.Info.WithColorSpace(newColorSpace);
        using var reinterpreted = new SKPixmap(newInfo, pixmap.GetPixels(), pixmap.RowBytes);

        return SKImage.FromPixelCopy(reinterpreted);
    }


    /// <summary>
    /// Builds an HDR color space from the transfer function and the source gamut.
    /// </summary>
    private static SKColorSpace? BuildHdrColorSpace(HdrTransferFunction transferFn, SKColorSpace? sourceCs)
    {
        var gamut = SKColorSpaceXyz.Rec2020;
        if (sourceCs?.ToColorSpaceXyz(out var sourceXyz) == true)
        {
            gamut = sourceXyz;
        }

        return transferFn switch
        {
            HdrTransferFunction.PQ => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Pq, gamut),
            HdrTransferFunction.HLG => SKColorSpace.CreateRgb(SKColorSpaceTransferFn.Hlg, gamut),
            _ => null,
        };
    }


    /// <summary>
    /// The reference-white override in nits, clamped to the range the HDR tool's slider offers.
    /// </summary>
    private static float ClampReferenceWhiteNits(HdrToneMappingOptions options)
        => Math.Clamp((float)options.ReferenceWhiteNits, 50f, 1000f);


    /// <summary>
    /// The luminance a linearized 1.0 stands for: Skia decodes PQ and HLG against BT.2408
    /// reference white, scRGB pins 1.0 to 80 nits, scene-referred calls 1.0 diffuse white.
    /// </summary>
    private static float EncodingWhiteNits(HdrTransferFunction transferFn) => transferFn switch
    {
        HdrTransferFunction.ScRgb => ScRgbWhiteNits,
        _ => Bt2408ReferenceWhiteNits,
    };


    /// <summary>
    /// Input level, in reference-white units, that the BT.2408 curve maps to SDR white. Taken from
    /// the file's declared peak so a 400-nit and a 4000-nit grade each land on white, not on a guess.
    /// </summary>
    /// <param name="compression">0 = white level at the declared peak, 1 = 4x it (max compression).</param>
    private static float ComputeToneCurveWhiteLevel(HdrTransferFunction transferFn,
        HdrToneMappingOptions options, double contentPeakNits, float measuredPeak, float compression)
    {
        var whiteNits = ClampReferenceWhiteNits(options);

        // metadata wins; failing that the samples are absolute, so the brightest one IS the peak
        var peakNits = contentPeakNits > 0d
            ? contentPeakNits
            : measuredPeak * EncodingWhiteNits(transferFn);

        var baseLevel = peakNits > 0d
            ? Math.Clamp((float)(peakNits / whiteNits),
                MinToneCurveWhiteLevel, MaxToneCurveWhiteLevel)
            : FallbackToneCurveWhiteLevel;

        // headroom past the peak, since a grade can exceed both its metadata and its own p99.99
        return baseLevel * (1f + 3f * compression);
    }


    /// <summary>
    /// Reference-white anchored roll-off (extended Reinhard), reaching SDR white exactly at
    /// <paramref name="whiteLevel"/> so content below it compresses instead of clipping.
    /// </summary>
    /// <param name="whiteLevel">Input level, in reference-white units, that maps to SDR white.</param>
    private static float ReferenceWhiteToneMap(float v, float whiteLevel)
    {
        // A knee curve is wrong here: real HDR grades put a lot of content above reference white
        // (29% on a 1164-nit sample), and a tight knee clips all of it to white.
        if (v <= 0f) return 0f;

        var mapped = v * (1f + v / (whiteLevel * whiteLevel)) / (1f + v);

        return MathF.Min(1f, mapped);
    }


    /// <summary>
    /// <see cref="ReferenceWhiteToneMap(float, float)"/> on four lanes, op for op, so both give the same bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> ReferenceWhiteToneMap(Vector128<float> v, float whiteLevel)
    {
        var one = Vector128<float>.One;
        var mapped = v * (one + v / Vector128.Create(whiteLevel * whiteLevel)) / (one + v);
        var clipped = Vector128.Min(one, mapped);
        var isNotPositive = Vector128.LessThanOrEqual(v, Vector128<float>.Zero);

        return Vector128.ConditionalSelect(isNotPositive, Vector128<float>.Zero, clipped);
    }


    /// <summary>
    /// Extended Reinhard with wide shoulder (no discontinuity).
    /// Trades SDR brightness for significantly more highlight detail.
    /// </summary>
    /// <param name="compression">0 = default knee at 0.7, 1 = knee at 0.3 (max compression).</param>
    private static float ExtendedReinhardToneMap(float v, float compression)
    {
        if (v <= 0f) return 0f;

        // Knee slides from 0.7 (no compression) to 0.3 (max compression)
        var kneeStart = 0.7f - 0.4f * compression;
        const float maxOut = 1.0f;
        var range = maxOut - kneeStart;

        if (v <= kneeStart) return v;

        float excess = v - kneeStart;
        return kneeStart + range * excess / (excess + range);
    }


    /// <summary>
    /// ACES-style filmic curve with wide shoulder (no discontinuity).
    /// Cinematic rolloff — punchier than Reinhard, more highlight headroom than BT.2408.
    /// </summary>
    /// <param name="compression">0 = default knee at 0.5, 1 = knee at 0.1 (max compression).</param>
    private static float AcesToneMap(float v, float compression)
    {
        if (v <= 0f) return 0f;

        // Knee slides from 0.5 (no compression) to 0.1 (max compression)
        var kneeStart = 0.5f - 0.4f * compression;
        const float maxOut = 1.0f;
        var range = maxOut - kneeStart;

        if (v <= kneeStart) return v;

        float excess = v - kneeStart;
        return kneeStart + range * MathF.Tanh(excess / range);
    }


    /// <summary>
    /// The tone curve of one <see cref="HdrToneMappingMode"/>, on a single sample or on four lanes at once.
    /// </summary>
    private readonly struct ToneCurve
    {
        private readonly HdrToneMappingMode _mode;
        private readonly float _whiteLevel;
        private readonly float _compression;


        private ToneCurve(HdrToneMappingMode mode, float whiteLevel, float compression)
        {
            _mode = mode;
            _whiteLevel = whiteLevel;
            _compression = compression;
        }


        /// <summary>
        /// The curve <paramref name="mode"/> maps with, or <c>null</c> when it has none.
        /// </summary>
        public static ToneCurve? For(HdrToneMappingMode mode, float whiteLevel, float compression) => mode switch
        {
            HdrToneMappingMode.BT2408 or HdrToneMappingMode.Reinhard or HdrToneMappingMode.ACES
                => new ToneCurve(mode, whiteLevel, compression),
            _ => null,
        };


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Map(float v) => _mode switch
        {
            HdrToneMappingMode.BT2408 => ReferenceWhiteToneMap(v, _whiteLevel),
            HdrToneMappingMode.Reinhard => ExtendedReinhardToneMap(v, _compression),
            _ => AcesToneMap(v, _compression),
        };


        /// <summary>
        /// Maps four samples, each to exactly what <see cref="Map(float)"/> gives it.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector128<float> Map(Vector128<float> v)
        {
            if (_mode == HdrToneMappingMode.BT2408) return ReferenceWhiteToneMap(v, _whiteLevel);

            // the knee curves stay per lane, so each keeps one definition (and ACES needs tanh)
            var lane0 = Map(v.GetElement(0));
            var lane1 = Map(v.GetElement(1));
            var lane2 = Map(v.GetElement(2));
            var lane3 = Map(v.GetElement(3));

            return Vector128.Create(lane0, lane1, lane2, lane3);
        }
    }


    #endregion // Private Methods

}


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
using ImageGlass.Common.Types;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ImageGlass.Common.ServiceProviders.Update;


/// <summary>
/// Builds the <c>User-Agent</c> for the update check. The scheduled check carries a few coarse,
/// runtime-derived tokens that the server counts in aggregate; nothing is stored on the device
/// for this, and no identifier of any kind is emitted.
/// <para>
/// Example: <c>ImageGlass/10.0.0.306 (Windows 11; x64; msix; beta; vi; pro; gap=7; new)</c>
/// </para>
/// </summary>
public static partial class UsageStatsAgent
{
    [GeneratedRegex(@"^[a-zA-Z]{2,3}(-[a-zA-Z]{2,8}){0,2}$", RegexOptions.CultureInvariant)]
    private static partial Regex LangTagRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();


    /// <summary>
    /// Reported when the app version is unavailable.
    /// </summary>
    private const string UNKNOWN_VERSION = "0.0.0.0";

    /// <summary>
    /// Emitted for a language pack whose code is not one we publish.
    /// </summary>
    private const string CUSTOM_LANG = "custom";

    /// <summary>
    /// Bounds for the reported gap, in days. A value outside this range is dropped, not clamped:
    /// a skewed clock must not poison the server's mean.
    /// </summary>
    private const int MIN_GAP_DAYS = 1;
    private const int MAX_GAP_DAYS = 400;

    /// <summary>
    /// Windows builds at or above this are Windows 11.
    /// </summary>
    private const int WIN11_MIN_BUILD = 22000;

    /// <summary>
    /// macOS majors outside this range report <c>other</c>.
    /// </summary>
    private const int MACOS_MIN_MAJOR = 10;
    private const int MACOS_MAX_MAJOR = 99;


    /// <summary>
    /// donottrack.sh convention: <c>DO_NOT_TRACK=1</c> opts out of usage reporting.
    /// Read once because env vars cannot change mid-process.
    /// </summary>
    public static bool IsDoNotTrackSet { get; } = ReadDoNotTrack();


    /// <summary>
    /// ISO 639-1 primary language subtags. A pack code whose primary subtag is not in here reports
    /// <see cref="CUSTOM_LANG"/>, so a user-authored pack can never put arbitrary text in the header.
    /// Fixed list, no maintenance: new language packs reuse these codes.
    /// </summary>
    private static readonly FrozenSet<string> _iso639 = (
        "aa ab ae af ak am an ar as av ay az ba be bg bh bi bm bn bo br bs ca ce ch co cr cs cu cv " +
        "cy da de dv dz ee el en eo es et eu fa ff fi fj fo fr fy ga gd gl gn gu gv ha he hi ho hr " +
        "ht hu hy hz ia id ie ig ii ik io is it iu ja jv ka kg ki kj kk kl km kn ko kr ks ku kv kw " +
        "ky la lb lg li ln lo lt lu lv mg mh mi mk ml mn mr ms mt my na nb nd ne ng nl nn no nr nv " +
        "ny oc oj om or os pa pi pl ps pt qu rm rn ro ru rw sa sc sd se sg si sk sl sm sn so sq sr " +
        "ss st su sv sw ta te tg th ti tk tl tn to tr ts tt tw ty ug uk ur uz ve vi vo wa wo xh yi " +
        "yo za zh zu")
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .ToFrozenSet(StringComparer.Ordinal);


    /// <summary>
    /// Builds the <c>User-Agent</c> value.
    /// </summary>
    /// <param name="withStats">
    /// <c>true</c> only for the scheduled background check. A manual check, or any build where
    /// <see cref="IsDoNotTrackSet"/> is on, sends the bare product token instead.
    /// </param>
    public static string Build(bool withStats)
    {
        var version = Core.BuildInfo?.Version;
        if (string.IsNullOrWhiteSpace(version)) version = UNKNOWN_VERSION;

        var product = $"ImageGlass/{version}";
        if (!withStats || IsDoNotTrackSet) return product;

        try
        {
            var tokens = BuildTokens();
            if (tokens.Count == 0) return product;

            return $"{product} ({string.Join("; ", tokens)})";
        }
        catch
        {
            // statistics must never be able to break the update check
            return product;
        }
    }


    /// <summary>
    /// Builds the comment-field tokens, in a fixed order so the server can parse positionally.
    /// </summary>
    private static List<string> BuildTokens()
    {
        var tokens = new List<string>(8)
        {
            GetOsToken(),
            GetArchToken(),
            SanitizeToken(Core.ShellProvider?.InstallChannelId) ?? "other",
            SanitizeToken(Core.BuildInfo?.ReleaseType) ?? "stable",
            GetLangToken(),
            Core.IsProEnabled ? "pro" : "classic",
        };

        // a first check has no interval; the bookmark still holds its built-in default, and
        // reporting that as a gap would feed a value the server would average as if it were real
        if (IsFirstCheck())
        {
            tokens.Add("new");
        }
        else if (GetGapDays() is int gap)
        {
            tokens.Add($"gap={gap}");
        }

        return tokens;
    }


    /// <summary>
    /// OS name plus a bucketed version. Linux reports no version: distro strings are high-entropy
    /// and the packaging signal is already carried by the install channel.
    /// </summary>
    private static string GetOsToken()
    {
        var v = Environment.OSVersion.Version;

        return BHelper.OS switch
        {
            OSType.Windows => v.Build >= WIN11_MIN_BUILD
                ? "Windows 11"
                : v.Major == 10 ? "Windows 10" : "Windows legacy",
            OSType.Mac => v.Major is >= MACOS_MIN_MAJOR and <= MACOS_MAX_MAJOR
                ? $"macOS {v.Major}"
                : "macOS other",
            OSType.Linux => "Linux",
            _ => "other",
        };
    }


    private static string GetArchToken() => BHelper.ArchToken;


    /// <summary>
    /// The UI language as an ISO 639-1 subtag, keeping only a script subtag where one is present
    /// (<c>zh-Hans</c>). The region is dropped: it adds entropy and changes no decision.
    /// Never reads <see cref="Localization.Lang.FileName"/>, which is a user-authored filename and
    /// can contain a person's or company's name.
    /// </summary>
    private static string GetLangToken()
    {
        var code = Core.Lang?.Metadata?.Code;
        if (string.IsNullOrWhiteSpace(code)) return CUSTOM_LANG;

        code = code.Trim();
        if (code.Length > 12 || !LangTagRegex().IsMatch(code)) return CUSTOM_LANG;

        var parts = code.Split('-');
        var primary = parts[0].ToLowerInvariant();
        if (!_iso639.Contains(primary)) return CUSTOM_LANG;

        // a 4-letter subtag is a script (Hans/Hant); anything else is a region and is discarded
        var script = Array.Find(parts, p => p.Length == 4);
        if (script is null) return primary;

        return $"{primary}-{char.ToUpperInvariant(script[0])}{script[1..].ToLowerInvariant()}";
    }


    /// <summary>
    /// Whole days since the last recorded check, or <c>null</c> when the bookmark is missing,
    /// unparseable, or implies a skewed clock.
    /// </summary>
    private static int? GetGapDays()
    {
        var raw = Core.Config?.AutoUpdate;
        if (string.IsNullOrEmpty(raw) || raw == "0") return null;
        if (!DateTime.TryParse(raw, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var last)) return null;

        var days = (int)Math.Round((DateTime.UtcNow - last.ToUniversalTime()).TotalDays);
        return days is >= MIN_GAP_DAYS and <= MAX_GAP_DAYS ? days : null;
    }


    /// <summary>
    /// Whether the updater has never recorded a check. Reads only whether the key was persisted;
    /// the default value cannot be told apart from a written one by its content alone.
    /// </summary>
    private static bool IsFirstCheck() => Core.Config?.HasValue(ConfigId.AutoUpdate) == false;


    /// <summary>
    /// Strips anything that could break the header grammar. Returns <c>null</c> when nothing is left.
    /// </summary>
    private static string? SanitizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = value.Trim();
        if (trimmed.Length > 20 || !TokenRegex().IsMatch(trimmed)) return null;

        return trimmed.ToLowerInvariant();
    }


    private static bool ReadDoNotTrack()
    {
        try
        {
            var value = Environment.GetEnvironmentVariable("DO_NOT_TRACK")?.Trim();
            return string.Equals(value, "1", StringComparison.Ordinal)
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }



}

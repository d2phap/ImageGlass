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
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImageGlass.Common;

public partial class BHelper
{
    /// <summary>
    /// Gets app name.
    /// </summary>
    public static string AppName { get; } = "ImageGlass";

    /// <summary>
    /// Gets app display name.
    /// </summary>
    public static string AppDisplayName { get; } = "ImageGlass 10";


    /// <summary>
    /// Gets the app executable file path.
    /// </summary>
    public static string AppExePath => Environment.ProcessPath ?? "";


    /// <summary>
    /// Gets the path to relaunch the app with: the .AppImage file, never the binary inside its
    /// mount, which is unmounted as soon as the exiting process dies.
    /// </summary>
    public static string AppRelaunchPath =>
        Environment.GetEnvironmentVariable("APPIMAGE") is { Length: > 0 } appImage ? appImage : AppExePath;


    /// <summary>
    /// Gets the type of operating system.
    /// </summary>
    public static OSType OS { get; } = GetOS();
    private static OSType GetOS()
    {
        if (OperatingSystem.IsLinux()) return OSType.Linux;
        if (OperatingSystem.IsMacOS()) return OSType.Mac;
        if (OperatingSystem.IsWindows()) return OSType.Windows;
        return OSType.Unknown;
    }


    /// <summary>
    /// Architecture token (<c>x64</c>/<c>arm64</c>/<c>x86</c>/<c>other</c>), shared by usage stats and update artifact keys.
    /// </summary>
    public static string ArchToken { get; } = RuntimeInformation.OSArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        Architecture.X86 => "x86",
        _ => "other",
    };

    /// <summary>
    /// Gets a value indicating whether the current operating system is Windows 10.
    /// </summary>
    public static bool IsWindows10 { get; } = Environment.OSVersion.Version.Major == 10
        && Environment.OSVersion.Version.Build < 22000;


    /// <summary>
    /// Generates a list of unique indexes within a specified range,
    /// wrapping around the center index, in the Center-Right-Left order.
    /// Example:
    /// <list type="bullet">
    ///   <item><c>GenerateWrappedIndexes(0, 2, 10, true) => [0, 1, 9, 2, 8]</c></item>
    ///   <item><c>GenerateWrappedIndexes(0, 2, 10, false) => [1, 9, 2, 8]</c></item>
    ///   <item><c>GenerateWrappedIndexes(0, 2, 1, true) => [0]</c></item>
    ///   <item><c>GenerateWrappedIndexes(-1, 2, 10, true) => []</c></item>
    /// </list>
    /// </summary>
    public static List<int> GenerateWrappedIndexes(int centerIndex, uint range, uint count, bool includeCenterIndex)
    {
        if (centerIndex < 0 || centerIndex > count - 1) return [];

        var unloadSet = new HashSet<int>();

        // include the center index
        if (includeCenterIndex)
        {
            unloadSet.Add(centerIndex);
        }

        // generate range [-range, centerIndex, +range]
        for (var i = 1; i <= range; i++)
        {
            var rightIndex = ComputeIndexInRange(centerIndex + i, count, true);
            var leftIndex = ComputeIndexInRange(centerIndex - i, count, true);

            unloadSet.Add(rightIndex);
            unloadSet.Add(leftIndex);
        }

        return unloadSet.ToList();
    }

    /// <summary>
    /// <para>
    /// Calculates a valid index within a circular range,
    /// ensure the index stays within <c>[0, count-1]</c>.
    /// </para>
    /// 
    /// <example>
    /// When loopIndex == true:
    /// <code>
    /// CalculateIndexInRange(1, 10, true); // => 1
    /// CalculateIndexInRange(0, 10, true); // => 0
    /// CalculateIndexInRange(-1, 10, true); // => 9
    /// CalculateIndexInRange(-2, 10, true); // => 8
    /// CalculateIndexInRange(-2, 0, true); // => 0
    /// </code>
    /// 
    /// When loopIndex == false:
    /// <code>
    /// CalculateIndexInRange(-2, 10, false); // => 0
    /// CalculateIndexInRange(12, 10, false); // => 9
    /// CalculateIndexInRange(12, 0, false); // => 0
    /// </code>
    /// </example>
    /// </summary>
    public static int ComputeIndexInRange(int index, uint count, bool loopIndex)
    {
        long newIndex;

        if (count <= 0)
        {
            newIndex = 0;
        }
        else if (loopIndex)
        {
            newIndex = (((index) % count) + count) % count;
        }
        else
        {
            newIndex = Math.Clamp(index, 0, count - 1);
        }

        return (int)newIndex;
    }


    /// <summary>
    /// Calculates a new size that preserves the aspect ratio of the original size, scaling it so that the longer side
    /// matches the specified length.
    /// </summary>
    public static Size ResizeRatio(Size size, double newSize)
    {
        if (size.Width >= size.Height)
        {
            return new Size(newSize, newSize * size.Height / size.Width);
        }

        return new Size(newSize * size.Width / size.Height, newSize);
    }


    /// <summary>
    /// Converts string to nullable boolean (ignore case).
    /// <list type="bullet">
    ///   <item><c>"true"</c> => <c>true</c></item>
    ///   <item><c>"false"</c> => <c>false</c></item>
    ///   <item>Other values, returns <c>null</c></item>
    /// </list>
    /// </summary>
    public static bool? ConvertStringToBool(string? value)
    {
        var strValue = value ?? string.Empty;
        bool? boolValue = null;

        if (strValue.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            boolValue = true;
        }
        else if (strValue.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            boolValue = false;
        }

        return boolValue;
    }


    /// <summary>
    /// Returns exception details including environment info.
    /// </summary>
    public static string GetExceptionDetails(Exception ex)
    {
        // get system info
        var osArch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

        var details = $"""
            Version: {BHelper.AppDisplayName} v{Core.BuildInfo.FullVersion}
            Magick.NET: {MagickNET.Version}
            Runtime: .NET {Environment.Version}
            OS: {OS} {Environment.OSVersion.VersionString} {osArch}

                                  :>>>>>>>:
                                  | Error |
                                  :<<<<<<<:
            
            {ex.ToString()}

            """;

        return details;
    }


    /// <summary>
    /// Returns debug info and error details for in-app message.
    /// </summary>
    public static (string DebugInfo, string Details) GetInAppError(Exception ex)
    {
        // get system info
        var osArch = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

        var debugInfo = $"""
            {BHelper.AppDisplayName} {Core.BuildInfo.FullVersion}
            {MagickNET.Version}
            {OS} {osArch} {Environment.OSVersion.Version}, .NET {Environment.Version}
            """;

        var errorLines = ex.StackTrace?.Split("\r\n", StringSplitOptions.RemoveEmptyEntries) ?? [];
        var errDetails = $"""
            {ex.Source} ▶ {ex.GetType().FullName} ▶ {ex.Message}

            ▶{string.Join("\r\n▶", errorLines)}
            """;

        return (debugInfo, errDetails);
    }

}
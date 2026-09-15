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
using Avalonia.Controls;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;

namespace ImageGlass.Common.Types;


public static class Resx
{
    /// <summary>
    /// Cached mapping from <see cref="ResxId"/> to its string name.
    /// </summary>
    private static readonly FrozenDictionary<ResxId, string> _resxIdNameCache =
        Enum.GetValues<ResxId>().ToFrozenDictionary(v => v, v => Enum.GetName(v) ?? string.Empty);


    /// <summary>
    /// Gets resource.
    /// </summary>
    public static T Get<T>(ResxId resxId)
    {
        var resName = GetResxName(resxId);
        _ = App.Current!.TryGetResource(resName, out var value);

        return (T)value!;
    }


    /// <summary>
    /// Sets resource.
    /// </summary>
    public static void Set(ResxId resxId, object resValue)
    {
        var resName = GetResxName(resxId);
        App.Current?.Resources[resName] = resValue;
    }


    /// <summary>
    /// Gets the resource name from resource id.
    /// </summary>
    public static string GetResxName(ResxId resxId)
    {
        return _resxIdNameCache.GetValueOrDefault(resxId, string.Empty);
    }


    /// <summary>
    /// Creates a binding to the input resource name.
    /// </summary>
    public static DynamicResourceExtension CreateBinding(ResxId resxId)
    {
        var resName = GetResxName(resxId);
        return new DynamicResourceExtension(resName);
    }


    /// <summary>
    /// Resolves a shared icon geometry (from IconResources) by id.
    /// </summary>
    public static StreamGeometry? GetIcon(ResxIconId? id)
    {
        if (id is null) return null;

        var resName = Enum.GetName(id.Value) ?? string.Empty;
        return Application.Current is { } app && app.TryFindResource(resName, out var res)
            ? res as StreamGeometry
            : null;
    }


    /// <summary>
    /// Gets stock icon.
    /// </summary>
    public static Bitmap? GetStockIcon(StockIconId? id)
    {
        if (id is null) return null;

        try
        {
            using var stream = AssetLoader.Open(new Uri($"avares://ImageGlass.Lib/Assets/{id}.png"));
            return Bitmap.DecodeToHeight(stream, 256);
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Gets a bundled SVG as a scalable image.
    /// </summary>
    public static SvgImage? GetSvg(ResxSvgId? id)
    {
        if (id is null) return null;

        try
        {
            var source = SvgSource.Load($"avares://ImageGlass.Lib/Assets/Svg/{id}.svg", null);
            if (source is not null) return new SvgImage { Source = source };
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Gets default app icon.
    /// </summary>
    public static WindowIcon? GetDefaultWindowIcon()
    {
        try
        {
            using var stream = GetDefaultWindowIconAsStream();
            if (stream is null) return null;

            return new WindowIcon(stream);
        }
        catch { }

        return null;
    }


    /// <summary>
    /// Gets default app icon.
    /// </summary>
    public static Stream? GetDefaultWindowIconAsStream()
    {
        try
        {
            var stream = AssetLoader.Open(new Uri($"avares://ImageGlass.Lib/Assets/icon256.ico"));
            return stream;
        }
        catch { }

        return null;
    }

}


public enum ResxId
{
    // accent colors
    SystemAccentColor,
    SystemAccentColorLight1,
    SystemAccentColorLight2,
    SystemAccentColorLight3,
    SystemAccentColorDark1,
    SystemAccentColorDark2,
    SystemAccentColorDark3,


    // accent button text (contrasts with the accent background)
    AccentButtonForeground,
    AccentButtonForegroundPointerOver,
    AccentButtonForegroundPressed,
    AccentButtonForegroundDisabled,


    // control styles
    ControlCornerRadius,
    ContentControlThemeFontFamily,


    // text color
    IG_TextAccentColor, // accent color tuned for readable text on the theme background
    SystemControlForegroundBaseHighBrush,
    TextControlForeground,
    TextControlForegroundPointerOver,
    TextControlForegroundFocused,
    TextControlPlaceholderForeground,
    CheckBoxForegroundChecked,
    CheckBoxForegroundCheckedPointerOver,
    CheckBoxForegroundUnchecked,
    CheckBoxForegroundUncheckedPointerOver,


    // border color
    TextControlBorderBrush,
    TextControlBorderBrushPointerOver,
    TextControlBorderBrushDisabled,
    ComboBoxBorderBrush,
    ComboBoxBorderBrushPointerOver,
    CheckBoxCheckBackgroundStrokeUnchecked,
    CheckBoxCheckBackgroundStrokeUncheckedPointerOver,


    // menu =======
    MenuFlyoutPresenterBackground,
    MenuFlyoutPresenterBorderBrush,

    IG_MenuSeparatorBackground,
    MenuFlyoutItemBackground,
    MenuFlyoutItemBackgroundPointerOver,
    MenuFlyoutItemBackgroundPressed,

    // menu text
    MenuFlyoutItemForeground,
    MenuFlyoutItemForegroundPointerOver,
    MenuFlyoutItemForegroundPressed,
    MenuFlyoutItemForegroundDisabled,

    // menu hotkey text
    MenuFlyoutItemKeyboardAcceleratorTextForeground,
    MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver,
    MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed,
    MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled,

    // menu chevron
    MenuFlyoutSubItemChevron,
    MenuFlyoutSubItemChevronPointerOver,
    MenuFlyoutSubItemChevronPressed,
    MenuFlyoutSubItemChevronDisabled,
    MenuFlyoutSubItemChevronSubMenuOpened,

    // tooltip
    ToolTipBackground,
    ToolTipForeground,
    ToolTipBorder,


    // combobox ========
    ComboBoxForeground,
    ComboBoxDropDownBackground,
    ComboBoxDropDownBorderBrush,

    ComboBoxItemForeground,
    ComboBoxItemForegroundPointerOver,
    ComboBoxItemForegroundPressed,
    ComboBoxItemForegroundDisabled,
    ComboBoxItemForegroundSelected,

    ComboBoxItemBackground,
    ComboBoxItemBackgroundPointerOver,
    ComboBoxItemBackgroundPressed,
    ComboBoxItemBackgroundSelected,


    // theme pack
    IG_ThemeBackgroundBrush,
    IG_ViewerBackgroundBrush,
    IG_ToolHostBackgroundBrush,
    IG_ThemeForegroundBrush,
    IG_ThemeToolbarBackgroundBrush,
    IG_ThemeGalleryBackgroundBrush,
    IG_ThemeMenuBackgroundBrush,

    // situational backgrounds
    IG_BackgroundInfoBrush,
    IG_BackgroundSuccessBrush,
    IG_BackgroundWarningBrush,
    IG_BackgroundDangerBrush,
    IG_BackgroundNeutralBrush,
    IG_BorderNeutralBrush,
    IG_BorderControlBrush,
    IG_MessageBackgroundBrush,

    IG_TextSuccessBrush,
    IG_TextWarningBrush,
    IG_TextDangerBrush,

    IG_TextSuccessColor,
    IG_TextWarningColor,
    IG_TextDangerColor,


    // tool button styles
    IG_ToolButtonBackground,
    IG_ToolButtonBackgroundHover,
    IG_ToolButtonBackgroundPressed,
    IG_ToolButtonBackgroundChecked,
}


/// <summary>
/// Shared icon geometries defined in <c>IconResources.axaml</c>; the name maps to the resource key.
/// </summary>
public enum ResxIconId
{
    IconEllipsis,
    IconClose,
    IconSearch,
    IconSettings,
    IconSave,
    IconSaveAs,
    IconCrop,
    IconCopy,
    IconReset,
    IconArrowPrevious,
    IconArrowNext,
    IconArrowLeft,
    IconArrowRight,
    IconPlay,
    IconPause,
    IconImageForward,
    IconLivePhoto,
    IconFolderOpen,
    IconEdit,
    IconIntegrated,
    IconInfo,
    IconVerify,
    IconPlaceholder,
    IconWeatherMoon,
    IconWeatherSunny,
    IconProStar,
}


public enum StockIconId
{
    Delete,
    Error,
    Find,
    Info,
    Lock,
    RecycleBin,
    Rename,
    Shield,
    Warning,
}


/// <summary>
/// Bundled SVG assets in <c>Assets/Svg</c>; the name is the file name. These are Fluent Emoji (MIT).
/// </summary>
public enum ResxSvgId
{
    Cyclone,
    SmilingFaceWithSmilingEyes,
    StarStruck,
}

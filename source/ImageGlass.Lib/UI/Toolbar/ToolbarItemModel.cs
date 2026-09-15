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
using ImageGlass.Common;
using ImageGlass.Common.Actions;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.Common.Types.JsonTypeConverters;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace ImageGlass.UI;


[JsonSerializable(typeof(ToolbarItemModel))]
public partial class ToolbarItemModelJsonContext : JsonSerializerContext { }


public partial class ToolbarItemModel : PhReactive, IJsonOnDeserialized
{
    #region Static Properties

    public static Config Config => Core.Config;

    /// <summary>
    /// Gets the ID for toolbar separator.
    /// </summary>
    public static string ID_SEPARATOR => "SEPARATOR";

    /// <summary>
    /// Gets a separator toolbar item.
    /// </summary>
    public static ToolbarItemModel Separator => new(ID_SEPARATOR);

    /// <summary>
    /// Gets inner spacing of toolbar item.
    /// </summary>
    public static double InnerSpacing => Core.Config.ToolbarIconHeight / 6f; // 4

    /// <summary>
    /// Gets the padding of toolbar button.
    /// </summary>
    public static Thickness ItemPadding => new(Core.Config.ToolbarIconHeight / 4.5f); // 5.33

    /// <summary>
    /// Gets the end point of separator line.
    /// </summary>
    public static Point SeparatorEndPoint => new Point(0, Core.Config.ToolbarIconHeight);


    #region Main Menu Items Binding
    public static bool IsZoomModeAutoZoom => Core.Config.ZoomMode == Viewer.ZoomMode.AutoZoom;
    public static bool IsZoomModeLockZoom => Core.Config.ZoomMode == Viewer.ZoomMode.LockZoom;
    public static bool IsZoomModeScaleToWidth => Core.Config.ZoomMode == Viewer.ZoomMode.ScaleToWidth;
    public static bool IsZoomModeScaleToHeight => Core.Config.ZoomMode == Viewer.ZoomMode.ScaleToHeight;
    public static bool IsZoomModeScaleToFit => Core.Config.ZoomMode == Viewer.ZoomMode.ScaleToFit;
    public static bool IsZoomModeScaleToFill => Core.Config.ZoomMode == Viewer.ZoomMode.ScaleToFill;


    public static bool IsLoadingByName => Core.Config.ImageLoadingOrder == ImageOrderBy.Name;
    public static bool IsLoadingByRandom => Core.Config.ImageLoadingOrder == ImageOrderBy.Random;
    public static bool IsLoadingByFileSize => Core.Config.ImageLoadingOrder == ImageOrderBy.FileSize;
    public static bool IsLoadingByExtension => Core.Config.ImageLoadingOrder == ImageOrderBy.Extension;
    public static bool IsLoadingByDateCreated => Core.Config.ImageLoadingOrder == ImageOrderBy.DateCreated;
    public static bool IsLoadingByDateAccessed => Core.Config.ImageLoadingOrder == ImageOrderBy.DateAccessed;
    public static bool IsLoadingByDateModified => Core.Config.ImageLoadingOrder == ImageOrderBy.DateModified;
    public static bool IsLoadingByExifDateTaken => Core.Config.ImageLoadingOrder == ImageOrderBy.ExifDateTaken;
    public static bool IsLoadingByExifRating => Core.Config.ImageLoadingOrder == ImageOrderBy.ExifRating;


    public static bool IsLoadingAsc => Core.Config.ImageLoadingOrderType == ImageOrderType.Asc;
    public static bool IsLoadingDesc => Core.Config.ImageLoadingOrderType == ImageOrderType.Desc;


    public static bool IsColorChannelR => Core.ColorChannels.HasFlag(ColorChannels.R);
    public static bool IsColorChannelG => Core.ColorChannels.HasFlag(ColorChannels.G);
    public static bool IsColorChannelB => Core.ColorChannels.HasFlag(ColorChannels.B);
    public static bool IsColorChannelA => Core.ColorChannels.HasFlag(ColorChannels.A);

    #endregion // Main Menu Items Binding


    #endregion // Static Properties



    #region JSON Properties

    /// <summary>
    /// Gets, sets the unique ID of toolbar button.
    /// </summary>
    public string Id
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(IsSeparator));
        }
    } = "";


    /// <summary>
    /// Gets, sets the SVG icon of toolbar button.
    /// It can be an absolute path,
    /// or the toolbar button name <see cref="IgTheme.ToolbarIcons"/> in theme pack.
    /// </summary>
    public string Image
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(ImagePath));
            _ = OnPropertyChanged(nameof(IsPlaceholderIconVisible));
        }
    } = "";


    /// <summary>
    /// Gets, sets the text of toolbar button, or a language key for localization.
    /// </summary>
    public string Text
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(DisplayText));
            _ = OnPropertyChanged(nameof(IsTextVisible));
            _ = OnPropertyChanged(nameof(Tooltip));
        }
    } = "";


    /// <summary>
    /// Gets the display text of toolbar button
    /// </summary>
    public string DisplayText => Core.Lang[Text];


    /// <summary>
    /// Gets, sets the value indicating that the toolbar button is displayed with text.
    /// </summary>
    public bool ShowText
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(IsTextVisible));
        }
    } = false;


    /// <summary>
    /// Gets, sets the config name for toggle binding.
    /// </summary>
    public string ConfigBinding
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(IsToggle));
        }
    } = string.Empty;


    /// <summary>
    /// Gets, sets the config value for toggle binding.
    /// </summary>
    public string ConfigBindingValue
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
        }
    } = string.Empty;


    /// <summary>
    /// Gets, sets the alignment of toolbar item.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumSafeConverter<ToolbarItemAlignment>))]
    public ToolbarItemAlignment Alignment
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
        }
    } = ToolbarItemAlignment.Left;


    /// <summary>
    /// Gets, sets the click action of toolbar button.
    /// </summary>
    public HotkeySingleAction? OnClick
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(HasClickAction));
            _ = OnPropertyChanged(nameof(IsPlaceholderIconVisible));
        }
    } = null;


    #endregion // JSON Properties



    #region Non-JSON Properties

    /// <summary>
    /// Gets full path of toolbar icon.
    /// </summary>
    [JsonIgnore]
    public string ImagePath
    {
        get
        {
            var svgPath = string.Empty;
            if (string.IsNullOrWhiteSpace(Image)) return svgPath;

            // absolute path: the file itself is the icon
            if (File.Exists(Image)) return Image;

            // get toolbar icon enum from theme
            if (!Enum.TryParse<IgThemeIcon>(Image, out var themeIconNameEnum)) return svgPath;

            // get icon file name from theme
            var themeIconName = Core.Theme.GetIconPath(themeIconNameEnum);
            if (string.IsNullOrWhiteSpace(themeIconName)) return svgPath;

            // theme icon path
            svgPath = Path.Combine(Core.Theme.FolderPath, themeIconName);

            return svgPath;
        }
    }


    /// <summary>
    /// Gets the value indicating that the toolbar button can be toggled.
    /// </summary>
    [JsonIgnore]
    public bool IsToggle => !string.IsNullOrWhiteSpace(ConfigBinding);


    /// <summary>
    /// Checks if the toolbar button has something to run on click; without it the button is inert.
    /// </summary>
    [JsonIgnore]
    public bool HasClickAction => !string.IsNullOrWhiteSpace(OnClick?.Executable);


    /// <summary>
    /// Gets, sets the check state of toolbar button.
    /// </summary>
    [JsonIgnore]
    public bool IsChecked
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
        }
    } = false;


    /// <summary>
    /// Gets, sets the index of toolbar source items.
    /// </summary>
    [JsonIgnore]
    public int SourceIndex
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
        }
    } = -1;


    /// <summary>
    /// Gets, sets the value indicating that the toolbar item is hidden due to overflow.
    /// </summary>
    [JsonIgnore]
    public bool IsNotOverflow
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
        }
    } = true;


    /// <summary>
    /// Checks if the toolbar item is a separator.
    /// </summary>
    [JsonIgnore]
    public bool IsSeparator => Id.Equals(ID_SEPARATOR, StringComparison.InvariantCultureIgnoreCase);


    /// <summary>
    /// Checks if the hatch placeholder icon is shown: a button that runs something but has no valid icon.
    /// </summary>
    [JsonIgnore]
    public bool IsPlaceholderIconVisible
    {
        get
        {
            if (!HasClickAction) return false;
            if (!string.IsNullOrEmpty(ImagePath)) return false;

            // an unloaded theme pack means the icon is pending, not failed
            return string.IsNullOrWhiteSpace(Image) || Core.Theme.IsValid;
        }
    }


    /// <summary>
    /// Checks if the button text is visible.
    /// </summary>
    [JsonIgnore]
    public bool IsTextVisible => ShowText && !string.IsNullOrWhiteSpace(Text);


    /// <summary>
    /// Gets, sets the hotkey string for toolbar item.
    /// </summary>
    [JsonIgnore]
    public string HotkeyText
    {
        get; set
        {
            if (field == value) return;
            field = value;

            _ = OnPropertyChanged();
            _ = OnPropertyChanged(nameof(Tooltip));
        }
    } = string.Empty;


    /// <summary>
    /// Gets the tooltip, or <c>null</c> with no text: an empty string still pops an empty box.
    /// </summary>
    public string? Tooltip
    {
        get
        {
            var text = DisplayText;
            if (string.IsNullOrWhiteSpace(text)) return null;

            if (string.IsNullOrWhiteSpace(HotkeyText))
                return text;

            return $"{text} ({HotkeyText})";
        }
    }

    #endregion // Non-JSON Properties



    public ToolbarItemModel() { }


    public ToolbarItemModel(string id)
    {
        Id = id;
    }



    /// <summary>
    /// Re-raises the computed localized properties, which read <see cref="Core.Lang"/> directly and
    /// therefore need an explicit notification when the app language changes.
    /// </summary>
    public void RefreshLanguage()
    {
        _ = OnPropertyChanged(nameof(DisplayText));
        _ = OnPropertyChanged(nameof(Tooltip));
    }


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString()
    {
        return $"[{SourceIndex} | {Id} | {Text} | {nameof(IsNotOverflow)}={IsNotOverflow}";
    }


    public void OnDeserialized()
    {
        // save action display text
        OnClick?.LangKey = Text;
    }

}


public enum ToolbarItemAlignment
{
    Left,
    Right,
}


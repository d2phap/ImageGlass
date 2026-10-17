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
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Reactive;
using Avalonia.Threading;
using ImageGlass.Common.AppThemes;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class AppearanceSettingsView : SettingsPageView
{
    private const string THEMES_URL = "https://imageglass.org/themes";

    // file picker filter pattern for installable theme packs
    private const string THEME_PACKAGE_PATTERN = "*.igtheme.zip";

    // preview thumbnail size (logical px) and the reduced width images are decoded to (lightweight)
    private const double PREVIEW_W = 132;
    private const double PREVIEW_H = 80;
    private const int PREVIEW_DECODE_WIDTH = 360;

    // square size of the delete button (also the reserved slot width on built-in cards)
    private const double DELETE_BTN_SIZE = 32;

    // max width of an info badge before its text is ellipsized
    private const double BADGE_MAX_WIDTH = 240;

    // fade in/out duration (ms) of the enlarged-preview overlay
    private const double OVERLAY_FADE_MS = 150;

    private List<IgTheme> _themes = [];
    private readonly List<ThemeCard> _cards = [];

    // bumped on every list reload so in-flight async preview loads from an older list are dropped
    private int _themeListGeneration;


    public AppearanceSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public AppearanceSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // resync the background color
        ResyncColorPicker(PART_BgColor, ConfigId.BackgroundColor, Core.Theme.Colors.BgColor);
    }


    protected override void Build()
    {
        BuildBackdropSection();

        // Viewer background color (group has no heading)
        BindColorPicker(PART_BgColor, ConfigId.BackgroundColor, Core.Theme.Colors.BgColor,
            LangId._BackgroundColor, null);

        BuildThemeSection();
    }



    #region Appearance group

    /// <summary>
    /// Binds the window-backdrop dropdown. The backdrop effect only exists on Windows 11+, so the
    /// row is hidden (and not registered) elsewhere.
    /// </summary>
    private void BuildBackdropSection()
    {
        var isWin11 = BHelper.OS == OSType.Windows && !BHelper.IsWindows10;
        PART_BackdropSection.IsVisible = isWin11;
        if (!isWin11) return;

        BindEnumDropdown(PART_WindowBackdrop, ConfigId.WindowBackdrop, BackdropStyle.Mica,
            LangId.Settings_WindowBackdrop, null);
    }

    #endregion // Appearance group



    #region Theme group

    /// <summary>
    /// Wires the theme action buttons and triggers the initial theme-pack load.
    /// </summary>
    private void BuildThemeSection()
    {
        SetLocalizedText(PART_InstallTheme, LangId.Settings_Theme_InstallTheme);
        PART_InstallTheme.Click += async (_, _) => await InstallThemesAsync();
        RegisterSearchKey(PART_InstallTheme, LangId.Settings_Theme_InstallTheme, null, LangId.Settings_Theme);

        SetLocalizedText(PART_RefreshThemes, LangId.Settings_Refresh);
        PART_RefreshThemes.Click += async (_, _) => await ReloadThemesAsync();
        RegisterSearchKey(PART_RefreshThemes, LangId.Settings_Refresh, null, LangId.Settings_Theme);

        SetLocalizedText(PART_OpenThemeFolder, LangId.Settings_Theme_OpenThemeFolder);
        PART_OpenThemeFolder.Click += (_, _) => BHelper.OpenFolderPath(BHelper.GetRealPlatformConfigDir(Dir.Themes));
        RegisterSearchKey(PART_OpenThemeFolder, LangId.Settings_Theme_OpenThemeFolder, null, LangId.Settings_Theme);

        BindLink(PART_GetMoreThemes, LangId.Settings_Theme_GetMoreThemes, THEMES_URL,
            () => _ = BHelper.OpenUrlAsync(this, THEMES_URL, "from_setting_appearance"));

        // refresh card labels/tooltips on language change (cards persist; texts are re-applied in place)
        AddLangRefresher(ApplyThemeCardTexts);

        UpdateThemeOverview();
        _ = ReloadThemesAsync();

        // theme selection is admin-locked when either the dark or light theme is locked; the
        // shared card list has no per-mode sub-container, so lock the whole list for either
        DisableIfLocked(ConfigId.DarkTheme, PART_ThemeList);
        DisableIfLocked(ConfigId.LightTheme, PART_ThemeList);
    }


    /// <summary>
    /// Reloads the installed theme packs from disk and rebuilds the list.
    /// </summary>
    private async Task ReloadThemesAsync()
    {
        _themes = await Config.LoadAllThemePacksAsync();
        RebuildThemeList();
    }


    /// <summary>
    /// Opens a file picker for <c>*.igtheme.zip</c> packs, installs them, and reports incompatible ones.
    /// </summary>
    private async Task InstallThemesAsync()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType(Core.Lang[LangId.Settings_Theme]) {
                    Patterns = [THEME_PACKAGE_PATTERN]
                },
            ],
        });

        var paths = files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrEmpty(p))
            .Cast<string>()
            .ToList();
        if (paths.Count == 0) return;

        var result = await Config.InstallThemePacksAsync(paths);
        await ReloadThemesAsync();

        // report packs rejected as incompatible
        if (result.IncompatiblePackNames.Count > 0)
        {
            var details = string.Join(Environment.NewLine, result.IncompatiblePackNames.Select(n => $"- {n}"));

            await ModalWindow.ShowErrorAsync(TopLevel.GetTopLevel(this) as PhWindow, new ModalWindowOptions
            {
                Title = Core.Lang[LangId.Settings_Theme_InstallTheme],
                Heading = Core.Lang[LangId._IncompatibleTheme],
                Description = Core.Lang[LangId._IncompatibleTheme_Description],
                Details = details,
            });
        }
    }


    /// <summary>
    /// Removes a user-installed theme pack (no confirmation), falling back to the default pack for
    /// any mode the removed pack was assigned to.
    /// </summary>
    private async Task UninstallThemeAsync(IgTheme theme)
    {
        if (!Config.UninstallThemePack(theme)) return;

        // a removed pack can't stay selected -> revert that mode to its default
        if (IsSelectedForDark(theme)) VM.SetValue(ConfigId.DarkTheme, Const.DEFAULT_THEME);
        if (IsSelectedForLight(theme)) VM.SetValue(ConfigId.LightTheme, Const.DEFAULT_LIGHT_THEME);

        await ReloadThemesAsync();
    }


    /// <summary>
    /// Disposes old previews and rebuilds the card list from <see cref="_themes"/>.
    /// </summary>
    private void RebuildThemeList()
    {
        foreach (var c in _cards) c.Preview?.Dispose();
        _cards.Clear();
        PART_ThemeList.Children.Clear();

        var gen = ++_themeListGeneration;
        PART_ThemeEmpty.IsVisible = _themes.Count == 0;

        foreach (var theme in _themes)
        {
            PART_ThemeList.Children.Add(BuildThemeCard(theme, gen));
        }

        RefreshThemeSelectionStates();
        ApplyThemeCardTexts();
    }


    /// <summary>
    /// Builds one theme-pack card (preview, info, dark/light toggles, uninstall/built-in trailing).
    /// </summary>
    private Border BuildThemeCard(IgTheme theme, int gen)
    {
        // preview: a neutral placeholder with the (async-loaded) image on top
        var placeholder = new Border
        {
            CornerRadius = Resx.Get<CornerRadius>(ResxId.ControlCornerRadius),
        };
        placeholder[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_BackgroundNeutralBrush);

        var img = new Image { Stretch = Stretch.UniformToFill, IsVisible = false };
        var previewBox = new Border
        {
            Width = PREVIEW_W,
            Height = PREVIEW_H,
            VerticalAlignment = VerticalAlignment.Top,
            CornerRadius = Resx.Get<CornerRadius>(ResxId.ControlCornerRadius),
            ClipToBounds = true,
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = new Grid { Children = { placeholder, img } },
        };
        previewBox.Tapped += (_, _) => ShowPreviewOverlay(theme);

        // info column — every line is single-line and truncated
        var nameText = new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(theme.Info.Name) ? theme.FolderName : theme.Info.Name,
            FontWeight = FontWeight.SemiBold,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };

        var descText = new TextBlock
        {
            Text = theme.Info.Description,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.75,
            IsVisible = !string.IsNullOrWhiteSpace(theme.Info.Description),
        };
        if (!string.IsNullOrWhiteSpace(theme.Info.Description)) ToolTip.SetTip(descText, theme.Info.Description);

        // full folder path: selectable so the user can copy it
        var pathText = new SelectableTextBlock
        {
            Text = theme.DisplayFolderPath,
            FontFamily = new FontFamily(Const.FONT_CODE),
            FontSize = Const.FONT_SIZE_SMALL,
            Opacity = 0.75,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        ToolTip.SetTip(pathText, theme.DisplayFolderPath);

        // author / version / mode badges below the path (mode text is localized in ApplyThemeCardTexts)
        var modeBadge = BadgeText(string.Empty);
        var badges = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Margin = new Thickness(0, 4, 0, 0),
        };
        if (!string.IsNullOrWhiteSpace(theme.Info.Author))
        {
            badges.Children.Add(MakeBadge(BadgeText(theme.Info.Author)));
        }
        badges.Children.Add(MakeBadge(BadgeText($"v{theme.Info.Version:0.#}")));
        badges.Children.Add(MakeBadge(modeBadge));

        var info = new StackPanel { VerticalAlignment = VerticalAlignment.Top, Spacing = 3 };
        info.Children.AddRange([nameText, descText, pathText, badges]);

        // dark/light toggles + delete: one inline row, top-aligned with the name
        var darkText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        var darkBtn = BuildModeToggle(ResxIconId.IconWeatherMoon, darkText);
        darkBtn.Click += (_, _) => SelectThemeForMode(theme, darkMode: true);

        var lightText = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
        var lightBtn = BuildModeToggle(ResxIconId.IconWeatherSunny, lightText);
        lightBtn.Click += (_, _) => SelectThemeForMode(theme, darkMode: false);

        var controls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        controls.Children.AddRange([darkBtn, lightBtn]);

        // trailing: uninstall (user packs, revealed on hover) or a blank spacer (built-in packs),
        // so the Dark/Light buttons line up across every card
        Control? uninstallBtn = null;
        if (Config.IsBuiltInThemePack(theme))
        {
            controls.Children.Add(new Border { Width = DELETE_BTN_SIZE });
        }
        else
        {
            var btn = BuildDeleteButton();
            btn.Click += async (_, _) => await UninstallThemeAsync(theme);
            uninstallBtn = btn;
            controls.Children.Add(btn);
        }

        // assemble: preview | info | controls
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 14,
        };
        Grid.SetColumn(previewBox, 0);
        Grid.SetColumn(info, 1);
        Grid.SetColumn(controls, 2);
        grid.Children.AddRange([previewBox, info, controls]);

        var card = new Border { Child = grid };
        card.Classes.Add("theme-card");

        // reveal the delete button only while hovering the card (opacity keeps its space)
        if (uninstallBtn is not null)
        {
            card.PointerEntered += (_, _) => uninstallBtn.Opacity = 1;
            card.PointerExited += (_, _) => uninstallBtn.Opacity = 0;
        }

        _cards.Add(new ThemeCard(theme, darkBtn, lightBtn, darkText, lightText, modeBadge, uninstallBtn));

        _ = LoadThemePreviewAsync(theme, img, placeholder, gen);

        return card;
    }


    /// <summary>
    /// Shows the theme's preview image enlarged in a full-window overlay; click anywhere to dismiss.
    /// </summary>
    private async void ShowPreviewOverlay(IgTheme theme)
    {
        if (string.IsNullOrEmpty(theme.Settings.PreviewImage)) return;

        var path = Path.Combine(theme.FolderPath, theme.Settings.PreviewImage);
        if (!File.Exists(path)) return;

        var layer = OverlayLayer.GetOverlayLayer(this);
        if (layer is null) return;

        // decode at native resolution so the overlay can map image pixels 1:1 to physical pixels
        Bitmap? bmp;
        try
        {
            bmp = await Task.Run(() => DecodePreview(path, 0));
        }
        catch { return; }
        if (bmp is null) return;

        // theme-aware scrim (transient overlay: read the current theme background once)
        var bgColor = Resx.Get<ISolidColorBrush>(ResxId.IG_ThemeBackgroundBrush)?.Color ?? Colors.Black;
        bgColor = bgColor.Blend(Core.Theme.InvertedBaseColor, 0.85f, 200);
        var scrim = new Border { Background = bgColor.ToBrush() };

        var image = new Image
        {
            Source = bmp,
            Stretch = Stretch.Uniform,
            MaxWidth = bmp.PixelSize.Width / this.Dpi,
            MaxHeight = bmp.PixelSize.Height / this.Dpi,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(48),
        };

        // the overlay layer is Canvas-like (no auto-stretch), so size the root to fill it.
        // starts transparent and fades in once attached (see below)
        var root = new Grid
        {
            Cursor = new Cursor(StandardCursorType.Hand),
            Opacity = 0,
            Transitions =
            [
                new DoubleTransition
                {
                    Property = Visual.OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(OVERLAY_FADE_MS),
                },
            ],
        };
        root.Children.AddRange([scrim, image]);

        var sizer = layer.GetObservable(Visual.BoundsProperty).Subscribe(new AnonymousObserver<Rect>(b =>
        {
            root.Width = b.Width;
            root.Height = b.Height;
        }));

        var closing = false;
        root.Tapped += async (_, _) =>
        {
            if (closing) return;
            closing = true;

            // fade out, then tear down
            root.Opacity = 0;
            await Task.Delay(TimeSpan.FromMilliseconds(OVERLAY_FADE_MS));

            sizer.Dispose();
            layer.Children.Remove(root);
            bmp.Dispose();
        };

        layer.Children.Add(root);
        Dispatcher.UIThread.Post(() => { if (!closing) root.Opacity = 1; });
    }


    /// <summary>
    /// Assigns the pack to dark or light mode (staged), unless it already holds that slot.
    /// </summary>
    private void SelectThemeForMode(IgTheme theme, bool darkMode)
    {
        if (darkMode)
        {
            if (IsSelectedForDark(theme)) return;
            VM.SetValue(ConfigId.DarkTheme, theme.FolderName);
        }
        else
        {
            if (IsSelectedForLight(theme)) return;
            VM.SetValue(ConfigId.LightTheme, theme.FolderName);
        }

        RefreshThemeSelectionStates();
    }


    /// <summary>
    /// Syncs every card's dark/light toggle to the staged <c>DarkTheme</c>/<c>LightTheme</c> values.
    /// </summary>
    private void RefreshThemeSelectionStates()
    {
        foreach (var c in _cards)
        {
            c.DarkButton.IsChecked = IsSelectedForDark(c.Theme);
            c.LightButton.IsChecked = IsSelectedForLight(c.Theme);
        }

        UpdateThemeOverview();
    }


    /// <summary>
    /// Updates the overview box with the display names of the packs currently assigned to each mode.
    /// </summary>
    private void UpdateThemeOverview()
    {
        PART_DarkThemeName.Text = GetThemeDisplayName(VM.GetValue(ConfigId.DarkTheme, Const.DEFAULT_THEME));
        PART_LightThemeName.Text = GetThemeDisplayName(VM.GetValue(ConfigId.LightTheme, Const.DEFAULT_LIGHT_THEME));
    }


    /// <summary>
    /// Resolves a pack folder name to its display name, falling back to the folder name when the
    /// pack isn't installed.
    /// </summary>
    private string GetThemeDisplayName(string folderName)
    {
        var theme = _themes.FirstOrDefault(t =>
            t.FolderName.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        if (theme is null) return folderName;

        return string.IsNullOrWhiteSpace(theme.Info.Name) ? theme.FolderName : theme.Info.Name;
    }


    /// <summary>
    /// Re-applies the localized labels/tooltips on the cards (the rest of a card is theme data).
    /// </summary>
    private void ApplyThemeCardTexts()
    {
        var darkLabel = Core.Lang[LangId.Settings_DarkTheme];
        var lightLabel = Core.Lang[LangId.Settings_LightTheme];

        foreach (var c in _cards)
        {
            c.DarkText.Text = darkLabel;
            c.LightText.Text = lightLabel;
            ToolTip.SetTip(c.DarkButton, Core.Lang[LangId.Settings_UseThemeForDarkMode]);
            ToolTip.SetTip(c.LightButton, Core.Lang[LangId.Settings_UseThemeForLightMode]);
            c.ModeBadge.Text = Core.Lang[c.Theme.Settings.IsDarkMode
                ? LangId.Settings_DarkTheme
                : LangId.Settings_LightTheme];

            if (c.UninstallButton is not null)
                ToolTip.SetTip(c.UninstallButton, Core.Lang[LangId._Delete]);
        }
    }


    /// <summary>
    /// Loads a theme's preview thumbnail off the UI thread (decoded small) and shows it if the list
    /// hasn't been rebuilt in the meantime.
    /// </summary>
    private async Task LoadThemePreviewAsync(IgTheme theme, Image img, Control placeholder, int gen)
    {
        if (string.IsNullOrEmpty(theme.Settings.PreviewImage)) return;

        var path = Path.Combine(theme.FolderPath, theme.Settings.PreviewImage);
        if (!File.Exists(path)) return;

        try
        {
            var bmp = await Task.Run(() => DecodePreview(path, PREVIEW_DECODE_WIDTH));
            if (bmp is null) return;

            // a newer reload happened while decoding -> drop this stale result
            if (gen != _themeListGeneration)
            {
                bmp.Dispose();
                return;
            }

            var card = _cards.FirstOrDefault(c => c.Theme.FolderPath == theme.FolderPath);
            if (card is not null) card.Preview = bmp;

            img.Source = bmp;
            img.IsVisible = true;
            placeholder.IsVisible = false;
        }
        catch { }
    }


    /// <summary>
    /// Decodes an image, optionally at a reduced width (<paramref name="decodeWidth"/> &lt;= 0 loads at
    /// native resolution). Uses Avalonia's decoder first, falling back to SkiaSharp (e.g. for formats
    /// Avalonia's pipeline doesn't decode directly).
    /// </summary>
    private static Bitmap? DecodePreview(string path, int decodeWidth)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return decodeWidth > 0 ? Bitmap.DecodeToWidth(stream, decodeWidth) : new Bitmap(stream);
        }
        catch
        {
            try
            {
                using var skbmp = SKBitmap.Decode(path);
                return SkiaCodec.ToWritableBitmap(skbmp);
            }
            catch { return null; }
        }
    }


    private bool IsSelectedForDark(IgTheme theme)
        => string.Equals(VM.GetValue(ConfigId.DarkTheme, Const.DEFAULT_THEME), theme.FolderName,
            StringComparison.OrdinalIgnoreCase);

    private bool IsSelectedForLight(IgTheme theme)
        => string.Equals(VM.GetValue(ConfigId.LightTheme, Const.DEFAULT_LIGHT_THEME), theme.FolderName,
            StringComparison.OrdinalIgnoreCase);


    #endregion // Theme group



    #region Card element builders

    /// <summary>
    /// Builds a dark/light mode toggle (icon + label). It does not self-toggle; selection is driven
    /// from <see cref="RefreshThemeSelectionStates"/>.
    /// </summary>
    private static PhToolButton BuildModeToggle(ResxIconId iconId, TextBlock label)
    {
        var icon = new PathIcon { Width = 14, Height = 14, Data = Resx.GetIcon(iconId) };
        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            VerticalAlignment = VerticalAlignment.Center,
        };
        content.Children.AddRange([icon, label]);

        return new PhToolButton
        {
            IsCheckOnClick = false,
            Padding = new Thickness(9, 5),
            Content = content,
        };
    }


    /// <summary>
    /// Builds the square "X" uninstall tool button.
    /// </summary>
    private static PhToolButton BuildDeleteButton()
    {
        var glyph = new Avalonia.Controls.Shapes.Path
        {
            Width = 12,
            Height = 12,
            Data = Resx.GetIcon(ResxIconId.IconClose),
            Stretch = Stretch.Uniform,
        };
        glyph[!Avalonia.Controls.Shapes.Path.FillProperty] = Resx.CreateBinding(ResxId.IG_ThemeForegroundBrush);

        return new PhToolButton
        {
            IsCheckOnClick = false,
            Width = DELETE_BTN_SIZE,
            Height = DELETE_BTN_SIZE,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Hand),
            Content = glyph,
            Opacity = 0,
            Transitions = new Transitions
            {
                new DoubleTransition { Property = Visual.OpacityProperty, Duration = TimeSpan.FromMilliseconds(120) },
            },
        };
    }


    /// <summary>
    /// Wraps badge text in a small rounded pill (neutral background).
    /// </summary>
    private static Border MakeBadge(TextBlock content)
    {
        var badge = new Border
        {
            CornerRadius = Resx.Get<CornerRadius>(ResxId.ControlCornerRadius),
            Padding = new Thickness(7, 1),
            VerticalAlignment = VerticalAlignment.Center,
            Child = content,
        };
        badge[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_BorderNeutralBrush);

        return badge;
    }


    /// <summary>
    /// Creates the small, muted, single-line text used inside a badge.
    /// </summary>
    private static TextBlock BadgeText(string text) => new()
    {
        Text = text,
        FontSize = Const.FONT_SIZE_SMALL,
        Opacity = 0.85,
        MaxWidth = BADGE_MAX_WIDTH,
        TextTrimming = TextTrimming.CharacterEllipsis,
    };

    #endregion // Card element builders



    /// <summary>
    /// Holds the per-card controls that need updating after creation (selection state, localized
    /// text, and the loaded preview bitmap to dispose on reload).
    /// </summary>
    private sealed class ThemeCard(IgTheme theme, PhToolButton darkButton, PhToolButton lightButton,
        TextBlock darkText, TextBlock lightText, TextBlock modeBadge, Control? uninstallButton)
    {
        public IgTheme Theme { get; } = theme;
        public PhToolButton DarkButton { get; } = darkButton;
        public PhToolButton LightButton { get; } = lightButton;
        public TextBlock DarkText { get; } = darkText;
        public TextBlock LightText { get; } = lightText;
        public TextBlock ModeBadge { get; } = modeBadge;
        public Control? UninstallButton { get; } = uninstallButton;
        public Bitmap? Preview { get; set; }
    }

}

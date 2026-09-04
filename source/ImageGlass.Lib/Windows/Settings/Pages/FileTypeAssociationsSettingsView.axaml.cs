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
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Photoing;
using ImageGlass.Common.ServiceProviders;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

public partial class FileTypeAssociationsSettingsView : SettingsPageView
{
    private const string EXT_ICON_PACKS_URL = "https://imageglass.org/extension-icons";

    // floor + bottom inset for fitting the formats table to the page viewport height
    private const double MIN_TABLE_HEIGHT = 220;
    private const double BOTTOM_GAP = 40;

    private static readonly FontFamily _codeFont = new(Const.FONT_CODE);

    // the hosting page's scroll viewer, used to size the table to the remaining viewport height
    private ScrollViewer? _pageScroll;

    // working copy of the user/built-in formats (persisted to Config.FileFormats); staged on change.
    // plugin formats are NOT included here, so they are never baked into the saved config.
    private readonly HashSet<string> _exts = new(StringComparer.OrdinalIgnoreCase);

    // extensions contributed by loaded codec plugins: shown as extra (non-removable) rows,
    // never staged into Config.FileFormats
    private readonly HashSet<string> _pluginExts = new(StringComparer.OrdinalIgnoreCase);

    // plugin extensions that are writable only; still supported formats, so they get rows too
    private readonly HashSet<string> _pluginEncodeExts = new(StringComparer.OrdinalIgnoreCase);

    // codec snapshot (ordered by decode priority, highest first) for the Decoder/Encoder columns
    private IReadOnlyList<CodecInfo> _codecs = [];

    // per-extension lookup caches; the filter runs these once per row per keystroke
    private readonly Dictionary<string, CodecInfo?> _decoderCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CodecInfo?> _encoderCache = new(StringComparer.OrdinalIgnoreCase);

    // current table filter query (matches extension or codec name)
    private string _filter = string.Empty;


    public FileTypeAssociationsSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public FileTypeAssociationsSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        BuildExtensionIcons();
        BuildDefaultPhotoViewer();
        BuildAppMenuEntry();
        BuildFileFormats();
    }


    #region Fit table to viewport height

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _pageScroll = this.FindAncestorOfType<ScrollViewer>();
        if (_pageScroll is not null) _pageScroll.PropertyChanged += PageScroll_PropertyChanged;

        // bounds are usually 0 at attach; size once layout settles
        Dispatcher.UIThread.Post(UpdateTableMaxHeight, DispatcherPriority.Background);
    }


    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_pageScroll is not null) _pageScroll.PropertyChanged -= PageScroll_PropertyChanged;
        _pageScroll = null;
        base.OnDetachedFromVisualTree(e);
    }


    private void PageScroll_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == BoundsProperty) UpdateTableMaxHeight();
    }


    /// <summary>
    /// Caps the formats table at the page viewport's remaining height (below the chrome above it),
    /// never below the floor, so it grows/shrinks with the window and scrolls its rows internally.
    /// </summary>
    private void UpdateTableMaxHeight()
    {
        if (_pageScroll is null) return;

        var viewport = _pageScroll.Bounds.Height;
        if (viewport <= 0) return;

        if (PART_Table.TranslatePoint(new Point(0, 0), _pageScroll) is not { } pt) return;

        var tableTop = pt.Y + _pageScroll.Offset.Y;
        PART_Table.MaxHeight = Math.Max(MIN_TABLE_HEIGHT, viewport - tableTop - BOTTOM_GAP);
    }

    #endregion // Fit table to viewport height


    #region File extension icons

    /// <summary>
    /// Wires the "File extension icons" group (open the icon folder, get icon packs online).
    /// </summary>
    private void BuildExtensionIcons()
    {
        // hide when associations can't reach the shell (virtualized Store MSIX)
        if (Core.ShellProvider?.IsDefaultViewerConfigurable == false)
        {
            PART_ExtIconsHeading.IsVisible = false;
            PART_ExtIconsGroup.IsVisible = false;
            return;
        }

        SetLocalizedText(PART_OpenExtIconFolder, LangId.Settings_OpenExtensionIconFolder);
        PART_OpenExtIconFolder.Click += (_, _) =>
            BHelper.OpenFolderPath(BHelper.GetRealPlatformConfigDir(Dir.ExtIcons));

        // the description references the 'Register' button name via its {0} placeholder
        AddLangRefresher(() => PART_ExtIconsDesc.LangParams = Core.Lang[LangId._Register]);

        SetLocalizedText(PART_GetExtIconPacks, LangId.Settings_GetExtensionIconPacks);
        PART_GetExtIconPacks.Click += async (_, _) =>
            await BHelper.OpenUrlAsync(this, EXT_ICON_PACKS_URL, "from_ext_icons");

        RegisterSearchKey(PART_OpenExtIconFolder, LangId.Settings_FileExtensionIcons, null,
            LangId.Settings_FileExtensionIcons);
    }

    #endregion // File extension icons


    #region Default photo viewer

    /// <summary>
    /// Wires the "Default photo viewer" group (make/remove default, the unmanaged-setting warning,
    /// and the shortcut to the Windows Default apps settings).
    /// </summary>
    private void BuildDefaultPhotoViewer()
    {
        // hide when associations can't reach the shell (virtualized Store MSIX)
        if (Core.ShellProvider?.IsDefaultViewerConfigurable == false)
        {
            PART_DefaultViewerHeading.IsVisible = false;
            PART_DefaultViewerGroup.IsVisible = false;
            return;
        }

        // per-user vs per-machine registration, derived from the install location
        var scope = Core.ShellProvider?.GetDefaultViewerScope() ?? DefaultAppScope.CurrentUser;

        // the Windows "Default apps" deep link key differs per scope; the app name must be URI-escaped
        var appQueryKey = scope == DefaultAppScope.LocalMachine ? "registeredAppMachine" : "registeredAppUser";
        var defaultAppsUri = $"ms-settings:defaultapps?{appQueryKey}={Uri.EscapeDataString(BHelper.AppName)}";

        SetLocalizedText(PART_Register, LangId._Register);
        AddLangRefresher(() => ToolTip.SetTip(PART_Register, Core.Lang[LangId.Settings_UnmanagedSettingReminder]));
        PART_Register.Click += async (_, _) => await AppAPIProvider.IG_SetDefaultPhotoViewerAsync();

        SetLocalizedText(PART_Unregister, LangId._Unregister);
        PART_Unregister.Click += async (_, _) => await AppAPIProvider.IG_RemoveDefaultPhotoViewerAsync();

        // show the registration scope (all users vs current user)
        AddLangRefresher(() => PART_ScopeInfo.Text = Core.Lang[scope == DefaultAppScope.LocalMachine
            ? LangId.Settings_DefaultPhotoViewer_ScopePerMachine
            : LangId.Settings_DefaultPhotoViewer_ScopePerUser]);

        SetLocalizedText(PART_OpenDefaultApps, LangId.Settings_OpenDefaultAppsSetting);
        PART_OpenDefaultApps.Click += async (_, _) => await BHelper.OpenUrlAsync(this, defaultAppsUri, "from_default_apps");

        RegisterSearchKey(PART_Register, LangId.Settings_DefaultPhotoViewer, null, LangId.Settings_DefaultPhotoViewer);
    }

    #endregion // Default photo viewer


    #region Applications menu

    /// <summary>
    /// Builds the applications-menu group. Hidden unless the build has an entry to register.
    /// </summary>
    private void BuildAppMenuEntry()
    {
        if (Core.ShellProvider?.CanRegisterAppMenuEntry != true) return;

        PART_AppMenuHeading.IsVisible = true;
        PART_AppMenuGroup.IsVisible = true;

        SetLocalizedText(PART_RegisterAppMenu, LangId._Register);
        AddLangRefresher(() => ToolTip.SetTip(PART_RegisterAppMenu, Core.Lang[LangId.Settings_UnmanagedSettingReminder]));
        PART_RegisterAppMenu.Click += async (_, _) => await AppAPIProvider.RegisterAppMenuEntryAsync(true);

        SetLocalizedText(PART_UnregisterAppMenu, LangId._Unregister);
        PART_UnregisterAppMenu.Click += async (_, _) => await AppAPIProvider.RegisterAppMenuEntryAsync(false);

        RegisterSearchKey(PART_RegisterAppMenu, LangId.Settings_AppMenuEntry, null, LangId.Settings_AppMenuEntry);
    }

    #endregion // Applications menu



    #region File formats

    /// <summary>
    /// Loads the working copy of the user/built-in formats (the plugin formats are tracked
    /// separately for display) and wires the Add / Reset buttons and the formats table.
    /// </summary>
    private void BuildFileFormats()
    {
        SnapshotCodecs();

        // copy the staged/config formats (user/built-in set) so edits don't mutate the live config
        // before commit. Plugin formats are deliberately NOT merged in here: they are shown from
        // _pluginExts but never staged, so disabling/removing a plugin drops its formats.
        var stored = VM.GetValue(ConfigId.FileFormats,
            new HashSet<string>(Config.DefaultFileFormats, StringComparer.OrdinalIgnoreCase));
        foreach (var ext in stored) _exts.Add(ext);

        PART_Table.MinHeight = MIN_TABLE_HEIGHT;

        SetLocalizedText(PART_AddFormat, LangId._Add);
        PART_AddFormat.Click += async (_, _) => await AddExtensionAsync();

        SetLocalizedText(PART_ResetFormats, LangId._ResetToDefault);
        PART_ResetFormats.Click += (_, _) => ResetFormats();

        // filter rows by extension or codec name
        AddLangRefresher(() => PART_Search.PlaceholderText = Core.Lang[LangId._TypeToFilter]);
        PART_Search.TextChanged += (_, _) =>
        {
            _filter = PART_Search.Text ?? string.Empty;
            RebuildTable();
        };

        // rebuild on language change (also performs the initial render)
        AddLangRefresher(RebuildTable);

        RegisterSearchKey(PART_AddFormat, LangId.Settings_FileFormats, ConfigId.FileFormats,
            LangId.Settings_FileFormats);

        DisableIfLocked(ConfigId.FileFormats, PART_AddFormat, PART_ResetFormats, PART_Table);
    }


    /// <summary>
    /// Stages the current working copy of formats into the view model.
    /// </summary>
    private void StageFormats() => VM.SetValue(ConfigId.FileFormats, new HashSet<string>(_exts, StringComparer.OrdinalIgnoreCase));


    /// <summary>
    /// Shows an input dialog to add a new extension, then scrolls to + flashes its row.
    /// A duplicate isn't added; the existing row is flashed instead.
    /// </summary>
    private async Task AddExtensionAsync()
    {
        var win = TopLevel.GetTopLevel(this) as PhWindow;
        var result = await ModalWindow.ShowInputAsync(win, new ModalWindowOptions
        {
            Title = Core.Lang[LangId.Settings_AddNewFileExtension],
            Description = Core.Lang[LangId._FileExtension],
            InputPlaceholder = ".jpg",
            AcceptValue = TextBoxAcceptValue.FileExtensionValueOnly,
        });
        if (result.ExitCode != DialogExitCode.OK) return;

        // normalize to a lowercase value with a leading dot (e.g. "PSD" -> ".psd")
        var raw = result.InputValue?.Trim().ToLowerInvariant() ?? string.Empty;
        if (raw.Length == 0) return;
        var ext = raw.StartsWith('.') ? raw : "." + raw;

        if (_exts.Add(ext))
        {
            StageFormats();
            RebuildTable();
        }

        // scroll to + flash the row (newly added or the existing duplicate)
        PART_Table.FlashRow(ext);
    }


    /// <summary>
    /// Removes a user/built-in extension from the working copy and re-renders. Plugin-provided
    /// formats have no Delete action, so this is only ever called for a removable format.
    /// </summary>
    private void DeleteExtension(string ext)
    {
        if (!_exts.Remove(ext)) return;

        StageFormats();
        RebuildTable();
    }


    /// <summary>
    /// Resets the formats to the built-in defaults. Plugin formats are unaffected (they are shown
    /// separately and never part of the persisted set).
    /// </summary>
    private void ResetFormats()
    {
        _exts.Clear();
        foreach (var ext in Config.DefaultFileFormats) _exts.Add(ext);

        StageFormats();
        RebuildTable();
    }


    /// <summary>
    /// Rebuilds the formats table (order number, extension, codec + a Delete action for user
    /// formats), sorted by extension, and updates the total count. The rows are the user/built-in
    /// formats plus the formats contributed by loaded plugins.
    /// </summary>
    private void RebuildTable()
    {
        PhTableColumn[] columns =
        [
            new() { Header = string.Empty },
            new() { Header = Core.Lang[LangId._FileExtension], MinWidth = 160 },
            // Decoder hugs its content so Encoder sits right next to it; Encoder takes the slack,
            // which keeps both left-aligned instead of splitting the row in half.
            new() { Header = Core.Lang[LangId._Decoder], MinWidth = 150 },
            new() { Header = Core.Lang[LangId._Encoder], Star = true, MinWidth = 150 },
        ];

        // browsable set = user/built-in formats + plugin decode formats; drives the total count
        var browsable = new HashSet<string>(_exts, StringComparer.OrdinalIgnoreCase);
        foreach (var ext in _pluginExts) browsable.Add(ext);

        // rows also include write-only plugin formats: still supported, just not browsable
        var all = new HashSet<string>(browsable, StringComparer.OrdinalIgnoreCase);
        foreach (var ext in _pluginEncodeExts) all.Add(ext);

        // filtered (by extension or either codec name) + sorted by extension ascending
        var q = _filter.Trim();
        var sorted = all
            .Where(e => q.Length == 0
                || e.Contains(q, StringComparison.OrdinalIgnoreCase)
                || DecodeCodecNameFor(e).Contains(q, StringComparison.OrdinalIgnoreCase)
                || EncodeCodecNameFor(e).Contains(q, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var rows = new List<PhTableRow>(sorted.Count);
        for (var i = 0; i < sorted.Count; i++)
        {
            var ext = sorted[i];
            var key = ext; // capture for the action closure

            // only user/built-in formats are removable; a plugin-provided format has no Delete action
            PhTableAction[] actions = _exts.Contains(ext) ? [
                new() {
                    Icon = ResxIconId.IconClose,
                    Tooltip = Core.Lang[LangId._Delete],
                    Click = () => DeleteExtension(key),
                }
            ] : [];

            rows.Add(new PhTableRow
            {
                Key = ext,
                Cells =
                [
                    PhTableControl.TextCell((i + 1).ToString()),
                    PhTableControl.TextCell(ext, selectable: true, font: _codeFont),
                    CodecCell(DecodeCodecFor(ext)),
                    CodecCell(EncodeCodecFor(ext)),
                ],
                Actions = actions,
            });
        }

        PART_Table.EmptyText = "–";
        PART_Table.Build(columns, rows);

        // the label means "formats ImageGlass can open", so write-only formats are excluded
        PART_TotalFormats.LangParams = browsable.Count;
    }


    /// <summary>
    /// Builds a codec cell: a link button (opens the owning plugin on the Plugins page) for a plugin
    /// codec, plain text for a built-in, and a muted placeholder when nothing handles it.
    /// </summary>
    private Control CodecCell(CodecInfo? codec)
    {
        var name = codec?.CodecName ?? string.Empty;
        if (name.Length == 0)
        {
            // "–" here means nothing can handle it; the plugin dialog's dash means
            // "capability not declared". Deliberately different, do not unify.
            return PhTableControl.TextCell("–", muted: true);
        }

        // built-in codec -> plain text
        var pluginId = codec?.PluginId;
        if (string.IsNullOrEmpty(pluginId)) return PhTableControl.TextCell(name);

        // plugin codec -> link that opens the plugin's info window on the Plugins page
        var link = new PhButton
        {
            Text = name,
            Variant = PhButtonVariant.Link,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Left,
        };
        ToolTip.SetTip(link, name);
        link.Click += (_, _) => OpenPluginSettings(pluginId);

        return PhTableControl.WrapCell(link);
    }


    /// <summary>
    /// Navigates to the Plugins page and highlights the plugin that owns the codec for the extension.
    /// </summary>
    private void OpenPluginSettings(string pluginId)
        => this.FindAncestorOfType<SettingsWindowView>()?.NavigateToPlugin(pluginId);


    /// <summary>
    /// Snapshots the current codecs + the extensions claimed by loaded plugins.
    /// </summary>
    private void SnapshotCodecs()
    {
        _codecs = Core.CodecRegistry.GetCodecInfos();
        _pluginExts.Clear();
        _pluginEncodeExts.Clear();
        _decoderCache.Clear();
        _encoderCache.Clear();

        foreach (var codec in _codecs)
        {
            if (!codec.IsPlugin) continue;
            foreach (var ext in codec.DecodingExtensions) _pluginExts.Add(ext);
            foreach (var ext in codec.EncodingExtensions) _pluginEncodeExts.Add(ext);
        }
    }


    /// <summary>
    /// Re-snapshots codecs and rebuilds the table after a plugin is enabled/disabled/installed/removed.
    /// </summary>
    public void RefreshCodecFormats()
    {
        SnapshotCodecs();
        RebuildTable();
    }


    /// <summary>
    /// Returns the codec that would decode the extension: the highest-priority claimant, else the
    /// fallback decoder that sniffs anything. Memoized; the filter hits this per row per keystroke.
    /// </summary>
    private CodecInfo? DecodeCodecFor(string ext)
    {
        if (_decoderCache.TryGetValue(ext, out var cached)) return cached;

        CodecInfo? found = null;

        // _codecs is ordered by decode priority (highest first)
        foreach (var codec in _codecs)
        {
            if (codec.DecodingExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                found = codec;
                break;
            }
        }

        // not explicitly claimed -> the fallback decoder, flagged rather than inferred from an
        // empty extension list (per-extension filtering can leave a plugin proxy empty too)
        found ??= _codecs.FirstOrDefault(c => c.IsFallback);

        _decoderCache[ext] = found;
        return found;
    }


    /// <summary>
    /// Returns the codec that would write the extension, or <c>null</c> when none can. Uses the
    /// registry's own selection so this matches what a save actually does.
    /// </summary>
    private CodecInfo? EncodeCodecFor(string ext)
    {
        if (_encoderCache.TryGetValue(ext, out var cached)) return cached;

        var found = Core.CodecRegistry.GetEncodeCodecInfo(ext);
        _encoderCache[ext] = found;
        return found;
    }


    /// <summary>
    /// Friendly name of the decoder for the extension.
    /// </summary>
    private string DecodeCodecNameFor(string ext) => DecodeCodecFor(ext)?.CodecName ?? string.Empty;


    /// <summary>
    /// Friendly name of the encoder for the extension.
    /// </summary>
    private string EncodeCodecNameFor(string ext) => EncodeCodecFor(ext)?.CodecName ?? string.Empty;

    #endregion // File formats

}

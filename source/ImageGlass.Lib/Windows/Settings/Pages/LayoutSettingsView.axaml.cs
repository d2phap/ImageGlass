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
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ImageGlass.Common.Windows;

public partial class LayoutSettingsView : SettingsPageView
{
    /// <summary>
    /// A single drop slot in the visual arranger: the host grid plus which control/position it represents.
    /// </summary>
    private sealed record LayoutZone(Grid Grid, LayoutControl Owner, LayoutPosition Position);


    // image-info tags the status bar understands (see AppStatusInfo.Text)
    private static readonly string[] _availableTags =
    [
        nameof(AppStatusInfo.Name),
        nameof(AppStatusInfo.Path),
        nameof(AppStatusInfo.FileSize),
        nameof(AppStatusInfo.ModifiedDateTime),
        nameof(AppStatusInfo.Dimension),
        nameof(AppStatusInfo.FrameCount),
        nameof(AppStatusInfo.ListCount),
        nameof(AppStatusInfo.Zoom),
        nameof(AppStatusInfo.ColorSpace),
        nameof(AppStatusInfo.DPI),
        nameof(AppStatusInfo.HdrInfo),
        nameof(AppStatusInfo.ExifRating),
        nameof(AppStatusInfo.ExifDateTime),
        nameof(AppStatusInfo.ExifDateTimeOriginal),
        nameof(AppStatusInfo.DateTimeAuto),
        nameof(AppStatusInfo.AppName),
    ];

    // canonical in-text separator for the image-info tags
    private const string TAG_SEPARATOR = "; ";

    private PhButton _toolbarChip = null!;
    private PhButton _galleryChip = null!;
    private List<LayoutZone> _zones = [];
    private readonly Dictionary<Grid, TextBlock> _placeholders = [];

    // guards the combo <-> arranger two-way sync from re-entering
    private bool _suppressComboEvents;

    // the floating chip shown while dragging; hosted on the window overlay so it isn't clipped
    private PhButton _ghost = null!;
    private Panel? _ghostHost; // the overlay layer hosting the ghost (falls back to PART_DragLayer)

    // drag state
    private PhButton? _dragChip;
    private LayoutControl _dragControl;
    private Point _dragStart;
    private bool _isDragging;


    public LayoutSettingsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// Creates the page bound to the given working-copy view model.
    /// </summary>
    public LayoutSettingsView(SettingsViewModel vm, SettingsNavId navId, LangId? pageLabel = null) : this()
    {
        Initialize(vm, navId, pageLabel);
    }


    protected override void Build()
    {
        // Window
        BindToggle(PART_ShowAppIcon, ConfigId.ShowAppIcon,
            LangId.Settings_ShowAppIcon, LangId.Settings_Window, true);
        BindToggle(PART_EnableCenterWindowFit, ConfigId.EnableCenterWindowFit,
            LangId.Settings_EnableCenterWindowFit, LangId.Settings_Window, true);

        // Image information
        BuildImageInfoTags();
        BindEnumDropdown(PART_ImageInfoOverlayMode, ConfigId.ImageInfoOverlayMode, ImageInfoOverlayMode.BorderlessAutoHide,
            LangId.Settings_ImageInfoOverlay, LangId.Settings_ImageInfo);

        // Controls
        BuildPositionCombos();
        BuildArranger();

        // lock the hand-wired composite editors when admin-managed (the reset button and the drag
        // arranger aren't reached by the registry disable pass; the reset button also re-stages via
        // the textbox, so it must be disabled too)
        DisableIfLocked(ConfigId.ImageInfoTags, PART_ImageInfoTags, PART_ResetImageInfoTags);
        DisableIfLocked(ConfigId.Layout, PART_ToolbarPosition, PART_GalleryPosition, PART_Preview);
    }


    #region Image info tags

    /// <summary>
    /// Binds the image-info tags text box (semicolon-separated) plus its reset link, and shows the
    /// available tags as a selectable, semicolon-separated list below it.
    /// </summary>
    private void BuildImageInfoTags()
    {
        var tags = VM.GetValue(ConfigId.ImageInfoTags,
            new ObservableCollection<string>(Config.DefaultImageInfoTags));
        PART_ImageInfoTags.Text = string.Join(TAG_SEPARATOR, tags);

        // stage while typing; normalize the displayed text once editing ends
        PART_ImageInfoTags.TextChanged += (_, _) => StageImageInfoTags(reformat: false);
        PART_ImageInfoTags.LostFocus += (_, _) => StageImageInfoTags(reformat: true);

        SetLocalizedText(PART_ResetImageInfoTags, LangId._ResetToDefault);
        PART_ResetImageInfoTags.Click += (_, _) =>
            PART_ImageInfoTags.Text = string.Join(TAG_SEPARATOR, Config.DefaultImageInfoTags);

        // available tags: selectable so they can be copied, separated by the same delimiter
        PART_AvailableTags.Text = string.Join(TAG_SEPARATOR, _availableTags);

        RegisterSearchKey(PART_ImageInfoTags, LangId.Settings_ImageInfoTags,
            ConfigId.ImageInfoTags, LangId.Settings_ImageInfo);
    }


    /// <summary>
    /// Parses the text box (tokens split on semicolons, commas, spaces or line breaks), keeps only known
    /// tags in canonical casing without duplicates, and stages the result. When <paramref name="reformat"/>
    /// is set, the box is rewritten as a clean, semicolon-separated list.
    /// </summary>
    private void StageImageInfoTags(bool reformat)
    {
        var tokens = (PART_ImageInfoTags.Text ?? string.Empty)
            .Split([';', ',', ' ', '\t', '\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var tags = new List<string>(tokens.Length);
        foreach (var token in tokens)
        {
            // match against the known tags (case-insensitive) and keep the canonical name; drop unknowns
            var canonical = Array.Find(_availableTags,
                t => t.Equals(token, StringComparison.OrdinalIgnoreCase));
            if (canonical is not null && !tags.Contains(canonical)) tags.Add(canonical);
        }

        VM.SetValue(ConfigId.ImageInfoTags, new ObservableCollection<string>(tags));

        if (reformat)
        {
            var normalized = string.Join(TAG_SEPARATOR, tags);
            if (!string.Equals(PART_ImageInfoTags.Text, normalized, StringComparison.Ordinal))
                PART_ImageInfoTags.Text = normalized;
        }
    }

    #endregion // Image info tags


    #region Position combos

    private void BuildPositionCombos()
    {
        FillPositionCombo(PART_ToolbarPosition, [LayoutPosition.Top, LayoutPosition.Bottom]);
        FillPositionCombo(PART_GalleryPosition,
            [LayoutPosition.Top, LayoutPosition.Bottom, LayoutPosition.Left, LayoutPosition.Right]);

        PART_ToolbarPosition.SelectionChanged += (_, _) =>
        {
            if (_suppressComboEvents) return;
            if (PART_ToolbarPosition.SelectedItem is ComboBoxItem { Tag: LayoutPosition pos })
                ApplyPosition(LayoutControl.Toolbar, pos);
        };
        PART_GalleryPosition.SelectionChanged += (_, _) =>
        {
            if (_suppressComboEvents) return;
            if (PART_GalleryPosition.SelectedItem is ComboBoxItem { Tag: LayoutPosition pos })
                ApplyPosition(LayoutControl.Gallery, pos);
        };

        RegisterSearchKey(PART_ToolbarPosition, LangId.Settings_Layout_ToolbarPosition,
            ConfigId.Layout, LangId.Settings_Controls);
        RegisterSearchKey(PART_GalleryPosition, LangId.Settings_Layout_GalleryPosition,
            ConfigId.Layout, LangId.Settings_Controls);
    }


    /// <summary>
    /// Adds a localized item per position to the combo, tagging each with its <see cref="LayoutPosition"/>.
    /// </summary>
    private void FillPositionCombo(ComboBox combo, LayoutPosition[] positions)
    {
        foreach (var pos in positions)
        {
            var item = new ComboBoxItem { Tag = pos };
            var key = Lang.GetKey($"_Position_{pos}");
            AddLangRefresher(() => item.Content = key is { } k ? Core.Lang[k] : pos.ToString());
            combo.Items.Add(item);
        }
    }


    private static void SelectComboPosition(ComboBox combo, LayoutPosition pos)
    {
        foreach (var obj in combo.Items)
        {
            if (obj is ComboBoxItem { Tag: LayoutPosition p } item && p == pos)
            {
                combo.SelectedItem = item;
                return;
            }
        }
    }

    #endregion // Position combos


    #region Layout arranger

    private void BuildArranger()
    {
        // draggable control chips
        _toolbarChip = CreateChip(LangId.Settings_Layout_Toolbar);
        _galleryChip = CreateChip(LangId.Settings_Layout_Gallery);

        // the floating ghost (hidden until a drag starts; parented to its host only while dragging)
        _ghost = new PhButton
        {
            Variant = PhButtonVariant.Outline,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsHitTestVisible = false,
            IsVisible = false,
        };

        // slot definitions (host grid + the control/position it stands for)
        _zones =
        [
            new(PART_ZoneToolbarTop, LayoutControl.Toolbar, LayoutPosition.Top),
            new(PART_ZoneToolbarBottom, LayoutControl.Toolbar, LayoutPosition.Bottom),
            new(PART_ZoneGalleryTop, LayoutControl.Gallery, LayoutPosition.Top),
            new(PART_ZoneGalleryBottom, LayoutControl.Gallery, LayoutPosition.Bottom),
            new(PART_ZoneGalleryLeft, LayoutControl.Gallery, LayoutPosition.Left),
            new(PART_ZoneGalleryRight, LayoutControl.Gallery, LayoutPosition.Right),
        ];

        // a faint label inside each empty slot showing just the control name
        foreach (var zone in _zones)
        {
            var ownerKey = zone.Owner == LayoutControl.Toolbar
                ? LangId.Settings_Layout_Toolbar
                : LangId.Settings_Layout_Gallery;

            var label = new TextBlock { Classes = { "slotLabel" } };
            AddLangRefresher(() => label.Text = Core.Lang[ownerKey]);

            _placeholders[zone.Grid] = label;
            zone.Grid.Children.Add(label);
        }

        // sync dropdowns to the current config, then draw the chips
        _suppressComboEvents = true;
        SelectComboPosition(PART_ToolbarPosition, GetPosition(LayoutControl.Toolbar));
        SelectComboPosition(PART_GalleryPosition, GetPosition(LayoutControl.Gallery));
        _suppressComboEvents = false;

        RenderArranger();
    }


    private PhButton CreateChip(LangId labelKey)
    {
        var chip = new PhButton
        {
            Variant = PhButtonVariant.Outline,
            Cursor = new Cursor(StandardCursorType.Hand),
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        AddLangRefresher(() => chip.Text = Core.Lang[labelKey]);

        // PhButton (a Button) marks pointer events handled, so listen on the tunnel route
        // with handledEventsToo to still drive the drag
        chip.AddHandler(InputElement.PointerPressedEvent, Chip_PointerPressed,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        chip.AddHandler(InputElement.PointerMovedEvent, Chip_PointerMoved,
            RoutingStrategies.Tunnel, handledEventsToo: true);
        chip.AddHandler(InputElement.PointerReleasedEvent, Chip_PointerReleased,
            RoutingStrategies.Tunnel, handledEventsToo: true);

        return chip;
    }


    /// <summary>
    /// Re-homes both chips into the slots matching the current config and toggles the empty-slot labels.
    /// </summary>
    private void RenderArranger()
    {
        PlaceChip(_toolbarChip, LayoutControl.Toolbar, GetPosition(LayoutControl.Toolbar));
        PlaceChip(_galleryChip, LayoutControl.Gallery, GetPosition(LayoutControl.Gallery));

        foreach (var zone in _zones)
        {
            var hasChip = zone.Grid.Children.Contains(_toolbarChip)
                || zone.Grid.Children.Contains(_galleryChip);
            _placeholders[zone.Grid].IsVisible = !hasChip;
        }
    }


    private void PlaceChip(PhButton chip, LayoutControl owner, LayoutPosition pos)
    {
        // fill the tall side slots; keep the default button height in the horizontal bars
        chip.VerticalAlignment = pos is LayoutPosition.Left or LayoutPosition.Right
            ? VerticalAlignment.Stretch
            : VerticalAlignment.Center;

        var target = _zones.First(z => z.Owner == owner && z.Position == pos).Grid;
        if (ReferenceEquals(chip.Parent, target)) return;

        (chip.Parent as Grid)?.Children.Remove(chip);
        target.Children.Add(chip);
    }


    /// <summary>
    /// Stages a new position, keeps the matching dropdown in sync, and redraws the arranger.
    /// </summary>
    private void ApplyPosition(LayoutControl control, LayoutPosition pos)
    {
        SetPosition(control, pos);

        _suppressComboEvents = true;
        SelectComboPosition(
            control == LayoutControl.Toolbar ? PART_ToolbarPosition : PART_GalleryPosition, pos);
        _suppressComboEvents = false;

        RenderArranger();
    }


    /// <summary>
    /// Gets the staged position of a control, applying defaults (toolbar=top, gallery=bottom) and
    /// clamping the toolbar away from the left/right edges it cannot occupy.
    /// </summary>
    private LayoutPosition GetPosition(LayoutControl control)
    {
        var layout = VM.GetValue(ConfigId.Layout, new Dictionary<LayoutControl, LayoutPosition>());
        var defaultPos = control == LayoutControl.Toolbar ? LayoutPosition.Top : LayoutPosition.Bottom;
        var pos = layout.TryGetValue(control, out var p) ? p : defaultPos;

        if (control == LayoutControl.Toolbar && pos is LayoutPosition.Left or LayoutPosition.Right)
            pos = LayoutPosition.Top;

        return pos;
    }


    private void SetPosition(LayoutControl control, LayoutPosition pos)
    {
        var layout = new Dictionary<LayoutControl, LayoutPosition>(
            VM.GetValue(ConfigId.Layout, new Dictionary<LayoutControl, LayoutPosition>()));
        layout[control] = pos;
        VM.SetValue(ConfigId.Layout, layout);
    }

    #endregion // Layout arranger


    #region Drag-and-drop

    private void Chip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not PhButton chip) return;
        if (!e.GetCurrentPoint(chip).Properties.IsLeftButtonPressed) return;

        _dragChip = chip;
        _dragControl = ReferenceEquals(chip, _toolbarChip) ? LayoutControl.Toolbar : LayoutControl.Gallery;
        _dragStart = e.GetPosition(PART_DragLayer);
        _isDragging = false;

        // don't mark the event handled: let the button enter its pressed state (content nudge + dim)
        e.Pointer.Capture(chip);
    }


    private void Chip_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragChip is null || !ReferenceEquals(sender, _dragChip)) return;
        if (!ReferenceEquals(e.Pointer.Captured, _dragChip)) return;

        var pos = e.GetPosition(PART_DragLayer);

        // ignore tiny moves so a click doesn't start a drag
        if (!_isDragging)
        {
            var delta = pos - _dragStart;
            if (Math.Abs(delta.X) < 3 && Math.Abs(delta.Y) < 3) return;

            StartGhost();
        }

        // float the ghost centered under the cursor (in its host's coordinate space)
        var host = _ghostHost ?? (Panel)PART_DragLayer;
        var gpos = e.GetPosition(host);
        Canvas.SetLeft(_ghost, gpos.X - _ghost.Width / 2);
        Canvas.SetTop(_ghost, gpos.Y - _ghost.Height / 2);
        UpdateHoveredZone(e);
    }


    private void Chip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragChip is null || !ReferenceEquals(sender, _dragChip)) return;

        var control = _dragControl;
        var target = _isDragging ? FindZoneAt(e) : null;

        e.Pointer.Capture(null);
        EndGhost();

        _dragChip = null;
        _isDragging = false;

        // apply only when dropped on a different, valid slot; otherwise just restore the dimmed chip
        if (target is not null && target.Owner == control && target.Position != GetPosition(control))
        {
            ApplyPosition(control, target.Position);
        }
        else
        {
            RenderArranger();
        }
    }


    /// <summary>
    /// Begins the floating-ghost visual: dims the source chip and shows a same-sized copy on the overlay.
    /// </summary>
    private void StartGhost()
    {
        _isDragging = true;
        _dragChip!.Opacity = 0.35;

        // host the ghost on the window overlay so it isn't clipped by the preview box / scroll
        // viewer; fall back to the local drag layer if the overlay is unavailable
        _ghostHost = OverlayLayer.GetOverlayLayer(this) ?? (Panel)PART_DragLayer;
        (_ghost.Parent as Panel)?.Children.Remove(_ghost);
        _ghostHost.Children.Add(_ghost);

        _ghost.Text = _dragChip.Text;
        _ghost.Width = _dragChip.Bounds.Width;
        _ghost.Height = _dragChip.Bounds.Height;
        _ghost.IsVisible = true;

        HighlightValidZones(true);
    }


    /// <summary>
    /// Ends the ghost visual: hides the overlay copy and un-dims the source chip.
    /// </summary>
    private void EndGhost()
    {
        _ghost.IsVisible = false;

        // unparent the ghost from whichever host it was added to
        var host = _ghostHost ?? (Panel)PART_DragLayer;
        host.Children.Remove(_ghost);
        _ghostHost = null;

        if (_dragChip is not null) _dragChip.Opacity = 1d;
        HighlightValidZones(false);
    }


    /// <summary>
    /// The slots the dragged control may actually be dropped on: same owner, excluding its current slot.
    /// </summary>
    private IEnumerable<LayoutZone> ValidDropZones()
    {
        var current = GetPosition(_dragControl);
        return _zones.Where(z => z.Owner == _dragControl && z.Position != current);
    }


    private void HighlightValidZones(bool on)
    {
        foreach (var zone in ValidDropZones())
        {
            if (DashOf(zone) is not { } dash) continue;

            SetClass(dash, "valid", on);
            if (!on) SetClass(dash, "hover", false);
        }
    }


    private void UpdateHoveredZone(PointerEventArgs e)
    {
        var hovered = FindZoneAt(e);
        foreach (var zone in ValidDropZones())
        {
            if (DashOf(zone) is { } dash) SetClass(dash, "hover", ReferenceEquals(hovered, zone));
        }
    }


    /// <summary>
    /// Gets the dashed-outline overlay that sits beside the slot's border (structure:
    /// <c>wrapper Grid &gt; [Border.zone &gt; zone.Grid], [Rectangle.zoneDash]</c>).
    /// </summary>
    private static Rectangle? DashOf(LayoutZone zone)
        => (zone.Grid.Parent as Border)?.Parent is Grid wrapper
            ? wrapper.Children.OfType<Rectangle>().FirstOrDefault()
            : null;


    private static void SetClass(StyledElement el, string className, bool on)
    {
        var has = el.Classes.Contains(className);
        if (on && !has) el.Classes.Add(className);
        else if (!on && has) el.Classes.Remove(className);
    }


    /// <summary>
    /// Returns the valid slot whose host border is under the pointer, or <c>null</c>.
    /// </summary>
    private LayoutZone? FindZoneAt(PointerEventArgs e)
    {
        foreach (var zone in ValidDropZones())
        {
            if (zone.Grid.Parent is not Border border) continue;

            var p = e.GetPosition(border);
            if (p.X >= 0 && p.Y >= 0 && p.X <= border.Bounds.Width && p.Y <= border.Bounds.Height)
            {
                return zone;
            }
        }

        return null;
    }

    #endregion // Drag-and-drop

}

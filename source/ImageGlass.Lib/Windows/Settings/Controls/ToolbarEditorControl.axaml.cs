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
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using ImageGlass.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGlass.Common.Windows;

/// <summary>
/// The drag-and-drop toolbar arranger used by the Toolbar settings page.
/// </summary>
public partial class ToolbarEditorControl : PhControl
{
    /// <summary>
    /// Identifies one of the three button lists shown in the editor.
    /// </summary>
    private enum EditorGroup { Primary, Secondary, Available }

    // editor chip size (icon only); kept fixed so chips stay uniform regardless of the toolbar icon size
    private const double ICON_SIZE = 24;
    private const double CHIP_PADDING = 6;
    private const double DRAG_THRESHOLD = 3;

    // working copies (clones); the catalog is never mutated
    private readonly List<ToolbarItemModel> _primary = [];
    private readonly List<ToolbarItemModel> _secondary = [];
    private readonly List<ToolbarItemModel> _available = [];

    // cache of parsed SVG sources by icon path so re-renders never hit the disk (keeps drag/drop snappy)
    private readonly Dictionary<string, SvgSource?> _svgCache = new(StringComparer.OrdinalIgnoreCase);

    // the built-in button catalog, built once (Config.BuiltInToolbarItems rebuilds it on each access)
    private List<ToolbarItemModel>? _catalog;
    private List<ToolbarItemModel> Catalog => _catalog ??= [.. Config.BuiltInToolbarItems];

    // drag state
    private Control? _dragChip;
    private ToolbarItemModel? _dragModel;
    private EditorGroup _dragSource;
    private Point _dragStart;
    private bool _isDragging;
    private bool _suppressClick; // set when a drag occurs so the chip's Click (open dialog) is skipped

    // drag visuals
    private Border? _ghost;
    private Border? _ghostTag; // "Delete" chip shown on the ghost when a custom button is dragged over Available
    private Border? _marker; // insertion line; stays on PART_DragLayer (inside the editor)
    private Panel? _ghostHost; // hosts the ghost: the window OverlayLayer so it isn't clipped

    // true while dragging a custom (non-built-in) current button: dropping it on Available deletes it for good
    private bool _dragCanDelete;

    // the chip to briefly highlight after a move (set just before a re-render)
    private ToolbarItemModel? _justMoved;

    // the model whose chip should receive focus after the next re-render (keyboard edits)
    private ToolbarItemModel? _focusAfterRender;


    /// <summary>
    /// Raised after any edit (drag, menu action, or reset) so the host can re-stage the buttons.
    /// </summary>
    public event EventHandler? ButtonsChanged;


    public ToolbarEditorControl()
    {
        InitializeComponent();

        PART_AddCustomBtn.Click += (_, _) => OnAddCustomClicked();
        PART_ResetBtn.Click += (_, _) => ResetToDefault();

        // Tab moves between groups; the control itself is focusable so settings-search navigation
        // can land here (forwarded into the Current buttons by OnGotFocus).
        Focusable = true;
        KeyboardNavigation.SetTabNavigation(PART_PrimaryGroup, KeyboardNavigationMode.Once);
        KeyboardNavigation.SetTabNavigation(PART_SecondaryGroup, KeyboardNavigationMode.Once);
        KeyboardNavigation.SetTabNavigation(PART_AvailableGroup, KeyboardNavigationMode.Once);

        // tunnel + handledEventsToo so we see Enter / Delete before the focused chip (a button) consumes them
        AddHandler(KeyDownEvent, OnEditorKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
    }


    #region Public API

    /// <summary>
    /// Loads the given toolbar buttons into the editor (cloned into working copies) and renders them.
    /// </summary>
    public void LoadButtons(IEnumerable<ToolbarItemModel> current)
    {
        _primary.Clear();
        _secondary.Clear();

        foreach (var item in current)
        {
            var clone = Clone(item);
            if (clone.Alignment == ToolbarItemAlignment.Right) _secondary.Add(clone);
            else _primary.Add(clone);
        }

        RecomputeAvailable();
        RenderAll();
    }


    /// <summary>
    /// Gets the current buttons as a flat collection (primary group first, then secondary),
    /// with each item's <see cref="ToolbarItemModel.Alignment"/> set to match its group.
    /// </summary>
    public ObservableCollection<ToolbarItemModel> CurrentButtons
    {
        get
        {
            var list = new ObservableCollection<ToolbarItemModel>();
            foreach (var m in _primary) { m.Alignment = ToolbarItemAlignment.Left; list.Add(m); }
            foreach (var m in _secondary) { m.Alignment = ToolbarItemAlignment.Right; list.Add(m); }
            return list;
        }
    }

    #endregion // Public API


    #region Control events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        // the theme (and thus icon paths) may have changed while this page was detached;
        // drop cached icons so they reload for the current dark/light pack
        _svgCache.Clear();
        base.OnLoaded(e); // base triggers OnIgLanguageChanged -> RenderAll with fresh icons
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        PART_AddCustomBtn.Text = Core.Lang[LangId.Settings_Toolbar_AddCustomButton];
        PART_ResetBtn.Text = Core.Lang[LangId._ResetToDefault];

        // tooltips and the available-list sort order are language-dependent
        RecomputeAvailable();
        RenderAll();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);

        // theme icon paths change with the dark/light pack: drop the cache and reload icons
        _svgCache.Clear();
        RenderAll();
    }


    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        // settings-search navigation focuses the editor itself; forward focus into the
        // Current buttons so keyboard users start where they can arrange the toolbar
        // (guard on IsFocused so a quick Tab-through that already left isn't yanked back)
        if (ReferenceEquals(e.Source, this))
        {
            Dispatcher.UIThread.Post(() => { if (IsFocused) FocusFirstButton(); }, DispatcherPriority.Input);
        }
    }

    #endregion // Control events


    #region Model helpers

    private List<ToolbarItemModel> ListFor(EditorGroup g) => g switch
    {
        EditorGroup.Primary => _primary,
        EditorGroup.Secondary => _secondary,
        _ => _available,
    };

    private WrapPanel PanelFor(EditorGroup g) => g switch
    {
        EditorGroup.Primary => PART_PrimaryGroup,
        EditorGroup.Secondary => PART_SecondaryGroup,
        _ => PART_AvailableGroup,
    };

    private Border ZoneFor(EditorGroup g) => g switch
    {
        EditorGroup.Primary => PART_PrimaryZone,
        EditorGroup.Secondary => PART_SecondaryZone,
        _ => PART_AvailableZone,
    };

    private Rectangle DashFor(EditorGroup g) => g switch
    {
        EditorGroup.Primary => PART_PrimaryDash,
        EditorGroup.Secondary => PART_SecondaryDash,
        _ => PART_AvailableDash,
    };


    /// <summary>
    /// Creates a shallow copy of a toolbar item (the click action is shared, it is never mutated).
    /// </summary>
    private static ToolbarItemModel Clone(ToolbarItemModel m) => new()
    {
        Id = m.Id,
        Image = m.Image,
        Text = m.Text,
        ShowText = m.ShowText,
        ConfigBinding = m.ConfigBinding,
        ConfigBindingValue = m.ConfigBindingValue,
        Alignment = m.Alignment,
        OnClick = m.OnClick,
    };


    /// <summary>
    /// Rebuilds the "Available" list: a separator template followed by every built-in button
    /// not already in the current toolbar, sorted by localized name.
    /// </summary>
    private void RecomputeAvailable()
    {
        _available.Clear();
        _available.Add(ToolbarItemModel.Separator);

        var currentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in _primary) if (!m.IsSeparator) currentIds.Add(m.Id);
        foreach (var m in _secondary) if (!m.IsSeparator) currentIds.Add(m.Id);

        var others = Catalog
            .Where(b => !currentIds.Contains(b.Id))
            .OrderBy(b => b.DisplayText, StringComparer.CurrentCultureIgnoreCase);
        _available.AddRange(others);
    }


    /// <summary>
    /// Gets a parsed SVG source for the given icon path, caching it so repeated renders avoid disk I/O.
    /// </summary>
    private SvgSource? GetSvg(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (_svgCache.TryGetValue(path, out var cached)) return cached;

        SvgSource? src = null;
        try { src = SvgSource.Load(path); }
        catch { }

        _svgCache[path] = src;
        return src;
    }

    #endregion // Model helpers


    #region Rendering

    private void RenderAll()
    {
        RenderGroup(EditorGroup.Primary);
        RenderGroup(EditorGroup.Secondary);
        RenderGroup(EditorGroup.Available);
        _justMoved = null;

        // a keyboard edit asked to keep focus on a specific button: re-focus it once the
        // fresh chips are in the tree (only set by keyboard/menu edits, never by mouse drag)
        var focusModel = _focusAfterRender;
        _focusAfterRender = null;

        if (focusModel is not null)
        {
            Dispatcher.UIThread.Post(() => FocusChipFor(focusModel), DispatcherPriority.Input);
        }
    }


    private void RenderGroup(EditorGroup g)
    {
        var panel = PanelFor(g);
        panel.Children.Clear();

        foreach (var model in ListFor(g))
        {
            panel.Children.Add(BuildChip(model));
        }
    }


    /// <summary>
    /// Builds a draggable chip (icon only, with a tooltip) for a toolbar item.
    /// </summary>
    private PhToolButton BuildChip(ToolbarItemModel model)
    {
        var name = model.IsSeparator ? Core.Lang[LangId._Separator] : model.DisplayText;

        var chip = new PhToolButton
        {
            Tag = model,
            Padding = new Thickness(CHIP_PADDING),
            Margin = new Thickness(3),
            Cursor = new Cursor(StandardCursorType.Hand),
            Focusable = true, // focusable so Tab reaches chips and Enter/Delete act on them
            Content = BuildIconVisual(model),
            Transitions = [new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(180) }],
        };

        // a button with no text gets no tooltip; an empty tip string still pops an empty box up
        if (!string.IsNullOrWhiteSpace(name)) ToolTip.SetTip(chip, name);

        AutomationProperties.SetName(chip, name); // screen-reader label

        // PhToolButton (a button) marks pointer events handled, so listen on the tunnel route
        // with handledEventsToo to still drive the drag
        chip.AddHandler(PointerPressedEvent, Chip_PointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        chip.AddHandler(PointerMovedEvent, Chip_PointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        chip.AddHandler(PointerReleasedEvent, Chip_PointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        chip.AddHandler(PointerCaptureLostEvent, Chip_PointerCaptureLost, handledEventsToo: true);

        // the button's action (click, tap, or Space/Enter on the focused chip) opens the dialog
        chip.Click += Chip_Click;

        // brief fade-in on the chip that was just moved/added so the landing spot is easy to spot
        if (ReferenceEquals(model, _justMoved))
        {
            chip.Opacity = 0.25;
            Dispatcher.UIThread.Post(() => chip.Opacity = 1, DispatcherPriority.Background);
        }

        return chip;
    }


    /// <summary>
    /// Builds the icon visual for a chip / ghost: the button's SVG icon, or a thin line for a separator.
    /// </summary>
    private Control BuildIconVisual(ToolbarItemModel model)
    {
        if (model.IsSeparator)
        {
            var lineBrush = Resx.Get<IBrush>(ResxId.TextControlForeground);

            return new Border
            {
                Width = ICON_SIZE,
                Height = ICON_SIZE,
                Child = new Border
                {
                    Width = 2,
                    Height = ICON_SIZE * 0.7,
                    CornerRadius = new CornerRadius(1),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.5,
                    Background = lineBrush,
                },
            };
        }

        var src = GetSvg(model.ImagePath);
        if (src is null)
        {
            // same hatch the toolbar shows for a button that runs something but has no icon
            if (!model.IsPlaceholderIconVisible) return new Border { Width = ICON_SIZE, Height = ICON_SIZE };

            return new PathIcon
            {
                Width = ICON_SIZE,
                Height = ICON_SIZE,
                Opacity = 0.6,
                Data = Resx.GetIcon(ResxIconId.IconPlaceholder),
            };
        }

        return new Image
        {
            Width = ICON_SIZE,
            Height = ICON_SIZE,
            Source = new SvgImage { Source = src },
        };
    }


    #endregion // Rendering


    #region Edit operations

    private void ResetToDefault()
    {
        LoadButtons(Config.DefaultToolbarItems);
        ButtonsChanged?.Invoke(this, EventArgs.Empty);
    }


    private void OnAddCustomClicked() => _ = OpenAddDialogAsync();


    /// <summary>
    /// Opens the editor dialog to create a new custom button, then appends it to the matching
    /// current group (Primary, or Secondary when right-aligned).
    /// </summary>
    private async Task OpenAddDialogAsync()
    {
        var win = new ToolbarButtonEditWindow(null, CollectTakenIds(except: null), isBuiltIn: false);
        if (await win.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) != DialogExitCode.OK) return;
        if (win.ResultModel is not { } model) return;

        if (model.Alignment == ToolbarItemAlignment.Right) _secondary.Add(model);
        else _primary.Add(model);

        _justMoved = model;
        _focusAfterRender = model;
        Commit();
    }


    /// <summary>
    /// Opens the editor dialog for an existing button: read-only for built-in buttons, editable for
    /// custom ones. Separators have nothing to edit. Applies the edit in place on success.
    /// </summary>
    private async Task OpenEditDialogAsync(ToolbarItemModel model)
    {
        if (model.IsSeparator) return;

        var win = new ToolbarButtonEditWindow(model, CollectTakenIds(except: model), IsBuiltIn(model));
        if (await win.ShowAsync(TopLevel.GetTopLevel(this) as PhWindow) != DialogExitCode.OK) return;
        if (win.ResultModel is not { } edited) return; // cancelled or read-only built-in

        ApplyEdit(model, edited);
        _justMoved = model;
        _focusAfterRender = model;
        Commit();
    }


    /// <summary>
    /// Copies the edited values onto the existing model (keeping its identity), hopping it between the
    /// Primary and Secondary groups when its alignment changed.
    /// </summary>
    private void ApplyEdit(ToolbarItemModel model, ToolbarItemModel edited)
    {
        model.Id = edited.Id;
        model.Image = edited.Image;
        model.Text = edited.Text;
        model.ShowText = edited.ShowText;
        model.ConfigBinding = edited.ConfigBinding;
        model.ConfigBindingValue = edited.ConfigBindingValue;
        model.OnClick = edited.OnClick;

        var group = GroupOf(model);
        if (group == EditorGroup.Primary && edited.Alignment == ToolbarItemAlignment.Right)
        {
            _primary.Remove(model);
            model.Alignment = ToolbarItemAlignment.Right;
            _secondary.Add(model);
        }
        else if (group == EditorGroup.Secondary && edited.Alignment == ToolbarItemAlignment.Left)
        {
            _secondary.Remove(model);
            model.Alignment = ToolbarItemAlignment.Left;
            _primary.Add(model);
        }
        else
        {
            model.Alignment = edited.Alignment;
        }
    }


    /// <summary>
    /// Gets the button IDs already in use across the built-in catalog and the current groups,
    /// excluding the given model's own ID (so editing a button doesn't clash with itself).
    /// </summary>
    private HashSet<string> CollectTakenIds(ToolbarItemModel? except)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Catalog) if (!c.IsSeparator) set.Add(c.Id);
        foreach (var m in _primary) if (!m.IsSeparator) set.Add(m.Id);
        foreach (var m in _secondary) if (!m.IsSeparator) set.Add(m.Id);
        if (except is not null && !except.IsSeparator) set.Remove(except.Id);
        return set;
    }


    /// <summary>
    /// Whether the model is a built-in button (its ID exists in the built-in catalog), and so is
    /// shown read-only in the editor dialog.
    /// </summary>
    private bool IsBuiltIn(ToolbarItemModel model) => !model.IsSeparator
        && Catalog.Any(c => !c.IsSeparator && c.Id.Equals(model.Id, StringComparison.OrdinalIgnoreCase));


    /// <summary>
    /// Recomputes the available list, re-renders, and notifies the host of the change.
    /// </summary>
    private void Commit()
    {
        RecomputeAvailable();
        RenderAll();
        ButtonsChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion // Edit operations


    #region Drag and drop

    private void Chip_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control chip || chip.Tag is not ToolbarItemModel model) return;

        // left button starts a drag; right button falls through to the context menu
        if (!e.GetCurrentPoint(chip).Properties.IsLeftButtonPressed) return;

        _suppressClick = false; // fresh gesture; a drag will set this to skip the button's Click
        _dragChip = chip;
        _dragModel = model;
        _dragSource = GroupOfChip(chip);
        _dragStart = e.GetPosition(PART_DragLayer);
        _isDragging = false;

        // don't mark handled: let the button show its pressed state
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
            if (Math.Abs(delta.X) < DRAG_THRESHOLD && Math.Abs(delta.Y) < DRAG_THRESHOLD) return;
            StartDrag(e);
        }

        // float the ghost centered under the cursor (in its host's coordinate space)
        var host = _ghostHost ?? (Panel)PART_DragLayer;
        var gpos = e.GetPosition(host);
        Canvas.SetLeft(_ghost!, gpos.X - _ghost!.Width / 2);
        Canvas.SetTop(_ghost, gpos.Y - _ghost.Height / 2);

        // float the "Delete" chip just above the ghost (its visibility is driven by UpdateDropTarget).
        // Fall back to an estimated height on the first frame before it has been laid out.
        if (_ghostTag is not null)
        {
            var tagH = _ghostTag.Bounds.Height > 0 ? _ghostTag.Bounds.Height : 24;
            Canvas.SetLeft(_ghostTag, gpos.X - _ghost.Width / 2);
            Canvas.SetTop(_ghostTag, gpos.Y - _ghost.Height / 2 - tagH - 4);
        }

        UpdateDropTarget(e);
    }


    private void Chip_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragChip is null || !ReferenceEquals(sender, _dragChip)) return;

        if (!_isDragging)
        {
            // not a drag: leave the pointer capture intact so the button raises its Click, which
            // opens the dialog via Chip_Click (matching Space/Enter). Just drop our drag tracking.
            _dragChip = null;
            _dragModel = null;
            return;
        }

        var srcModel = _dragModel!;
        var srcGroup = _dragSource;
        var target = HitZone(e);
        var dropIndex = target is EditorGroup tg && tg != EditorGroup.Available
            ? ComputeInsertIndex(PanelFor(tg), e.GetPosition(PanelFor(tg)))
            : 0;

        e.Pointer.Capture(null);
        EndDrag();

        _dragChip = null;
        _dragModel = null;
        _isDragging = false;

        if (target is EditorGroup dst)
        {
            PerformDrop(srcModel, srcGroup, dst, dropIndex);
        }
        else
        {
            // dropped outside any zone: just restore the dimmed chip
            RenderAll();
        }
    }


    private void Chip_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (!ReferenceEquals(sender, _dragChip)) return;

        var wasDragging = _isDragging;
        EndDrag();
        _dragChip = null;
        _dragModel = null;
        _isDragging = false;

        if (wasDragging) RenderAll();
    }


    /// <summary>
    /// The chip's button action (mouse click, touch tap, or Space/Enter on the focused chip): opens
    /// the edit dialog. Skipped right after a drag, which already did the rearranging.
    /// </summary>
    private void Chip_Click(object? sender, RoutedEventArgs e)
    {
        if (_suppressClick) { _suppressClick = false; return; }
        if (sender is not Control chip || chip.Tag is not ToolbarItemModel model) return;

        // post so the click/key event unwinds before the modal opens
        Dispatcher.UIThread.Post(() => _ = OpenEditDialogAsync(model));
    }


    /// <summary>
    /// Applies a drop: dropping onto the Available zone removes the button from the toolbar; dropping
    /// onto a group adds (from Available) or moves (from another/the same group) at the given index.
    /// </summary>
    private void PerformDrop(ToolbarItemModel src, EditorGroup srcGroup, EditorGroup dst, int index)
    {
        if (dst == EditorGroup.Available)
        {
            // only current buttons can be dropped here (Available isn't a valid target for itself)
            if (srcGroup == EditorGroup.Available) { RenderAll(); return; }
            ListFor(srcGroup).Remove(src);
        }
        else
        {
            var dstList = ListFor(dst);
            var dstAlignment = dst == EditorGroup.Secondary
                ? ToolbarItemAlignment.Right
                : ToolbarItemAlignment.Left;

            if (srcGroup == EditorGroup.Available)
            {
                // adding a copy from the catalog (a fresh instance for separators)
                var clone = src.IsSeparator ? ToolbarItemModel.Separator : Clone(src);
                index = Math.Clamp(index, 0, dstList.Count);
                dstList.Insert(index, clone);
                clone.Alignment = dstAlignment;
                _justMoved = clone;
            }
            else
            {
                // moving an existing button (reorder within a group, or across groups)
                var srcList = ListFor(srcGroup);
                var oldIndex = srcList.IndexOf(src);
                if (oldIndex < 0) { RenderAll(); return; }

                srcList.RemoveAt(oldIndex);
                if (srcGroup == dst && oldIndex < index) index--;
                index = Math.Clamp(index, 0, dstList.Count);
                dstList.Insert(index, src);
                src.Alignment = dstAlignment;
                _justMoved = src;
            }
        }

        Commit();
    }


    private void StartDrag(PointerEventArgs e)
    {
        _isDragging = true;
        _dragChip!.Opacity = 0.35;
        _suppressClick = true; // this gesture became a drag; skip the button's Click on release
        _ghostHost = OverlayLayer.GetOverlayLayer(this) ?? (Panel)PART_DragLayer;

        var accent = Resx.Get<Color>(ResxId.SystemAccentColor).ToBrush();
        var ghostBg = Resx.Get<IBrush>(ResxId.IG_BackgroundNeutralBrush);

        // floating ghost
        _ghost = new Border
        {
            Padding = new Thickness(6),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = accent,
            Background = ghostBg,
            BoxShadow = BoxShadows.Parse("0 4 12 0 #40000000"),
            Width = _dragChip.Bounds.Width,
            Height = _dragChip.Bounds.Height,
            IsHitTestVisible = false,
            Child = BuildIconVisual(_dragModel!),
        };
        _ghostHost.Children.Add(_ghost);

        // dragging chip for custom button
        _dragCanDelete = _dragSource != EditorGroup.Available
            && !_dragModel!.IsSeparator
            && !IsBuiltIn(_dragModel);
        if (_dragCanDelete)
        {
            var dangerBg = Resx.Get<IBrush>(ResxId.IG_TextDangerBrush);
            var dangerFg = Core.Theme.BaseColor.ToBrush();

            _ghostTag = new Border
            {
                IsVisible = false,
                IsHitTestVisible = false,
                Background = dangerBg,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(8, 3),
                BoxShadow = BoxShadows.Parse("0 2 6 0 #40000000"),
                Child = new TextBlock
                {
                    Text = Core.Lang[LangId._Delete],
                    Foreground = dangerFg,
                    FontWeight = FontWeight.SemiBold,
                    FontSize = 12,
                },
            };
            _ghostHost.Children.Add(_ghostTag);
        }

        // outline valid drop zones (not the group the button came from)
        var hovered = HitZone(e);
        foreach (var g in ValidTargets())
        {
            var show = g != _dragSource;
            SetZoneState(g, valid: show, hover: show && g == hovered);
        }
    }


    private void EndDrag()
    {
        var host = _ghostHost ?? (Panel)PART_DragLayer;

        if (_ghost is not null)
        {
            host.Children.Remove(_ghost);
            _ghost = null;
        }
        if (_ghostTag is not null)
        {
            host.Children.Remove(_ghostTag);
            _ghostTag = null;
        }
        _ghostHost = null;
        _dragCanDelete = false;

        HideMarker();
        ResetZoneStates();

        if (_dragChip is not null) _dragChip.Opacity = 1;
    }


    /// <summary>
    /// Gets the groups a button can be dropped onto: a button from Available can only land in a group;
    /// a current button can also be dropped on the Available zone to remove it.
    /// </summary>
    private IEnumerable<EditorGroup> ValidTargets()
    {
        yield return EditorGroup.Primary;
        yield return EditorGroup.Secondary;
        if (_dragSource != EditorGroup.Available) yield return EditorGroup.Available;
    }


    /// <summary>
    /// Highlights the hovered zone (and positions the insertion marker for a group), keeping the
    /// dashed "valid" outline on the other valid zones.
    /// </summary>
    private void UpdateDropTarget(PointerEventArgs e)
    {
        HideMarker();

        var hovered = HitZone(e);
        foreach (var g in ValidTargets())
        {
            // don't outline the group the button came from
            var show = g != _dragSource;
            var isHover = show && g == hovered;

            // a custom button over Available means "delete": flag the zone red instead of accent
            var isDanger = isHover && g == EditorGroup.Available && _dragCanDelete;
            SetZoneState(g, valid: show, hover: isHover && !isDanger, danger: isDanger);
        }

        // surface the "Delete" chip only while the custom button is over the Available zone
        if (_ghostTag is not null) _ghostTag.IsVisible = hovered == EditorGroup.Available;

        if (hovered is EditorGroup hg && hg != EditorGroup.Available)
        {
            var panel = PanelFor(hg);
            ShowMarker(panel, ComputeInsertIndex(panel, e.GetPosition(panel)));
        }
    }


    /// <summary>
    /// Returns the valid drop group whose zone is under the pointer, or <c>null</c>.
    /// </summary>
    private EditorGroup? HitZone(PointerEventArgs e)
    {
        foreach (var g in ValidTargets())
        {
            var zone = ZoneFor(g);
            var p = e.GetPosition(zone);
            if (p.X >= 0 && p.Y >= 0 && p.X <= zone.Bounds.Width && p.Y <= zone.Bounds.Height)
            {
                return g;
            }
        }

        return null;
    }


    private EditorGroup GroupOfChip(Control chip)
    {
        if (PART_PrimaryGroup.Children.Contains(chip)) return EditorGroup.Primary;
        if (PART_SecondaryGroup.Children.Contains(chip)) return EditorGroup.Secondary;
        return EditorGroup.Available;
    }


    private void SetZoneState(EditorGroup g, bool valid, bool hover, bool danger = false)
    {
        SetClass(ZoneFor(g), "valid", valid);
        SetClass(ZoneFor(g), "hover", hover);
        SetClass(DashFor(g), "valid", valid);
        SetClass(DashFor(g), "hover", hover);
        SetClass(DashFor(g), "danger", danger);
    }


    private void ResetZoneStates()
    {
        SetZoneState(EditorGroup.Primary, false, false);
        SetZoneState(EditorGroup.Secondary, false, false);
        SetZoneState(EditorGroup.Available, false, false);
    }


    /// <summary>
    /// Computes the index at which a dropped chip should be inserted, based on the pointer position
    /// over the wrapped chips (row-aware: respects line wrapping).
    /// </summary>
    private static int ComputeInsertIndex(WrapPanel panel, Point p)
    {
        var chips = panel.Children.Where(c => c.Tag is ToolbarItemModel).ToList();
        var n = chips.Count;
        if (n == 0) return 0;

        for (var i = 0; i < n; i++)
        {
            var b = chips[i].Bounds;

            // chip's row is entirely above the pointer: keep scanning
            if (p.Y > b.Bottom) continue;

            // pointer is above this chip's row entirely: insert before it
            if (p.Y < b.Top) return i;

            // pointer is within this chip's row
            if (p.X < b.X + b.Width / 2) return i;

            // past this chip; if it's the last on its row, insert after it
            var lastInRow = i == n - 1 || chips[i + 1].Bounds.Top > b.Bottom - 0.5;
            if (lastInRow) return i + 1;
        }

        return n;
    }


    private void ShowMarker(WrapPanel panel, int index)
    {
        EnsureMarker();

        var chips = panel.Children.Where(c => c.Tag is ToolbarItemModel).ToList();
        double x, y, height;

        if (chips.Count == 0)
        {
            var topLeft = panel.TranslatePoint(default, PART_DragLayer) ?? default;
            x = topLeft.X;
            y = topLeft.Y;
            height = ICON_SIZE + CHIP_PADDING * 2;
        }
        else if (index >= chips.Count)
        {
            var last = chips[^1];
            var corner = last.TranslatePoint(new Point(last.Bounds.Width, 0), PART_DragLayer) ?? default;
            x = corner.X + 1;
            y = corner.Y;
            height = last.Bounds.Height;
        }
        else
        {
            var chip = chips[index];
            var corner = chip.TranslatePoint(default, PART_DragLayer) ?? default;
            x = corner.X - 4;
            y = corner.Y;
            height = chip.Bounds.Height;
        }

        _marker!.Height = height;
        Canvas.SetLeft(_marker, x);
        Canvas.SetTop(_marker, y);
        _marker.IsVisible = true;
    }


    private void EnsureMarker()
    {
        if (_marker is not null) return;

        _marker = new Border { Classes = { "dropMarker" }, IsVisible = false };
        PART_DragLayer.Children.Add(_marker);
    }


    private void HideMarker()
    {
        if (_marker is not null) _marker.IsVisible = false;
    }


    private static void SetClass(StyledElement el, string className, bool on)
    {
        // only mutate when the state actually changes (avoids duplicate classes + redundant
        // style invalidation on every pointer move while dragging)
        var has = el.Classes.Contains(className);
        if (on && !has) el.Classes.Add(className);
        else if (!on && has) el.Classes.Remove(className);
    }

    #endregion // Drag and drop


    #region Keyboard navigation

    /// <summary>
    /// Keyboard navigation on the focused chip: arrow keys move focus between chips, Delete removes a
    /// current button. Space/Enter are left for the chip's default button activation, which opens the
    /// dialog via Chip_Click. Runs on the tunnel route so the arrows beat default focus handling.
    /// </summary>
    private void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is not Control focused
            || focused.Tag is not ToolbarItemModel model)
            return;

        // a key press clears any stale drag suppression so Space/Enter activation isn't swallowed
        _suppressClick = false;

        var group = GroupOfChip(focused);

        switch (e.Key)
        {
            case Key.Left:
                e.Handled = MoveFocusHorizontal(focused, group, -1);
                break;
            case Key.Right:
                e.Handled = MoveFocusHorizontal(focused, group, +1);
                break;
            case Key.Up:
                e.Handled = MoveFocusVertical(focused, group, up: true);
                break;
            case Key.Down:
                e.Handled = MoveFocusVertical(focused, group, up: false);
                break;

            case Key.Delete:
            case Key.Back:
                if (group != EditorGroup.Available) e.Handled = RemoveByKeyboard(model, group);
                break;
        }
    }


    /// <summary>
    /// Moves focus to the previous/next chip in the group, flowing across the two side-by-side
    /// Current groups at their inner edge. Returns whether focus actually moved.
    /// </summary>
    private bool MoveFocusHorizontal(Control chip, EditorGroup group, int delta)
    {
        var chips = ChipsOf(PanelFor(group));
        var i = chips.IndexOf(chip);
        if (i < 0) return false;

        var target = i + delta;
        if (target >= 0 && target < chips.Count)
        {
            chips[target].Focus(NavigationMethod.Tab);
            return true;
        }

        // hop between Primary and Secondary at their inner edge
        Control? cross = null;
        if (group == EditorGroup.Primary && delta > 0) cross = FirstChip(PART_SecondaryGroup);
        else if (group == EditorGroup.Secondary && delta < 0) cross = LastChip(PART_PrimaryGroup);

        if (cross is null) return false;
        cross.Focus(NavigationMethod.Tab);
        return true;
    }


    /// <summary>
    /// Moves focus to the chip on the nearest adjacent row whose horizontal centre is closest
    /// (so Up/Down feel natural across wrapped rows). Returns whether focus moved.
    /// </summary>
    private bool MoveFocusVertical(Control chip, EditorGroup group, bool up)
    {
        var chips = ChipsOf(PanelFor(group));
        if (chips.Count == 0) return false;

        var cur = chip.Bounds;
        var cx = cur.X + cur.Width / 2;

        // find the nearest adjacent row by its Top
        double? rowTop = null;
        foreach (var c in chips)
        {
            if (ReferenceEquals(c, chip)) continue;
            var top = c.Bounds.Top;
            if (up)
            {
                if (top >= cur.Top - 0.5) continue;                 // not on a higher row
                if (rowTop is null || top > rowTop) rowTop = top;   // closest above = largest Top
            }
            else
            {
                if (top <= cur.Top + 0.5) continue;                 // not on a lower row
                if (rowTop is null || top < rowTop) rowTop = top;   // closest below = smallest Top
            }
        }
        if (rowTop is null) return false;

        // on that row, pick the chip whose horizontal centre is closest
        Control? best = null;
        var bestDx = double.PositiveInfinity;
        foreach (var c in chips)
        {
            if (ReferenceEquals(c, chip)) continue;
            var b = c.Bounds;
            if (Math.Abs(b.Top - rowTop.Value) > 1) continue;
            var dx = Math.Abs((b.X + b.Width / 2) - cx);
            if (dx < bestDx) { bestDx = dx; best = c; }
        }

        best?.Focus(NavigationMethod.Tab);
        return best is not null;
    }


    /// <summary>
    /// Returns which working list currently holds the given model, or <c>null</c>.
    /// </summary>
    private EditorGroup? GroupOf(ToolbarItemModel model)
    {
        if (_primary.Contains(model)) return EditorGroup.Primary;
        if (_secondary.Contains(model)) return EditorGroup.Secondary;
        if (_available.Contains(model)) return EditorGroup.Available;
        return null;
    }


    /// <summary>
    /// Removes a current button (keyboard), keeping focus on a neighbour, or the first button if the
    /// group is now empty.
    /// </summary>
    private bool RemoveByKeyboard(ToolbarItemModel model, EditorGroup group)
    {
        var list = ListFor(group);
        var i = list.IndexOf(model);
        if (i < 0) return false;

        list.Remove(model);
        _focusAfterRender = list.Count > 0 ? list[Math.Clamp(i, 0, list.Count - 1)] : null;
        var emptied = _focusAfterRender is null;
        Commit();

        if (emptied) Dispatcher.UIThread.Post(FocusFirstButton, DispatcherPriority.Input);
        return true;
    }


    /// <summary>
    /// Focuses the first button in the editor (Primary, else Secondary, else Available). Called when
    /// the page is navigated to so keyboard users land on the Current buttons.
    /// </summary>
    public void FocusFirstButton()
        => (FirstChip(PART_PrimaryGroup) ?? FirstChip(PART_SecondaryGroup) ?? FirstChip(PART_AvailableGroup))
            ?.Focus(NavigationMethod.Tab);


    /// <summary>
    /// Focuses the chip currently bound to the given model (across all three groups).
    /// </summary>
    private void FocusChipFor(ToolbarItemModel model)
    {
        foreach (var panel in new Panel[] { PART_PrimaryGroup, PART_SecondaryGroup, PART_AvailableGroup })
        {
            foreach (var c in panel.Children)
            {
                if (ReferenceEquals(c.Tag, model)) { c.Focus(NavigationMethod.Tab); return; }
            }
        }
    }


    private static List<Control> ChipsOf(Panel panel)
        => panel.Children.Where(c => c.Tag is ToolbarItemModel).ToList();

    private static Control? FirstChip(Panel panel)
        => panel.Children.FirstOrDefault(c => c.Tag is ToolbarItemModel);

    private static Control? LastChip(Panel panel)
        => panel.Children.LastOrDefault(c => c.Tag is ToolbarItemModel);

    #endregion // Keyboard navigation

}

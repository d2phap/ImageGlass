/*
ImageGlass - A Fast, Seamless Photo Viewer
Copyright (C) 2010 - 2027 DUONG DIEU PHAP
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
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageGlass.Common;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGlass.UI;


/// <summary>
/// A read-only data table. Rows are virtualized: only the rows intersecting the viewport are
/// kept in the visual tree, so a long table stays cheap to lay out while the window resizes.
/// </summary>
public partial class PhTableControl : PhControl
{
    private static readonly Thickness CELL_PADDING = new(10, 6);
    private static readonly TimeSpan REVEAL_DURATION = TimeSpan.FromMilliseconds(120);
    private const double FLASH_OPACITY = 0.18;
    private const int MAX_SCROLL_PASSES = 4; // scroll-into-view corrections over estimated offsets

    private readonly Border _frame;
    private readonly RowsPanel _panel;
    private readonly ScrollViewer _scroll;
    private readonly TextBlock _emptyLabel;

    private readonly List<RowSlot> _rows = [];

    // header visuals: full-width layers + one label per column (null for the actions column).
    // Always realized, and pinned to the top while the body scrolls under them.
    private Control[] _headerSpanners = [];
    private Control?[] _headerCells = [];
    private Border? _headerBg; // opaque header fill (re-resolved on theme change)

    private int _hoveredRow = -1;
    private int _focusedRow = -1;
    private RowSlot? _flashSlot;
    private DispatcherTimer? _flashTimer;


    #region Public Properties

    /// <summary>
    /// Gets, sets the text shown (in place of the table) when there are no rows.
    /// </summary>
    public string EmptyText
    {
        get => _emptyLabel.Text ?? string.Empty;
        set => _emptyLabel.Text = value;
    }

    #endregion // Public Properties


    public PhTableControl()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;

        // transparent background so the whole area (incl. gaps) reports pointer moves for row hit-testing
        _panel = new RowsPanel(this) { Background = Brushes.Transparent };
        _panel.PointerMoved += Panel_PointerMoved;
        _panel.PointerExited += Panel_PointerExited;

        // scrolls internally when the control is given a MaxHeight (e.g. a page fitting it to the window)
        _scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Content = _panel,
        };
        // realization is driven by the panel's EffectiveViewportChanged (in-pass), and the header is
        // pinned by arranging it at the scroll offset, so nothing is wired to ScrollChanged here

        _frame = new Border
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            BorderThickness = new Thickness(1),
            ClipToBounds = true,
            Child = _scroll,
        };
        _frame[!Border.BackgroundProperty] = Resx.CreateBinding(ResxId.IG_BackgroundNeutralBrush);
        _frame[!Border.BorderBrushProperty] = Resx.CreateBinding(ResxId.IG_BorderControlBrush);
        _frame[!Border.CornerRadiusProperty] = Resx.CreateBinding(ResxId.ControlCornerRadius);

        _emptyLabel = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            FontStyle = FontStyle.Italic,
            Opacity = 0.6,
            IsVisible = false,
        };

        Content = new Panel { Children = { _frame, _emptyLabel } };
    }


    #region Public Methods

    /// <summary>
    /// Rebuilds the whole table: a header row from <paramref name="columns"/> plus one row per
    /// entry in <paramref name="rows"/> (an implicit actions column is appended after the columns).
    /// Shows <see cref="EmptyText"/> instead when <paramref name="rows"/> is empty.
    /// </summary>
    public void Build(IReadOnlyList<PhTableColumn> columns, IReadOnlyList<PhTableRow> rows)
    {
        _flashTimer?.Stop();
        _flashSlot = null;
        _rows.Clear();
        _headerSpanners = [];
        _headerCells = [];
        _headerBg = null;
        _hoveredRow = _focusedRow = -1;

        // a rebuilt (or filtered) list belongs at the top; keeping the old offset would realize the
        // wrong band for one frame, since Extent is only republished on the next arrange
        _scroll.Offset = default;

        var hasRows = rows.Count > 0;
        _emptyLabel.IsVisible = !hasRows;
        _frame.IsVisible = hasRows;

        if (!hasRows)
        {
            _panel.Reset(columns, 0);
            return;
        }

        var contentCols = columns.Count;
        var totalCols = contentCols + 1; // + actions column

        // header: an opaque fill (so rows don't bleed through while pinned), the labels, the underline
        _headerBg = new Border { IsHitTestVisible = false };
        ApplyHeaderBackground();
        _headerSpanners = [_headerBg, HLine(ResxId.IG_BorderControlBrush, VerticalAlignment.Bottom)];
        foreach (var spanner in _headerSpanners) spanner.ZIndex = 1;

        _headerCells = new Control?[totalCols];
        for (var c = 0; c < contentCols; c++)
        {
            var column = columns[c];
            Control cell = column.HeaderContent is { } content
                ? WrapHeaderCell(content)
                : HeaderCell(column.Header);
            cell.ZIndex = 1;
            _headerCells[c] = cell;
        }

        for (var i = 0; i < rows.Count; i++) _rows.Add(BuildRow(rows[i], i, totalCols));

        _panel.Reset(columns, totalCols);
    }


    /// <summary>
    /// Gets how many always-realized header visuals lead <c>Children</c>, so the panel can insert
    /// realized rows after them and keep <c>Children</c> in row order.
    /// </summary>
    internal int HeaderVisualCount
    {
        get
        {
            var count = _headerSpanners.Length;
            foreach (var cell in _headerCells)
            {
                if (cell is not null) count++;
            }

            return count;
        }
    }


    /// <summary>
    /// Builds a text cell that truncates with an ellipsis (optionally capped to <paramref name="maxWidth"/>)
    /// and shows the full text in a tooltip. Pass <paramref name="selectable"/> for copy-able text,
    /// or <paramref name="muted"/> for an italic placeholder (e.g. an empty value).
    /// </summary>
    public static Control TextCell(string text, double maxWidth = 0,
        bool selectable = false, bool muted = false, FontFamily? font = null)
    {
        TextBlock tb = selectable ? new SelectableTextBlock() : new TextBlock();
        tb.Text = text;
        tb.Padding = CELL_PADDING;
        tb.VerticalAlignment = VerticalAlignment.Center;
        tb.TextTrimming = TextTrimming.CharacterEllipsis;
        tb.IsTabStop = false; // only the action buttons take tab focus
        if (maxWidth > 0) tb.MaxWidth = maxWidth;
        if (font is not null) tb.FontFamily = font;

        if (muted)
        {
            tb.FontStyle = FontStyle.Italic;
            tb.Opacity = 0.6;
        }
        else if (!string.IsNullOrEmpty(text))
        {
            ToolTip.SetTip(tb, text);
        }

        return tb;
    }


    /// <summary>
    /// Wraps custom cell <paramref name="content"/> with the standard cell padding, centered on the row.
    /// </summary>
    public static Border WrapCell(Control content)
    {
        content.VerticalAlignment = VerticalAlignment.Center;
        return new Border { Padding = CELL_PADDING, Child = content };
    }


    /// <summary>
    /// Scrolls the row carrying <paramref name="key"/> into view and pulses its accent background.
    /// No-op when no row matches.
    /// </summary>
    public void FlashRow(string key)
    {
        var index = _rows.FindIndex(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) FlashRow(index);
    }


    /// <summary>
    /// Scrolls the row at <paramref name="index"/> into view and pulses its accent background.
    /// </summary>
    public void FlashRow(int index)
    {
        if (index < 0 || index >= _rows.Count) return;
        var slot = _rows[index];

        // defer so a freshly (re)built table has had a layout pass before we scroll/measure
        Dispatcher.UIThread.Post(() =>
        {
            if (!_rows.Contains(slot)) return;

            BringRowIntoView(slot);

            // a pulse still running on another row would stay tinted once its timer is dropped
            _flashTimer?.Stop();
            if (_flashSlot is not null && _flashSlot != slot) _flashSlot.Flash.Opacity = 0;

            _flashSlot = slot;
            var pulses = 0;
            slot.Flash.Opacity = FLASH_OPACITY;

            // toggle opacity (smoothed by the fade transition); end hidden after a few pulses
            _flashTimer = new DispatcherTimer { Interval = REVEAL_DURATION + TimeSpan.FromMilliseconds(140) };
            _flashTimer.Tick += (_, _) =>
            {
                slot.Flash.Opacity = slot.Flash.Opacity > 0 ? 0 : FLASH_OPACITY;
                if (++pulses >= 5)
                {
                    slot.Flash.Opacity = 0;
                    _flashSlot = null;
                    _flashTimer!.Stop();
                }
            };
            _flashTimer.Start();
        }, DispatcherPriority.Loaded);
    }

    #endregion // Public Methods


    #region Scrolling

    /// <summary>
    /// Scrolls <paramref name="slot"/> into view and realizes it. Iterates because rows above the
    /// target may still be unmeasured, so the first scroll aims at an estimated offset; each pass
    /// measures the band it lands on and corrects. Stops as soon as the offset settles.
    /// </summary>
    private void BringRowIntoView(RowSlot slot)
    {
        for (var pass = 0; pass < MAX_SCROLL_PASSES; pass++)
        {
            var before = _scroll.Offset.Y;

            ScrollRowIntoView(slot);
            _panel.UpdateLayout();

            // settled once the target is realized and the offset stopped moving
            if (slot.IsRealized && Math.Abs(_scroll.Offset.Y - before) < 0.5) break;
        }
    }


    /// <summary>
    /// Scrolls <paramref name="slot"/> fully into view, keeping it clear of the pinned header.
    /// No-op when it is already visible.
    /// </summary>
    private void ScrollRowIntoView(RowSlot slot)
    {
        var headerHeight = _panel.HeaderHeight;
        var top = headerHeight + slot.Y;
        var bottom = top + _panel.RowHeightOf(slot);

        var viewport = _scroll.Viewport.Height;
        if (viewport <= 0) return;

        // the pinned header covers the top band of the viewport, so that is not usable space
        var viewTop = _scroll.Offset.Y + headerHeight;
        var viewBottom = _scroll.Offset.Y + viewport;
        if (top >= viewTop && bottom <= viewBottom) return;

        var target = top < viewTop ? top - headerHeight : bottom - viewport;
        var max = Math.Max(0, _scroll.Extent.Height - viewport);
        _scroll.Offset = new Vector(_scroll.Offset.X, Math.Clamp(target, 0, max));
    }


    #endregion // Scrolling


    #region Hover / focus reveal

    private void Panel_PointerMoved(object? sender, PointerEventArgs e)
    {
        // the header is arranged at the scroll offset, so its band is the same coordinate system as
        // the rows: ignore it, since it sits visually on top of them while scrolled
        var y = e.GetPosition(_panel).Y;
        var overHeader = y >= _scroll.Offset.Y && y < _scroll.Offset.Y + _panel.HeaderHeight;

        var index = overHeader ? -1 : _panel.RowAt(y);
        if (index == _hoveredRow) return;

        var previous = _hoveredRow;
        _hoveredRow = index;
        UpdateReveal(previous, index);
    }


    private void Panel_PointerExited(object? sender, PointerEventArgs e)
    {
        if (_hoveredRow == -1) return;

        var previous = _hoveredRow;
        _hoveredRow = -1;
        UpdateReveal(previous, -1);
    }


    /// <summary>
    /// Re-applies the reveal state of the rows whose hovered/focused status just changed.
    /// </summary>
    private void UpdateReveal(params int[] changed)
    {
        foreach (var index in changed)
        {
            if (index >= 0 && index < _rows.Count) RefreshRowReveal(_rows[index]);
        }
    }


    /// <summary>
    /// Applies the hover tint and the actions visibility for one row. Called on state changes and
    /// again whenever the row is realized, since an unrealized row cannot show them.
    /// </summary>
    private void RefreshRowReveal(RowSlot slot)
    {
        var isHovered = slot.Index == _hoveredRow;
        slot.Actions.Opacity = isHovered || slot.Index == _focusedRow ? 1 : 0;
        slot.Highlight.Opacity = isHovered ? 1 : 0;
    }


    private bool RowHasFocus(int row) => _rows[row].Buttons.Any(b => b.IsFocused);

    #endregion // Hover / focus reveal


    #region Keyboard navigation

    /// <summary>
    /// Keeps every row's actions reachable by Tab: virtualization leaves the off-screen rows out of
    /// the visual tree, so hops between rows are handled here (scroll + realize, then focus).
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || e.Key != Key.Tab) return;

        // plain Tab / Shift+Tab only
        var modifiers = e.KeyModifiers & ~KeyModifiers.Shift;
        if (modifiers != KeyModifiers.None) return;

        if (MoveActionFocus(back: e.KeyModifiers.HasFlag(KeyModifiers.Shift))) e.Handled = true;
    }


    /// <summary>
    /// Moves focus to the next (or previous) focusable in the table, crossing into rows that are
    /// not realized yet. Covers every focusable a cell may hold (action buttons, but also the
    /// toggles, checkboxes and link buttons consumers put in cells), not just the actions column.
    /// Returns <c>false</c> when the table is exhausted, so the default traversal can leave it.
    /// </summary>
    private bool MoveActionFocus(bool back)
    {
        if (!TryFindFocusedRow(out var row, out var focused)) return false;

        var step = back ? -1 : 1;

        // next focusable still inside the same row
        var current = FocusablesOf(_rows[row]);
        var at = current.IndexOf(focused);
        if (at >= 0)
        {
            var next = at + step;
            if (next >= 0 && next < current.Count) return current[next].Focus();
        }

        // otherwise the first/last focusable of the nearest row that has one. The scan works on
        // unrealized rows too (their cells keep Focusable/IsVisible while detached), so we only
        // scroll once a target is known instead of walking the table.
        for (var r = row + step; r >= 0 && r < _rows.Count; r += step)
        {
            var slot = _rows[r];
            var candidates = FocusablesOf(slot);
            if (candidates.Count == 0) continue;

            BringRowIntoView(slot);

            var target = back ? candidates[^1] : candidates[0];
            if (target.Focus()) return true;
        }

        return false;
    }


    /// <summary>
    /// The row's focusable descendants in visual order (content cells left to right, actions last).
    /// </summary>
    private static List<Control> FocusablesOf(RowSlot slot)
    {
        var found = new List<Control>();

        foreach (var cell in slot.Cells)
        {
            if (cell is not null) CollectFocusables(cell, found);
        }

        return found;
    }


    private static void CollectFocusables(Control control, List<Control> found)
    {
        if (!control.IsVisible || !control.IsEnabled) return;

        if (control.Focusable) found.Add(control);

        foreach (var child in control.GetVisualChildren())
        {
            if (child is Control c) CollectFocusables(c, found);
        }
    }


    /// <summary>
    /// Finds which row currently holds keyboard focus, and the focused control itself.
    /// </summary>
    private bool TryFindFocusedRow(out int row, out Control focused)
    {
        for (var r = 0; r < _rows.Count; r++)
        {
            foreach (var candidate in FocusablesOf(_rows[r]))
            {
                if (!candidate.IsFocused) continue;

                row = r;
                focused = candidate;
                return true;
            }
        }

        row = -1;
        focused = null!;
        return false;
    }

    #endregion // Keyboard navigation


    #region Cell builders

    /// <summary>
    /// Builds one row's visuals: the full-width hover/flash/separator layers, the content cells
    /// (padded to the column count) and the actions cell.
    /// </summary>
    private RowSlot BuildRow(PhTableRow spec, int index, int totalCols)
    {
        // full-row layer behind the cells: tints on hover.
        // uses the sidebar ListBoxItem's Fluent 2 SubtleFill (neutral, not accent) for a matching hover.
        var highlight = new Border { IsHitTestVisible = false, Opacity = 0 };
        highlight[!Border.BackgroundProperty] = new DynamicResourceExtension("PhListItemFillSecondary");
        highlight.Transitions = new Transitions { FadeTransition() };

        // accent flash layer (above the hover tint): pulsed by FlashRow to notify the user
        var flash = new Border { IsHitTestVisible = false, Opacity = 0 };
        flash[!Border.BackgroundProperty] = new DynamicResourceExtension("PhAccentFill");
        flash.Transitions = new Transitions { FadeTransition() };

        var layers = new List<Control> { highlight, flash };

        // separator above every row except the first
        if (index > 0) layers.Add(HLine(ResxId.IG_BorderNeutralBrush, VerticalAlignment.Top));

        var contentCols = totalCols - 1;
        var cells = new Control?[totalCols];
        for (var c = 0; c < contentCols && c < spec.Cells.Count; c++) cells[c] = spec.Cells[c];

        var (actionsCell, buttons) = BuildActionsCell(spec.Actions, index);
        cells[contentCols] = actionsCell;

        return new RowSlot(index, spec.Key, highlight, flash, actionsCell, buttons)
        {
            Layers = [.. layers],
            Cells = cells,
        };
    }


    /// <summary>
    /// Gives the sticky header an opaque fill (the theme neutral color forced to full alpha) so the
    /// rows scrolling beneath it don't bleed through. Re-resolved on theme change.
    /// </summary>
    private void ApplyHeaderBackground()
    {
        if (_headerBg is null) return;

        var c = Resx.Get<ISolidColorBrush>(ResxId.IG_BackgroundNeutralBrush)?.Color ?? Colors.Transparent;
        _headerBg.Background = new SolidColorBrush(new Color(255, c.R, c.G, c.B));
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        ApplyHeaderBackground();
    }


    private static TextBlock HeaderCell(string text) => new()
    {
        Text = text,
        FontWeight = FontWeight.SemiBold,
        Padding = CELL_PADDING,
        VerticalAlignment = VerticalAlignment.Center,
    };


    /// <summary>
    /// Wraps a custom header control with the standard cell padding, centered so it lines up
    /// with the plain text headers.
    /// </summary>
    private static Border WrapHeaderCell(Control content)
    {
        content.VerticalAlignment = VerticalAlignment.Center;
        return new Border { Padding = CELL_PADDING, Child = content };
    }


    /// <summary>
    /// A 1px horizontal rule whose color follows the theme (via a dynamic resource binding).
    /// </summary>
    private static Border HLine(ResxId brushId, VerticalAlignment align)
    {
        var line = new Border { Height = 1, VerticalAlignment = align };
        line[!Border.BackgroundProperty] = Resx.CreateBinding(brushId);
        return line;
    }


    /// <summary>
    /// Builds the actions cell (right-aligned icon buttons) and returns its hover-revealed
    /// wrapper plus the buttons (for focus tracking).
    /// </summary>
    private (Border cell, List<PhToolButton> buttons) BuildActionsCell(
        IReadOnlyList<PhTableAction> actions, int rowIndex)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttons = new List<PhToolButton>(actions.Count);
        foreach (var action in actions)
        {
            var btn = BuildActionButton(action, rowIndex);
            buttons.Add(btn);
            panel.Children.Add(btn);
        }

        var cell = new Border { Padding = new Thickness(8, 2), Opacity = 0, Child = panel };
        cell.Transitions = new Transitions { FadeTransition() };

        return (cell, buttons);
    }


    /// <summary>
    /// Builds one action button: a filled icon glyph, a tooltip, the click action, and the
    /// focus hooks that reveal the row while the button is focused.
    /// </summary>
    private PhToolButton BuildActionButton(PhTableAction action, int rowIndex)
    {
        var glyph = new Path
        {
            Width = Const.FONT_SIZE_BODY,
            Height = Const.FONT_SIZE_BODY,
            Data = Resx.GetIcon(action.Icon),
            Stretch = Stretch.Uniform,
        };
        glyph[!Shape.FillProperty] = Resx.CreateBinding(ResxId.TextControlForeground);

        var btn = new PhToolButton
        {
            Padding = new Thickness(7),
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = action.IsVisible,
            Content = glyph,
        };
        if (!string.IsNullOrEmpty(action.Tooltip)) ToolTip.SetTip(btn, action.Tooltip);

        var click = action.Click;
        btn.Click += (_, _) => click?.Invoke();

        // keep hidden actions reachable by Tab: reveal the row on focus, hide again when focus leaves it
        btn.GotFocus += (_, _) =>
        {
            var previous = _focusedRow;
            _focusedRow = rowIndex;
            UpdateReveal(previous, rowIndex);
        };
        btn.LostFocus += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            if (_focusedRow == rowIndex && rowIndex < _rows.Count && !RowHasFocus(rowIndex))
            {
                _focusedRow = -1;
                UpdateReveal(rowIndex);
            }
        });

        return btn;
    }


    private static DoubleTransition FadeTransition() => new()
    {
        Property = OpacityProperty,
        Duration = REVEAL_DURATION,
    };

    #endregion // Cell builders

}




/// <summary>
/// A content column of a <see cref="PhTableControl"/>.
/// </summary>
public sealed class PhTableColumn
{
    /// <summary>
    /// Gets, sets the (already localized) header text.
    /// </summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets a control rendered as the header instead of <see cref="Header"/>, for an
    /// interactive header (e.g. a select-all checkbox). Measured like any cell.
    /// </summary>
    public Control? HeaderContent { get; set; }

    /// <summary>
    /// Gets, sets whether the column fills remaining width (<c>*</c>); otherwise it auto-fits its content.
    /// </summary>
    public bool Star { get; set; }

    /// <summary>
    /// Gets, sets the column's minimum width (0 = none).
    /// </summary>
    public double MinWidth { get; set; }
}


/// <summary>
/// An action (icon button) shown in a <see cref="PhTableControl"/> row's actions column.
/// </summary>
public sealed class PhTableAction
{
    /// <summary>
    /// Gets, sets the button's icon glyph.
    /// </summary>
    public ResxIconId Icon { get; set; }

    /// <summary>
    /// Gets, sets the (already localized) tooltip text.
    /// </summary>
    public string Tooltip { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets whether the button is shown (still built when hidden, for future reveal).
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets, sets the click handler.
    /// </summary>
    public Action? Click { get; set; }
}


/// <summary>
/// One data row for <see cref="PhTableControl.Build"/>: the content cells (one per column) and
/// the row's actions.
/// </summary>
public sealed class PhTableRow
{
    /// <summary>
    /// Gets, sets the content cells, one per column (build with <see cref="PhTableControl.TextCell"/>
    /// / <see cref="PhTableControl.WrapCell"/>).
    /// </summary>
    public IReadOnlyList<Control> Cells { get; set; } = [];

    /// <summary>
    /// Gets, sets the row's actions, rendered as hover/focus-revealed icon buttons.
    /// </summary>
    public IReadOnlyList<PhTableAction> Actions { get; set; } = [];

    /// <summary>
    /// Gets, sets an optional identifier used to locate the row later (e.g. for <see cref="PhTableControl.FlashRow(string)"/>).
    /// </summary>
    public string? Key { get; set; }
}

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
using Avalonia.Controls;
using Avalonia.Layout;
using System;
using System.Collections.Generic;

namespace ImageGlass.UI;


partial class PhTableControl
{

    #region Row model

    /// <summary>
    /// The visuals and layout metrics of one data row. Kept for every row, realized or not: the
    /// cells are supplied by the caller and carry their own state, so they are never rebuilt.
    /// </summary>
    private sealed class RowSlot(int index, string? key, Border highlight, Border flash,
        Border actions, List<PhToolButton> buttons)
    {
        public int Index { get; } = index;
        public string? Key { get; } = key;
        public Border Highlight { get; } = highlight;
        public Border Flash { get; } = flash;
        public Border Actions { get; } = actions;
        public List<PhToolButton> Buttons { get; } = buttons;

        // full-row layers: hover tint, accent flash, top separator
        public Control[] Layers { get; init; } = [];

        // one entry per column with the actions cell last; null where the row supplied no cell
        public Control?[] Cells { get; init; } = [];

        // last measured height, NaN until the row has been measured once
        public double Height = double.NaN;

        // vertical offset below the header
        public double Y;

        public bool IsRealized;
    }

    #endregion // Row model


    #region Column layout

    /// <summary>
    /// Resolved column geometry shared by the header and every row: Auto columns fit the widest
    /// cell measured so far, Star columns split the width the Auto columns left over.
    /// </summary>
    private sealed class ColumnLayout
    {
        private bool[] _isStar = [];
        private double[] _minWidths = [];
        private double[] _autoMax = [];

        public double[] Widths { get; private set; } = [];
        public double[] Offsets { get; private set; } = [];

        /// <summary>
        /// Gets whether a cell measured wider than its Auto column, so the widths need re-resolving.
        /// </summary>
        public bool Dirty { get; private set; }

        public int Count => Widths.Length;

        public bool IsStar(int col) => _isStar[col];


        /// <summary>
        /// Re-seeds the layout for a new set of columns; the implicit trailing actions column is Auto.
        /// </summary>
        public void Reset(IReadOnlyList<PhTableColumn> columns, int totalCols)
        {
            _isStar = new bool[totalCols];
            _minWidths = new double[totalCols];
            _autoMax = new double[totalCols];
            Widths = new double[totalCols];
            Offsets = new double[totalCols];

            for (var c = 0; c < columns.Count && c < totalCols; c++)
            {
                _isStar[c] = columns[c].Star;
                _minWidths[c] = columns[c].MinWidth;
            }

            Dirty = false;
        }


        /// <summary>
        /// Grows an Auto column to fit a cell. The maximum is sticky, so columns don't jitter as
        /// rows are realized and dropped again while scrolling.
        /// </summary>
        public void Observe(int col, double width)
        {
            if (_isStar[col] || width <= _autoMax[col]) return;

            _autoMax[col] = width;
            Dirty = true;
        }


        /// <summary>
        /// Recomputes the column widths and x offsets for the given content width.
        /// </summary>
        public void Resolve(double availableWidth)
        {
            var used = 0d;
            var stars = 0;

            for (var c = 0; c < Count; c++)
            {
                if (_isStar[c])
                {
                    stars++;
                    continue;
                }

                Widths[c] = Math.Max(_autoMax[c], _minWidths[c]);
                used += Widths[c];
            }

            // like Grid, leftover width is only reclaimed by Star columns: with none, the trailing
            // actions column stays right after the last content column instead of moving to the edge
            if (stars > 0)
            {
                var share = (availableWidth - used) / stars;
                for (var c = 0; c < Count; c++)
                {
                    if (_isStar[c]) Widths[c] = Math.Max(_minWidths[c], share);
                }
            }

            var x = 0d;
            for (var c = 0; c < Count; c++)
            {
                Offsets[c] = x;
                x += Widths[c];
            }

            Dirty = false;
        }
    }

    #endregion // Column layout


    #region Virtualizing rows panel

    /// <summary>
    /// Lays out the header and the data rows, keeping only the rows that intersect the viewport
    /// (plus a buffer) in the visual tree. The panel always reports the full extent height, so the
    /// scrollbar is correct even though most rows are not realized.
    /// </summary>
    private sealed class RowsPanel(PhTableControl owner) : Panel
    {
        private const double REALIZE_BUFFER = 120; // extra px realized above and below the viewport
        private const double FALLBACK_VIEWPORT = 600; // before the ScrollViewer reports a viewport
        private const double FALLBACK_WIDTH = 600;
        private const double DEFAULT_ROW_HEIGHT = 34; // seeds the extent until a row is measured
        private const int MAX_WIDTH_PASSES = 3;

        private readonly ColumnLayout _layout = new();

        // the realized window is always contiguous
        private int _firstRealized = -1;
        private int _lastRealized = -1;

        private double _measuredSum;
        private int _measuredCount;
        private double _bodyHeight;

        // the visible band in panel coordinates, from EffectiveViewportChanged
        private Rect _viewport;


        /// <summary>
        /// Gets the measured height of the pinned header row.
        /// </summary>
        public double HeaderHeight { get; private set; }


        /// <summary>
        /// Gets the mean measured row height, used for the rows not measured yet.
        /// </summary>
        public double EstimatedRowHeight
            => _measuredCount > 0 ? _measuredSum / _measuredCount : DEFAULT_ROW_HEIGHT;


        /// <summary>
        /// Gets a row's measured height, falling back to the estimate while it is unmeasured.
        /// </summary>
        public double RowHeightOf(RowSlot slot)
            => double.IsNaN(slot.Height) ? EstimatedRowHeight : slot.Height;


        /// <summary>
        /// Drops every realized row and re-seeds the layout for a freshly built table.
        /// </summary>
        public void Reset(IReadOnlyList<PhTableColumn> columns, int totalCols)
        {
            Children.Clear();
            foreach (var slot in owner._rows) slot.IsRealized = false;

            _firstRealized = _lastRealized = -1;
            _measuredSum = 0;
            _measuredCount = 0;
            _bodyHeight = 0;
            HeaderHeight = 0;

            // the old band would realize the wrong rows; Build just reset the offset, so fall back
            // to a top-anchored band until the first EffectiveViewportChanged reports the real one
            _viewport = default;
            _layout.Reset(columns, totalCols);

            if (owner._rows.Count > 0)
            {
                foreach (var spanner in owner._headerSpanners) Children.Add(spanner);
                foreach (var cell in owner._headerCells)
                {
                    if (cell is not null) Children.Add(cell);
                }

                // offsets must exist before the first realization, or every row looks visible at y=0
                ResolveRowOffsets();
                EnsureRealized();
            }

            InvalidateMeasure();
        }


        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            EffectiveViewportChanged += Panel_EffectiveViewportChanged;
        }


        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            EffectiveViewportChanged -= Panel_EffectiveViewportChanged;
            base.OnDetachedFromVisualTree(e);
        }


        /// <summary>
        /// The realization trigger. This fires inside the layout pass (unlike ScrollViewer's
        /// ScrollChanged, which fires after the pass is already closed), so rows realized here are
        /// measured and arranged in the same frame instead of flashing empty for one frame.
        /// </summary>
        private void Panel_EffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            _viewport = e.EffectiveViewport;
            EnsureRealized();
        }


        /// <summary>
        /// Realizes the rows intersecting the viewport and drops the rest.
        /// </summary>
        public void EnsureRealized()
        {
            if (owner._rows.Count == 0) return;

            var (first, last) = VisibleRange();
            if (first == _firstRealized && last == _lastRealized) return;

            if (_firstRealized >= 0)
            {
                for (var i = _firstRealized; i <= _lastRealized; i++)
                {
                    if (i < first || i > last) Unrealize(owner._rows[i]);
                }
            }

            _firstRealized = first;
            _lastRealized = last;

            // realize in row order so Children order (= tab order) matches the visual order
            for (var i = first; i <= last; i++) Realize(owner._rows[i], i);

            InvalidateMeasure();
        }


        /// <summary>
        /// Returns the data-row index whose vertical band contains <paramref name="y"/>
        /// (panel space), or -1. Works for unrealized rows too.
        /// </summary>
        public int RowAt(double y)
        {
            var rows = owner._rows;
            var body = y - HeaderHeight;
            if (body < 0) return -1;

            for (var i = 0; i < rows.Count; i++)
            {
                var slot = rows[i];
                if (body < slot.Y) return -1;
                if (body < slot.Y + RowHeightOf(slot)) return i;
            }

            return -1;
        }


        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            if (owner._rows.Count == 0) return default;

            var width = ResolveWidth(availableSize);

            // the header labels seed every Auto column before the rows are measured
            MeasureHeader();

            // a row can widen an Auto column, which shrinks the Star columns and may re-wrap
            // their cells, so keep measuring until the widths settle
            var pass = 0;
            for (; pass < MAX_WIDTH_PASSES; pass++)
            {
                _layout.Resolve(width);
                MeasureRows(width);
                if (!_layout.Dirty) break;
            }

            var headerConstraint = new Size(width, HeaderHeight);
            foreach (var spanner in owner._headerSpanners) spanner.Measure(headerConstraint);

            ResolveRowOffsets();

            return new Size(width, HeaderHeight + _bodyHeight);
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            if (owner._rows.Count == 0) return finalSize;

            // reuse the frozen measure-time widths rather than re-resolving
            var offsetY = owner._scroll.Offset.Y;

            // the header is pinned by arranging it at the scroll offset: an Offset change re-runs
            // arrange (the presenter arranges us at -Offset) so it costs no measure and cannot lag
            foreach (var spanner in owner._headerSpanners)
            {
                spanner.Arrange(new Rect(0, offsetY, finalSize.Width, HeaderHeight));
            }
            for (var c = 0; c < owner._headerCells.Length; c++)
            {
                owner._headerCells[c]?.Arrange(
                    new Rect(_layout.Offsets[c], offsetY, _layout.Widths[c], HeaderHeight));
            }

            if (_firstRealized >= 0)
            {
                for (var i = _firstRealized; i <= _lastRealized; i++)
                {
                    ArrangeRow(owner._rows[i], finalSize.Width);
                }
            }

            // never report less than the viewport: the transparent background has to cover the
            // whole frame or the empty area below a short table stops reporting row hover
            return new Size(finalSize.Width, Math.Max(finalSize.Height, HeaderHeight + _bodyHeight));
        }


        private void MeasureHeader()
        {
            var constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            var height = 0d;

            for (var c = 0; c < owner._headerCells.Length; c++)
            {
                var cell = owner._headerCells[c];
                if (cell is null) continue;

                cell.Measure(constraint);
                _layout.Observe(c, cell.DesiredSize.Width);
                height = Math.Max(height, cell.DesiredSize.Height);
            }

            HeaderHeight = height;
        }


        private void MeasureRows(double width)
        {
            if (_firstRealized < 0) return;

            var layerConstraint = new Size(width, double.PositiveInfinity);

            for (var i = _firstRealized; i <= _lastRealized; i++)
            {
                var slot = owner._rows[i];
                var height = 0d;

                for (var c = 0; c < slot.Cells.Length; c++)
                {
                    var cell = slot.Cells[c];
                    if (cell is null) continue;

                    // Auto columns measure against infinity (like Grid) so the content sets the
                    // width; a Star cell needs its real width or wrapping content mis-measures
                    var available = _layout.IsStar(c) ? _layout.Widths[c] : double.PositiveInfinity;
                    cell.Measure(new Size(available, double.PositiveInfinity));

                    _layout.Observe(c, cell.DesiredSize.Width);
                    height = Math.Max(height, cell.DesiredSize.Height);
                }

                foreach (var layer in slot.Layers) layer.Measure(layerConstraint);

                SetRowHeight(slot, height);
            }
        }


        private void ArrangeRow(RowSlot slot, double width)
        {
            var top = HeaderHeight + slot.Y;
            var height = RowHeightOf(slot);

            // the layers span the row; their own alignment puts the 1px separator on the top edge
            var full = new Rect(0, top, width, height);
            foreach (var layer in slot.Layers) layer.Arrange(full);

            for (var c = 0; c < slot.Cells.Length; c++)
            {
                slot.Cells[c]?.Arrange(new Rect(_layout.Offsets[c], top, _layout.Widths[c], height));
            }
        }


        /// <summary>
        /// Records a freshly measured row height, keeping the running mean used as the estimate.
        /// </summary>
        private void SetRowHeight(RowSlot slot, double height)
        {
            if (!double.IsNaN(slot.Height))
            {
                if (Math.Abs(slot.Height - height) < 0.5) return;

                _measuredSum -= slot.Height;
                _measuredCount--;
            }

            slot.Height = height;
            _measuredSum += height;
            _measuredCount++;
        }


        /// <summary>
        /// Recomputes every row's vertical offset (and the total body height) from the measured
        /// heights, using the estimate for the rows not measured yet.
        /// </summary>
        private void ResolveRowOffsets()
        {
            var rows = owner._rows;
            var estimate = EstimatedRowHeight;
            var y = 0d;

            for (var i = 0; i < rows.Count; i++)
            {
                var slot = rows[i];
                slot.Y = y;
                y += double.IsNaN(slot.Height) ? estimate : slot.Height;
            }

            _bodyHeight = y;
        }


        private double ResolveWidth(Size available)
        {
            if (!double.IsInfinity(available.Width) && available.Width > 0) return available.Width;

            var viewport = owner._scroll.Viewport.Width;
            return viewport > 0 ? viewport : FALLBACK_WIDTH;
        }

        #endregion // Layout


        #region Realization

        /// <summary>
        /// Returns the inclusive row range to keep realized for the current visible band.
        /// </summary>
        private (int First, int Last) VisibleRange()
        {
            var rows = owner._rows;

            // EffectiveViewportChanged already gives the band in panel coordinates, clipped by the
            // frame; fall back to the presenter only before the first pass has reported one
            var height = _viewport.Height > 0 ? _viewport.Height : FALLBACK_VIEWPORT;
            var viewTop = _viewport.Height > 0 ? _viewport.Y : owner._scroll.Offset.Y;

            var top = viewTop - HeaderHeight - REALIZE_BUFFER;
            var bottom = viewTop - HeaderHeight + height + REALIZE_BUFFER;

            var first = -1;
            var last = -1;

            for (var i = 0; i < rows.Count; i++)
            {
                var slot = rows[i];
                if (slot.Y >= bottom) break;
                if (slot.Y + RowHeightOf(slot) <= top) continue;

                if (first < 0) first = i;
                last = i;
            }

            // keep one row realized so there is always a measured height to estimate from
            return first < 0 ? (0, 0) : (first, last);
        }


        /// <summary>
        /// Adds a row's visuals, keeping <see cref="Panel.Children"/> in row order: that order is
        /// also the logical/tab order, so appending would make Tab follow scroll history.
        /// </summary>
        private void Realize(RowSlot slot, int index)
        {
            if (slot.IsRealized) return;
            slot.IsRealized = true;

            var at = ChildIndexFor(index);
            foreach (var layer in slot.Layers) Children.Insert(at++, layer);
            foreach (var cell in slot.Cells)
            {
                if (cell is not null) Children.Insert(at++, cell);
            }

            // an unrealized row cannot show its hover tint or actions, so re-apply them here
            owner.RefreshRowReveal(slot);
        }


        /// <summary>
        /// Where a row's visuals belong in <see cref="Panel.Children"/>: after the header and after
        /// every already-realized row that sorts before it.
        /// </summary>
        private int ChildIndexFor(int index)
        {
            var at = owner.HeaderVisualCount;

            for (var i = _firstRealized; i < index; i++)
            {
                var slot = owner._rows[i];
                if (!slot.IsRealized) continue;

                at += slot.Layers.Length;
                foreach (var cell in slot.Cells)
                {
                    if (cell is not null) at++;
                }
            }

            return at;
        }


        private void Unrealize(RowSlot slot)
        {
            if (!slot.IsRealized) return;
            slot.IsRealized = false;

            foreach (var layer in slot.Layers) Children.Remove(layer);
            foreach (var cell in slot.Cells)
            {
                if (cell is not null) Children.Remove(cell);
            }
        }

        #endregion // Realization

    }

    #endregion // Virtualizing rows panel

}

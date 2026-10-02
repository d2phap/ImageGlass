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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ImageGlass.Tools;


/// <summary>
/// Draws the screens to scale, with the main window and the projectors on them; a click picks a screen.
/// </summary>
public sealed class ScreenMapControl : PhControl
{
    // the map takes the shape of the desktop within these widths
    private const double MIN_MAP_WIDTH = 120;
    private const double MAX_MAP_WIDTH = 360;
    private const double DEFAULT_MAP_HEIGHT = 96;

    // room between two screens
    private const double SCREEN_GAP = 6;
    private const float SCREEN_CORNER_RADIUS = 5;

    // badges of the projectors on a screen
    private const double BADGE_HEIGHT = 18;
    private const double BADGE_PADDING = 6;
    private const double BADGE_MARGIN = 4;

    // a screen this small has no room for a window glyph, or a title-sized number
    private const double MIN_GLYPH_SCREEN_WIDTH = 36;
    private const double MIN_TITLE_SCREEN_HEIGHT = 40;

    // cells of the tiled projectors on a screen
    private const double TILE_INSET = 3;
    private const double TILE_GAP = 3;
    private const float TILE_CORNER_RADIUS = 3;
    internal const int TILE_FILL_ALPHA = 110;

    private readonly List<ScreenSlot> _slots = [];
    private ProjectorManager? _manager;
    private Screen? _hoveredScreen;
    private Screen? _dropTargetScreen;


    /// <summary>
    /// Gets, sets the screen a dragged projector would be dropped on, highlighted like a hovered one.
    /// </summary>
    public Screen? DropTargetScreen
    {
        get => _dropTargetScreen;
        set
        {
            if (value == _dropTargetScreen) return;

            _dropTargetScreen = value;
            InvalidateVisual();
        }
    }


    /// <summary>
    /// Occurs when a screen with a projector on it is clicked.
    /// </summary>
    public event TEventHandler<ScreenMapControl, Screen>? ScreenClicked;


    /// <summary>
    /// Gets, sets the number of the projector whose screen is highlighted; <c>0</c> for none.
    /// </summary>
    public int SelectedProjectorNumber
    {
        get => GetValue(SelectedProjectorNumberProperty);
        set => SetValue(SelectedProjectorNumberProperty, value);
    }
    public static readonly StyledProperty<int> SelectedProjectorNumberProperty =
        AvaloniaProperty.Register<ScreenMapControl, int>(nameof(SelectedProjectorNumber));


    /// <summary>
    /// Gets, sets the projectors to draw the screens and windows of.
    /// </summary>
    public ProjectorManager? Manager
    {
        get => _manager;
        set
        {
            if (ReferenceEquals(_manager, value)) return;

            DetachManager();
            _manager = value;
            if (IsLoaded) AttachManager();

            RefreshMap();
        }
    }


    static ScreenMapControl()
    {
        AffectsRender<ScreenMapControl>(SelectedProjectorNumberProperty);
    }


    public ScreenMapControl()
    {
        // hit-testable all over, so the gaps between screens do not flicker the hover state
        Background = Brushes.Transparent;
        ClipToBounds = true;
    }



    #region Control Events

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        AttachManager();
        RefreshMap();
    }


    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        DetachManager();
    }


    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();
        UpdateTooltip();
    }


    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var position = e.GetPosition(this);
        var screen = GetScreenAt(position);
        SetHoveredScreen(screen);
    }


    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHoveredScreen(null);
    }


    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left) return;

        // a screen with no projector has no layout to pick
        var position = e.GetPosition(this);
        var slot = GetSlotAt(position);
        if (slot is null) return;
        if (slot.Projectors.Length == 0) return;

        ScreenClicked?.Invoke(this, slot.Screen);
    }


    private void Manager_Changed(object? sender, EventArgs e)
    {
        RefreshMap();
    }


    private void MainWindow_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        RefreshMap();
    }

    #endregion // Control Events



    #region Layout & Render

    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        var height = double.IsFinite(availableSize.Height) ? availableSize.Height : DEFAULT_MAP_HEIGHT;
        var width = GetWidthForHeight(height);

        if (double.IsFinite(availableSize.Width))
        {
            width = Math.Min(width, availableSize.Width);
        }

        return new Size(width, height);
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);
        UpdateSlots();

        foreach (var slot in _slots)
        {
            DrawScreen(c, slot);
        }
    }


    /// <summary>
    /// Draws one screen with its number, the main window glyph and the projectors: their cells if tiled, else badges.
    /// </summary>
    private void DrawScreen(DrawingContext c, ScreenSlot slot)
    {
        var accent = Core.AccentColor;
        var foreground = Resx.GetBrushColor(ResxId.IG_ThemeForegroundBrush, Core.Theme.InvertedBaseColor);
        var hasSelectedProjector = slot.Projectors.Any(p => p.Number == SelectedProjectorNumber);

        // only a screen with a projector opens a layout menu, while a drag can drop on any
        var isClickable = slot.Projectors.Length > 0;
        var isHovered = (slot.Screen == _hoveredScreen && isClickable) || slot.Screen == _dropTargetScreen;


        // 1. the screen, with the cells of its tiled projectors
        Color fill;
        if (isHovered) fill = accent.WithAlpha(70);
        else if (hasSelectedProjector) fill = accent.WithAlpha(40);
        else fill = Resx.GetBrushColor(ResxId.IG_BackgroundNeutralBrush, foreground.WithAlpha(14));

        var isHighlighted = isHovered || hasSelectedProjector;
        var normalBorder = Resx.GetBrushColor(ResxId.IG_BorderControlBrush, foreground.WithAlpha(70));
        var border = isHighlighted ? accent.WithAlpha(230) : normalBorder;
        var borderWidth = hasSelectedProjector ? 2f : 1f;

        c.DrawRectangleEx(slot.Rect, SCREEN_CORNER_RADIUS, border, fill, borderWidth);

        var hasTiles = false;
        foreach (var projector in slot.Projectors)
        {
            if (projector.CellRect is not { } cellRect) continue;

            DrawTile(c, cellRect, projector.Number, foreground);
            hasTiles = true;
        }


        // 2. its number, as the user counts screens: left to right; tiles carry their own labels instead
        if (!hasTiles)
        {
            var number = slot.Number.ToString(CultureInfo.InvariantCulture);
            var numberSize = slot.Rect.Height >= MIN_TITLE_SCREEN_HEIGHT ? Const.FONT_SIZE_TITLE : Const.FONT_SIZE_BODY;
            var numberBounds = c.MeasureTextEx(number, FontFamily, numberSize, true);
            c.DrawTextEx(number, FontFamily, numberSize,
                slot.Rect.Center.X - numberBounds.Width / 2,
                slot.Rect.Center.Y - numberBounds.Height / 2,
                foreground, isBold: true);
        }


        // 3. the main window, as a small window glyph in the bottom left corner
        if (slot.HasMainWindow)
        {
            DrawWindowGlyph(c, slot.Rect, foreground);
        }


        // 4. the projectors not tiled, as badges in the top right corner, or the bottom one when a cell covers the top
        var badgeRight = slot.Rect.Right - BADGE_MARGIN;
        var topCorner = new Point(badgeRight - 1, slot.Rect.Top + BADGE_MARGIN + 1);
        var isTopCornerTiled = slot.Projectors.Any(p => p.CellRect?.Contains(topCorner) == true);
        var badgeTop = isTopCornerTiled
            ? slot.Rect.Bottom - BADGE_MARGIN - BADGE_HEIGHT
            : slot.Rect.Top + BADGE_MARGIN;
        var untiledNumbers = slot.Projectors
            .Where(p => p.CellRect is null)
            .Select(p => p.Number)
            .OrderDescending();
        foreach (var projectorNumber in untiledNumbers)
        {
            badgeRight = DrawProjectorBadge(c, projectorNumber, badgeRight, badgeTop);
        }
    }


    /// <summary>
    /// Draws the cell a tiled projector fills, labelled with the projector where it fits.
    /// </summary>
    private void DrawTile(DrawingContext c, Rect cellRect, int projectorNumber, Color foreground)
    {
        var accent = Core.AccentColor;
        var isSelected = projectorNumber == SelectedProjectorNumber;
        var fill = isSelected ? accent : accent.WithAlpha(TILE_FILL_ALPHA);
        c.DrawRectangleEx(cellRect, TILE_CORNER_RADIUS, null, fill);

        var label = $"P{projectorNumber}";
        var fontSize = Const.FONT_SIZE_BODY;
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize);
        var isNarrowEnough = labelSize.Width + 2 <= cellRect.Width;
        if (!isNarrowEnough) return;

        var isShortEnough = labelSize.Height <= cellRect.Height;
        if (!isShortEnough) return;

        var labelColor = isSelected ? accent.InvertBlackOrWhite() : foreground;
        c.DrawTextEx(label, FontFamily, fontSize,
            cellRect.Center.X - labelSize.Width / 2,
            cellRect.Center.Y - labelSize.Height / 2,
            labelColor);
    }


    /// <summary>
    /// Draws a window glyph: a frame with a title bar.
    /// </summary>
    private static void DrawWindowGlyph(DrawingContext c, Rect screenRect, Color color)
    {
        var hasRoom = screenRect.Width >= MIN_GLYPH_SCREEN_WIDTH;
        if (!hasRoom) return;

        var glyph = new Rect(screenRect.Left + 6, screenRect.Bottom - 6 - 10, 14, 10);

        c.DrawRectangleEx(glyph, 1.5f, color, null, 1.2f);
        c.DrawLineEx(glyph.Left, glyph.Top + 3, glyph.Right, glyph.Top + 3, color, 1.2f);
    }


    /// <summary>
    /// Draws the badge of a projector ending at <paramref name="right"/>; returns where the next badge ends.
    /// </summary>
    private double DrawProjectorBadge(DrawingContext c, int projectorNumber, double right, double top)
    {
        var accent = Core.AccentColor;
        var label = $"P{projectorNumber}";
        var fontSize = Const.FONT_SIZE_SMALL;
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize);

        var badgeWidth = labelSize.Width + BADGE_PADDING * 2;
        var badge = new Rect(right - badgeWidth, top, badgeWidth, BADGE_HEIGHT);
        c.DrawRectangleEx(badge, (float)(BADGE_HEIGHT / 2), null, accent);

        var labelColor = accent.InvertBlackOrWhite();
        c.DrawTextEx(label, FontFamily, fontSize,
            badge.X + BADGE_PADDING,
            badge.Y + (BADGE_HEIGHT - labelSize.Height) / 2,
            labelColor);

        return badge.Left - BADGE_MARGIN;
    }


    /// <summary>
    /// Lays the screens out to scale in the control, and finds what is on each one.
    /// </summary>
    private void UpdateSlots()
    {
        _slots.Clear();
        if (_manager is null) return;

        var screens = _manager.GetOrderedScreens();
        if (screens.Count == 0) return;

        // fit the desktop in the control, centered
        var desktop = GetDesktopBounds();
        var area = new Rect(Bounds.Size).Deflate(SCREEN_GAP / 2);
        if (area.Width <= 0) return;
        if (area.Height <= 0) return;

        var scale = Math.Min(area.Width / desktop.Width, area.Height / desktop.Height);
        var offsetX = area.X + (area.Width - desktop.Width * scale) / 2;
        var offsetY = area.Y + (area.Height - desktop.Height * scale) / 2;

        var mainScreen = _manager.GetMainWindowScreen();
        for (var i = 0; i < screens.Count; i++)
        {
            var screen = screens[i];
            var bounds = screen.Bounds;
            var rect = new Rect(
                offsetX + (bounds.X - desktop.X) * scale,
                offsetY + (bounds.Y - desktop.Y) * scale,
                bounds.Width * scale,
                bounds.Height * scale).Deflate(SCREEN_GAP / 2);

            var projectors = _manager.Windows
                .Where(w => w.GetScreen() == screen)
                .Select(w => new ProjectorMark(w.Number, GetCellRect(w.Tile, rect)))
                .ToArray();

            _slots.Add(new ScreenSlot(screen, i + 1, rect, screen == mainScreen, projectors));
        }
    }


    /// <summary>
    /// Gets where the cell of a tiled projector lies in the slot of its screen; <c>null</c> when it is not tiled.
    /// </summary>
    private static Rect? GetCellRect(ProjectorTile? tile, Rect slotRect)
    {
        if (tile is not { } cellTile) return null;

        // the cells fill the whole slot; the taskbar the real ones leave out would only show as a gap
        var area = slotRect.Deflate(TILE_INSET);
        var cellRect = cellTile.Layout.GetCellBounds(area, cellTile.Cell);

        return cellRect.Deflate(TILE_GAP / 2);
    }


    /// <summary>
    /// Gets the bounds of all screens together, in desktop pixels.
    /// </summary>
    private PixelRect GetDesktopBounds()
    {
        var screens = _manager?.AllScreens;
        if (screens is null || screens.Count == 0) return new PixelRect(0, 0, 16, 9);

        var desktop = screens[0].Bounds;
        foreach (var screen in screens)
        {
            desktop = desktop.Union(screen.Bounds);
        }

        return desktop;
    }

    #endregion // Layout & Render



    #region Public Methods

    /// <summary>
    /// Gets how wide the map is at <paramref name="height"/>: the shape of the whole desktop, within limits that grow with the height.
    /// </summary>
    public double GetWidthForHeight(double height)
    {
        var desktop = GetDesktopBounds();
        var aspectRatio = desktop.Height > 0 ? (double)desktop.Width / desktop.Height : 16d / 9;
        var maxWidth = MAX_MAP_WIDTH * Math.Max(1, height / DEFAULT_MAP_HEIGHT);

        return Math.Clamp(height * aspectRatio, MIN_MAP_WIDTH, maxWidth);
    }


    /// <summary>
    /// Gets the screen drawn at <paramref name="position"/>, in the coordinates of this control.
    /// </summary>
    public Screen? GetScreenAt(Point position) => GetSlotAt(position)?.Screen;


    /// <summary>
    /// Gets where <paramref name="screen"/> is drawn, in the coordinates of this control.
    /// </summary>
    public Rect GetScreenBounds(Screen screen)
    {
        var slot = _slots.FirstOrDefault(s => s.Screen == screen);

        return slot?.Rect ?? new Rect(Bounds.Size);
    }

    #endregion // Public Methods



    #region Private Methods

    private void AttachManager()
    {
        _manager?.Changed -= Manager_Changed;
        _manager?.Changed += Manager_Changed;

        App.MainWindow.PositionChanged -= MainWindow_PositionChanged;
        App.MainWindow.PositionChanged += MainWindow_PositionChanged;
    }


    private void DetachManager()
    {
        _manager?.Changed -= Manager_Changed;
        App.MainWindow.PositionChanged -= MainWindow_PositionChanged;
    }


    /// <summary>
    /// Redraws the map, which can take a new shape when the screens change.
    /// </summary>
    private void RefreshMap()
    {
        InvalidateMeasure();
        InvalidateVisual();
        UpdateTooltip();
    }


    /// <summary>
    /// Gets the slot of the screen at <paramref name="position"/>.
    /// </summary>
    private ScreenSlot? GetSlotAt(Point position)
    {
        foreach (var slot in _slots)
        {
            if (slot.Rect.Contains(position)) return slot;
        }

        return null;
    }


    /// <summary>
    /// Highlights the screen under the pointer.
    /// </summary>
    private void SetHoveredScreen(Screen? screen)
    {
        var isSameScreen = screen == _hoveredScreen;
        if (isSameScreen) return;

        _hoveredScreen = screen;

        InvalidateVisual();
        UpdateTooltip();
    }


    /// <summary>
    /// Describes the screen under the pointer: its size, scale, and what is on it.
    /// </summary>
    private void UpdateTooltip()
    {
        var slot = _slots.FirstOrDefault(s => s.Screen == _hoveredScreen);
        if (slot is null)
        {
            ToolTip.SetTip(this, null);
            return;
        }

        var bounds = slot.Screen.Bounds;
        var scalePercent = (int)Math.Round(slot.Screen.Scaling * 100);
        var lines = new List<string>
        {
            Core.Lang[LangId.Tool_Projector_ScreenInfo, slot.Number, bounds.Width, bounds.Height, scalePercent],
        };

        if (!string.IsNullOrWhiteSpace(slot.Screen.DisplayName)) lines.Add(slot.Screen.DisplayName);
        if (slot.HasMainWindow) lines.Add(Core.Lang[LangId.Tool_Projector_MainWindow]);

        foreach (var projectorNumber in slot.Projectors.Select(p => p.Number).Order())
        {
            lines.Add(Core.Lang[LangId.Tool_Projector_WindowTitle, projectorNumber]);
        }

        ToolTip.SetTip(this, string.Join(Environment.NewLine, lines));
    }

    #endregion // Private Methods



    /// <summary>
    /// A screen as laid out in the map, with what is on it.
    /// </summary>
    private sealed record ScreenSlot(Screen Screen, int Number, Rect Rect, bool HasMainWindow, ProjectorMark[] Projectors);


    /// <summary>
    /// A projector on a screen of the map, with the cell it fills there if tiled.
    /// </summary>
    private sealed record ProjectorMark(int Number, Rect? CellRect);

}

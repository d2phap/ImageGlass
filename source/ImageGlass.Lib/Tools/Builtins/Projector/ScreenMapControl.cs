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
    private const double BADGE_HEIGHT = 16;
    private const double BADGE_PADDING = 5;
    private const double BADGE_MARGIN = 4;

    // a screen this small has no room for a window glyph
    private const double MIN_GLYPH_SCREEN_WIDTH = 36;

    private readonly List<ScreenSlot> _slots = [];
    private ProjectorManager? _manager;
    private Screen? _hoveredScreen;


    /// <summary>
    /// Occurs when a screen is clicked.
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
        var screen = HitTestScreen(position);
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

        var position = e.GetPosition(this);
        var screen = HitTestScreen(position);
        if (screen is null) return;

        ScreenClicked?.Invoke(this, screen);
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

        // take the aspect ratio of the whole desktop
        var desktop = GetDesktopBounds();
        var height = double.IsFinite(availableSize.Height) ? availableSize.Height : DEFAULT_MAP_HEIGHT;
        var aspectRatio = desktop.Height > 0 ? (double)desktop.Width / desktop.Height : 16d / 9;
        var width = Math.Clamp(height * aspectRatio, MIN_MAP_WIDTH, MAX_MAP_WIDTH);

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
    /// Draws one screen with its number, the main window glyph and the projector badges.
    /// </summary>
    private void DrawScreen(DrawingContext c, ScreenSlot slot)
    {
        var accent = Core.AccentColor;
        var foreground = Core.Theme.InvertedBaseColor;
        var isHovered = slot.Screen == _hoveredScreen;
        var hasSelectedProjector = slot.ProjectorNumbers.Contains(SelectedProjectorNumber);


        // 1. the screen
        Color fill;
        if (isHovered) fill = accent.WithAlpha(70);
        else if (hasSelectedProjector) fill = accent.WithAlpha(40);
        else fill = foreground.WithAlpha(14);

        var isHighlighted = isHovered || hasSelectedProjector;
        var border = isHighlighted ? accent.WithAlpha(230) : foreground.WithAlpha(70);
        var borderWidth = hasSelectedProjector ? 2f : 1f;

        c.DrawRectangleEx(slot.Rect, SCREEN_CORNER_RADIUS, border, fill, borderWidth);


        // 2. its number, as the user counts screens: left to right
        var number = slot.Number.ToString(CultureInfo.InvariantCulture);
        var numberSize = Math.Clamp(slot.Rect.Height * 0.32, Const.FONT_SIZE_SMALL, Const.FONT_SIZE_TITLE);
        var numberBounds = c.MeasureTextEx(number, FontFamily, numberSize, true);
        c.DrawTextEx(number, FontFamily, numberSize,
            slot.Rect.Center.X - numberBounds.Width / 2,
            slot.Rect.Center.Y - numberBounds.Height / 2,
            foreground.WithAlpha(150), isBold: true);


        // 3. the main window, as a small window glyph in the bottom left corner
        if (slot.HasMainWindow)
        {
            DrawWindowGlyph(c, slot.Rect, foreground.WithAlpha(190));
        }


        // 4. the projectors, as badges along the top right corner
        var badgeRight = slot.Rect.Right - BADGE_MARGIN;
        foreach (var projectorNumber in slot.ProjectorNumbers.OrderDescending())
        {
            badgeRight = DrawProjectorBadge(c, projectorNumber, badgeRight, slot.Rect.Top + BADGE_MARGIN);
        }
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
        var fontSize = Const.FONT_SIZE_SMALL - 2;
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize, true);

        var badgeWidth = labelSize.Width + BADGE_PADDING * 2;
        var badge = new Rect(right - badgeWidth, top, badgeWidth, BADGE_HEIGHT);
        c.DrawRectangleEx(badge, (float)(BADGE_HEIGHT / 2), null, accent);

        var labelColor = accent.InvertBlackOrWhite();
        c.DrawTextEx(label, FontFamily, fontSize,
            badge.X + BADGE_PADDING,
            badge.Y + (BADGE_HEIGHT - labelSize.Height) / 2,
            labelColor, isBold: true);

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

            var projectorNumbers = _manager.Windows
                .Where(w => w.GetScreen() == screen)
                .Select(w => w.Number)
                .ToArray();

            _slots.Add(new ScreenSlot(screen, i + 1, rect, screen == mainScreen, projectorNumbers));
        }
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
    /// Gets the screen at <paramref name="position"/>.
    /// </summary>
    private Screen? HitTestScreen(Point position)
    {
        foreach (var slot in _slots)
        {
            if (slot.Rect.Contains(position)) return slot.Screen;
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
        Cursor = screen is null ? Cursor.Default : new Cursor(StandardCursorType.Hand);

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

        foreach (var projectorNumber in slot.ProjectorNumbers.Order())
        {
            lines.Add(Core.Lang[LangId.Tool_Projector_WindowTitle, projectorNumber]);
        }

        ToolTip.SetTip(this, string.Join(Environment.NewLine, lines));
    }

    #endregion // Private Methods



    /// <summary>
    /// A screen as laid out in the map, with what is on it.
    /// </summary>
    private sealed record ScreenSlot(Screen Screen, int Number, Rect Rect, bool HasMainWindow, int[] ProjectorNumbers);

}

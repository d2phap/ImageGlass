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
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Localization;
using ImageGlass.Common.Types;
using ImageGlass.UI;
using System;
using System.Globalization;

namespace ImageGlass.Tools;


/// <summary>
/// Draws a screen split into the cells of a <see cref="ProjectorLayout"/>, with the projectors that would fill them.
/// </summary>
public sealed class LayoutThumbnailControl : PhControl
{
    // the thumbnail takes the shape of its screen within this box, large enough to read the layout
    private const double BOX_WIDTH = 120;
    private const double BOX_HEIGHT = 80;

    private const float FRAME_RADIUS = 6;
    private const float CELL_RADIUS = 3;
    private const double FRAME_PADDING = 5;
    private const double CELL_GAP = 4;

    // a cell this small shows its fill only
    private const double MIN_LABEL_CELL_SIZE = 14;

    // room around the text of the default option
    private const double LABEL_PADDING_X = 10;
    private const double LABEL_PADDING_Y = 4;


    #region Public Properties

    /// <summary>
    /// Gets the layout to draw; <c>null</c> for the default, where each projector covers its screen as it prefers.
    /// </summary>
    public ProjectorLayout? Layout { get; }


    /// <summary>
    /// Gets the numbers of the projectors to tile, in the order of the used cells of the layout.
    /// </summary>
    public int[] ProjectorNumbers { get; }


    /// <summary>
    /// Gets the width to height ratio of the screen.
    /// </summary>
    public double AspectRatio { get; }


    /// <summary>
    /// Gets, sets whether the projectors are laid out like this now.
    /// </summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<LayoutThumbnailControl, bool>(nameof(IsActive));

    #endregion // Public Properties



    static LayoutThumbnailControl()
    {
        AffectsRender<LayoutThumbnailControl>(IsActiveProperty);
    }


    public LayoutThumbnailControl(ProjectorLayout? layout, int[] projectorNumbers, double aspectRatio)
    {
        Layout = layout;
        ProjectorNumbers = projectorNumbers;
        AspectRatio = double.IsFinite(aspectRatio) && aspectRatio > 0 ? aspectRatio : 16d / 9;
        IsHitTestVisible = false;
    }



    #region Layout & Render

    protected override void OnIgThemeChanged(ThemePackChangedEventArgs e)
    {
        base.OnIgThemeChanged(e);
        InvalidateVisual();
    }


    protected override void OnIgLanguageChanged()
    {
        base.OnIgLanguageChanged();

        // the default option draws its text
        InvalidateVisual();
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        // the shape of the screen, as large as the box allows
        var width = BOX_WIDTH;
        var height = width / AspectRatio;
        if (height > BOX_HEIGHT)
        {
            height = BOX_HEIGHT;
            width = height * AspectRatio;
        }

        return new Size(width, height);
    }


    public override void Render(DrawingContext c)
    {
        base.Render(c);

        var accent = Core.AccentColor;
        var foreground = Core.Theme.InvertedBaseColor;


        // 1. the screen, bordered like the screens on the map
        var frame = new Rect(Bounds.Size).Deflate(0.5);
        var normalBorder = Resx.Get<IBrush?>(ResxId.IG_BorderControlBrush) is ISolidColorBrush controlBorder
            ? controlBorder.Color
            : foreground.WithAlpha(90);
        var frameBorder = IsActive ? accent.WithAlpha(230) : normalBorder;
        c.DrawRectangleEx(frame, FRAME_RADIUS, frameBorder, foreground.WithAlpha(14), IsActive ? 2f : 1f);


        // 2. the default: a label in the middle
        if (Layout is null)
        {
            DrawDefaultLabel(c, frame, accent, foreground);
            return;
        }


        // 3. the cells, with the projector each one gets; the gaps between them stay even
        var area = frame.Deflate(FRAME_PADDING - CELL_GAP / 2);
        for (var cell = 0; cell < Layout.CellCount; cell++)
        {
            var cellRect = Layout.GetCellBounds(area, cell).Deflate(CELL_GAP / 2);
            if (cellRect.Width <= 0 || cellRect.Height <= 0) continue;

            var projectorIndex = Layout.GetProjectorIndex(cell);
            var hasProjector = projectorIndex >= 0 && projectorIndex < ProjectorNumbers.Length;
            if (hasProjector)
            {
                DrawFilledCell(c, cellRect, ProjectorNumbers[projectorIndex], accent, foreground);
            }
            else
            {
                DrawEmptyCell(c, cellRect, foreground);
            }
        }
    }


    /// <summary>
    /// Draws the name of the default option in the middle of the screen.
    /// </summary>
    private void DrawDefaultLabel(DrawingContext c, Rect frame, Color accent, Color foreground)
    {
        var label = Core.Lang[LangId._Default];
        var fontSize = Const.FONT_SIZE_BODY;
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize);

        var box = new Rect(
            frame.Center.X - labelSize.Width / 2 - LABEL_PADDING_X,
            frame.Center.Y - labelSize.Height / 2 - LABEL_PADDING_Y,
            labelSize.Width + LABEL_PADDING_X * 2,
            labelSize.Height + LABEL_PADDING_Y * 2);
        var fill = IsActive ? accent : foreground.WithAlpha(70);
        c.DrawRectangleEx(box, CELL_RADIUS, null, fill);

        var labelColor = IsActive ? accent.InvertBlackOrWhite() : foreground.WithAlpha(230);
        c.DrawTextEx(label, FontFamily, fontSize,
            box.X + LABEL_PADDING_X,
            box.Y + LABEL_PADDING_Y,
            labelColor);
    }


    /// <summary>
    /// Draws a cell a projector would fill, with its number where it fits.
    /// </summary>
    private void DrawFilledCell(DrawingContext c, Rect cellRect, int projectorNumber, Color accent, Color foreground)
    {
        var fill = IsActive ? accent : foreground.WithAlpha(70);
        c.DrawRectangleEx(cellRect, CELL_RADIUS, null, fill);

        var hasRoom = cellRect.Width >= MIN_LABEL_CELL_SIZE && cellRect.Height >= MIN_LABEL_CELL_SIZE;
        if (!hasRoom) return;

        var label = projectorNumber.ToString(CultureInfo.InvariantCulture);
        var fontSize = Math.Min(Const.FONT_SIZE_BODY, cellRect.Height - 2);
        var labelSize = c.MeasureTextEx(label, FontFamily, fontSize, true);
        var labelColor = IsActive ? accent.InvertBlackOrWhite() : foreground.WithAlpha(230);

        c.DrawTextEx(label, FontFamily, fontSize,
            cellRect.Center.X - labelSize.Width / 2,
            cellRect.Center.Y - labelSize.Height / 2,
            labelColor, isBold: true);
    }


    /// <summary>
    /// Draws a cell no projector would fill, as a dashed outline.
    /// </summary>
    private static void DrawEmptyCell(DrawingContext c, Rect cellRect, Color foreground)
    {
        var pen = new Pen(foreground.WithAlpha(80).ToBrush(), 1, DashStyle.Dash);
        var outline = new RoundedRect(cellRect.Deflate(0.5), CELL_RADIUS);

        c.DrawRectangle(null, pen, outline);
    }

    #endregion // Layout & Render

}

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
using Avalonia.Input;
using Avalonia.Media;
using ImageGlass.Common;
using ImageGlass.Common.Extensions;
using ImageGlass.Common.Types;
using System;
using System.Collections.Generic;

namespace ImageGlass.UI.Viewer;


public partial class ViewerControl
{
    // selection
    private SelectionInfo _selection = new();


    /// <summary>
    /// Occurs when the <see cref="ClientSelection"/> is changed.
    /// </summary>
    public event TEventHandler<ViewerControl, ViewerSelectionChangedEventArgs>? SelectionChanged;



    #region Public Properties

    /// <summary>
    /// Gets the client selection area.
    /// </summary>
    public Rect ClientSelection => RectSourceToClient(_selection.SourceRect);


    /// <summary>
    /// Gets, sets the image source selection. This will emit the event <see cref="SelectionChanged"/>.
    /// Use <see cref="SetSourceSelection"/> to control the selection event.
    /// </summary>
    public Rect SourceSelection
    {
        get => _selection.SourceRect;
        set => SetSourceSelection(value, true);
    }


    /// <summary>
    /// Gets, sets selection aspect ratio.
    /// If Width or Height is <c>less than or equals 0</c>, we will use free aspect ratio.
    /// </summary>
    public Size SelectionAspectRatio { get; set; } = new();


    /// <summary>
    /// Enables or disables the selection.
    /// </summary>
    public bool EnableSelection
    {
        get => _selection.Enabled;
        set
        {
            _selection.Enabled = value;
            if (!_selection.Enabled && Parent != null)
            {
                Cursor = Avalonia.Input.Cursor.Default;
            }
        }
    }


    /// <summary>
    /// Gets the current action of selection.
    /// </summary>
    public SelectionAction CurrentSelectionAction { get; private set; } = SelectionAction.None;


    /// <summary>
    /// Gets 8 resizers of the selection rectangle
    /// </summary>
    public List<SelectionResizer> SelectionResizers
    {
        get
        {
            if (SourceSelection.IsEmpty) return [];

            var resizerSize = DpiScale(7f);
            var resizerMargin = DpiScale(1f);
            var hitSize = DpiScale(resizerSize * 1.3f);


            // top left
            var topLeft = new Rect(
                ClientSelection.X + resizerMargin,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topLeftHit = new Rect(
                ClientSelection.X - hitSize / 2 + resizerSize / 2,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                hitSize, hitSize);


            // top right
            var topRight = new Rect(
                ClientSelection.Right - resizerSize - resizerMargin,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topRightHit = new Rect(
                ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                hitSize, hitSize);


            // bottom left
            var bottomLeft = new Rect(
                ClientSelection.X + resizerMargin,
                ClientSelection.Bottom - resizerSize - resizerMargin,
                resizerSize, resizerSize);
            var bottomLeftHit = new Rect(
                ClientSelection.X - hitSize / 2 + resizerSize / 2,
                ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                hitSize, hitSize);


            // bottom right
            var bottomRight = new Rect(
                ClientSelection.Right - resizerSize - resizerMargin,
                ClientSelection.Bottom - resizerSize - resizerMargin,
                resizerSize, resizerSize);
            var bottomRightHit = new Rect(
                ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                hitSize, hitSize);


            // top
            var top = new Rect(
                ClientSelection.X + ClientSelection.Width / 2 - resizerSize / 2,
                ClientSelection.Y + resizerMargin,
                resizerSize, resizerSize);
            var topHit = new Rect(
                topLeftHit.Right,
                ClientSelection.Y - hitSize / 2 + resizerSize / 2,
                Math.Abs(topRightHit.X - topLeftHit.Right), hitSize);


            // right
            var right = new Rect(
                    ClientSelection.Right - resizerSize - resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var rightHit = new Rect(
                    ClientSelection.Right - hitSize / 2 - resizerSize / 2,
                    topRightHit.Bottom,
                    hitSize, Math.Abs(bottomRightHit.Y - topRightHit.Bottom));


            // bottom
            var bottom = new Rect(
                    ClientSelection.X + ClientSelection.Width / 2 - resizerSize / 2,
                    ClientSelection.Bottom - resizerSize - resizerMargin,
                    resizerSize, resizerSize);
            var bottomHit = new Rect(
                    bottomLeftHit.Right,
                    ClientSelection.Bottom - hitSize / 2 - resizerSize / 2,
                    Math.Abs(bottomRightHit.X - bottomLeftHit.Right), hitSize);


            // left
            var left = new Rect(
                    ClientSelection.X + resizerMargin,
                    ClientSelection.Y + ClientSelection.Height / 2 - resizerSize / 2,
                    resizerSize, resizerSize);
            var leftHit = new Rect(
                    ClientSelection.X - hitSize / 2 + resizerSize / 2,
                    topLeftHit.Bottom,
                    hitSize, Math.Abs(bottomLeftHit.Y - topLeftHit.Bottom));


            // 8 resizers
            return [
                // bottom-right is in higher layer
                new(SelectionResizerType.BottomRight, bottomRight, bottomRightHit),
                new(SelectionResizerType.TopLeft, topLeft, topLeftHit),
                new(SelectionResizerType.BottomLeft, bottomLeft, bottomLeftHit),
                new(SelectionResizerType.TopRight, topRight, topRightHit),

                new(SelectionResizerType.Right, right, rightHit),
                new(SelectionResizerType.Bottom, bottom, bottomHit),
                new(SelectionResizerType.Left, left, leftHit),
                new(SelectionResizerType.Top, top, topHit),
            ];
        }
    }


    #endregion // Public Properties



    #region Public Functions

    /// <summary>
    /// Sets selection area on the source image coordinates.
    /// </summary>
    public void SetSourceSelection(Rect srcRect, bool triggerEvent = true)
    {
        // use integer numbers for pixel rounding
        var w = (int)BitmapSize.Width;
        var h = (int)BitmapSize.Height;
        _selection.SourceRect = srcRect.GetIntersection(new Rect(0, 0, w, h));

        if (triggerEvent)
        {
            SelectionChanged?.Invoke(this, new ViewerSelectionChangedEventArgs(ClientSelection, SourceSelection));
        }
    }


    #region Coordinate converters

    /// <summary>
    /// Computes the location of the client point into image source coords.
    /// </summary>
    public Point PointClientToSource(Point clientPoint)
    {
        var dpiFactor = _zooming.Factor / Dpi;
        var x = (clientPoint.X - DestRect.X) / dpiFactor + SrcRect.X;
        var y = (clientPoint.Y - DestRect.Y) / dpiFactor + SrcRect.Y;

        return new Point(x, y);
    }


    /// <summary>
    /// Computes and scale the rectangle of the client to image source coords
    /// </summary>
    public Rect RectClientToSource(Rect rect)
    {
        var safeRect = rect.Normalize();
        var p1 = PointClientToSource(new Point(safeRect.X, safeRect.Y));
        var p2 = PointClientToSource(new Point(safeRect.Right, safeRect.Bottom));


        // get the min int value
        var floorP1X = Math.Floor(Math.Round(p1.X, 1));
        var floorP1Y = Math.Floor(Math.Round(p1.Y, 1));

        if (floorP1X < 0) floorP1X = 0;
        if (floorP1Y < 0) floorP1Y = 0;
        if (floorP1X > BitmapSize.Width) floorP1X = BitmapSize.Width;
        if (floorP1Y > BitmapSize.Height) floorP1Y = BitmapSize.Height;

        if (p1 == p2) return new Rect(floorP1X, floorP1Y, 0, 0);


        // get the max int value
        var ceilP2X = Math.Ceiling(Math.Round(p2.X, 1));
        var ceilP2Y = Math.Ceiling(Math.Round(p2.Y, 1));
        if (ceilP2X < 0) ceilP2X = 0;
        if (ceilP2Y < 0) ceilP2Y = 0;
        if (ceilP2X > BitmapSize.Width) ceilP2X = BitmapSize.Width;
        if (ceilP2Y > BitmapSize.Height) ceilP2Y = BitmapSize.Height;


        var width = Math.Max(0, ceilP2X - floorP1X);
        var height = Math.Max(0, ceilP2Y - floorP1Y);

        // the selection area is where the p1 and p2 intersected.
        return new Rect(floorP1X, floorP1Y, width, height);
    }


    /// <summary>
    /// Computes the location of the image source point into client coords.
    /// </summary>
    public Point PointSourceToClient(Point srcPoint)
    {
        var dpiFactor = _zooming.Factor / Dpi;
        var x = (srcPoint.X - SrcRect.X) * dpiFactor + DestRect.X;
        var y = (srcPoint.Y - SrcRect.Y) * dpiFactor + DestRect.Y;

        return new Point(x, y);
    }


    /// <summary>
    /// Computes and scale the rectangle of the image source to client coords
    /// </summary>
    public Rect RectSourceToClient(Rect rect)
    {
        var safeRect = rect.Normalize();
        var dpiFactor = _zooming.Factor / Dpi;

        var loc = PointSourceToClient(new Point(safeRect.X, safeRect.Y));
        var size = new Size(safeRect.Width * dpiFactor, safeRect.Height * dpiFactor);

        return new Rect(loc, size);
    }

    #endregion // Coordinate converters


    #endregion // Public Functions





    #region Private Functions

    /// <summary>
    /// Initializes the selection logics when <see cref="OnPointerPressed"/> event occurs.
    /// </summary>
    private bool OnSelectionBegin(PointerPoint p)
    {
        if (_imgSource?.Image?.IsDisposed() ?? true) return false;

        _selection.PointerDownPoint = p.Position;
        _selection.IsLeftButtonPressed = p.Properties.IsLeftButtonPressed;


        // check if we can start the selection
        var canSelect = EnableSelection && p.Properties.IsLeftButtonPressed;
        var isSelectionHovered = ClientSelection.Contains(p.Position);
        var requestRerender = canSelect && !SourceSelection.IsEmpty;


        // get the selected resizer
        _selection.SelectedResizer = SelectionResizers.Find(i => i.HitRegion.Contains(p.Position));
        CurrentSelectionAction = SelectionAction.None;


        if (canSelect)
        {
            _selection.SourceRectBeforeMoved = new Rect(_selection.SourceRect.Position, _selection.SourceRect.Size);

            // resize selection
            if (canSelect && _selection.SelectedResizer != null)
            {
                CurrentSelectionAction = SelectionAction.Resizing;
            }
            // move selection
            else if (isSelectionHovered)
            {
                CurrentSelectionAction = SelectionAction.Moving;
            }
            // draw selection
            else if (canSelect)
            {
                CurrentSelectionAction = SelectionAction.Drawing;
            }
        }


        return requestRerender;
    }


    /// <summary>
    /// Updates selection logics when <see cref="OnPointerMoved"/> event occurs.
    /// </summary>
    private bool OnSelectionUpdating(Point position)
    {
        var requestRerender = false;
        var canSelect = EnableSelection && _selection.IsLeftButtonPressed;
        _selection.PointerMovePoint = position;


        if (_selection.IsLeftButtonPressed)
        {
            // resize the selection
            if (CurrentSelectionAction == SelectionAction.Resizing && _selection.SelectedResizer != null)
            {
                ResizeSelection(position, _selection.SelectedResizer.Type);
                requestRerender = true;
            }
            // move selection
            else if (CurrentSelectionAction == SelectionAction.Moving)
            {
                MoveSelection(position);
                requestRerender = true;
            }
            // draw new selection
            else if (CurrentSelectionAction == SelectionAction.Drawing)
            {
                CreateNewSelection();
                requestRerender = true;
            }
        }


        // change cursor
        if (EnableSelection)
        {
            // set resizer cursor
            var hoveredResizer = SelectionResizers.Find(i => i.HitRegion.Contains(position));

            if (hoveredResizer != null)
            {
                Cursor = hoveredResizer.GetCursor();
            }
            else if (ClientSelection.Contains(position))
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
            }
            else
            {
                Cursor = new Cursor(StandardCursorType.Cross);
            }


            // redraw the canvas
            var isSelectionHovered = ClientSelection.Contains(position);
            requestRerender = requestRerender
                || _selection.IsHovered != isSelectionHovered
                || _selection.HoveredResizer != hoveredResizer;


            _selection.IsHovered = isSelectionHovered;
            _selection.HoveredResizer = hoveredResizer;
        }
        // restore default cursor
        else
        {
            Cursor = Avalonia.Input.Cursor.Default;
        }


        return requestRerender;
    }


    /// <summary>
    /// Updates selection when one of these events occurs:
    /// <list type="bullet">
    ///   <item><see cref="OnPointerReleased"/></item>
    ///   <item><see cref="OnPointerCaptureLost"/></item>
    ///   <item><see cref="OnPointerExited"/></item>
    /// </list>
    /// </summary>
    private bool OnSelectionEnd(bool isPointerExited)
    {
        var requestRerender = false;

        // if the pointer is exited
        if (isPointerExited)
        {
            _selection.IsHovered = false;
            _selection.PointerMovePoint = null;

            return requestRerender;
        }


        // if the pointer is released or canceled
        var canSelect = EnableSelection && _selection.IsLeftButtonPressed;

        _selection.PointerDownPoint = null;
        _selection.IsLeftButtonPressed = false;
        _selection.SelectedResizer = null;


        if (canSelect)
        {
            SelectionChanged?.Invoke(this, new ViewerSelectionChangedEventArgs(ClientSelection, SourceSelection));
            requestRerender = true;
        }

        return requestRerender;
    }


    /// <summary>
    /// Draws the selection visuals on a canvas, including selection borders, grids, and resizers
    /// when <see cref="Render"/> occurs.
    /// </summary>
    private void OnDrawSelection(DrawingContext g)
    {
        if (!EnableSelection || SourceSelection.IsEmpty) return;


        // 1. draw the clip selection region
        var selectionGeo = g.GetCombinedRectGeometryEx(ClientSelection, DestRect, 0, 0, GeometryCombineMode.Xor);
        g.DrawGeometryEx(selectionGeo, Colors.Transparent,
            Colors.Black.WithAlpha(_selection.IsLeftButtonPressed ? 100 : 180));


        // 2. draw selection grid, resizers
        if (_selection.IsLeftButtonPressed || _selection.IsHovered || _selection.HoveredResizer != null)
        {
            var width3 = ClientSelection.Width / 3;
            var height3 = ClientSelection.Height / 3;


            // draw grid, ignore alpha value
            for (int i = 1; i < 3; i++)
            {
                g.DrawLineEx(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, Colors.Black.WithAlpha(200),
                    0.4f);
                g.DrawLineEx(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, Colors.White.WithAlpha(200),
                    0.4f);
                g.DrawLineEx(
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y,
                    ClientSelection.X + (i * width3),
                    ClientSelection.Y + ClientSelection.Height, Core.AccentColor.WithAlpha(200),
                    0.4f);


                g.DrawLineEx(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), Colors.Black.WithAlpha(200),
                    0.4f);
                g.DrawLineEx(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), Colors.White.WithAlpha(200),
                    0.4f);
                g.DrawLineEx(
                    ClientSelection.X,
                    ClientSelection.Y + (i * height3),
                    ClientSelection.X + ClientSelection.Width,
                    ClientSelection.Y + (i * height3), Core.AccentColor.WithAlpha(200),
                    0.4f);
            }


            // draw selection size
            var text = $"{_selection.SourceRect.Width} x {_selection.SourceRect.Height}";
            var textSize = g.MeasureTextEx(text, FontFamily, FontSize * 0.95);
            var textPadding = new Thickness(5, 2.5);
            var textX = ClientSelection.X + (ClientSelection.Width / 2 - textSize.Width / 2);
            var textY = ClientSelection.Y + (ClientSelection.Height / 2 - textSize.Height / 2);
            var textBgRect = new Rect(
                textX - textPadding.Left,
                textY - textPadding.Top,
                textSize.Width + textPadding.Left + textPadding.Right,
                textSize.Height + textPadding.Top + textPadding.Bottom);

            if (textBgRect.Width + 10 < ClientSelection.Width
                && textBgRect.Height + 10 < ClientSelection.Height)
            {
                g.DrawRectangleEx(textBgRect, (float)textSize.Height / 5, Colors.White.WithAlpha(100), Colors.Black.WithAlpha(100));
                g.DrawRectangleEx(textBgRect, (float)textSize.Height / 5, Core.AccentColor.WithAlpha(100), Core.AccentColor.WithAlpha(150));
                g.DrawTextEx(text, FontFamily, FontSize * 0.95, textX, textY, Colors.White);
            }


            // draw resizers with layer order
            var resizers = SelectionResizers;
            resizers.Reverse();

            foreach (var rItem in resizers)
            {
                var hideTopBottomResizers = ClientSelection.Width < rItem.IndicatorRegion.Width * 5;
                if (hideTopBottomResizers
                    && (rItem.Type == SelectionResizerType.Top
                    || rItem.Type == SelectionResizerType.Bottom)) continue;

                var hideLeftRightResizers = ClientSelection.Height < rItem.IndicatorRegion.Height * 5;
                if (hideLeftRightResizers
                    && (rItem.Type == SelectionResizerType.Left
                    || rItem.Type == SelectionResizerType.Right)) continue;

                // hover style
                var resizerRect = rItem.IndicatorRegion;
                var fillColor = Colors.White.WithAlpha(200);
                if (rItem.Type == _selection.HoveredResizer?.Type)
                {
                    resizerRect.Inflate(DpiScale(1.45f));
                    fillColor = Core.AccentColor.WithAlpha(200);
                }

                g.DrawEllipseEx(resizerRect, Colors.White.WithAlpha(50), Colors.Black.WithAlpha(200), 4f);
                g.DrawEllipseEx(resizerRect, Core.AccentColor.WithAlpha(255), fillColor, 1f);


                // draw debug Hit region
                if (EnableDebug)
                {
                    g.DrawRectangleEx(rItem.HitRegion, 0, Colors.Red);
                }
            }
        }


        // 3. draw the selection border
        var borderWidth = (_selection.IsHovered && _selection.IsLeftButtonPressed) ? 0.6f : 0.3f;
        g.DrawRectangleEx(ClientSelection, 0, Colors.White, null, borderWidth);
        g.DrawRectangleEx(ClientSelection, 0, Core.AccentColor, null, borderWidth);

    }



    /// <summary>
    /// Creates a new selection based on mouse pointer positions.
    /// </summary>
    private void CreateNewSelection()
    {
        if (_selection.PointerDownPoint == null || _selection.PointerMovePoint == null) return;

        var cliRect = GetRectBetween2Points(
            _selection.PointerDownPoint.Value,
            _selection.PointerMovePoint.Value,
            SelectionAspectRatio,
            BitmapSize.Width, BitmapSize.Height, DestRect);

        // limit the selected area to the image
        cliRect = cliRect.GetIntersection(DestRect);

        var srcRect = RectClientToSource(cliRect);
        SetSourceSelection(srcRect, true);
    }


    /// <summary>
    /// Gets the rectangle between 2 points with aspect ratio.
    /// </summary>
    /// <param name="point1">The first point</param>
    /// <param name="point2">The second point</param>
    /// <param name="aspectRatio">Aspect ratio</param>
    /// <param name="limitRect">The rectangle to limit the selection</param>
    private static Rect GetRectBetween2Points(Point? point1, Point? point2,
        Size aspectRatio, double srcWidth, double srcHeight, Rect limitRect)
    {
        var selectedArea = new Rect();
        var fromPoint = point1 ?? new Point();
        var toPoint = point2 ?? new Point();

        // swap fromPoint and toPoint value if toPoint is less than fromPoint
        if (toPoint.X < fromPoint.X)
        {
            var tempX = fromPoint.X;
            fromPoint = fromPoint.WithX(toPoint.X);
            toPoint = toPoint.WithX(tempX);
        }
        if (toPoint.Y < fromPoint.Y)
        {
            var tempY = fromPoint.Y;
            fromPoint = fromPoint.WithY(toPoint.Y);
            toPoint = toPoint.WithY(tempY);
        }

        var width = Math.Abs(fromPoint.X - toPoint.X);
        var height = Math.Abs(fromPoint.Y - toPoint.Y);

        selectedArea = selectedArea.WithX(fromPoint.X);
        selectedArea = selectedArea.WithY(fromPoint.Y);
        selectedArea = selectedArea.WithWidth(width);
        selectedArea = selectedArea.WithHeight(height);

        // limit the selected area to the limitRect
        selectedArea = selectedArea.GetIntersection(limitRect);


        // free aspect ratio
        if (aspectRatio.Width <= 0 || aspectRatio.Height <= 0)
            return selectedArea;


        var wRatio = aspectRatio.Width / aspectRatio.Height;
        var hRatio = aspectRatio.Height / aspectRatio.Width;

        // update selection size according to the ratio
        if (wRatio > hRatio)
        {
            selectedArea = selectedArea.WithHeight(selectedArea.Width / wRatio);

            if (selectedArea.Bottom >= limitRect.Bottom)
            {
                var maxHeight = limitRect.Bottom - selectedArea.Y;
                selectedArea = selectedArea.WithWidth(maxHeight * wRatio);
                selectedArea = selectedArea.WithHeight(maxHeight);
            }
        }
        else
        {
            selectedArea = selectedArea.WithWidth(selectedArea.Height / hRatio);

            if (selectedArea.Right >= limitRect.Right)
            {
                var maxWidth = limitRect.Right - selectedArea.X;
                selectedArea = selectedArea.WithWidth(maxWidth);
                selectedArea = selectedArea.WithHeight(maxWidth * hRatio);
            }
        }

        return selectedArea;
    }


    /// <summary>
    /// Moves the current selection to the given location.
    /// </summary>
    private void MoveSelection(Point clientPoint)
    {
        if (!EnableSelection || _selection.PointerDownPoint == null) return;

        var srcPoint = PointClientToSource(clientPoint);
        var srcMouseDownPoint = PointClientToSource(_selection.PointerDownPoint.Value);


        // get the distance the source rect moved
        var dX = srcMouseDownPoint.X - _selection.SourceRectBeforeMoved.X;
        var dY = srcMouseDownPoint.Y - _selection.SourceRectBeforeMoved.Y;


        // get the new selection start point
        var newSrcPoint = new Point(srcPoint.X - dX, srcPoint.Y - dY);


        // limit the new selection to the image source
        if (newSrcPoint.X < 0) newSrcPoint = newSrcPoint.WithX(0); // left edge
        if (newSrcPoint.Y < 0) newSrcPoint = newSrcPoint.WithY(0); // right edge

        // right edge
        if (newSrcPoint.X + _selection.SourceRectBeforeMoved.Width > BitmapSize.Width)
        {
            newSrcPoint = newSrcPoint.WithX(BitmapSize.Width - _selection.SourceRectBeforeMoved.Width);
        }
        // bottom edge
        if (newSrcPoint.Y + _selection.SourceRectBeforeMoved.Height > BitmapSize.Height)
        {
            newSrcPoint = newSrcPoint.WithY(BitmapSize.Height - _selection.SourceRectBeforeMoved.Height);
        }


        // set the final source selection after moved
        var srcRect = new Rect(
            (int)newSrcPoint.X, (int)newSrcPoint.Y,
            (int)_selection.SourceRectBeforeMoved.Width, (int)_selection.SourceRectBeforeMoved.Height);

        SetSourceSelection(srcRect, true);
    }


    /// <summary>
    /// Resizes the current selection.
    /// </summary>
    private void ResizeSelection(Point clientPoint, SelectionResizerType direction)
    {
        if (!EnableSelection || _selection.PointerDownPoint == null) return;

        var srcPoint = PointClientToSource(clientPoint);
        var srcMouseDownPoint = PointClientToSource(_selection.PointerDownPoint.Value);
        var srcSelectionBeforeMoved = _selection.SourceRectBeforeMoved;
        var finalSrcRect = new Rect();

        var newX = SourceSelection.X;
        var newY = SourceSelection.Y;
        var newWidth = SourceSelection.Width;
        var newHeight = SourceSelection.Height;


        #region 1. Get correct size and location of new selection

        var isTopDirections = direction == SelectionResizerType.Top
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.TopRight;

        var isBottomDirections = direction == SelectionResizerType.Bottom
            || direction == SelectionResizerType.BottomLeft
            || direction == SelectionResizerType.BottomRight;

        var isLeftDirections = direction == SelectionResizerType.Left
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.BottomLeft;

        var isRightDirections = direction == SelectionResizerType.Right
            || direction == SelectionResizerType.TopRight
            || direction == SelectionResizerType.BottomRight;


        // top resizers
        if (isTopDirections)
        {
            var gapY = srcSelectionBeforeMoved.Y - srcMouseDownPoint.Y;
            var dH = srcPoint.Y - srcSelectionBeforeMoved.Y + gapY;

            newY = srcSelectionBeforeMoved.Y + dH;
            newHeight = Math.Max(0, srcSelectionBeforeMoved.Height - dH);
        }

        // right resizers
        if (isRightDirections)
        {
            var gapX = srcSelectionBeforeMoved.Right - srcMouseDownPoint.X;
            var dW = srcPoint.X - srcSelectionBeforeMoved.Right + gapX;

            newWidth = Math.Max(0, srcSelectionBeforeMoved.Width + dW);
        }

        // bottom resizers
        if (isBottomDirections)
        {
            var gapY = srcSelectionBeforeMoved.Bottom - srcMouseDownPoint.Y;
            var dH = srcPoint.Y - srcSelectionBeforeMoved.Bottom + gapY;

            newHeight = Math.Max(0, srcSelectionBeforeMoved.Height + dH);
        }

        // left resizers
        if (isLeftDirections)
        {
            var gapX = srcSelectionBeforeMoved.X - srcMouseDownPoint.X;
            var dW = srcPoint.X - srcSelectionBeforeMoved.X + gapX;

            newX = srcSelectionBeforeMoved.X + dW;
            newWidth = Math.Max(0, srcSelectionBeforeMoved.Width - dW);
        }


        // limit the selected client rect to the image source
        var newSrcRect = new Rect(newX, newY, newWidth, newHeight)
            .GetIntersection(new Rect(0, 0, BitmapSize.Width, BitmapSize.Height));

        #endregion // 1. Get correct size and location of new selection


        #region 2. Handle Aspect ratio

        // update selection size according to the ratio
        if (SelectionAspectRatio.Width > 0 && SelectionAspectRatio.Height > 0)
        {
            var wRatio = SelectionAspectRatio.Width / SelectionAspectRatio.Height;
            var hRatio = SelectionAspectRatio.Height / SelectionAspectRatio.Width;

            if (wRatio > hRatio)
            {
                if (direction == SelectionResizerType.Top
                    || direction == SelectionResizerType.TopRight
                    || direction == SelectionResizerType.TopLeft
                    || direction == SelectionResizerType.Bottom
                    || direction == SelectionResizerType.BottomLeft
                    || direction == SelectionResizerType.BottomRight)
                {
                    newSrcRect = newSrcRect.WithWidth(newSrcRect.Height / hRatio);

                    if (newSrcRect.Right >= BitmapSize.Width)
                    {
                        var maxWidth = BitmapSize.Width - newSrcRect.X; ;
                        newSrcRect = newSrcRect.WithWidth(maxWidth);
                        newSrcRect = newSrcRect.WithHeight(maxWidth * hRatio);
                    }
                }
                else
                {
                    newSrcRect = newSrcRect.WithHeight(newSrcRect.Width / wRatio);
                }


                if (newSrcRect.Bottom >= BitmapSize.Height)
                {
                    var maxHeight = BitmapSize.Height - newSrcRect.Y;
                    newSrcRect = newSrcRect.WithWidth(maxHeight * wRatio);
                    newSrcRect = newSrcRect.WithHeight(maxHeight);
                }
            }
            else
            {
                if (direction == SelectionResizerType.Left
                    || direction == SelectionResizerType.TopLeft
                    || direction == SelectionResizerType.BottomLeft
                    || direction == SelectionResizerType.Right
                    || direction == SelectionResizerType.TopRight
                    || direction == SelectionResizerType.BottomRight)
                {
                    newSrcRect = newSrcRect.WithHeight(newSrcRect.Width / wRatio);

                    if (newSrcRect.Bottom >= BitmapSize.Height)
                    {
                        var maxHeight = BitmapSize.Height - newSrcRect.Y;
                        newSrcRect = newSrcRect.WithWidth(maxHeight * wRatio);
                        newSrcRect = newSrcRect.WithHeight(maxHeight);
                    }
                }
                else
                {
                    newSrcRect = newSrcRect.WithWidth(newSrcRect.Height / hRatio);
                }


                if (newSrcRect.Right >= BitmapSize.Width)
                {
                    var maxWidth = BitmapSize.Width - newSrcRect.X;
                    newSrcRect = newSrcRect.WithWidth(maxWidth);
                    newSrcRect = newSrcRect.WithHeight(maxWidth * hRatio);
                }
            }
        }

        #endregion // 2. Handle Aspect ratio


        #region 3. Convert float values to int

        // round the values of location & size
        if (isTopDirections || isLeftDirections)
        {
            finalSrcRect = new Rect(
                (int)newSrcRect.X, (int)newSrcRect.Y,
                (int)Math.Ceiling(newSrcRect.Width),
                (int)Math.Ceiling(newSrcRect.Height));
        }
        else
        {
            finalSrcRect = new Rect(
                (int)newSrcRect.X, (int)newSrcRect.Y,
                (int)Math.Round(newSrcRect.Width),
                (int)Math.Round(newSrcRect.Height));
        }

        #endregion // 3. Convert float values to int


        #region 4. Handle small size (<= 1px)



        // limit the size to 1 pixel
        if (finalSrcRect.Width <= 1) finalSrcRect = finalSrcRect.WithWidth(1);
        if (finalSrcRect.Height <= 1) finalSrcRect = finalSrcRect.WithHeight(1);


        // make sure selection rect is not moved when size <= 1

        // top, top-left, left
        if (direction == SelectionResizerType.Top
            || direction == SelectionResizerType.TopLeft
            || direction == SelectionResizerType.Left)
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect = finalSrcRect.WithX((int)srcSelectionBeforeMoved.Right - 1);
            }
            if (finalSrcRect.Height <= 1)
            {
                finalSrcRect = finalSrcRect.WithY((int)srcSelectionBeforeMoved.Bottom - 1);
            }
        }

        // right, bottom-right, bottom
        else if (direction == SelectionResizerType.Right
            || direction == SelectionResizerType.BottomRight
            || direction == SelectionResizerType.Bottom)
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect = finalSrcRect.WithX((int)srcSelectionBeforeMoved.X);
            }
            if (finalSrcRect.Height <= 1)
            {
                finalSrcRect = finalSrcRect.WithY((int)srcSelectionBeforeMoved.Y);
            }
        }

        // top-right
        else if (direction == SelectionResizerType.TopRight)
        {
            if ((finalSrcRect.Width <= 1 && finalSrcRect.Height <= 1)
                || (finalSrcRect.Width > 1 && finalSrcRect.Height <= 1))
            {
                finalSrcRect = finalSrcRect.WithX((int)srcSelectionBeforeMoved.Left);
                finalSrcRect = finalSrcRect.WithY((int)srcSelectionBeforeMoved.Bottom - 1);
            }
            else if (finalSrcRect.Width <= 1 && finalSrcRect.Height > 1)
            {
                finalSrcRect = finalSrcRect.WithX((int)srcSelectionBeforeMoved.Left);
            }
        }
        // bottom-left
        else
        {
            if (finalSrcRect.Width <= 1)
            {
                finalSrcRect = finalSrcRect.WithX((int)srcSelectionBeforeMoved.Right - 1);
            }
        }

        #endregion // 4. Handle small size (<= 1px)


        SetSourceSelection(finalSrcRect, true);
    }

    #endregion // Private Functions


}

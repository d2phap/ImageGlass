using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ImageGlass {
    // Cyotek ImageBox
    // Copyright (c) 2010-2015 Cyotek Ltd.
    // http://cyotek.com
    // http://cyotek.com/blog/tag/imagebox

    // Licensed under the MIT License. See license.txt for the full text.

    // If you use this control in your applications, attribution, donations or contributions are welcome.

    public class ImageBoxEx: ImageBox {
        #region Instance Fields

        private readonly DragHandleCollection _dragHandles;
        private int _dragHandleSize;
        private Size _maximumSelectionSize;
        private Size _minimumSelectionSize;

        #endregion


        #region Public Constructors

        public ImageBoxEx() {
            _dragHandles = new DragHandleCollection();
            this.DragHandleSize = 8;
            this.MinimumSelectionSize = Size.Empty;
            this.PositionDragHandles();
        }

        #endregion


        #region Events

        /// <summary>
        /// Occurs when the DragHandleSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler DragHandleSizeChanged;

        /// <summary>
        /// Occurs when the MaximumSelectionSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler MaximumSelectionSizeChanged;

        /// <summary>
        /// Occurs when the MinimumSelectionSize property value changes
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler MinimumSelectionSizeChanged;

        [Category("Action")]
        public event EventHandler SelectionMoved;

        [Category("Action")]
        public event CancelEventHandler SelectionMoving;

        [Category("Action")]
        public event EventHandler SelectionResized;

        [Category("Action")]
        public event CancelEventHandler SelectionResizing;

        #endregion


        #region Overridden Methods

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseDown" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseDown(MouseEventArgs e) {
            Point imagePoint;

            imagePoint = this.PointToImage(e.Location);

            if (e.Button == MouseButtons.Left && (this.SelectionRegion.Contains(imagePoint) || this.HitDragHandleTest(e.Location) != DragHandleAnchor.None)) {
                this.DragOrigin = e.Location;
                this.DragOriginOffset = new Point(imagePoint.X - (int)this.SelectionRegion.X, imagePoint.Y - (int)this.SelectionRegion.Y);
            }
            else {
                this.DragOriginOffset = Point.Empty;
                this.DragOrigin = Point.Empty;
            }

            base.OnMouseDown(e);
        }


        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseMove" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e) {
            // start either a move or a resize operation
            if (!this.IsSelecting && !this.IsMovingSelection && !this.IsResizingSelection && e.Button == MouseButtons.Left && !this.DragOrigin.IsEmpty && this.IsOutsideDragZone(e.Location)) {
                DragHandleAnchor anchor;

                anchor = this.HitDragHandleTest(this.DragOrigin);

                if (anchor == DragHandleAnchor.None) {
                    // move
                    this.StartMove();
                }
                else if (this.DragHandles[anchor].Enabled && this.DragHandles[anchor].Visible) {
                    // resize
                    this.StartResize(anchor);
                }
            }

            // set the cursor
            this.SetCursor(e.Location);

            // perform operations
            this.ProcessSelectionMove(e.Location);
            this.ProcessSelectionResize(e.Location);

            base.OnMouseMove(e);
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.MouseUp" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnMouseUp(MouseEventArgs e) {
            if (this.IsMovingSelection) {
                this.CompleteMovingSelection();
            }
            else if (this.IsResizingSelection) {
                this.CompleteResizingSelection();
            }

            base.OnMouseUp(e);
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">
        ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);

            if (this.AllowPainting && !this.SelectionRegion.IsEmpty) {
                foreach (DragHandle handle in this.DragHandles) {
                    if (handle.Visible) {
                        this.DrawDragHandle(e.Graphics, handle);
                    }
                }
            }
        }

        /// <summary>
        ///   Raises the <see cref="ImageBox.PanStart" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.
        /// </param>
        protected override void OnPanStart(CancelEventArgs e) {
            if (this.IsMovingSelection || this.IsResizingSelection || !this.DragOrigin.IsEmpty) {
                e.Cancel = true;
            }

            base.OnPanStart(e);
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.Control.Resize" /> event.
        /// </summary>
        /// <param name="e">
        ///   An <see cref="T:System.EventArgs" /> that contains the event data.
        /// </param>
        protected override void OnResize(EventArgs e) {
            base.OnResize(e);

            this.PositionDragHandles();
        }

        /// <summary>
        ///   Raises the <see cref="System.Windows.Forms.ScrollableControl.Scroll" /> event.
        /// </summary>
        /// <param name="se">
        ///   A <see cref="T:System.Windows.Forms.ScrollEventArgs" /> that contains the event data.
        /// </param>
        protected override void OnScroll(ScrollEventArgs se) {
            base.OnScroll(se);

            this.PositionDragHandles();
        }

        /// <summary>
        ///   Raises the <see cref="ImageBox.Selecting" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected override void OnSelecting(ImageBoxCancelEventArgs e) {
            e.Cancel = this.IsMovingSelection || this.IsResizingSelection || this.SelectionRegion.Contains(this.PointToImage(e.Location)) || this.HitDragHandleTest(e.Location) != DragHandleAnchor.None;

            base.OnSelecting(e);
        }

        /// <summary>
        ///   Raises the <see cref="ImageBox.SelectionRegionChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected override void OnSelectionRegionChanged(EventArgs e) {
            base.OnSelectionRegionChanged(e);

            this.PositionDragHandles();
        }

        /// <summary>
        ///   Raises the <see cref="ImageBox.ZoomChanged" /> event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected override void OnZoomChanged(EventArgs e) {
            base.OnZoomChanged(e);

            this.PositionDragHandles();
        }


        /// <summary>
        ///   Raises the <see cref="ImageBox.AutoScrollPositionChanged" /> event. [IG_CHANGE] This is new event.
        /// </summary>
        /// <param name="e">
        ///   The <see cref="System.EventArgs" /> instance containing the event data.
        /// </param>
        protected override void OnAutoScrollPositionChanged(EventArgs e) {
            base.OnAutoScrollPositionChanged(e);

            this.PositionDragHandles();
        }

        /// <summary>
        /// Processes a dialog key.
        /// </summary>
        /// <returns>
        /// true if the key was processed by the control; otherwise, false.
        /// </returns>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process. </param>
        protected override bool ProcessDialogKey(Keys keyData) {
            bool result;

            if (keyData == Keys.Escape && (this.IsResizingSelection || this.IsMovingSelection)) {
                if (this.IsResizingSelection) {
                    this.CancelResize();
                }
                else {
                    this.CancelMovingSelection();
                }

                result = true;
            }
            else {
                result = base.ProcessDialogKey(keyData);
            }

            return result;
        }

        #endregion


        #region Public Properties

        [Category("Appearance")]
        [DefaultValue(8)]
        public virtual int DragHandleSize {
            get { return _dragHandleSize; }
            set {
                if (this.DragHandleSize != value) {
                    _dragHandleSize = value;

                    this.OnDragHandleSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public DragHandleCollection DragHandles {
            get { return _dragHandles; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsMovingSelection { get; protected set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsResizingSelection { get; protected set; }

        [Category("Behavior")]
        [DefaultValue(typeof(Size), "0, 0")]
        public virtual Size MaximumSelectionSize {
            get { return _maximumSelectionSize; }
            set {
                if (this.MaximumSelectionSize != value) {
                    _maximumSelectionSize = value;

                    this.OnMaximumSelectionSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Behavior")]
        [DefaultValue(typeof(Size), "0, 0")]
        public virtual Size MinimumSelectionSize {
            get { return _minimumSelectionSize; }
            set {
                if (this.MinimumSelectionSize != value) {
                    _minimumSelectionSize = value;

                    this.OnMinimumSelectionSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public RectangleF PreviousSelectionRegion { get; protected set; }

        #endregion


        #region Protected Properties

        protected Point DragOrigin { get; set; }
        protected Point DragOriginOffset { get; set; }
        protected DragHandleAnchor ResizeAnchor { get; set; }
        #endregion


        #region Public Members

        public void CancelResize() {
            this.SelectionRegion = this.PreviousSelectionRegion;
            this.CompleteResizingSelection();
        }

        public void StartMove() {
            CancelEventArgs e;

            if (this.IsMovingSelection || this.IsResizingSelection) {
                throw new InvalidOperationException("A move or resize action is currently being performed.");
            }

            e = new CancelEventArgs();

            this.OnSelectionMoving(e);

            if (!e.Cancel) {
                this.PreviousSelectionRegion = this.SelectionRegion;
                this.IsMovingSelection = true;
            }
        }

        #endregion


        #region Protected Members

        protected virtual void DrawDragHandle(Graphics graphics, DragHandle handle) {
            Pen outerPen;
            Brush innerBrush;

            var left = handle.Bounds.Left;
            var top = handle.Bounds.Top;
            var width = handle.Bounds.Width;
            var height = handle.Bounds.Height;

            if (handle.Enabled) {
                outerPen = SystemPens.WindowFrame;
                innerBrush = SystemBrushes.Window;
            }
            else {
                outerPen = SystemPens.ControlDark;
                innerBrush = SystemBrushes.Control;
            }

            graphics.FillRectangle(innerBrush, left + 1, top + 1, width - 2, height - 2);
            graphics.DrawLine(outerPen, left + 1, top, left + width - 2, top);
            graphics.DrawLine(outerPen, left, top + 1, left, top + height - 2);
            graphics.DrawLine(outerPen, left + 1, top + height - 1, left + width - 2, top + height - 1);
            graphics.DrawLine(outerPen, left + width - 1, top + 1, left + width - 1, top + height - 2);
        }

        /// <summary>
        /// Raises the <see cref="DragHandleSizeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnDragHandleSizeChanged(EventArgs e) {
            EventHandler handler;

            this.PositionDragHandles();
            this.Invalidate();

            handler = this.DragHandleSizeChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MaximumSelectionSizeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnMaximumSelectionSizeChanged(EventArgs e) {
            EventHandler handler;

            handler = this.MaximumSelectionSizeChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="MinimumSelectionSizeChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnMinimumSelectionSizeChanged(EventArgs e) {
            EventHandler handler;

            handler = this.MinimumSelectionSizeChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SelectionMoved" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectionMoved(EventArgs e) {
            EventHandler handler;

            handler = this.SelectionMoved;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SelectionMoving" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectionMoving(CancelEventArgs e) {
            CancelEventHandler handler;

            handler = this.SelectionMoving;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SelectionResized" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectionResized(EventArgs e) {
            EventHandler handler;

            handler = this.SelectionResized;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SelectionResizing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectionResizing(CancelEventArgs e) {
            CancelEventHandler handler;

            handler = this.SelectionResizing;
            handler?.Invoke(this, e);
        }

        #endregion


        #region Private Members

        private void CancelMovingSelection() {
            this.SelectionRegion = this.PreviousSelectionRegion;
            this.CompleteMovingSelection();
        }

        private void CompleteMovingSelection() {
            this.ResetDrag();
            this.OnSelectionMoved(EventArgs.Empty);
        }

        private void CompleteResizingSelection() {
            this.ResetDrag();
            this.OnSelectionResized(EventArgs.Empty);
        }

        private DragHandleAnchor HitDragHandleTest(Point cursorPosition) {
            return this.DragHandles.HitTest(cursorPosition);
        }

        private bool IsOutsideDragZone(Point location) {
            Rectangle dragZone;
            int dragWidth;
            int dragHeight;

            dragWidth = SystemInformation.DragSize.Width;
            dragHeight = SystemInformation.DragSize.Height;
            dragZone = new Rectangle(this.DragOrigin.X - (dragWidth / 2), this.DragOrigin.Y - (dragHeight / 2), dragWidth, dragHeight);

            return !dragZone.Contains(location);
        }

        /// <summary>
        /// [IG_CHANGE]
        /// </summary>
        private void PositionDragHandles() {
            if (this.DragHandles == null || this.DragHandleSize == 0) return;

            // user hasn't selected
            if (this.SelectionRegion.IsEmpty) {
                foreach (DragHandle handle in this.DragHandles) {
                    handle.Bounds = Rectangle.Empty;
                }

                return;
            }

            // get the visible image region
            var viewport = this.GetImageViewPort();
            var offsetX = viewport.Left + this.Padding.Left + this.AutoScrollPosition.X;
            var offsetY = viewport.Top + this.Padding.Top + this.AutoScrollPosition.Y;
            var halfDragHandleSize = this.DragHandleSize / 2;


            // selection factors
            var left = (int)(this.SelectionRegion.Left * this.ZoomFactor) + offsetX;
            var top = (int)(this.SelectionRegion.Top * this.ZoomFactor) + offsetY;
            var right = left + (int)(this.SelectionRegion.Width * this.ZoomFactor);
            var bottom = top + (int)(this.SelectionRegion.Height * this.ZoomFactor);
            var halfWidth = (int)(this.SelectionRegion.Width * this.ZoomFactor) / 2;
            var halfHeight = (int)(this.SelectionRegion.Height * this.ZoomFactor) / 2;


            this.DragHandles[DragHandleAnchor.TopLeft].Bounds = new Rectangle(
                left - this.DragHandleSize,
                top - this.DragHandleSize,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.TopCenter].Bounds = new Rectangle(
                left + halfWidth - halfDragHandleSize,
                top - this.DragHandleSize,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.TopRight].Bounds = new Rectangle(
                right,
                top - this.DragHandleSize,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.MiddleLeft].Bounds = new Rectangle(
                left - this.DragHandleSize,
                top + halfHeight - halfDragHandleSize,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.MiddleRight].Bounds = new Rectangle(
                right,
                top + halfHeight - halfDragHandleSize,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.BottomLeft].Bounds = new Rectangle(
                left - this.DragHandleSize,
                bottom,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.BottomCenter].Bounds = new Rectangle(
                left + halfWidth - halfDragHandleSize,
                bottom,
                this.DragHandleSize,
                this.DragHandleSize);

            this.DragHandles[DragHandleAnchor.BottomRight].Bounds = new Rectangle(
                right,
                bottom,
                this.DragHandleSize,
                this.DragHandleSize);
        }

        private void ProcessSelectionMove(Point cursorPosition) {
            if (this.IsMovingSelection) {
                int x;
                int y;
                Point imagePoint;

                imagePoint = this.PointToImage(cursorPosition, false);

                x = Math.Max(0, imagePoint.X - this.DragOriginOffset.X);
                if (x + this.SelectionRegion.Width >= this.ViewSize.Width) {
                    x = this.ViewSize.Width - (int)this.SelectionRegion.Width;
                }

                y = Math.Max(0, imagePoint.Y - this.DragOriginOffset.Y);
                if (y + this.SelectionRegion.Height >= this.ViewSize.Height) {
                    y = this.ViewSize.Height - (int)this.SelectionRegion.Height;
                }

                this.SelectionRegion = new RectangleF(x, y, this.SelectionRegion.Width, this.SelectionRegion.Height);
            }
        }

        private void ProcessSelectionResize(Point cursorPosition) {
            if (this.IsResizingSelection) {
                Point imagePosition;
                float left;
                float top;
                float right;
                float bottom;
                bool resizingTopEdge;
                bool resizingBottomEdge;
                bool resizingLeftEdge;
                bool resizingRightEdge;

                imagePosition = this.PointToImage(cursorPosition);

                // get the current selection
                left = this.SelectionRegion.Left;
                top = this.SelectionRegion.Top;
                right = this.SelectionRegion.Right;
                bottom = this.SelectionRegion.Bottom;

                // decide which edges we're resizing
                resizingTopEdge = this.ResizeAnchor >= DragHandleAnchor.TopLeft && this.ResizeAnchor <= DragHandleAnchor.TopRight;
                resizingBottomEdge = this.ResizeAnchor >= DragHandleAnchor.BottomLeft && this.ResizeAnchor <= DragHandleAnchor.BottomRight;
                resizingLeftEdge = this.ResizeAnchor == DragHandleAnchor.TopLeft || this.ResizeAnchor == DragHandleAnchor.MiddleLeft || this.ResizeAnchor == DragHandleAnchor.BottomLeft;
                resizingRightEdge = this.ResizeAnchor == DragHandleAnchor.TopRight || this.ResizeAnchor == DragHandleAnchor.MiddleRight || this.ResizeAnchor == DragHandleAnchor.BottomRight;

                // and resize!
                if (resizingTopEdge) {
                    top = imagePosition.Y > 0 ? imagePosition.Y : 0;

                    if (bottom - top < this.MinimumSelectionSize.Height) {
                        top = bottom - this.MinimumSelectionSize.Height;
                    }
                    else if (this.MaximumSelectionSize.Height > 0 && bottom - top > this.MaximumSelectionSize.Height) {
                        top = bottom - this.MaximumSelectionSize.Height;
                    }
                }
                else if (resizingBottomEdge) {
                    bottom = imagePosition.Y < this.ViewSize.Height ? imagePosition.Y : this.ViewSize.Height;

                    if (bottom - top < this.MinimumSelectionSize.Height) {
                        bottom = top + this.MinimumSelectionSize.Height;
                    }
                    else if (this.MaximumSelectionSize.Height > 0 && bottom - top > this.MaximumSelectionSize.Height) {
                        bottom = top + this.MaximumSelectionSize.Height;
                    }
                }

                if (resizingLeftEdge) {
                    left = imagePosition.X > 0 ? imagePosition.X : 0;

                    if (right - left < this.MinimumSelectionSize.Width) {
                        left = right - this.MinimumSelectionSize.Width;
                    }
                    else if (this.MaximumSelectionSize.Width > 0 && right - left > this.MaximumSelectionSize.Width) {
                        left = right - this.MaximumSelectionSize.Width;
                    }
                }
                else if (resizingRightEdge) {
                    right = imagePosition.X < this.ViewSize.Width ? imagePosition.X : this.ViewSize.Width;

                    if (right - left < this.MinimumSelectionSize.Width) {
                        right = left + this.MinimumSelectionSize.Width;
                    }
                    else if (this.MaximumSelectionSize.Width > 0 && right - left > this.MaximumSelectionSize.Width) {
                        right = left + this.MaximumSelectionSize.Width;
                    }
                }

                this.SelectionRegion = new RectangleF(left, top, right - left, bottom - top);
            }
        }

        private void ResetDrag() {
            this.IsResizingSelection = false;
            this.IsMovingSelection = false;
            this.DragOrigin = Point.Empty;
            this.DragOriginOffset = Point.Empty;
        }

        protected virtual void SetCursor(Point point) {
            // http://forums.cyotek.com/imagebox/cursor-issue-in-imageboxex/msg92/#msg92

            if (this.IsPanning) return;

            // default cursor
            var cursor = Cursors.Default;

            if (!this.IsSelecting) {
                var handleAnchor = this.IsResizingSelection ? this.ResizeAnchor : this.HitDragHandleTest(point);

                if (handleAnchor != DragHandleAnchor.None && this.DragHandles[handleAnchor].Enabled) {
                    switch (handleAnchor) {
                        case DragHandleAnchor.TopLeft:
                        case DragHandleAnchor.BottomRight:
                            cursor = Cursors.SizeNWSE;
                            break;
                        case DragHandleAnchor.TopCenter:
                        case DragHandleAnchor.BottomCenter:
                            cursor = Cursors.SizeNS;
                            break;
                        case DragHandleAnchor.TopRight:
                        case DragHandleAnchor.BottomLeft:
                            cursor = Cursors.SizeNESW;
                            break;
                        case DragHandleAnchor.MiddleLeft:
                        case DragHandleAnchor.MiddleRight:
                            cursor = Cursors.SizeWE;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (this.IsMovingSelection || this.SelectionRegion.Contains(this.PointToImage(point))) {
                    cursor = Cursors.SizeAll;
                }
                else {
                    cursor = Cursors.Default;
                }
            }

            this.Cursor = cursor;
        }

        private void StartResize(DragHandleAnchor anchor) {
            if (this.IsMovingSelection || this.IsResizingSelection) {
                throw new InvalidOperationException("A move or resize action is currently being performed.");
            }

            var e = new CancelEventArgs();

            this.OnSelectionResizing(e);

            if (!e.Cancel) {
                this.ResizeAnchor = anchor;
                this.PreviousSelectionRegion = this.SelectionRegion;
                this.IsResizingSelection = true;
            }
        }

        #endregion
    }
}

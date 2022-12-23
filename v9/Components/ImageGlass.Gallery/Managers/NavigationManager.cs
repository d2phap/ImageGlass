/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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

---------------------
ImageGlass.Gallery is based on ImageListView v13.8.2:
Url: https://github.com/oozcitak/imagelistview
License: Apache License Version 2.0, http://www.apache.org/licenses/
---------------------
*/
namespace ImageGlass.Gallery;


public partial class ImageGallery
{
    /// <summary>
    /// Represents details of keyboard and mouse navigation events.
    /// </summary>
    internal class NavigationManager : IDisposable
    {

        #region Member Variables
        private readonly ImageGallery _imageGallery;

        private bool inItemArea = false;
        private bool overCheckBox = false;

        private Point lastViewOffset = new();
        private Point lastMouseDownLocation = new();
        private readonly Dictionary<ImageGalleryItem, bool> highlightedItems = new();

        private bool lastMouseDownInItemArea = false;
        private bool lastMouseDownOverItem = false;
        private bool lastMouseDownOverCheckBox = false;

        private bool selfDragging = false;

        private readonly System.Windows.Forms.Timer scrollTimer = new()
        {
            Interval = 100,
            Enabled = false,
        };

        #endregion


        #region Properties
        /// <summary>
        /// Gets whether the left mouse button is down.
        /// </summary>
        public bool LeftButton { get; private set; } = false;

        /// <summary>
        /// Gets whether the right mouse button is down.
        /// </summary>
        public bool RightButton { get; private set; } = false;

        /// <summary>
        /// Gets whether the shift key is down.
        /// </summary>
        public bool ShiftKey { get; private set; } = false;

        /// <summary>
        /// Gets whether the control key is down.
        /// </summary>
        public bool ControlKey { get; private set; } = false;

        /// <summary>
        /// Gets the item under the mouse.
        /// </summary>
        public ImageGalleryItem? HoveredItem { get; private set; } = null;

        /// <summary>
        /// Gets whether a mouse selection is in progress.
        /// </summary>
        public bool MouseSelecting { get; private set; } = false;

        /// <summary>
        /// Gets the target item for a drop operation.
        /// </summary>
        public ImageGalleryItem? DropTarget { get; private set; } = null;

        /// <summary>
        /// Gets whether drop target is to the right of the item.
        /// </summary>
        public bool DropToRight { get; private set; } = false;

        /// <summary>
        /// Gets the selection rectangle.
        /// </summary>
        public Rectangle SelectionRectangle { get; private set; } = new();

        #endregion


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationManager"/> class.
        /// </summary>
        /// <param name="owner">The owner control.</param>
        public NavigationManager(ImageGallery owner)
        {
            _imageGallery = owner;
            scrollTimer.Tick += new EventHandler(ScrollTimer_Tick);
        }

        #endregion


        #region Instance Methods
        /// <summary>
        /// Determines whether the item is highlighted.
        /// </summary>
        public ItemHighlightState HighlightState(ImageGalleryItem item)
        {
            if (highlightedItems.TryGetValue(item, out bool highlighted))
            {
                if (highlighted)
                    return ItemHighlightState.HighlightedAndSelected;
                else
                    return ItemHighlightState.HighlightedAndUnSelected;
            }
            return ItemHighlightState.NotHighlighted;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            scrollTimer.Dispose();
        }

        #endregion


        #region Mouse Event Handlers
        /// <summary>
        /// Handles control's MouseDown event.
        /// </summary>
        public void MouseDown(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
                LeftButton = true;
            if ((e.Button & MouseButtons.Right) != MouseButtons.None)
                RightButton = true;

            DoHitTest(e.Location);
            lastMouseDownInItemArea = inItemArea;
            lastMouseDownOverItem = HoveredItem != null;
            lastMouseDownOverCheckBox = overCheckBox;

            lastViewOffset = _imageGallery.ViewOffset;
            lastMouseDownLocation = e.Location;

            if (HoveredItem is not null)
            {
                HoveredItem.Pressed = true;
            }
        }

        /// <summary>
        /// Handles control's MouseMove event.
        /// </summary>
        public void MouseMove(MouseEventArgs e)
        {
            var oldHoveredItem = HoveredItem;

            DoHitTest(e.Location);

            _imageGallery.SuspendPaint();

            // Do we need to scroll the view?
            if (MouseSelecting && _imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll && !scrollTimer.Enabled)
            {
                if (e.Y > _imageGallery.ClientRectangle.Bottom)
                {
                    scrollTimer.Tag = -SystemInformation.MouseWheelScrollDelta;
                    scrollTimer.Enabled = true;
                }
                else if (e.Y < _imageGallery.ClientRectangle.Top)
                {
                    scrollTimer.Tag = SystemInformation.MouseWheelScrollDelta;
                    scrollTimer.Enabled = true;
                }
            }
            else if (MouseSelecting && _imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll && !scrollTimer.Enabled)
            {
                if (e.X > _imageGallery.ClientRectangle.Right)
                {
                    scrollTimer.Tag = -SystemInformation.MouseWheelScrollDelta;
                    scrollTimer.Enabled = true;
                }
                else if (e.X < _imageGallery.ClientRectangle.Left)
                {
                    scrollTimer.Tag = SystemInformation.MouseWheelScrollDelta;
                    scrollTimer.Enabled = true;
                }
            }
            else if (scrollTimer.Enabled && _imageGallery.ClientRectangle.Contains(e.Location))
            {
                scrollTimer.Enabled = false;
            }


            if (MouseSelecting)
            {
                if (!ShiftKey && !ControlKey)
                    _imageGallery.SelectedItems.Clear(false);

                // Create the selection rectangle
                var viewOffset = _imageGallery.ViewOffset;
                var pt1 = new Point(
                    lastMouseDownLocation.X - (viewOffset.X - lastViewOffset.X),
                    lastMouseDownLocation.Y - (viewOffset.Y - lastViewOffset.Y));
                var pt2 = new Point(e.Location.X, e.Location.Y);
                SelectionRectangle = new Rectangle(Math.Min(pt1.X, pt2.X), Math.Min(pt1.Y, pt2.Y), Math.Abs(pt1.X - pt2.X), Math.Abs(pt1.Y - pt2.Y));

                // Determine which items are highlighted
                highlightedItems.Clear();

                // Normalize to item area coordinates
                pt1 = new Point(SelectionRectangle.Left, SelectionRectangle.Top);
                pt2 = new Point(SelectionRectangle.Right, SelectionRectangle.Bottom);
                var itemAreaOffset = new Point(
                    -_imageGallery.layoutManager.ItemAreaBounds.Left,
                    -_imageGallery.layoutManager.ItemAreaBounds.Top);
                pt1.Offset(itemAreaOffset);
                pt2.Offset(itemAreaOffset);

                var startRow = (int)Math.Floor((Math.Min(pt1.Y, pt2.Y) + viewOffset.Y) /
                    (float)_imageGallery.layoutManager.ItemSizeWithMargin.Height);
                var endRow = (int)Math.Floor((Math.Max(pt1.Y, pt2.Y) + viewOffset.Y) /
                    (float)_imageGallery.layoutManager.ItemSizeWithMargin.Height);
                var startCol = (int)Math.Floor((Math.Min(pt1.X, pt2.X) + viewOffset.X) /
                    (float)_imageGallery.layoutManager.ItemSizeWithMargin.Width);
                var endCol = (int)Math.Floor((Math.Max(pt1.X, pt2.X) + viewOffset.X) /
                    (float)_imageGallery.layoutManager.ItemSizeWithMargin.Width);

                if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll
                    && (startRow >= 0 || endRow >= 0))
                {
                    for (int i = startCol; i <= endCol; i++)
                    {
                        if (i >= 0
                            && i <= _imageGallery.Items.Count - 1
                            && !highlightedItems.ContainsKey(_imageGallery.Items[i])
                            && _imageGallery.Items[i].Enabled)
                        {
                            highlightedItems.Add(
                                _imageGallery.Items[i],
                                !ControlKey || !_imageGallery.Items[i].Selected);
                        }
                    }
                }
                else if (_imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll
                    && (startCol >= 0 || endCol >= 0)
                    && (startRow >= 0 || endRow >= 0)
                    && (startCol <= _imageGallery.layoutManager.Cols - 1 || endCol <= _imageGallery.layoutManager.Cols - 1))
                {
                    startCol = Math.Min(_imageGallery.layoutManager.Cols - 1, Math.Max(0, startCol));
                    endCol = Math.Min(_imageGallery.layoutManager.Cols - 1, Math.Max(0, endCol));

                    for (int row = startRow; row <= endRow; row++)
                    {
                        for (int col = startCol; col <= endCol; col++)
                        {
                            int i = row * _imageGallery.layoutManager.Cols + col;
                            if (i >= 0
                                && i <= _imageGallery.Items.Count - 1
                                && !highlightedItems.ContainsKey(_imageGallery.Items[i])
                                && _imageGallery.Items[i].Enabled)
                            {
                                highlightedItems.Add(
                                    _imageGallery.Items[i],
                                    !ControlKey || !_imageGallery.Items[i].Selected);
                            }
                        }
                    }
                }


                _imageGallery.Refresh();
            }
            else if (!MouseSelecting
                && inItemArea
                && lastMouseDownInItemArea
                && (LeftButton || RightButton)
                && (Math.Abs(e.Location.X - lastMouseDownLocation.X) > SystemInformation.DragSize.Width
                    || Math.Abs(e.Location.Y - lastMouseDownLocation.Y) > SystemInformation.DragSize.Height))
            {
                if (_imageGallery.MultiSelect && !lastMouseDownOverItem && HoveredItem == null)
                {
                    // Start mouse selection
                    MouseSelecting = true;
                    SelectionRectangle = new Rectangle(lastMouseDownLocation, new Size(0, 0));
                    _imageGallery.Refresh();
                }
                else if (lastMouseDownOverItem && HoveredItem != null && (_imageGallery.AllowItemReorder || _imageGallery.AllowDrag))
                {
                    // Start drag&drop
                    if (!HoveredItem.Selected)
                    {
                        _imageGallery.SelectedItems.Clear(false);
                        HoveredItem.mSelected = true;
                        _imageGallery.OnSelectionChangedInternal();
                        DropTarget = null;
                        _imageGallery.Refresh(true);
                    }

                    DropTarget = null;
                    selfDragging = true;
                    var oldAllowDrop = _imageGallery.AllowDrop;
                    _imageGallery.AllowDrop = true;

                    if (_imageGallery.AllowDrag)
                    {
                        // Set drag data
                        var filenames = new List<string>();
                        foreach (var item in _imageGallery.SelectedItems)
                        {
                            // Get the source image
                            var sourceFile = item.Adaptor.GetSourceImage(item.VirtualItemKey);
                            if (!string.IsNullOrEmpty(sourceFile))
                                filenames.Add(sourceFile);
                        }
                        var data = new DataObject(DataFormats.FileDrop, filenames.ToArray());
                        _imageGallery.DoDragDrop(data, DragDropEffects.All);
                    }
                    else
                    {
                        _imageGallery.DoDragDrop(new object(), DragDropEffects.Move);
                    }
                    _imageGallery.AllowDrop = oldAllowDrop;
                    selfDragging = false;

                    // Since the MouseUp event will be eaten by DoDragDrop we will not receive
                    // the MouseUp event. We need to manually update mouse button flags after
                    // the drop.
                    if ((MouseButtons & MouseButtons.Left) == MouseButtons.None)
                        LeftButton = false;
                    if ((MouseButtons & MouseButtons.Right) == MouseButtons.None)
                        RightButton = false;
                }
            }

            else if (!ReferenceEquals(HoveredItem, oldHoveredItem))
            {
                // Hovered item changed
                if (!ReferenceEquals(HoveredItem, oldHoveredItem))
                    _imageGallery.OnItemHover(new ItemHoverEventArgs(HoveredItem, oldHoveredItem));

                _imageGallery.Refresh();
            }

            _imageGallery.ResumePaint();
        }

        /// <summary>
        /// Handles control's MouseUp event.
        /// </summary>
        public void MouseUp(MouseEventArgs e)
        {
            DoHitTest(e.Location);

            _imageGallery.SuspendPaint();

            // Stop if we are scrolling
            if (scrollTimer.Enabled)
                scrollTimer.Enabled = false;

            if (MouseSelecting)
            {
                // Apply highlighted items
                if (highlightedItems.Count != 0)
                {
                    foreach (KeyValuePair<ImageGalleryItem, bool> pair in highlightedItems)
                    {
                        if (pair.Key.Enabled)
                            pair.Key.mSelected = pair.Value;
                    }
                    highlightedItems.Clear();
                }

                _imageGallery.OnSelectionChangedInternal();

                MouseSelecting = false;

                _imageGallery.Refresh();
            }
            else if (_imageGallery.AllowCheckBoxClick
                && lastMouseDownInItemArea
                && lastMouseDownOverCheckBox
                && HoveredItem != null
                && overCheckBox
                && LeftButton)
            {
                if (HoveredItem.Selected)
                {
                    // if multiple items selected and Hovered item among selected,
                    // then give all selected check state !HoveredItem.Checked
                    bool check = !HoveredItem.Checked;
                    foreach (ImageGalleryItem item in _imageGallery.Items)
                    {
                        if (item.Selected)
                            item.Checked = check;
                    }
                }
                else
                {
                    // if multiple items selected and HoveredItem NOT among selected,
                    // or if only HoveredItem selected or hovered
                    // then toggle HoveredItem.Checked
                    HoveredItem.Checked = !HoveredItem.Checked;
                }
                _imageGallery.Refresh();
            }
            else if (lastMouseDownInItemArea
                && lastMouseDownOverItem
                && HoveredItem != null
                && LeftButton)
            {
                // Select the item under the cursor
                if (!_imageGallery.MultiSelect && ControlKey)
                {
                    bool oldSelected = HoveredItem.Selected;
                    _imageGallery.SelectedItems.Clear(false);
                    HoveredItem.mSelected = !oldSelected;
                }
                else if (!_imageGallery.MultiSelect)
                {
                    _imageGallery.SelectedItems.Clear(false);
                    HoveredItem.mSelected = true;
                }
                else if (ControlKey)
                {
                    HoveredItem.mSelected = !HoveredItem.mSelected;
                }
                else if (ShiftKey)
                {
                    var startIndex = 0;
                    if (_imageGallery.SelectedItems.Count != 0)
                    {
                        startIndex = _imageGallery.SelectedItems[0].Index;
                        _imageGallery.SelectedItems.Clear(false);
                    }

                    var endIndex = HoveredItem.Index;
                    if (_imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll)
                    {
                        var startRow = Math.Min(startIndex, endIndex) / _imageGallery.layoutManager.Cols;
                        var endRow = Math.Max(startIndex, endIndex) / _imageGallery.layoutManager.Cols;
                        var startCol = Math.Min(startIndex, endIndex) % _imageGallery.layoutManager.Cols;
                        var endCol = Math.Max(startIndex, endIndex) % _imageGallery.layoutManager.Cols;

                        for (var row = startRow; row <= endRow; row++)
                        {
                            for (var col = startCol; col <= endCol; col++)
                            {
                                var index = row * _imageGallery.layoutManager.Cols + col;
                                _imageGallery.Items[index].mSelected = true;
                            }
                        }
                    }
                    else
                    {
                        for (var i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
                        {
                            _imageGallery.Items[i].mSelected = true;
                        }
                    }
                }
                else
                {
                    _imageGallery.SelectedItems.Clear(false);
                    HoveredItem.mSelected = true;
                }

                // Raise the selection change event
                _imageGallery.OnSelectionChangedInternal();
                _imageGallery.OnItemClick(new ItemClickEventArgs(HoveredItem, e.Location, e.Button));

                // Set the item as the focused item
                _imageGallery.Items.FocusedItem = HoveredItem;

                _imageGallery.Refresh();
            }
            else if (lastMouseDownInItemArea
                && lastMouseDownOverItem
                && HoveredItem != null
                && RightButton)
            {
                if (!ControlKey && !HoveredItem.Selected)
                {
                    // Clear the selection if Control key is not pressed
                    _imageGallery.SelectedItems.Clear(false);
                    HoveredItem.mSelected = true;
                    _imageGallery.OnSelectionChangedInternal();
                }

                _imageGallery.OnItemClick(new ItemClickEventArgs(HoveredItem, e.Location, e.Button));
                _imageGallery.Items.FocusedItem = HoveredItem;
            }
            else if (lastMouseDownInItemArea
                && inItemArea
                && HoveredItem == null
                && (LeftButton || RightButton))
            {
                // Clear selection if clicked in empty space
                _imageGallery.SelectedItems.Clear();
                _imageGallery.Refresh();
            }

            if (HoveredItem is not null)
            {
                HoveredItem.Pressed = false;
            }

            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
                LeftButton = false;
            if ((e.Button & MouseButtons.Right) != MouseButtons.None)
                RightButton = false;

            _imageGallery.ResumePaint();
        }

        /// <summary>
        /// Handles control's MouseDoubleClick event.
        /// </summary>
        public void MouseDoubleClick(MouseEventArgs e)
        {
            if (lastMouseDownInItemArea && lastMouseDownOverItem && HoveredItem != null)
            {
                _imageGallery.OnItemDoubleClick(new ItemClickEventArgs(HoveredItem, e.Location, e.Button));
            }
        }

        /// <summary>
        /// Handles control's MouseLeave event.
        /// </summary>
        public void MouseLeave()
        {
            if (HoveredItem != null)
            {
                if (HoveredItem != null)
                {
                    _imageGallery.OnItemHover(new ItemHoverEventArgs(null, HoveredItem));
                    HoveredItem.Pressed = false;
                }

                HoveredItem = null;
                _imageGallery.Refresh();
            }
        }

        #endregion


        #region Key Event Handlers
        /// <summary>
        /// Handles control's KeyDown event.
        /// </summary>
        public void KeyDown(KeyEventArgs e)
        {
            if (!_imageGallery.EnableKeyNavigation)
            {
                return;
            }

            ShiftKey = (e.Modifiers & Keys.Shift) == Keys.Shift;
            ControlKey = (e.Modifiers & Keys.Control) == Keys.Control;

            _imageGallery.SuspendPaint();

            // If the shift key or the control key is pressed and there is no focused item
            // set the first item as the focused item.
            if ((ShiftKey || ControlKey)
                && _imageGallery.Items.Count != 0
                && _imageGallery.Items.FocusedItem == null)
            {
                _imageGallery.Items.FocusedItem = _imageGallery.Items[0];
                _imageGallery.Refresh();
            }

            if (_imageGallery.Items.Count != 0)
            {
                var index = 0;
                if (_imageGallery.Items.FocusedItem != null)
                    index = _imageGallery.Items.FocusedItem.Index;

                var newindex = ApplyNavKey(index, e.KeyCode);
                if (index != newindex)
                {
                    if (ControlKey)
                    {
                        // Just move the focus
                    }
                    else if (_imageGallery.MultiSelect && ShiftKey)
                    {
                        var startIndex = 0;
                        var endIndex = 0;
                        var selCount = _imageGallery.SelectedItems.Count;
                        if (selCount != 0)
                        {
                            startIndex = _imageGallery.SelectedItems[0].Index;
                            endIndex = _imageGallery.SelectedItems[selCount - 1].Index;
                            _imageGallery.SelectedItems.Clear(false);
                        }

                        if (newindex > index) // Moving right or down
                        {
                            if (newindex > endIndex)
                                endIndex = newindex;
                            else
                                startIndex = newindex;
                        }
                        else // Moving left or up
                        {
                            if (newindex < startIndex)
                                startIndex = newindex;
                            else
                                endIndex = newindex;
                        }

                        for (var i = Math.Min(startIndex, endIndex); i <= Math.Max(startIndex, endIndex); i++)
                        {
                            if (_imageGallery.Items[i].mEnabled)
                                _imageGallery.Items[i].mSelected = true;
                        }
                        _imageGallery.OnSelectionChangedInternal();
                    }
                    else if (_imageGallery.Items[newindex].mEnabled)
                    {
                        _imageGallery.SelectedItems.Clear(false);
                        _imageGallery.Items[newindex].mSelected = true;
                        _imageGallery.OnSelectionChangedInternal();
                    }
                    _imageGallery.Items.FocusedItem = _imageGallery.Items[newindex];
                    _imageGallery.ScrollToIndex(newindex);
                    _imageGallery.Refresh();
                }
            }

            _imageGallery.ResumePaint();
        }

        /// <summary>
        /// Handles control's KeyUp event.
        /// </summary>
        public void KeyUp(KeyEventArgs e)
        {
            ShiftKey = (e.Modifiers & Keys.Shift) == Keys.Shift;
            ControlKey = (e.Modifiers & Keys.Control) == Keys.Control;
        }

        #endregion


        #region Drag and Drop Event Handlers
        /// <summary>
        /// Handles control's DragDrop event.
        /// </summary>
        public void DragDrop(DragEventArgs e)
        {
            _imageGallery.SuspendPaint();

            if (selfDragging)
            {
                int index = -1;
                if (DropTarget != null) index = DropTarget.Index;
                if (DropToRight) index++;
                if (index > _imageGallery.Items.Count)
                {
                    index = _imageGallery.Items.Count;
                }

                if (index != -1)
                {
                    var i = 0;
                    var draggedItems = new ImageGalleryItem[_imageGallery.SelectedItems.Count];
                    foreach (var item in _imageGallery.SelectedItems)
                    {
                        draggedItems[i] = item;
                        i++;
                    }

                    _imageGallery.OnDropItems(new DropItemEventArgs(index, draggedItems));
                }
            }
            else
            {
                var index = _imageGallery.Items.Count;
                if (DropTarget != null) index = DropTarget.Index;
                if (DropToRight) index++;
                if (index > _imageGallery.Items.Count)
                    index = _imageGallery.Items.Count;

                if (index != -1)
                {
                    if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        var filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

                        _imageGallery.OnDropFiles(new DropFileEventArgs(index, filenames));
                    }
                }
            }

            DropTarget = null;
            selfDragging = false;

            _imageGallery.Refresh();
            _imageGallery.ResumePaint();
        }

        /// <summary>
        /// Handles control's DragEnter event.
        /// </summary>
        public void DragEnter(DragEventArgs e)
        {
            if (selfDragging)
                e.Effect = DragDropEffects.Move;
            else if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Handles control's DragOver event.
        /// </summary>
        public void DragOver(DragEventArgs e)
        {
            if ((selfDragging && _imageGallery.AllowItemReorder)
                || (!selfDragging
                    && _imageGallery.AllowDrop
                    && e.Data != null
                    && e.Data.GetDataPresent(DataFormats.FileDrop)))
            {
                if (_imageGallery.Items.Count == 0)
                {
                    if (selfDragging)
                        e.Effect = DragDropEffects.None;
                    else
                        e.Effect = DragDropEffects.Copy;
                }
                else if (_imageGallery.AllowItemReorder)
                {
                    // Calculate the location of the insertion cursor
                    var pt = new Point(e.X, e.Y);
                    pt = _imageGallery.PointToClient(pt);

                    // Do we need to scroll the view?
                    if (_imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll &&
                        pt.Y > _imageGallery.ClientRectangle.Bottom - 20)
                    {
                        scrollTimer.Tag = -SystemInformation.MouseWheelScrollDelta;
                        scrollTimer.Enabled = true;
                    }
                    else if (_imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll
                        && pt.Y < _imageGallery.ClientRectangle.Top + 20)
                    {
                        scrollTimer.Tag = SystemInformation.MouseWheelScrollDelta;
                        scrollTimer.Enabled = true;
                    }
                    else if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll
                        && pt.X > _imageGallery.ClientRectangle.Right - 20)
                    {
                        scrollTimer.Tag = -SystemInformation.MouseWheelScrollDelta;
                        scrollTimer.Enabled = true;
                    }
                    else if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll
                        && pt.X < _imageGallery.ClientRectangle.Left + 20)
                    {
                        scrollTimer.Tag = SystemInformation.MouseWheelScrollDelta;
                        scrollTimer.Enabled = true;
                    }
                    else
                    {
                        scrollTimer.Enabled = false;
                    }

                    // Normalize to item area coordinates
                    pt.X -= _imageGallery.layoutManager.ItemAreaBounds.Left;
                    pt.Y -= _imageGallery.layoutManager.ItemAreaBounds.Top;

                    // Row and column mouse is over
                    bool dragCaretOnRight = false;
                    int index;

                    if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll)
                    {
                        index = (pt.X + _imageGallery.ViewOffset.X) / _imageGallery.layoutManager.ItemSizeWithMargin.Width;
                    }
                    else
                    {
                        int col = pt.X / _imageGallery.layoutManager.ItemSizeWithMargin.Width;
                        int row = (pt.Y + _imageGallery.ViewOffset.Y) / _imageGallery.layoutManager.ItemSizeWithMargin.Height;
                        if (col > _imageGallery.layoutManager.Cols - 1)
                        {
                            col = _imageGallery.layoutManager.Cols - 1;
                            dragCaretOnRight = true;
                        }
                        index = row * _imageGallery.layoutManager.Cols + col;
                    }

                    if (index < 0) index = 0;
                    if (index > _imageGallery.Items.Count - 1)
                    {
                        index = _imageGallery.Items.Count - 1;
                        dragCaretOnRight = true;
                    }

                    var dragDropTarget = _imageGallery.Items[index];

                    if (selfDragging && (dragDropTarget.Selected ||
                        (!dragCaretOnRight && index > 0 && _imageGallery.Items[index - 1].Selected) ||
                        (dragCaretOnRight && index < _imageGallery.Items.Count - 1 && _imageGallery.Items[index + 1].Selected)))
                    {
                        e.Effect = DragDropEffects.None;

                        dragDropTarget = null;
                    }
                    else if (selfDragging)
                        e.Effect = DragDropEffects.Move;
                    else
                        e.Effect = DragDropEffects.Copy;

                    if (!ReferenceEquals(dragDropTarget, DropTarget) || dragCaretOnRight != DropToRight)
                    {
                        DropTarget = dragDropTarget;
                        DropToRight = dragCaretOnRight;
                        _imageGallery.Refresh(true);
                    }
                }
                else
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Handles control's DragLeave event.
        /// </summary>
        public void DragLeave()
        {
            DropTarget = null;
            _imageGallery.Refresh(true);

            if (scrollTimer.Enabled)
                scrollTimer.Enabled = false;
        }

        #endregion


        #region Helper Methods
        /// <summary>
        /// Performs a hit test.
        /// </summary>
        private void DoHitTest(Point pt)
        {
            _imageGallery.HitTest(pt, out HitInfo h);

            if (h.IsItemHit && _imageGallery.Items[h.ItemIndex].Enabled)
            {
                HoveredItem = _imageGallery.Items[h.ItemIndex];
            }
            else
            {
                HoveredItem = null;
            }

            inItemArea = h.IsInItemArea;
            overCheckBox = h.IsCheckBoxHit;
        }

        /// <summary>
        /// Returns the item index after applying the given navigation key.
        /// </summary>
        private int ApplyNavKey(int index, Keys key)
        {
            if (_imageGallery.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                if (key == Keys.Up && index >= _imageGallery.layoutManager.Cols)
                    index -= _imageGallery.layoutManager.Cols;
                else if (key == Keys.Down && index < _imageGallery.Items.Count - _imageGallery.layoutManager.Cols)
                    index += _imageGallery.layoutManager.Cols;
                else if (key == Keys.Left && index > 0)
                    index--;
                else if (key == Keys.Right && index < _imageGallery.Items.Count - 1)
                    index++;
                else if (key == Keys.PageUp && index >= _imageGallery.layoutManager.Cols * (_imageGallery.layoutManager.Rows - 1))
                    index -= _imageGallery.layoutManager.Cols * (_imageGallery.layoutManager.Rows - 1);
                else if (key == Keys.PageDown && index < _imageGallery.Items.Count - _imageGallery.layoutManager.Cols * (_imageGallery.layoutManager.Rows - 1))
                    index += _imageGallery.layoutManager.Cols * (_imageGallery.layoutManager.Rows - 1);
                else if (key == Keys.Home)
                    index = 0;
                else if (key == Keys.End)
                    index = _imageGallery.Items.Count - 1;
            }
            else
            {
                if (key == Keys.Left && index > 0)
                    index--;
                else if (key == Keys.Right && index < _imageGallery.Items.Count - 1)
                    index++;
                else if (key == Keys.PageUp && index >= _imageGallery.layoutManager.Cols)
                    index -= _imageGallery.layoutManager.Cols;
                else if (key == Keys.PageDown && index < _imageGallery.Items.Count - _imageGallery.layoutManager.Cols)
                    index += _imageGallery.layoutManager.Cols;
                else if (key == Keys.Home)
                    index = 0;
                else if (key == Keys.End)
                    index = _imageGallery.Items.Count - 1;
            }

            if (index < 0)
                index = 0;
            else if (index > _imageGallery.Items.Count - 1)
                index = _imageGallery.Items.Count - 1;

            return index;
        }

        #endregion


        #region Scroll Timer
        /// <summary>
        /// Handles the Tick event of the scrollTimer control.
        /// </summary>
        private void ScrollTimer_Tick(object? sender, EventArgs e)
        {
            var delta = (int)scrollTimer.Tag;
            var location = _imageGallery.PointToClient(MousePosition);

            _imageGallery.OnMouseMove(new(MouseButtons, 0, location.X, location.Y, 0));
            _imageGallery.OnMouseWheel(new(MouseButtons.None, 0, location.X, location.Y, delta));
        }

        #endregion

    }

}
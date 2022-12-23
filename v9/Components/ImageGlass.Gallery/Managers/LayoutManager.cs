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


/// <summary>
/// Represents the layout of the image list view drawing area.
/// </summary>
internal class LayoutManager
{
    #region Member Variables
    private Rectangle mClientArea;
    private ImageGallery _imageGallery;
    private Rectangle mItemAreaBounds;
    private Size mItemSize;
    private Size mItemSizeWithMargin;
    private int mDisplayedCols;
    private int mDisplayedRows;
    private int mItemCols;
    private int mItemRows;
    private int mFirstPartiallyVisible;
    private int mLastPartiallyVisible;
    private int mFirstVisible;
    private int mLastVisible;

    private View cachedView;
    private Point cachedViewOffset;
    private Size cachedSize;
    private int cachedItemCount;
    private Size cachedItemSize;
    private bool cachedIntegralScroll;
    private Size cachedItemMargin;
    private bool cachedScrollBars;
    private readonly Dictionary<Guid, bool> cachedVisibleItems = new();

    private bool vScrollVisible = false;
    private bool hScrollVisible = false;

    // Size required to display all items (i.e. scroll range)
    private int totalWidth;
    private int totalHeight;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the bounds of the entire client area.
    /// </summary>
    public Rectangle ClientArea => mClientArea;

    /// <summary>
    /// Gets the owner image list view.
    /// </summary>
    public ImageGallery ImageGalleryOwner => _imageGallery;

    /// <summary>
    /// Gets the extends of the item area.
    /// </summary>
    public Rectangle ItemAreaBounds => mItemAreaBounds;

    /// <summary>
    /// Gets the items size.
    /// </summary>
    public Size ItemSize => mItemSize;

    /// <summary>
    /// Gets the items size including the margin around the item.
    /// </summary>
    public Size ItemSizeWithMargin => mItemSizeWithMargin;

    /// <summary>
    /// Gets the maximum number of columns that can be displayed.
    /// </summary>
    public int Cols => mDisplayedCols;

    /// <summary>
    /// Gets the maximum number of rows that can be displayed.
    /// </summary>
    public int Rows => mDisplayedRows;

    /// <summary>
    /// Gets the index of the first partially visible item.
    /// </summary>
    public int FirstPartiallyVisible => mFirstPartiallyVisible;

    /// <summary>
    /// Gets the index of the last partially visible item.
    /// </summary>
    public int LastPartiallyVisible => mLastPartiallyVisible;

    /// <summary>
    /// Gets the index of the first fully visible item.
    /// </summary>
    public int FirstVisible => mFirstVisible;

    /// <summary>
    /// Gets the index of the last fully visible item.
    /// </summary>
    public int LastVisible => mLastVisible;

    /// <summary>
    /// Determines whether an update is required.
    /// </summary>
    public bool UpdateRequired
    {
        get
        {
            if (_imageGallery.View != cachedView)
                return true;
            else if (_imageGallery.ViewOffset != cachedViewOffset)
                return true;
            else if (_imageGallery.ClientSize != cachedSize)
                return true;
            else if (_imageGallery.Items.Count != cachedItemCount)
                return true;
            else if (_imageGallery.mRenderer.MeasureItem(_imageGallery.View) != cachedItemSize)
                return true;
            else if (_imageGallery.mRenderer.MeasureItemMargin(_imageGallery.View) != cachedItemMargin)
                return true;
            else if (_imageGallery.ScrollBars != cachedScrollBars)
                return true;
            else if (_imageGallery.IntegralScroll != cachedIntegralScroll)
                return true;
            else if (_imageGallery.Items.collectionModified)
                return true;
            else
                return false;
        }
    }

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="owner">The owner control.</param>
    public LayoutManager(ImageGallery owner)
    {
        _imageGallery = owner;

        Update();
    }

    #endregion


    #region Instance Methods
    /// <summary>
    /// Determines whether the item with the given guid is
    /// (partially) visible.
    /// </summary>
    /// <param name="guid">The guid of the item to check.</param>
    public bool IsItemVisible(Guid guid)
    {
        return cachedVisibleItems.ContainsKey(guid);
    }

    /// <summary>
    /// Determines whether the item with the given guid is partially visible.
    /// </summary>
    /// <param name="index">The index of the item to check.</param>
    /// <returns></returns>
    public bool IsItemPartialyVisible(int index)
    {
        return index == FirstPartiallyVisible || index == LastPartiallyVisible;
    }

    /// <summary>
    /// Returns the bounds of the item with the specified index.
    /// </summary>
    public Rectangle GetItemBounds(int itemIndex)
    {
        var location = mItemAreaBounds.Location;
        location.X += cachedItemMargin.Width / 2 - _imageGallery.ViewOffset.X;
        location.Y += cachedItemMargin.Height / 2 - _imageGallery.ViewOffset.Y;

        if (ImageGalleryOwner.View == View.HorizontalStrip)
        {
            var startGap = 0;

            // center the items
            if (ImageGalleryOwner.Items.Count <= mDisplayedCols)
            {
                var currentItemsWidth = mItemSizeWithMargin.Width * ImageGalleryOwner.Items.Count;
                startGap = mItemAreaBounds.Width / 2 - currentItemsWidth / 2;
            }

            location.X += (itemIndex * mItemSizeWithMargin.Width) + startGap;
        }
        else
        {
            location.X += (itemIndex % mDisplayedCols) * mItemSizeWithMargin.Width;
            location.Y += (itemIndex / mDisplayedCols) * mItemSizeWithMargin.Height;
        }

        return new Rectangle(location, mItemSize);
    }

    /// <summary>
    /// Returns the bounds of the item with the specified index, 
    /// including the margin around the item.
    /// </summary>
    public Rectangle GetItemBoundsWithMargin(int itemIndex)
    {
        Rectangle rec = GetItemBounds(itemIndex);
        rec.Inflate(cachedItemMargin.Width / 2, cachedItemMargin.Height / 2);
        return rec;
    }

    /// <summary>
    /// Returns the item checkbox bounds.
    /// This method assumes a checkbox icon size of 16x16
    /// </summary>
    public Rectangle GetCheckBoxBounds(int itemIndex)
    {
        var bounds = GetWidgetBounds(GetItemBounds(itemIndex), new Size(16, 16),
            _imageGallery.CheckBoxPadding, _imageGallery.CheckBoxAlignment);

        // If the checkbox and the icon have the same alignment,
        // move the checkbox horizontally away from the icon
        if (_imageGallery.CheckBoxAlignment == _imageGallery.IconAlignment
            && _imageGallery.ShowCheckBoxes
            && _imageGallery.ShowFileIcons)
        {
            var alignment = _imageGallery.CheckBoxAlignment;
            if (alignment == ContentAlignment.BottomCenter
                || alignment == ContentAlignment.MiddleCenter
                || alignment == ContentAlignment.TopCenter)
            {
                bounds.X -= 8 + _imageGallery.IconPadding.Width / 2;
            }
            else if (alignment == ContentAlignment.BottomRight
                || alignment == ContentAlignment.MiddleRight
                || alignment == ContentAlignment.TopRight)
            {
                bounds.X -= 16 + _imageGallery.IconPadding.Width;
            }
        }

        return bounds;
    }

    /// <summary>
    /// Returns the item icon bounds.
    /// This method assumes an icon size of 16x16
    /// </summary>
    public Rectangle GetIconBounds(int itemIndex)
    {
        var bounds = GetWidgetBounds(GetItemBounds(itemIndex), new Size(16, 16),
            _imageGallery.IconPadding, _imageGallery.IconAlignment);

        // If the checkbox and the icon have the same alignment,
        // or in details view move the icon horizontally away from the checkbox
        if (_imageGallery.ShowCheckBoxes && _imageGallery.ShowFileIcons)
        {
            bounds.X += 16 + 2;
        }
        else if (_imageGallery.CheckBoxAlignment == _imageGallery.IconAlignment
            && _imageGallery.ShowCheckBoxes
            && _imageGallery.ShowFileIcons)
        {
            var alignment = _imageGallery.CheckBoxAlignment;
            if (alignment == ContentAlignment.BottomLeft
                || alignment == ContentAlignment.MiddleLeft
                || alignment == ContentAlignment.TopLeft)
            {
                bounds.X += 16 + _imageGallery.IconPadding.Width;
            }
            else if (alignment == ContentAlignment.BottomCenter
                || alignment == ContentAlignment.MiddleCenter
                || alignment == ContentAlignment.TopCenter)
            {
                bounds.X += 8 + _imageGallery.IconPadding.Width / 2;
            }
        }

        return bounds;
    }

    /// <summary>
    /// Returns the bounds of a widget.
    /// Used to calculate the bounds of checkboxes and icons.
    /// </summary>
    private Rectangle GetWidgetBounds(Rectangle bounds, Size size, Size padding, ContentAlignment alignment)
    {
        // Apply padding
        bounds.Inflate(-padding.Width, -padding.Height);

        int x;
        if (alignment == ContentAlignment.BottomLeft
            || alignment == ContentAlignment.MiddleLeft
            || alignment == ContentAlignment.TopLeft)
        {
            x = bounds.Left;
        }
        else if (alignment == ContentAlignment.BottomCenter
            || alignment == ContentAlignment.MiddleCenter
            || alignment == ContentAlignment.TopCenter)
        {
            x = bounds.Left + bounds.Width / 2 - size.Width / 2;
        }
        else // if (alignment == ContentAlignment.BottomRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.TopRight)
        {
            x = bounds.Right - size.Width;
        }

        int y;
        if (alignment == ContentAlignment.BottomLeft
            || alignment == ContentAlignment.BottomCenter
            || alignment == ContentAlignment.BottomRight)
        {
            y = bounds.Bottom - size.Height;
        }
        else if (alignment == ContentAlignment.MiddleLeft
            || alignment == ContentAlignment.MiddleCenter
            || alignment == ContentAlignment.MiddleRight)
        {
            y = bounds.Top + bounds.Height / 2 - size.Height / 2;
        }
        else // if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopRight)
        {
            y = bounds.Top;
        }

        return new Rectangle(x, y, size.Width, size.Height);
    }

    /// <summary>
    /// Recalculates the control layout.
    /// </summary>
    public void Update()
    {
        Update(false);
    }

    /// <summary>
    /// Recalculates the control layout.
    /// <param name="forceUpdate">true to force an update; otherwise false.</param>
    /// </summary>
    public void Update(bool forceUpdate)
    {
        if (_imageGallery.ClientRectangle.Width == 0
            || _imageGallery.ClientRectangle.Height == 0)
            return;

        // If only item order is changed, just update visible items.
        if (!forceUpdate && !UpdateRequired && _imageGallery.Items.collectionModified)
        {
            UpdateVisibleItems();
            return;
        }

        if (!forceUpdate && !UpdateRequired)
            return;

        // Get the item size from the renderer
        mItemSize = _imageGallery.mRenderer.MeasureItem(_imageGallery.View);
        cachedItemMargin = _imageGallery.mRenderer.MeasureItemMargin(_imageGallery.View).ToSize();
        mItemSizeWithMargin = mItemSize + cachedItemMargin;

        // Cache current properties to determine if we will need an update later
        var viewChanged = cachedView != _imageGallery.View;
        cachedView = _imageGallery.View;
        cachedViewOffset = _imageGallery.ViewOffset;
        cachedSize = _imageGallery.ClientSize;
        cachedItemCount = _imageGallery.Items.Count;
        cachedIntegralScroll = _imageGallery.IntegralScroll;
        cachedItemSize = mItemSize;
        cachedScrollBars = _imageGallery.ScrollBars;
        _imageGallery.Items.collectionModified = false;

        // Calculate item area bounds
        if (!UpdateItemArea())
            return;

        // Let the calculated bounds modified by the renderer
        var eLayout = new LayoutEventArgs(mItemAreaBounds);
        _imageGallery.mRenderer.OnLayout(eLayout);
        mItemAreaBounds = eLayout.ItemAreaBounds;
        if (mItemAreaBounds.Width <= 0 || mItemAreaBounds.Height <= 0)
            return;

        // Calculate the number of rows and columns
        CalculateGrid();

        // Check if we need the scroll bars.
        // Recalculate the layout if scroll bar visibility changes.
        if (CheckScrollBars())
        {
            Update(true);
            return;
        }

        // Update scroll range
        UpdateScrollBars();

        // Cache visible items
        UpdateVisibleItems();

        // Recalculate the layout if view mode was changed
        if (viewChanged)
            Update();
    }

    /// <summary>
    /// Calculates the maximum number of rows and columns 
    /// that can be fully displayed.
    /// </summary>
    private void CalculateGrid()
    {
        // Number of rows and columns shown on screen
        mDisplayedRows = (int)Math.Floor(mItemAreaBounds.Height / (float)mItemSizeWithMargin.Height);
        mDisplayedCols = (int)
            Math.Floor(mItemAreaBounds.Width / (float)mItemSizeWithMargin.Width);

        if (_imageGallery.View == View.VerticalStrip) mDisplayedCols = 1;
        if (_imageGallery.View == View.HorizontalStrip) mDisplayedRows = 1;
        if (mDisplayedCols < 1) mDisplayedCols = 1;
        if (mDisplayedRows < 1) mDisplayedRows = 1;

        // Number of rows and columns to enclose all items
        if (_imageGallery.View == View.HorizontalStrip)
        {
            mItemRows = mDisplayedRows;
            mItemCols = (int)Math.Ceiling(_imageGallery.Items.Count / (float)mDisplayedRows);
        }
        else
        {
            mItemCols = mDisplayedCols;
            mItemRows = (int)Math.Ceiling(_imageGallery.Items.Count / (float)mDisplayedCols);
        }

        totalWidth = mItemCols * mItemSizeWithMargin.Width;
        totalHeight = mItemRows * mItemSizeWithMargin.Height;
    }

    /// <summary>
    /// Calculates the item area.
    /// </summary>
    /// <returns>true if the item area is not empty (both width and height
    /// greater than zero); otherwise false.</returns>
    private bool UpdateItemArea()
    {
        // Calculate drawing area
        mClientArea = _imageGallery.ClientRectangle;
        if (_imageGallery.BorderStyle != BorderStyle.None)
        {
            mClientArea.Inflate(-1, -1);
        }
        mItemAreaBounds = mClientArea;

        // Allocate space for scrollbars
        if (_imageGallery.hScrollBar.Visible)
        {
            mClientArea.Height -= _imageGallery.hScrollBar.Height;
            mItemAreaBounds.Height -= _imageGallery.hScrollBar.Height;
        }

        if (_imageGallery.vScrollBar.Visible)
        {
            mClientArea.Width -= _imageGallery.vScrollBar.Width;
            mItemAreaBounds.Width -= _imageGallery.vScrollBar.Width;
        }

        return mItemAreaBounds.Width > 0 && mItemAreaBounds.Height > 0;
    }

    /// <summary>
    /// Shows or hides the scroll bars.
    /// Returns true if the layout needs to be recalculated; otherwise false.
    /// </summary>
    /// <returns></returns>
    private bool CheckScrollBars()
    {
        // Horizontal scroll bar
        var hScrollRequired = false;
        var hScrollChanged = false;
        if (_imageGallery.ScrollBars)
        {
            hScrollRequired = (_imageGallery.Items.Count > 0) && (mItemAreaBounds.Width < totalWidth);
        }

        if (hScrollRequired != hScrollVisible)
        {
            hScrollVisible = hScrollRequired;
            _imageGallery.hScrollBar.Visible = hScrollRequired;
            hScrollChanged = true;
        }

        // Vertical scroll bar
        var vScrollRequired = false;
        var vScrollChanged = false;
        if (_imageGallery.ScrollBars)
        {
            vScrollRequired = (_imageGallery.Items.Count > 0) && (mItemAreaBounds.Height < totalHeight);
        }

        if (vScrollRequired != vScrollVisible)
        {
            vScrollVisible = vScrollRequired;
            _imageGallery.vScrollBar.Visible = vScrollRequired;
            vScrollChanged = true;
        }

        // Determine if the layout needs to be recalculated
        return hScrollChanged || vScrollChanged;
    }

    /// <summary>
    /// Updates scroll bar parameters.
    /// </summary>
    private void UpdateScrollBars()
    {
        // Set scroll range
        if (_imageGallery.Items.Count != 0)
        {
            // Horizontal scroll range
            if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                _imageGallery.hScrollBar.Minimum = 0;
                _imageGallery.hScrollBar.Maximum = Math.Max(0, totalWidth - 1);

                if (!_imageGallery.IntegralScroll)
                {
                    _imageGallery.hScrollBar.LargeChange = mItemAreaBounds.Width;
                }
                else
                {
                    _imageGallery.hScrollBar.LargeChange = mItemSizeWithMargin.Width * mDisplayedCols;
                }
                _imageGallery.hScrollBar.SmallChange = mItemSizeWithMargin.Width;
            }
            else
            {
                _imageGallery.hScrollBar.Minimum = 0;
                _imageGallery.hScrollBar.Maximum = mDisplayedCols * mItemSizeWithMargin.Width;
                _imageGallery.hScrollBar.LargeChange = mItemAreaBounds.Width;
                _imageGallery.hScrollBar.SmallChange = 1;
            }
            if (_imageGallery.ViewOffset.X > _imageGallery.hScrollBar.Maximum - _imageGallery.hScrollBar.LargeChange + 1)
            {
                _imageGallery.hScrollBar.Value = _imageGallery.hScrollBar.Maximum - _imageGallery.hScrollBar.LargeChange + 1;
                _imageGallery.ViewOffset = new Point(_imageGallery.hScrollBar.Value, _imageGallery.ViewOffset.Y);
            }

            // Vertical scroll range
            if (_imageGallery.ScrollOrientation == ScrollOrientation.HorizontalScroll)
            {
                _imageGallery.vScrollBar.Minimum = 0;
                _imageGallery.vScrollBar.Maximum = mDisplayedRows * mItemSizeWithMargin.Height;
                _imageGallery.vScrollBar.LargeChange = mItemAreaBounds.Height;
                _imageGallery.vScrollBar.SmallChange = 1;
            }
            else
            {
                _imageGallery.vScrollBar.Minimum = 0;
                _imageGallery.vScrollBar.Maximum = Math.Max(0, totalHeight - 1);
                if (!_imageGallery.IntegralScroll)
                {
                    _imageGallery.vScrollBar.LargeChange = mItemAreaBounds.Height;
                }
                else
                {
                    _imageGallery.vScrollBar.LargeChange = mItemSizeWithMargin.Height * mDisplayedRows;
                }
                _imageGallery.vScrollBar.SmallChange = mItemSizeWithMargin.Height;
            }
            if (_imageGallery.ViewOffset.Y > _imageGallery.vScrollBar.Maximum - _imageGallery.vScrollBar.LargeChange + 1)
            {
                _imageGallery.vScrollBar.Value = _imageGallery.vScrollBar.Maximum - _imageGallery.vScrollBar.LargeChange + 1;
                _imageGallery.ViewOffset = new Point(_imageGallery.ViewOffset.X, _imageGallery.vScrollBar.Value);
            }
        }
        else // if (mImageListView.Items.Count == 0)
        {
            // Zero out the scrollbars if we don't have any items
            _imageGallery.hScrollBar.Minimum = 0;
            _imageGallery.hScrollBar.Maximum = 0;
            _imageGallery.hScrollBar.Value = 0;
            _imageGallery.vScrollBar.Minimum = 0;
            _imageGallery.vScrollBar.Maximum = 0;
            _imageGallery.vScrollBar.Value = 0;
            _imageGallery.ViewOffset = new Point(0, 0);
        }

        var bounds = _imageGallery.ClientRectangle;
        if (_imageGallery.BorderStyle != BorderStyle.None)
            bounds.Inflate(-1, -1);

        // Horizontal scrollbar position
        _imageGallery.hScrollBar.Left = bounds.Left;
        _imageGallery.hScrollBar.Top = bounds.Bottom - _imageGallery.hScrollBar.Height;
        _imageGallery.hScrollBar.Width = bounds.Width - (_imageGallery.vScrollBar.Visible ? _imageGallery.vScrollBar.Width : 0);

        // Vertical scrollbar position
        _imageGallery.vScrollBar.Left = bounds.Right - _imageGallery.vScrollBar.Width;
        _imageGallery.vScrollBar.Top = bounds.Top;
        _imageGallery.vScrollBar.Height = bounds.Height - (_imageGallery.hScrollBar.Visible ? _imageGallery.hScrollBar.Height : 0);
    }

    /// <summary>
    /// Updates the dictionary of visible items.
    /// </summary>
    private void UpdateVisibleItems()
    {
        // Find the first and last visible items
        if (_imageGallery.View == View.HorizontalStrip)
        {
            mFirstPartiallyVisible = (int)Math.Floor(_imageGallery.ViewOffset.X / (float)mItemSizeWithMargin.Width) * mDisplayedRows;
            mLastPartiallyVisible = (int)Math.Ceiling((_imageGallery.ViewOffset.X + mItemAreaBounds.Width) / (float)mItemSizeWithMargin.Width) * mDisplayedRows - 1;
            mFirstVisible = (int)Math.Ceiling(_imageGallery.ViewOffset.X / (float)mItemSizeWithMargin.Width) * mDisplayedRows;
            mLastVisible = (int)Math.Floor((_imageGallery.ViewOffset.X + mItemAreaBounds.Width) / (float)mItemSizeWithMargin.Width) * mDisplayedRows - 1;
        }
        else
        {
            mFirstPartiallyVisible = (int)Math.Floor(_imageGallery.ViewOffset.Y / (float)mItemSizeWithMargin.Height) * mDisplayedCols;
            mLastPartiallyVisible = (int)Math.Ceiling((_imageGallery.ViewOffset.Y + mItemAreaBounds.Height) / (float)mItemSizeWithMargin.Height) * mDisplayedCols - 1;
            mFirstVisible = (int)Math.Ceiling(_imageGallery.ViewOffset.Y / (float)mItemSizeWithMargin.Height) * mDisplayedCols;
            mLastVisible = (int)Math.Floor((_imageGallery.ViewOffset.Y + mItemAreaBounds.Height) / (float)mItemSizeWithMargin.Height) * mDisplayedCols - 1;
        }

        // Bounds check
        if (mFirstPartiallyVisible < 0) mFirstPartiallyVisible = 0;
        if (mFirstPartiallyVisible > _imageGallery.Items.Count - 1) mFirstPartiallyVisible = _imageGallery.Items.Count - 1;
        if (mLastPartiallyVisible < 0) mLastPartiallyVisible = 0;
        if (mLastPartiallyVisible > _imageGallery.Items.Count - 1) mLastPartiallyVisible = _imageGallery.Items.Count - 1;
        if (mFirstVisible < 0) mFirstVisible = 0;
        if (mFirstVisible > _imageGallery.Items.Count - 1) mFirstVisible = _imageGallery.Items.Count - 1;
        if (mLastVisible < 0) mLastVisible = 0;
        if (mLastVisible > _imageGallery.Items.Count - 1) mLastVisible = _imageGallery.Items.Count - 1;

        // Cache visible items
        cachedVisibleItems.Clear();

        if (mFirstPartiallyVisible >= 0
            && mLastPartiallyVisible >= 0
            && mFirstPartiallyVisible <= _imageGallery.Items.Count - 1
            && mLastPartiallyVisible <= _imageGallery.Items.Count - 1)
        {
            for (int i = mFirstPartiallyVisible; i <= mLastPartiallyVisible; i++)
            {
                cachedVisibleItems.Add(_imageGallery.Items[i].Guid, false);
            }
        }

        // Current item state processed
        _imageGallery.Items.collectionModified = false;
    }

    #endregion
}


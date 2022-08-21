/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
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
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents an overridable class for image list view renderers.
/// </summary>
public class StyleRenderer : IDisposable
{
    #region Constants
    /// <summary>
    /// Represents the time in milliseconds after which the control deems to be needing a refresh.
    /// </summary>
    internal const int LazyRefreshInterval = 100;
    #endregion


    #region Member Variables
    private BufferedGraphics? bufferGraphics;
    private bool disposed;
    private bool creatingGraphics;
    private DateTime lastRenderTime;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the <see cref="ImageGallery"/> owning this item.
    /// </summary>
    public ImageGallery ImageGalleryOwner { get; internal set; }

    /// <summary>
    /// Gets or sets whether the graphics is clipped to the bounds of 
    /// drawing elements.
    /// </summary>
    public bool Clip { get; set; }

    /// <summary>
    /// Gets or sets the order by which items are drawn.
    /// </summary>
    public ItemDrawOrder ItemDrawOrder { get; set; }

    /// <summary>
    /// Gets or sets whether items are drawn before of after headers and the gallery images.
    /// </summary>
    public bool ItemsDrawnFirst { get; set; }

    /// <summary>
    /// Gets the rectangle bounding the client area of the control without the scroll bars.
    /// </summary>
    public Rectangle ClientBounds { get { return ImageGalleryOwner.layoutManager.ClientArea; } }

    /// <summary>
    /// Gets the rectangle bounding the item display area.
    /// </summary>
    public Rectangle ItemAreaBounds { get { return ImageGalleryOwner.layoutManager.ItemAreaBounds; } }

    /// <summary>
    /// Gets a value indicating whether this renderer can apply custom colors.
    /// </summary>
    public virtual bool CanApplyColors { get { return true; } }

    /// <summary>
    /// Gets whether the lazy refresh interval is exceeded.
    /// </summary>
    internal bool LazyRefreshIntervalExceeded { get { return ((int)(DateTime.Now - lastRenderTime).TotalMilliseconds > LazyRefreshInterval); } }

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="StyleRenderer"/> class.
    /// </summary>
    public StyleRenderer()
    {
        creatingGraphics = false;
        disposed = false;
        Clip = true;
        ItemsDrawnFirst = false;
        ItemDrawOrder = ItemDrawOrder.ItemIndex;
        lastRenderTime = DateTime.MinValue;
    }

    #endregion


    #region DrawItemParams
    /// <summary>
    /// Represents the paramaters required to draw an item.
    /// </summary>
    private struct DrawItemParams
    {
        public ImageGalleryItem Item;
        public ItemState State;
        public Rectangle Bounds;

        public DrawItemParams(ImageGalleryItem item, ItemState state, Rectangle bounds)
        {
            Item = item;
            State = state;
            Bounds = bounds;
        }

    }

    #endregion


    #region ItemDrawOrderComparer
    /// <summary>
    /// Compares items by the draw order.
    /// </summary>
    private class ItemDrawOrderComparer : IComparer<DrawItemParams>
    {
        private readonly ItemDrawOrder mDrawOrder;

        public ItemDrawOrderComparer(ItemDrawOrder drawOrder)
        {
            mDrawOrder = drawOrder;
        }

        /// <summary>
        /// Compares items by the draw order.
        /// </summary>
        /// <param name="param1">First item to compare.</param>
        /// <param name="param2">Second item to compare.</param>
        /// <returns>1 if the first item should be drawn first, 
        /// -1 if the second item should be drawn first,
        /// 0 if the two items can be drawn in any order.</returns>
        public int Compare(DrawItemParams param1, DrawItemParams param2)
        {
            if (param1.Equals(param2))
                return 0;
            if (ReferenceEquals(param1.Item, param2.Item))
                return 0;

            int comparison;

            if (mDrawOrder == ItemDrawOrder.ItemIndex)
            {
                return CompareByIndex(param1, param2);
            }
            else if (mDrawOrder == ItemDrawOrder.ZOrder)
            {
                return CompareByZOrder(param1, param2);
            }
            else if (mDrawOrder == ItemDrawOrder.NormalSelectedHovered)
            {
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
            }
            else if (mDrawOrder == ItemDrawOrder.NormalHoveredSelected)
            {
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
            }
            else if (mDrawOrder == ItemDrawOrder.SelectedNormalHovered)
            {
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
            }
            else if (mDrawOrder == ItemDrawOrder.SelectedHoveredNormal)
            {
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
            }
            else if (mDrawOrder == ItemDrawOrder.HoveredNormalSelected)
            {
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
            }
            else if (mDrawOrder == ItemDrawOrder.HoveredSelectedNormal)
            {
                comparison = -CompareByNormal(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareBySelected(param1, param2);
                if (comparison != 0) return comparison;
                comparison = -CompareByHovered(param1, param2);
                if (comparison != 0) return comparison;
            }

            // Compare by zorder
            comparison = CompareByZOrder(param1, param2);
            if (comparison != 0) return comparison;

            // Finally compare by index
            comparison = CompareByIndex(param1, param2);
            return comparison;
        }

        /// <summary>
        /// Compares items by their index property.
        /// </summary>
        private static int CompareByIndex(DrawItemParams param1, DrawItemParams param2)
        {
            if (param1.Item.Index < param2.Item.Index)
                return -1;
            else if (param1.Item.Index > param2.Item.Index)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Compares items by their zorder property.
        /// </summary>
        private static int CompareByZOrder(DrawItemParams param1, DrawItemParams param2)
        {
            if (param1.Item.ZOrder < param2.Item.ZOrder)
                return -1;
            else if (param1.Item.ZOrder > param2.Item.ZOrder)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Compares items by their neutral state.
        /// </summary>
        private static int CompareByNormal(DrawItemParams param1, DrawItemParams param2)
        {
            if (param1.State == ItemState.None && param2.State != ItemState.None)
                return -1;
            else if (param1.State != ItemState.None && param2.State == ItemState.None)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Compares items by their selected state.
        /// </summary>
        private static int CompareBySelected(DrawItemParams param1, DrawItemParams param2)
        {
            if ((param1.State & ItemState.Selected) == ItemState.Selected &&
                (param2.State & ItemState.Selected) != ItemState.Selected)
                return -1;
            else if ((param1.State & ItemState.Selected) != ItemState.Selected &&
                (param2.State & ItemState.Selected) == ItemState.Selected)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Compares items by their hovered state.
        /// </summary>
        private static int CompareByHovered(DrawItemParams param1, DrawItemParams param2)
        {
            if ((param1.State & ItemState.Hovered) == ItemState.Hovered)
                return -1;
            else if ((param2.State & ItemState.Hovered) == ItemState.Hovered)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Compares items by their focused state.
        /// </summary>
        private int CompareByFocused(DrawItemParams param1, DrawItemParams param2)
        {
            if ((param1.State & ItemState.Focused) == ItemState.Focused)
                return -1;
            else if ((param2.State & ItemState.Focused) == ItemState.Focused)
                return 1;
            else
                return 0;
        }
    }
    #endregion


    #region Instance Methods
    /// <summary>
    /// Reads and returns the image for the given item.
    /// </summary>
    /// <param name="item">The item to read.</param>
    /// <param name="size">The size of the requested image..</param>
    /// <returns>Item thumbnail of requested size.</returns>
    public Image? GetImageAsync(ImageGalleryItem item, Size size)
    {
        var img = ImageGalleryOwner.thumbnailCache.GetRendererImage(item.Guid, size, ImageGalleryOwner.UseEmbeddedThumbnails,
            ImageGalleryOwner.AutoRotateThumbnails);

        if (img == null)
        {
            ImageGalleryOwner.thumbnailCache.AddToRendererCache(item.Guid, item.mAdaptor, item.VirtualItemKey,
                size, ImageGalleryOwner.UseEmbeddedThumbnails, ImageGalleryOwner.AutoRotateThumbnails);
        }

        return img;
    }

    #endregion


    #region Internal Methods
    /// <summary>
    /// Renders the border of the control.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderBorder(Graphics g)
    {
        // Background
        g.ResetClip();
        DrawBorder(g, new Rectangle(0, 0, ImageGalleryOwner.Width, ImageGalleryOwner.Height));
    }

    /// <summary>
    /// Renders the background of the control.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderBackground(Graphics g)
    {
        // Background
        g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);
        DrawBackground(g, ImageGalleryOwner.layoutManager.ClientArea);
    }

    /// <summary>
    /// Renders the items.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderItems(Graphics g)
    {
        // Is the control empty?
        if (ImageGalleryOwner.Items.Count == 0)
            return;

        // No items visible?
        if (ImageGalleryOwner.layoutManager.FirstPartiallyVisible == -1 ||
            ImageGalleryOwner.layoutManager.LastPartiallyVisible == -1)
            return;

        var drawItemParams = new List<DrawItemParams>();
        for (int i = ImageGalleryOwner.layoutManager.FirstPartiallyVisible; i <= ImageGalleryOwner.layoutManager.LastPartiallyVisible; i++)
        {
            var item = ImageGalleryOwner.Items[i];

            // Determine item state
            var state = ItemState.None;
            var highlightState = ImageGalleryOwner.navigationManager.HighlightState(item);

            if (highlightState == ItemHighlightState.HighlightedAndSelected ||
                (highlightState == ItemHighlightState.NotHighlighted && item.Selected))
                state |= ItemState.Selected;

            if (ReferenceEquals(ImageGalleryOwner.navigationManager.HoveredItem, item) &&
                ImageGalleryOwner.navigationManager.MouseSelecting == false)
                state |= ItemState.Hovered;

            if (item.Pressed)
                state |= ItemState.Pressed;

            if (item.Focused)
                state |= ItemState.Focused;

            if (!item.Enabled)
                state |= ItemState.Disabled;

            // Get item bounds
            var bounds = ImageGalleryOwner.layoutManager.GetItemBounds(i);

            // Add to params to be sorted and drawn
            drawItemParams.Add(new DrawItemParams(item, state, bounds));
        }

        // Sort items by draw order
        drawItemParams.Sort(new ItemDrawOrderComparer(ItemDrawOrder));

        // Draw items
        foreach (var param in drawItemParams)
        {
            if (Clip)
            {
                Rectangle clip = Rectangle.Intersect(param.Bounds, ImageGalleryOwner.layoutManager.ItemAreaBounds);
                g.SetClip(clip);
            }
            else
                g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);

            // Draw the item
            DrawItem(g, param.Item, param.State, param.Bounds);


            // Draw the checkbox and file icon
            if (ImageGalleryOwner.ShowCheckBoxes)
            {
                var cBounds = ImageGalleryOwner.layoutManager.GetCheckBoxBounds(param.Item.Index);
                if (Clip)
                {
                    var clip = Rectangle.Intersect(cBounds, ImageGalleryOwner.layoutManager.ItemAreaBounds);
                    g.SetClip(clip);
                }
                else
                    g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);

                DrawCheckBox(g, param.Item, cBounds);
            }

            if (ImageGalleryOwner.ShowFileIcons)
            {
                var cBounds = ImageGalleryOwner.layoutManager.GetIconBounds(param.Item.Index);
                if (Clip)
                {
                    var clip = Rectangle.Intersect(cBounds, ImageGalleryOwner.layoutManager.ItemAreaBounds);
                    g.SetClip(clip);
                }
                else
                    g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);

                DrawFileIcon(g, param.Item, cBounds);
            }
        }
    }

    /// <summary>
    /// Renders the overlay.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderOverlay(Graphics g)
    {
        g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);
        DrawOverlay(g, ImageGalleryOwner.layoutManager.ClientArea);
    }

    /// <summary>
    /// Renders the drag-drop insertion caret.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderInsertionCaret(Graphics g)
    {
        if (ImageGalleryOwner.navigationManager.DropTarget == null)
            return;

        var bounds = ImageGalleryOwner.layoutManager.GetItemBounds(ImageGalleryOwner.navigationManager.DropTarget.Index);
        if (ImageGalleryOwner.View == View.VerticalStrip)
        {
            if (ImageGalleryOwner.navigationManager.DropToRight)
                bounds.Offset(0, ImageGalleryOwner.layoutManager.ItemSizeWithMargin.Height);

            var itemMargin = MeasureItemMargin(ImageGalleryOwner.View);
            bounds.Offset(0, -(itemMargin.Height - 2) / 2 - 2);
            bounds.Height = 2;
        }
        else
        {
            if (ImageGalleryOwner.navigationManager.DropToRight)
                bounds.Offset(ImageGalleryOwner.layoutManager.ItemSizeWithMargin.Width, 0);

            var itemMargin = MeasureItemMargin(ImageGalleryOwner.View);
            bounds.Offset(-(itemMargin.Width - 2) / 2 - 2, 0);
            bounds.Width = 2;
        }

        if (Clip)
            g.SetClip(bounds);
        else
            g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);

        DrawInsertionCaret(g, bounds);
    }

    /// <summary>
    /// Renders the selection rectangle.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    private void RenderSelectionRectangle(Graphics g)
    {
        if (!ImageGalleryOwner.navigationManager.MouseSelecting)
            return;

        var sel = ImageGalleryOwner.navigationManager.SelectionRectangle;
        if (sel.Height > 0 && sel.Width > 0)
        {
            g.SetClip(ImageGalleryOwner.layoutManager.ClientArea);
            if (Clip)
            {
                var selclip = new Rectangle(sel.Left, sel.Top, sel.Width + 1, sel.Height + 1);
                g.IntersectClip(selclip);
            }

            DrawSelectionRectangle(g, sel);
        }
    }


    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="graphics">The graphics to draw on.</param>
    internal void Render(Graphics graphics)
    {
        if (disposed) return;

        if (bufferGraphics == null)
        {
            if (!RecreateBuffer(graphics)) return;
        }

        // Save the timne of this render for lazy refreshes
        lastRenderTime = DateTime.Now;

        // Update the layout
        ImageGalleryOwner.layoutManager.Update();

        // Set drawing area
        var g = bufferGraphics.Graphics;
        g.ResetClip();

        // Draw control border
        RenderBorder(g);

        // Draw background
        RenderBackground(g);


        // Draw items if they should be drawn first
        bool itemsDrawn = false;
        if (ItemsDrawnFirst)
        {
            RenderItems(g);
            itemsDrawn = true;
        }

        // Draw items if they should be drawn last.
        if (!itemsDrawn)
            RenderItems(g);

        // Draw the overlay image
        RenderOverlay(g);

        // Draw the selection rectangle
        RenderSelectionRectangle(g);

        // Draw the insertion caret
        RenderInsertionCaret(g);

        // Scrollbar filler
        RenderScrollbarFiller(g);

        // Draw on to the control
        bufferGraphics.Render(graphics);
    }
    
    /// <summary>
    /// Loads and returns the large gallery image for the given item.
    /// </summary>
    private Image GetGalleryImageAsync(ImageGalleryItem item, Size size)
    {
        var img = ImageGalleryOwner.thumbnailCache.GetGalleryImage(item.Guid, size, ImageGalleryOwner.UseEmbeddedThumbnails,
            ImageGalleryOwner.AutoRotateThumbnails);

        if (img == null)
        {
            ImageGalleryOwner.thumbnailCache.AddToGalleryCache(item.Guid, item.mAdaptor, item.VirtualItemKey,
                size, ImageGalleryOwner.UseEmbeddedThumbnails, ImageGalleryOwner.AutoRotateThumbnails);
        }

        return img;
    }

    /// <summary>
    /// Clears the graphics buffer objects.
    /// </summary>
    internal void ClearBuffer()
    {
        if (bufferGraphics != null)
            bufferGraphics.Dispose();
        bufferGraphics = null;
    }

    /// <summary>
    /// Destroys the current buffer and creates a new buffered graphics 
    /// sized to the client area of the owner control.
    /// </summary>
    /// <param name="graphics">The Graphics to match the pixel format to.</param>
    internal bool RecreateBuffer(Graphics graphics)
    {
        if (creatingGraphics) return false;

        creatingGraphics = true;

        var bufferContext = BufferedGraphicsManager.Current;

        if (disposed)
            throw new ObjectDisposedException(nameof(bufferContext));

        var width = Math.Max(ImageGalleryOwner.Width, 1);
        var height = Math.Max(ImageGalleryOwner.Height, 1);

        bufferContext.MaximumBuffer = new Size(width, height);

        ClearBuffer();

        bufferGraphics = bufferContext.Allocate(graphics, new Rectangle(0, 0, width, height));

        creatingGraphics = false;

        InitializeGraphics(bufferGraphics.Graphics);

        return true;
    }

    /// <summary>
    /// Releases buffered graphics objects.
    /// </summary>
    void IDisposable.Dispose()
    {
        if (!disposed)
        {
            ClearBuffer();

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }

#if DEBUG
    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="StyleRenderer"/> is reclaimed by garbage collection.
    /// </summary>
    ~StyleRenderer()
    {
        System.Diagnostics.Debug.Print("Finalizer of {0} called.", GetType());
        Dispose();
    }

#endif

    #endregion


    #region Virtual Methods

    /// <summary>
    /// Initializes the System.Drawing.Graphics used to draw
    /// control elements.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    public virtual void InitializeGraphics(Graphics g)
    {
        g.PixelOffsetMode = PixelOffsetMode.None;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }

    /// <summary>
    /// Renders the area between scrollbars.
    /// </summary>
    /// <param name="g">The graphics to draw on.</param>
    public virtual void RenderScrollbarFiller(Graphics g)
    {
        if (!ImageGalleryOwner.hScrollBar.Visible || !ImageGalleryOwner.vScrollBar.Visible)
            return;

        var bounds = ImageGalleryOwner.layoutManager.ClientArea;
        var filler = new Rectangle(bounds.Right, bounds.Bottom, ImageGalleryOwner.vScrollBar.Width, ImageGalleryOwner.hScrollBar.Height);

        g.SetClip(filler);
        g.FillRectangle(SystemBrushes.Control, filler);
    }

    /// <summary>
    /// Returns the spacing between items for the given view mode.
    /// </summary>
    /// <param name="view">The view mode for which the measurement should be made.</param>
    /// <returns>The spacing between items.</returns>
    public virtual Size MeasureItemMargin(View view)
    {
        return new Size(4, 4);
    }

    /// <summary>
    /// Returns item size for the given view mode.
    /// </summary>
    /// <param name="view">The view mode for which the measurement should be made.</param>
    /// <returns>The item size.</returns>
    public virtual Size MeasureItem(View view)
    {
        // Reference text height
        int textHeight = ImageGalleryOwner.Font.Height;

        var itemPadding = new Size(4, 4);
        var itemSize = ImageGalleryOwner.ThumbnailSize + itemPadding + itemPadding;
        itemSize.Height += textHeight + Math.Max(4, textHeight / 3); // textHeight / 3 = vertical space between thumbnail and text

        return itemSize;
    }

    /// <summary>
    /// Draws the border of the control.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="bounds">The coordinates of the border.</param>
    public virtual void DrawBorder(Graphics g, Rectangle bounds)
    {
        if (ImageGalleryOwner.BorderStyle != BorderStyle.None)
        {
            var style = (ImageGalleryOwner.BorderStyle == BorderStyle.FixedSingle) ? Border3DStyle.Flat : Border3DStyle.SunkenInner;

            ControlPaint.DrawBorder3D(g, bounds, style);
        }
    }

    /// <summary>
    /// Draws the background of the control.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="bounds">The client coordinates of the item area.</param>
    public virtual void DrawBackground(Graphics g, Rectangle bounds)
    {
        // Clear the background
        g.Clear(ImageGalleryOwner.BackColor);

        // Draw the background image
        if (ImageGalleryOwner.BackgroundImage != null)
        {
            var img = ImageGalleryOwner.BackgroundImage;

            if (ImageGalleryOwner.BackgroundImageLayout == ImageLayout.None)
            {
                g.DrawImageUnscaled(img, ImageGalleryOwner.layoutManager.ItemAreaBounds.Location);
            }
            else if (ImageGalleryOwner.BackgroundImageLayout == ImageLayout.Center)
            {
                var x = bounds.Left + (bounds.Width - img.Width) / 2;
                var y = bounds.Top + (bounds.Height - img.Height) / 2;
                g.DrawImageUnscaled(img, x, y);
            }
            else if (ImageGalleryOwner.BackgroundImageLayout == ImageLayout.Stretch)
            {
                g.DrawImage(img, bounds);
            }
            else if (ImageGalleryOwner.BackgroundImageLayout == ImageLayout.Tile)
            {
                using var imgBrush = new TextureBrush(img, WrapMode.Tile);
                g.FillRectangle(imgBrush, bounds);
            }
            else if (ImageGalleryOwner.BackgroundImageLayout == ImageLayout.Zoom)
            {
                var xscale = bounds.Width / (float)img.Width;
                var yscale = bounds.Height / (float)img.Height;
                var scale = Math.Min(xscale, yscale);

                var width = (int)(img.Width * scale);
                var height = (int)(img.Height * scale);
                var x = bounds.Left + (bounds.Width - width) / 2;
                var y = bounds.Top + (bounds.Height - height) / 2;

                g.DrawImage(img, x, y, width, height);
            }
        }
    }

    /// <summary>
    /// Draws the selection rectangle.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="selection">The client coordinates of the selection rectangle.</param>
    public virtual void DrawSelectionRectangle(Graphics g, Rectangle selection)
    {
        using var brush = new SolidBrush(Color.FromArgb(170, SystemColors.Highlight));
        using var pen = new Pen(SystemColors.Highlight);

        g.FillRectangle(brush, selection);
        g.DrawRectangle(pen, selection);
    }

    /// <summary>
    /// Draws the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The ImageListViewItem to draw.</param>
    /// <param name="state">The current view state of item.</param>
    /// <param name="bounds">The bounding rectangle of item in client coordinates.</param>
    public virtual void DrawItem(Graphics g, ImageGalleryItem item, ItemState state, Rectangle bounds)
    {
        var itemPadding = new Size(4, 4);
        var alternate = item.Index % 2 == 1;

        // Paint background Disabled
        if ((state & ItemState.Disabled) != ItemState.None)
        {
            //using var bDisabled = new LinearGradientBrush(bounds, ImageListView.Colors.DisabledColor1, ImageListView.Colors.DisabledColor2, LinearGradientMode.Vertical);
            //Utility.FillRoundedRectangle(g, bDisabled, bounds, 4);
        }

        // Paint background Selected
        else if ((ImageGalleryOwner.Focused && ((state & ItemState.Selected) != ItemState.None)) ||
            (!ImageGalleryOwner.Focused && ((state & ItemState.Selected) != ItemState.None) && ((state & ItemState.Hovered) != ItemState.None)))
        {
            //using var bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical);
            //Utility.FillRoundedRectangle(g, bSelected, bounds, 4);
        }

        // Paint background unfocused
        else if (!ImageGalleryOwner.Focused && ((state & ItemState.Selected) != ItemState.None))
        {
            //using var bGray64 = new LinearGradientBrush(bounds, ImageListView.Colors.UnFocusedColor1, ImageListView.Colors.UnFocusedColor2, LinearGradientMode.Vertical);
            //Utility.FillRoundedRectangle(g, bGray64, bounds, 4);
        }

        // Paint background Pressed
        if ((state & ItemState.Pressed) != ItemState.None)
        {
            //using var bHovered = new LinearGradientBrush(bounds, ImageListView.Colors.PressedColor2, ImageListView.Colors.PressedColor2, LinearGradientMode.Vertical);
            //Utility.FillRoundedRectangle(g, bHovered, bounds, 4);
        }
        // Paint background Hover
        else if ((state & ItemState.Hovered) != ItemState.None)
        {
            //using var bHovered = new LinearGradientBrush(bounds, ImageListView.Colors.HoverColor1, ImageListView.Colors.HoverColor2, LinearGradientMode.Vertical);
            //Utility.FillRoundedRectangle(g, bHovered, bounds, 4);
        }


        // Draw the image
        var img = item.GetCachedImage(CachedImageType.Thumbnail);
        if (img != null)
        {
            var pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageGalleryOwner.ThumbnailSize));
            g.DrawImage(img, pos);

            // Draw image border
            if (Math.Min(pos.Width, pos.Height) > 32)
            {
                //using var pOuterBorder = new Pen(ImageListView.Colors.ImageOuterBorderColor);
                //g.DrawRectangle(pOuterBorder, pos);

                //if (Math.Min(ImageListView.ThumbnailSize.Width, ImageListView.ThumbnailSize.Height) > 32)
                //{
                //    using var pInnerBorder = new Pen(ImageListView.Colors.ImageInnerBorderColor);
                //    g.DrawRectangle(pInnerBorder, Rectangle.Inflate(pos, -1, -1));
                //}
            }
        }

        //// Draw item text
        //var foreColor = ImageListView.Colors.ForeColor;
        //if ((state & ItemState.Disabled) != ItemState.None)
        //{
        //    foreColor = ImageListView.Colors.DisabledForeColor;
        //}
        //else if ((state & ItemState.Pressed) != ItemState.None)
        //{
        //    foreColor = ImageListView.Colors.PressedForeColor;
        //}
        //else if ((state & ItemState.Selected) != ItemState.None)
        //{
        //    if (ImageListView.Focused)
        //        foreColor = ImageListView.Colors.SelectedForeColor;
        //    else
        //        foreColor = ImageListView.Colors.UnFocusedForeColor;
        //}

        //var szt = TextRenderer.MeasureText(item.Text, ImageListView.Font);
        //var rt = new Rectangle(bounds.Left + itemPadding.Width, bounds.Top + 2 * itemPadding.Height + ImageListView.ThumbnailSize.Height, ImageListView.ThumbnailSize.Width, szt.Height);

        //TextRenderer.DrawText(g, item.Text, ImageListView.Font, rt, foreColor,
        //    TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);


        //// Item border
        //using var pWhite128 = new Pen(Color.FromArgb(128, ImageListView.Colors.ControlBackColor));
        //Utility.DrawRoundedRectangle(g, pWhite128, bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3, 4);

        //if ((state & ItemState.Disabled) != ItemState.None)
        //{
        //    using var pHighlight128 = new Pen(ImageListView.Colors.DisabledBorderColor);
        //    Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}
        //else if (ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
        //{
        //    using var pHighlight128 = new Pen(ImageListView.Colors.SelectedBorderColor);
        //    Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}
        //else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
        //{
        //    using var pGray128 = new Pen(ImageListView.Colors.UnFocusedBorderColor);
        //    Utility.DrawRoundedRectangle(g, pGray128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}
        //else if ((state & ItemState.Pressed) == ItemState.None)
        //{
        //    using var pGray64 = new Pen(ImageListView.Colors.PressedBorderColor);
        //    Utility.DrawRoundedRectangle(g, pGray64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}
        //else if ((state & ItemState.Selected) == ItemState.None)
        //{
        //    using var pGray64 = new Pen(ImageListView.Colors.BorderColor);
        //    Utility.DrawRoundedRectangle(g, pGray64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}

        //if (ImageListView.Focused && ((state & ItemState.Hovered) != ItemState.None))
        //{
        //    using var pHighlight64 = new Pen(ImageListView.Colors.HoverBorderColor);
        //    Utility.DrawRoundedRectangle(g, pHighlight64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
        //}

        // Focus rectangle
        if (ImageGalleryOwner.Focused && ((state & ItemState.Focused) != ItemState.None))
        {
            ControlPaint.DrawFocusRectangle(g, bounds);
        }
    }

    /// <summary>
    /// Draws the checkbox icon for the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The ImageListViewItem to draw.</param>
    /// <param name="bounds">The bounding rectangle of the checkbox in client coordinates.</param>
    public virtual void DrawCheckBox(Graphics g, ImageGalleryItem item, Rectangle bounds)
    {
        var size = CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.CheckedNormal);
        var pt = new PointF(bounds.X + (bounds.Width - (float)size.Width) / 2.0f,
            bounds.Y + (bounds.Height - (float)size.Height) / 2.0f);
        CheckBoxState state;

        if (item.Enabled)
            state = item.Checked ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
        else
            state = item.Checked ? CheckBoxState.CheckedDisabled : CheckBoxState.UncheckedDisabled;

        CheckBoxRenderer.DrawCheckBox(g, Point.Round(pt), state);
    }

    /// <summary>
    /// Draws the file icon for the specified item on the given graphics.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="item">The ImageListViewItem to draw.</param>
    /// <param name="bounds">The bounding rectangle of the file icon in client coordinates.</param>
    public virtual void DrawFileIcon(Graphics g, ImageGalleryItem item, Rectangle bounds)
    {
        var icon = item.GetCachedImage(CachedImageType.SmallIcon);
        if (icon != null)
        {
            var size = icon.Size;
            var ptf = new PointF(bounds.X + (bounds.Width - (float)size.Width) / 2.0f,
                bounds.Y + (bounds.Height - (float)size.Height) / 2.0f);
            var pt = Point.Round(ptf);

            g.DrawImage(icon, pt.X, pt.Y);
        }
    }


    /// <summary>
    /// Draws the insertion caret for drag and drop operations.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="bounds">The bounding rectangle of the insertion caret.</param>
    public virtual void DrawInsertionCaret(Graphics g, Rectangle bounds)
    {
        using var b = new SolidBrush(SystemColors.Highlight);
        g.FillRectangle(b, bounds);
    }

    /// <summary>
    /// Draws an overlay image over the client area.
    /// </summary>
    /// <param name="g">The System.Drawing.Graphics to draw on.</param>
    /// <param name="bounds">The bounding rectangle of the client area.</param>
    public virtual void DrawOverlay(Graphics g, Rectangle bounds)
    {

    }

    /// <summary>
    /// Releases managed resources.
    /// </summary>
    public virtual void Dispose()
    {
        ((IDisposable)this).Dispose();
    }

    /// <summary>
    /// Sets the layout of the control.
    /// </summary>
    /// <param name="e">A LayoutEventArgs that contains event data.</param>
    public virtual void OnLayout(LayoutEventArgs e)
    {

    }
    #endregion

}


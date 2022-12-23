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

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ImageGlass.Gallery;


#region Event Delegates
/// <summary>
/// Represents the method that will handle the CacheError event.
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A CacheErrorEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void CacheErrorEventHandler(object sender, CacheErrorEventArgs e);

/// <summary>
/// Represents the method that will handle the DropFiles event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A DropFileEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void DropFilesEventHandler(object sender, DropFileEventArgs e);

/// <summary>
/// Represents the method that will handle the DropItems event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A DropItemEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void DropItemsEventHandler(object sender, DropItemEventArgs e);

/// <summary>
/// Represents the method that will handle the DropComplete event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A DropCompleteEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void DropCompleteEventHandler(object sender, DropCompleteEventArgs e);

/// <summary>
/// Represents the method that will handle the ItemClick event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ItemClickEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

/// <summary>
/// Represents the method that will handle the ItemCheckBoxClick event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ItemEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ItemCheckBoxClickEventHandler(object sender, ItemEventArgs e);

/// <summary>
/// Represents the method that will handle the ItemHover event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ItemHoverEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ItemHoverEventHandler(object sender, ItemHoverEventArgs e);

/// <summary>
/// Represents the method that will handle the ItemDoubleClick event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">An ItemClickEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ItemDoubleClickEventHandler(object sender, ItemClickEventArgs e);

/// <summary>
/// Represents the method that will handle the ThumbnailCaching event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ThumbnailCachingEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ThumbnailCachingEventHandler(object sender, ThumbnailCachingEventArgs e);

/// <summary>
/// Represents the method that will handle the ThumbnailCached event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ThumbnailCachedEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ThumbnailCachedEventHandler(object sender, ThumbnailCachedEventArgs e);

/// <summary>
/// Represents the method that will handle the DetailsCaching event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">An ItemEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void DetailsCachingEventHandler(object sender, ItemEventArgs e);

/// <summary>
/// Represents the method that will handle the DetailsCached event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">An ItemEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void DetailsCachedEventHandler(object sender, ItemEventArgs e);

/// <summary>
/// Represents the method that will handle the ShellInfoCachingEventHandler event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ShellInfoCachingEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ShellInfoCachingEventHandler(object sender, ShellInfoCachingEventArgs e);

/// <summary>
/// Represents the method that will handle the ShellInfoCachedEventHandler event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ShellInfoCachedEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ShellInfoCachedEventHandler(object sender, ShellInfoCachedEventArgs e);

/// <summary>
/// Refreshes the owner control.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal delegate void RefreshDelegateInternal();

/// <summary>
/// Represents the method that will handle the ItemCollectionChanged event. 
/// </summary>
/// <param name="sender">The <see cref="ImageGallery"/> object that is the source of the event.</param>
/// <param name="e">A ItemCollectionChangedEventArgs that contains event data.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public delegate void ItemCollectionChangedEventHandler(object sender, ItemCollectionChangedEventArgs e);

#endregion


#region Event Arguments
/// <summary>
/// Represents the event arguments for errors during cache operations.
/// </summary>
[Serializable, ComVisible(true)]
public class CacheErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is associated with this error.
    /// This parameter can be null.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }
    /// <summary>
    /// Gets a value indicating which error occurred during an asynchronous operation.
    /// </summary>
    public Exception Error { get; private set; }
    /// <summary>
    /// Gets the thread raising the error.
    /// </summary>
    public CacheThread CacheThread { get; private set; }

    /// <summary>
    /// Initializes a new instance of the CacheErrorEventArgs class.
    /// </summary>
    /// <param name="item">The <see cref="ImageGalleryItem"/> that is associated with this error.</param>
    /// <param name="error">The error that occurred during an asynchronous operation.</param>
    /// <param name="cacheThread">The thread raising the error.</param>
    public CacheErrorEventArgs(ImageGalleryItem? item, Exception error, CacheThread cacheThread)
    {
        Item = item;
        Error = error;
        CacheThread = cacheThread;
    }
}

/// <summary>
/// Represents the event arguments for external drag drop events.
/// </summary>
[Serializable, ComVisible(true)]
public class DropFileEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets whether default event code will be processed.
    /// When set to true, the control will automatically insert the new items.
    /// Otherwise, the control will not process the dropped files.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the position of the insertion caret.
    /// This determines where the new items will be inserted.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Gets the array of filenames droppped on the control.
    /// </summary>
    public string[] FileNames { get; private set; }


    /// <summary>
    /// Initializes a new instance of the DropFileEventArgs class.
    /// </summary>
    /// <param name="index">The position of the insertion caret.</param>
    /// <param name="fileNames">The array of filenames droppped on the control.</param>
    public DropFileEventArgs(int index, string[] fileNames)
    {
        Cancel = false;
        Index = index;
        FileNames = fileNames;
    }

}

/// <summary>
/// Represents the event arguments for internal drag drop events.
/// </summary>
[Serializable, ComVisible(true)]
public class DropItemEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets whether default event code will be processed.
    /// When set to true, the control will automatically insert the new items.
    /// Otherwise, the control will not process the dropped items.
    /// </summary>
    public bool Cancel { get; set; }

    /// <summary>
    /// Gets the position of the insertion caret.
    /// This determines where the new items will be inserted.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Gets the array of items droppped on the control.
    /// </summary>
    public ImageGalleryItem[] Items { get; private set; }


    /// <summary>
    /// Initializes a new instance of the DropItemEventArgs class.
    /// </summary>
    /// <param name="index">The position of the insertion caret.</param>
    /// <param name="items">The array of items droppped on the control.</param>
    public DropItemEventArgs(int index, ImageGalleryItem[] items)
    {
        Cancel = false;
        Index = index;
        Items = items;
    }
}

/// <summary>
/// Represents the event arguments for drag drop event completion.
/// </summary>
[Serializable, ComVisible(true)]
public class DropCompleteEventArgs : EventArgs
{
    /// <summary>
    /// Gets the array of items droppped on the control.
    /// </summary>
    public ImageGalleryItem[] Items { get; private set; }

    /// <summary>
    /// Gets if the drag operation is internal or external to the control.
    /// In an internal drag operation, own items of the control are reordered.
    /// </summary>
    public bool InternalDrag { get; private set; }

    /// <summary>
    /// Initializes a new instance of the DropCompleteEventArgs class.
    /// </summary>
    /// <param name="items">The array of items droppped on the control.</param>
    /// <param name="internalDrag">true if a drop event occurred after an internal reordering of items,
    /// otherwise false if image files were externally dropped onto the control.</param>
    public DropCompleteEventArgs(ImageGalleryItem[] items, bool internalDrag)
    {
        Items = items;
        InternalDrag = internalDrag;
    }
}

/// <summary>
/// Represents the event arguments for item related events.
/// </summary>
[Serializable, ComVisible(true)]
public class ItemEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is the target of the event.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ItemEventArgs class.
    /// </summary>
    /// <param name="item">The item that is the target of this event.</param>
    public ItemEventArgs(ImageGalleryItem? item)
    {
        Item = item;
    }
}

/// <summary>
/// Represents the event arguments for item click related events.
/// </summary>
[Serializable, ComVisible(true)]
public class ItemClickEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is the target of the event.
    /// </summary>
    public ImageGalleryItem Item { get; private set; }

    /// <summary>
    /// Gets the coordinates of the cursor.
    /// </summary>
    public Point Location { get; private set; }

    /// <summary>
    /// Gets the x-coordinates of the cursor.
    /// </summary>
    public int X => Location.X;

    /// <summary>
    /// Gets the y-coordinates of the cursor.
    /// </summary>
    public int Y => Location.Y;

    /// <summary>
    /// Gets the state of the mouse buttons.
    /// </summary>
    public MouseButtons Buttons { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ItemClickEventArgs class.
    /// </summary>
    /// <param name="item">The item that is the target of this event.</param>
    /// <param name="location">The location of the mouse.</param>
    /// <param name="buttons">One of the System.Windows.Forms.MouseButtons values 
    /// indicating which mouse button was pressed.</param>
    public ItemClickEventArgs(ImageGalleryItem item, Point location, MouseButtons buttons)
    {
        Item = item;
        Location = location;
        Buttons = buttons;
    }
}

/// <summary>
/// Represents the event arguments for item hover related events.
/// </summary>
[Serializable, ComVisible(true)]
public class ItemHoverEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that was previously hovered.
    /// Returns null if there was no previously hovered item.
    /// </summary>
    public ImageGalleryItem? PreviousItem { get; private set; }

    /// <summary>
    /// Gets the currently hovered <see cref="ImageGalleryItem"/>.
    /// Returns null if there is no hovered item.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }


    /// <summary>
    /// Initializes a new instance of the ItemEventArgs class.
    /// </summary>
    /// <param name="item">The currently hovered item.</param>
    /// <param name="previousItem">The previously hovered item.</param>
    public ItemHoverEventArgs(ImageGalleryItem? item, ImageGalleryItem? previousItem)
    {
        Item = item;
        PreviousItem = previousItem;
    }
}

/// <summary>
/// Represents the event arguments related to control layout.
/// </summary>
[Serializable, ComVisible(true)]
public class LayoutEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the rectangle bounding the item area.
    /// </summary>
    public Rectangle ItemAreaBounds { get; set; }

    /// <summary>
    /// Initializes a new instance of the LayoutEventArgs class.
    /// </summary>
    /// <param name="itemAreaBounds">The rectangle bounding the item area.</param>
    public LayoutEventArgs(Rectangle itemAreaBounds)
    {
        ItemAreaBounds = itemAreaBounds;
    }
}

/// <summary>
/// Represents the event arguments for the thumbnail caching event.
/// </summary>
[Serializable, ComVisible(true)]
public class ThumbnailCachingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is the target of the event.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }

    /// <summary>
    /// Gets the size of the thumbnail request.
    /// </summary>
    public Size Size { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ThumbnailCachingEventArgs class.
    /// </summary>
    /// <param name="item">The item that is the target of this event.</param>
    /// <param name="size">The size of the thumbnail request.</param>
    public ThumbnailCachingEventArgs(ImageGalleryItem? item, Size size)
    {
        Item = item;
        Size = size;
    }
}

/// <summary>
/// Represents the event arguments for the thumbnail cached event.
/// </summary>
[Serializable, ComVisible(true)]
public class ThumbnailCachedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is the target of the event.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }

    /// <summary>
    /// Gets the size of the thumbnail request.
    /// </summary>
    public Size Size { get; private set; }

    /// <summary>
    /// Gets the cached thumbnail image.
    /// </summary>
    public Image? Thumbnail { get; private set; }

    /// <summary>
    /// Gets whether the cached image is a thumbnail image or
    /// a large image for gallery or pane views.
    /// </summary>
    public bool IsThumbnail { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ThumbnailCachedEventArgs class.
    /// </summary>
    /// <param name="item">The item that is the target of this event.</param>
    /// <param name="thumbnail">The cached thumbnail image.</param>
    /// <param name="size">The size of the thumbnail request.</param>
    /// <param name="thumbnailImage">true if the cached image is a thumbnail image; otherwise false
    /// if the image is a large image for gallery or pane views.</param>
    public ThumbnailCachedEventArgs(ImageGalleryItem? item, Image? thumbnail, Size size, bool thumbnailImage)
    {
        Item = item;
        Thumbnail = thumbnail;
        Size = size;
        IsThumbnail = thumbnailImage;
    }
}

/// <summary>
/// Represents the event arguments for the shell info caching event.
/// </summary>
[Serializable, ComVisible(true)]
public class ShellInfoCachingEventArgs : EventArgs
{
    /// <summary>
    /// Gets the file extension for which the shell info is requested.
    /// </summary>
    public string Extension { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ShellInfoCachingEventArgs class.
    /// </summary>
    /// <param name="extension">The file extension for which the shell info is requested.</param>
    public ShellInfoCachingEventArgs(string extension)
    {
        Extension = extension;
    }
}

/// <summary>
/// Represents the event arguments for the shell info cached event.
/// </summary>
[Serializable, ComVisible(true)]
public class ShellInfoCachedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the file extension for which the shell info is requested.
    /// </summary>
    public string Extension { get; private set; }

    /// <summary>
    /// Gets the small shell icon.
    /// </summary>
    public Image SmallIcon { get; private set; }

    /// <summary>
    /// Gets the large shell icon.
    /// </summary>
    public Image LargeIcon { get; private set; }

    /// <summary>
    /// Gets the shell file type.
    /// </summary>
    public string FileType { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ShellInfoCachedEventArgs"/> class.
    /// </summary>
    /// <param name="extension">The file extension for which the shell info is requested.</param>
    /// <param name="smallIcon">The small shell icon.</param>
    /// <param name="largeIcon">The large shell icon.</param>
    /// <param name="filetype">The shell file type.</param>
    public ShellInfoCachedEventArgs(string extension, Image smallIcon, Image largeIcon, string filetype)
    {
        Extension = extension;
        SmallIcon = smallIcon;
        LargeIcon = largeIcon;
        FileType = filetype;
    }
}

/// <summary>
/// Represents the event arguments for item collection related events.
/// </summary>
[Serializable, ComVisible(true)]
public class ItemCollectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of action causing the change.
    /// </summary>
    public CollectionChangeAction Action { get; private set; }

    /// <summary>
    /// Gets the <see cref="ImageGalleryItem"/> that is the target of the event.
    /// </summary>
    public ImageGalleryItem? Item { get; private set; }

    /// <summary>
    /// Initializes a new instance of the ItemCollectionChangedEventArgs class.
    /// </summary>
    /// <param name="action">The type of action causing the change.</param>
    /// <param name="item">The item that is the target of this event. This parameter will be null
    /// if the collection is cleared.</param>
    public ItemCollectionChangedEventArgs(CollectionChangeAction action, ImageGalleryItem? item)
    {
        Action = action;
        Item = item;
    }

}


/// <summary>
/// Represents the event arguments for item tooltip showing events.
/// </summary>
[Serializable, ComVisible(true)]
public class ItemTooltipShowingEventArgs : EventArgs
{
    /// <summary>
    /// Gets, sets the tooltip title.
    /// </summary>
    public string TooltipTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets, sets the tooltip content.
    /// </summary>
    public string TooltipContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets the thumbnail item.
    /// </summary>
    public ImageGalleryItem Item { get; init; }


    /// <summary>
    /// Initialize the new instance of <see cref="ItemTooltipShowingEventArgs"/>.
    /// </summary>
    /// <param name="item"></param>
    public ItemTooltipShowingEventArgs(ImageGalleryItem item)
    {
        Item = item;
    }
}

#endregion // Event arguments


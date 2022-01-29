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

using ImageGlass.Base.Cache;
using System.ComponentModel;
using static ImageGlass.Gallery.ImageListView;

namespace ImageGlass.Gallery;

/// <summary>
/// Represents an item in the image list view.
/// </summary>
public class ImageListViewItem : ICloneable
{
    #region Member Variables

    private static readonly StringCache _stringCache = new();

    // Property backing fields
    internal int mIndex = -1;
    internal bool mChecked = false;
    internal bool mSelected = false;
    internal bool mPressed = false;
    internal bool mEnabled = true;
    private string mText = string.Empty;
    private int mZOrder = 0;

    // File info
    private ImageDetails? _details = null;
    internal string extension = string.Empty;
    private string mFileName = string.Empty;

    // Adaptor
    internal object? mVirtualItemKey = null;
    internal IAdaptor? mAdaptor = null;

    // Used for cloned items
    internal Image? clonedThumbnail;

    internal ImageListViewItemCollection? owner = null;
    internal bool isDirty = true;
    private bool editing = false;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the cache state of the item thumbnail.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public CacheState ThumbnailCacheState
    {
        get
        {
            if (ImageListView is null)
            {
                return CacheState.Unknown;
            }

            return ImageListView.thumbnailCache.GetCacheState(
                Guid,
                ImageListView.ThumbnailSize,
                ImageListView.UseEmbeddedThumbnails,
                ImageListView.AutoRotateThumbnails);
        }
    }
    /// <summary>
    /// Gets a value determining if the item is focused.
    /// </summary>
    [Category("Appearance"), Browsable(false)]
    public bool Focused
    {
        get
        {
            if (owner == null || owner.FocusedItem == null) return false;
            return (this == owner.FocusedItem);
        }
        set
        {
            if (owner != null)
                owner.FocusedItem = this;
        }
    }
    /// <summary>
    /// Gets a value determining if the item is enabled.
    /// </summary>
    [Category("Appearance")]
    public bool Enabled
    {
        get => mEnabled;
        set
        {
            mEnabled = value;
            if (!mEnabled && mSelected)
            {
                mSelected = false;
                if (ImageListView != null)
                    ImageListView.OnSelectionChangedInternal();
            }
            if (ImageListView != null && ImageListView.IsItemVisible(Guid))
                ImageListView.Refresh();
        }
    }

    /// <summary>
    /// Gets image details.
    /// </summary>
    [Browsable(false)]
    public ImageDetails Details
    {
        get
        {
            if (_details is null)
            {
                _details = FetchItemDetails();
            }

            return _details;
        }
    }

    /// <summary>
    /// Gets the unique identifier for this item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    internal Guid Guid { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the adaptor of this item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public IAdaptor? Adaptor => mAdaptor;

    /// <summary>
    /// [IG_CHANGE] Gets the virtual item key associated with this item.
    /// Returns null if the item is not a virtual item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public object VirtualItemKey => mVirtualItemKey ?? mFileName;

    /// <summary>
    /// Gets the ImageListView owning this item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public ImageListView? ImageListView { get; internal set; } = null;

    /// <summary>
    /// Gets the index of the item.
    /// </summary>
    [Category("Behavior"), Browsable(false), EditorBrowsable(EditorBrowsableState.Advanced)]
    public int Index => mIndex;

    /// <summary>
    /// Gets or sets a value determining if the item is checked.
    /// </summary>
    [Category("Appearance"), DefaultValue(false)]
    public bool Checked
    {
        get => mChecked;
        set
        {
            if (value != mChecked)
            {
                mChecked = value;
                if (ImageListView != null)
                    ImageListView.OnItemCheckBoxClickInternal(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value determining if the item is selected.
    /// </summary>
    [Category("Appearance"), Browsable(false), DefaultValue(false)]
    public bool Selected
    {
        get => mSelected;
        set
        {
            if (value != mSelected && mEnabled)
            {
                mSelected = value;
                if (ImageListView != null)
                {
                    ImageListView.OnSelectionChangedInternal();
                    if (ImageListView.IsItemVisible(Guid))
                        ImageListView.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a value determining if the item is pressed.
    /// </summary>
    public bool Pressed
    {
        get => mPressed;
        set
        {
            if (value != mPressed && mEnabled)
            {
                mPressed = value;
                if (ImageListView != null)
                {
                    if (ImageListView.IsItemVisible(Guid))
                        ImageListView.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the user-defined data associated with the item.
    /// </summary>
    [Category("Data"), TypeConverter(typeof(StringConverter))]
    public object? Tag { get; set; } = null;

    /// <summary>
    /// Gets or sets the text associated with this item. If left blank, item Text 
    /// reverts to the name of the image file.
    /// </summary>
    [Category("Appearance")]
    public string Text
    {
        get => mText;
        set
        {
            mText = value;
            if (ImageListView != null && ImageListView.IsItemVisible(Guid))
                ImageListView.Refresh();
        }
    }

    /// <summary>
    /// Gets or sets the name of the image file represented by this item.
    /// </summary>        
    [Category("File Properties")]
    public string FileName
    {
        get => mFileName;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("FileName cannot be null");

            if (mFileName != value)
            {
                mFileName = value;
                mVirtualItemKey = mFileName;
                extension = _stringCache.GetFromCache(Path.GetExtension(mFileName));

                isDirty = true;
                if (ImageListView != null)
                {
                    ImageListView.thumbnailCache.Remove(Guid, true);
                    ImageListView.metadataCache.Remove(Guid);
                    ImageListView.metadataCache.Add(Guid, Adaptor, mFileName);
                    if (ImageListView.IsItemVisible(Guid))
                        ImageListView.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Gets the thumbnail image. If the thumbnail image is not cached, it will be 
    /// added to the cache queue and null will be returned. The returned image needs
    /// to be disposed by the caller.
    /// </summary>
    [Category("Appearance"), Browsable(false)]
    public Image? ThumbnailImage
    {
        get
        {
            if (ImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            if (ThumbnailCacheState != CacheState.Cached)
            {
                ImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, ImageListView.ThumbnailSize,
                    ImageListView.UseEmbeddedThumbnails, ImageListView.AutoRotateThumbnails);
            }

            return ImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageListView.ThumbnailSize, ImageListView.UseEmbeddedThumbnails,
                ImageListView.AutoRotateThumbnails, true);
        }
    }

    /// <summary>
    /// Gets or sets the draw order of the item.
    /// </summary>
    [Category("Appearance"), DefaultValue(0)]
    public int ZOrder
    {
        get => mZOrder; set
        {
            mZOrder = value;
        }
    }

    #endregion


    #region Shell Properties
    /// <summary>
    /// Gets the small shell icon of the image file represented by this item.
    /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
    /// </summary>
    [Category("Appearance"), Browsable(false)]
    public Image? SmallIcon
    {
        get
        {
            if (ImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageListView.shellInfoCache.GetSmallIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageListView.RetryOnError)
                {
                    ImageListView.shellInfoCache.Remove(iconPath);
                    ImageListView.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                ImageListView.shellInfoCache.Add(iconPath);
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the large shell icon of the image file represented by this item.
    /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
    /// </summary>
    [Category("Appearance"), Browsable(false)]
    public Image? LargeIcon
    {
        get
        {
            if (ImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageListView.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageListView.RetryOnError)
                {
                    ImageListView.shellInfoCache.Remove(iconPath);
                    ImageListView.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                ImageListView.shellInfoCache.Add(iconPath);
                return null;
            }
        }
    }
    
    /// <summary>
    /// Gets the shell type of the image file represented by this item.
    /// </summary>
    [Category("File Properties")]
    public string FileType
    {
        get
        {
            if (ImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageListView.shellInfoCache.GetFileType(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageListView.RetryOnError)
                {
                    ImageListView.shellInfoCache.Remove(iconPath);
                    ImageListView.shellInfoCache.Add(iconPath);
                }
                return string.Empty;
            }
            else
            {
                ImageListView.shellInfoCache.Add(iconPath);
                return string.Empty;
            }
        }
    }
    
    #endregion


    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
    /// </summary>
    public ImageListViewItem()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="filename">The image filename representing the item.</param>
    /// <param name="text">Item text</param>
    public ImageListViewItem(string filename, string text) : this()
    {
        if (File.Exists(filename))
        {
            mFileName = filename;
            extension = _stringCache.GetFromCache(Path.GetExtension(filename));

            // if text parameter is empty then get file name for item text
            if (string.IsNullOrEmpty(text))
            {
                mText = Path.GetFileName(filename);
            }
        }
        else if (string.IsNullOrEmpty(text))
        {
            mFileName = filename;
            mText = filename;
        }
        else
        {
            mFileName = filename;
            mText = text;
        }

        mVirtualItemKey = mFileName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="filename">The image filename representing the item.</param>
    public ImageListViewItem(string filename) : this(filename, string.Empty) { }

    /// <summary>
    /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="key">The key identifying this item.</param>
    /// <param name="text">Text of this item.</param>
    public ImageListViewItem(object key, string text) : this()
    {
        mVirtualItemKey = key;
        mText = text;
    }

    /// <summary>
    /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="key">The key identifying this item.</param>
    public ImageListViewItem(object key) : this(key, string.Empty) { }

    #endregion


    #region Instance Methods
    /// <summary>
    /// Begins editing the item.
    /// This method must be used while editing the item
    /// to prevent collisions with the cache manager.
    /// </summary>
    public void BeginEdit()
    {
        if (editing == true)
            throw new InvalidOperationException("Already editing this item.");

        if (ImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        ImageListView.thumbnailCache.BeginItemEdit(Guid);
        ImageListView.metadataCache.BeginItemEdit(Guid);

        editing = true;
    }

    /// <summary>
    /// Ends editing and updates the item.
    /// </summary>
    /// <param name="update">If set to true, the item will be immediately updated.</param>
    public void EndEdit(bool update)
    {
        if (editing == false)
            throw new InvalidOperationException("This item is not being edited.");

        if (ImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        ImageListView.thumbnailCache.EndItemEdit(Guid);
        ImageListView.metadataCache.EndItemEdit(Guid);

        editing = false;
        if (update) Update();
    }

    /// <summary>
    /// Ends editing and updates the item.
    /// </summary>
    public void EndEdit()
    {
        EndEdit(true);
    }

    /// <summary>
    /// Updates item thumbnail and item details.
    /// </summary>
    public void Update()
    {
        isDirty = true;
        if (ImageListView != null)
        {
            ImageListView.thumbnailCache.Remove(Guid, true);
            ImageListView.metadataCache.Remove(Guid);
            ImageListView.metadataCache.Add(Guid, mAdaptor, mVirtualItemKey);
            ImageListView.Refresh();
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(mText))
            return mText;
        else if (!string.IsNullOrEmpty(mFileName))
            return Path.GetFileName(mFileName);
        else
            return string.Format("Item {0}", mIndex);
    }

    #endregion


    #region Helper Methods

    /// <summary>
    /// Gets an image from the cache manager.
    /// If the thumbnail image is not cached, it will be 
    /// added to the cache queue and DefaultImage of the owner image list view will
    /// be returned. If the thumbnail could not be cached ErrorImage of the owner
    /// image list view will be returned.
    /// </summary>
    /// <param name="imageType">Type of cached image to return.</param>
    /// <returns>Requested thumbnail or icon.</returns>
    public Image? GetCachedImage(CachedImageType imageType)
    {
        if (ImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        string iconPath = PathForShellIcon();

        if (imageType == CachedImageType.SmallIcon || imageType == CachedImageType.LargeIcon)
        {
            if (string.IsNullOrEmpty(iconPath))
                return ImageListView.DefaultImage;

            CacheState state = ImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                if (imageType == CachedImageType.SmallIcon)
                    return ImageListView.shellInfoCache.GetSmallIcon(iconPath);
                else
                    return ImageListView.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageListView.RetryOnError)
                {
                    ImageListView.shellInfoCache.Remove(iconPath);
                    ImageListView.shellInfoCache.Add(iconPath);
                }
                return ImageListView.ErrorImage;
            }
            else
            {
                ImageListView.shellInfoCache.Add(iconPath);
                return ImageListView.DefaultImage;
            }
        }
        else
        {
            Image? img = null;
            CacheState state = ThumbnailCacheState;

            if (state == CacheState.Error)
            {
                if (ImageListView.ShellIconFallback && !string.IsNullOrEmpty(iconPath))
                {
                    CacheState iconstate = ImageListView.shellInfoCache.GetCacheState(iconPath);
                    if (iconstate == CacheState.Cached)
                    {
                        if (ImageListView.ThumbnailSize.Width > 32 && ImageListView.ThumbnailSize.Height > 32)
                            img = ImageListView.shellInfoCache.GetLargeIcon(iconPath);
                        else
                            img = ImageListView.shellInfoCache.GetSmallIcon(iconPath);
                    }
                    else if (iconstate == CacheState.Error)
                    {
                        if (ImageListView.RetryOnError)
                        {
                            ImageListView.shellInfoCache.Remove(iconPath);
                            ImageListView.shellInfoCache.Add(iconPath);
                        }
                    }
                    else
                    {
                        ImageListView.shellInfoCache.Add(iconPath);
                    }
                }

                if (img == null)
                    img = ImageListView.ErrorImage;
                return img;
            }

            img = ImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageListView.ThumbnailSize, ImageListView.UseEmbeddedThumbnails,
                ImageListView.AutoRotateThumbnails, false);

            if (state == CacheState.Cached)
                return img;

            ImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, ImageListView.ThumbnailSize,
                ImageListView.UseEmbeddedThumbnails, ImageListView.AutoRotateThumbnails);

            if (img == null && string.IsNullOrEmpty(iconPath))
                return ImageListView.DefaultImage;

            if (img == null && ImageListView.ShellIconFallback && ImageListView.ThumbnailSize.Width > 16 && ImageListView.ThumbnailSize.Height > 16)
                img = ImageListView.shellInfoCache.GetLargeIcon(iconPath);
            if (img == null && ImageListView.ShellIconFallback)
                img = ImageListView.shellInfoCache.GetSmallIcon(iconPath);
            if (img == null)
                img = ImageListView.DefaultImage;

            return img;
        }
    }

    /// <summary>
    /// Fetch file info for the image file represented by this item,
    /// then update the <see cref="Details"/> property.
    /// </summary>
    /// <param name="force">Force to fetch details from file.</param>
    public ImageDetails? FetchItemDetails(bool force = false)
    {
        if (!isDirty
            || Adaptor is null
            || ImageListView is null
            || mVirtualItemKey is null) return null;

        if (force || _details is null)
        {
            return Adaptor.GetDetails(mVirtualItemKey);
        }

        return null;
    }

    /// <summary>
    /// Returns a path string to be used for extracting the shell icon
    /// of the item. Returns the filename for icon files and executables,
    /// file extension for other files.
    /// </summary>
    private string PathForShellIcon()
    {
        if (ImageListView != null && ImageListView.ShellIconFromFileContent &&
            (string.Compare(extension, ".ico", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(extension, ".exe", StringComparison.OrdinalIgnoreCase) == 0))
            return mFileName;
        else
            return extension;
    }

    #endregion


    #region ICloneable Members
    /// <summary>
    /// Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new object that is a copy of this instance.
    /// </returns>
    public object Clone()
    {
        var item = new ImageListViewItem
        {
            mText = mText,

            // File info
            extension = extension,
            mFileName = mFileName,

            _details = _details,

            // Virtual item properties
            mAdaptor = mAdaptor,
            mVirtualItemKey = mVirtualItemKey
        };

        // Current thumbnail
        if (ImageListView != null && mAdaptor != null)
        {
            item.clonedThumbnail = ImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageListView.ThumbnailSize,
                ImageListView.UseEmbeddedThumbnails, ImageListView.AutoRotateThumbnails, true);
        }

        return item;
    }
    #endregion

}



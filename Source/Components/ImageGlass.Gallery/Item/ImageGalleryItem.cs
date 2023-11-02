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

using ImageGlass.Base.Cache;
using ImageGlass.Base.Photoing.Codecs;
using System.ComponentModel;
using static ImageGlass.Gallery.ImageGallery;

namespace ImageGlass.Gallery;

/// <summary>
/// Represents an item in the image list view.
/// </summary>
public class ImageGalleryItem : ICloneable
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
    private IgMetadata? _details = null;
    internal string extension = string.Empty;
    internal string mFileName = string.Empty;

    // Adaptor
    internal object mVirtualItemKey;
    internal IAdaptor? mAdaptor = null;

    // Used for cloned items
    internal Image? clonedThumbnail;

    internal ItemCollection? owner = null;
    internal bool isDirty = true;
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
            if (ImageGalleryOwner is null)
            {
                return CacheState.Unknown;
            }

            return ImageGalleryOwner.thumbnailCache.GetCacheState(
                Guid,
                ImageGalleryOwner.ThumbnailSize,
                ImageGalleryOwner.UseEmbeddedThumbnails,
                ImageGalleryOwner.AutoRotateThumbnails);
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
                if (ImageGalleryOwner != null)
                    ImageGalleryOwner.OnSelectionChangedInternal();
            }
            if (ImageGalleryOwner != null && ImageGalleryOwner.IsItemVisible(Guid))
                ImageGalleryOwner.Refresh();
        }
    }

    /// <summary>
    /// Gets image details.
    /// </summary>
    [Browsable(false)]
    public IgMetadata Details
    {
        get
        {
            if (_details is null)
            {
                UpdateDetails();
            }

            return _details ?? new();
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
    /// Gets the virtual item key associated with this item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public object VirtualItemKey => mVirtualItemKey;

    /// <summary>
    /// Gets the ImageListView owning this item.
    /// </summary>
    [Category("Behavior"), Browsable(false)]
    public ImageGallery? ImageGalleryOwner { get; internal set; } = null;

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
                if (ImageGalleryOwner != null)
                    ImageGalleryOwner.OnItemCheckBoxClickInternal(this);
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
                if (ImageGalleryOwner != null)
                {
                    ImageGalleryOwner.OnSelectionChangedInternal();
                    if (ImageGalleryOwner.IsItemVisible(Guid))
                        ImageGalleryOwner.Refresh();
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
                if (ImageGalleryOwner != null)
                {
                    if (ImageGalleryOwner.IsItemVisible(Guid))
                        ImageGalleryOwner.Refresh();
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
            if (ImageGalleryOwner != null && ImageGalleryOwner.IsItemVisible(Guid))
                ImageGalleryOwner.Refresh();
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
                if (ImageGalleryOwner != null)
                {
                    ImageGalleryOwner.thumbnailCache.Remove(Guid, true);
                    ImageGalleryOwner.metadataCache.Remove(Guid);
                    ImageGalleryOwner.metadataCache.Add(Guid, Adaptor, mFileName);
                    if (ImageGalleryOwner.IsItemVisible(Guid))
                        ImageGalleryOwner.Refresh();
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
            if (ImageGalleryOwner == null)
                throw new InvalidOperationException("Owner control is null.");

            if (ThumbnailCacheState != CacheState.Cached)
            {
                ImageGalleryOwner.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, ImageGalleryOwner.ThumbnailSize,
                    ImageGalleryOwner.UseEmbeddedThumbnails, ImageGalleryOwner.AutoRotateThumbnails);
            }

            return ImageGalleryOwner.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageGalleryOwner.ThumbnailSize, ImageGalleryOwner.UseEmbeddedThumbnails,
                ImageGalleryOwner.AutoRotateThumbnails, true);
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
            if (ImageGalleryOwner == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageGalleryOwner.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageGalleryOwner.shellInfoCache.GetSmallIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageGalleryOwner.RetryOnError)
                {
                    ImageGalleryOwner.shellInfoCache.Remove(iconPath);
                    ImageGalleryOwner.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                ImageGalleryOwner.shellInfoCache.Add(iconPath);
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
            if (ImageGalleryOwner == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageGalleryOwner.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageGalleryOwner.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageGalleryOwner.RetryOnError)
                {
                    ImageGalleryOwner.shellInfoCache.Remove(iconPath);
                    ImageGalleryOwner.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                ImageGalleryOwner.shellInfoCache.Add(iconPath);
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
            if (ImageGalleryOwner == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = ImageGalleryOwner.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return ImageGalleryOwner.shellInfoCache.GetFileType(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageGalleryOwner.RetryOnError)
                {
                    ImageGalleryOwner.shellInfoCache.Remove(iconPath);
                    ImageGalleryOwner.shellInfoCache.Add(iconPath);
                }
                return string.Empty;
            }
            else
            {
                ImageGalleryOwner.shellInfoCache.Add(iconPath);
                return string.Empty;
            }
        }
    }

    #endregion


    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageGalleryItem"/> class.
    /// </summary>
    /// <param name="filename">The image filename representing the item.</param>
    /// <param name="text">Item text</param>
    public ImageGalleryItem(string filename, string text = "")
    {
        // important! to make it load faster using virtual items
        mVirtualItemKey = filename;

        mFileName = filename;
        mText = string.IsNullOrEmpty(text) ? Path.GetFileName(filename) : text;
        extension = _stringCache.GetFromCache(Path.GetExtension(filename));
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
        if (ImageGalleryOwner == null)
            throw new InvalidOperationException("Owner control is null.");

        string iconPath = PathForShellIcon();

        if (imageType == CachedImageType.SmallIcon || imageType == CachedImageType.LargeIcon)
        {
            if (string.IsNullOrEmpty(iconPath))
                return ImageGalleryOwner.DefaultImage;

            CacheState state = ImageGalleryOwner.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                if (imageType == CachedImageType.SmallIcon)
                    return ImageGalleryOwner.shellInfoCache.GetSmallIcon(iconPath);
                else
                    return ImageGalleryOwner.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (ImageGalleryOwner.RetryOnError)
                {
                    ImageGalleryOwner.shellInfoCache.Remove(iconPath);
                    ImageGalleryOwner.shellInfoCache.Add(iconPath);
                }
                return ImageGalleryOwner.ErrorImage;
            }
            else
            {
                ImageGalleryOwner.shellInfoCache.Add(iconPath);
                return ImageGalleryOwner.DefaultImage;
            }
        }
        else
        {
            Image? img = null;
            CacheState state = ThumbnailCacheState;

            if (state == CacheState.Error)
            {
                if (ImageGalleryOwner.ShellIconFallback && !string.IsNullOrEmpty(iconPath))
                {
                    CacheState iconstate = ImageGalleryOwner.shellInfoCache.GetCacheState(iconPath);
                    if (iconstate == CacheState.Cached)
                    {
                        if (ImageGalleryOwner.ThumbnailSize.Width > 32 && ImageGalleryOwner.ThumbnailSize.Height > 32)
                            img = ImageGalleryOwner.shellInfoCache.GetLargeIcon(iconPath);
                        else
                            img = ImageGalleryOwner.shellInfoCache.GetSmallIcon(iconPath);
                    }
                    else if (iconstate == CacheState.Error)
                    {
                        if (ImageGalleryOwner.RetryOnError)
                        {
                            ImageGalleryOwner.shellInfoCache.Remove(iconPath);
                            ImageGalleryOwner.shellInfoCache.Add(iconPath);
                        }
                    }
                    else
                    {
                        ImageGalleryOwner.shellInfoCache.Add(iconPath);
                    }
                }

                if (img == null)
                    img = ImageGalleryOwner.ErrorImage;
                return img;
            }

            img = ImageGalleryOwner.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageGalleryOwner.ThumbnailSize, ImageGalleryOwner.UseEmbeddedThumbnails,
                ImageGalleryOwner.AutoRotateThumbnails, false);

            if (state == CacheState.Cached)
                return img;

            ImageGalleryOwner.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, ImageGalleryOwner.ThumbnailSize,
                ImageGalleryOwner.UseEmbeddedThumbnails, ImageGalleryOwner.AutoRotateThumbnails);

            if (img == null && string.IsNullOrEmpty(iconPath))
                return ImageGalleryOwner.DefaultImage;

            if (img == null && ImageGalleryOwner.ShellIconFallback && ImageGalleryOwner.ThumbnailSize.Width > 16 && ImageGalleryOwner.ThumbnailSize.Height > 16)
                img = ImageGalleryOwner.shellInfoCache.GetLargeIcon(iconPath);
            if (img == null && ImageGalleryOwner.ShellIconFallback)
                img = ImageGalleryOwner.shellInfoCache.GetSmallIcon(iconPath);
            if (img == null)
                img = ImageGalleryOwner.DefaultImage;

            return img;
        }
    }

    /// <summary>
    /// Fetch file info for the image file represented by this item,
    /// then update the <see cref="Details"/> property.
    /// </summary>
    /// <param name="force">Force to fetch details from file.</param>
    public void UpdateDetails(bool force = false)
    {
        if (!isDirty
            || Adaptor == null
            || ImageGalleryOwner == null
            || mVirtualItemKey == null) return;

        if (force || _details == null)
        {
            _details = Adaptor.GetDetails(mVirtualItemKey);
        }
    }

    /// <summary>
    /// Removes the current thumbnail and requests to update it.
    /// </summary>
    public void UpdateThumbnail()
    {
        isDirty = true;
        if (ImageGalleryOwner != null)
        {
            ImageGalleryOwner.thumbnailCache.Remove(Guid, true);
            ImageGalleryOwner.metadataCache.Remove(Guid);
            ImageGalleryOwner.metadataCache.Add(Guid, Adaptor, mFileName);
            if (ImageGalleryOwner.IsItemVisible(Guid))
                ImageGalleryOwner.Refresh();
        }
    }

    /// <summary>
    /// Returns a path string to be used for extracting the shell icon
    /// of the item. Returns the filename for icon files and executables,
    /// file extension for other files.
    /// </summary>
    private string PathForShellIcon()
    {
        if (ImageGalleryOwner != null && ImageGalleryOwner.ShellIconFromFileContent &&
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
        var item = new ImageGalleryItem(mFileName, mText)
        {
            // File info
            extension = extension,
            _details = _details,

            // Virtual item properties
            mAdaptor = mAdaptor,
        };

        // Current thumbnail
        if (ImageGalleryOwner != null && mAdaptor != null)
        {
            item.clonedThumbnail = ImageGalleryOwner.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, ImageGalleryOwner.ThumbnailSize,
                ImageGalleryOwner.UseEmbeddedThumbnails, ImageGalleryOwner.AutoRotateThumbnails, true);
        }

        return item;
    }
    #endregion

}




using ImageGlass.Base.Cache;
using System.ComponentModel;
using static ImageGlass.Gallery.ImageListView;

namespace ImageGlass.Gallery;

/// <summary>
/// Represents an item in the image list view.
/// </summary>
[TypeConverter(typeof(ImageListViewItemTypeConverter))]
public class ImageListViewItem : ICloneable
{
    #region Member Variables
    // [IG_CHANGE] Cache often repeated strings, e.g. extensions, directory path
    private static readonly StringCache _stringCache = new();

    // Property backing fields
    internal int mIndex;
    private Guid mGuid;
    internal ImageListView? mImageListView;
    internal bool mChecked;
    internal bool mSelected;
    internal bool mPressed;
    internal bool mEnabled;
    private string mText;
    private int mZOrder;

    // File info
    internal string extension;
    private DateTime mDateAccessed;
    private DateTime mDateCreated;
    private DateTime mDateModified;
    private string mFileName;
    private string mFilePath;
    private long mFileSize;
    private Size mDimensions;
    private SizeF mResolution;

    // Exif tags
    private string mImageDescription;
    private string mEquipmentModel;
    private DateTime mDateTaken;
    private string mArtist;
    private string mCopyright;
    private float mExposureTime;
    private float mFNumber;
    private ushort mISOSpeed;
    private string mUserComment;
    private ushort mRating;
    private ushort mStarRating;
    private string mSoftware;
    private float mFocalLength;

    // Adaptor
    internal object? mVirtualItemKey;
    internal IAdaptor mAdaptor;

    // Used for cloned items
    internal Image? clonedThumbnail;

    internal ImageListViewItemCollection? owner;
    internal bool isDirty;
    private bool editing;
    #endregion


    #region Properties
    /// <summary>
    /// Gets the cache state of the item thumbnail.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the cache state of the item thumbnail.")]
    public CacheState ThumbnailCacheState
    {
        get
        {
            return mImageListView.thumbnailCache.GetCacheState(mGuid, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                mImageListView.AutoRotateThumbnails);
        }
    }
    /// <summary>
    /// Gets a value determining if the item is focused.
    /// </summary>
    [Category("Appearance"), Browsable(false), Description("Gets a value determining if the item is focused."), DefaultValue(false)]
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
    [Category("Appearance"), Browsable(true), Description("Gets a value determining if the item is enabled."), DefaultValue(true)]
    public bool Enabled
    {
        get
        {
            return mEnabled;
        }
        set
        {
            mEnabled = value;
            if (!mEnabled && mSelected)
            {
                mSelected = false;
                if (mImageListView != null)
                    mImageListView.OnSelectionChangedInternal();
            }
            if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                mImageListView.Refresh();
        }
    }

    /// <summary>
    /// Gets the unique identifier for this item.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the unique identifier for this item.")]
    internal Guid Guid
    {
        get => mGuid; private set
        {
            mGuid = value;
        }
    }

    /// <summary>
    /// Gets the adaptor of this item.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the adaptor of this item.")]
    public IAdaptor Adaptor => mAdaptor;

    /// <summary>
    /// [IG_CHANGE] Gets the virtual item key associated with this item.
    /// Returns null if the item is not a virtual item.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the virtual item key associated with this item.")]
    public object VirtualItemKey => mVirtualItemKey ?? mFileName;

    /// <summary>
    /// Gets the ImageListView owning this item.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the ImageListView owning this item.")]
    public ImageListView? ImageListView
    {
        get => mImageListView;
        private set
        {
            mImageListView = value;
        }
    }

    /// <summary>
    /// Gets the index of the item.
    /// </summary>
    [Category("Behavior"), Browsable(false), Description("Gets the index of the item."), EditorBrowsable(EditorBrowsableState.Advanced)]
    public int Index => mIndex;

    /// <summary>
    /// Gets or sets a value determining if the item is checked.
    /// </summary>
    [Category("Appearance"), Browsable(true), Description("Gets or sets a value determining if the item is checked."), DefaultValue(false)]
    public bool Checked
    {
        get => mChecked;
        set
        {
            if (value != mChecked)
            {
                mChecked = value;
                if (mImageListView != null)
                    mImageListView.OnItemCheckBoxClickInternal(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value determining if the item is selected.
    /// </summary>
    [Category("Appearance"), Browsable(false), Description("Gets or sets a value determining if the item is selected."), DefaultValue(false)]
    public bool Selected
    {
        get => mSelected;
        set
        {
            if (value != mSelected && mEnabled)
            {
                mSelected = value;
                if (mImageListView != null)
                {
                    mImageListView.OnSelectionChangedInternal();
                    if (mImageListView.IsItemVisible(mGuid))
                        mImageListView.Refresh();
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
                if (mImageListView != null)
                {
                    if (mImageListView.IsItemVisible(mGuid))
                        mImageListView.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the user-defined data associated with the item.
    /// </summary>
    [Category("Data"), Browsable(true), Description("Gets or sets the user-defined data associated with the item."), TypeConverter(typeof(StringConverter))]
    public object Tag { get; set; }

    /// <summary>
    /// Gets or sets the text associated with this item. If left blank, item Text 
    /// reverts to the name of the image file.
    /// </summary>
    [Category("Appearance"), Browsable(true), Description("Gets or sets the text associated with this item. If left blank, item Text reverts to the name of the image file.")]
    public string Text
    {
        get => mText;
        set
        {
            mText = value;
            if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                mImageListView.Refresh();
        }
    }

    /// <summary>
    /// Gets or sets the name of the image file represented by this item.
    /// </summary>        
    [Category("File Properties"), Browsable(true), Description("Gets or sets the name of the image file represented by this item.")]
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
                mVirtualItemKey = null; // mFileName; [IG_CHANGE] don't duplicate the filename

                // [IG_CHANGE] use string cache
                extension = _stringCache.GetFromCache(Path.GetExtension(mFileName));

                isDirty = true;
                if (mImageListView != null)
                {
                    mImageListView.thumbnailCache.Remove(mGuid, true);
                    mImageListView.metadataCache.Remove(mGuid);
                    mImageListView.metadataCache.Add(mGuid, Adaptor, mFileName);
                    if (mImageListView.IsItemVisible(mGuid))
                        mImageListView.Refresh();
                }
            }
        }
    }

    /// <summary>
    /// Gets the thumbnail image. If the thumbnail image is not cached, it will be 
    /// added to the cache queue and null will be returned. The returned image needs
    /// to be disposed by the caller.
    /// </summary>
    [Category("Appearance"), Browsable(false), Description("Gets the thumbnail image.")]
    public Image ThumbnailImage
    {
        get
        {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            if (ThumbnailCacheState != CacheState.Cached)
            {
                mImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                    mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails);
            }

            return mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                mImageListView.AutoRotateThumbnails, true);
        }
    }

    /// <summary>
    /// Gets or sets the draw order of the item.
    /// </summary>
    [Category("Appearance"), Browsable(true), Description("Gets or sets the draw order of the item."), DefaultValue(0)]
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
    [Category("Appearance"), Browsable(false), Description("Gets the small shell icon of the image file represented by this item.")]
    public Image? SmallIcon
    {
        get
        {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = mImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return mImageListView.shellInfoCache.GetSmallIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (mImageListView.RetryOnError)
                {
                    mImageListView.shellInfoCache.Remove(iconPath);
                    mImageListView.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                mImageListView.shellInfoCache.Add(iconPath);
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the large shell icon of the image file represented by this item.
    /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
    /// </summary>
    [Category("Appearance"), Browsable(false), Description("Gets the large shell icon of the image file represented by this item.")]
    public Image? LargeIcon
    {
        get
        {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = mImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return mImageListView.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (mImageListView.RetryOnError)
                {
                    mImageListView.shellInfoCache.Remove(iconPath);
                    mImageListView.shellInfoCache.Add(iconPath);
                }
                return null;
            }
            else
            {
                mImageListView.shellInfoCache.Add(iconPath);
                return null;
            }
        }
    }
    
    /// <summary>
    /// Gets the shell type of the image file represented by this item.
    /// </summary>
    [Category("File Properties"), Browsable(true), Description("Gets the shell type of the image file represented by this item.")]
    public string FileType
    {
        get
        {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            var iconPath = PathForShellIcon();
            var state = mImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                return mImageListView.shellInfoCache.GetFileType(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (mImageListView.RetryOnError)
                {
                    mImageListView.shellInfoCache.Remove(iconPath);
                    mImageListView.shellInfoCache.Add(iconPath);
                }
                return string.Empty;
            }
            else
            {
                mImageListView.shellInfoCache.Add(iconPath);
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
        mIndex = -1;
        owner = null;

        mZOrder = 0;

        Guid = Guid.NewGuid();
        ImageListView = null;
        Checked = false;
        Selected = false;
        Enabled = true;

        isDirty = true;
        editing = false;

        mVirtualItemKey = null;

        Tag = null;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="filename">The image filename representing the item.</param>
    /// <param name="text">Item text</param>
    public ImageListViewItem(string filename, string text)
        : this()
    {
        if (File.Exists(filename))
        {
            mFileName = filename;

            // [IG_CHANGE] use string cache
            extension = _stringCache.GetFromCache(Path.GetExtension(filename));

            // if text parameter is empty then get file name for item text
            if (string.IsNullOrEmpty(text))
            {
                // [IG_CHANGE] don't duplicate filename text = Path.GetFileName(filename);
                mText = text;
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
    public ImageListViewItem(string filename) : this(filename, string.Empty)
    {
        ;
    }
    /// <summary>
    /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="key">The key identifying this item.</param>
    /// <param name="text">Text of this item.</param>
    public ImageListViewItem(object key, string text)
        : this()
    {
        mVirtualItemKey = key;
        mText = text;
    }
    /// <summary>
    /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
    /// </summary>
    /// <param name="key">The key identifying this item.</param>
    public ImageListViewItem(object key)
        : this(key, string.Empty)
    {
        ;
    }
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

        if (mImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        mImageListView.thumbnailCache.BeginItemEdit(mGuid);
        mImageListView.metadataCache.BeginItemEdit(mGuid);

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

        if (mImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        mImageListView.thumbnailCache.EndItemEdit(mGuid);
        mImageListView.metadataCache.EndItemEdit(mGuid);

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
        if (mImageListView != null)
        {
            mImageListView.thumbnailCache.Remove(mGuid, true);
            mImageListView.metadataCache.Remove(mGuid);
            mImageListView.metadataCache.Add(mGuid, mAdaptor, mVirtualItemKey);
            mImageListView.Refresh();
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
    /// Gets the simpel rating (0-5)
    /// </summary>
    /// <returns></returns>
    internal ushort GetSimpleRating()
    {
        return mStarRating;
    }

    /// <summary>
    /// Sets the simple rating (0-5) from rating (0-99).
    /// </summary>
    private void UpdateRating()
    {
        if (mRating >= 1 && mRating <= 12)
            mStarRating = 1;
        else if (mRating >= 13 && mRating <= 37)
            mStarRating = 2;
        else if (mRating >= 38 && mRating <= 62)
            mStarRating = 3;
        else if (mRating >= 63 && mRating <= 87)
            mStarRating = 4;
        else if (mRating >= 88 && mRating <= 99)
            mStarRating = 5;
        else
            mStarRating = 0;
    }

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
        if (mImageListView == null)
            throw new InvalidOperationException("Owner control is null.");

        string iconPath = PathForShellIcon();

        if (imageType == CachedImageType.SmallIcon || imageType == CachedImageType.LargeIcon)
        {
            if (string.IsNullOrEmpty(iconPath))
                return mImageListView.DefaultImage;

            CacheState state = mImageListView.shellInfoCache.GetCacheState(iconPath);
            if (state == CacheState.Cached)
            {
                if (imageType == CachedImageType.SmallIcon)
                    return mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                else
                    return mImageListView.shellInfoCache.GetLargeIcon(iconPath);
            }
            else if (state == CacheState.Error)
            {
                if (mImageListView.RetryOnError)
                {
                    mImageListView.shellInfoCache.Remove(iconPath);
                    mImageListView.shellInfoCache.Add(iconPath);
                }
                return mImageListView.ErrorImage;
            }
            else
            {
                mImageListView.shellInfoCache.Add(iconPath);
                return mImageListView.DefaultImage;
            }
        }
        else
        {
            Image? img = null;
            CacheState state = ThumbnailCacheState;

            if (state == CacheState.Error)
            {
                if (mImageListView.ShellIconFallback && !string.IsNullOrEmpty(iconPath))
                {
                    CacheState iconstate = mImageListView.shellInfoCache.GetCacheState(iconPath);
                    if (iconstate == CacheState.Cached)
                    {
                        if (mImageListView.ThumbnailSize.Width > 32 && mImageListView.ThumbnailSize.Height > 32)
                            img = mImageListView.shellInfoCache.GetLargeIcon(iconPath);
                        else
                            img = mImageListView.shellInfoCache.GetSmallIcon(iconPath);
                    }
                    else if (iconstate == CacheState.Error)
                    {
                        if (mImageListView.RetryOnError)
                        {
                            mImageListView.shellInfoCache.Remove(iconPath);
                            mImageListView.shellInfoCache.Add(iconPath);
                        }
                    }
                    else
                    {
                        mImageListView.shellInfoCache.Add(iconPath);
                    }
                }

                if (img == null)
                    img = mImageListView.ErrorImage;
                return img;
            }

            img = mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                mImageListView.AutoRotateThumbnails, false);

            if (state == CacheState.Cached)
                return img;

            mImageListView.thumbnailCache.Add(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails);

            if (img == null && string.IsNullOrEmpty(iconPath))
                return mImageListView.DefaultImage;

            if (img == null && mImageListView.ShellIconFallback && mImageListView.ThumbnailSize.Width > 16 && mImageListView.ThumbnailSize.Height > 16)
                img = mImageListView.shellInfoCache.GetLargeIcon(iconPath);
            if (img == null && mImageListView.ShellIconFallback)
                img = mImageListView.shellInfoCache.GetSmallIcon(iconPath);
            if (img == null)
                img = mImageListView.DefaultImage;

            return img;
        }
    }

    /// <summary>
    /// Returns a path string to be used for extracting the shell icon
    /// of the item. Returns the filename for icon files and executables,
    /// file extension for other files.
    /// </summary>
    private string PathForShellIcon()
    {
        if (mImageListView != null && mImageListView.ShellIconFromFileContent &&
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
            mDateAccessed = mDateAccessed,
            mDateCreated = mDateCreated,
            mDateModified = mDateModified,
            mFileName = mFileName,
            mFilePath = mFilePath,
            mFileSize = mFileSize,

            // Image info
            mDimensions = mDimensions,
            mResolution = mResolution,

            // Exif tags
            mImageDescription = mImageDescription,
            mEquipmentModel = mEquipmentModel,
            mDateTaken = mDateTaken,
            mArtist = mArtist,
            mCopyright = mCopyright,
            mExposureTime = mExposureTime,
            mFNumber = mFNumber,
            mISOSpeed = mISOSpeed,
            mUserComment = mUserComment,
            mRating = mRating,
            mStarRating = mStarRating,
            mSoftware = mSoftware,
            mFocalLength = mFocalLength,

            // Virtual item properties
            mAdaptor = mAdaptor,
            mVirtualItemKey = mVirtualItemKey
        };

        // Current thumbnail
        if (mImageListView != null)
        {
            item.clonedThumbnail = mImageListView.thumbnailCache.GetImage(Guid, mAdaptor, mVirtualItemKey, mImageListView.ThumbnailSize,
                mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails, true);
        }

        return item;
    }
    #endregion

}



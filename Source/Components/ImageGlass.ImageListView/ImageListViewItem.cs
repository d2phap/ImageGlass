// ImageListView - A listview control for image files
// Copyright (C) 2009 Ozgur Ozcitak
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Ozgur Ozcitak (ozcitak@yahoo.com)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;

// ReSharper disable InconsistentNaming

namespace ImageGlass.ImageListView {
    /// <summary>
    /// Represents an item in the image list view.
    /// </summary>
    [TypeConverter(typeof(ImageListViewItemTypeConverter))]
    public class ImageListViewItem: ICloneable {
        // [IG_CHANGE] Cache often repeated strings, e.g. extensions, directory path
        private static readonly StringCache _stringCache = new StringCache();

        #region Member Variables
        // Property backing fields
        internal int mIndex;
        private Guid mGuid;
        internal ImageListView mImageListView;
        internal bool mChecked;
        internal bool mSelected;
        internal bool mEnabled;
        private string mText;
        private int mZOrder;
        // File info
        internal string extension;
        private DateTime mDateAccessed;
        private DateTime mDateCreated;
        private DateTime mDateModified;
        private string mFileType;
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
        private object mVirtualItemKey;
        internal ImageListView.ImageListViewItemAdaptor mAdaptor;
        // Used for custom columns
        private Dictionary<Guid, string> subItems;
        // Used for cloned items
        internal Image clonedThumbnail;
        // Group info
        internal string group;
        internal int groupOrder;

        internal ImageListView.ImageListViewItemCollection owner;
        internal bool isDirty;
        private bool editing;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the cache state of the item thumbnail.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the cache state of the item thumbnail.")]
        public CacheState ThumbnailCacheState {
            get {
                return mImageListView.thumbnailCache.GetCacheState(mGuid, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly);
            }
        }
        /// <summary>
        /// Gets a value determining if the item is focused.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets a value determining if the item is focused."), DefaultValue(false)]
        public bool Focused {
            get {
                if (owner?.FocusedItem == null) return false;
                return (this == owner.FocusedItem);
            }
            set {
                if (owner != null)
                    owner.FocusedItem = this;
            }
        }
        /// <summary>
        /// Gets a value determining if the item is enabled.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets a value determining if the item is enabled."), DefaultValue(true)]
        public bool Enabled {
            get {
                return mEnabled;
            }
            set {
                mEnabled = value;
                if (!mEnabled && mSelected) {
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
        internal Guid Guid { get { return mGuid; } private set { mGuid = value; } }
        /// <summary>
        /// Gets the adaptor of this item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the adaptor of this item.")]
        public ImageListView.ImageListViewItemAdaptor Adaptor { get { return mAdaptor; } }
        /// <summary>
        /// Gets the virtual item key associated with this item.
        /// Returns null if the item is not a virtual item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the virtual item key associated with this item.")]
        public object VirtualItemKey { get { return mVirtualItemKey ?? mFileName; } } // [IG_CHANGE]
        /// <summary>
        /// Gets the ImageListView owning this item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the ImageListView owning this item.")]
        public ImageListView ImageListView { get { return mImageListView; } private set { mImageListView = value; } }
        /// <summary>
        /// Gets the index of the item.
        /// </summary>
        [Category("Behavior"), Browsable(false), Description("Gets the index of the item."), EditorBrowsable(EditorBrowsableState.Advanced)]
        public int Index { get { return mIndex; } }
        /// <summary>
        /// Gets or sets a value determining if the item is checked.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets or sets a value determining if the item is checked."), DefaultValue(false)]
        public bool Checked {
            get {
                return mChecked;
            }
            set {
                if (value != mChecked) {
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
        public bool Selected {
            get {
                return mSelected;
            }
            set {
                if (value != mSelected && mEnabled) {
                    mSelected = value;
                    if (mImageListView != null) {
                        mImageListView.OnSelectionChangedInternal();
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
        public string Text {
            get {
                return mText ?? Path.GetFileName(mFileName); // [IG_CHANGE]
            }
            set {
                mText = value;
                if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                    mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Gets or sets the name of the image file represented by this item.
        /// </summary>        
        [Category("File Properties"), Browsable(true), Description("Gets or sets the name of the image file represented by this item.")]
        [Editor(typeof(OpenFileDialogEditor), typeof(UITypeEditor))]
        public string FileName {
            get {
                return mFileName;
            }
            set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("FileName cannot be null");

                if (mFileName != value) {
                    mFileName = value;
                    mVirtualItemKey = null; //mFileName; [IG_CHANGE] don't duplicate the filename

                    // [IG_CHANGE]
                    //if (string.IsNullOrEmpty(mText))
                    //    mText = Path.GetFileName(mFileName);
                    // [IG_CHANGE] use string cache
                    extension = _stringCache.GetFromCache(Path.GetExtension(mFileName));

                    isDirty = true;
                    if (mImageListView != null) {
                        mImageListView.thumbnailCache.Remove(mGuid, true);
                        mImageListView.metadataCache.Remove(mGuid);
                        mImageListView.metadataCache.Add(mGuid, Adaptor, mFileName,
                            (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.DetailsOnly));
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
        public Image ThumbnailImage {
            get {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                if (ThumbnailCacheState != CacheState.Cached) {
                    mImageListView.thumbnailCache.Add(Guid, mAdaptor, VirtualItemKey, mImageListView.ThumbnailSize,
                        mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                        (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly));
                }

                return mImageListView.thumbnailCache.GetImage(Guid, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, true);
            }
        }
        /// <summary>
        /// Gets or sets the draw order of the item.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets or sets the draw order of the item."), DefaultValue(0)]
        public int ZOrder { get { return mZOrder; } set { mZOrder = value; } }
        #endregion

        #region Shell Properties
        /// <summary>
        /// Gets the small shell icon of the image file represented by this item.
        /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets the small shell icon of the image file represented by this item.")]
        public Image SmallIcon {
            get {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                CacheState state = mImageListView.shellInfoCache.GetCacheState(extension);
                if (state == CacheState.Cached) {
                    return mImageListView.shellInfoCache.GetSmallIcon(extension);
                }
                else if (state == CacheState.Error) {
                    if (mImageListView.RetryOnError) {
                        mImageListView.shellInfoCache.Remove(extension);
                        mImageListView.shellInfoCache.Add(extension);
                    }
                    return null;
                }
                else {
                    mImageListView.shellInfoCache.Add(extension);
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets the large shell icon of the image file represented by this item.
        /// If the icon image is not cached, it will be added to the cache queue and null will be returned.
        /// </summary>
        [Category("Appearance"), Browsable(false), Description("Gets the large shell icon of the image file represented by this item.")]
        public Image LargeIcon {
            get {
                if (mImageListView == null)
                    throw new InvalidOperationException("Owner control is null.");

                CacheState state = mImageListView.shellInfoCache.GetCacheState(extension);
                if (state == CacheState.Cached) {
                    return mImageListView.shellInfoCache.GetLargeIcon(extension);
                }
                else if (state == CacheState.Error) {
                    if (mImageListView.RetryOnError) {
                        mImageListView.shellInfoCache.Remove(extension);
                        mImageListView.shellInfoCache.Add(extension);
                    }
                    return null;
                }
                else {
                    mImageListView.shellInfoCache.Add(extension);
                    return null;
                }
            }
        }
        /// <summary>
        /// Gets the last access date of the image file represented by this item.
        /// </summary>
        [Category("File Properties"), Browsable(true), Description("Gets the last access date of the image file represented by this item.")]
        public DateTime DateAccessed { get { UpdateFileInfo(); return mDateAccessed; } }
        /// <summary>
        /// Gets the creation date of the image file represented by this item.
        /// </summary>
        [Category("File Properties"), Browsable(true), Description("Gets the creation date of the image file represented by this item.")]
        public DateTime DateCreated { get { UpdateFileInfo(); return mDateCreated; } }
        /// <summary>
        /// Gets the modification date of the image file represented by this item.
        /// </summary>
        [Category("File Properties"), Browsable(true), Description("Gets the modification date of the image file represented by this item.")]
        public DateTime DateModified { get { UpdateFileInfo(); return mDateModified; } }
        /// <summary>
        /// Gets the shell type of the image file represented by this item.
        /// </summary>
        [Category("File Properties"), Browsable(true), Description("Gets the shell type of the image file represented by this item.")]
        public string FileType { get { UpdateFileInfo(); return mFileType; } }
        /// <summary>
        /// Gets the path of the image file represented by this item.
        /// </summary>        
        [Category("File Properties"), Browsable(true), Description("Gets the path of the image file represented by this item.")]
        public string FilePath { get { UpdateFileInfo(); return mFilePath; } }
        /// <summary>
        /// Gets file size in bytes.
        /// </summary>
        [Category("File Properties"), Browsable(true), Description("Gets file size in bytes.")]
        public long FileSize { get { UpdateFileInfo(); return mFileSize; } }
        #endregion

        #region Exif Properties
        /// <summary>
        /// Gets image dimensions.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets image dimensions.")]
        public Size Dimensions { get { UpdateFileInfo(); return mDimensions; } }
        /// <summary>
        /// Gets image resolution in pixels per inch.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets image resolution in pixels per inch.")]
        public SizeF Resolution { get { UpdateFileInfo(); return mResolution; } }
        /// <summary>
        /// Gets image description.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets image description.")]
        public string ImageDescription { get { UpdateFileInfo(); return mImageDescription; } }
        /// <summary>
        /// Gets the camera model.
        /// </summary>
        [Category("Camera Properties"), Browsable(true), Description("Gets the camera model.")]
        public string EquipmentModel { get { UpdateFileInfo(); return mEquipmentModel; } }
        /// <summary>
        /// Gets the date and time the image was taken.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets the date and time the image was taken.")]
        public DateTime DateTaken { get { UpdateFileInfo(); return mDateTaken; } }
        /// <summary>
        /// Gets the name of the artist.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets the name of the artist.")]
        public string Artist { get { UpdateFileInfo(); return mArtist; } }
        /// <summary>
        /// Gets image copyright information.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets image copyright information.")]
        public string Copyright { get { UpdateFileInfo(); return mCopyright; } }
        /// <summary>
        /// Gets the exposure time in seconds.
        /// </summary>
        [Category("Camera Properties"), Browsable(true), Description("Gets the exposure time in seconds.")]
        public float ExposureTime { get { UpdateFileInfo(); return mExposureTime; } }
        /// <summary>
        /// Gets the F number.
        /// </summary>
        [Category("Camera Properties"), Browsable(true), Description("Gets the F number.")]
        public float FNumber { get { UpdateFileInfo(); return mFNumber; } }
        /// <summary>
        /// Gets the ISO speed.
        /// </summary>
        [Category("Camera Properties"), Browsable(true), Description("Gets the ISO speed.")]
        public ushort ISOSpeed { get { UpdateFileInfo(); return mISOSpeed; } }
        /// <summary>
        /// Gets user comments.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets user comments.")]
        public string UserComment { get { UpdateFileInfo(); return mUserComment; } }
        /// <summary>
        /// Gets rating in percent between 0-99 (Windows specific).
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets rating in percent between 0-99.")]
        public ushort Rating { get { UpdateFileInfo(); return mRating; } }
        /// <summary>
        /// Gets the star rating between 0-5 (Windows specific).
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets the star rating between 0-5.")]
        public ushort StarRating { get { UpdateFileInfo(); return mStarRating; } }
        /// <summary>
        /// Gets the name of the application that created this file.
        /// </summary>
        [Category("Image Properties"), Browsable(true), Description("Gets the name of the application that created this file.")]
        public string Software { get { UpdateFileInfo(); return mSoftware; } }
        /// <summary>
        /// Gets focal length of the lens in millimeters.
        /// </summary>
        [Category("Camera Properties"), Browsable(true), Description("Gets focal length of the lens in millimeters.")]
        public float FocalLength { get { UpdateFileInfo(); return mFocalLength; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
        /// </summary>
        public ImageListViewItem() {
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

            //[IG_CHANGE] we don't use sub-items, don't alloc memory for 'em
            //subItems = new Dictionary<Guid, string>();

            groupOrder = 0;
            group = string.Empty;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="filename">The image filename representing the item.</param>
        /// <param name="text">Item text</param>
        public ImageListViewItem(string filename, string text)
            : this() {
            mFileName = filename;
            // [IG_CHANGE] use string cache
            extension = _stringCache.GetFromCache(Path.GetExtension(filename));
            if (!string.IsNullOrEmpty(text))
                // [IG_CHANGE] don't duplicate filename text = Path.GetFileName(filename);
                mText = text;
            mVirtualItemKey = null; //mFileName; [IG_CHANGE] don't duplicate filename
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="filename">The image filename representing the item.</param>
        public ImageListViewItem(string filename)
            : this(filename, string.Empty) {
            ;
        }
        /// <summary>
        /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="key">The key identifying this item.</param>
        /// <param name="text">Text of this item.</param>
        public ImageListViewItem(object key, string text)
            : this() {
            mVirtualItemKey = key;
            mText = text;
        }
        /// <summary>
        /// Initializes a new instance of a virtual <see cref="ImageListViewItem"/> class.
        /// </summary>
        /// <param name="key">The key identifying this item.</param>
        public ImageListViewItem(object key)
            : this(key, string.Empty) {
            ;
        }
        #endregion

        #region Instance Methods
        /// <summary>
        /// Begins editing the item.
        /// This method must be used while editing the item
        /// to prevent collisions with the cache manager.
        /// </summary>
        public void BeginEdit() {
            if (editing)
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
        public void EndEdit(bool update) {
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
        public void EndEdit() {
            EndEdit(true);
        }
        /// <summary>
        /// Updates item thumbnail and item details.
        /// </summary>
        public void Update() {
            isDirty = true;
            if (mImageListView != null) {
                mImageListView.thumbnailCache.Remove(mGuid, true);
                mImageListView.metadataCache.Remove(mGuid);
                mImageListView.metadataCache.Add(mGuid, mAdaptor, VirtualItemKey,
                (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.DetailsOnly));
                mImageListView.Refresh();
            }
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the custom column with the given index.
        /// </summary>
        /// <param name="index">Index of the custom column.</param>
        /// <returns>Sub item text text for the given custom column type.</returns>
        public string GetSubItemText(int index) {
            int i = 0;
            foreach (string val in subItems.Values) {
                if (i == index)
                    return val;
                i++;
            }

            throw new IndexOutOfRangeException();
        }
        /// <summary>
        /// Sets the sub item item text corresponding to the custom column with the given index.
        /// </summary>
        /// <param name="index">Index of the custom column.</param>
        /// <param name="text">New sub item text</param>
        public void SetSubItemText(int index, string text) {
            int i = 0;
            Guid found = Guid.Empty;
            foreach (Guid guid in subItems.Keys) {
                if (i == index) {
                    found = guid;
                    break;
                }

                i++;
            }

            if (found != Guid.Empty) {
                subItems[found] = text;
                if (mImageListView != null && mImageListView.IsItemVisible(mGuid))
                    mImageListView.Refresh();
            }
            else
                throw new IndexOutOfRangeException();
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the specified column type.
        /// </summary>
        /// <param name="type">The type of information to return.</param>
        /// <returns>Formatted text for the given column type.</returns>
        internal string GetSubItemText(ColumnType type) {
            switch (type) {
                case ColumnType.Custom:
                    throw new ArgumentException("Column type is ambiguous. You must access custom columns by index.", "type");
                case ColumnType.Name:
                    return Text;
                case ColumnType.FileName:
                    return FileName;
                case ColumnType.DateAccessed:
                    if (mDateAccessed == DateTime.MinValue)
                        return "";
                    else
                        return mDateAccessed.ToString("g");
                case ColumnType.DateCreated:
                    if (mDateCreated == DateTime.MinValue)
                        return "";
                    else
                        return mDateCreated.ToString("g");
                case ColumnType.DateModified:
                    if (mDateModified == DateTime.MinValue)
                        return "";
                    else
                        return mDateModified.ToString("g");
                case ColumnType.FilePath:
                    return mFilePath;
                case ColumnType.FileSize:
                    if (mFileSize == 0)
                        return "";
                    else
                        return Utility.FormatSize(mFileSize);
                case ColumnType.FileType:
                    if (!string.IsNullOrEmpty(mFileType))
                        return mFileType;
                    if (mImageListView != null) {
                        if (!string.IsNullOrEmpty(extension)) {
                            CacheState state = mImageListView.shellInfoCache.GetCacheState(extension);
                            if (state == CacheState.Cached) {
                                mFileType = mImageListView.shellInfoCache.GetFileType(extension);
                                return mFileType;
                            }
                            else if (state == CacheState.Error) {
                                mImageListView.shellInfoCache.Remove(extension);
                                mImageListView.shellInfoCache.Add(extension);
                                return "";
                            }
                            else {
                                mImageListView.shellInfoCache.Add(extension);
                                return "";
                            }
                        }
                        return "";
                    }
                    else
                        return "";
                case ColumnType.Dimensions:
                    if (mDimensions == Size.Empty)
                        return "";
                    else
                        return string.Format("{0} x {1}", mDimensions.Width, mDimensions.Height);
                case ColumnType.Resolution:
                    if (mResolution == SizeF.Empty)
                        return "";
                    else
                        return string.Format("{0} x {1}", mResolution.Width, mResolution.Height);
                case ColumnType.ImageDescription:
                    return mImageDescription;
                case ColumnType.EquipmentModel:
                    return mEquipmentModel;
                case ColumnType.DateTaken:
                    if (mDateTaken == DateTime.MinValue)
                        return "";
                    else
                        return mDateTaken.ToString("g");
                case ColumnType.Artist:
                    return mArtist;
                case ColumnType.Copyright:
                    return mCopyright;
                case ColumnType.ExposureTime:
                    if (mExposureTime < double.Epsilon)
                        return "";
                    else if (mExposureTime >= 1.0f)
                        return mExposureTime.ToString("f1");
                    else
                        return string.Format("1/{0:f0}", (1.0f / mExposureTime));
                case ColumnType.FNumber:
                    if (mFNumber < double.Epsilon)
                        return "";
                    else
                        return mFNumber.ToString("f1");
                case ColumnType.ISOSpeed:
                    if (mISOSpeed == 0)
                        return "";
                    else
                        return mISOSpeed.ToString();
                case ColumnType.UserComment:
                    return mUserComment;
                case ColumnType.Rating:
                    if (mRating == 0)
                        return "";
                    else return mRating.ToString();
                case ColumnType.Software:
                    return mSoftware;
                case ColumnType.FocalLength:
                    if (mFocalLength < double.Epsilon)
                        return "";
                    else
                        return mFocalLength.ToString("f1");
                default:
                    throw new ArgumentException("Unknown column type", "type");
            }
        }
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString() {
            if (!string.IsNullOrEmpty(mText))
                return mText;
            if (!string.IsNullOrEmpty(mFileName))
                return Path.GetFileName(mFileName);
            return $"Item {mIndex}";
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Gets the simple rating (0-5)
        /// </summary>
        /// <returns></returns>
        internal ushort GetSimpleRating() {
            return mStarRating;
        }
        /// <summary>
        /// Sets the simple rating (0-5) from rating (0-99).
        /// </summary>
        private void UpdateRating() {
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
        internal Image GetCachedImage(CachedImageType imageType) {
            if (mImageListView == null)
                throw new InvalidOperationException("Owner control is null.");

            if (imageType == CachedImageType.SmallIcon || imageType == CachedImageType.LargeIcon) {
                if (string.IsNullOrEmpty(extension))
                    return mImageListView.DefaultImage;

                CacheState state = mImageListView.shellInfoCache.GetCacheState(extension);
                if (state == CacheState.Cached) {
                    if (imageType == CachedImageType.SmallIcon)
                        return mImageListView.shellInfoCache.GetSmallIcon(extension);
                    else
                        return mImageListView.shellInfoCache.GetLargeIcon(extension);
                }
                else if (state == CacheState.Error) {
                    if (mImageListView.RetryOnError) {
                        mImageListView.shellInfoCache.Remove(extension);
                        mImageListView.shellInfoCache.Add(extension);
                    }
                    return mImageListView.ErrorImage;
                }
                else {
                    mImageListView.shellInfoCache.Add(extension);
                    return mImageListView.DefaultImage;
                }
            }
            else {
                Image img = null;
                CacheState state = ThumbnailCacheState;

                if (state == CacheState.Error) {
                    if (string.IsNullOrEmpty(extension))
                        return mImageListView.ErrorImage;

                    if (mImageListView.ShellIconFallback && mImageListView.ThumbnailSize.Width > 32 && mImageListView.ThumbnailSize.Height > 32)
                        img = mImageListView.shellInfoCache.GetLargeIcon(extension);
                    if (img == null && mImageListView.ShellIconFallback)
                        img = mImageListView.shellInfoCache.GetSmallIcon(extension);
                    if (img == null)
                        img = mImageListView.ErrorImage;
                    return img;
                }

                img = mImageListView.thumbnailCache.GetImage(Guid, mImageListView.ThumbnailSize, mImageListView.UseEmbeddedThumbnails,
                    mImageListView.AutoRotateThumbnails, mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, false);

                if (state == CacheState.Cached)
                    return img;

                mImageListView.thumbnailCache.Add(Guid, mAdaptor, VirtualItemKey, mImageListView.ThumbnailSize,
                    mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                    (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly));

                if (img == null && string.IsNullOrEmpty(extension))
                    return mImageListView.DefaultImage;

                if (img == null && mImageListView.ShellIconFallback && mImageListView.ThumbnailSize.Width > 16 && mImageListView.ThumbnailSize.Height > 16)
                    img = mImageListView.shellInfoCache.GetLargeIcon(extension);
                if (img == null && mImageListView.ShellIconFallback)
                    img = mImageListView.shellInfoCache.GetSmallIcon(extension);
                if (img == null)
                    img = mImageListView.DefaultImage;

                return img;
            }
        }
        /// <summary>
        /// Adds a new subitem for the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        internal void AddSubItemText(Guid guid) {
            subItems.Add(guid, "");
        }
        /// <summary>
        /// Returns the sub item item text corresponding to the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        /// <returns>Formatted text for the given column.</returns>
        internal string GetSubItemText(Guid guid) {
            return subItems[guid];
        }
        /// <summary>
        /// Sets the sub item item text corresponding to the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        /// <param name="text">The text of the subitem.</param>
        /// <returns>Formatted text for the given column.</returns>
        internal void SetSubItemText(Guid guid, string text) {
            subItems[guid] = text;
        }
        /// <summary>
        /// Removes the sub item item text corresponding to the specified custom column.
        /// </summary>
        /// <param name="guid">The Guid of the custom column.</param>
        /// <returns>true if the item was removed; otherwise false.</returns>
        internal bool RemoveSubItemText(Guid guid) {
            return subItems.Remove(guid);
        }
        /// <summary>
        /// Removes all sub item item texts.
        /// </summary>
        internal void RemoveAllSubItemTexts() {
            subItems.Clear();
        }
        /// <summary>
        /// Updates file info for the image file represented by this item.
        /// Item details will be updated synchronously without waiting for the
        /// cache thread.
        /// </summary>
        private void UpdateFileInfo() {
            if (!isDirty) return;

            if (mImageListView != null) {
                UpdateDetailsInternal(Adaptor.GetDetails(VirtualItemKey,
                    (mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.DetailsOnly)));
            }
        }
        /// <summary>
        /// Invoked by the worker thread to update item details.
        /// </summary>
        /// <param name="info">Item details.</param>
        internal void UpdateDetailsInternal(Utility.Tuple<ColumnType, string, object>[] info) {
            if (!isDirty) return;

            // File info
            foreach (Utility.Tuple<ColumnType, string, object> item in info) {
                switch (item.Item1) {
                    case ColumnType.DateAccessed:
                        mDateAccessed = (DateTime)item.Item3;
                        break;
                    case ColumnType.DateCreated:
                        mDateCreated = (DateTime)item.Item3;
                        break;
                    case ColumnType.DateModified:
                        mDateModified = (DateTime)item.Item3;
                        break;
                    case ColumnType.FileSize:
                        mFileSize = (long)item.Item3;
                        break;
                    case ColumnType.FilePath:
                        mFilePath = (string)item.Item3;
                        break;
                    case ColumnType.Dimensions:
                        mDimensions = (Size)item.Item3;
                        break;
                    case ColumnType.Resolution:
                        mResolution = (SizeF)item.Item3;
                        break;
                    case ColumnType.ImageDescription:
                        mImageDescription = (string)item.Item3;
                        break;
                    case ColumnType.EquipmentModel:
                        mEquipmentModel = (string)item.Item3;
                        break;
                    case ColumnType.DateTaken:
                        mDateTaken = (DateTime)item.Item3;
                        break;
                    case ColumnType.Artist:
                        mArtist = (string)item.Item3;
                        break;
                    case ColumnType.Copyright:
                        mCopyright = (string)item.Item3;
                        break;
                    case ColumnType.ExposureTime:
                        mExposureTime = (float)item.Item3;
                        break;
                    case ColumnType.FNumber:
                        mFNumber = (float)item.Item3;
                        break;
                    case ColumnType.ISOSpeed:
                        mISOSpeed = (ushort)item.Item3;
                        break;
                    case ColumnType.UserComment:
                        mUserComment = (string)item.Item3;
                        break;
                    case ColumnType.Rating:
                        mRating = (ushort)item.Item3;
                        break;
                    case ColumnType.Software:
                        mSoftware = (string)item.Item3;
                        break;
                    case ColumnType.FocalLength:
                        mFocalLength = (float)item.Item3;
                        break;
                    case ColumnType.Custom:
                        string label = item.Item2;
                        string value = (string)item.Item3;
                        Guid columnID = Guid.Empty;
                        foreach (ImageListView.ImageListViewColumnHeader column in mImageListView.Columns) {
                            if (label == column.Text)
                                columnID = column.Guid;
                        }
                        if (columnID == Guid.Empty) {
                            ImageListView.ImageListViewColumnHeader column = new ImageListView.ImageListViewColumnHeader(ColumnType.Custom, label);
                            columnID = column.Guid;
                        }
                        if (subItems.ContainsKey(columnID))
                            subItems[columnID] = value;
                        else
                            subItems.Add(columnID, value);
                        break;
                    default:
                        throw new Exception("Unknown column type.");
                }
            }

            UpdateRating();

            isDirty = false;
        }
        /// <summary>
        /// Updates group order and name of the item.
        /// </summary>
        /// <param name="column">The group column.</param>
        internal void UpdateGroup(ImageListView.ImageListViewColumnHeader column) {
            if (column == null) {
                groupOrder = 0;
                group = string.Empty;
                return;
            }

            Utility.Tuple<int, string> groupInfo;

            switch (column.Type) {
                case ColumnType.DateAccessed:
                    groupInfo = Utility.GroupTextDate(DateAccessed);
                    break;
                case ColumnType.DateCreated:
                    groupInfo = Utility.GroupTextDate(DateCreated);
                    break;
                case ColumnType.DateModified:
                    groupInfo = Utility.GroupTextDate(DateModified);
                    break;
                case ColumnType.Dimensions:
                    groupInfo = Utility.GroupTextDimension(Dimensions);
                    break;
                case ColumnType.FileName:
                    groupInfo = Utility.GroupTextAlpha(FileName);
                    break;
                case ColumnType.FilePath:
                    groupInfo = Utility.GroupTextAlpha(FilePath);
                    break;
                case ColumnType.FileSize:
                    groupInfo = Utility.GroupTextFileSize(FileSize);
                    break;
                case ColumnType.FileType:
                    groupInfo = Utility.GroupTextAlpha(FileType);
                    break;
                case ColumnType.Name:
                    groupInfo = Utility.GroupTextAlpha(Text);
                    break;
                case ColumnType.ImageDescription:
                    groupInfo = Utility.GroupTextAlpha(ImageDescription);
                    break;
                case ColumnType.EquipmentModel:
                    groupInfo = Utility.GroupTextAlpha(EquipmentModel);
                    break;
                case ColumnType.DateTaken:
                    groupInfo = Utility.GroupTextDate(DateTaken);
                    break;
                case ColumnType.Artist:
                    groupInfo = Utility.GroupTextAlpha(Artist);
                    break;
                case ColumnType.Copyright:
                    groupInfo = Utility.GroupTextAlpha(Copyright);
                    break;
                case ColumnType.UserComment:
                    groupInfo = Utility.GroupTextAlpha(UserComment);
                    break;
                case ColumnType.Software:
                    groupInfo = Utility.GroupTextAlpha(Software);
                    break;
                case ColumnType.Custom:
                    groupInfo = Utility.GroupTextAlpha(GetSubItemText(column.Guid));
                    break;
                case ColumnType.ISOSpeed:
                    groupInfo = new Utility.Tuple<int, string>(ISOSpeed, ISOSpeed.ToString());
                    break;
                case ColumnType.Rating:
                    groupInfo = new Utility.Tuple<int, string>(Rating / 5, (Rating / 5).ToString());
                    break;
                case ColumnType.FocalLength:
                    groupInfo = new Utility.Tuple<int, string>((int)FocalLength, FocalLength.ToString());
                    break;
                case ColumnType.ExposureTime:
                    groupInfo = new Utility.Tuple<int, string>((int)ExposureTime, ExposureTime.ToString());
                    break;
                case ColumnType.FNumber:
                    groupInfo = new Utility.Tuple<int, string>((int)FNumber, FNumber.ToString());
                    break;
                case ColumnType.Resolution:
                    groupInfo = new Utility.Tuple<int, string>((int)Resolution.Width, Resolution.Width.ToString());
                    break;
                default:
                    groupInfo = new Utility.Tuple<int, string>(0, "Unknown");
                    break;
            }

            groupOrder = groupInfo.Item1;
            group = groupInfo.Item2;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone() {
            ImageListViewItem item = new ImageListViewItem();

            item.mText = mText;

            // File info
            item.extension = extension;
            item.mDateAccessed = mDateAccessed;
            item.mDateCreated = mDateCreated;
            item.mDateModified = mDateModified;
            item.mFileType = mFileType;
            item.mFileName = mFileName;
            item.mFilePath = mFilePath;
            item.mFileSize = mFileSize;

            // Image info
            item.mDimensions = mDimensions;
            item.mResolution = mResolution;

            // Exif tags
            item.mImageDescription = mImageDescription;
            item.mEquipmentModel = mEquipmentModel;
            item.mDateTaken = mDateTaken;
            item.mArtist = mArtist;
            item.mCopyright = mCopyright;
            item.mExposureTime = mExposureTime;
            item.mFNumber = mFNumber;
            item.mISOSpeed = mISOSpeed;
            item.mUserComment = mUserComment;
            item.mRating = mRating;
            item.mStarRating = mStarRating;
            item.mSoftware = mSoftware;
            item.mFocalLength = mFocalLength;

            // Virtual item properties
            item.mAdaptor = mAdaptor;
            item.mVirtualItemKey = mVirtualItemKey;

            // Sub items
            foreach (KeyValuePair<Guid, string> kv in subItems)
                item.subItems.Add(kv.Key, kv.Value);

            // Current thumbnail
            if (mImageListView != null) {
                item.clonedThumbnail = mImageListView.thumbnailCache.GetImage(Guid, mImageListView.ThumbnailSize,
                    mImageListView.UseEmbeddedThumbnails, mImageListView.AutoRotateThumbnails,
                    mImageListView.UseWIC == UseWIC.Auto || mImageListView.UseWIC == UseWIC.ThumbnailsOnly, true);
            }

            return item;
        }
        #endregion
    }
}

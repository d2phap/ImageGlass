namespace ImageGlass.Gallery;


#region ImageListView Public Enums
/// <summary>
/// Represents the cache mode.
/// </summary>
public enum CacheMode
{
    /// <summary>
    /// Item thumbnails will be generated only when requested.
    /// </summary>
    OnDemand,
    /// <summary>
    /// Item thumbnails will be continuously generated. Setting
    /// the CacheMode to Continuous disables the CacheLimit.
    /// </summary>
    Continuous,
}

/// <summary>
/// Represents the type of image in the cache manager.
/// </summary>
public enum CachedImageType
{
    /// <summary>
    /// Thumbnail image.
    /// </summary>
    Thumbnail,
    /// <summary>
    /// Small shell icon.
    /// </summary>
    SmallIcon,
    /// <summary>
    /// Large shell icon.
    /// </summary>
    LargeIcon,
}

/// <summary>
/// Represents the cache state of a thumbnail image.
/// </summary>
public enum CacheState
{
    /// <summary>
    /// The item is either not cached or it is in the cache queue.
    /// </summary>
    Unknown,
    /// <summary>
    /// Item thumbnail is cached.
    /// </summary>
    Cached,
    /// <summary>
    /// An error occurred while creating the item thumbnail.
    /// </summary>
    Error,
}

/// <summary>
/// Represents the cache thread.
/// </summary>
public enum CacheThread
{
    /// <summary>
    /// The cache thread responsible for generating item image thumbnails.
    /// </summary>
    Thumbnail,
    /// <summary>
    /// The cache thread responsible for generating item details.
    /// </summary>
    Details,
    /// <summary>
    /// The cache thread responsible for generating shell information.
    /// </summary>
    ShellInfo,
}


/// <summary>
/// Represents the type of information displayed in an image list view column.
/// </summary>
public enum ColumnType
{
    /// <summary>
    /// A custom text column.
    /// </summary>
    Custom,
    /// <summary>
    /// The text of the item, defaults to filename if
    /// the text is not provided.
    /// </summary>
    Name,
    
    FileType,
    /// <summary>
    /// The full path to the file.
    /// </summary>
    FileName,
    /// <summary>
    /// The path to the folder containing the file.
    /// </summary>
    FilePath,
    /// <summary>
    /// The name of the folder containing the file.
    /// </summary>
    FolderName,
    /// <summary>
    /// The size of the file.
    /// </summary>
}
/// <summary>
/// Represents the order by which items are drawn.
/// </summary>
public enum ItemDrawOrder
{
    /// <summary>
    /// Draw order is determined by item insertion index.
    /// </summary>
    ItemIndex,
    /// <summary>
    /// Draw order is determined by the ZOrder properties of items.
    /// </summary>
    ZOrder,
    /// <summary>
    /// Hovered items are drawn first, followed by normal items and selected items.
    /// </summary>
    HoveredNormalSelected,
    /// <summary>
    /// Hovered items are drawn first, followed by selected items and normal items.
    /// </summary>
    HoveredSelectedNormal,
    /// <summary>
    /// Normal items are drawn first, followed by hovered items and selected items.
    /// </summary>
    NormalHoveredSelected,
    /// <summary>
    /// Normal items are drawn first, followed by selected items and hovered items.
    /// </summary>
    NormalSelectedHovered,
    /// <summary>
    /// Selected items are drawn first, followed by hovered items and normal items.
    /// </summary>
    SelectedHoveredNormal,
    /// <summary>
    /// Selected items are drawn first, followed by normal items and hovered items.
    /// </summary>
    SelectedNormalHovered,
}
/// <summary>
/// Represents the visual state of an image list view item.
/// </summary>
[Flags]
public enum ItemState
{
    /// <summary>
    /// The item is neither selected nor hovered.
    /// </summary>
    None = 0,
    /// <summary>
    /// The item is selected.
    /// </summary>
    Selected = 1,
    /// <summary>
    /// The item has the input focus.
    /// </summary>
    Focused = 2,
    /// <summary>
    /// Mouse cursor is over the item.
    /// </summary>
    Hovered = 4,
    /// <summary>
    /// The item is disabled.
    /// </summary>
    Disabled = 8,
}
/// <summary>
/// Determines the visibility of an item.
/// </summary>
public enum ItemVisibility
{
    /// <summary>
    /// The item is not visible.
    /// </summary>
    NotVisible,
    /// <summary>
    /// The item is partially visible.
    /// </summary>
    PartiallyVisible,
    /// <summary>
    /// The item is fully visible.
    /// </summary>
    Visible,
}
/// <summary>
/// Represents the embedded thumbnail extraction behavior.
/// </summary>
public enum UseEmbeddedThumbnails
{
    /// <summary>
    /// Always creates the thumbnail from the embedded thumbnail.
    /// </summary>
    Always = 0,
    /// <summary>
    /// Creates the thumbnail from the embedded thumbnail when possible,
    /// reverts to the source image otherwise.
    /// </summary>
    Auto = 1,
    /// <summary>
    /// Always creates the thumbnail from the source image.
    /// </summary>
    Never = 2,
}
/// <summary>
/// Represents the view mode of the image list view.
/// </summary>
public enum View
{
    /// <summary>
    /// Displays thumbnails laid out in a grid. The view can be 
    /// scrolled vertically.
    /// </summary>
    Thumbnails,
    /// <summary>
    /// Displays a single row of thumbnails.
    /// The view can be scrolled horizontally.
    /// </summary>
    HorizontalStrip,
    /// <summary>
    /// Displays a single column of thumbnails.
    /// The view can be scrolled vertically.
    /// </summary>
    VerticalStrip,
}
#endregion


#region Internal Enums
/// <summary>
/// Represents the item highlight state during mouse selection.
/// </summary>
internal enum ItemHighlightState
{
    /// <summary>
    /// Item is not highlighted.
    /// </summary>
    NotHighlighted,
    /// <summary>
    /// Item is highlighted and will be removed from the selection set.
    /// </summary>
    HighlightedAndUnSelected,
    /// <summary>
    /// Item is highlighted and will be added to the selection set.
    /// </summary>
    HighlightedAndSelected,
}
#endregion


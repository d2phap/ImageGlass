
namespace ImageGlass.Gallery;


/// <summary>
/// Represents a class that supplies group information for items.
/// </summary>
public interface IGrouper
{
    #region Instance Methods
    /// <summary>
    /// Supplies grouping information for the given item.
    /// </summary>
    /// <param name="item">The item that requesting grouping information.</param>
    GroupInfo GetGroupInfo(ImageListViewItem item);
    #endregion
}


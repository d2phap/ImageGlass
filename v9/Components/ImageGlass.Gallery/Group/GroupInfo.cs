
namespace ImageGlass.Gallery;

/// <summary>
/// Represents ordering and name information for a group.
/// </summary>
public struct GroupInfo
{
    #region Properties
    /// <summary>
    /// The order of group. A group with a lower order number will be displayed above other groups
    /// when sorted in ascending order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The display name of the group. Items with the same group name will be collected and displayed 
    /// under the same group.
    /// </summary>
    public string Name { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the GroupInfo class.
    /// </summary>
    /// <param name="name">Display name of the group.</param>
    /// <param name="order">Order of the group.</param>
    public GroupInfo(string name, int order)
    {
        Name = name;
        Order = order;
    }
    #endregion
}

namespace ImageGlass.Gallery;


/// <summary>
/// Represents the details of a mouse hit test.
/// </summary>
public class HitInfo
{

    #region Properties
    /// <summary>
    /// Gets whether an item is under the hit point.
    /// </summary>
    public bool ItemHit { get { return ItemIndex != -1; } }

    /// <summary>
    /// Gets whether an item checkbox is under the hit point.
    /// </summary>
    public bool CheckBoxHit { get; private set; }

    /// <summary>
    /// Gets whether the file icon is under the hit point.
    /// </summary>
    public bool FileIconHit { get; private set; }

    /// <summary>
    /// Gets the index of the item under the hit point.
    /// </summary>
    public int ItemIndex { get; private set; }

    /// <summary>
    /// Gets whether the hit point is inside the item area.
    /// </summary>
    public bool InItemArea { get; private set; }

    #endregion


    #region Constructor
    /// <summary>
    /// Initializes a new instance of the HitInfo class.
    /// </summary>
    /// <param name="itemIndex">Index of the item.</param>
    /// <param name="checkBoxHit">if set to true the mouse cursor is over a checkbox.</param>
    /// <param name="fileIconHit">if set to true the mouse cursor is over a file icon.</param>
    /// <param name="inItemArea">if set to true the mouse is in the item area.</param>
    private HitInfo(int itemIndex, bool checkBoxHit, bool fileIconHit, bool inItemArea)
    {
        ItemIndex = itemIndex;
        CheckBoxHit = checkBoxHit;
        FileIconHit = fileIconHit;

        InItemArea = inItemArea;
    }
    /// <summary>
    /// Initializes a new instance of the HitInfo class.
    /// Used when the control registered an item hit.
    /// </summary>
    /// <param name="itemIndex">Index of the item.</param>
    /// <param name="subItemIndex">Index of the sub item.</param>
    /// <param name="checkBoxHit">if set to true the mouse cursor is over a checkbox.</param>
    /// <param name="fileIconHit">if set to true the mouse cursor is over a file icon.</param>
    internal HitInfo(int itemIndex, int subItemIndex, bool checkBoxHit, bool fileIconHit)
        : this(itemIndex, checkBoxHit, fileIconHit, true)
    {
        ;
    }
    #endregion

}

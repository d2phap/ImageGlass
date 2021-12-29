
using System.ComponentModel;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents a sub item in the image list view.
/// </summary>
public class ImageListViewSubItem
{
    #region Member Variables
    // Property backing fields
    private string mText;
    private ImageListViewItem mParent;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the text associated with this sub item.
    /// </summary>
    [Category("Appearance")]
    [Description("Gets or sets the text associated with this sub item.")]
    [Browsable(true)]
    public string Text
    {
        get
        {
            return mText;
        }
        set
        {
            mText = value;
            if (mParent != null && mParent.ImageListView != null && mParent.ImageListView.IsItemVisible(mParent.Guid))
                mParent.ImageListView.Refresh();
        }
    }
    #endregion
    

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageListViewSubItem"/> class.
    /// </summary>
    /// <param name="parent">The parent item.</param>
    /// <param name="text">Sub item text.</param>
    public ImageListViewSubItem(ImageListViewItem parent, string text)
    {
        mParent = parent;
        mText = text;
    }
    #endregion
}


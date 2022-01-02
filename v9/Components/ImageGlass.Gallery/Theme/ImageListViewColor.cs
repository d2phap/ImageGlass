using System.ComponentModel;
using System.Reflection;

namespace ImageGlass.Gallery;


/// <summary>
/// Represents the color palette of the image list view.
/// </summary>
public class ImageListViewColor
{
    #region Member Variables
    // control background color
    Color mControlBackColor;
    Color mDisabledBackColor;

    // item colors
    Color mBackColor;
    Color mBorderColor;
    Color mUnFocusedColor1;
    Color mUnFocusedColor2;
    Color mUnFocusedBorderColor;
    Color mUnFocusedForeColor;
    Color mForeColor;
    Color mHoverColor1;
    Color mHoverColor2;
    Color mHoverBorderColor;
    Color mInsertionCaretColor;
    Color mPressedColor1;
    Color mPressedColor2;
    Color mPressedBorderColor;
    Color mPressedForeColor;
    Color mSelectedColor1;
    Color mSelectedColor2;
    Color mSelectedBorderColor;
    Color mSelectedForeColor;
    Color mDisabledColor1;
    Color mDisabledColor2;
    Color mDisabledBorderColor;
    Color mDisabledForeColor;

    // thumbnail & pane
    Color mImageInnerBorderColor;
    Color mImageOuterBorderColor;

    // details view
    Color mCellForeColor;
    Color mColumnHeaderBackColor1;
    Color mColumnHeaderBackColor2;
    Color mColumnHeaderForeColor;
    Color mColumnHeaderHoverColor1;
    Color mColumnHeaderHoverColor2;
    Color mColumnSelectColor;
    Color mColumnSeparatorColor;
    Color mAlternateBackColor;
    Color mAlternateCellForeColor;

    // selection rectangle
    Color mSelectionRectangleColor1;
    Color mSelectionRectangleColor2;
    Color mSelectionRectangleBorderColor;
    #endregion


    #region Properties
    /// <summary>
    /// Gets or sets the background color of the ImageListView control.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background color of the ImageListView control.")]
    [DefaultValue(typeof(Color), "Window")]
    public Color ControlBackColor
    {
        get => mControlBackColor;
        set { mControlBackColor = value; }
    }

    /// <summary>
    /// Gets or sets the background color of the ImageListView control in its disabled state.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background color of the ImageListView control in its disabled state.")]
    [DefaultValue(typeof(Color), "Control")]
    public Color DisabledBackColor
    {
        get => mDisabledBackColor;
        set { mDisabledBackColor = value; }
    }

    /// <summary>
    /// Gets or sets the background color of the ImageListViewItem.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background color of the ImageListViewItem.")]
    [DefaultValue(typeof(Color), "Window")]
    public Color BackColor
    {
        get => mBackColor;
        set { mBackColor = value; }
    }

    /// <summary>
    /// Gets or sets the background color of alternating cells in Details View.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the background color of alternating cells in Details View.")]
    [DefaultValue(typeof(Color), "Window")]
    public Color AlternateBackColor
    {
        get => mAlternateBackColor;
        set { mAlternateBackColor = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem.")]
    [DefaultValue(typeof(Color), "64, 128, 128, 128")]
    public Color BorderColor
    {
        get => mBorderColor;
        set { mBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the foreground color of the ImageListViewItem.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the foreground color of the ImageListViewItem.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color ForeColor
    {
        get => mForeColor;
        set { mForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 of the ImageListViewItem if the control is not focused.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color1 of the ImageListViewItem if the control is not focused.")]
    [DefaultValue(typeof(Color), "16, 128, 128, 128")]
    public Color UnFocusedColor1
    {
        get => mUnFocusedColor1;
        set { mUnFocusedColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 of the ImageListViewItem if the control is not focused.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color2 of the ImageListViewItem if the control is not focused.")]
    [DefaultValue(typeof(Color), "64, 128, 128, 128")]
    public Color UnFocusedColor2
    {
        get => mUnFocusedColor2;
        set { mUnFocusedColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem if the control is not focused.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem if the control is not focused.")]
    [DefaultValue(typeof(Color), "128, 128, 128, 128")]
    public Color UnFocusedBorderColor
    {
        get => mUnFocusedBorderColor;
        set { mUnFocusedBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the fore color of the ImageListViewItem if the control is not focused.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the fore color of the ImageListViewItem if the control is not focused.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color UnFocusedForeColor
    {
        get => mUnFocusedForeColor;
        set { mUnFocusedForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 if the ImageListViewItem is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color1 if the ImageListViewItem is hovered.")]
    [DefaultValue(typeof(Color), "8, 10, 36, 106")]
    public Color HoverColor1
    {
        get => mHoverColor1;
        set { mHoverColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 if the ImageListViewItem is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color2 if the ImageListViewItem is hovered.")]
    [DefaultValue(typeof(Color), "64, 10, 36, 106")]
    public Color HoverColor2
    {
        get => mHoverColor2;
        set { mHoverColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem if the item is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem if the item is hovered.")]
    [DefaultValue(typeof(Color), "64, 10, 36, 106")]
    public Color HoverBorderColor
    {
        get => mHoverBorderColor;
        set { mHoverBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the color of the insertion caret.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the color of the insertion caret.")]
    [DefaultValue(typeof(Color), "Highlight")]
    public Color InsertionCaretColor
    {
        get => mInsertionCaretColor;
        set { mInsertionCaretColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 if the ImageListViewItem is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color1 if the ImageListViewItem is pressed.")]
    [DefaultValue(typeof(Color), "8, 10, 36, 106")]
    public Color PressedColor1
    {
        get => mPressedColor1;
        set { mPressedColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 if the ImageListViewItem is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color2 if the ImageListViewItem is pressed.")]
    [DefaultValue(typeof(Color), "40, 10, 36, 106")]
    public Color PressedColor2
    {
        get => mPressedColor2;
        set { mPressedColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the fore color of the ImageListViewItem if the item is selected.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the fore color of the ImageListViewItem if the item is pressed.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color PressedForeColor
    {
        get => mPressedForeColor;
        set { mPressedForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem if the item is hovered.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem if the item is pressed.")]
    [DefaultValue(typeof(Color), "40, 10, 36, 106")]
    public Color PressedBorderColor
    {
        get => mPressedBorderColor;
        set { mPressedBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 if the ImageListViewItem is selected.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color1 if the ImageListViewItem is selected.")]
    [DefaultValue(typeof(Color), "16, 10, 36, 106")]
    public Color SelectedColor1
    {
        get => mSelectedColor1;
        set { mSelectedColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 if the ImageListViewItem is selected.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color2 if the ImageListViewItem is selected.")]
    [DefaultValue(typeof(Color), "128, 10, 36, 106")]
    public Color SelectedColor2
    {
        get => mSelectedColor2;
        set { mSelectedColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem if the item is selected.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem if the item is selected.")]
    [DefaultValue(typeof(Color), "128, 10, 36, 106")]
    public Color SelectedBorderColor
    {
        get => mSelectedBorderColor;
        set { mSelectedBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the fore color of the ImageListViewItem if the item is selected.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the fore color of the ImageListViewItem if the item is selected.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color SelectedForeColor
    {
        get => mSelectedForeColor;
        set { mSelectedForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 if the ImageListViewItem is disabled.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color1 if the ImageListViewItem is disabled.")]
    [DefaultValue(typeof(Color), "0, 128, 128, 128")]
    public Color DisabledColor1
    {
        get => mDisabledColor1;
        set { mDisabledColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 if the ImageListViewItem is disabled.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background gradient color2 if the ImageListViewItem is disabled.")]
    [DefaultValue(typeof(Color), "32, 128, 128, 128")]
    public Color DisabledColor2
    {
        get => mDisabledColor2;
        set { mDisabledColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the border color of the ImageListViewItem if the item is disabled.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the border color of the ImageListViewItem if the item is disabled.")]
    [DefaultValue(typeof(Color), "32, 128, 128, 128")]
    public Color DisabledBorderColor
    {
        get => mDisabledBorderColor;
        set { mDisabledBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the fore color of the ImageListViewItem if the item is disabled.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the fore color of the ImageListViewItem if the item is disabled.")]
    [DefaultValue(typeof(Color), "128, 128, 128")]
    public Color DisabledForeColor
    {
        get => mDisabledForeColor;
        set { mDisabledForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color1 of the column header.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the cells background color1 of the column header.")]
    [DefaultValue(typeof(Color), "32, 212, 208, 200")]
    public Color ColumnHeaderBackColor1
    {
        get => mColumnHeaderBackColor1;
        set { mColumnHeaderBackColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background gradient color2 of the column header.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the cells background color2 of the column header.")]
    [DefaultValue(typeof(Color), "196, 212, 208, 200")]
    public Color ColumnHeaderBackColor2
    {
        get => mColumnHeaderBackColor2;
        set { mColumnHeaderBackColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the background hover gradient color1 of the column header.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the background hover color1 of the column header.")]
    [DefaultValue(typeof(Color), "16, 10, 36, 106")]
    public Color ColumnHeaderHoverColor1
    {
        get => mColumnHeaderHoverColor1;
        set { mColumnHeaderHoverColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background hover gradient color2 of the column header.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the background hover color2 of the column header.")]
    [DefaultValue(typeof(Color), "64, 10, 36, 106")]
    public Color ColumnHeaderHoverColor2
    {
        get => mColumnHeaderHoverColor2;
        set { mColumnHeaderHoverColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the cells foreground color of the column header text.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the cells foreground color of the column header text.")]
    [DefaultValue(typeof(Color), "WindowText")]
    public Color ColumnHeaderForeColor
    {
        get => mColumnHeaderForeColor;
        set { mColumnHeaderForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the cells background color if column is selected in Details View.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the cells background color if column is selected in Details View.")]
    [DefaultValue(typeof(Color), "16, 128, 128, 128")]
    public Color ColumnSelectColor
    {
        get => mColumnSelectColor;
        set { mColumnSelectColor = value; }
    }

    /// <summary>
    /// Gets or sets the color of the separator in Details View.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the color of the separator in Details View.")]
    [DefaultValue(typeof(Color), "32, 128, 128, 128")]
    public Color ColumnSeparatorColor
    {
        get => mColumnSeparatorColor;
        set { mColumnSeparatorColor = value; }
    }

    /// <summary>
    /// Gets or sets the foreground color of the cell text in Details View.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the foreground color of the cell text in Details View.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color CellForeColor
    {
        get => mCellForeColor;
        set { mCellForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the foreground color of alternating cells text in Details View.
    /// </summary>
    [Category("Appearance Details View"), Description("Gets or sets the foreground color of alternating cells text in Details View.")]
    [DefaultValue(typeof(Color), "ControlText")]
    public Color AlternateCellForeColor
    {
        get => mAlternateCellForeColor;
        set { mAlternateCellForeColor = value; }
    }

    /// <summary>
    /// Gets or sets the image inner border color for thumbnails and pane.
    /// </summary>
    [Category("Appearance Image"), Description("Gets or sets the image inner border color for thumbnails and pane.")]
    [DefaultValue(typeof(Color), "128, 255, 255, 255")]
    public Color ImageInnerBorderColor
    {
        get => mImageInnerBorderColor;
        set { mImageInnerBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the image outer border color for thumbnails and pane.
    /// </summary>
    [Category("Appearance Image"), Description("Gets or sets the image outer border color for thumbnails and pane.")]
    [DefaultValue(typeof(Color), "128, 128, 128, 128")]
    public Color ImageOuterBorderColor
    {
        get => mImageOuterBorderColor;
        set { mImageOuterBorderColor = value; }
    }

    /// <summary>
    /// Gets or sets the background color1 of the selection rectangle.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background color1 of the selection rectangle.")]
    [DefaultValue(typeof(Color), "128, 10, 36, 106")]
    public Color SelectionRectangleColor1
    {
        get => mSelectionRectangleColor1;
        set { mSelectionRectangleColor1 = value; }
    }

    /// <summary>
    /// Gets or sets the background color2 of the selection rectangle.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the background color2 of the selection rectangle.")]
    [DefaultValue(typeof(Color), "128, 10, 36, 106")]
    public Color SelectionRectangleColor2
    {
        get => mSelectionRectangleColor2;
        set { mSelectionRectangleColor2 = value; }
    }

    /// <summary>
    /// Gets or sets the color of the selection rectangle border.
    /// </summary>
    [Category("Appearance"), Description("Gets or sets the color of the selection rectangle border.")]
    [DefaultValue(typeof(Color), "Highlight")]
    public Color SelectionRectangleBorderColor
    {
        get => mSelectionRectangleBorderColor;
        set { mSelectionRectangleBorderColor = value; }
    }
    #endregion


    #region Constructors
    /// <summary>
    /// Initializes a new instance of the ImageListViewColor class.
    /// </summary>
    public ImageListViewColor()
    {
        // control
        mControlBackColor = SystemColors.Window;
        mDisabledBackColor = SystemColors.Control;

        // item
        mBackColor = SystemColors.Window;
        mForeColor = SystemColors.ControlText;

        mBorderColor = Color.FromArgb(64, SystemColors.GrayText);

        mUnFocusedColor1 = Color.FromArgb(16, SystemColors.GrayText);
        mUnFocusedColor2 = Color.FromArgb(64, SystemColors.GrayText);
        mUnFocusedBorderColor = Color.FromArgb(128, SystemColors.GrayText);
        mUnFocusedForeColor = SystemColors.ControlText;

        mHoverColor1 = Color.FromArgb(8, SystemColors.Highlight);
        mHoverColor2 = Color.FromArgb(64, SystemColors.Highlight);
        mHoverBorderColor = Color.FromArgb(64, SystemColors.Highlight);

        mSelectedColor1 = Color.FromArgb(16, SystemColors.Highlight);
        mSelectedColor2 = Color.FromArgb(128, SystemColors.Highlight);
        mSelectedBorderColor = Color.FromArgb(128, SystemColors.Highlight);
        mSelectedForeColor = SystemColors.ControlText;

        mDisabledColor1 = Color.FromArgb(0, SystemColors.GrayText);
        mDisabledColor2 = Color.FromArgb(32, SystemColors.GrayText);
        mDisabledBorderColor = Color.FromArgb(32, SystemColors.GrayText);
        mDisabledForeColor = Color.FromArgb(128, 128, 128);

        mInsertionCaretColor = SystemColors.Highlight;

        // thumbnails
        mImageInnerBorderColor = Color.FromArgb(128, Color.White);
        mImageOuterBorderColor = Color.FromArgb(128, Color.Gray);

        // details view
        mColumnHeaderBackColor1 = Color.FromArgb(32, SystemColors.Control);
        mColumnHeaderBackColor2 = Color.FromArgb(196, SystemColors.Control);
        mColumnHeaderHoverColor1 = Color.FromArgb(16, SystemColors.Highlight);
        mColumnHeaderHoverColor2 = Color.FromArgb(64, SystemColors.Highlight);
        mColumnHeaderForeColor = SystemColors.WindowText;
        mColumnSelectColor = Color.FromArgb(16, SystemColors.GrayText);
        mColumnSeparatorColor = Color.FromArgb(32, SystemColors.GrayText);
        mCellForeColor = SystemColors.ControlText;
        mAlternateBackColor = SystemColors.Window;
        mAlternateCellForeColor = SystemColors.ControlText;

        // selection rectangle
        mSelectionRectangleColor1 = Color.FromArgb(128, SystemColors.Highlight);
        mSelectionRectangleColor2 = Color.FromArgb(128, SystemColors.Highlight);
        mSelectionRectangleBorderColor = SystemColors.Highlight;
    }

    /// <summary>
    /// Initializes a new instance of the ImageListViewColor class
    /// from its string representation.
    /// </summary>
    /// <param name="definition">String representation of the object.</param>
    public ImageListViewColor(string definition) : this()
    {
        try
        {
            // First check if the color matches a predefined color setting
            foreach (var info in typeof(ImageListViewColor).GetMembers(BindingFlags.Static | BindingFlags.Public))
            {
                if (info.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyInfo = (PropertyInfo)info;
                    if (propertyInfo.PropertyType == typeof(ImageListViewColor))
                    {
                        // If the color setting is equal to a preset value
                        // return the preset
                        if (definition == string.Format("({0})", propertyInfo.Name) ||
                            definition == propertyInfo.Name)
                        {
                            var presetValue = (ImageListViewColor)propertyInfo.GetValue(null, null);
                            CopyFrom(presetValue);
                            return;
                        }
                    }
                }
            }

            // Convert color values
            foreach (var line in definition.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Read the color setting
                var pair = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                var name = pair[0].Trim();
                var color = Color.FromName(pair[1].Trim());
                // Set the property value
                var property = typeof(ImageListViewColor).GetProperty(name);
                property.SetValue(this, color, null);
            }
        }
        catch
        {
            throw new ArgumentException("Invalid string format", nameof(definition));
        }
    }
    #endregion


    #region Instance Methods
    /// <summary>
    /// Copies color values from the given object.
    /// </summary>
    /// <param name="source">The source object.</param>
    public void CopyFrom(ImageListViewColor source)
    {
        foreach (PropertyInfo info in typeof(ImageListViewColor).GetProperties())
        {
            // Walk through color properties
            if (info.PropertyType == typeof(Color))
            {
                Color color = (Color)info.GetValue(source, null);
                info.SetValue(this, color, null);
            }
        }
    }
    #endregion


    #region Static Members
    /// <summary>
    /// Represents the default color theme.
    /// </summary>
    public static ImageListViewColor Default { get { return GetDefaultTheme(); } }

    /// <summary>
    /// Represents the noir color theme.
    /// </summary>
    public static ImageListViewColor Noir { get { return GetNoirTheme(); } }

    /// <summary>
    /// Represents the mandarin color theme.
    /// </summary>
    public static ImageListViewColor Mandarin { get { return GetMandarinTheme(); } }

    /// <summary>
    /// Sets the color palette to default colors.
    /// </summary>
    private static ImageListViewColor GetDefaultTheme()
    {
        return new ImageListViewColor();
    }

    /// <summary>
    /// Sets the color palette to mandarin colors.
    /// </summary>
    private static ImageListViewColor GetMandarinTheme()
    {
        var c = new ImageListViewColor
        {

            // control
            ControlBackColor = Color.White,
            DisabledBackColor = Color.FromArgb(220, 220, 220),

            // item
            BackColor = Color.White,
            ForeColor = Color.FromArgb(60, 60, 60),
            BorderColor = Color.FromArgb(187, 190, 183),

            UnFocusedColor1 = Color.FromArgb(235, 235, 235),
            UnFocusedColor2 = Color.FromArgb(217, 217, 217),
            UnFocusedBorderColor = Color.FromArgb(168, 169, 161),
            UnFocusedForeColor = Color.FromArgb(40, 40, 40),

            HoverColor1 = Color.Transparent,
            HoverColor2 = Color.Transparent,
            HoverBorderColor = Color.Transparent,

            SelectedColor1 = Color.FromArgb(244, 125, 77),
            SelectedColor2 = Color.FromArgb(235, 110, 60),
            SelectedBorderColor = Color.FromArgb(240, 119, 70),
            SelectedForeColor = Color.White,

            DisabledColor1 = Color.FromArgb(217, 217, 217),
            DisabledColor2 = Color.FromArgb(197, 197, 197),
            DisabledBorderColor = Color.FromArgb(128, 128, 128),
            DisabledForeColor = Color.FromArgb(128, 128, 128),

            InsertionCaretColor = Color.FromArgb(240, 119, 70),

            // thumbnails & pane
            ImageInnerBorderColor = Color.Transparent,
            ImageOuterBorderColor = Color.White,

            // details view
            CellForeColor = Color.FromArgb(60, 60, 60),
            ColumnHeaderBackColor1 = Color.FromArgb(247, 247, 247),
            ColumnHeaderBackColor2 = Color.FromArgb(235, 235, 235),
            ColumnHeaderHoverColor1 = Color.White,
            ColumnHeaderHoverColor2 = Color.FromArgb(245, 245, 245),
            ColumnHeaderForeColor = Color.FromArgb(60, 60, 60),
            ColumnSelectColor = Color.FromArgb(34, 128, 128, 128),
            ColumnSeparatorColor = Color.FromArgb(106, 128, 128, 128),
            mAlternateBackColor = Color.FromArgb(234, 234, 234),
            mAlternateCellForeColor = Color.FromArgb(40, 40, 40),

            // selection rectangle
            SelectionRectangleColor1 = Color.FromArgb(64, 240, 116, 68),
            SelectionRectangleColor2 = Color.FromArgb(64, 240, 116, 68),
            SelectionRectangleBorderColor = Color.FromArgb(240, 119, 70)
        };

        return c;
    }

    /// <summary>
    /// Sets the color palette to noir colors.
    /// </summary>
    private static ImageListViewColor GetNoirTheme()
    {
        var c = new ImageListViewColor
        {

            // control
            ControlBackColor = Color.Black,
            DisabledBackColor = Color.Black,

            // item
            BackColor = Color.FromArgb(0x31, 0x31, 0x31),
            ForeColor = Color.LightGray,

            BorderColor = Color.DarkGray,

            UnFocusedColor1 = Color.FromArgb(16, SystemColors.GrayText),
            UnFocusedColor2 = Color.FromArgb(64, SystemColors.GrayText),
            UnFocusedBorderColor = Color.FromArgb(128, SystemColors.GrayText),
            UnFocusedForeColor = Color.LightGray,

            HoverColor1 = Color.FromArgb(64, Color.White),
            HoverColor2 = Color.FromArgb(16, Color.White),
            HoverBorderColor = Color.FromArgb(64, SystemColors.Highlight),

            SelectedColor1 = Color.FromArgb(64, 96, 160),
            SelectedColor2 = Color.FromArgb(64, 64, 96, 160),
            SelectedBorderColor = Color.FromArgb(128, SystemColors.Highlight),
            SelectedForeColor = Color.LightGray,

            DisabledColor1 = Color.FromArgb(0, SystemColors.GrayText),
            DisabledColor2 = Color.FromArgb(32, SystemColors.GrayText),
            DisabledBorderColor = Color.FromArgb(96, SystemColors.GrayText),
            DisabledForeColor = Color.LightGray,

            InsertionCaretColor = Color.FromArgb(96, 144, 240),

            // thumbnails & pane
            ImageInnerBorderColor = Color.FromArgb(128, Color.White),
            ImageOuterBorderColor = Color.FromArgb(128, Color.Gray),

            // details view
            CellForeColor = Color.WhiteSmoke,
            ColumnHeaderBackColor1 = Color.FromArgb(32, 128, 128, 128),
            ColumnHeaderBackColor2 = Color.FromArgb(196, 128, 128, 128),
            ColumnHeaderHoverColor1 = Color.FromArgb(64, 96, 144, 240),
            ColumnHeaderHoverColor2 = Color.FromArgb(196, 96, 144, 240),
            ColumnHeaderForeColor = Color.White,
            ColumnSelectColor = Color.FromArgb(96, 128, 128, 128),
            ColumnSeparatorColor = Color.Gold,
            AlternateBackColor = Color.FromArgb(0x31, 0x31, 0x31),
            AlternateCellForeColor = Color.WhiteSmoke,

            // selection rectangke
            SelectionRectangleColor1 = Color.FromArgb(160, 96, 144, 240),
            SelectionRectangleColor2 = Color.FromArgb(32, 96, 144, 240),
            SelectionRectangleBorderColor = Color.FromArgb(64, 96, 144, 240)
        };

        return c;
    }

    #endregion


    #region System.Object Overrides
    /// <summary>
    /// Determines whether all color values of the specified 
    /// ImageListViewColor are equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>true if the two instances have the same color values; 
    /// otherwise false.</returns>
    public override bool Equals(object obj)
    {
        if (obj == null)
            throw new NullReferenceException();

        var other = obj as ImageListViewColor;
        if (other == null) return false;

        foreach (var info in typeof(ImageListViewColor).GetProperties())
        {
            // Walk through color properties
            if (info.PropertyType == typeof(Color))
            {
                // Compare colors
                var color1 = (Color?)info.GetValue(this, null);
                var color2 = (Color?)info.GetValue(other, null);

                if (color1 != color2) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in 
    /// hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Returns a string that represents this instance.
    /// </summary>
    /// <returns>
    /// A string that represents this instance.
    /// </returns>
    public override string ToString()
    {
        var colors = this;

        // First check if the color matches a predefined color setting
        foreach (var info in typeof(ImageListViewColor).GetMembers(BindingFlags.Static | BindingFlags.Public))
        {
            if (info.MemberType == MemberTypes.Property)
            {
                var propertyInfo = (PropertyInfo)info;
                if (propertyInfo.PropertyType == typeof(ImageListViewColor))
                {
                    var presetValue = (ImageListViewColor?)propertyInfo.GetValue(null, null);
                    // If the color setting is equal to a preset value
                    // return the name of the preset
                    if (colors.Equals(presetValue))
                        return string.Format("({0})", propertyInfo.Name);
                }
            }
        }

        // Serialize all colors which are different from the default setting
        var lines = new List<string>();
        foreach (var info in typeof(ImageListViewColor).GetProperties())
        {
            // Walk through color properties
            if (info.PropertyType == typeof(Color))
            {
                // Get property name
                var name = info.Name;

                // Get the current value
                var color = (Color?)info.GetValue(colors, null);

                // Find the default value atribute
                var attributes = (Attribute[])info.GetCustomAttributes(typeof(DefaultValueAttribute), false);

                if (attributes.Length != 0)
                {
                    // Get the default value
                    var attribute = (DefaultValueAttribute)attributes[0];
                    var defaultColor = (Color?)attribute.Value;
                    // Serialize only if colors are different
                    if (color != defaultColor)
                    {
                        lines.Add(string.Format("{0} = {1}", name, color?.Name));
                    }
                }
            }
        }

        return string.Join("; ", lines.ToArray());
    }
    #endregion

}

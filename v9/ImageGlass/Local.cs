using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass;

internal class Local
{
    public static ToolStripButton BtnMainMenu = new()
    {
        Name = "_mainMenu",
        Image = Config.Theme.ToolbarIcons.ThumbnailBar,
        DisplayStyle = ToolStripItemDisplayStyle.Image,
        TextImageRelation = TextImageRelation.ImageBeforeText,
        Text = "Main menu",
        ToolTipText = "Main menu (Alf+F)",
        ForeColor = Config.Theme.Settings.TextColor,
        CheckOnClick = true,
        Padding = Constants.TOOLBAR_BTN_PADDING,
        Margin = Constants.TOOLBAR_BTN_MARGIN,

        Alignment = ToolStripItemAlignment.Right,
        Overflow = ToolStripItemOverflow.Never,
    };
}
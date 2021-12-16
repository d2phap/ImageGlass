using ImageGlass.Base;
using ImageGlass.Settings;

namespace ImageGlass;

internal class Local
{
    public static ToolStripButton BtnMainMenu = new()
    {
        Name = "Btn_MainMenu",
        DisplayStyle = ToolStripItemDisplayStyle.Image,
        TextImageRelation = TextImageRelation.ImageBeforeText,
        Text = "Main menu",
        ToolTipText = "Main menu (Alf+F)",
        CheckOnClick = true,

        // save icon name to load later
        Tag = nameof(Config.Theme.ToolbarIcons.MainMenu),

        Alignment = ToolStripItemAlignment.Right,
        Overflow = ToolStripItemOverflow.Never,
    };
}
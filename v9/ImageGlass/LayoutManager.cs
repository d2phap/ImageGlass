
namespace ImageGlass;

public static class Layout
{
    public static List<string> Top { get; set; } = new();
    public static List<string> Bottom { get; set; } = new();
    public static List<string> Left { get; set; } = new();
    public static List<string> Right { get; set; } = new();



    //Tb0.Controls.Remove(Toolbar);
    //Sp1.Panel2.Controls.Remove(Gallery);

    //Sp1.Panel2.Controls.Add(Toolbar);
    //Sp1.SplitterDistance = Sp1.Height - Toolbar.Height - Gallery.Height;

    //Tb0.Controls.Add(Gallery, 0, 0);
    //Tb0.RowStyles[0].SizeType = SizeType.AutoSize;
    //Tb0.RowStyles[0].Height = Gallery.Height;
}

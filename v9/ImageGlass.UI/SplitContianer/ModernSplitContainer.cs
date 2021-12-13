
namespace ImageGlass.UI;

public class ModernSplitContainer : SplitContainer
{
    private Color _splitterColor = Color.Transparent;
    
    public Color SplitterBackColor
    {
        get => _splitterColor;
        set
        {
            _splitterColor = value;
            Invalidate();
        }
    }

    public ModernSplitContainer()
    {
        // set default splitter width
        SplitterWidth = 10;
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        // change default cursors
        Cursor = Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
    }


    protected override void OnGotFocus(EventArgs e)
    {
        //base.OnGotFocus(e);
    }



    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // draw splitter background
        e.Graphics.FillRectangle(new SolidBrush(SplitterBackColor), SplitterRectangle);
    }
}
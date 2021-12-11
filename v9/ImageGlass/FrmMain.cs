namespace ImageGlass;

public partial class FrmMain : Form
{
    public FrmMain()
    {
        InitializeComponent();
        SetUpFrmMainConfigs();
        SetUpFrmMainTheme();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        //button2.Visible = !button2.Visible;
        //button2.Height += 10;
    }

    bool resizing;
    int rowindex = -1;
    int nextHeight;

    private void tb1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            resizing = true;
        }
    }

    private void tb1_MouseMove(object sender, MouseEventArgs e)
    {
        Control c = (Control)sender;
        if (!resizing)
        {
            rowindex = -1;
            tb1.Cursor = Cursors.Default;

            if (e.Y <= 3)
            {
                rowindex = tb1.GetPositionFromControl(c).Row - 1;
                if (rowindex >= 0)
                    tb1.Cursor = Cursors.HSplit;
            }
            if (c.Height - e.Y <= 3)
            {
                rowindex = tb1.GetPositionFromControl(c).Row;
                if (rowindex < tb1.RowStyles.Count)
                    tb1.Cursor = Cursors.HSplit;
            }
        }
        if (resizing && rowindex > -1)
        {
            nextHeight = e.Y;
        }
    }

    private void tb1_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (nextHeight > 0)
                tb1.RowStyles[rowindex].Height = nextHeight;
            resizing = false;
        }
    }
}

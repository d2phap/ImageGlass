namespace ImageGlass;

public partial class FrmMain : Form
{
    public FrmMain()
    {
        InitializeComponent();

        SetUpFrmMainConfigs();
        SetUpFrmMainTheme();
    }

    

    private void FrmMain_Resize(object sender, EventArgs e)
    {

    }

    protected override void WndProc(ref Message m)
    {
        // WM_SYSCOMMAND
        if (m.Msg == 0x0112)
        {
            // When user clicks on MAXIMIZE button on title bar
            if (m.WParam == new IntPtr(0xF030)) // SC_MAXIMIZE
            {
                // The window is being maximized
            }
            // When user clicks on the RESTORE button on title bar
            else if (m.WParam == new IntPtr(0xF120)) // SC_RESTORE
            {
                // The window is being restored
            }
        }

        base.WndProc(ref m);
    }

    private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
    {

    }
}

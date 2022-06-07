using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;

namespace ImageGlass.UI.BuiltInForms;

public partial class InputForm : Form
{

    public override string Text
    {
        get => base.Text; set
        {
            lblTitle.Text = value;
            base.Text = value;
        }
    }

    public string ContentText
    {
        get => lblContent.Text;
        set
        {
            lblContent.Text = value;
        }
    }

    public string Value
    {
        get => txtValue.Text;
        set
        {
            txtValue.Text = value;
        }
    }

    public InputForm()
    {
        InitializeComponent();
        RegisterFormEvents();
    }

    #region Borderless form

    private bool isAeroEnabled;
    private const int WM_NCPAINT = 0x0085;
    private const int WS_EX_NOACTIVATE = 0x8000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;


    protected override CreateParams CreateParams
    {
        get
        {
            if (DesignMode) return base.CreateParams;

            isAeroEnabled = CheckAeroEnabled();
            var cp = base.CreateParams;


            if (!isAeroEnabled)
            {
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            }

            return cp;
        }
    }


    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_NCPAINT:
                if (isAeroEnabled)
                {
                    unsafe
                    {
                        var value = 2;

                        PInvoke.DwmSetWindowAttribute(new(Handle),
                            DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY,
                            &value, sizeof(int));


                        var margins = new MARGINS()
                        {
                            cyTopHeight = 1,
                            cyBottomHeight = 1,
                            cxLeftWidth = 1,
                            cxRightWidth = 1,
                        };

                        PInvoke.DwmExtendFrameIntoClientArea(new(Handle), &margins);
                    }
                }
                break;

            default:
                break;
        }


        base.WndProc(ref m);

    }


    private static bool CheckAeroEnabled()
    {
        if (Environment.OSVersion.Version.Major >= 6)
        {
            PInvoke.DwmIsCompositionEnabled(out var enabled);

            return enabled ? true : false;
        }

        return false;
    }

    #endregion

    #region Borderless form moving

    private bool isMouseDown; // moving windows is taking place
    private Point lastLocation; // initial mouse position


    /// <summary>
    /// Initialize all event handlers required to manage borderless window movement.
    /// </summary>
    protected void RegisterFormEvents()
    {
        MouseDown += InputForm_MouseDown;
        MouseUp += InputForm_MouseUp;
        MouseMove += InputForm_MouseMove;

        foreach (Control control in Controls)
        {
            if (control is Label ||
                control is TableLayoutPanel ||
                control.HasChildren)
            {
                control.MouseDown += InputForm_MouseDown;
                control.MouseUp += InputForm_MouseUp;
                control.MouseMove += InputForm_MouseMove;
            }

            // child controls
            foreach (Control childControl in control.Controls)
            {
                if (childControl is Label || childControl is TableLayoutPanel)
                {
                    childControl.MouseDown += InputForm_MouseDown;
                    childControl.MouseUp += InputForm_MouseUp;
                    childControl.MouseMove += InputForm_MouseMove;
                }
            }
        }
    }

    private void InputForm_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Clicks == 1)
        {
            isMouseDown = true;
        }

        lastLocation = e.Location;
    }

    private void InputForm_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!isMouseDown)
        {
            return; // not moving windows, ignore
        }

        Location = new Point(Location.X - lastLocation.X + e.X,
                Location.Y - lastLocation.Y + e.Y);

        Update();
    }

    private void InputForm_MouseUp(object? sender, MouseEventArgs e)
    {
        isMouseDown = false;
    }


    #endregion



    /// <summary>
    /// Apply theme colors to controls
    /// </summary>
    /// <param name="th">Theme</param>
    public void SetTheme(IgTheme th)
    {
        lblTitle.ForeColor = 
            lblContent.ForeColor = th.Settings.TextColor;

        txtValue.BackColor = th.Settings.ToolbarBgColor;
        txtValue.ForeColor = th.Settings.TextColor;
        txtValue.BorderStyle = BorderStyle.FixedSingle;

        lblTitle.BackColor =
        panBottom.BackColor = th.Settings.ToolbarBgColor;

        BackColor = th.Settings.BgColor;
    }


    private void InputForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            btnCancel.PerformClick();
        }
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    
}

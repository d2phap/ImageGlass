using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;

namespace ImageGlass.UI.BuiltInForms;

public partial class ToolForm : Form
{
    private bool isAeroEnabled; // variables for box shadow

    private const int CS_DROPSHADOW = 0x00020000;
    private const int WM_NCPAINT = 0x0085;
    private const int HTCLIENT = 0x1;
    private const int HTCAPTION = 0x2;

    private const int WS_MINIMIZEBOX = 0x20000;
    private const int WS_SYSMENU = 0x80000;
    private const int WS_SIZEBOX = 0x40000;


    public ToolForm()
    {
        InitializeComponent();

        RegisterToolFormEvents();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            isAeroEnabled = CheckAeroEnabled();
            var cp = base.CreateParams;

            if (!isAeroEnabled)
            {
                //cp.ClassStyle |= CS_DROPSHADOW | WS_MINIMIZEBOX | WS_SYSMENU | WS_SIZEBOX;
                cp.ExStyle |= 0x8000000 // WS_EX_NOACTIVATE
                        | 0x00000080;   // WS_EX_TOOLWINDOW
            }

            return cp;
        }
    }


    protected override void WndProc(ref Message m)
    {
        const int wmNcHitTest = 0x84;
        const int htLeft = 10;
        const int htRight = 11;
        const int htTop = 12;
        const int htTopLeft = 13;
        const int htTopRight = 14;
        const int htBottom = 15;
        const int htBottomLeft = 16;
        const int htBottomRight = 17;

        switch (m.Msg)
        {
            case WM_NCPAINT: // box shadow
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


    /// <summary>
    /// Apply theme colors to controls
    /// </summary>
    /// <param name="th">Theme</param>
    public virtual void SetTheme(IgTheme th)
    {
        var bgColor = th.Settings.BgColor;
        var fontColor = th.Settings.TextColor;

        foreach (Control control in this.Controls)
        {
            if (control is Label ||
                control is NumericUpDown ||
                control is Panel ||
                control is TableLayoutPanel)
            {
                control.BackColor = bgColor;
                control.ForeColor = fontColor;
            }
            else if (control is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.BackColor = bgColor;
                txt.ForeColor = fontColor;
            }

            // container
            if (control.HasChildren)
            {
                foreach (Control childControl in control.Controls)
                {
                    if (childControl is Label ||
                        childControl is TextBox ||
                        childControl is NumericUpDown ||
                        childControl is TableLayoutPanel)
                    {
                        childControl.BackColor = bgColor;
                        childControl.ForeColor = fontColor;
                    }
                }
            }
        }

        BackColor = bgColor;
    }


    private void btnOK_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }




    #region Borderless form moving

    private bool mouseDown; // moving windows is taking place
    private Point lastLocation; // initial mouse position
    private bool moveSnapped; // move toolform windows together

    private void Form1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Clicks == 1)
        {
            mouseDown = true;
        }

        if (ModifierKeys == Keys.Control)
        {
            moveSnapped = true;
        }

        lastLocation = e.Location;
    }

    private void Form1_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!mouseDown)
        {
            return; // not moving windows, ignore
        }

        Location = new Point(Location.X - lastLocation.X + e.X,
                Location.Y - lastLocation.Y + e.Y);

        Update();
    }

    private void Form1_MouseUp(object? sender, MouseEventArgs e)
    {
        mouseDown = false;
        moveSnapped = false;
    }

    /// <summary>
    /// Initialize all event handlers required to manage borderless window movement.
    /// </summary>
    protected void RegisterToolFormEvents()
    {
        Move += ToolForm_Move;

        MouseDown += Form1_MouseDown;
        MouseUp += Form1_MouseUp;
        MouseMove += Form1_MouseMove;

        foreach (Control control in Controls)
        {
            if (control is Label ||
                control is TableLayoutPanel ||
                control.HasChildren)
            {
                control.MouseDown += Form1_MouseDown;
                control.MouseUp += Form1_MouseUp;
                control.MouseMove += Form1_MouseMove;
            }

            control.MouseEnter += ToolForm_MouseEnter;
            control.MouseLeave += ToolForm_MouseLeave;

            // child controls
            foreach (Control childControl in control.Controls)
            {
                if (childControl is Label || childControl is TableLayoutPanel)
                {
                    childControl.MouseDown += Form1_MouseDown;
                    childControl.MouseUp += Form1_MouseUp;
                    childControl.MouseMove += Form1_MouseMove;
                }

                childControl.MouseEnter += ToolForm_MouseEnter;
                childControl.MouseLeave += ToolForm_MouseLeave;
            }
        }
    }

    // The tool windows itself has moved; track its location relative to parent
    private void ToolForm_Move(object? sender, EventArgs e)
    {
        //if (!formOwnerMoving)
        //{
        //    _locationOffset = new Point(Left - Owner.Left, Top - Owner.Top);
        //    parentOffset = _locationOffset;
        //}
    }


    private void ToolForm_MouseLeave(object? sender, EventArgs e)
    {
        if (ActiveForm != this)
        {
            try
            {
                //this.Opacity = 0.85;
            }
            catch { }
        }
    }

    private void ToolForm_MouseEnter(object? sender, EventArgs e)
    {
        this.Opacity = 1;
    }

    private void ToolForm_Activated(object? sender, EventArgs e)
    {
        this.Opacity = 1;
    }

    private void ToolForm_Deactivate(object? sender, EventArgs e)
    {
        try
        {
            //this.Opacity = 0.85;
        }
        catch { }
    }


    #endregion

}

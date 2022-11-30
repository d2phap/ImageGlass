/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;

namespace ImageGlass.UI;


/// <summary>
/// Common functionalities for floating tool window
/// </summary>
public partial class ToolForm : ModernForm
{
    protected Form _currentOwner;


    /// <summary>
    /// Gets, set the theme.
    /// </summary>
    public IgTheme Theme { get; set; }


    /// <summary>
    /// Gets, sets the init location
    /// </summary>
    public Point InitLocation { get; set; }


    #region Events to manage the form location relative to parent

    private bool _isFormOwnerMoving;
    protected Point OwnerPosition = Point.Empty;
    protected Point FormPosition;

    private void AttachEventsToParent(ModernForm frmOwner)
    {
        if (frmOwner is not ModernForm owner) return;

        owner.Move += Owner_Move;
        owner.SizeChanged += Owner_Move;
        owner.VisibleChanged += Owner_Move;
        owner.LocationChanged += Owner_LocationChanged;
    }

    private void Owner_LocationChanged(object? sender, EventArgs e)
    {
        _isFormOwnerMoving = false;
    }

    private void DetachEventsFromParent(Form frmOwner)
    {
        if (frmOwner == null) return;

        frmOwner.Move -= Owner_Move;
        frmOwner.SizeChanged -= Owner_Move;
        frmOwner.VisibleChanged -= Owner_Move;
        frmOwner.LocationChanged -= Owner_LocationChanged;
    }

    private void Owner_Move(object? sender, EventArgs e)
    {
        if (Owner == null) return;

        _isFormOwnerMoving = true;
        

        if (Owner.WindowState == FormWindowState.Normal)
        {
            SetLocationBasedOnParent();
        }
    }

    // The tool windows itself has moved; track its location relative to parent
    private void Form_Move(object? sender, EventArgs e)
    {
        if (!_isFormOwnerMoving)
        {
            FormPosition = new Point(Left - Owner.Left, Top - Owner.Top);
            OwnerPosition = FormPosition;
        }
    }

    protected override void OnShown(EventArgs e)
    {
        if (Owner != _currentOwner)
        {
            DetachEventsFromParent(_currentOwner);
            _currentOwner = Owner;
            AttachEventsToParent(_currentOwner as ModernForm);
        }

        base.OnShown(e);
    }

    #endregion


    #region Borderless form moving

    private bool _isMouseDown; // moving windows is taking place
    private Point _lastLocation; // initial mouse position

    private void Form_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Clicks == 1)
        {
            _isMouseDown = true;
        }

        _lastLocation = e.Location;
    }

    private void Form_MouseMove(object? sender, MouseEventArgs e)
    {
        // not moving windows, ignore
        if (!_isMouseDown) return;

        Location = new Point(Location.X - _lastLocation.X + e.X,
                Location.Y - _lastLocation.Y + e.Y);

        Update();
    }

    private void Form_MouseUp(object? sender, MouseEventArgs e)
    {
        _isMouseDown = false;
    }


    #endregion


    public ToolForm()
    {
        InitializeComponent();
    }

    public ToolForm(IgTheme theme)
    {
        InitializeComponent();
        Theme = theme;

        Opacity = 0.85;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        AddFormEvents();

        SetLocationBasedOnParent(InitLocation);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        RemoveFormEvents();
    }


    /// <summary>
    /// Initialize all event handlers required to manage borderless window movement.
    /// </summary>
    protected virtual void AddFormEvents()
    {
        Move += Form_Move;
        MouseDown += Form_MouseDown;
        MouseUp += Form_MouseUp;
        MouseMove += Form_MouseMove;

        Activated += Form_Activated;
        Deactivate += Form_Deactivate;
        MouseEnter += Form_MouseEnter;
        MouseLeave += Form_MouseLeave;

        AddFormControlsEvents(this);
    }


    protected virtual void RemoveFormEvents()
    {
        Move -= Form_Move;
        MouseDown -= Form_MouseDown;
        MouseUp -= Form_MouseUp;
        MouseMove -= Form_MouseMove;

        Activated -= Form_Activated;
        Deactivate -= Form_Deactivate;
        MouseEnter -= Form_MouseEnter;
        MouseLeave -= Form_MouseLeave;

        RemoveFormControlsEvents(this);
    }

    private void AddFormControlsEvents(Control c)
    {
        foreach (Control childControl in c.Controls)
        {
            if (childControl is Label
                || childControl is PictureBox
                || childControl is TableLayoutPanel)
            {
                childControl.MouseDown += Form_MouseDown;
                childControl.MouseUp += Form_MouseUp;
                childControl.MouseMove += Form_MouseMove;
            }

            childControl.MouseEnter += Form_MouseEnter;
            childControl.MouseLeave += Form_MouseLeave;


            AddFormControlsEvents(childControl);
        }
    }


    private void RemoveFormControlsEvents(Control c)
    {
        foreach (Control childControl in c.Controls)
        {
            if (childControl is Label
                || childControl is PictureBox
                || childControl is TableLayoutPanel)
            {
                childControl.MouseDown -= Form_MouseDown;
                childControl.MouseUp -= Form_MouseUp;
                childControl.MouseMove -= Form_MouseMove;
            }

            childControl.MouseEnter -= Form_MouseEnter;
            childControl.MouseLeave -= Form_MouseLeave;


            RemoveFormControlsEvents(childControl);
        }
    }


    /// <summary>
    /// Sets window location to parent's location.
    /// </summary>
    protected virtual void SetLocationBasedOnParent(Point initLoc = default)
    {
        if (Owner == null) return;

        if (Owner.WindowState == FormWindowState.Minimized || !Owner.Visible)
        {
            Visible = false;
            return;
        }

        // set location based on the main form
        var loc = Owner.Location;
        loc.Offset(OwnerPosition);

        loc.X += initLoc.X;
        loc.Y += initLoc.Y;


        if (loc.X < 0) { loc.X = 0; }
        if (loc.Y < 0) { loc.Y = 0; }

        var workingArea = Screen.FromControl(this).WorkingArea;
        if (loc.X + Width > workingArea.Right)
        {
            loc.X = workingArea.Right - Width;
        }
        if (loc.Y + Height > workingArea.Bottom)
        {
            loc.Y = workingArea.Bottom - Height;
        }

        Location = loc;
    }


    private void Form_MouseEnter(object? sender, EventArgs e)
    {
        Opacity = 1;
    }

    private void Form_MouseLeave(object? sender, EventArgs e)
    {
        if (ActiveForm != this)
        {
            try
            {
                Opacity = 0.85;
            }
            catch { }
        }
    }

    private void Form_Activated(object? sender, EventArgs e)
    {
        Opacity = 1;
    }

    private void Form_Deactivate(object? sender, EventArgs e)
    {
        try
        {
            Opacity = 0.85;
        }
        catch { }
    }

}

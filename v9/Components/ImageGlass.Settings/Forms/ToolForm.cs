/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.UI;
using System.ComponentModel;

namespace ImageGlass.Settings;


/// <summary>
/// Common functionalities for floating tool window
/// </summary>
public partial class ToolForm : ModernForm
{
    protected Form _currentOwner;


    #region Public properties

    /// <summary>
    /// Gets, sets the init location.
    /// </summary>
    public Point InitLocation { get; set; }


    /// <summary>
    /// Occurs when the tool form is being closed.
    /// </summary>
    public event ToolFormClosingHandler? ToolFormClosing;
    public delegate void ToolFormClosingHandler(ToolFormClosingEventArgs e);


    /// <summary>
    /// Occurs when the tool form is closed.
    /// </summary>
    public event ToolFormClosedHandler? ToolFormClosed;
    public delegate void ToolFormClosedHandler(ToolFormClosedEventArgs e);

    #endregion // public properties


    #region Make a tool window

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size MinimumSize => new Size();


    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size MaximumSize => new Size();


    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams baseParams = base.CreateParams;

            const int WS_EX_NOACTIVATE = 0x08000000;
            baseParams.ExStyle |= WS_EX_NOACTIVATE;

            return baseParams;
        }
    }

    #endregion // Make a tool window


    #region Events to manage the form location relative to parent

    private bool _isFormOwnerMoving;
    protected Point OwnerPosition = Point.Empty;
    protected Point FormPosition;

    private void AttachEventsToParent(ModernForm frmOwner)
    {
        if (DesignMode) return;
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
        if (!DesignMode && Owner != _currentOwner)
        {
            DetachEventsFromParent(_currentOwner);
            _currentOwner = Owner;
            AttachEventsToParent(_currentOwner as ModernForm);
        }

        base.OnShown(e);
    }

    #endregion


    public ToolForm() : base()
    {
        InitializeComponent();
        Opacity = 0.85;
    }


    #region Override / Virtual methods

    /// <summary>
    /// Recalculates and updates window height on <see cref="Form.OnLoad(EventArgs)"/> event.
    /// </summary>
    protected virtual int OnUpdateHeight(bool performUpdate = true)
    {
        // calculate form height
        var formHeight = Padding.Vertical;

        if (performUpdate)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void OnLoad(EventArgs e)
    {
        // adjust form size
        _ = OnUpdateHeight();

        base.OnLoad(e);
        if (DesignMode) return;

        AddFormEvents();

        EnableFormFreeMoving(this);
        SetLocationBasedOnParent(InitLocation);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // update theme
        ApplyTheme(Config.Theme.Settings.IsDarkMode);
        Invalidate(true);

        base.OnRequestUpdatingColorMode(e);
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (DesignMode) return;

        var args = new ToolFormClosingEventArgs(Name, e.CloseReason, e.Cancel);
        OnToolFormClosing(args);

        DisableFormFreeMoving(this);
        RemoveFormEvents();
    }


    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        if (DesignMode) return;

        var args = new ToolFormClosedEventArgs(Name, e.CloseReason);
        OnToolFormClosed(args);
    }


    /// <summary>
    /// Raises the <see cref="ToolForm.ToolFormClosing"/> event.
    /// </summary>
    protected virtual void OnToolFormClosing(ToolFormClosingEventArgs e)
    {
        ToolFormClosing?.Invoke(e);
    }


    /// <summary>
    /// Raises the <see cref="ToolForm.ToolFormClosed"/> event.
    /// </summary>
    protected virtual void OnToolFormClosed(ToolFormClosedEventArgs e)
    {
        ToolFormClosed?.Invoke(e);
    }


    /// <summary>
    /// Initialize all event handlers required to manage borderless window movement.
    /// </summary>
    protected virtual void AddFormEvents()
    {
        if (DesignMode) return;

        Move += Form_Move;

        Activated += Form_Activated;
        Deactivate += Form_Deactivate;
        MouseEnter += Form_MouseEnter;
        MouseLeave += Form_MouseLeave;

        AddFormControlsEvents(this);
    }


    /// <summary>
    /// Removes all tool form events initialized by <see cref="AddFormEvents"/>.
    /// </summary>
    protected virtual void RemoveFormEvents()
    {
        Move -= Form_Move;

        Activated -= Form_Activated;
        Deactivate -= Form_Deactivate;
        MouseEnter -= Form_MouseEnter;
        MouseLeave -= Form_MouseLeave;

        RemoveFormControlsEvents(this);
    }


    /// <summary>
    /// Sets window location to parent's location.
    /// </summary>
    protected virtual void SetLocationBasedOnParent(Point initLoc = default)
    {
        if (DesignMode || Owner == null) return;

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

    #endregion // Override / Virtual methods


    #region Private methods

    private void AddFormControlsEvents(Control c)
    {
        foreach (Control childControl in c.Controls)
        {
            childControl.MouseEnter += Form_MouseEnter;
            childControl.MouseLeave += Form_MouseLeave;

            AddFormControlsEvents(childControl);
        }
    }


    private void RemoveFormControlsEvents(Control c)
    {
        foreach (Control childControl in c.Controls)
        {
            childControl.MouseEnter -= Form_MouseEnter;
            childControl.MouseLeave -= Form_MouseLeave;

            RemoveFormControlsEvents(childControl);
        }
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

    #endregion // Private methods


}


public interface IToolForm<T>
{
    /// <summary>
    /// Gets the ID of the tool.
    /// </summary>
    string ToolId { get; }


    /// <summary>
    /// Gets, sets settings for this tool, written in app's config file.
    /// </summary>
    T Settings { get; set; }

}

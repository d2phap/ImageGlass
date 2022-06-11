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
using ImageGlass.Base.WinApi;
using System.Globalization;
using System.Media;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;

namespace ImageGlass.UI;


/// <summary>
/// The built-in buttons for Popup.
/// </summary>
public enum PopupButtons: uint
{
    OK = 0,
    Close = 1,
    Yes_No = 2,
    OK_Cancel = 3,
    OK_Close = 4,
    LearnMore_Close = 5,
    Continue_Quit = 6,
}


public partial class Popup : Form
{
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
            if (control is Label
                || control is PictureBox
                || control is TableLayoutPanel
                || control.HasChildren)
            {
                control.MouseDown += InputForm_MouseDown;
                control.MouseUp += InputForm_MouseUp;
                control.MouseMove += InputForm_MouseMove;
            }

            // child controls
            foreach (Control childControl in control.Controls)
            {
                if (childControl is Label
                    || childControl is PictureBox
                    || childControl is TableLayoutPanel)
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


    private bool _intValueOnly = false;
    private bool _unsignedIntValueOnly = false;
    private bool _floatValueOnly = false;
    private bool _unsignedFloatValueOnly = false;


    #region Public properties

    public IgTheme Theme { get; private set; }
    public IgLang? Language { get; private set; }


    /// <summary>
    /// Form title
    /// </summary>
    public string Title
    {
        get => lblTitle.Text;
        set
        {
            lblTitle.Text = value;
        }
    }


    /// <summary>
    /// Heading text
    /// </summary>
    public string Heading
    {
        get => lblHeading.Text;
        set
        {
            lblHeading.Text = value;
            var isVisible = !string.IsNullOrEmpty(value);
            var rowIndex = tableMain.GetRow(lblHeading);

            if (isVisible)
            {
                lblHeading.Visible = true;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                lblHeading.Visible = false;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.Absolute;
                tableMain.RowStyles[rowIndex].Height = 0;
            }
        }
    }


    /// <summary>
    /// Description text
    /// </summary>
    public string Description
    {
        get => lblDescription.Text;
        set
        {
            lblDescription.Text = value;
            var isVisible = !string.IsNullOrEmpty(value);
            var rowIndex = tableMain.GetRow(lblDescription);

            if (isVisible)
            {
                lblDescription.Visible = true;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                lblDescription.Visible = false;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.Absolute;
                tableMain.RowStyles[rowIndex].Height = 0;
            }
        }
    }


    /// <summary>
    /// Form value.
    /// </summary>
    public string Value
    {
        get => txtValue.Text;
        set => txtValue.Text = value;
    }


    /// <summary>
    /// Shows or hides text input.
    /// </summary>
    public bool ShowTextInput
    {
        get => txtValue.Visible;
        set => txtValue.Visible = value;
    }


    /// <summary>
    /// Gets, set the value indicates that text input <c>Multiline</c> is enabled.
    /// </summary>
    public bool TextInputMultiLine
    {
        get => txtValue.Multiline;
        set
        {
            txtValue.Multiline = value;
            txtValue.Font = new Font("Consolas", Font.Size);
        }
    }


    /// <summary>
    /// Gets, set the value indicates that text input <c>ReadOnly</c> is enabled.
    /// </summary>
    public bool TextInputReadOnly
    {
        get => txtValue.ReadOnly;
        set => txtValue.ReadOnly = value;
    }


    /// <summary>
    /// Shows or hides the Shield icon for the <see cref="BtnAccept"/>.
    /// </summary>
    public bool ShowAcceptButtonShieldIcon
    {
        get => BtnAccept.SystemIcon == SHSTOCKICONID.SIID_SHIELD;
        set => BtnAccept.SystemIcon = value ? SHSTOCKICONID.SIID_SHIELD : null;
    }

    /// <summary>
    /// Gets, sets visibility value of the <see cref="BtnAccept"/>.
    /// </summary>
    public bool ShowAcceptButton
    {
        get => BtnAccept.Visible;
        set => BtnAccept.Visible = value;
    }

    /// <summary>
    /// Gets, sets text of the <see cref="BtnAccept"/>.
    /// </summary>
    public string AcceptButtonText
    {
        get => BtnAccept.Text;
        set => BtnAccept.Text = value;
    }


    /// <summary>
    /// Gets, sets visibility value of the <see cref="BtnCancel"/>.
    /// </summary>
    public bool ShowCancelButton
    {
        get => BtnCancel.Visible;
        set => BtnCancel.Visible = value;
    }


    /// <summary>
    /// Gets, sets text of the <see cref="BtnCancel"/>.
    /// </summary>
    public string CancelButtonText
    {
        get => BtnCancel.Text;
        set => BtnCancel.Text = value;
    }


    /// <summary>
    /// Gets, sets the thumbnail overlay image.
    /// </summary>
    public Image? ThumbnailOverlay
    {
        get => picThumbnail.Image;
        set
        {
            if (value == null)
            {
                picThumbnail.Image = null;
                return;
            };

            // draw thumbnail overlay
            var bmp = new Bitmap(picThumbnail.Width, picThumbnail.Height);
            var g = Graphics.FromImage(bmp);

            g.DrawImageUnscaled(value, new Point(
                bmp.Width - value.Width,
                bmp.Height - value.Height));

            picThumbnail.Image = bmp;
        }
    }


    /// <summary>
    /// Gets, sets the thumbnail image
    /// </summary>
    public Image? Thumbnail
    {
        get => picThumbnail.BackgroundImage;
        set
        {
            if (value != null && picThumbnail.Width >= Math.Max(value.Width, value.Height))
            {
                // image is smaller than picThumbnail
                picThumbnail.BackgroundImageLayout = ImageLayout.Center;
            }
            else
            {
                // image is bigger than picThumbnail
                picThumbnail.BackgroundImageLayout = ImageLayout.Zoom;
            }

            picThumbnail.BackgroundImage = value;
        }
    }


    /// <summary>
    /// Pattern for validation
    /// </summary>
    public string RegexPattern { get; set; } = "";


    /// <summary>
    /// Limit the number of characters the user can enter
    /// </summary>
    public int MaxLimit
    {
        set => txtValue.MaxLength = value;
    }


    /// <summary>
    /// Allows integer number value only
    /// </summary>
    public bool IntValueOnly
    {
        get => _intValueOnly;
        set
        {
            _intValueOnly = value;

            if (_intValueOnly)
            {
                var negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign;
                var positiveSign = NumberFormatInfo.CurrentInfo.PositiveSign;

                RegexPattern = $"^[{positiveSign}{negativeSign}]?[0-9]+$";
            }
        }
    }


    /// <summary>
    /// Allows unsigned integer number only
    /// </summary>
    public bool UnsignedIntValueOnly
    {
        get => _unsignedIntValueOnly;
        set
        {
            _unsignedIntValueOnly = value;

            if (_unsignedIntValueOnly)
            {
                RegexPattern = $"^[0-9]+$";
            }
        }
    }


    /// <summary>
    /// Allows float number only
    /// </summary>
    public bool FloatValueOnly
    {
        get => _unsignedFloatValueOnly;
        set
        {
            _unsignedFloatValueOnly = value;

            if (_unsignedFloatValueOnly)
            {
                var decSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                RegexPattern = $"^([0-9]+([{decSeparator}][0-9]*)?|[{decSeparator}][0-9]+)$";
            }
        }
    }


    /// <summary>
    /// Allows unsigned float number only
    /// </summary>
    public bool UnsignedFloatValueOnly
    {
        get => _floatValueOnly;
        set
        {
            _floatValueOnly = value;

            if (_floatValueOnly)
            {
                var negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign;
                var positiveSign = NumberFormatInfo.CurrentInfo.PositiveSign;
                var decSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                RegexPattern = $"^[{positiveSign}{negativeSign}]?([0-9]+([{decSeparator}][0-9]*)?|[{decSeparator}][0-9]+)$";
            }
        }
    }


    /// <summary>
    /// Allow valid filename only
    /// </summary>
    public bool FileNameValueOnly { get; set; } = false;


    #endregion


    public Popup(IgTheme theme, IgLang? lang)
    {
        InitializeComponent();
        RegisterFormEvents();

        ShowInTaskbar = false;
        Heading = "";
        Description = "";
        Title = "";
        Thumbnail = null; // hide thumbnail by default

        Language = lang;
        ApplyLanguage();

        Theme = theme;
        ApplyTheme();
    }


    #region Override functions

    protected override void OnLoad(EventArgs e)
    {
        // show thumbnail
        var showThumbnail = Thumbnail != null || ThumbnailOverlay != null;
        var columnIndex = tableMain.GetColumn(picThumbnail);

        if (showThumbnail)
        {
            picThumbnail.Visible = true;
            tableMain.ColumnStyles[columnIndex].SizeType = SizeType.AutoSize;
        }
        else
        {
            picThumbnail.Visible = false;

            tableMain.ColumnStyles[columnIndex].SizeType = SizeType.Absolute;
            tableMain.ColumnStyles[columnIndex].Width = 0;
        }


        // calculate form height
        var contentHeight = lblHeading.Height + lblHeading.Margin.Vertical +
            lblDescription.Height + lblDescription.Margin.Vertical +
            txtValue.Height + txtValue.Margin.Vertical;

        var height = lblTitle.Height + lblTitle.Margin.Vertical +
            Math.Max(picThumbnail.Height, contentHeight) +
            panBottom.Height;

        Height = height;



        // set default focus
        if (!TextInputReadOnly)
        {
            tableMain.TabIndex = 0;
            panBottom.TabIndex = 1;
            txtValue.Focus();
            txtValue.SelectAll();
        }
        else
        {
            tableMain.TabIndex = 1;
            panBottom.TabIndex = 0;

            if (ShowAcceptButton)
            {
                BtnAccept.Focus();
            }
            else
            {
                BtnCancel.Focus();
            }
        }

        base.OnLoad(e);
    }


    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        lblTitle.ForeColor = Theme.Settings.TextColor;
        lblTitle.BackColor = Theme.Settings.ToolbarBgColor;
    }


    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);


        // title text color
        lblTitle.ForeColor = ThemeUtils.AdjustLightness(
            Theme.Settings.TextColor,
            Theme.Info.IsDark ? -0.5f : 0.5f);

        lblTitle.BackColor = ThemeUtils.AdjustLightness(
            Theme.Settings.ToolbarBgColor,
            Theme.Info.IsDark ? -0.2f : 0.2f);
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // disable parent form shotcuts
        return false;
    }

    #endregion


    #region Form and control events

    private void Popup_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
        {
            AbortForm();
        }
    }

    private void TxtValue_TextChanged(object sender, EventArgs e)
    {
        _ = ValidateInput();
    }

    private void BtnAccept_Click(object sender, EventArgs e)
    {
        if (ValidateInput())
        {
            AcceptForm();
        }
        else
        {
            txtValue.Focus();
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        CancelForm();
    }

    #endregion


    #region Private functions

    /// <summary>
    /// Apply language pack
    /// </summary>
    private void ApplyLanguage()
    {
        if (Language != null)
        {
            BtnAccept.Text = Language["_._OK"];
            BtnCancel.Text = Language["_._Cancel"];
        }
    }


    /// <summary>
    /// Apply theme to the form
    /// </summary>
    private void ApplyTheme()
    {
        BackColor = Theme.Settings.BgColor;

        // text color
        lblTitle.ForeColor =
            lblHeading.ForeColor =
            lblDescription.ForeColor = Theme.Settings.TextColor;

        // header and footer
        lblTitle.BackColor =
            picThumbnail.BackColor =
            panBottom.BackColor = Theme.Settings.ToolbarBgColor;

        // dark mode
        txtValue.DarkMode =
            BtnAccept.DarkMode =
            BtnCancel.DarkMode = Theme.Info.IsDark;

    }


    /// <summary>
    /// Validates the input and shows error.
    /// </summary>
    /// <returns></returns>
    private bool ValidateInput()
    {
        var isValid = true;

        if (!string.IsNullOrEmpty(RegexPattern))
        {
            isValid = Regex.IsMatch(txtValue.Text, RegexPattern);
        }
        else if (FileNameValueOnly)
        {
            var badChars = Path.GetInvalidFileNameChars();

            foreach (var c in badChars)
            {
                if (txtValue.Text.Contains(c))
                {
                    isValid = false;
                    break;
                }
            }
        }

        // invalid char
        if (!isValid)
        {
            BtnAccept.Enabled = false;

            txtValue.BackColor = Theme.DangerColor;
        }
        else
        {
            BtnAccept.Enabled = true;
            txtValue.BackColor = Theme.Settings.ToolbarBgColor;
        }

        return isValid;
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogResult.Cancel"/> code.
    /// </summary>
    private void CancelForm()
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogResult.Abort"/> code.
    /// </summary>
    private void AbortForm()
    {
        DialogResult = DialogResult.Abort;
        Close();
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogResult.OK"/> code.
    /// </summary>
    private void AcceptForm()
    {
        DialogResult = DialogResult.OK;
        Close();
    }


    #endregion


    #region Static functions

    /// <summary>
    /// Shows popup widow.
    /// </summary>
    /// <param name="theme">Popup theme.</param>
    /// <param name="lang">Popup language.</param>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    /// <param name="icon">Popup icon.</param>
    /// <returns>
    /// <list type="table">
    ///   <item>
    ///     <see cref="DialogResult.OK"/> if user clicks on
    ///     the <c>OK</c>, <c>Yes</c> or <c>Learn more</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Cancel"/> if user clicks on
    ///     the <c>Cancel</c>, <c>No</c> or <c>Close</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Abort"/> if user presses <c>ESC</c>.
    ///   </item>
    /// </list>
    /// </returns>
    public static DialogResult ShowDialog(IgTheme theme, IgLang lang,
        string title = "",
        string heading = "",
        string description = "",
        string details = "",
        PopupButtons buttons = PopupButtons.OK,
        SHSTOCKICONID? icon = null)
    {
        var frm = new Popup(theme, lang)
        {
            Title = title,
            Heading = heading,
            Description = description,

            Thumbnail = SystemIconApi.GetSystemIcon(icon),
            ShowTextInput = false,
            ShowInTaskbar = true,
        };

        if (!string.IsNullOrEmpty(details.Trim()))
        {
            frm.Value = details;
            frm.TextInputMultiLine = true;
            frm.TextInputReadOnly = true;
            frm.ShowTextInput = true;

            frm.Width += 200;
        }

        if (buttons == PopupButtons.OK_Cancel)
        {
            frm.AcceptButtonText = lang["_._OK"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = lang["_._Cancel"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButtons.OK_Close)
        {
            frm.AcceptButtonText = lang["_._OK"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = lang["_._Close"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButtons.Yes_No)
        {
            frm.AcceptButtonText = lang["_._Yes"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = lang["_._No"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButtons.LearnMore_Close)
        {
            frm.AcceptButtonText = lang["_._LearnMore"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = lang["_._Close"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButtons.Continue_Quit)
        {
            frm.AcceptButtonText = lang["_._Continue"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = lang["_._Quit"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButtons.Close)
        {
            frm.ShowAcceptButton = false;

            frm.CancelButtonText = lang["_._Close"];
            frm.ShowCancelButton = true;
        }
        else
        {
            frm.CancelButtonText = lang["_._OK"];
            frm.ShowAcceptButton = false;

            frm.ShowCancelButton = false;
        }

        return frm.ShowDialog();
    }


    /// <summary>
    /// Shows information popup widow.
    /// </summary>
    /// <param name="theme">Popup theme.</param>
    /// <param name="lang">Popup language.</param>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    /// <returns>
    /// <list type="table">
    ///   <item>
    ///     <see cref="DialogResult.OK"/> if user clicks on
    ///     the <c>OK</c>, <c>Yes</c> or <c>Learn more</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Cancel"/> if user clicks on
    ///     the <c>Cancel</c>, <c>No</c> or <c>Close</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Abort"/> if user presses <c>ESC</c>.
    ///   </item>
    /// </list>
    /// </returns>
    public static DialogResult ShowInfo(IgTheme theme, IgLang lang,
        string title = "",
        string heading = "",
        string description = "",
        string details = "",
        PopupButtons buttons = PopupButtons.OK)
    {
        SystemSounds.Question.Play();

        return ShowDialog(theme, lang, title, heading, description, details, buttons, SHSTOCKICONID.SIID_INFO);
    }


    /// <summary>
    /// Shows warning popup widow.
    /// </summary>
    /// <param name="theme">Popup theme.</param>
    /// <param name="lang">Popup language.</param>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    /// <returns>
    /// <list type="table">
    ///   <item>
    ///     <see cref="DialogResult.OK"/> if user clicks on
    ///     the <c>OK</c>, <c>Yes</c> or <c>Learn more</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Cancel"/> if user clicks on
    ///     the <c>Cancel</c>, <c>No</c> or <c>Close</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Abort"/> if user presses <c>ESC</c>.
    ///   </item>
    /// </list>
    /// </returns>
    public static DialogResult ShowWarning(IgTheme theme, IgLang lang,
        string title = "",
        string heading = "",
        string description = "",
        string details = "",
        PopupButtons buttons = PopupButtons.OK)
    {
        SystemSounds.Exclamation.Play();

        return ShowDialog(theme, lang, title, heading, description, details, buttons, SHSTOCKICONID.SIID_WARNING);
    }


    /// <summary>
    /// Shows error popup widow.
    /// </summary>
    /// <param name="theme">Popup theme.</param>
    /// <param name="lang">Popup language.</param>
    /// <param name="title">Popup title.</param>
    /// <param name="heading">Popup heading text.</param>
    /// <param name="description">Popup description.</param>
    /// <param name="details">Other details</param>
    /// <param name="buttons">Popup buttons.</param>
    /// <returns>
    /// <list type="table">
    ///   <item>
    ///     <see cref="DialogResult.OK"/> if user clicks on
    ///     the <c>OK</c>, <c>Yes</c> or <c>Learn more</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Cancel"/> if user clicks on
    ///     the <c>Cancel</c>, <c>No</c> or <c>Close</c> button.
    ///   </item>
    ///   <item>
    ///     <see cref="DialogResult.Abort"/> if user presses <c>ESC</c>.
    ///   </item>
    /// </list>
    /// </returns>
    public static DialogResult ShowError(IgTheme theme, IgLang lang,
        string title = "",
        string heading = "",
        string description = "",
        string details = "",
        PopupButtons buttons = PopupButtons.OK)
    {
        SystemSounds.Asterisk.Play();

        return ShowDialog(theme, lang, title, heading, description, details, buttons, SHSTOCKICONID.SIID_ERROR);
    }


    #endregion


}

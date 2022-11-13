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
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;

namespace ImageGlass.UI;


public partial class Popup : ModernForm
{

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
    private StatusType _noteStatusType = StatusType.Neutral;


    #region Public properties

    public IgTheme Theme { get; private set; }
    public IgLang? Language { get; private set; }


    /// <summary>
    /// Form title
    /// </summary>
    public string Title
    {
        get => Text;
        set
        {
            Text = value;
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
    /// Note text.
    /// </summary>
    public string Note
    {
        get => lblNote.Text;
        set
        {
            lblNote.Text = value;
            var isVisible = !string.IsNullOrEmpty(value);
            var rowIndex = tableMain.GetRow(panNote);

            if (isVisible)
            {
                panNote.Visible = true;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                panNote.Visible = false;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.Absolute;
                tableMain.RowStyles[rowIndex].Height = 0;
            }
        }
    }


    /// <summary>
    /// Gets, sets the type of the note.
    /// </summary>
    public StatusType NoteStatusType
    {
        get => _noteStatusType;
        set
        {
            _noteStatusType = value;

            panNote.BackColor = ThemeUtils.GetBackgroundColorForStatus(value, IsDarkMode, 100);
            lblNote.ForeColor = Theme.ColorPalatte.LightText;
        }
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
            txtValue.Height = 200;
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
    /// Gets, sets check state of <see cref="ChkOption"/>.
    /// </summary>
    public bool OptionCheckBoxChecked
    {
        get => ChkOption.Checked;
        set => ChkOption.Checked = value;
    }


    /// <summary>
    /// Gets, sets text of the <see cref="ChkOption"/>.
    /// </summary>
    public string OptionCheckBoxText
    {
        get => ChkOption.Text;
        set
        {
            ChkOption.Text = value;
            var isVisible = !string.IsNullOrEmpty(value);
            var rowIndex = tableMain.GetRow(ChkOption);

            if (isVisible)
            {
                ChkOption.Visible = true;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                ChkOption.Visible = false;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.Absolute;
                tableMain.RowStyles[rowIndex].Height = 0;
            }
        }
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
        Note = "";
        Thumbnail = null; // hide thumbnail by default
        OptionCheckBoxText = "";

        Language = lang;
        ApplyLanguage();

        Theme = theme;
    }


    #region Override functions

    protected override void OnLoad(EventArgs e)
    {
        ApplyTheme(Theme.Settings.IsDarkMode);

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
        UpdateHeight();


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
        var isValid = ValidateInput();
        SetTextInputStyle(isValid, IsDarkMode);
    }

    private void BtnAccept_Click(object sender, EventArgs e)
    {
        var isValid = ValidateInput();
        SetTextInputStyle(isValid, IsDarkMode);

        if (isValid)
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
    public override void ApplyTheme(bool darkMode, BackdropStyle? backDrop = null)
    {
        SuspendLayout();

        var isDarkMode = Theme.Settings.IsDarkMode;

        // text color
        lblHeading.ForeColor =
            lblDescription.ForeColor =
            lblNote.ForeColor =
            ChkOption.ForeColor = Theme.ColorPalatte.LightText;

        
        panNote.BackColor = ThemeUtils.GetBackgroundColorForStatus(StatusType.Warning, isDarkMode, 100);
        panBottom.BackColor = Theme.ColorPalatte.BlueSelection.WithAlpha(10);


        // dark mode
        txtValue.DarkMode =
            BtnAccept.DarkMode =
            BtnCancel.DarkMode = isDarkMode;


        SetTextInputStyle(ValidateInput(), isDarkMode);

        ResumeLayout(false);

        base.ApplyTheme(isDarkMode, backDrop);
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

        return isValid;
    }


    /// <summary>
    /// Sets text input style
    /// </summary>
    private void SetTextInputStyle(bool isValid, bool darkMode)
    {
        // invalid char
        if (!isValid)
        {
            BtnAccept.Enabled = false;

            txtValue.BackColor = ThemeUtils.GetBackgroundColorForStatus(StatusType.Danger, darkMode);
        }
        else
        {
            BtnAccept.Enabled = true;
            txtValue.DarkMode = darkMode;
        }
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
    /// <param name="note">Note text.</param>
    /// <param name="noteStatusType">Background color of the note.</param>
    /// <param name="buttons">Popup buttons.</param>
    /// <param name="icon">Popup icon.</param>
    /// <param name="thumbnail"></param>
    /// <param name="optionText"></param>
    public static PopupResult ShowDialog(IgTheme theme, IgLang lang,
        string description = "",
        string title = "",
        string heading = "",
        string details = "",
        string note = "",
        StatusType? noteStatusType = null,
        PopupButtons buttons = PopupButtons.OK,
        SHSTOCKICONID? icon = null,
        Image? thumbnail = null,
        string optionText = "")
    {
        var sysIcon = SystemIconApi.GetSystemIcon(icon);

        var frm = new Popup(theme, lang)
        {
            Title = title,
            Heading = heading,
            Description = description,
            Note = note,
            NoteStatusType = noteStatusType ?? StatusType.Neutral,

            Thumbnail = thumbnail ?? sysIcon,
            ThumbnailOverlay = (thumbnail != null && sysIcon != null) ? sysIcon : null,
            ShowTextInput = false,
            ShowInTaskbar = true,
        };

        if (sysIcon != null)
        {
            var formIconHandle = sysIcon.GetHicon();
            frm.Icon = Icon.FromHandle(formIconHandle);
            FormIconApi.SetTaskbarIcon(frm, formIconHandle);
        }

        if (!string.IsNullOrEmpty(details.Trim()))
        {
            frm.Value = details;
            frm.TextInputMultiLine = true;
            frm.TextInputReadOnly = true;
            frm.ShowTextInput = true;

            frm.Width += 200;
        }

        if (!string.IsNullOrEmpty(optionText.Trim()))
        {
            frm.OptionCheckBoxText = optionText;
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
            frm.ShowAcceptButton = true;

            frm.ShowCancelButton = false;
        }

        var exitResult = (PopupExitResult)frm.ShowDialog();


        return new PopupResult()
        {
            ExitResult = exitResult,
            Value = frm.Value,
            IsOptionChecked = frm.OptionCheckBoxChecked,
        };
    }



    #endregion


    /// <summary>
    /// Recalculate and update window height.
    /// </summary>
    public void UpdateHeight()
    {
        // calculate form height
        var contentHeight = lblHeading.Height + lblHeading.Margin.Vertical
            + lblDescription.Height + lblDescription.Margin.Vertical
            + txtValue.Height + txtValue.Margin.Vertical;

        if (!string.IsNullOrEmpty(Note.Trim()))
        {
            contentHeight += panNote.Height + panNote.Padding.Vertical;
        }

        var height = SystemInformation.CaptionHeight + (Padding.Top * 2) +
            Math.Max(picThumbnail.Height, contentHeight) +
            panBottom.Height;

        Height = height;
    }

}



/// <summary>
/// The built-in buttons for Popup.
/// </summary>
public enum PopupButtons : uint
{
    OK = 0,
    Close = 1,
    Yes_No = 2,
    OK_Cancel = 3,
    OK_Close = 4,
    LearnMore_Close = 5,
    Continue_Quit = 6,
}

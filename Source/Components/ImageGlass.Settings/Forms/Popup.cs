/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2024 DUONG DIEU PHAP
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
using ImageGlass.UI;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ImageGlass.Settings;


public partial class Popup : DialogForm
{
    private bool _intValueOnly = false;
    private bool _unsignedIntValueOnly = false;
    private bool _floatValueOnly = false;
    private bool _unsignedFloatValueOnly = false;
    private ColorStatusType _noteStatusType = ColorStatusType.Neutral;


    // Public properties
    #region Public properties

    /// <summary>
    /// Form title
    /// </summary>
    public string Title
    {
        get => Text;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Text = " ";
            }
            else
            {
                Text = value;
            }
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
            var rowIndex = tableMain.GetRow(lblNote);

            if (isVisible)
            {
                lblNote.Visible = true;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.AutoSize;

                // need to set the following row AutoSize to make this row visible
                var chkOptionRowIndex = tableMain.GetRow(ChkOption);
                tableMain.RowStyles[chkOptionRowIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                lblNote.Visible = false;
                tableMain.RowStyles[rowIndex].SizeType = SizeType.Absolute;
                tableMain.RowStyles[rowIndex].Height = 0;
            }
        }
    }


    /// <summary>
    /// Gets, sets the type of the note.
    /// </summary>
    public ColorStatusType NoteStatusType
    {
        get => _noteStatusType;
        set
        {
            _noteStatusType = value;

            lblNote.BackColor = BHelper.GetBackgroundColorForStatus(value, DarkMode);
            lblNote.ForeColor = Config.Theme.ColorPalatte.AppText;
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
            txtValue.Height = 300;
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
    public string RegexPattern { get; set; } = string.Empty;


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


    #endregion // Public properties


    public Popup() : base()
    {
        InitializeComponent();

        lblHeading.Font = new Font(lblHeading.Font.FontFamily, SystemInformation.MenuFont.SizeInPoints * 1.35f);
        CloseFormHotkey = Keys.Escape;
        ShowInTaskbar = false;
        Heading = "";
        Description = "";
        Title = "";
        OptionCheckBoxText = "";
        Note = "";
        Thumbnail = null; // hide thumbnail by default

        ApplyLanguage();
    }


    // Override / Virtual methods
    #region Override / Virtual methods

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

        base.OnLoad(e);

        ApplyTheme(Config.Theme.Settings.IsDarkMode);
    }


    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        SetFocus();
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();


        lblNote.BackColor = BHelper.GetBackgroundColorForStatus(NoteStatusType, darkMode);
        lblNote.ForeColor = Config.Theme.ColorPalatte.AppText;
        SetTextInputStyle(ValidateInput(), darkMode);

        lblHeading.ForeColor = WinColorsApi.GetAccentColor(false)
            .WithBrightness(darkMode ? 0.4f : 0f);

        tableMain.BackColor = Config.Theme.ColorPalatte.AppBg;


        base.ApplyTheme(darkMode, style);
        ResumeLayout();
    }


    protected override void OnSystemAccentColorChanged(SystemAccentColorChangedEventArgs e)
    {
        base.OnSystemAccentColorChanged(e);

        // update the heading color to match system accent color
        lblHeading.ForeColor = SystemAccentColorChangedEventArgs.AccentColor
            .NoAlpha()
            .WithBrightness(DarkMode ? 0.4f : 0f);
    }


    protected override int OnUpdateHeight(bool performUpdate = true)
    {
        var baseHeight = base.OnUpdateHeight(false);

        // calculate form height
        var contentHeight = tableMain.Height;
        var formHeight = contentHeight + baseHeight;

        if (performUpdate && Height != formHeight)
        {
            Height = formHeight;
        }

        return formHeight;
    }


    protected override void CloseFormByKeys()
    {
        // Closes the form and returns <see cref="DialogResult.Abort"/> code.
        DialogResult = DialogResult.Abort;

        base.CloseFormByKeys();
    }

    #endregion // Override / Virtual methods


    // Form and control events
    #region Form and control events

    private void TxtValue_TextChanged(object sender, EventArgs e)
    {
        var isValid = ValidateInput();
        SetTextInputStyle(isValid, DarkMode);
    }

    #endregion // Form and control events


    // Private methods
    #region Private methods

    /// <summary>
    /// Apply language pack
    /// </summary>
    private void ApplyLanguage()
    {
        BtnAccept.Text = Config.Language["_._OK"];
        BtnCancel.Text = Config.Language["_._Cancel"];
    }


    /// <summary>
    /// Set default focus to the form
    /// </summary>
    private void SetFocus()
    {
        // set default focus
        if (!TextInputReadOnly)
        {
            txtValue.Focus();
            txtValue.SelectAll();
        }
        else
        {
            if (ShowAcceptButton)
            {
                BtnAccept.Focus();
            }
            else
            {
                BtnCancel.Focus();
            }
        }
    }


    /// <summary>
    /// Validates the input and shows error.
    /// </summary>
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

            txtValue.BackColor = BHelper.GetBackgroundColorForStatus(ColorStatusType.Danger, darkMode);
        }
        else
        {
            BtnAccept.Enabled = true;
            txtValue.DarkMode = darkMode;
        }
    }

    #endregion // Private methods


    // Static functions
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
    public static PopupResult ShowDialog(
        string description = "",
        string title = "",
        string heading = "",
        string details = "",
        string note = "",
        ColorStatusType? noteStatusType = null,
        PopupButton buttons = PopupButton.OK,
        ShellStockIcon? icon = null,
        Image? thumbnail = null,
        string optionText = "",
        bool topMost = false,
        Form? formOwner = null)
    {
        var sysIcon = SystemIconApi.GetSystemIcon(icon);
        var hasDetails = string.IsNullOrEmpty(details);

        using var frm = new Popup()
        {
            Title = title,
            Heading = heading,
            Description = description,
            NoteStatusType = noteStatusType ?? ColorStatusType.Neutral,
            TextInputReadOnly = hasDetails,
            TextInputMultiLine = hasDetails,

            Thumbnail = thumbnail ?? sysIcon,
            ThumbnailOverlay = (thumbnail != null && sysIcon != null) ? sysIcon : null,
            ShowTextInput = false,
            ShowInTaskbar = true,

            TopMost = topMost,
        };

        if (sysIcon != null)
        {
            var formIconHandle = sysIcon.GetHicon();
            frm.Icon = Icon.FromHandle(formIconHandle);

            FormIconApi.SetTaskbarIcon(frm, formIconHandle);
        }

        if (!string.IsNullOrEmpty(details))
        {
            frm.Value = details;
            frm.Width += 200;
        }

        if (!string.IsNullOrEmpty(optionText))
        {
            frm.OptionCheckBoxText = optionText;
        }

        if (!string.IsNullOrEmpty(note))
        {
            frm.Note = note;
        }

        if (buttons == PopupButton.OK_Cancel)
        {
            frm.AcceptButtonText = Config.Language["_._OK"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = Config.Language["_._Cancel"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButton.OK_Close)
        {
            frm.AcceptButtonText = Config.Language["_._OK"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = Config.Language["_._Close"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButton.Yes_No)
        {
            frm.AcceptButtonText = Config.Language["_._Yes"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = Config.Language["_._No"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButton.LearnMore_Close)
        {
            frm.AcceptButtonText = Config.Language["_._LearnMore"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = Config.Language["_._Close"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButton.Continue_Quit)
        {
            frm.AcceptButtonText = Config.Language["_._Continue"];
            frm.ShowAcceptButton = true;

            frm.CancelButtonText = Config.Language["_._Quit"];
            frm.ShowCancelButton = true;
        }
        else if (buttons == PopupButton.Close)
        {
            frm.ShowAcceptButton = false;

            frm.CancelButtonText = Config.Language["_._Close"];
            frm.ShowCancelButton = true;
        }
        else
        {
            frm.CancelButtonText = Config.Language["_._OK"];
            frm.ShowAcceptButton = true;

            frm.ShowCancelButton = false;
        }

        var exitResult = (PopupExitResult)frm.ShowDialog(formOwner);


        return new PopupResult()
        {
            ExitResult = exitResult,
            Value = frm.Value,
            IsOptionChecked = frm.OptionCheckBoxChecked,
        };
    }



    #endregion

}

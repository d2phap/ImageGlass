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

using ImageGlass.Base;

namespace ImageGlass.UI;


/// <summary>
/// Dialog form with dark mode and backdrop support.
/// </summary>
public partial class DialogForm : ModernForm
{

    // Public properties
    #region Public properties

    /// <summary>
    /// Gets, sets <see cref="BtnAccept"/>'s text.
    /// </summary>
    public string AcceptButtonText { get; set; } = "OK";


    /// <summary>
    /// Gets, sets <see cref="BtnCancel"/>'s text.
    /// </summary>
    public string CancelButtonText { get; set; } = "Cancel";


    public override Keys CloseFormHotkey => Keys.Escape;

    #endregion // Public properties


    /// <summary>
    /// Initializes the new instance of <see cref="DialogForm"/>. This appends
    /// an action bar with <see cref="BtnAccept"/> and <see cref="BtnCancel"/>
    /// to the bottom of the form.
    /// </summary>
    public DialogForm() : base()
    {
        InitializeComponent();

        AppendActionBar();
    }


    // Action bar codes
    #region Action bar codes

    private TableLayoutPanel TableActions;
    private ModernButton BtnAccept;
    private ModernButton BtnCancel;


    /// <summary>
    /// Adds action bar with OK and Cancel buttons to the form.
    /// </summary>
    private void AppendActionBar()
    {
        this.TableActions = new TableLayoutPanel();
        this.BtnAccept = new ModernButton();
        this.BtnCancel = new ModernButton();
        this.TableActions.SuspendLayout();
        this.SuspendLayout();
        // 
        // TableActions
        // 
        this.TableActions.AutoSize = true;
        this.TableActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.TableActions.ColumnCount = 3;
        this.TableActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        this.TableActions.ColumnStyles.Add(new ColumnStyle());
        this.TableActions.ColumnStyles.Add(new ColumnStyle());
        this.TableActions.Controls.Add(this.BtnAccept, 1, 0);
        this.TableActions.Controls.Add(this.BtnCancel, 2, 0);
        this.TableActions.Dock = DockStyle.Bottom;
        this.TableActions.Location = new Point(0, 367);
        this.TableActions.Margin = new Padding(0);
        this.TableActions.Name = "TableActions";
        this.TableActions.RowCount = 1;
        this.TableActions.RowStyles.Add(new RowStyle());
        this.TableActions.Size = new Size(800, 83);
        this.TableActions.TabIndex = 2;
        // 
        // BtnAccept
        // 
        this.BtnAccept.AutoSize = true;
        this.BtnAccept.DarkMode = false;
        this.BtnAccept.ImagePadding = 2;
        this.BtnAccept.Location = new Point(500, 20);
        this.BtnAccept.Margin = new Padding(0, 20, 20, 20);
        this.BtnAccept.MinimumSize = new Size(130, 40);
        this.BtnAccept.Name = "BtnAccept";
        this.BtnAccept.Padding = new Padding(5);
        this.BtnAccept.Size = new Size(130, 43);
        this.BtnAccept.SystemIcon = null;
        this.BtnAccept.TabIndex = 1;
        this.BtnAccept.Text = AcceptButtonText;
        this.BtnAccept.TextImageRelation = TextImageRelation.ImageBeforeText;
        this.BtnAccept.Click += BtnAccept_Click;
        // 
        // BtnCancel
        // 
        this.BtnCancel.AutoSize = true;
        this.BtnCancel.DarkMode = false;
        this.BtnCancel.ImagePadding = 2;
        this.BtnCancel.Location = new Point(650, 20);
        this.BtnCancel.Margin = new Padding(0, 20, 20, 20);
        this.BtnCancel.MinimumSize = new Size(130, 40);
        this.BtnCancel.Name = "BtnCancel";
        this.BtnCancel.Padding = new Padding(5);
        this.BtnCancel.Size = new Size(130, 43);
        this.BtnCancel.SystemIcon = null;
        this.BtnCancel.TabIndex = 2;
        this.BtnCancel.Text = CancelButtonText;
        this.BtnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
        this.BtnCancel.Click += BtnCancel_Click;

        this.TableActions.ResumeLayout(false);
        this.TableActions.PerformLayout();

        this.Controls.Add(this.TableActions);
        this.ResumeLayout(false);
        this.PerformLayout();
    }


    private void BtnAccept_Click(object? sender, EventArgs e)
    {
        AcceptForm();
    }


    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        CancelForm();
    }

    #endregion // Action bar codes


    // Override / Virtual methods
    #region Override / Virtual methods

    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();

        EnableTransparent = darkMode;
        TableActions.BackColor = BackColor.InvertBlackOrWhite(30);

        // dark mode
        BtnAccept.DarkMode = BtnCancel.DarkMode = darkMode;


        if (!darkMode)
        {
            BackColor = Color.White;
            TableActions.BackColor = BackColor.InvertBlackOrWhite(10);
        }


        ResumeLayout(false);
        base.ApplyTheme(darkMode, style);
    }


    protected override void OnLoad(EventArgs e)
    {
        // adjust form size
        _ = OnUpdateHeight();

        base.OnLoad(e);

        this.TableActions.BringToFront();
    }


    protected override void CloseFormByKeys()
    {
        // Closes the form and returns <see cref="DialogResult.Abort"/> code.
        DialogResult = DialogResult.Abort;

        base.CloseFormByKeys();
    }


    /// <summary>
    /// Recalculates and updates window height on <see cref="Form.OnLoad(EventArgs)"/> event.
    /// </summary>
    protected virtual int OnUpdateHeight(bool performUpdate = true)
    {
        // calculate form height
        var contentHeight = TableActions.Height + (TableActions.Padding.Vertical * 2);


        if (performUpdate)
        {
            Height = contentHeight;
        }

        return contentHeight;
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogResult.OK"/> code.
    /// </summary>
    protected virtual void AcceptForm()
    {
        DialogResult = DialogResult.OK;
        Close();
    }


    /// <summary>
    /// Closes the form and returns <see cref="DialogResult.Cancel"/> code.
    /// </summary>
    protected virtual void CancelForm()
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    #endregion // Override / Virtual methods


}

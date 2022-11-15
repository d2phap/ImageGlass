namespace ImageGlass.UI
{
    partial class Popup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.panNote = new System.Windows.Forms.FlowLayoutPanel();
            this.lblNote = new System.Windows.Forms.Label();
            this.lblHeading = new System.Windows.Forms.Label();
            this.txtValue = new ImageGlass.UI.ModernTextBox();
            this.lblDescription = new System.Windows.Forms.Label();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.ChkOption = new System.Windows.Forms.CheckBox();
            this.tableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.BtnAccept = new ImageGlass.UI.ModernButton();
            this.BtnCancel = new ImageGlass.UI.ModernButton();
            this.tableMain.SuspendLayout();
            this.panNote.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            this.tableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableMain
            // 
            this.tableMain.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.ColumnCount = 2;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.panNote, 0, 4);
            this.tableMain.Controls.Add(this.lblHeading, 1, 1);
            this.tableMain.Controls.Add(this.txtValue, 1, 3);
            this.tableMain.Controls.Add(this.lblDescription, 1, 2);
            this.tableMain.Controls.Add(this.picThumbnail, 0, 1);
            this.tableMain.Controls.Add(this.ChkOption, 1, 4);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMain.Location = new System.Drawing.Point(0, 20);
            this.tableMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 5;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.Size = new System.Drawing.Size(600, 312);
            this.tableMain.TabIndex = 1;
            // 
            // panNote
            // 
            this.panNote.AutoSize = true;
            this.panNote.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panNote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(52)))), ((int)(((byte)(32)))));
            this.tableMain.SetColumnSpan(this.panNote, 10);
            this.panNote.Controls.Add(this.lblNote);
            this.panNote.Dock = System.Windows.Forms.DockStyle.Top;
            this.panNote.Location = new System.Drawing.Point(20, 164);
            this.panNote.Margin = new System.Windows.Forms.Padding(20, 20, 20, 0);
            this.panNote.Name = "panNote";
            this.panNote.Padding = new System.Windows.Forms.Padding(20, 10, 20, 10);
            this.panNote.Size = new System.Drawing.Size(560, 43);
            this.panNote.TabIndex = 102;
            this.panNote.Visible = false;
            // 
            // lblNote
            // 
            this.lblNote.AutoSize = true;
            this.lblNote.BackColor = System.Drawing.Color.Transparent;
            this.lblNote.ForeColor = System.Drawing.Color.White;
            this.lblNote.Location = new System.Drawing.Point(20, 10);
            this.lblNote.Margin = new System.Windows.Forms.Padding(0);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(50, 23);
            this.lblNote.TabIndex = 5;
            this.lblNote.Text = "[###]";
            // 
            // lblHeading
            // 
            this.lblHeading.AutoSize = true;
            this.lblHeading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblHeading.Location = new System.Drawing.Point(135, 0);
            this.lblHeading.Margin = new System.Windows.Forms.Padding(15, 0, 20, 20);
            this.lblHeading.Name = "lblHeading";
            this.lblHeading.Size = new System.Drawing.Size(115, 31);
            this.lblHeading.TabIndex = 100;
            this.lblHeading.Text = "[Heading]";
            // 
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.SystemColors.Window;
            this.txtValue.DarkMode = false;
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtValue.Location = new System.Drawing.Point(140, 94);
            this.txtValue.Margin = new System.Windows.Forms.Padding(20, 0, 20, 20);
            this.txtValue.MaximumSize = new System.Drawing.Size(0, 240);
            this.txtValue.Name = "txtValue";
            this.txtValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtValue.Size = new System.Drawing.Size(440, 30);
            this.txtValue.TabIndex = 0;
            this.txtValue.TextChanged += new System.EventHandler(this.TxtValue_TextChanged);
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(135, 51);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(15, 0, 20, 20);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(82, 23);
            this.lblDescription.TabIndex = 101;
            this.lblDescription.Text = "[Content]";
            // 
            // picThumbnail
            // 
            this.picThumbnail.BackColor = System.Drawing.Color.Transparent;
            this.picThumbnail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picThumbnail.Location = new System.Drawing.Point(20, 0);
            this.picThumbnail.Margin = new System.Windows.Forms.Padding(20, 0, 0, 20);
            this.picThumbnail.MaximumSize = new System.Drawing.Size(100, 100);
            this.picThumbnail.Name = "picThumbnail";
            this.tableMain.SetRowSpan(this.picThumbnail, 3);
            this.picThumbnail.Size = new System.Drawing.Size(100, 100);
            this.picThumbnail.TabIndex = 4;
            this.picThumbnail.TabStop = false;
            this.picThumbnail.Visible = false;
            // 
            // ChkOption
            // 
            this.ChkOption.AutoSize = true;
            this.tableMain.SetColumnSpan(this.ChkOption, 2);
            this.ChkOption.Location = new System.Drawing.Point(0, 207);
            this.ChkOption.Margin = new System.Windows.Forms.Padding(0);
            this.ChkOption.Name = "ChkOption";
            this.ChkOption.Padding = new System.Windows.Forms.Padding(20, 10, 20, 20);
            this.ChkOption.Size = new System.Drawing.Size(329, 57);
            this.ChkOption.TabIndex = 1;
            this.ChkOption.Text = "[Do not show this message again]";
            // 
            // tableBottom
            // 
            this.tableBottom.AutoSize = true;
            this.tableBottom.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableBottom.ColumnCount = 3;
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.Controls.Add(this.BtnAccept, 1, 0);
            this.tableBottom.Controls.Add(this.BtnCancel, 2, 0);
            this.tableBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableBottom.Location = new System.Drawing.Point(0, 410);
            this.tableBottom.Margin = new System.Windows.Forms.Padding(0);
            this.tableBottom.Name = "tableBottom";
            this.tableBottom.RowCount = 1;
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableBottom.Size = new System.Drawing.Size(600, 83);
            this.tableBottom.TabIndex = 0;
            // 
            // BtnAccept
            // 
            this.BtnAccept.AutoSize = true;
            this.BtnAccept.DarkMode = false;
            this.BtnAccept.ImagePadding = 2;
            this.BtnAccept.Location = new System.Drawing.Point(300, 20);
            this.BtnAccept.Margin = new System.Windows.Forms.Padding(0, 20, 20, 20);
            this.BtnAccept.MinimumSize = new System.Drawing.Size(130, 40);
            this.BtnAccept.Name = "BtnAccept";
            this.BtnAccept.Padding = new System.Windows.Forms.Padding(5);
            this.BtnAccept.Size = new System.Drawing.Size(130, 43);
            this.BtnAccept.SystemIcon = null;
            this.BtnAccept.TabIndex = 1;
            this.BtnAccept.Text = "[OK]";
            this.BtnAccept.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnAccept.Click += new System.EventHandler(this.BtnAccept_Click);
            // 
            // BtnCancel
            // 
            this.BtnCancel.AutoSize = true;
            this.BtnCancel.DarkMode = false;
            this.BtnCancel.ImagePadding = 2;
            this.BtnCancel.Location = new System.Drawing.Point(450, 20);
            this.BtnCancel.Margin = new System.Windows.Forms.Padding(0, 20, 20, 20);
            this.BtnCancel.MinimumSize = new System.Drawing.Size(130, 40);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Padding = new System.Windows.Forms.Padding(5);
            this.BtnCancel.Size = new System.Drawing.Size(130, 43);
            this.BtnCancel.SystemIcon = null;
            this.BtnCancel.TabIndex = 2;
            this.BtnCancel.Text = "[Cancel]";
            this.BtnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // Popup
            // 
            this.AcceptButton = this.BtnAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackdropMargin = new System.Windows.Forms.Padding(-1);
            this.BackdropStyle = ImageGlass.Base.BackdropStyle.MicaAlt;
            this.ClientSize = new System.Drawing.Size(600, 493);
            this.ControlBox = false;
            this.Controls.Add(this.tableBottom);
            this.Controls.Add(this.tableMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 240);
            this.Name = "Popup";
            this.Padding = new System.Windows.Forms.Padding(0, 20, 0, 0);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "[Title]";
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            this.panNote.ResumeLayout(false);
            this.panNote.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            this.tableBottom.ResumeLayout(false);
            this.tableBottom.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TableLayoutPanel tableMain;
        private TableLayoutPanel tableBottom;
        private Label lblDescription;
        private ModernTextBox txtValue;
        private PictureBox picThumbnail;
        private Label lblHeading;
        private ModernButton BtnAccept;
        private ModernButton BtnCancel;
        private CheckBox ChkOption;
        private FlowLayoutPanel panNote;
        private Label lblNote;
    }
}
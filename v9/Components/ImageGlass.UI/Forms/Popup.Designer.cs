namespace ImageGlass.UI.BuiltInForms
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
            this.lblHeading = new System.Windows.Forms.Label();
            this.txtValue = new ImageGlass.UI.ModernTextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.picThumbnail = new System.Windows.Forms.PictureBox();
            this.panBottom = new System.Windows.Forms.Panel();
            this.tableBottom = new System.Windows.Forms.TableLayoutPanel();
            this.BtnAccept = new ImageGlass.UI.ModernButton();
            this.BtnCancel = new ImageGlass.UI.ModernButton();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).BeginInit();
            this.panBottom.SuspendLayout();
            this.tableBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableMain
            // 
            this.tableMain.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.ColumnCount = 2;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.lblHeading, 1, 1);
            this.tableMain.Controls.Add(this.txtValue, 1, 3);
            this.tableMain.Controls.Add(this.lblTitle, 0, 0);
            this.tableMain.Controls.Add(this.lblDescription, 1, 2);
            this.tableMain.Controls.Add(this.picThumbnail, 0, 1);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMain.Location = new System.Drawing.Point(1, 1);
            this.tableMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 4;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.Size = new System.Drawing.Size(578, 898);
            this.tableMain.TabIndex = 0;
            // 
            // lblHeading
            // 
            this.lblHeading.AutoSize = true;
            this.lblHeading.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblHeading.Location = new System.Drawing.Point(135, 73);
            this.lblHeading.Margin = new System.Windows.Forms.Padding(15, 0, 20, 20);
            this.lblHeading.Name = "lblHeading";
            this.lblHeading.Size = new System.Drawing.Size(115, 31);
            this.lblHeading.TabIndex = 4;
            this.lblHeading.Text = "[Heading]";
            // 
            // txtValue
            // 
            this.txtValue.BackColor = System.Drawing.SystemColors.Window;
            this.txtValue.DarkMode = false;
            this.txtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtValue.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtValue.Location = new System.Drawing.Point(140, 167);
            this.txtValue.Margin = new System.Windows.Forms.Padding(20, 0, 20, 20);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(418, 30);
            this.txtValue.TabIndex = 1;
            this.txtValue.TextChanged += new System.EventHandler(this.TxtValue_TextChanged);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.tableMain.SetColumnSpan(this.lblTitle, 2);
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0, 0, 0, 30);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Padding = new System.Windows.Forms.Padding(15, 10, 0, 10);
            this.lblTitle.Size = new System.Drawing.Size(578, 43);
            this.lblTitle.TabIndex = 2;
            this.lblTitle.Text = "[Title]";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(135, 124);
            this.lblDescription.Margin = new System.Windows.Forms.Padding(15, 0, 20, 20);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(82, 23);
            this.lblDescription.TabIndex = 3;
            this.lblDescription.Text = "[Content]";
            // 
            // picThumbnail
            // 
            this.picThumbnail.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.picThumbnail.Location = new System.Drawing.Point(20, 73);
            this.picThumbnail.Margin = new System.Windows.Forms.Padding(20, 0, 0, 20);
            this.picThumbnail.MaximumSize = new System.Drawing.Size(100, 100);
            this.picThumbnail.Name = "picThumbnail";
            this.tableMain.SetRowSpan(this.picThumbnail, 3);
            this.picThumbnail.Size = new System.Drawing.Size(100, 100);
            this.picThumbnail.TabIndex = 4;
            this.picThumbnail.TabStop = false;
            this.picThumbnail.Visible = false;
            // 
            // panBottom
            // 
            this.panBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panBottom.Controls.Add(this.tableBottom);
            this.panBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panBottom.Location = new System.Drawing.Point(1, 223);
            this.panBottom.Name = "panBottom";
            this.panBottom.Padding = new System.Windows.Forms.Padding(20);
            this.panBottom.Size = new System.Drawing.Size(578, 81);
            this.panBottom.TabIndex = 1;
            // 
            // tableBottom
            // 
            this.tableBottom.BackColor = System.Drawing.Color.Transparent;
            this.tableBottom.ColumnCount = 3;
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableBottom.Controls.Add(this.BtnAccept, 1, 0);
            this.tableBottom.Controls.Add(this.BtnCancel, 2, 0);
            this.tableBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableBottom.Location = new System.Drawing.Point(20, 20);
            this.tableBottom.Name = "tableBottom";
            this.tableBottom.RowCount = 1;
            this.tableBottom.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBottom.Size = new System.Drawing.Size(538, 41);
            this.tableBottom.TabIndex = 0;
            // 
            // BtnAccept
            // 
            this.BtnAccept.AutoSize = true;
            this.BtnAccept.DarkMode = false;
            this.BtnAccept.ImagePadding = 2;
            this.BtnAccept.Location = new System.Drawing.Point(275, 0);
            this.BtnAccept.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.BtnAccept.MinimumSize = new System.Drawing.Size(120, 0);
            this.BtnAccept.Name = "BtnAccept";
            this.BtnAccept.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.BtnAccept.Size = new System.Drawing.Size(120, 40);
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
            this.BtnCancel.Location = new System.Drawing.Point(415, 0);
            this.BtnCancel.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.BtnCancel.MinimumSize = new System.Drawing.Size(120, 0);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.BtnCancel.Size = new System.Drawing.Size(123, 40);
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
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(580, 305);
            this.Controls.Add(this.panBottom);
            this.Controls.Add(this.tableMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(580, 240);
            this.Name = "Popup";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InputBox";
            this.Load += new System.EventHandler(this.InputForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputForm_KeyDown);
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picThumbnail)).EndInit();
            this.panBottom.ResumeLayout(false);
            this.tableBottom.ResumeLayout(false);
            this.tableBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TableLayoutPanel tableMain;
        private Panel panBottom;
        private TableLayoutPanel tableBottom;
        private Label lblTitle;
        private Label lblDescription;
        private ModernTextBox txtValue;
        private PictureBox picThumbnail;
        private Label lblHeading;
        private ModernButton BtnAccept;
        private ModernButton BtnCancel;
    }
}
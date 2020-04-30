namespace ImageGlass
{
    partial class frmCrop
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
            this.btnSnapTo = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblY = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblX = new System.Windows.Forms.Label();
            this.tableFactors = new System.Windows.Forms.TableLayoutPanel();
            this.numHeight = new System.Windows.Forms.NumericUpDown();
            this.numWidth = new System.Windows.Forms.NumericUpDown();
            this.numY = new System.Windows.Forms.NumericUpDown();
            this.numX = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnSaveAs = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.tableActions = new System.Windows.Forms.TableLayoutPanel();
            this.lblFormTitle = new System.Windows.Forms.Label();
            this.tableFactors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).BeginInit();
            this.tableActions.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSnapTo
            // 
            this.btnSnapTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSnapTo.AutoSize = true;
            this.btnSnapTo.BackColor = System.Drawing.Color.Teal;
            this.btnSnapTo.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.btnSnapTo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.btnSnapTo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnSnapTo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSnapTo.ForeColor = System.Drawing.Color.White;
            this.btnSnapTo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSnapTo.Location = new System.Drawing.Point(164, 2);
            this.btnSnapTo.Margin = new System.Windows.Forms.Padding(4);
            this.btnSnapTo.Name = "btnSnapTo";
            this.btnSnapTo.Size = new System.Drawing.Size(74, 40);
            this.btnSnapTo.TabIndex = 5;
            this.btnSnapTo.Text = "^";
            this.btnSnapTo.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.AutoSize = true;
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(15)))), ((int)(((byte)(29)))));
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(62)))), ((int)(((byte)(74)))));
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(17)))), ((int)(((byte)(35)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(241, 2);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(74, 40);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // lblY
            // 
            this.lblY.AutoSize = true;
            this.lblY.BackColor = System.Drawing.Color.Transparent;
            this.lblY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblY.ForeColor = System.Drawing.Color.White;
            this.lblY.Location = new System.Drawing.Point(15, 37);
            this.lblY.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(79, 37);
            this.lblY.TabIndex = 17;
            this.lblY.Text = "Y:";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.BackColor = System.Drawing.Color.Transparent;
            this.lblHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeight.ForeColor = System.Drawing.Color.White;
            this.lblHeight.Location = new System.Drawing.Point(15, 111);
            this.lblHeight.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(79, 37);
            this.lblHeight.TabIndex = 16;
            this.lblHeight.Text = "[Height:]";
            this.lblHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.BackColor = System.Drawing.Color.Transparent;
            this.lblWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblWidth.ForeColor = System.Drawing.Color.White;
            this.lblWidth.Location = new System.Drawing.Point(15, 74);
            this.lblWidth.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(79, 37);
            this.lblWidth.TabIndex = 14;
            this.lblWidth.Text = "[Width:]";
            this.lblWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblX
            // 
            this.lblX.AutoSize = true;
            this.lblX.BackColor = System.Drawing.Color.Transparent;
            this.lblX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblX.ForeColor = System.Drawing.Color.White;
            this.lblX.Location = new System.Drawing.Point(15, 0);
            this.lblX.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(79, 37);
            this.lblX.TabIndex = 19;
            this.lblX.Text = "X:";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableFactors
            // 
            this.tableFactors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableFactors.BackColor = System.Drawing.Color.Transparent;
            this.tableFactors.ColumnCount = 2;
            this.tableFactors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableFactors.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableFactors.Controls.Add(this.numHeight, 1, 3);
            this.tableFactors.Controls.Add(this.numWidth, 1, 2);
            this.tableFactors.Controls.Add(this.numY, 1, 1);
            this.tableFactors.Controls.Add(this.lblX, 0, 0);
            this.tableFactors.Controls.Add(this.lblY, 0, 1);
            this.tableFactors.Controls.Add(this.lblWidth, 0, 2);
            this.tableFactors.Controls.Add(this.lblHeight, 0, 3);
            this.tableFactors.Controls.Add(this.numX, 1, 0);
            this.tableFactors.Location = new System.Drawing.Point(0, 64);
            this.tableFactors.Name = "tableFactors";
            this.tableFactors.RowCount = 5;
            this.tableFactors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableFactors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableFactors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableFactors.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableFactors.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableFactors.Size = new System.Drawing.Size(315, 165);
            this.tableFactors.TabIndex = 21;
            // 
            // numHeight
            // 
            this.numHeight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.numHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numHeight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numHeight.ForeColor = System.Drawing.Color.White;
            this.numHeight.Location = new System.Drawing.Point(101, 114);
            this.numHeight.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.numHeight.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numHeight.Name = "numHeight";
            this.numHeight.Size = new System.Drawing.Size(194, 31);
            this.numHeight.TabIndex = 4;
            this.numHeight.ValueChanged += new System.EventHandler(this.Numeric_ValueChanged);
            this.numHeight.Click += new System.EventHandler(this.Numeric_Click);
            // 
            // numWidth
            // 
            this.numWidth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.numWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numWidth.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numWidth.ForeColor = System.Drawing.Color.White;
            this.numWidth.Location = new System.Drawing.Point(101, 77);
            this.numWidth.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.numWidth.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numWidth.Name = "numWidth";
            this.numWidth.Size = new System.Drawing.Size(194, 31);
            this.numWidth.TabIndex = 3;
            this.numWidth.ValueChanged += new System.EventHandler(this.Numeric_ValueChanged);
            this.numWidth.Click += new System.EventHandler(this.Numeric_Click);
            // 
            // numY
            // 
            this.numY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.numY.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numY.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numY.ForeColor = System.Drawing.Color.White;
            this.numY.Location = new System.Drawing.Point(101, 40);
            this.numY.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.numY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numY.Name = "numY";
            this.numY.Size = new System.Drawing.Size(194, 31);
            this.numY.TabIndex = 2;
            this.numY.ValueChanged += new System.EventHandler(this.Numeric_ValueChanged);
            this.numY.Click += new System.EventHandler(this.Numeric_Click);
            // 
            // numX
            // 
            this.numX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.numX.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.numX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.numX.ForeColor = System.Drawing.Color.White;
            this.numX.Location = new System.Drawing.Point(101, 3);
            this.numX.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.numX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.numX.Name = "numX";
            this.numX.Size = new System.Drawing.Size(194, 31);
            this.numX.TabIndex = 1;
            this.numX.ValueChanged += new System.EventHandler(this.Numeric_ValueChanged);
            this.numX.Click += new System.EventHandler(this.Numeric_Click);
            // 
            // btnSave
            // 
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(20, 15);
            this.btnSave.Margin = new System.Windows.Forms.Padding(20, 15, 5, 5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(133, 45);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "[Save]";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.CropActionButton_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveAs.ForeColor = System.Drawing.Color.White;
            this.btnSaveAs.Location = new System.Drawing.Point(163, 15);
            this.btnSaveAs.Margin = new System.Windows.Forms.Padding(5, 15, 20, 5);
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(133, 45);
            this.btnSaveAs.TabIndex = 23;
            this.btnSaveAs.Text = "[Save as]";
            this.btnSaveAs.UseVisualStyleBackColor = true;
            this.btnSaveAs.Click += new System.EventHandler(this.CropActionButton_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopy.ForeColor = System.Drawing.Color.White;
            this.btnCopy.Location = new System.Drawing.Point(20, 70);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(20, 5, 5, 15);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(133, 45);
            this.btnCopy.TabIndex = 24;
            this.btnCopy.Text = "[Copy]";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.CropActionButton_Click);
            // 
            // btnClear
            // 
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(163, 70);
            this.btnClear.Margin = new System.Windows.Forms.Padding(5, 5, 20, 15);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(133, 45);
            this.btnClear.TabIndex = 25;
            this.btnClear.Text = "[Clear]";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // tableActions
            // 
            this.tableActions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.tableActions.ColumnCount = 2;
            this.tableActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableActions.Controls.Add(this.btnSave, 0, 0);
            this.tableActions.Controls.Add(this.btnClear, 1, 1);
            this.tableActions.Controls.Add(this.btnSaveAs, 1, 0);
            this.tableActions.Controls.Add(this.btnCopy, 0, 1);
            this.tableActions.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableActions.Location = new System.Drawing.Point(0, 230);
            this.tableActions.Name = "tableActions";
            this.tableActions.RowCount = 2;
            this.tableActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableActions.Size = new System.Drawing.Size(316, 130);
            this.tableActions.TabIndex = 26;
            // 
            // lblFormTitle
            // 
            this.lblFormTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFormTitle.AutoEllipsis = true;
            this.lblFormTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblFormTitle.ForeColor = System.Drawing.Color.White;
            this.lblFormTitle.Location = new System.Drawing.Point(0, 5);
            this.lblFormTitle.Margin = new System.Windows.Forms.Padding(15, 0, 4, 0);
            this.lblFormTitle.Name = "lblFormTitle";
            this.lblFormTitle.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblFormTitle.Size = new System.Drawing.Size(156, 32);
            this.lblFormTitle.TabIndex = 28;
            this.lblFormTitle.Text = "[Cropping]";
            this.lblFormTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmCrop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(74)))), ((int)(((byte)(72)))));
            this.ClientSize = new System.Drawing.Size(316, 360);
            this.Controls.Add(this.lblFormTitle);
            this.Controls.Add(this.tableActions);
            this.Controls.Add(this.tableFactors);
            this.Controls.Add(this.btnSnapTo);
            this.Controls.Add(this.btnClose);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(316, 360);
            this.Name = "frmCrop";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmCrop";
            this.Load += new System.EventHandler(this.frmCrop_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmCrop_KeyDown);
            this.tableFactors.ResumeLayout(false);
            this.tableFactors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numX)).EndInit();
            this.tableActions.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSnapTo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.TableLayoutPanel tableFactors;
        private System.Windows.Forms.NumericUpDown numX;
        private System.Windows.Forms.NumericUpDown numHeight;
        private System.Windows.Forms.NumericUpDown numWidth;
        private System.Windows.Forms.NumericUpDown numY;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnSaveAs;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TableLayoutPanel tableActions;
        private System.Windows.Forms.Label lblFormTitle;
    }
}
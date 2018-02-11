namespace ImageGlass
{
    partial class frmCustToolbar
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
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveLeft = new System.Windows.Forms.Button();
            this.btnMoveRight = new System.Windows.Forms.Button();
            this.availButtons = new System.Windows.Forms.ListView();
            this.usedButtons = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMoveUp.Location = new System.Drawing.Point(408, 114);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(37, 34);
            this.btnMoveUp.TabIndex = 2;
            this.btnMoveUp.Text = "p";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMoveDown.Location = new System.Drawing.Point(410, 169);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(34, 35);
            this.btnMoveDown.TabIndex = 3;
            this.btnMoveDown.Text = "q";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnMoveLeft
            // 
            this.btnMoveLeft.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMoveLeft.Location = new System.Drawing.Point(192, 116);
            this.btnMoveLeft.Name = "btnMoveLeft";
            this.btnMoveLeft.Size = new System.Drawing.Size(35, 30);
            this.btnMoveLeft.TabIndex = 4;
            this.btnMoveLeft.Text = "t";
            this.btnMoveLeft.UseVisualStyleBackColor = true;
            this.btnMoveLeft.Click += new System.EventHandler(this.btnMoveLeft_Click);
            // 
            // btnMoveRight
            // 
            this.btnMoveRight.Font = new System.Drawing.Font("Wingdings 3", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnMoveRight.Location = new System.Drawing.Point(193, 179);
            this.btnMoveRight.Name = "btnMoveRight";
            this.btnMoveRight.Size = new System.Drawing.Size(33, 35);
            this.btnMoveRight.TabIndex = 5;
            this.btnMoveRight.Text = "u";
            this.btnMoveRight.UseVisualStyleBackColor = true;
            this.btnMoveRight.Click += new System.EventHandler(this.btnMoveRight_Click);
            // 
            // availButtons
            // 
            this.availButtons.GridLines = true;
            this.availButtons.HideSelection = false;
            this.availButtons.Location = new System.Drawing.Point(21, 48);
            this.availButtons.Name = "availButtons";
            this.availButtons.ShowGroups = false;
            this.availButtons.ShowItemToolTips = true;
            this.availButtons.Size = new System.Drawing.Size(151, 283);
            this.availButtons.TabIndex = 6;
            this.availButtons.UseCompatibleStateImageBehavior = false;
            this.availButtons.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // usedButtons
            // 
            this.usedButtons.AutoArrange = false;
            this.usedButtons.FullRowSelect = true;
            this.usedButtons.GridLines = true;
            this.usedButtons.HideSelection = false;
            this.usedButtons.LabelWrap = false;
            this.usedButtons.Location = new System.Drawing.Point(245, 48);
            this.usedButtons.Name = "usedButtons";
            this.usedButtons.ShowGroups = false;
            this.usedButtons.ShowItemToolTips = true;
            this.usedButtons.Size = new System.Drawing.Size(145, 283);
            this.usedButtons.TabIndex = 6;
            this.usedButtons.UseCompatibleStateImageBehavior = false;
            this.usedButtons.View = System.Windows.Forms.View.List;
            this.usedButtons.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            // 
            // frmCustToolbar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 414);
            this.Controls.Add(this.usedButtons);
            this.Controls.Add(this.availButtons);
            this.Controls.Add(this.btnMoveRight);
            this.Controls.Add(this.btnMoveLeft);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnMoveUp);
            this.Name = "frmCustToolbar";
            this.Text = "frmCustToolbar";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCustToolbar_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveLeft;
        private System.Windows.Forms.Button btnMoveRight;
        private System.Windows.Forms.ListView availButtons;
        private System.Windows.Forms.ListView usedButtons;
    }
}
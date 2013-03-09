using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

namespace Language
{
    /// <summary>
    /// Summary description for ctlMain.
    /// </summary>
    public class ctlMain : System.Windows.Forms.UserControl
    {
        private Label lblRAM;
        private Label label1;
        private TrackBar trbBoost;
        private Label lblBoostValue;
        private Label label2;
        private Label label3;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ctlMain()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ctlMain));
            this.lblRAM = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.trbBoost = new System.Windows.Forms.TrackBar();
            this.lblBoostValue = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trbBoost)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRAM
            // 
            this.lblRAM.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblRAM.AutoSize = true;
            this.lblRAM.Location = new System.Drawing.Point(26, 76);
            this.lblRAM.Name = "lblRAM";
            this.lblRAM.Size = new System.Drawing.Size(139, 15);
            this.lblRAM.TabIndex = 5;
            this.lblRAM.Text = "Installed memory (RAM):";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.Location = new System.Drawing.Point(26, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(463, 43);
            this.label1.TabIndex = 10;
            this.label1.Text = "Use Random Access Memory to boost up image loading. If your RAM is low, keeping d" +
                "efault setting is recommended";
            // 
            // trbBoost
            // 
            this.trbBoost.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.trbBoost.BackColor = System.Drawing.Color.White;
            this.trbBoost.Location = new System.Drawing.Point(112, 115);
            this.trbBoost.Maximum = 4;
            this.trbBoost.Name = "trbBoost";
            this.trbBoost.Size = new System.Drawing.Size(377, 45);
            this.trbBoost.TabIndex = 11;
            this.trbBoost.Scroll += new System.EventHandler(this.trbBoost_Scroll);
            // 
            // lblBoostValue
            // 
            this.lblBoostValue.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblBoostValue.AutoSize = true;
            this.lblBoostValue.Location = new System.Drawing.Point(26, 115);
            this.lblBoostValue.Name = "lblBoostValue";
            this.lblBoostValue.Size = new System.Drawing.Size(80, 15);
            this.lblBoostValue.TabIndex = 12;
            this.lblBoostValue.Text = "Boost value: 0";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(26, 195);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(463, 81);
            this.label2.TabIndex = 14;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(26, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 15);
            this.label3.TabIndex = 15;
            this.label3.Text = "What does it do?";
            // 
            // ctlMain
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblBoostValue);
            this.Controls.Add(this.trbBoost);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRAM);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(517, 328);
            this.Load += new System.EventHandler(this.ctlMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trbBoost)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private void trbBoost_Scroll(object sender, EventArgs e)
        {
            lblBoostValue.Text = "Boost value: " + trbBoost.Value.ToString();

            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            Microsoft.Win32.Registry.SetValue(hkey, "BoostValue", trbBoost.Value.ToString());
        }

        private void ctlMain_Load(object sender, EventArgs e)
        {
            int v = 0;
            string hkey = "HKEY_CURRENT_USER\\SOFTWARE\\PhapSoftware\\ImageGlass\\";
            int.TryParse(Microsoft.Win32.Registry.GetValue(hkey, "BoostValue", 0).ToString(),
                         out v);

            trbBoost.Value = v;
            lblBoostValue.Text = "Boost value: " + trbBoost.Value.ToString();

            
            
        }
    }
}

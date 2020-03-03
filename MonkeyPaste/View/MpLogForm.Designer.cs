using MonkeyPaste.View;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MonkeyPaste {
    public partial class MpLogForm : MpResizableBorderlessForm {
       
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpLogForm));
            this.mpPanel1 = new MonkeyPaste.MpAdvancedPanel();
            this.SuspendLayout();
            // 
            // mpPanel1
            // 
            this.mpPanel1.BackColor = System.Drawing.Color.Transparent;
            this.mpPanel1.BackgroundGradientMode = BevelPanel.AdvancedPanel.PanelGradientMode.Vertical;
            this.mpPanel1.EdgeWidth = 1;
            this.mpPanel1.EndColor = System.Drawing.Color.SandyBrown;
            this.mpPanel1.FlatBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(102)))), ((int)(((byte)(102)))), ((int)(((byte)(102)))));
            this.mpPanel1.Location = new System.Drawing.Point(12, 75);
            this.mpPanel1.Name = "mpPanel1";
            this.mpPanel1.RectRadius = 15;
            this.mpPanel1.ShadowColor = System.Drawing.Color.DimGray;
            this.mpPanel1.ShadowShift = 0;
            this.mpPanel1.ShadowStyle = BevelPanel.AdvancedPanel.ShadowMode.ForwardDiagonal;
            this.mpPanel1.Size = new System.Drawing.Size(200, 150);
            this.mpPanel1.StartColor = System.Drawing.Color.SandyBrown;
            this.mpPanel1.Style = BevelPanel.AdvancedPanel.BevelStyle.Flat;
            this.mpPanel1.TabIndex = 0;
            this.mpPanel1.ViewData = this.mpPanel1;
            this.mpPanel1.ViewId = 1820011674;
            this.mpPanel1.ViewName = "MonkeyPaste.MpPanel_1820011674";
            this.mpPanel1.ViewType = "MonkeyPaste.MpPanel";
            // 
            // MpLogForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(799, 359);
            this.Controls.Add(this.mpPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MpLogForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log";
            this.ResumeLayout(false);

        }


        #endregion

        private MpAdvancedPanel mpPanel1;
    }
}
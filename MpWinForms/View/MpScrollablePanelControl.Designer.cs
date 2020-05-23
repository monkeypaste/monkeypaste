namespace MonkeyPaste {
    partial class MpMpScrollAblePanelControl {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.ScrollContainter = new System.Windows.Forms.Panel();
            this.ScrollAt = new System.Windows.Forms.Panel();
            this.ScrollPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // ScrollContainter
            // 
            this.ScrollContainter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ScrollContainter.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.ScrollContainter.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ScrollContainter.Location = new System.Drawing.Point(272,3);
            this.ScrollContainter.Name = "ScrollContainter";
            this.ScrollContainter.Size = new System.Drawing.Size(10,222);
            this.ScrollContainter.TabIndex = 0;
            // 
            // ScrollAt
            // 
            this.ScrollAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ScrollAt.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ScrollAt.Location = new System.Drawing.Point(273,4);
            this.ScrollAt.Name = "ScrollAt";
            this.ScrollAt.Size = new System.Drawing.Size(7,16);
            this.ScrollAt.TabIndex = 1;
            this.ScrollAt.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ScrollAt_MouseMove);
            this.ScrollAt.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ScrollAt_MouseDown);
            this.ScrollAt.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ScrollAt_MouseUp);
            // 
            // ScrollPanel
            // 
            this.ScrollPanel.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.ScrollPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ScrollPanel.Location = new System.Drawing.Point(-1,-1);
            this.ScrollPanel.Name = "ScrollPanel";
            this.ScrollPanel.Size = new System.Drawing.Size(268,409);
            this.ScrollPanel.TabIndex = 2;
            this.ScrollPanel.MouseLeave += new System.EventHandler(this.ScrollPanel_MouseLeave);
            this.ScrollPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MpScrollAblePanelControl_MouseMove);
            this.ScrollPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MpScrollAblePanelControl_MouseDown);
            this.ScrollPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MpScrollAblePanelControl_MouseUp);
            this.ScrollPanel.SizeChanged += new System.EventHandler(this.MpScrollAblePanelControl_SizeChanged);
            this.ScrollPanel.MouseEnter += new System.EventHandler(this.ScrollPanel_MouseEnter);
            // 
            // MpScrollAblePanelControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F,13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.ScrollAt);
            this.Controls.Add(this.ScrollContainter);
            this.Controls.Add(this.ScrollPanel);
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Name = "MpScrollAblePanelControl";
            this.Size = new System.Drawing.Size(285,229);
            this.SizeChanged += new System.EventHandler(this.MpScrollAblePanelControl_SizeChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ScrollContainter;
        private System.Windows.Forms.Panel ScrollAt;
        private System.Windows.Forms.Panel ScrollPanel;
    }
}

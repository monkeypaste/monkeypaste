namespace MpLogForm {
    partial class MpKeywordItem {
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
            this.KeywordLabel = new System.Windows.Forms.Label();
            this.KeywordCloseButton = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.KeywordCloseButton)).BeginInit();
            this.SuspendLayout();
            // 
            // KeywordLabel
            // 
            this.KeywordLabel.AutoSize = true;
            this.KeywordLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.KeywordLabel.Font = new System.Drawing.Font("Gill Sans MT Condensed", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.KeywordLabel.Location = new System.Drawing.Point(0, 0);
            this.KeywordLabel.Name = "KeywordLabel";
            this.KeywordLabel.Size = new System.Drawing.Size(59, 25);
            this.KeywordLabel.TabIndex = 0;
            this.KeywordLabel.Text = "Keyword";
            // 
            // KeywordCloseButton
            // 
            this.KeywordCloseButton.BackgroundImage = global::MpLogForm.Resources.close;
            this.KeywordCloseButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.KeywordCloseButton.Location = new System.Drawing.Point(57, 4);
            this.KeywordCloseButton.Name = "KeywordCloseButton";
            this.KeywordCloseButton.Size = new System.Drawing.Size(18, 18);
            this.KeywordCloseButton.TabIndex = 2;
            this.KeywordCloseButton.TabStop = false;
            // 
            // MpKeywordItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.KeywordCloseButton);
            this.Controls.Add(this.KeywordLabel);
            this.Name = "MpKeywordItem";
            this.Size = new System.Drawing.Size(80, 28);
            ((System.ComponentModel.ISupportInitialize)(this.KeywordCloseButton)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label KeywordLabel;
        private System.Windows.Forms.PictureBox KeywordCloseButton;
    }
}

namespace MpWpfApp.View {
    partial class MpInputActionForm {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.Key1ComboBox = new System.Windows.Forms.ComboBox();
            this.Key2ComboBox = new System.Windows.Forms.ComboBox();
            this.Key3ComboBox = new System.Windows.Forms.ComboBox();
            this.Key4ComboBox = new System.Windows.Forms.ComboBox();
            this.LoadOnLoginCheckBox = new System.Windows.Forms.CheckBox();
            this.resetDbButton = new System.Windows.Forms.Button();
            this.deleteDbButton = new System.Windows.Forms.Button();
            this.MaxStoredClipBoardEntries = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStoredClipBoardEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // Key1ComboBox
            // 
            this.Key1ComboBox.FormattingEnabled = true;
            this.Key1ComboBox.Location = new System.Drawing.Point(126, 92);
            this.Key1ComboBox.Name = "Key1ComboBox";
            this.Key1ComboBox.Size = new System.Drawing.Size(121, 33);
            this.Key1ComboBox.TabIndex = 0;
            // 
            // Key2ComboBox
            // 
            this.Key2ComboBox.FormattingEnabled = true;
            this.Key2ComboBox.Location = new System.Drawing.Point(126, 162);
            this.Key2ComboBox.Name = "Key2ComboBox";
            this.Key2ComboBox.Size = new System.Drawing.Size(121, 33);
            this.Key2ComboBox.TabIndex = 1;
            // 
            // Key3ComboBox
            // 
            this.Key3ComboBox.FormattingEnabled = true;
            this.Key3ComboBox.Location = new System.Drawing.Point(126, 213);
            this.Key3ComboBox.Name = "Key3ComboBox";
            this.Key3ComboBox.Size = new System.Drawing.Size(121, 33);
            this.Key3ComboBox.TabIndex = 2;
            // 
            // Key4ComboBox
            // 
            this.Key4ComboBox.FormattingEnabled = true;
            this.Key4ComboBox.Location = new System.Drawing.Point(126, 272);
            this.Key4ComboBox.Name = "Key4ComboBox";
            this.Key4ComboBox.Size = new System.Drawing.Size(121, 33);
            this.Key4ComboBox.TabIndex = 3;
            // 
            // LoadOnLoginCheckBox
            // 
            this.LoadOnLoginCheckBox.AutoSize = true;
            this.LoadOnLoginCheckBox.Location = new System.Drawing.Point(126, 40);
            this.LoadOnLoginCheckBox.Name = "LoadOnLoginCheckBox";
            this.LoadOnLoginCheckBox.Size = new System.Drawing.Size(185, 29);
            this.LoadOnLoginCheckBox.TabIndex = 4;
            this.LoadOnLoginCheckBox.Text = "Load On Login";
            this.LoadOnLoginCheckBox.UseVisualStyleBackColor = true;
            // 
            // resetDbButton
            // 
            this.resetDbButton.Location = new System.Drawing.Point(878, 92);
            this.resetDbButton.Name = "resetDbButton";
            this.resetDbButton.Size = new System.Drawing.Size(171, 47);
            this.resetDbButton.TabIndex = 5;
            this.resetDbButton.Text = "Reset DB";
            this.resetDbButton.UseVisualStyleBackColor = true;
            // 
            // deleteDbButton
            // 
            this.deleteDbButton.Location = new System.Drawing.Point(878, 199);
            this.deleteDbButton.Name = "deleteDbButton";
            this.deleteDbButton.Size = new System.Drawing.Size(171, 47);
            this.deleteDbButton.TabIndex = 6;
            this.deleteDbButton.Text = "Delete DB";
            this.deleteDbButton.UseVisualStyleBackColor = true;
            // 
            // MaxStoredClipBoardEntries
            // 
            this.MaxStoredClipBoardEntries.Location = new System.Drawing.Point(828, 510);
            this.MaxStoredClipBoardEntries.Name = "MaxStoredClipBoardEntries";
            this.MaxStoredClipBoardEntries.Size = new System.Drawing.Size(120, 31);
            this.MaxStoredClipBoardEntries.TabIndex = 8;
            // 
            // MpInputActionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1220, 865);
            this.Controls.Add(this.MaxStoredClipBoardEntries);
            this.Controls.Add(this.deleteDbButton);
            this.Controls.Add(this.resetDbButton);
            this.Controls.Add(this.LoadOnLoginCheckBox);
            this.Controls.Add(this.Key4ComboBox);
            this.Controls.Add(this.Key3ComboBox);
            this.Controls.Add(this.Key2ComboBox);
            this.Controls.Add(this.Key1ComboBox);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "MpInputActionForm";
            this.Text = "-";
            this.Load += new System.EventHandler(this.MpInputActionForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MaxStoredClipBoardEntries)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Key1ComboBox;
        private System.Windows.Forms.ComboBox Key2ComboBox;
        private System.Windows.Forms.ComboBox Key3ComboBox;
        private System.Windows.Forms.ComboBox Key4ComboBox;
        private System.Windows.Forms.CheckBox LoadOnLoginCheckBox;
        private System.Windows.Forms.Button resetDbButton;
        private System.Windows.Forms.Button deleteDbButton;
        private System.Windows.Forms.NumericUpDown MaxStoredClipBoardEntries;
    }
}
namespace MonkeyPaste.View {
    partial class MpResetPasswordForm {
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
            this.label1 = new System.Windows.Forms.Label();
            this.resetEmailTextBox = new System.Windows.Forms.TextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.cancelResetButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 96);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(247, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "Reset Email";
            // 
            // resetEmailTextBox
            // 
            this.resetEmailTextBox.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetEmailTextBox.Location = new System.Drawing.Point(282, 77);
            this.resetEmailTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.resetEmailTextBox.Name = "resetEmailTextBox";
            this.resetEmailTextBox.Size = new System.Drawing.Size(560, 70);
            this.resetEmailTextBox.TabIndex = 1;
            // 
            // resetButton
            // 
            this.resetButton.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resetButton.Location = new System.Drawing.Point(628, 202);
            this.resetButton.Margin = new System.Windows.Forms.Padding(6);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(218, 71);
            this.resetButton.TabIndex = 3;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // cancelResetButton
            // 
            this.cancelResetButton.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelResetButton.Location = new System.Drawing.Point(26, 202);
            this.cancelResetButton.Margin = new System.Windows.Forms.Padding(6);
            this.cancelResetButton.Name = "cancelResetButton";
            this.cancelResetButton.Size = new System.Drawing.Size(218, 71);
            this.cancelResetButton.TabIndex = 2;
            this.cancelResetButton.Text = "Cancel";
            this.cancelResetButton.UseVisualStyleBackColor = true;
            // 
            // MpResetPasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(870, 296);
            this.Controls.Add(this.cancelResetButton);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.resetEmailTextBox);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MpResetPasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MonkeyPaste Reset Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox resetEmailTextBox;
        public System.Windows.Forms.Button resetButton;
        public System.Windows.Forms.Button cancelResetButton;
    }
}
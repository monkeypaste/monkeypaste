namespace MonkeyPaste.View {
    partial class MpSetDbPasswordForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpSetDbPasswordForm));
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.ConfirmTextBox = new System.Windows.Forms.TextBox();
            this.RememberPasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.SetPasswordButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.InvalidateLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasswordTextBox.Location = new System.Drawing.Point(78, 101);
            this.PasswordTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(584, 66);
            this.PasswordTextBox.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Calibri", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(73, 37);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(431, 63);
            this.textBox2.TabIndex = 2;
            this.textBox2.TabStop = false;
            this.textBox2.Text = "Enter New Password";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Font = new System.Drawing.Font("Calibri", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox3.Location = new System.Drawing.Point(78, 175);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(431, 63);
            this.textBox3.TabIndex = 4;
            this.textBox3.TabStop = false;
            this.textBox3.Text = "Confirm Password";
            // 
            // ConfirmTextBox
            // 
            this.ConfirmTextBox.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConfirmTextBox.Location = new System.Drawing.Point(78, 238);
            this.ConfirmTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.ConfirmTextBox.Name = "ConfirmTextBox";
            this.ConfirmTextBox.PasswordChar = '*';
            this.ConfirmTextBox.Size = new System.Drawing.Size(584, 66);
            this.ConfirmTextBox.TabIndex = 1;
            // 
            // RememberPasswordCheckbox
            // 
            this.RememberPasswordCheckbox.AutoSize = true;
            this.RememberPasswordCheckbox.Font = new System.Drawing.Font("Calibri", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RememberPasswordCheckbox.Location = new System.Drawing.Point(78, 378);
            this.RememberPasswordCheckbox.Name = "RememberPasswordCheckbox";
            this.RememberPasswordCheckbox.Size = new System.Drawing.Size(246, 57);
            this.RememberPasswordCheckbox.TabIndex = 2;
            this.RememberPasswordCheckbox.Text = "Remember";
            this.RememberPasswordCheckbox.UseVisualStyleBackColor = true;
            // 
            // SetPasswordButton
            // 
            this.SetPasswordButton.Font = new System.Drawing.Font("Calibri", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetPasswordButton.Location = new System.Drawing.Point(423, 496);
            this.SetPasswordButton.Name = "SetPasswordButton";
            this.SetPasswordButton.Size = new System.Drawing.Size(239, 63);
            this.SetPasswordButton.TabIndex = 4;
            this.SetPasswordButton.Text = "Set";
            this.SetPasswordButton.UseVisualStyleBackColor = true;
            this.SetPasswordButton.Click += new System.EventHandler(this.SetPasswordButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Font = new System.Drawing.Font("Calibri", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelButton.Location = new System.Drawing.Point(78, 496);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(239, 63);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // InvalidateLabel
            // 
            this.InvalidateLabel.AutoSize = true;
            this.InvalidateLabel.Font = new System.Drawing.Font("Calibri", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.InvalidateLabel.ForeColor = System.Drawing.Color.Red;
            this.InvalidateLabel.Location = new System.Drawing.Point(278, 308);
            this.InvalidateLabel.Name = "InvalidateLabel";
            this.InvalidateLabel.Size = new System.Drawing.Size(384, 45);
            this.InvalidateLabel.TabIndex = 8;
            this.InvalidateLabel.Text = "Passwords do not match";
            // 
            // MpSetDbPasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 604);
            this.Controls.Add(this.InvalidateLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SetPasswordButton);
            this.Controls.Add(this.RememberPasswordCheckbox);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.ConfirmTextBox);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.PasswordTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "MpSetDbPasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Set Data Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox ConfirmTextBox;
        private System.Windows.Forms.CheckBox RememberPasswordCheckbox;
        private System.Windows.Forms.Button SetPasswordButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label InvalidateLabel;
    }
}
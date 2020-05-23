namespace MonkeyPaste {
    partial class MpEnterDbPasswordForm {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpEnterDbPasswordForm));
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.RememberPasswordCheckbox = new System.Windows.Forms.CheckBox();
            this.SetPasswordButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Font = new System.Drawing.Font("Calibri",18F,System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,((byte)(0)));
            this.PasswordTextBox.Location = new System.Drawing.Point(78,101);
            this.PasswordTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(584,66);
            this.PasswordTextBox.TabIndex = 0;
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Calibri",16.125F,System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(73,32);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(480,60);
            this.textBox2.TabIndex = 5;
            this.textBox2.Text = "Enter Database Password";
            // 
            // RememberPasswordCheckbox
            // 
            this.RememberPasswordCheckbox.AutoSize = true;
            this.RememberPasswordCheckbox.Font = new System.Drawing.Font("Calibri",16.125F,System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,((byte)(0)));
            this.RememberPasswordCheckbox.Location = new System.Drawing.Point(73,197);
            this.RememberPasswordCheckbox.Name = "RememberPasswordCheckbox";
            this.RememberPasswordCheckbox.Size = new System.Drawing.Size(246,57);
            this.RememberPasswordCheckbox.TabIndex = 2;
            this.RememberPasswordCheckbox.Text = "Remember";
            this.RememberPasswordCheckbox.UseVisualStyleBackColor = true;
            // 
            // SetPasswordButton
            // 
            this.SetPasswordButton.Font = new System.Drawing.Font("Calibri",16.125F,System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,((byte)(0)));
            this.SetPasswordButton.Location = new System.Drawing.Point(423,287);
            this.SetPasswordButton.Name = "SetPasswordButton";
            this.SetPasswordButton.Size = new System.Drawing.Size(239,63);
            this.SetPasswordButton.TabIndex = 4;
            this.SetPasswordButton.Text = "Set";
            this.SetPasswordButton.UseVisualStyleBackColor = true;
            this.SetPasswordButton.Click += new System.EventHandler(this.SetPasswordButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Font = new System.Drawing.Font("Calibri",16.125F,System.Drawing.FontStyle.Regular,System.Drawing.GraphicsUnit.Point,((byte)(0)));
            this.CancelButton.Location = new System.Drawing.Point(78,287);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(239,63);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // MpEnterDbPasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F,25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738,406);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SetPasswordButton);
            this.Controls.Add(this.RememberPasswordCheckbox);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.PasswordTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MpEnterDbPasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Database Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox RememberPasswordCheckbox;
        private System.Windows.Forms.Button SetPasswordButton;
        private new System.Windows.Forms.Button CancelButton;
    }
}
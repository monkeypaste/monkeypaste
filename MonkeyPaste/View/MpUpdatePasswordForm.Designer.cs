namespace MonkeyPaste.View {
    partial class MpUpdatePasswordForm {
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
            this.cancelPasswordUpdateButton = new System.Windows.Forms.Button();
            this.updatePasswordButton = new System.Windows.Forms.Button();
            this.newPassword1TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.newPassword2TextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancelPasswordUpdateButton
            // 
            this.cancelPasswordUpdateButton.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelPasswordUpdateButton.Location = new System.Drawing.Point(46, 338);
            this.cancelPasswordUpdateButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cancelPasswordUpdateButton.Name = "cancelPasswordUpdateButton";
            this.cancelPasswordUpdateButton.Size = new System.Drawing.Size(218, 71);
            this.cancelPasswordUpdateButton.TabIndex = 7;
            this.cancelPasswordUpdateButton.Text = "Cancel";
            this.cancelPasswordUpdateButton.UseVisualStyleBackColor = true;
            // 
            // updatePasswordButton
            // 
            this.updatePasswordButton.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updatePasswordButton.Location = new System.Drawing.Point(648, 338);
            this.updatePasswordButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.updatePasswordButton.Name = "updatePasswordButton";
            this.updatePasswordButton.Size = new System.Drawing.Size(218, 71);
            this.updatePasswordButton.TabIndex = 6;
            this.updatePasswordButton.Text = "Update";
            this.updatePasswordButton.UseVisualStyleBackColor = true;
            // 
            // newPassword1TextBox
            // 
            this.newPassword1TextBox.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newPassword1TextBox.Location = new System.Drawing.Point(436, 46);
            this.newPassword1TextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.newPassword1TextBox.Name = "newPassword1TextBox";
            this.newPassword1TextBox.PasswordChar = '*';
            this.newPassword1TextBox.Size = new System.Drawing.Size(420, 70);
            this.newPassword1TextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(38, 65);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(294, 47);
            this.label1.TabIndex = 4;
            this.label1.Text = "New Password";
            // 
            // newPassword2TextBox
            // 
            this.newPassword2TextBox.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newPassword2TextBox.Location = new System.Drawing.Point(436, 179);
            this.newPassword2TextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.newPassword2TextBox.Name = "newPassword2TextBox";
            this.newPassword2TextBox.PasswordChar = '*';
            this.newPassword2TextBox.Size = new System.Drawing.Size(420, 70);
            this.newPassword2TextBox.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(38, 198);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(379, 47);
            this.label2.TabIndex = 8;
            this.label2.Text = "Re-Enter Password";
            // 
            // MpUpdatePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(898, 433);
            this.Controls.Add(this.newPassword2TextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancelPasswordUpdateButton);
            this.Controls.Add(this.updatePasswordButton);
            this.Controls.Add(this.newPassword1TextBox);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "MpUpdatePasswordForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MonkeyPaste Update Password";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button cancelPasswordUpdateButton;
        public System.Windows.Forms.Button updatePasswordButton;
        public System.Windows.Forms.TextBox newPassword1TextBox;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox newPassword2TextBox;
        public System.Windows.Forms.Label label2;
    }
}
using System.Windows.Forms;

namespace MpWpfApp {
    partial class MpLoginForm:Form {
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
            this.emailTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.registerLinkLabel = new System.Windows.Forms.LinkLabel();
            this.resetLinkLabel = new System.Windows.Forms.LinkLabel();
            this.loginButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.remeberCredentialsCheckBox = new System.Windows.Forms.CheckBox();
            this.autoLoginCheckBox = new System.Windows.Forms.CheckBox();
            this.connectionNameComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // emailTextBox
            // 
            this.emailTextBox.Font = new System.Drawing.Font("Arial", 19.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.emailTextBox.Location = new System.Drawing.Point(44, 65);
            this.emailTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.emailTextBox.Name = "emailTextBox";
            this.emailTextBox.Size = new System.Drawing.Size(710, 68);
            this.emailTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(38, 13);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(300, 49);
            this.label1.TabIndex = 1;
            this.label1.Text = "Email Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(42, 146);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 49);
            this.label2.TabIndex = 3;
            this.label2.Text = "Password";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Font = new System.Drawing.Font("Arial", 19.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passwordTextBox.Location = new System.Drawing.Point(48, 198);
            this.passwordTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(710, 68);
            this.passwordTextBox.TabIndex = 1;
            // 
            // registerLinkLabel
            // 
            this.registerLinkLabel.AutoSize = true;
            this.registerLinkLabel.Location = new System.Drawing.Point(661, 500);
            this.registerLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.registerLinkLabel.Name = "registerLinkLabel";
            this.registerLinkLabel.Size = new System.Drawing.Size(92, 25);
            this.registerLinkLabel.TabIndex = 3;
            this.registerLinkLabel.TabStop = true;
            this.registerLinkLabel.Text = "Register";
            // 
            // resetLinkLabel
            // 
            this.resetLinkLabel.AutoSize = true;
            this.resetLinkLabel.Location = new System.Drawing.Point(565, 540);
            this.resetLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.resetLinkLabel.Name = "resetLinkLabel";
            this.resetLinkLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.resetLinkLabel.Size = new System.Drawing.Size(186, 25);
            this.resetLinkLabel.TabIndex = 4;
            this.resetLinkLabel.TabStop = true;
            this.resetLinkLabel.Text = "Forgot Password?";
            // 
            // loginButton
            // 
            this.loginButton.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loginButton.Location = new System.Drawing.Point(598, 673);
            this.loginButton.Margin = new System.Windows.Forms.Padding(4);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(160, 66);
            this.loginButton.TabIndex = 6;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.loginButton_ClickAsync);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(44, 673);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(176, 66);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // remeberCredentialsCheckBox
            // 
            this.remeberCredentialsCheckBox.AutoSize = true;
            this.remeberCredentialsCheckBox.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.remeberCredentialsCheckBox.Location = new System.Drawing.Point(44, 472);
            this.remeberCredentialsCheckBox.Name = "remeberCredentialsCheckBox";
            this.remeberCredentialsCheckBox.Size = new System.Drawing.Size(231, 53);
            this.remeberCredentialsCheckBox.TabIndex = 2;
            this.remeberCredentialsCheckBox.Text = "Remeber";
            this.remeberCredentialsCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoLoginCheckBox
            // 
            this.autoLoginCheckBox.AutoSize = true;
            this.autoLoginCheckBox.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.autoLoginCheckBox.Location = new System.Drawing.Point(44, 571);
            this.autoLoginCheckBox.Name = "autoLoginCheckBox";
            this.autoLoginCheckBox.Size = new System.Drawing.Size(263, 53);
            this.autoLoginCheckBox.TabIndex = 7;
            this.autoLoginCheckBox.Text = "Auto-Login";
            this.autoLoginCheckBox.UseVisualStyleBackColor = true;
            this.autoLoginCheckBox.CheckedChanged += new System.EventHandler(this.autoLoginCheckBox_CheckedChanged);
            // 
            // connectionNameComboBox
            // 
            this.connectionNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectionNameComboBox.Font = new System.Drawing.Font("Arial", 19.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.connectionNameComboBox.FormattingEnabled = true;
            this.connectionNameComboBox.Items.AddRange(new object[] {
            "Username-Password-Authentication",
            "google-oauth2",
            "twitter",
            "facebook",
            "github",
            "windowslive"});
            this.connectionNameComboBox.Location = new System.Drawing.Point(51, 344);
            this.connectionNameComboBox.Margin = new System.Windows.Forms.Padding(6);
            this.connectionNameComboBox.Name = "connectionNameComboBox";
            this.connectionNameComboBox.Size = new System.Drawing.Size(700, 68);
            this.connectionNameComboBox.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 16.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(42, 289);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(241, 49);
            this.label3.TabIndex = 8;
            this.label3.Text = "Connection";
            // 
            // MpLoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 774);
            this.Controls.Add(this.connectionNameComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.autoLoginCheckBox);
            this.Controls.Add(this.remeberCredentialsCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.resetLinkLabel);
            this.Controls.Add(this.registerLinkLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.emailTextBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MpLoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MonkeyPaste Login";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox emailTextBox;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox passwordTextBox;
        public System.Windows.Forms.LinkLabel registerLinkLabel;
        public System.Windows.Forms.LinkLabel resetLinkLabel;
        public System.Windows.Forms.Button loginButton;
        public System.Windows.Forms.Button cancelButton;
        public CheckBox remeberCredentialsCheckBox;
        private CheckBox autoLoginCheckBox;
        private ComboBox connectionNameComboBox;
        private Label label3;
    }
}
namespace ExtractLargeIconFromFile
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chooseFileButton = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.iconSizesComboBox = new System.Windows.Forms.ComboBox();
            this.labelFilePath = new System.Windows.Forms.Label();
            this.chooseFolderButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // chooseFileButton
            // 
            this.chooseFileButton.Location = new System.Drawing.Point(12, 12);
            this.chooseFileButton.Name = "chooseFileButton";
            this.chooseFileButton.Size = new System.Drawing.Size(108, 23);
            this.chooseFileButton.TabIndex = 0;
            this.chooseFileButton.Text = "Choose file...";
            this.chooseFileButton.UseVisualStyleBackColor = true;
            this.chooseFileButton.Click += new System.EventHandler(this.chooseFileButton_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(13, 60);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(490, 337);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // iconSizesComboBox
            // 
            this.iconSizesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.iconSizesComboBox.FormattingEnabled = true;
            this.iconSizesComboBox.Location = new System.Drawing.Point(240, 14);
            this.iconSizesComboBox.Name = "iconSizesComboBox";
            this.iconSizesComboBox.Size = new System.Drawing.Size(180, 21);
            this.iconSizesComboBox.TabIndex = 2;
            // 
            // labelFilePath
            // 
            this.labelFilePath.AutoSize = true;
            this.labelFilePath.Location = new System.Drawing.Point(13, 41);
            this.labelFilePath.Name = "labelFilePath";
            this.labelFilePath.Size = new System.Drawing.Size(14, 13);
            this.labelFilePath.TabIndex = 3;
            this.labelFilePath.Text = "#";
            // 
            // chooseFolderButton
            // 
            this.chooseFolderButton.Location = new System.Drawing.Point(126, 12);
            this.chooseFolderButton.Name = "chooseFolderButton";
            this.chooseFolderButton.Size = new System.Drawing.Size(108, 23);
            this.chooseFolderButton.TabIndex = 4;
            this.chooseFolderButton.Text = "Choose folder...";
            this.chooseFolderButton.UseVisualStyleBackColor = true;
            this.chooseFolderButton.Click += new System.EventHandler(this.chooseFolderButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(515, 409);
            this.Controls.Add(this.chooseFolderButton);
            this.Controls.Add(this.labelFilePath);
            this.Controls.Add(this.iconSizesComboBox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.chooseFileButton);
            this.Name = "Form1";
            this.Text = "Extract Icon From UNC File";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button chooseFileButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox iconSizesComboBox;
        private System.Windows.Forms.Label labelFilePath;
        private System.Windows.Forms.Button chooseFolderButton;
    }
}


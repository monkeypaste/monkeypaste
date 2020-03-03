using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace MonkeyPaste
{
    partial class MpSettingsForm : System.Windows.Forms.Form
    {

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {

            //ChangeClipboardChain(this.Handle, nextClipboardViewer);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public MpSettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.ImportButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DataDetailsLabel = new MonkeyPaste.MpDataDetailsLabel();
            this.OpenDbFolderButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.LoadOnStartUpCheckbox = new System.Windows.Forms.CheckBox();
            this.MoveDbButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImportButton
            // 
            this.ImportButton.Location = new System.Drawing.Point(22, 42);
            this.ImportButton.Name = "ImportButton";
            this.ImportButton.Size = new System.Drawing.Size(208, 64);
            this.ImportButton.TabIndex = 0;
            this.ImportButton.Text = "Import...";
            this.ImportButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.MoveDbButton);
            this.groupBox1.Controls.Add(this.DataDetailsLabel);
            this.groupBox1.Controls.Add(this.OpenDbFolderButton);
            this.groupBox1.Controls.Add(this.ResetButton);
            this.groupBox1.Controls.Add(this.ImportButton);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.groupBox1.Location = new System.Drawing.Point(12, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(486, 263);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // DataDetailsLabel
            // 
            this.DataDetailsLabel.AutoSize = true;
            this.DataDetailsLabel.Location = new System.Drawing.Point(155, 215);
            this.DataDetailsLabel.Name = "DataDetailsLabel";
            this.DataDetailsLabel.Size = new System.Drawing.Size(178, 26);
            this.DataDetailsLabel.TabIndex = 5;
            this.DataDetailsLabel.Text = "DataDetailsLabel";
            this.DataDetailsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DataDetailsLabel.ViewData = this.DataDetailsLabel;
            this.DataDetailsLabel.ViewId = 1593487619;
            this.DataDetailsLabel.ViewName = "MonkeyPaste.MpDataDetailsLabel";
            this.DataDetailsLabel.ViewType = "MonkeyPaste.MpDataDetailsLabel";
            // 
            // OpenDbFolderButton
            // 
            this.OpenDbFolderButton.Location = new System.Drawing.Point(22, 127);
            this.OpenDbFolderButton.Name = "OpenDbFolderButton";
            this.OpenDbFolderButton.Size = new System.Drawing.Size(208, 64);
            this.OpenDbFolderButton.TabIndex = 3;
            this.OpenDbFolderButton.Text = "Open Folder";
            this.OpenDbFolderButton.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(250, 127);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(208, 64);
            this.ResetButton.TabIndex = 2;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            // 
            // LoadOnStartUpCheckbox
            // 
            this.LoadOnStartUpCheckbox.AutoSize = true;
            this.LoadOnStartUpCheckbox.Location = new System.Drawing.Point(18, 322);
            this.LoadOnStartUpCheckbox.Name = "LoadOnStartUpCheckbox";
            this.LoadOnStartUpCheckbox.Size = new System.Drawing.Size(197, 29);
            this.LoadOnStartUpCheckbox.TabIndex = 3;
            this.LoadOnStartUpCheckbox.Text = "Load on Startup";
            this.LoadOnStartUpCheckbox.UseVisualStyleBackColor = true;
            // 
            // MoveDbButton
            // 
            this.MoveDbButton.Location = new System.Drawing.Point(250, 42);
            this.MoveDbButton.Name = "MoveDbButton";
            this.MoveDbButton.Size = new System.Drawing.Size(208, 64);
            this.MoveDbButton.TabIndex = 6;
            this.MoveDbButton.Text = "Move...";
            this.MoveDbButton.UseVisualStyleBackColor = true;
            // 
            // MpSettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(522, 371);
            this.Controls.Add(this.LoadOnStartUpCheckbox);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MpSettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MpSettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private CheckBox LoadOnLoginCheckBox;
        private CheckBox StoreClipBoardCheckBox;
        private NumericUpDown MaxStoredClipBoardEntries;
        private Label label1;
        private Label label2;
        private ComboBox Key1ComboBox;
        private Label label3;
        private ComboBox Key2ComboBox;
        private ComboBox Key3ComboBox;
        private Label label4;
        private ComboBox Key4ComboBox;
        private Label label5;
        private Button resetDbButton;
        private Button deleteDbButton;
        private GroupBox groupBox1;
        public Button ImportButton;
        public Button OpenDbFolderButton;
        public Button ResetButton;
        public CheckBox LoadOnStartUpCheckbox;
        public MpDataDetailsLabel DataDetailsLabel;
        public Button MoveDbButton;
    }
}


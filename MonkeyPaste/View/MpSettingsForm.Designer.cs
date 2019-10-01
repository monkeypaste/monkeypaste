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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MpSettingsForm));
            this.LoadOnLoginCheckBox = new System.Windows.Forms.CheckBox();
            this.StoreClipBoardCheckBox = new System.Windows.Forms.CheckBox();
            this.MaxStoredClipBoardEntries = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Key1ComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Key2ComboBox = new System.Windows.Forms.ComboBox();
            this.Key3ComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Key4ComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.resetDbButton = new System.Windows.Forms.Button();
            this.deleteDbButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MaxStoredClipBoardEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadOnLoginCheckBox
            // 
            this.LoadOnLoginCheckBox.AutoSize = true;
            this.LoadOnLoginCheckBox.Location = new System.Drawing.Point(24, 22);
            this.LoadOnLoginCheckBox.Name = "LoadOnLoginCheckBox";
            this.LoadOnLoginCheckBox.Size = new System.Drawing.Size(185, 29);
            this.LoadOnLoginCheckBox.TabIndex = 1;
            this.LoadOnLoginCheckBox.Text = "Load On Login";
            this.LoadOnLoginCheckBox.UseVisualStyleBackColor = true;
            this.LoadOnLoginCheckBox.CheckedChanged += new System.EventHandler(this.loadOnLoginCheckBox_CheckedChanged);
            // 
            // StoreClipBoardCheckBox
            // 
            this.StoreClipBoardCheckBox.AutoSize = true;
            this.StoreClipBoardCheckBox.Location = new System.Drawing.Point(24, 66);
            this.StoreClipBoardCheckBox.Name = "StoreClipBoardCheckBox";
            this.StoreClipBoardCheckBox.Size = new System.Drawing.Size(212, 29);
            this.StoreClipBoardCheckBox.TabIndex = 2;
            this.StoreClipBoardCheckBox.Text = "Save to database";
            this.StoreClipBoardCheckBox.UseVisualStyleBackColor = true;
            this.StoreClipBoardCheckBox.CheckedChanged += new System.EventHandler(this.storeClipBoardCheckBox_CheckedChanged);
            // 
            // MaxStoredClipBoardEntries
            // 
            this.MaxStoredClipBoardEntries.Location = new System.Drawing.Point(24, 109);
            this.MaxStoredClipBoardEntries.Name = "MaxStoredClipBoardEntries";
            this.MaxStoredClipBoardEntries.Size = new System.Drawing.Size(118, 31);
            this.MaxStoredClipBoardEntries.TabIndex = 3;
            this.MaxStoredClipBoardEntries.ValueChanged += new System.EventHandler(this.maxStoredClipBoardEntries_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(154, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Max Stored Entries";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 159);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Hot Key";
            // 
            // Key1ComboBox
            // 
            this.Key1ComboBox.FormattingEnabled = true;
            this.Key1ComboBox.Location = new System.Drawing.Point(30, 190);
            this.Key1ComboBox.Name = "Key1ComboBox";
            this.Key1ComboBox.Size = new System.Drawing.Size(262, 33);
            this.Key1ComboBox.TabIndex = 7;
            this.Key1ComboBox.SelectedIndexChanged += new System.EventHandler(this.Key1ComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(154, 231);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "+";
            // 
            // Key2ComboBox
            // 
            this.Key2ComboBox.FormattingEnabled = true;
            this.Key2ComboBox.Location = new System.Drawing.Point(30, 260);
            this.Key2ComboBox.Name = "Key2ComboBox";
            this.Key2ComboBox.Size = new System.Drawing.Size(262, 33);
            this.Key2ComboBox.TabIndex = 9;
            this.Key2ComboBox.SelectedIndexChanged += new System.EventHandler(this.Key2ComboBox_SelectedIndexChanged);
            // 
            // Key3ComboBox
            // 
            this.Key3ComboBox.FormattingEnabled = true;
            this.Key3ComboBox.Location = new System.Drawing.Point(30, 332);
            this.Key3ComboBox.Name = "Key3ComboBox";
            this.Key3ComboBox.Size = new System.Drawing.Size(262, 33);
            this.Key3ComboBox.TabIndex = 11;
            this.Key3ComboBox.SelectedIndexChanged += new System.EventHandler(this.Key3ComboBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(154, 305);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 25);
            this.label4.TabIndex = 10;
            this.label4.Text = "+";
            // 
            // Key4ComboBox
            // 
            this.Key4ComboBox.FormattingEnabled = true;
            this.Key4ComboBox.Location = new System.Drawing.Point(30, 404);
            this.Key4ComboBox.Name = "Key4ComboBox";
            this.Key4ComboBox.Size = new System.Drawing.Size(262, 33);
            this.Key4ComboBox.TabIndex = 13;
            this.Key4ComboBox.SelectedIndexChanged += new System.EventHandler(this.Key4ComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(154, 377);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 25);
            this.label5.TabIndex = 12;
            this.label5.Text = "+";
            // 
            // resetDbButton
            // 
            this.resetDbButton.Location = new System.Drawing.Point(423, 49);
            this.resetDbButton.Name = "resetDbButton";
            this.resetDbButton.Size = new System.Drawing.Size(233, 61);
            this.resetDbButton.TabIndex = 14;
            this.resetDbButton.Text = "Reset Database";
            this.resetDbButton.UseVisualStyleBackColor = true;
            // 
            // deleteDbButton
            // 
            this.deleteDbButton.Location = new System.Drawing.Point(423, 141);
            this.deleteDbButton.Name = "deleteDbButton";
            this.deleteDbButton.Size = new System.Drawing.Size(233, 61);
            this.deleteDbButton.TabIndex = 15;
            this.deleteDbButton.Text = "Delete Database";
            this.deleteDbButton.UseVisualStyleBackColor = true;
            // 
            // MpSettingsForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(10, 24);
            this.ClientSize = new System.Drawing.Size(694, 479);
            this.Controls.Add(this.deleteDbButton);
            this.Controls.Add(this.resetDbButton);
            this.Controls.Add(this.Key4ComboBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Key3ComboBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Key2ComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Key1ComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.MaxStoredClipBoardEntries);
            this.Controls.Add(this.StoreClipBoardCheckBox);
            this.Controls.Add(this.LoadOnLoginCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MpSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Deactivate += new System.EventHandler(this.SettingsForm_Deactivate);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.MaxStoredClipBoardEntries)).EndInit();
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
    }
}


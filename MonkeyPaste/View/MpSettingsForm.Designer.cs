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
            this.SuspendLayout();
            // 
            // MpSettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(612, 227);
            this.Name = "MpSettingsForm";
            this.Load += new System.EventHandler(this.MpSettingsForm_Load);
            this.ResumeLayout(false);

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


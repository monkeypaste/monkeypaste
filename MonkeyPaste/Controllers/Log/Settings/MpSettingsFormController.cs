using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSettingsFormController : MpController {
        public MpDataDetailsLabelController DataDetailLabelController { get; set; }
        public MpSettingsForm SettingsForm { get; set; }

        public MpSettingsFormController(MpController parent) : base(parent) {
            SettingsForm = new MpSettingsForm();
            SettingsForm.FormClosed += SettingsForm_FormClosed;
            SettingsForm.ImportButton.Click += ImportButton_Click;
            SettingsForm.ExportButton.Click += ExportButton_Click;
            SettingsForm.OpenDbFolderButton.Click += OpenDbFolderButton_Click;
            SettingsForm.ResetButton.Click += ResetButton_Click;
            SettingsForm.LoadOnStartUpCheckbox.Click += LoadOnStartUpCheckbox_Click;

            DataDetailLabelController = new MpDataDetailsLabelController(this);
            DataDetailLabelController.DataDetailsLabel = (MpDataDetailsLabel)SettingsForm.DataDetailsLabel;
            SettingsForm.DataDetailsLabel.MouseEnter += DataDetailLabelController.DataDetailsLabel_MouseEnter;
        }

        private void LoadOnStartUpCheckbox_Click(object sender,EventArgs e) {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",true);

            if(SettingsForm.LoadOnStartUpCheckbox.Checked) {
                rk.SetValue(Properties.Settings.Default.AppName,Application.ExecutablePath);
            }
            else {
                rk.DeleteValue(Properties.Settings.Default.AppName,false);
            }
        }

        private void ResetButton_Click(object sender,EventArgs e) {
            throw new NotImplementedException();
        }

        private void OpenDbFolderButton_Click(object sender,EventArgs e) {
            throw new NotImplementedException();
        }

        private void ExportButton_Click(object sender,EventArgs e) {
            throw new NotImplementedException();
        }

        private void ImportButton_Click(object sender,EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                FileName = "Select a .db file to import",
                Filter = "Db files (*.db)|*.db",
                Title = "Open DB File"
            };
            DialogResult openResult = openFileDialog.ShowDialog();
            if(openResult == DialogResult.OK) {
                string importDbPath = openFileDialog.FileName;
                try {
                    List<MpCopyItem> newCopyItems = MpLogFormController.Db.GetCopyItems(importDbPath);
                    var tileChooserController = (MpTileChooserPanelController)Find("MpTileChooserPanelController");
                    foreach(MpCopyItem nci in newCopyItems) {
                        //clear all pk's since merging to a new database
                        nci.CopyItemId = 0;
                        nci.AppId = 0;
                        tileChooserController.AddNewCopyItemPanel(nci);
                    }
                    tileChooserController.Sort("CopyDateTime",false);
                } catch(Exception ex) {
                    Console.WriteLine("Error importing " + importDbPath + " with error: " + ex.ToString());
                    //MessageBox.Show("Error importing "+importDbPath+" with error: "+ex.ToString(),"Import Error",MessageBoxButtons.OK,MessageBoxIcon.Error,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);                    
                }                
            }
        }

        private void SettingsForm_FormClosed(object sender,System.Windows.Forms.FormClosedEventArgs e) {
            ((MpLogFormController)Find("MpLogFormController")).ShowLogForm();
        }

        public override void Update() {
            //not dynamic so ignore resizing
        }
    }
}

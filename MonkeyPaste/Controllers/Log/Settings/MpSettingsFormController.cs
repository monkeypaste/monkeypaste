using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            SettingsForm.Shown += SettingsForm_Shown;
            SettingsForm.FormClosed += SettingsForm_FormClosed;
            SettingsForm.ImportButton.Click += ImportButton_Click;
            SettingsForm.MoveDbButton.Click += MoveDbButton_Click;
            SettingsForm.OpenDbFolderButton.Click += OpenDbFolderButton_Click;
            SettingsForm.ResetButton.Click += ResetButton_Click;
            SettingsForm.LoadOnStartUpCheckbox.Click += LoadOnStartUpCheckbox_Click;

            DataDetailLabelController = new MpDataDetailsLabelController(this);
            DataDetailLabelController.DataDetailsLabel = (MpDataDetailsLabel)SettingsForm.DataDetailsLabel;
            SettingsForm.DataDetailsLabel.MouseEnter += DataDetailLabelController.DataDetailsLabel_MouseEnter;
        }

        private void SettingsForm_Shown(object sender,EventArgs e) {

            DataDetailLabelController.Update();
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
            DialogResult confirmResetResult = MessageBox.Show("Are you sure you want to reset ALL your data?","Reset?",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2,MessageBoxOptions.DefaultDesktopOnly);

            if(confirmResetResult == DialogResult.Yes) {
                MpLogFormController.Db.ResetDb();
            }
        }

        private void OpenDbFolderButton_Click(object sender,EventArgs e) {
            if(!System.IO.File.Exists(MpLogFormController.Db.DbPath)) {
                throw new Exception("OpenDbFolder exception: Db file or folder does not exist " + MpLogFormController.Db.DbPath);
            }
            System.Diagnostics.Process.Start("explorer.exe",string.Format("/select,\"{0}\"",MpLogFormController.Db.DbPath));
        }

        private void MoveDbButton_Click(object sender,EventArgs e) {
            FolderBrowserDialog setNewDirectoryDialog = new FolderBrowserDialog() {
                Description = "Select new location for data file",
                ShowNewFolderButton = true,
                SelectedPath = Path.GetDirectoryName(MpLogFormController.Db.DbPath),
                //RootFolder = Environment.SpecialFolder.Personal
            };
            DialogResult newDataDirectoryResult = setNewDirectoryDialog.ShowDialog();
            if(newDataDirectoryResult == DialogResult.OK) {
                string newDbPath = setNewDirectoryDialog.SelectedPath + Path.GetFileName(MpLogFormController.Db.DbPath);
                try {
                    MpLogFormController.Db.CloseDb();
                    File.Move(MpLogFormController.Db.DbPath,newDbPath);
                } 
                catch(Exception ex) {
                    Console.WriteLine("MoveDb exception from path "+ MpLogFormController.Db.DbPath+" to new path "+ newDbPath+" : " + ex.ToString());
                }

                MpLogFormController.Db.DbPath = newDbPath;

                MpRegistryHelper.Instance.SetValue("DBPath",MpLogFormController.Db.DbPath);
            }
        }

        private void ImportButton_Click(object sender,EventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog() {
                FileName = "mp.db",
                Filter = "Db files (*.db)|*.db",
                Title = "Open DB File"
            };
            DialogResult openResult = openFileDialog.ShowDialog();
            if(openResult == DialogResult.OK) {
                string importDbPath = openFileDialog.FileName;
                List<MpCopyItem> newCopyItems = new List<MpCopyItem>();
                try {
                    //import copyitems
                    newCopyItems = MpLogFormController.Db.GetCopyItems(importDbPath);
                    var tileChooserController = (MpTileChooserPanelController)Find("MpTileChooserPanelController");
                    foreach(MpCopyItem nci in newCopyItems) {
                        //clear all pk's since merging to a new database
                        nci.CopyItemId = 0;
                        nci.AppId = 0;
                        tileChooserController.AddNewCopyItemPanel(nci);
                    }
                    tileChooserController.Sort("CopyDateTime",false);

                    //import tags

                } catch(Exception ex) {
                    Console.WriteLine("Error importing " + importDbPath + " with error: " + ex.ToString());
                    //MessageBox.Show("Error importing "+importDbPath+" with error: "+ex.ToString(),"Import Error",MessageBoxButtons.OK,MessageBoxIcon.Error,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);                    
                }      
                // TODO account for missing duplicates in import count
                MessageBox.Show("Successfully imported "+newCopyItems.Count+" items");
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

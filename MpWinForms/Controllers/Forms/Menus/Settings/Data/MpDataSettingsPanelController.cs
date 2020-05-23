using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpDataSettingsPanelController : MpController {
        public MpDataDetailsLabelController DataDetailLabelController { get; set; }

        public Panel DataSettingsPanel { get; set; }

        public Button ImportButton { get; set; }
        public Button MoveDbButton { get; set; }
        public Button OpenDbFolderButton { get; set; }
        public Button ResetButton { get; set; }

        public MpDataSettingsPanelController(MpController p) : base(p) {
            DataSettingsPanel = new Panel()
            {
                AutoSize = false
            };

            ImportButton = new Button()
            {
                Text = "Import"
            };
            DataSettingsPanel.Controls.Add(ImportButton);
            ImportButton.Click += ImportButton_Click;

            MoveDbButton = new Button()
            {
                Text = "Move Db"
            };
            DataSettingsPanel.Controls.Add(MoveDbButton);
            MoveDbButton.Click += MoveDbButton_Click;

            OpenDbFolderButton = new Button()
            {
                Text = "Open Db Folder"
            };
            DataSettingsPanel.Controls.Add(OpenDbFolderButton);
            OpenDbFolderButton.Click += OpenDbFolderButton_Click;

            ResetButton = new Button()
            {
                Text = "Reset Db"
            };
            DataSettingsPanel.Controls.Add(ResetButton);
            ResetButton.Click += ResetButton_Click;

            DataDetailLabelController = new MpDataDetailsLabelController(this);
            DataSettingsPanel.Controls.Add(DataDetailLabelController.DataDetailsLabel);

            //Link(new List<MpIView>() { DataSettingsPanel });
            Update();
        }

           public override void Update() {
            DataDetailLabelController.Update();
        }

        private void ResetButton_Click(object sender,EventArgs e) {
            DialogResult confirmResetResult = MessageBox.Show("Are you sure you want to reset ALL your data?","Reset?",MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2,MessageBoxOptions.DefaultDesktopOnly);

            if(confirmResetResult == DialogResult.Yes) {
                MpAppManager.Instance.DataModel.Db.ResetDb();
            }
             //((MpLogFormPanelController)Find("MpLogFormPanelController")).InitTileControllerList();
        }

        private void OpenDbFolderButton_Click(object sender,EventArgs e) {
            if(!System.IO.File.Exists(MpAppManager.Instance.DataModel.Db.DbPath)) {
                throw new Exception("OpenDbFolder exception: Db file or folder does not exist " + MpAppManager.Instance.DataModel.Db.DbPath);
            }
            System.Diagnostics.Process.Start("explorer.exe",string.Format("/select,\"{0}\"",MpAppManager.Instance.DataModel.Db.DbPath));
        }

        private void MoveDbButton_Click(object sender,EventArgs e) {
            FolderBrowserDialog setNewDirectoryDialog = new FolderBrowserDialog() {
                Description = "Select new location for data file",
                ShowNewFolderButton = true,
                SelectedPath = Path.GetDirectoryName(MpAppManager.Instance.DataModel.Db.DbPath),
                //RootFolder = Environment.SpecialFolder.Personal
            };
            DialogResult newDataDirectoryResult = setNewDirectoryDialog.ShowDialog();
            if(newDataDirectoryResult == DialogResult.OK) {
                string newDbPath = setNewDirectoryDialog.SelectedPath + Path.GetFileName(MpAppManager.Instance.DataModel.Db.DbPath);
                try {
                    MpAppManager.Instance.DataModel.Db.CloseDb();
                    File.Move(MpAppManager.Instance.DataModel.Db.DbPath,newDbPath);
                } 
                catch(Exception ex) {
                    Console.WriteLine("MoveDb exception from path "+ MpAppManager.Instance.DataModel.Db.DbPath+" to new path "+ newDbPath+" : " + ex.ToString());
                }

                MpAppManager.Instance.DataModel.Db.DbPath = newDbPath;

                MpRegistryHelper.Instance.SetValue("DBPath",MpAppManager.Instance.DataModel.Db.DbPath);
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
                    newCopyItems = MpAppManager.Instance.DataModel.Db.GetCopyItems(importDbPath);
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
    }
}

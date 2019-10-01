using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste
{
    public partial class MpSettingsForm : Form {
        //default show log hotkey is Ctrl+D
        public Keys key1 = Keys.Control;
        public Keys key2 = Keys.D;
        public Keys key3 = Keys.None;
        public Keys key4 = Keys.None;

        public bool loadOnLogin = false;
        public bool storeHistory = false;
        public int storeHistoryMaxCount = 0;

        private void SettingsForm_Load(object sender,EventArgs e) {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\"+MpProgram.AppName);

            Key1ComboBox.DataSource = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList();
            Key2ComboBox.DataSource = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList();
            Key3ComboBox.DataSource = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList();
            Key4ComboBox.DataSource = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList();

            //if it does exist, retrieve the stored values  
            int key1Idx = 0, key2Idx = 0, key3Idx = 0, key4Idx = 0;
            if(key != null) {
                string storeHistoryValue = (string)key.GetValue("StoreHistory");
                storeHistory = (storeHistoryValue != "0") ? true : false;
                int.TryParse(storeHistoryValue,out storeHistoryMaxCount);
                List<Keys> k = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList();
                string[] hotKeyValues = ((string)key.GetValue("HotKey")).Split(' ');
                int.TryParse(hotKeyValues[0],out key1Idx);
                int.TryParse(hotKeyValues[1],out key2Idx);
                int.TryParse(hotKeyValues[2],out key3Idx);
                int.TryParse(hotKeyValues[3],out key4Idx);
                key.Close();
            }

            Key1ComboBox.SelectedIndex = key1Idx;
            Key2ComboBox.SelectedIndex = key2Idx;
            Key3ComboBox.SelectedIndex = key3Idx;
            Key4ComboBox.SelectedIndex = key4Idx;

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",true);
            foreach(string vn in rk.GetValueNames()) {
                if(vn == MpProgram.AppName) {
                    this.LoadOnLoginCheckBox.Checked = true;
                }
                else {
                    this.LoadOnLoginCheckBox.Checked = false;
                }
            }

            this.resetDbButton.Click += resetDbButtonClicked;
            this.deleteDbButton.Click += DeleteDbButton_Click;
        }

        private void DeleteDbButton_Click(object sender,EventArgs e) {
            MpSingletonController.Instance.GetMpData().DeleteDb();
        }

        private void SettingsForm_Deactivate(object sender,EventArgs e) {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\" + MpProgram.AppName);
            key.SetValue("StoreHistory",storeHistoryMaxCount.ToString());
            key.SetValue("HotKey",Key1ComboBox.SelectedIndex + " " + Key2ComboBox.SelectedIndex + " " + Key3ComboBox.SelectedIndex + " " + Key4ComboBox.SelectedIndex);
            key.Close();
        }
        private void loadOnLoginCheckBox_CheckedChanged(object sender,EventArgs e) {
            loadOnLogin = ((CheckBox)sender).Checked;
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",true);
            if(loadOnLogin) {
                rk.SetValue(MpProgram.AppName,Application.ExecutablePath);
            }
            else {
                rk.DeleteValue(MpProgram.AppName,false);
            }
        }

        private void storeClipBoardCheckBox_CheckedChanged(object sender,EventArgs e) {
            storeHistory = ((CheckBox)sender).Checked;
            MaxStoredClipBoardEntries.Value = 0;
            storeHistoryMaxCount = 0;
            MaxStoredClipBoardEntries.Enabled = storeHistory;
        }       

        private void maxStoredClipBoardEntries_ValueChanged(object sender,EventArgs e) {
            storeHistoryMaxCount = (int)((NumericUpDown)sender).Value;
        }

        private void Key1ComboBox_SelectedIndexChanged(object sender,EventArgs e) {
            key1 = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList()[((ComboBox)sender).SelectedIndex];
        }
        private void Key2ComboBox_SelectedIndexChanged(object sender,EventArgs e) {
            key2 = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList()[((ComboBox)sender).SelectedIndex];
        }
        private void Key3ComboBox_SelectedIndexChanged(object sender,EventArgs e) {
            key3 = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList()[((ComboBox)sender).SelectedIndex];
        }
        private void Key4ComboBox_SelectedIndexChanged(object sender,EventArgs e) {
            key4 = Enum.GetValues(typeof(Keys)).OfType<Keys>().ToList()[((ComboBox)sender).SelectedIndex];
        }    
        
        private void resetDbButtonClicked(object sender,EventArgs e) {
            // TODO Add confirmation here
            MpSingletonController.Instance.GetMpData().ResetDb();
        }
    }
}

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

namespace MonkeyPaste {
    public partial class MpSettingsForm : Form {              
        private void MpSettingsForm_Load(object sender,EventArgs e) {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run",true);
            this.LoadOnStartUpCheckbox.Checked = rk.GetValue(Properties.Settings.Default.AppName) != null;
            //InitSettings();
        }

        private void groupBox1_Enter(object sender,EventArgs e) {

        }
    }
}

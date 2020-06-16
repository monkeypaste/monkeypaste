using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpLoadOnLoginCheckBoxController : MpController {
        public CheckBox LoadOnStartUpCheckbox { get; set; }

        public MpLoadOnLoginCheckBoxController(MpController p) : base(p) {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            LoadOnStartUpCheckbox = new CheckBox();
            LoadOnStartUpCheckbox.Checked = !(rk.GetValue(Properties.Settings.Default.AppName) == null);


            LoadOnStartUpCheckbox.Click += LoadOnStartUpCheckbox_Click;

        }

           public override void Update() {
            //throw new NotImplementedException();
        }
        private void LoadOnStartUpCheckbox_Click(object sender, EventArgs e) {
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (LoadOnStartUpCheckbox.Checked) {
                rk.SetValue(Properties.Settings.Default.AppName, Application.ExecutablePath);
            } else {
                rk.DeleteValue(Properties.Settings.Default.AppName, false);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpSettingsFormController : MpController {
        public Size SettingsFormMargin = new Size(10, 10);

        public MpDataSettingsPanelController DataSettingsPanelController { get; set; }
        public MpExcludedAppListPanelController ExcludedAppListPanelController { get; set; }
        public MpAppPreferencesPanelController AppPreferencesPanelController { get; set; }

        public MpSettingsForm SettingsForm { get; set; }
        
        
        public MpSettingsFormController(MpController parent) : base(parent) {
            SettingsForm = new MpSettingsForm()
            {
                AutoSize = false,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };
            SettingsForm.Shown += SettingsForm_Shown;
            SettingsForm.FormClosed += SettingsForm_FormClosed;

            AppPreferencesPanelController = new MpAppPreferencesPanelController(this);
            SettingsForm.Controls.Add(AppPreferencesPanelController.AppPreferencesPanel);

            DataSettingsPanelController = new MpDataSettingsPanelController(this);
            SettingsForm.Controls.Add(DataSettingsPanelController.DataSettingsPanel);

            //ExcludedAppListPanelController = new MpExcludedAppListPanelController(this);
            //SettingsForm.Controls.Add(ExcludedAppListPanelController.ExcludedAppListPanel);

            Update();
        }  
        public override void Update() {
            AppPreferencesPanelController.Update();
            DataSettingsPanelController.Update();
            //ExcludedAppListPanelController.Update();
        }
        
        private void SettingsForm_Shown(object sender,EventArgs e) {
            Update();
        }

        

        

        private void SettingsForm_FormClosed(object sender,System.Windows.Forms.FormClosedEventArgs e) {
            ((MpLogFormController)Find("MpLogFormController")).ShowLogForm();
        }
    }
}

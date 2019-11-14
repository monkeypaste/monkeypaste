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
    public partial class MpEnterDbPasswordForm:Form {
        public MpEnterDbPasswordForm() {
            InitializeComponent();
        }
        private void SetPasswordButton_Click(object sender,EventArgs e) {
            if(RememberPasswordCheckbox.Checked) {
                MpRegistryHelper.Instance.SetValue("DBPassword",PasswordTextBox.Text);
            } else {
                MpRegistryHelper.Instance.SetValue("DBPassword","");
            }
            MpLogFormController.Db.SetDbPassword(PasswordTextBox.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender,EventArgs e) {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

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

namespace MpWinFormsApp {
    public partial class MpSetDbPasswordForm:Form {
        public MpSetDbPasswordForm() {
            InitializeComponent();
            InvalidateLabel.Visible = false;
        }

        private void label1_Click(object sender,EventArgs e) {

        }

        private void textBox2_TextChanged(object sender,EventArgs e) {

        }

        private void SetPasswordButton_Click(object sender,EventArgs e) {
            if(PasswordTextBox.Text != ConfirmTextBox.Text) {
                InvalidateLabel.Visible = true;
                return;
            }
            if(RememberPasswordCheckbox.Checked) {
                MpRegistryHelper.Instance.SetValue("DBPassword",PasswordTextBox.Text);
            } else {
                MpRegistryHelper.Instance.SetValue("DBPassword","");                
            }
           // MpSingletonController.Instance.GetMpData().SetDbPassword(PasswordTextBox.Text);
            this.Close();
        }

        private void CancelButton_Click(object sender,EventArgs e) {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Auth0.OidcClient;

namespace MonkeyPaste {
    public partial class MpLoginForm:Form {
        public MpLoginForm() {

            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            InitializeComponent();
            
            //if(MpRegistryController.Instance.GetEmail() != null && MpRegistryController.Instance.GetPassword() != null) {
            //    this.remeberCredentialsCheckBox.Checked = true;
            //}
            //this.autoLoginCheckBox.Checked = MpRegistryController.Instance.GetAutoLogin();
        }

        private void autoLoginCheckBox_CheckedChanged(object sender,EventArgs e) {
            //MpRegistryController.Instance.SetAutoLogin(this.autoLoginCheckBox.Checked);
        }
        private void loginButton_ClickAsync(object sender,EventArgs e) {
            
            //Console.WriteLine("auth9 error: " + loginResult.Error.ToString());
            //var extraParameters = new Dictionary<string,string>();

            //if(!string.IsNullOrEmpty(connectionNameComboBox.Text))
             //    extraParameters.Add("connection",connectionNameComboBox.Text);

            //if(!string.IsNullOrEmpty(audienceTextBox.Text))
            //    extraParameters.Add("audience",audienceTextBox.Text);

           // DisplayResult(await client.LoginAsync(extraParameters: extraParameters));

            //MessageBox.Show("Login successful "+ MpSDbConnection.Instance.GetUser().ToString());
            /*
            if(MpRegistryController.Instance.GetEmail() == null || MpRegistryController.Instance.GetPassword() == null) {
                MessageBox.Show("Please enter your username and password");
                return;
            }
            if(remeberCredentialsCheckBox.Checked) {
                MpRegistryController.Instance.SetEmail(this.emailTextBox.Text);
                MpRegistryController.Instance.SetPassword(this.passwordTextBox.Text);
            }*/
        }

       /* private void DisplayResult(LoginResult loginResult) {
            // Display error
            if(loginResult.IsError) {
                Console.WriteLine("Auth0 error: " + loginResult.Error);
                return;
            }

            // Display result
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Tokens");
            sb.AppendLine("------");
            sb.AppendLine($"id_token: {loginResult.IdentityToken}");
            sb.AppendLine($"access_token: {loginResult.AccessToken}");
            sb.AppendLine($"refresh_token: {loginResult.RefreshToken}");
            sb.AppendLine();

            sb.AppendLine("Claims");
            sb.AppendLine("------");
            foreach(var claim in loginResult.User.Claims) {
                sb.AppendLine($"{claim.Type}: {claim.Value}");
            }

            Console.WriteLine("Auth0 error: " + sb.ToString());

        }*/

    }
}

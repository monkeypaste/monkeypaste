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

        private void InitSettings() {
            this.Controls.Clear();
            int ts = 50;
            int p = 15;
            int curY = p;
           /* foreach(KeyValuePair<string,object> s in MpSingletonController.Instance.Settings.SettingDictionary) {
                Panel sPanel = new Panel() {
                    AutoSize = false,
                    Bounds = new Rectangle(p,curY,this.Width-p*2,ts+(int)(p/2))
                };
                TextBox newLabel = new TextBox() {
                    Text = s.Key,
                    Font = new Font("Calibri",ts,GraphicsUnit.Pixel),
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    TextAlign = HorizontalAlignment.Right,                        
                    Bounds = new Rectangle(0,0,(int)(sPanel.Width/2)-p,sPanel.Height)
                };
                sPanel.Controls.Add(newLabel);

                TextBox sTextBox = new TextBox() {
                    ReadOnly = false,
                    Font = new Font("Calibri",ts,GraphicsUnit.Pixel),
                    Bounds = new Rectangle(newLabel.Right+p,0,(int)(sPanel.Width / 2)-p*2,sPanel.Height),
                    Tag = (object)s.Key,
                    Text = s.Value.GetType() == typeof(Color) ? MpHelperSingleton.Instance.GetColorString((Color)s.Value): Convert.ToString(s.Value)
                };
                sTextBox.LostFocus += STextBox_LostFocus;
                sPanel.Controls.Add(sTextBox);
                this.Controls.Add(sPanel);
                curY += sPanel.Height+p;
            }*/
            curY += (p * 4);

            Button cancelButton = new Button() {
                Text = "Cancel",
                AutoSize = true,
                Font = new Font("Calibri",ts,GraphicsUnit.Pixel),
                Size = new Size(ts*10,ts),
                Location = new Point(15,curY)
            };
            cancelButton.Click += CancelButton_Click;
            this.Controls.Add(cancelButton);

            Button resetButton = new Button() {
                Text = "Reset",
                Font = new Font("Calibri",ts,GraphicsUnit.Pixel),
                Size = new Size(ts * 10,ts),
                AutoSize = true,
                Location = new Point(cancelButton.Right + 5,curY)
            };
            resetButton.Click += ResetButton_Click;
            this.Controls.Add(resetButton);

            Button okButton = new Button() {
                Text = "Ok",
                Font = new Font("Calibri",ts,GraphicsUnit.Pixel),
                Size = new Size(ts * 10,ts),
                AutoSize = true,
                Location = new Point(resetButton.Right + 5,curY)
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);
            curY = okButton.Bottom + p+this.Bounds.Height - this.ClientRectangle.Height;
            this.Size = new Size(this.Width,curY);
        }

        private void OkButton_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void ResetButton_Click(object sender,EventArgs e) {
            //MpSingletonController.Instance.Settings.Reset();
            InitSettings();
        }

        private void CancelButton_Click(object sender,EventArgs e) {
            this.Close();
        }

        private void STextBox_LostFocus(object sender,EventArgs e) {
            if(sender.GetType() == typeof(TextBox)) {
                TextBox tb = (TextBox)sender;
               /* MpSettingValueType st = MpSingletonController.Instance.Settings.GetSettingValueType((string)tb.Tag);
                switch(st) {
                    case MpSettingValueType.Int:
                        MpSingletonController.Instance.SetSetting((string)tb.Tag,Convert.ToInt32(tb.Text));
                        break;
                    case MpSettingValueType.Float:
                        MpSingletonController.Instance.SetSetting((string)tb.Tag,(float)Convert.ToDouble(tb.Text));
                        break;
                    case MpSettingValueType.Color:
                        MpSingletonController.Instance.SetSetting((string)tb.Tag,MpHelperSingleton.Instance.GetColorFromString(tb.Text));
                        break;
                    case MpSettingValueType.String:
                        MpSingletonController.Instance.SetSetting((string)tb.Tag,tb.Text);
                        break;
                }**/
                
            }
            
        }

        private void MpSettingsForm_Load(object sender,EventArgs e) {
            this.AutoSize = false;
            this.Size = new Size(1800,1800);
            InitSettings();
        }
    }
}

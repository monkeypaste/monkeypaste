using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuSettingsButtonController:MpController {
        public MpSettingsFormController SettingsFormController { get;set;}

        public MpLogMainMenuSettingsButton LogMainMenuSettingsButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMainMenuSettingsButtonController(MpController parentController) : base(parentController) {
            LogMainMenuSettingsButton = new MpLogMainMenuSettingsButton() {
                Margin = new Padding(3),  
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.info,
                DefaultImage = Properties.Resources.info,
                OverImage = Properties.Resources.info2,
                DownImage = Properties.Resources.info2
            };
            LogMainMenuSettingsButton.MouseHover += MpLogMainMenuSettingsButton_MouseHover;
            LogMainMenuSettingsButton.MouseLeave += MpLogMainMenuSettingsButton_MouseLeave;
            LogMainMenuSettingsButton.MouseClick += MpLogMainMenuSettingsButton_MouseClick;

            SettingsFormController = new MpSettingsFormController((MpTaskbarIconController)Find(typeof(MpTaskbarIconController)));
        }
           public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;
            //button size
            int bs = (int)((float)lmmpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);
            //button pad
            int bp = (int)((lmmpr.Height - bs)/2.0f);
            LogMainMenuSettingsButton.SetBounds(bp,bp,bs,bs);

            LogMainMenuSettingsButton.Invalidate();
        }
        private void MpLogMainMenuSettingsButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
            SettingsFormController.SettingsForm.ShowDialog();
        }

        private void MpLogMainMenuSettingsButton_MouseLeave(object sender,EventArgs e) {
            LogMainMenuSettingsButton.Image = LogMainMenuSettingsButton.DefaultImage;
        }

        private void MpLogMainMenuSettingsButton_MouseHover(object sender,EventArgs e) {
            LogMainMenuSettingsButton.Image = LogMainMenuSettingsButton.OverImage;
        }
    }
}
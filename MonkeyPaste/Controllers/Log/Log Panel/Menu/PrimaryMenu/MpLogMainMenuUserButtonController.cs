using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuUserButtonController:MpController {
        public MpLogMainMenuUserButton LogMainMenuUserButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMainMenuUserButtonController(MpController parentController) : base(parentController) {
            LogMainMenuUserButton = new MpLogMainMenuUserButton() {
                Margin = new Padding(3),
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.user,
                DefaultImage = Properties.Resources.user,
                OverImage = Properties.Resources.user2,
                DownImage = Properties.Resources.user2
            };
            LogMainMenuUserButton.MouseHover += MpLogMainMenuUserButton_MouseHover;
            LogMainMenuUserButton.MouseLeave += MpLogMainMenuUserButton_MouseLeave;
            LogMainMenuUserButton.MouseClick += MpLogMainMenuUserButton_MouseClick;
        }
        public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;
            //log main menu info button rect
            Rectangle lmmibr = ((MpLogMainMenuPanelController)Parent).LogMainMenuInfoButtonController.LogMainMenuInfoButton.Bounds;
            //icon pad
            int ip = (int)((float)lmmpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);
            LogMainMenuUserButton.SetBounds(lmmpr.Right - lmmpr.Height,0,ip,ip);

            LogMainMenuUserButton.Invalidate();
        }
        private void MpLogMainMenuUserButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void MpLogMainMenuUserButton_MouseLeave(object sender,EventArgs e) {
            LogMainMenuUserButton.Image = LogMainMenuUserButton.DefaultImage;
        }

        private void MpLogMainMenuUserButton_MouseHover(object sender,EventArgs e) {
            LogMainMenuUserButton.Image = LogMainMenuUserButton.OverImage;
        }
    }
}

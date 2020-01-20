using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuInfoButtonController:MpController {
        public MpLogMainMenuInfoButton LogMainMenuInfoButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMainMenuInfoButtonController(MpController parentController) : base(parentController) {
            LogMainMenuInfoButton = new MpLogMainMenuInfoButton() {
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
            LogMainMenuInfoButton.MouseHover += MpLogMainMenuInfoButton_MouseHover;
            LogMainMenuInfoButton.MouseLeave += MpLogMainMenuInfoButton_MouseLeave;
            LogMainMenuInfoButton.MouseClick += MpLogMainMenuInfoButton_MouseClick;
        }
        public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;
            //icon pad
            int ip = (int)((float)lmmpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);
            LogMainMenuInfoButton.SetBounds(lmmpr.Right - (lmmpr.Height*2)-5,0,ip,ip);

            LogMainMenuInfoButton.Invalidate();
        }
        private void MpLogMainMenuInfoButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void MpLogMainMenuInfoButton_MouseLeave(object sender,EventArgs e) {
            LogMainMenuInfoButton.Image = LogMainMenuInfoButton.DefaultImage;
        }

        private void MpLogMainMenuInfoButton_MouseHover(object sender,EventArgs e) {
            LogMainMenuInfoButton.Image = LogMainMenuInfoButton.OverImage;
        }
    }
}
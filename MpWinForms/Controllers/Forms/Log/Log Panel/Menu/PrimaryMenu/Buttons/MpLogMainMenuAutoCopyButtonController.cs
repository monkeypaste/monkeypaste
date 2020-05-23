using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuAutoCopyButtonController:MpController {
        public MpLogMainMenuAutoCopyButton LogMainMenuAutoCopyButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMainMenuAutoCopyButtonController(MpController parentController) : base(parentController) {
            LogMainMenuAutoCopyButton = new MpLogMainMenuAutoCopyButton() {
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
            LogMainMenuAutoCopyButton.MouseHover += MpLogMainMenuAutoCopyButton_MouseHover;
            LogMainMenuAutoCopyButton.MouseLeave += MpLogMainMenuAutoCopyButton_MouseLeave;
            LogMainMenuAutoCopyButton.MouseClick += MpLogMainMenuAutoCopyButton_MouseClick;
        }
           public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;
            //button size
            int bs = (int)((float)lmmpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);
            //button pad
            int bp = (int)((lmmpr.Height - bs) / 2.0f);
            LogMainMenuAutoCopyButton.SetBounds(bs + bs + bp + bp + bp,bp,bs,bs);

            LogMainMenuAutoCopyButton.Invalidate();
        }
        private void MpLogMainMenuAutoCopyButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void MpLogMainMenuAutoCopyButton_MouseLeave(object sender,EventArgs e) {
            LogMainMenuAutoCopyButton.Image = LogMainMenuAutoCopyButton.DefaultImage;
        }

        private void MpLogMainMenuAutoCopyButton_MouseHover(object sender,EventArgs e) {
            LogMainMenuAutoCopyButton.Image = LogMainMenuAutoCopyButton.OverImage;
        }
    }
}
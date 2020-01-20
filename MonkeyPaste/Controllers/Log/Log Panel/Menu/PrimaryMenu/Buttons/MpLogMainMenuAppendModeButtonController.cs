using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMainMenuAppendModeButtonController:MpController {
        public MpLogMainMenuAppendModeButton LogMainMenuAppendModeButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMainMenuAppendModeButtonController(MpController parentController) : base(parentController) {
            LogMainMenuAppendModeButton = new MpLogMainMenuAppendModeButton() {
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
            LogMainMenuAppendModeButton.MouseHover += MpLogMainMenuAppendModeButton_MouseHover;
            LogMainMenuAppendModeButton.MouseLeave += MpLogMainMenuAppendModeButton_MouseLeave;
            LogMainMenuAppendModeButton.MouseClick += MpLogMainMenuAppendModeButton_MouseClick;
        }
        public override void Update() {
            //log main menu panel rect
            Rectangle lmmpr = ((MpLogMainMenuPanelController)Parent).LogMainMenuPanel.Bounds;
            //button size
            int bs = (int)((float)lmmpr.Height * Properties.Settings.Default.LogMainMenuIconSizeRatio);
            //button pad
            int bp = (int)((lmmpr.Height - bs) / 2.0f);
            LogMainMenuAppendModeButton.SetBounds(bs+bp+bp,bp,bs,bs);

            LogMainMenuAppendModeButton.Invalidate();
        }
        private void MpLogMainMenuAppendModeButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void MpLogMainMenuAppendModeButton_MouseLeave(object sender,EventArgs e) {
            LogMainMenuAppendModeButton.Image = LogMainMenuAppendModeButton.DefaultImage;
        }

        private void MpLogMainMenuAppendModeButton_MouseHover(object sender,EventArgs e) {
            LogMainMenuAppendModeButton.Image = LogMainMenuAppendModeButton.OverImage;
        }
    }
}
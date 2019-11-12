using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogMenuTileTokenButtonController : MpController {
        public MpLogMenuTileTokenButton LogMenuTileTokenButton { get; set; }
        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpLogMenuTileTokenButtonController(MpController parentController) : base(parentController) {
            LogMenuTileTokenButton = new MpLogMenuTileTokenButton(((MpLogMenuTileTokenPanelController)parentController).TokenId) {        
                Margin = new Padding(3),
                Padding = Padding.Empty,
                BackColor =((MpLogMenuTileTokenPanelController)Parent).LogMenuTileTokenPanel.BackColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.close2,
                DefaultImage = Properties.Resources.close2,
                OverImage = Properties.Resources.close,
                DownImage = Properties.Resources.close
            };
            LogMenuTileTokenButton.MouseHover += LogMenuTileTokenButton_MouseHover;
            LogMenuTileTokenButton.MouseLeave += LogMenuTileTokenButton_MouseLeave;
            LogMenuTileTokenButton.MouseClick += LogMenuTileTokenButton_MouseClick;
        }

        private void LogMenuTileTokenButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void LogMenuTileTokenButton_MouseLeave(object sender,EventArgs e) {
            LogMenuTileTokenButton.Image = LogMenuTileTokenButton.DefaultImage;
        }

        private void LogMenuTileTokenButton_MouseHover(object sender,EventArgs e) {
            LogMenuTileTokenButton.Image = LogMenuTileTokenButton.OverImage;
        }

        public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpLogMenuTileTokenPanelController)Parent).LogMenuTileTokenPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;
            //token textbox font size
            int ttfs = (int)(tph * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio);
            int h = tpr.Height;// (int)((float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenFontSizeRatio);
            int p = (int)(ttfs/4.0f);// (int)((float)(tpr.Height - h)/2.0f);
            LogMenuTileTokenButton.Size = new Size(ttfs,ttfs);
            LogMenuTileTokenButton.Location = new Point(tpr.Width - ttfs - p,(int)((float)p*0.5f));
            LogMenuTileTokenButton.BringToFront();
        }
    }
}

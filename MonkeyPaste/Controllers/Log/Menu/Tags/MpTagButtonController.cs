using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagButtonController : MpController {
        public MpTagButton LogMenuTagButton { get; set; }
        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpTagButtonController(MpController parentController) : base(parentController) {
            LogMenuTagButton = new MpTagButton(this,((MpTagPanelController)parentController).TokenId) {        
                Margin = new Padding(3),
                Padding = Padding.Empty,
                BackColor =((MpTagPanelController)Parent).TagPanel.BackColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.close2,
                DefaultImage = Properties.Resources.close2,
                OverImage = Properties.Resources.close,
                DownImage = Properties.Resources.close
            };
            LogMenuTagButton.MouseHover += LogMenuTileTokenButton_MouseHover;
            LogMenuTagButton.MouseLeave += LogMenuTileTokenButton_MouseLeave;
            LogMenuTagButton.MouseClick += LogMenuTileTokenButton_MouseClick;
        }

        private void LogMenuTileTokenButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void LogMenuTileTokenButton_MouseLeave(object sender,EventArgs e) {
            LogMenuTagButton.Image = LogMenuTagButton.DefaultImage;
        }

        private void LogMenuTileTokenButton_MouseHover(object sender,EventArgs e) {
            LogMenuTagButton.Image = LogMenuTagButton.OverImage;
        }

        public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpTagPanelController)Parent).TagPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;
            //token textbox font size
            int ttfs = (int)(tph * Properties.Settings.Default.LogMenuTileTokenPanelHeightRatio);
            int h = tpr.Height;// (int)((float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenFontSizeRatio);
            int p = (int)(ttfs/4.0f);// (int)((float)(tpr.Height - h)/2.0f);
            LogMenuTagButton.Size = new Size(ttfs,ttfs);
            LogMenuTagButton.Location = new Point(tpr.Width - ttfs - p,(int)((float)p*0.5f));
            LogMenuTagButton.BringToFront();
        }
    }
}

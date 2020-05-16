using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagButtonController : MpController {
        public MpTagButton TagButton { get; set; }
        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpTagButtonController(MpController parentController,bool isNew) : base(parentController) {
            TagButton = new MpTagButton(((MpTagChooserPanelController)((MpTagPanelController)parentController).Parent).TagPanelControllerList.IndexOf(((MpTagPanelController)Parent))) {                    
                Margin = new Padding(3),
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor =((MpTagPanelController)Parent).TagPanel.BackColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = isNew ? Properties.Resources.add2 : Properties.Resources.close2,
                DefaultImage = isNew ? Properties.Resources.add2 : Properties.Resources.close2,
                OverImage = isNew ? Properties.Resources.add : Properties.Resources.close,
                DownImage = isNew ? Properties.Resources.add : Properties.Resources.close
            };
            TagButton.MouseHover += LogMenuTileTokenButton_MouseHover;
            TagButton.MouseLeave += LogMenuTileTokenButton_MouseLeave;
            TagButton.MouseClick += LogMenuTileTokenButton_MouseClick;
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
            TagButton.Size = new Size(ttfs,ttfs);
            TagButton.Location = new Point(tpr.Width - ttfs - p,(int)((float)p*0.5f));
            TagButton.BringToFront();

            TagButton.Invalidate();
        }

        private void LogMenuTileTokenButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void LogMenuTileTokenButton_MouseLeave(object sender,EventArgs e) {
            TagButton.Image = TagButton.DefaultImage;
        }

        private void LogMenuTileTokenButton_MouseHover(object sender,EventArgs e) {
            TagButton.Image = TagButton.OverImage;
        }
    }
}

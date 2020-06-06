using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpExcludedAppButtonController : MpController {
        public delegate void ButtonClicked(object sender, EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpButton ExcludedAppButton { get; set; }

        public MpExcludedAppButtonController(MpController parentController,bool isNew) : base(parentController) {
            ExcludedAppButton = new MpButton() {                    
                Margin = new Padding(3),
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor = Color.Yellow,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = isNew ? Properties.Resources.add2 : Properties.Resources.close2,
                DefaultImage = isNew ? Properties.Resources.add2 : Properties.Resources.close2,
                OverImage = isNew ? Properties.Resources.add : Properties.Resources.close,
                DownImage = isNew ? Properties.Resources.add : Properties.Resources.close
            };
            ExcludedAppButton.MouseHover += LogMenuTileTokenButton_MouseHover;
            ExcludedAppButton.MouseLeave += LogMenuTileTokenButton_MouseLeave;
            ExcludedAppButton.MouseClick += LogMenuTileTokenButton_MouseClick;
        }       

           public override void Update() {
            //token panel rect
            Rectangle tpr = ((MpExcludedAppPanelController)Parent).ExcludedAppPanel.Bounds;
            //token panel height
            float tph = (float)tpr.Height * Properties.Settings.Default.TagPanelHeightRatio;
            //token chooser pad
            int tcp = tpr.Height - (int)tph;
            //token textbox font size
            int ttfs = (int)(tph * Properties.Settings.Default.TagPanelHeightRatio);
            int h = tpr.Height;// (int)((float)tpr.Height * Properties.Settings.Default.LogMenuTileTokenFontSizeRatio);
            int p = (int)(ttfs/4.0f);// (int)((float)(tpr.Height - h)/2.0f);
            ExcludedAppButton.Size = new Size(ttfs,ttfs);
            ExcludedAppButton.Location = new Point(tpr.Width - ttfs - p,(int)((float)p*0.5f));
            ExcludedAppButton.BringToFront();

            ExcludedAppButton.Invalidate();
        }

        private void LogMenuTileTokenButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void LogMenuTileTokenButton_MouseLeave(object sender,EventArgs e) {
            ExcludedAppButton.Image = ExcludedAppButton.DefaultImage;
        }

        private void LogMenuTileTokenButton_MouseHover(object sender,EventArgs e) {
            ExcludedAppButton.Image = ExcludedAppButton.OverImage;
        }
    }
}

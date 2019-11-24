using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileHeaderCloseButtonController : MpController {
        public MpTileHeaderButton CloseButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpTileHeaderCloseButtonController(int tileId,int panelId,MpController parentController) : base(parentController) {
            CloseButton = new MpTileHeaderButton(tileId,panelId) {
                Margin = new Padding(3),
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor = Color.Transparent,//((MpTileHeaderPanelController)Parent).TileHeaderPanel.BackColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.close2,
                DefaultImage = Properties.Resources.close2,
                OverImage = Properties.Resources.close,
                DownImage = Properties.Resources.close
            };
            CloseButton.MouseHover += CloseButton_MouseHover;
            CloseButton.MouseLeave += CloseButton_MouseLeave;
            CloseButton.MouseClick += CloseButton_MouseClick;


            Link(new List<MpIView> { CloseButton });
        }
        public override void Update() {
            //tile header panel rect
            Rectangle thpr = ((MpTileHeaderPanelController)Parent).TileHeaderPanel.Bounds;
            //close button height
            int cbh = (int)((float)thpr.Height * Properties.Settings.Default.TileHeaderHeightRatio);
            int h = thpr.Height;
            int p = 5;
            CloseButton.Size = new Size(h,h);
            CloseButton.Location = new Point(thpr.Width - h - p,0);
            CloseButton.BringToFront();

            CloseButton.Invalidate();
        }
        private void CloseButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void CloseButton_MouseLeave(object sender,EventArgs e) {
            CloseButton.Image = CloseButton.DefaultImage;
        }

        private void CloseButton_MouseHover(object sender,EventArgs e) {
            CloseButton.Image = CloseButton.OverImage;
        }
    }
}

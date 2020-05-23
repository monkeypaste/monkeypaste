using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileHeaderExpandButtonController : MpController {
        public MpButton ExpandButton { get; set; }

        public delegate void ButtonClicked(object sender,EventArgs e);
        public event ButtonClicked ButtonClickedEvent;

        public MpTileHeaderExpandButtonController(int tileId,int panelId,MpController parentController) : base(parentController) {
            ExpandButton = new MpButton() {
                Margin = new Padding(3),
                Padding = Padding.Empty,
                TabIndex = 1,
                BackColor = Color.Transparent,//((MpTileHeaderPanelController)Parent).TileHeaderPanel.BackColor,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.doubleSidedArrow2,
                DefaultImage = Properties.Resources.doubleSidedArrow2,
                OverImage = Properties.Resources.doubleSidedArrow,
                DownImage = Properties.Resources.doubleSidedArrow
            };
            ExpandButton.MouseHover += ExpandButton_MouseHover;
            ExpandButton.MouseLeave += ExpandButton_MouseLeave;
            ExpandButton.MouseClick += ExpandButton_MouseClick;


            //Link(new List<MpIView> { ExpandButton });
        }
           public override void Update() {
            //tile header close button rect
            Rectangle thcbr = ((MpTileHeaderPanelController)Parent).TileHeaderCloseButtonController.CloseButton.Bounds;
            int h = thcbr.Height;
            int p = 2;
            ExpandButton.Size = new Size(h,h);
            ExpandButton.Location = new Point(thcbr.Location.X - h - p,0);
            ExpandButton.BringToFront();

            ExpandButton.Invalidate();
        }
        private void ExpandButton_MouseClick(object sender,MouseEventArgs e) {
            ButtonClickedEvent(this,e);
        }

        private void ExpandButton_MouseLeave(object sender,EventArgs e) {
            ExpandButton.Image = ExpandButton.DefaultImage;
        }

        private void ExpandButton_MouseHover(object sender,EventArgs e) {
            ExpandButton.Image = ExpandButton.OverImage;
        }
    }
}

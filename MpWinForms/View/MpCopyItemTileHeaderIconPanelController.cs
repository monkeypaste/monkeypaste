using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItemTileHeaderIconPanelController : MpController {        
        public MpCopyItemTileTitleIconPanel CopyItemTileTitleIconPanel { get; set; }

        public PictureBox CopyItemTitleIconBox { get; set; }

        public MpCopyItemTileHeaderIconPanelController(int titleHeight,MpCopyItem ci,MpController parentController) : base(parentController) {
            CopyItemTileTitleIconPanel = new MpCopyItemTileTitleIconPanel() {
                BackColor = (Color)MpSingletonController.Instance.Settings.GetSetting("TileIconBorderColor"),//Color.Transparent,
                AutoSize = false
                //Anchor = AnchorStyles.Top,
                //Location = new Point(),
                //AutoSize = true,
                //Size = new Size(tileSize,(int)((float)tileSize* (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio")))
            };
            CopyItemTileTitleIconPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            CopyItemTitleIconBox = new PictureBox() {
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                //BorderStyle = BorderStyle.FixedSingle,
                //Margin = new Padding(10),
                //Anchor = AnchorStyles.Left,
                Image = MpSingletonController.Instance.GetMpData().GetMpIcon(MpSingletonController.Instance.GetMpData().GetMpApp(ci.appId).iconId).IconImage
            };
            CopyItemTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            CopyItemTileTitleIconPanel.Controls.Add(CopyItemTitleIconBox);
            CopyItemTitleIconBox.BringToFront();

            UpdateBounds();
        }

        public override void UpdateBounds() {
            //tileheaderpanelcontroller rect
            Rectangle thpcr = ((MpCopyItemTileHeaderPanelController)ParentController).CopyItemTileHeaderPanel.Bounds;
            //header icon pad
            int hip = (int)((float)thpcr.Height*(float)MpSingletonController.Instance.Settings.GetSetting("TileIconPadWidthRatio"));
            CopyItemTileTitleIconPanel.SetBounds(0,0,thpcr.Height,thpcr.Height);
            CopyItemTitleIconBox.SetBounds(hip,hip,thpcr.Width - (hip * 2),thpcr.Width - (hip * 2));            
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}

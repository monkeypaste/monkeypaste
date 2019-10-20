using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleIconPanelController : MpController {        
        public MpTileIconPanel TileTitleIconPanel { get; set; }

        public PictureBox TileTitleIconBox { get; set; }

        public MpTileTitleIconPanelController(MpCopyItem ci,MpController parentController) : base(parentController) {
            TileTitleIconPanel = new MpTileIconPanel() {
                BackColor = (Color)MpSingletonController.Instance.Settings.GetSetting("TileIconBorderColor"),
                AutoSize = false
            };
            TileTitleIconPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitleIconBox = new PictureBox() {
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = MpSingletonController.Instance.GetMpData().GetMpIcon(MpSingletonController.Instance.GetMpData().GetMpApp(ci.appId).iconId).IconImage
            };
            TileTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleIconPanel.Controls.Add(TileTitleIconBox);

            UpdateBounds();
        }

        public override void UpdateBounds() {
            //tiletitlepanelcontroller rect
            Rectangle thpcr = ((MpTileTitlePanelController)ParentController).TileTitlePanel.Bounds;
            //header icon pad
            int hip = (int)((float)thpcr.Width*(float)MpSingletonController.Instance.Settings.GetSetting("TilePadWidthRatio"));
            TileTitleIconPanel.SetBounds(hip,hip,thpcr.Height-hip,thpcr.Height-hip);
            Rectangle ttipr = TileTitleIconPanel.Bounds;
            //icon border pad
            int ibp = (int)((float)MpSingletonController.Instance.GetSetting("TileIconBorderRatio") * (float)ttipr.Width);
            TileTitleIconBox.SetBounds(ibp,ibp,ttipr.Width - (ibp * 2),ttipr.Width - (ibp * 2));
            TileTitleIconBox.Refresh();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}

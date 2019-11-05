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

        public MpTileTitleIconBox TileTitleIconBox { get; set; }

        public MpTileTitleIconPanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileTitleIconPanel = new MpTileIconPanel(tileId,panelId) {
                AutoSize = false,
                Thickness = 0
            };
            TileTitleIconPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitleIconBox = new MpTileTitleIconBox(tileId,panelId) {
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = MpSingletonController.Instance.GetMpData().GetMpIcon(MpSingletonController.Instance.GetMpData().GetMpApp(ci.appId).iconId).IconImage
            };
            TileTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            TileTitleIconPanel.Controls.Add(TileTitleIconBox);

            Update();

            Link(new List<MpIView> { (MpIView)TileTitleIconPanel,(MpIView)TileTitleIconBox });
        }

        public override void Update() {
            //tiletitlepanelcontroller rect
            Rectangle thpcr = ((MpTileTitlePanelController)Parent).TileTitlePanel.Bounds;
            //header icon pad
            int hip = (int)((float)thpcr.Width* Properties.Settings.Default.TilePadWidthRatio);
            TileTitleIconPanel.SetBounds(hip,hip,thpcr.Height-hip,thpcr.Height-hip);
            Rectangle ttipr = TileTitleIconPanel.Bounds;
            //icon border pad
            int ibp = (int)(Properties.Settings.Default.TileIconBorderRatio * (float)ttipr.Width);
            TileTitleIconBox.SetBounds(ibp,ibp,ttipr.Width - (ibp * 2),ttipr.Width - (ibp * 2));
            TileTitleIconPanel.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            TileTitleIconBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            TileTitleIconBox.Refresh();
        }
    }
}

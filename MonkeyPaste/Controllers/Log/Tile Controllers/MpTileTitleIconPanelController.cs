using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleIconPanelController : MpController {        
        //public MpTileIconPanel TileTitleIconPanel { get; set; }

        public MpTileTitleIconBox TileTitleIconBox { get; set; }

        public MpTileTitleIconPanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            //TileTitleIconPanel = new MpTileIconPanel(tileId,panelId) {
            //    AutoSize = false,
            //    Thickness = 0
            //};
            //TileTitleIconPanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            TileTitleIconBox = new MpTileTitleIconBox(tileId,panelId) {
                
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = MpSingletonController.Instance.GetMpData().GetMpIcon(MpSingletonController.Instance.GetMpData().GetMpApp(ci.appId).iconId).IconImage
            };
            TileTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
            //TileTitleIconPanel.Controls.Add(TileTitleIconBox);

            //Update();

            Link(new List<MpIView> { (MpIView)TileTitleIconBox });
        }

        public override void Update() {
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Parent).TileTitlePanel.Bounds;
            //target icon size
            int tis = (int)((float)ttpr.Width * Properties.Settings.Default.TileTitleIconWidthRatio);
            //target icon center x,y
            int ticx = (int)((float)(ttpr.Right - tis) / 2.0f);// + (ttpr.Right - tis);
            int ticy = (int)((float)ttpr.Height / 2.0f);
            //target icon x,y
            int tix = (int)((float)ticx + ((float)tis * 1.75f));
            int tiy = (int)((float)ticy - ((float)tis / 2.0f));

            TileTitleIconBox.Size = new Size(tis,tis);
            TileTitleIconBox.Location = new Point(tix,tiy);

            //tile pad
            //int tp = (int)((float)ttpr.Width* Properties.Settings.Default.TilePadWidthRatio);

            //TileTitleIconPanel.SetBounds(tp,tp,ttpr.Height-tp,ttpr.Height-tp);
            //Rectangle ttipr = TileTitleIconPanel.Bounds;
            //icon border pad
            //int ibp = (int)(Properties.Settings.Default.TileIconBorderRatio * (float)ttipr.Width);
            //TileTitleIconBox.SetBounds(ibp,ibp,ttipr.Width - (ibp * 2),ttipr.Width - (ibp * 2));

            //TileTitleIconPanel.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
            //TileTitleIconBox.BackColor = ((MpTilePanelController)((MpTileTitlePanelController)Parent).Parent).TilePanel.BackColor;
        }
    }
}

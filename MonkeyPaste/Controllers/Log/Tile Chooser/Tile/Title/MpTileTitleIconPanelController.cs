using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleIconPanelController : MpController {        
        public MpTileTitleIconBox TileTitleIconBox { get; set; }

        public MpTileTitleIconPanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {
            TileTitleIconBox = new MpTileTitleIconBox(tileId,panelId) {
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = MpHelperSingleton.Instance.RotateImage(ci.App.Icon.IconImage,20.0f,false,false,Color.Transparent)
            };
            TileTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

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
            int tix = (int)((float)ticx + ((float)tis * 1.618f));
            int tiy = (int)((float)ticy - ((float)tis / 2.0f));
            
            TileTitleIconBox.Size = new Size(tis,tis);
            TileTitleIconBox.Location = new Point(tix,tiy);


            TileTitleIconBox.Invalidate();
        }
    }
}

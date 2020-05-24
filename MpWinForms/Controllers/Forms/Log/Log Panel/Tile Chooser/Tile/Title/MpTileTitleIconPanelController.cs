using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleIconPanelController : MpControlController {        
        public PictureBox TileTitleIconBox { get; set; }

        public MpTileTitleIconPanelController(MpCopyItem ci,MpController Parent) : base(Parent) {
            TileTitleIconBox = new PictureBox() {
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = MpHelperSingleton.Instance.RotateImage(ci.App.Icon.IconImage,20.0f,false,false,Color.Transparent),
                Bounds = GetBounds()
            };
            TileTitleIconBox.DoubleBuffered(true);
            TileTitleIconBox.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
        }
        public override void Update() {
            TileTitleIconBox.Bounds = GetBounds();
            TileTitleIconBox.Invalidate();
        }

        public override Rectangle GetBounds() {
            //tile title panel rect
            Rectangle ttpr = ((MpTileTitlePanelController)Parent).GetBounds();
            //target icon size
            int tis = (int)((float)ttpr.Height * Properties.Settings.Default.TileTitleIconHeightRatio);
            //target icon center x,y
            int ticx = (int)((float)( tis) / 6.0f);
            int ticy = (int)((float)(ttpr.Height-tis) / 2.0f);
            //target icon x,y
            //int tix = (int)((float)ticx + ((float)tis * 1.618f));
            //int tiy = (int)((float)ticy - ((float)tis / 2.0f));

            return new Rectangle(-ticx, ticy, tis, tis);
        }
    }
}

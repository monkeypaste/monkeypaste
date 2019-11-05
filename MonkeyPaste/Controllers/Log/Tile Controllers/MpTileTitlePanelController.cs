using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        //title panel
        public MpTileTitlePanel TileTitlePanel { get; set; }

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }       
        

        public MpTileTitlePanelController(int tileId,int panelId,MpCopyItem ci,MpController Parent) : base(Parent) {            
            //parent panel
            TileTitlePanel = new MpTileTitlePanel(tileId,panelId) {
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,//((MpTilePanelController)Parent).TilePanel.BackColor,
            };
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;            

            TileTitleTextBoxController = new MpTileTitleTextBoxController(tileId,panelId,ci,this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);

            TileTitleIconPanelController = new MpTileTitleIconPanelController(tileId,panelId,ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconPanel);
                        
            Update();

            Link(new List<MpIView> { TileTitlePanel});
        }
        public override void Update() {
            //tile rect
            Rectangle tr = ((MpTilePanelController)Parent).TilePanel.Bounds;
            //tile title height
            int tth = (int)((float)tr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            TileTitlePanel.SetBounds(tp,tp,tr.Width-tp,tth);

            TileTitlePanel.BackColor = Color.Transparent;// ((MpTilePanelController)Parent).TilePanel.BackColor;
            TileTitleIconPanelController.Update();
            TileTitleTextBoxController.Update();
        }
    }
}
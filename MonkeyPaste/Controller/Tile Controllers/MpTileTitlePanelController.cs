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
        public MpTileDetailsPanelController TileDetailsPanelController { get; set; }

        public MpTileTitlePanelController(MpCopyItem ci,MpController parentController) : base(parentController) {            
            //parent panel
            TileTitlePanel = new MpTileTitlePanel() {
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,//((MpTilePanelController)ParentController).TilePanel.BackColor,
                Margin = new Padding(20)
            };
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;            

            TileTitleTextBoxController = new MpTileTitleTextBoxController(ci,this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);

            TileTitleIconPanelController = new MpTileTitleIconPanelController(ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconPanel);

            TileDetailsPanelController = new MpTileDetailsPanelController(this);
            TileTitlePanel.Controls.Add(TileDetailsPanelController.TileDetailsPanel);

            UpdateBounds();
        }
        public override void UpdateBounds() {
            //tile rect
            Rectangle tr = ((MpTilePanelController)ParentController).TilePanel.Bounds;
            //tiletitle height
            int tth = (int)((float)tr.Height * (float)MpSingletonController.Instance.GetSetting("TileTitleHeightRatio"));
            TileTitlePanel.SetBounds(0,0,tr.Width,tth);

            TileTitlePanel.BackColor = Color.Transparent;// ((MpTilePanelController)ParentController).TilePanel.BackColor;

            TileTitleIconPanelController.UpdateBounds();
            TileTitleTextBoxController.UpdateBounds();
            TileDetailsPanelController.UpdateBounds();
            TileDetailsPanelController.TileDetailsPanel.BringToFront();
        }
    }
}
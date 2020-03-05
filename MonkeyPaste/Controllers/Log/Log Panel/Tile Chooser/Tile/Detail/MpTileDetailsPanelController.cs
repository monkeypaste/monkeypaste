using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsPanelController : MpController {
        public MpTileDetailsLabelController TileDetailsTextBoxController { get; set; }
        public MpTileDetailPanel TileDetailsPanel { get; set; }

        public MpTileDetailsPanelController(MpCopyItem ci,int tileId,int panelId,MpController Parent) : base(Parent) {
            TileDetailsPanel = new MpTileDetailPanel(tileId,panelId) {
                AutoSize = false,
                BackColor = Color.FromArgb(0,0,0,0),
                BorderStyle = BorderStyle.None
            };

            TileDetailsTextBoxController = new MpTileDetailsLabelController(tileId,panelId,this);
            TileDetailsPanel.Controls.Add(TileDetailsTextBoxController.DetailsLabel);

            Link(new List<MpIView> { TileDetailsPanel });
        }
        public override void Update() {
            //tile  rect
            Rectangle tr = ((MpTilePanelController)Parent).TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //tile details height
            int tdh = (int)(Properties.Settings.Default.TileDetailHeightRatio * tr.Height);
            TileDetailsPanel.SetBounds(tp,tr.Height-tdh-tp-tp,tr.Width-(tp*2),tdh);

            TileDetailsPanel.Invalidate();

            TileDetailsTextBoxController.Update();
        }
    }
}

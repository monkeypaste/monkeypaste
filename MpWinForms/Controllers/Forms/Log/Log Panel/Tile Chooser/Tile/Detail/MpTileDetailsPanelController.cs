using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileDetailsPanelController : MpController {
        public MpTileDetailsLabelController TileDetailsLabelController { get; set; }
        public Panel TileDetailsPanel { get; set; }

        public MpTileDetailsPanelController(MpCopyItem ci,MpController Parent) : base(Parent) {
            TileDetailsPanel = new Panel() {
                AutoSize = false,
                BackColor = Color.FromArgb(0,0,0,0),
                BorderStyle = BorderStyle.None
            };
            TileDetailsPanel.DoubleBuffered(true);
            TileDetailsLabelController = new MpTileDetailsLabelController(this);
            TileDetailsPanel.Controls.Add(TileDetailsLabelController.DetailsLabel);
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

            TileDetailsLabelController.Update();
        }
    }
}

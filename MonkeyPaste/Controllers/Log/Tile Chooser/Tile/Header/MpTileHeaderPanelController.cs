using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileHeaderPanelController : MpController {
        public MpTileHeaderCloseButtonController TileHeaderCloseButtonController { get; set; }
        public MpTileHeaderExpandButtonController TileHeaderExpandButtonController { get; set; }

        public MpTileHeaderPanel TileHeaderPanel { get; set; }

        public MpTileHeaderPanelController(int tileId,int panelId,MpController Parent) : base(Parent) {
            TileHeaderPanel = new MpTileHeaderPanel(tileId,panelId) {
                AutoSize = false,
                BackColor = Properties.Settings.Default.TileItemBgColor,
                BorderStyle = BorderStyle.None
            };

            TileHeaderCloseButtonController = new MpTileHeaderCloseButtonController(tileId,panelId,this);
            TileHeaderPanel.Controls.Add(TileHeaderCloseButtonController.CloseButton);

            TileHeaderExpandButtonController = new MpTileHeaderExpandButtonController(tileId,panelId,this);
            TileHeaderPanel.Controls.Add(TileHeaderExpandButtonController.ExpandButton);

            Link(new List<MpIView> { TileHeaderPanel });
        }
        public override void Update() {
            //tile  rect
            Rectangle tr = ((MpTilePanelController)Parent).TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width);
            //tile header height
            int tdh = (int)(Properties.Settings.Default.TileHeaderHeightRatio * tr.Height);
            TileHeaderPanel.SetBounds(tp,tp,tr.Width-(tp*2),tdh);
            TileHeaderPanel.BackColor = ((MpTilePanelController)Parent).TileTitlePanelController.TileTitlePanel.BackColor;

            TileHeaderPanel.Invalidate();

            TileHeaderCloseButtonController.Update();
            TileHeaderExpandButtonController.Update();
        }
    }
}

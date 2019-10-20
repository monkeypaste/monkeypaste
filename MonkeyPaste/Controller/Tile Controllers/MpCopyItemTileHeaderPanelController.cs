using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpCopyItemTileHeaderPanelController : MpController {
        //public MpCopyItemTileHeaderIconPanelController CopyItemTileHeaderIconPanelController { get; set; } 
        public MpTileMenuPanelController CopyItemTileHeaderMenuPanelController { get; set; }

        public MpCopyItemTileHeaderPanel CopyItemTileHeaderPanel { get; set; }

        public MpCopyItemTileHeaderPanelController(MpController parentController) : base(parentController) {
            CopyItemTileHeaderPanel = new MpCopyItemTileHeaderPanel();
            CopyItemTileHeaderMenuPanelController = new MpTileMenuPanelController(this);
        }

        public override void UpdateBounds() {
            //tile rect
            Rectangle tr = ((MpTilePanelController)ParentController).CopyItemTilePanel.Bounds;
            //header height
            int hh = (int)((float)tr.Height * (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio"));
            CopyItemTileHeaderPanel.SetBounds(0,0,tr.Width,hh);
            CopyItemTileHeaderMenuPanelController.UpdateBounds();
        }
    }
}

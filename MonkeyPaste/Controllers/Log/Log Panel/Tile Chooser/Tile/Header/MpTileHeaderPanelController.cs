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
        //public MpTileHeaderExpandButtonController TileHeaderExpandButtonController { get; set; }

        public MpTileHeaderPanel TileHeaderPanel { get; set; }

        public MpTileHeaderPanelController(int tileId,int panelId,MpController Parent) : base(Parent) {
            TileHeaderPanel = new MpTileHeaderPanel(tileId,panelId) {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.LightGray,
                Radius = Properties.Settings.Default.TileBorderRadius,
                OnlyRoundedOnTop = true
            };
            TileHeaderCloseButtonController = new MpTileHeaderCloseButtonController(tileId,panelId,this);
            TileHeaderPanel.Controls.Add(TileHeaderCloseButtonController.CloseButton);

            //TileHeaderExpandButtonController = new MpTileHeaderExpandButtonController(tileId,panelId,this);
            //TileHeaderPanel.Controls.Add(TileHeaderExpandButtonController.ExpandButton);

            Link(new List<MpIView> { TileHeaderPanel });
        }
        public override void Update() {
            //tile panel
            var tp = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel;
            //tile rect
            Rectangle tr = tp.Bounds;
            //tile title height
            int tth = (int)((float)tr.Height * Properties.Settings.Default.TileHeaderHeightRatio);
            //tile padding
            int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width) + ((MpTilePanelController)Find("MpTilePanelController")).TilePanel.EdgeWidth;
            TileHeaderPanel.SetBounds(tpd,tpd,tr.Width - tpd - tp.ShadowShift - tp.EdgeWidth,tth);

            TileHeaderCloseButtonController.Update();
            //TileHeaderExpandButtonController.Update();

            TileHeaderPanel.Invalidate();            
        }
    }
}

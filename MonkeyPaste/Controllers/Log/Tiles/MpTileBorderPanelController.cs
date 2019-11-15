using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSelectedTileBorderPanelController : MpController {
        public MpTileBorderPanel TileBorderPanel { get; set; }

        public MpSelectedTileBorderPanelController(MpController parentController,int panelId) : base(parentController) {
            TileBorderPanel = new MpTileBorderPanel(panelId) {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Color.Gray,// Properties.Settings.Default.TileSelectedColor,
                BorderColor = Properties.Settings.Default.TileSelectedColor,
                Radius = Properties.Settings.Default.TileBorderRadius
            };
            Link(new List<MpIView>() { TileBorderPanel });
        }
        public override void Update() {
            //selecred tile panel rect
            Rectangle stpr = ((MpTileChooserPanelController)Parent).SelectedTileController.TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * stpr.Height);
            //border delta size
            float r = 1.25f;
            int bds = (int)((float)tp * r) + stpr.Width;

            TileBorderPanel.Location = new Point(stpr.Location.X - (int)((float)tp*(r/2.0f)),stpr.Location.Y - (int)((float)tp* (r / 2.0f)));
            TileBorderPanel.Size = new Size(bds,bds);

            TileBorderPanel.SendToBack();
        }
    }
}

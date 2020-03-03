using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTileBorderPanelController : MpController {
        public MpTileBorderPanel TileBorderPanel { get; set; }
        public MpTilePanelController TilePanelControllerRef { get; set; }

        public MpTileBorderPanelController(MpController parentController,MpTilePanelController refTpc) : base(parentController) {
            TilePanelControllerRef = refTpc;

            TileBorderPanel = new MpTileBorderPanel() {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Properties.Settings.Default.TileSelectedColor,
                BorderColor = Properties.Settings.Default.TileSelectedColor,
                Radius = Properties.Settings.Default.TileBorderRadius,
                Visible = false
            };

            Link(new List<MpIView>() { TileBorderPanel });
        }
        public override void Update() {
            if(TilePanelControllerRef == null) {
                Console.WriteLine("Tile border error, no tile for border");
                return;
            }
            
            Color borderColor = Color.Black;

            switch(TilePanelControllerRef.TilePanelState) {
                case MpTilePanelState.None:
                case MpTilePanelState.Hidden:
                case MpTilePanelState.Unselected:
                    borderColor = Color.FromArgb(0,0,0,0);
                    break;
                case MpTilePanelState.Hover:
                    borderColor = Color.Yellow;
                    break;
                case MpTilePanelState.Selected:
                    borderColor = Color.Red;
                    break;
            }

            //selected tile panel rect
            Rectangle stpr = TilePanelControllerRef.TilePanel.Bounds;
            //tile padding
            int tp = (int)(Properties.Settings.Default.TileChooserPadHeightRatio * stpr.Height);
            //border delta size
            float r = 1.25f;
            int bds = (int)((float)tp * r) + stpr.Width;

            //TileBorderPanel.Location = new Point(stpr.Location.X - (int)((float)tp*(r/2.0f)),stpr.Location.Y - (int)((float)tp* (r / 2.0f)));
            //TileBorderPanel.Size = new Size(bds,bds);
            TileBorderPanel.SetBounds(stpr.Location.X - (int)((float)tp * (r / 2.0f)),stpr.Location.Y - (int)((float)tp * (r / 2.0f)),bds,bds);
            TileBorderPanel.BackColor = borderColor;
            TileBorderPanel.SendToBack();

            TileBorderPanel.Invalidate();
        }
    }
}

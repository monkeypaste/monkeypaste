using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileHeaderPanelController : MpController {
        public MpRoundedPanel TileHeaderPanel { get; set; }

        public static Color HeaderColor = Color.FromArgb(100, 1, 1, 1);
        public MpTileHeaderCloseButtonController TileHeaderCloseButtonController { get; set; }
        //public MpTileHeaderExpandButtonController TileHeaderExpandButtonController { get; set; }

        public delegate void ButtonClicked(object sender, EventArgs e);
        public event ButtonClicked ButtonClickedEvent;
        public bool IsMouseOver { get; set; } = false;

        public MpTileHeaderPanelController(MpController Parent) : base(Parent) {
            TileHeaderPanel = new MpRoundedPanel() {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent,
                Radius = Properties.Settings.Default.TileBorderRadius,
                OnlyRoundedOnTop = true
            };
            TileHeaderPanel.DoubleBuffered(true);
            TileHeaderCloseButtonController = new MpTileHeaderCloseButtonController(this);
            TileHeaderPanel.Controls.Add(TileHeaderCloseButtonController.CloseButton);
            TileHeaderPanel.MouseEnter += TileHeaderPanel_MouseEnter;
            TileHeaderPanel.MouseLeave += TileHeaderPanel_MouseLeave;
            //TileHeaderExpandButtonController = new MpTileHeaderExpandButtonController(tileId,panelId,this);
            //TileHeaderPanel.Controls.Add(TileHeaderExpandButtonController.ExpandButton);

            //Link(new List<MpIView> { TileHeaderPanel });
        }

        private void TileHeaderPanel_MouseLeave(object sender, EventArgs e) {
            IsMouseOver = false;
            TileHeaderPanel.BackColor = Color.Transparent;
            TileHeaderPanel.Invalidate();
        }

        private void TileHeaderPanel_MouseEnter(object sender, EventArgs e) {
            IsMouseOver = true;
            TileHeaderPanel.BackColor = HeaderColor;
            TileHeaderPanel.Invalidate();
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

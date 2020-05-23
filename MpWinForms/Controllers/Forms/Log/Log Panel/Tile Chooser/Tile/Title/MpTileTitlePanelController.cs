using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        public Panel TileTitlePanel { get; set; }

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }
        private int _copyItemId;

        public MpTileTitlePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {            
            _copyItemId = ci.CopyItemId;
            TileTitlePanel = new Panel() {
                BorderStyle = BorderStyle.None,
                BackColor = ci.ItemColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TileTitlePanel.DoubleBuffered(true);
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;       
           
            TileTitleIconPanelController = new MpTileTitleIconPanelController(ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconBox);

            TileTitleTextBoxController = new MpTileTitleTextBoxController(ci, this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabel);
        }
        
        public override void Update() {
            //tile panel
            var tp = ((MpTilePanelController)Find("MpTilePanelController")).TilePanel;
            //tile rect
            Rectangle tr = tp.Bounds;
            //tile header rect
            Rectangle thr = ((MpTilePanelController)Find("MpTilePanelController")).TileHeaderPanelController.TileHeaderPanel.Bounds;

            //tile title height
            int tth = (int)((float)tr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            //tile padding
            int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tr.Width) + ((MpTilePanelController)Find("MpTilePanelController")).TilePanel.EdgeWidth;
            TileTitlePanel.SetBounds(tpd, thr.Bottom, tr.Width - tpd - tp.ShadowShift - tp.EdgeWidth, tth);

            TileTitleIconPanelController.Update();
            TileTitleTextBoxController.Update();

            TileTitlePanel.Invalidate();
        }
        
        
    }
}
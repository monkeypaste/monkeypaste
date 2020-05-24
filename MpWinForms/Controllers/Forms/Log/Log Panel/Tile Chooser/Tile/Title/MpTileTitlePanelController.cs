using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpControlController {
        public MpTilePanel TileTitlePanel { get; set; }        

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }

        private bool _isExpanded { get; set; } = true;

        private int _copyItemId;

        public MpTileTitlePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {            
            _copyItemId = ci.CopyItemId;
            TileTitlePanel = new MpTilePanel() {
                Bounds = GetBounds(),
                BorderStyle = BorderStyle.None,
                FlatBorderColor = Color.Transparent,
                EdgeWidth = 3,
                Style = BeveledPanel.AdvancedPanel.BevelStyle.Raised,
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                StartColor = ci.ItemColor.Color,
                EndColor = ci.ItemColor.Color,
                ShadowColor = Color.Transparent,
                ShadowShift = 0,
                ShadowStyle = BeveledPanel.AdvancedPanel.ShadowMode.ForwardDiagonal,
                RoundedBottom = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                RectRadius = Properties.Settings.Default.TileBorderRadius
            };
            TileTitlePanel.DoubleBuffered(true);
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;

            
            TileTitleIconPanelController = new MpTileTitleIconPanelController(ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconBox);

            TileTitleTextBoxController = new MpTileTitleTextBoxController(ci, this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabel);
        }
        
        public override Rectangle GetBounds() {
            //tile panel controller
            var tpc = ((MpTilePanelController)Parent);
            //tile content rect
            Rectangle tcr = tpc.GetBounds();

            //tile title height
            int tth = (int)((float)tcr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            //tile content padding
           // int tpd = (int)(Properties.Settings.Default.TilePadWidthRatio * (float)tcr.Width) + TileTitlePanel.EdgeWidth;
            
            return new Rectangle(0,0, tcr.Width, tth);
        }
        public override void Update() {
            TileTitlePanel.Bounds = GetBounds();


            TileTitleIconPanelController.Update();
            TileTitleTextBoxController.Update();

            TileTitlePanel.Invalidate();
        }

    }
}
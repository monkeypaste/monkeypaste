using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitlePanelController : MpController {
        public Panel TileTitlePanel { get; set; }        

        public MpTileTitleIconPanelController TileTitleIconPanelController { get; set; }
        public MpTileTitleTextBoxController TileTitleTextBoxController { get; set; }

        private bool _isExpanded { get; set; } = true;

        private int _copyItemId;

        public MpTileTitlePanelController(MpCopyItem ci,MpController Parent) : base(Parent) {            
            _copyItemId = ci.CopyItemId;
            TileTitlePanel = new Panel() {
                Bounds = GetBounds(),
                AutoSize = false,
                BackColor = ci.ItemColor.Color,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TileTitlePanel.DoubleBuffered(true);
            
            TileTitleIconPanelController = new MpTileTitleIconPanelController(ci,this);
            TileTitlePanel.Controls.Add(TileTitleIconPanelController.TileTitleIconBox);

            TileTitleTextBoxController = new MpTileTitleTextBoxController(ci, this);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleTextBox);
            TileTitlePanel.Controls.Add(TileTitleTextBoxController.TileTitleLabel);

            DefineEvents();
        }
        public override void DefineEvents() {
            TileTitlePanel.MouseWheel += MpSingletonController.Instance.ScrollWheelListener;
        }
        public override Rectangle GetBounds() {
            //tile panel controller
            var tpc = ((MpTilePanelController)Parent);
            //tile rect
            Rectangle tpr = tpc.TilePanel.GetChildBounds();

            //tile title height
            int tth = (int)((float)tpr.Height * Properties.Settings.Default.TileTitleHeightRatio);
            tpr.Size = new Size(tpr.Width, tth);
            return tpr;
        }
        public override void Update() {
            TileTitlePanel.Bounds = GetBounds();


            TileTitleIconPanelController.Update();
            TileTitleTextBoxController.Update();

            TileTitlePanel.Invalidate();
        }

    }
}
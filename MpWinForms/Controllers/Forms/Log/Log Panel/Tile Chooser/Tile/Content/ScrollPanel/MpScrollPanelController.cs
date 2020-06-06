using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpScrollPanelController : MpController {
        public MpScrollbarPanelController VScrollbarPanelController { get; set; }
        public MpScrollbarPanelController HScrollbarPanelController { get; set; }

        public MpTileContentControlController TileContentControlController { get; set; }

        public MpPanel ScrollPanel { get; set; }

        private bool _isShiftDown = false;

        public MpScrollPanelController(MpCopyItem ci,MpController parent) : base(parent) {
            ScrollPanel = new MpPanel() {
                AutoScroll = false,
                AutoSize = false,
                BackColor = Color.Yellow,
                Bounds = GetBounds()
            };
            ScrollPanel.DoubleBuffered(true);

            TileContentControlController = new MpTileContentControlController(ci, this);
            ScrollPanel.Controls.Add(TileContentControlController.TileContentControl);

            VScrollbarPanelController = new MpScrollbarPanelController(this, TileContentControlController.TileContentControl,false);
            ScrollPanel.Controls.Add(VScrollbarPanelController.ScrollbarPanel);
            
            HScrollbarPanelController = new MpScrollbarPanelController(this, TileContentControlController.TileContentControl, true);
            ScrollPanel.Controls.Add(HScrollbarPanelController.ScrollbarPanel);

            DefineEvents();
        }

        public override void DefineEvents() {
            VScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                Point offset = TileContentControlController.Offset;
                offset.Y += -e.ScrollAmount;
                TileContentControlController.Offset = offset;
            };

            HScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                Point offset = TileContentControlController.Offset;
                offset.X += -e.ScrollAmount;
                TileContentControlController.Offset = offset;
            };

            TileContentControlController.TileContentControl.KeyDown += (s, e) => {
                if(e.Shift) {
                    _isShiftDown = true;
                }
            };
            TileContentControlController.TileContentControl.KeyUp += (s, e) => {
                _isShiftDown = false;
            };
            TileContentControlController.TileContentControl.MouseWheel += (s, e) => {
                if(_isShiftDown) {
                    HScrollbarPanelController.PerformScroll(-(int)((float)e.Delta / 100.0f));
                } else {
                    VScrollbarPanelController.PerformScroll(-(int)((float)e.Delta / 100.0f));
                }
            };
        }
        public override Rectangle GetBounds() {
            Rectangle tcpr = ((MpTileContentPanelController)Parent).GetBounds();
            return new Rectangle(0,0, tcpr.Width, tcpr.Height);
        }
        public override void Update() {
            ScrollPanel.Bounds = GetBounds();

            TileContentControlController.Update();
            VScrollbarPanelController.Update();
            HScrollbarPanelController.Update();

            VScrollbarPanelController.ScrollbarPanel.BringToFront();
            HScrollbarPanelController.ScrollbarPanel.BringToFront();

            ScrollPanel.Invalidate();
        }
        public void ShowScrollbars() {
            //scroll panel size
            Size sps = GetBounds().Size;
            //tile content control size
            Size tccs = TileContentControlController.GetBounds().Size;

            if(tccs.Height > sps.Height) {
                VScrollbarPanelController.ScrollbarPanel.Visible = true;
                VScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = true;
            } else {
                VScrollbarPanelController.ScrollbarPanel.Visible = false;
                VScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = false;
            }
            if (tccs.Width > sps.Width) {
                HScrollbarPanelController.ScrollbarPanel.Visible = true;
                HScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = true;
            }
            else {
                HScrollbarPanelController.ScrollbarPanel.Visible = false;
                HScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = false;
            }
            Update();
        }
        public void HideScrollbars() {
            VScrollbarPanelController.ScrollbarPanel.Visible = false;
            VScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = false;

            HScrollbarPanelController.ScrollbarPanel.Visible = false;
            HScrollbarPanelController.ScrollbarGripControlController.GripPanel.Visible = false;
            
            Update();
        }
    }
}

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

        public Panel ScrollPanel { get; set; }

        public MpScrollPanelController(MpCopyItem ci,MpController parent) : base(parent) {
            ScrollPanel = new Panel() {
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
        }
        public override Rectangle GetBounds() {
            //tile  rect
            int p = ((MpTilePanelController)Parent.Parent).TilePanel.EdgeWidth + ((MpTilePanelController)Parent.Parent).TilePanel.ShadowShift;
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

            ScrollPanel.Refresh();
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

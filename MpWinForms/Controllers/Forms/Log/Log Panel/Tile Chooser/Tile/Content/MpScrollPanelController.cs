using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpScrollPanelController : MpControlController {
        public MpScrollbarPanelController VScrollbarPanelController { get; set; }
        public MpScrollbarPanelController HScrollbarPanelController { get; set; }

        public MpTileContentControlController TileContentControlController { get; set; }

        public Panel ScrollPanel { get; set; }

        public MpScrollPanelController(MpCopyItem ci,MpControlController parent) : base(parent) {
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
            VScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                Point offset = TileContentControlController.Offset;
                offset.Y += e.ScrollAmount;
                TileContentControlController.Offset = offset;
            };
            HScrollbarPanelController = new MpScrollbarPanelController(this, TileContentControlController.TileContentControl, true);
            HScrollbarPanelController.ScrollContinueEvent += (s, e) => {
                Point offset = TileContentControlController.Offset;
                offset.X += -e.ScrollAmount;
                TileContentControlController.Offset = offset;
            };
            ScrollPanel.Controls.Add(HScrollbarPanelController.ScrollbarPanel);           
        }

        public override Rectangle GetBounds() {
            //    //tile panel controller
            //    var tpc = ((MpTilePanelController)Parent);
            //    //tile title height
            //    int tth = tpc.TileTitlePanelController.GetBounds().Height;
            //    //tile  rect
            //    Rectangle tr = tpc.GetBounds();
            //    //control width
            //    int cw = (int)((float)tr.Width * Properties.Settings.Default.TileItemPadWidthRatio);
            //    //adjust for scrollbar
            //    cw = (int)(cw * Properties.Settings.Default.TileItemScrollBarThicknessRatio);
            //    //itemcontrol padding
            //    int icp = (int)((tr.Width-cw)/2);
            //    //control height
            //    int ch = tr.Height - (icp * 2) - tth;
            //    return new Rectangle(0, 0, cw, ch);
            //tile content panel rect
            Rectangle tcpr = ((MpTileContentPanelController)Parent).GetBounds();
            return new Rectangle(0, 0, tcpr.Width, tcpr.Height);
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
            Size sps = ScrollPanel.Size;
            //tile content control size
            Size tccs = TileContentControlController.TileContentControl.Size;

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

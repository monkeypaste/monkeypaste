using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileContentPanelController : MpControlController {
        public MpScrollPanelController ScrollPanelController { get; set; }
        
        public Panel TileContentPanel { get; set; }
        

        private Point _lastMouseLoc = Point.Empty;
        private Point _itemControlPanelOrigin = Point.Empty;

        public MpTileContentPanelController(MpCopyItem ci,MpController Parent) : base(Parent) {            
            TileContentPanel = new Panel() {
                AutoSize = false,
                AutoScroll = false,
                Bounds = GetBounds(),
                BackColor = Color.Brown
            };
            TileContentPanel.DoubleBuffered(true);

            ScrollPanelController = new MpScrollPanelController(ci, this);
            TileContentPanel.Controls.Add(ScrollPanelController.ScrollPanel);
        }
        //pub
        public override Rectangle GetBounds() {
            //tile panel controller
            var tpc = ((MpTilePanelController)Parent);
            //tile title height
            int tth = tpc.TileTitlePanelController.GetBounds().Height;
            //tile  rect
            Rectangle tr = tpc.GetBounds();
            int p = tpc.TilePanel.EdgeWidth;
            return new Rectangle(p,tth, tr.Width-(p*2), tr.Height-tth-p);
        }
        public override void Update() {
            TileContentPanel.Bounds = GetBounds();

            ScrollPanelController.Update();
            TileContentPanel.Invalidate();
        }
        public void TraverseItem(Point ml) {
            //if(!((MpTilePanelController)Parent).IsSelected()) {
            //    return;
            //}
            //if(_itemControl.GetType().IsSubclassOf(typeof(TextBoxBase))) {
            //    //item control size
            //    Size ics = MpHelperSingleton.Instance.GetTextSize(((TextBoxBase)_itemControl).Text,_itemFont);
            //    //item panel size
            //    Size ips = ItemPanel.Size;
            //    Point p = ItemControlPanel.Location;
            //    if(ics.Width > ips.Width) {
            //        p.X = ItemControlPanel.Location.X - (int)((((float)ml.X / (float)ips.Width) * (float)(ics.Width-ips.Width)));
            //    }
            //    if(ics.Height > ips.Height) {
            //        p.Y = ItemControlPanel.Location.Y - (int)((((float)ml.Y / (float)ips.Height) * (float)(ics.Height-ips.Height)));
            //    }
            //    //constrain scrolling for left justified text
            //    p = new Point(Math.Min(-ItemPanel.Width,p.X),Math.Min(-ItemPanel.Height,p.Y));

            //    ItemControlPanel.Location = p;
            //    ItemControlPanel.Refresh();
            //    //Console.WriteLine("Traversing item: "+ControllerName+" P:"+p.ToString()+" Item Dimensions:"+ics.ToString());
            //}
            //if(_itemControlPanelOrigin == Point.Empty) {
            //    _itemControlPanelOrigin = ItemControlPanel.Location;
            //}
        }
        
    }
}

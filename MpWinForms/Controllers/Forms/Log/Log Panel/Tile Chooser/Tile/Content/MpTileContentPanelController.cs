using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileContentPanelController : MpController {
        public MpScrollPanelController ScrollPanelController { get; set; }
        
        public Panel TileContentPanel { get; set; }
       
        public MpTileContentPanelController(MpCopyItem ci,MpController Parent) : base(Parent) {            
            TileContentPanel = new Panel() {
                AutoSize = false,
                AutoScroll = false,
                Bounds = GetBounds(),
                BackColor = Color.Pink
            };
            TileContentPanel.DoubleBuffered(true);

            ScrollPanelController = new MpScrollPanelController(ci, this);
            TileContentPanel.Controls.Add(ScrollPanelController.ScrollPanel);
        }
        public override Rectangle GetBounds() {
            //tile panel controller
            var tpc = ((MpTilePanelController)Parent);
            //tile title height
            int tth = tpc.TileTitlePanelController.GetBounds().Height;
            //tile  rect
            Rectangle tr = tpc.TilePanel.GetChildBounds();
            return new Rectangle(tr.X,tr.Y+tth, tr.Width, tr.Height-tth);
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
            //    ItemControlPanel.Invalidate();
            //    //Console.WriteLine("Traversing item: "+ControllerName+" P:"+p.ToString()+" Item Dimensions:"+ics.ToString());
            //}
            //if(_itemControlPanelOrigin == Point.Empty) {
            //    _itemControlPanelOrigin = ItemControlPanel.Location;
            //}
        }
        
    }
}

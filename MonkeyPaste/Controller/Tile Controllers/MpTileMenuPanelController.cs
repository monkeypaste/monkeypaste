using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileMenuPanelController : MpController {
        public MpTileMenuPanel TileMenuPanel { get; set; }
        public List<MpTileMenuButtonController> TileMenuButtonControllerList = new List<MpTileMenuButtonController>();

        public Button b1,b2,b3,b4;
        public MpTileMenuPanelController(MpController parentController) : base(parentController) {
            TileMenuPanel = new MpTileMenuPanel() {
                Visible = false,
                BackColor = Color.Transparent
            };
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(0,"PASTE",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(1,"EDIT",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(2,"APPEND",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(3,"DELETE",this));
            
            foreach(MpTileMenuButtonController bc in TileMenuButtonControllerList) {
                TileMenuPanel.Controls.Add(bc.TileMenuButton);
            }

            UpdateBounds();            
        }

        public override void UpdateBounds() {
            //get item rect
            Rectangle ir = ((MpTilePanelController)ParentController).TileControlController.ItemControl.Bounds;
            //get icon rect
            //Rectangle ir = ((MpCopyItemTileHeaderPanelController)ParentController).CopyItemTileHeaderIconPanelController.CopyItemTileTitleIconPanel.Bounds;//((MpCopyItemTileTitlePanelController)ParentController).CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel.Bounds;
            //CopyItemTileHeaderMenuPanel.SetBounds(ir.Right,0,tr.Width - ir.Width,tr.Height);
            TileMenuPanel.BackColor = Color.Transparent;
            TileMenuPanel.ForeColor = Color.Transparent;
            TileMenuPanel.SetBounds(ir.Left,ir.Top,ir.Width,(int)((float)ir.Height * (float)MpSingletonController.Instance.GetSetting("TileMenuHeightRatio")));
            foreach(MpTileMenuButtonController bc in TileMenuButtonControllerList) {
                bc.UpdateBounds();
            }            
        }
    }
}
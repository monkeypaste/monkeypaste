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
        public int RowCount { get; set; }
        public int ColCount { get; set; }
        public Button b1,b2,b3,b4;
        public MpTileMenuPanelController(int tileId,int panelId,MpController Parent) : base(Parent) {
            RowCount = 2;
            ColCount = 2;
            TileMenuPanel = new MpTileMenuPanel(tileId,panelId) {
                Visible = false,
                //FormBorderStyle = FormBorderStyle.None,
                //TopLevel = false,
                //TopMost = false,
                //AutoScaleMode = AutoScaleMode.None,
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(50,Color.Black),
                //Opacity = 0.25
            };
            /*TileMenuButtonControllerList.Add(new MpTileMenuButtonController(0,0,0,tileId,panelId,"PASTE",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(0,1,1,tileId,panelId,"EDIT",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(1,0,2,tileId,panelId,"APPEND",this));
            TileMenuButtonControllerList.Add(new MpTileMenuButtonController(1,1,3,tileId,panelId,"DELETE",this));
            foreach(MpTileMenuButtonController bc in TileMenuButtonControllerList) {
                TileMenuPanel.Controls.Add(bc.TileMenuButton);
            }*/

            Update();

            Link(new List<MpIView> { TileMenuPanel});
        }

        public override void Update() {
            if(TileMenuPanel.Visible == false) {
                //TileMenuPanel.Hide();
               // return;
            } 
            //get item rect
            Rectangle ir = ((MpTilePanelController)Parent).TileControlController.ItemPanel.Bounds;
            //get icon rect
            //Rectangle ir = ((MpCopyItemTileHeaderPanelController)Parent).CopyItemTileHeaderIconPanelController.CopyItemTileTitleIconPanel.Bounds;//((MpCopyItemTileTitlePanelController)Parent).CopyItemTileTitleIconPanelController.CopyItemTileTitleIconPanel.Bounds;
            //CopyItemTileHeaderMenuPanel.SetBounds(ir.Right,0,tr.Width - ir.Width,tr.Height);
            //TileMenuPanel.BackColor = Color.Transparent;
            //TileMenuPanel.ForeColor = Color.Transparent;
            //TileMenuPanel.SetBounds(ir.Left,ir.Top,ir.Width,(int)((float)ir.Height * Properties.Settings.Default.TileMenuHeightRatio));
            Point tp = ((MpTilePanelController)Parent).TilePanel.PointToScreen(ir.Location);
            Point cp = ((MpTileChooserPanelController)((MpTilePanelController)Parent).Parent).TileChooserPanel.PointToScreen(tp);
            Point lp = ((MpLogFormController)((MpTileChooserPanelController)((MpTilePanelController)Parent).Parent).Parent).LogForm.PointToScreen(cp);
            TileMenuPanel.Location = ir.Location;
            TileMenuPanel.Size = ir.Size;
            //TileMenuPanel.Show();
            //TileMenuPanel.Invalidate();
            TileMenuPanel.BringToFront();
            /*foreach(MpTileMenuButtonController bc in TileMenuButtonControllerList) {
                bc.Update();
            }*/
            //TileMenuPanel.Show();
        }
    }
}
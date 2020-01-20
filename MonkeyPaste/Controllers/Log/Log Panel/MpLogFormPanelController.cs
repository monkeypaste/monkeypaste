using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogFormPanelController : MpController {
        public MpTileChooserPanelController TileChooserPanelController { get; set; }
        public MpLogMenuPanelController LogMenuPanelController { get; set; }
        public MpTreeViewPanelController TreeViewPanelController { get; set; }

        public MpLogFormPanel LogFormPanel { get; set; }
        
        public MpLogFormPanelController(MpController parentController) : base(parentController) {
            LogFormPanel = new MpLogFormPanel() {
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                MinimumSize = new Size(15,200),
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty                
            };
            LogMenuPanelController = new MpLogMenuPanelController(this);
            LogMenuPanelController.LogSubMenuPanelController.LogMenuSearchTextBoxController.SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            LogFormPanel.Controls.Add(LogMenuPanelController.LogMenuPanel);

            var allCopyItemList = MpLogFormController.Db.GetCopyItems();

            TreeViewPanelController = new MpTreeViewPanelController(this,allCopyItemList);
            LogFormPanel.Controls.Add(TreeViewPanelController.TreeViewPanel);

            TileChooserPanelController = new MpTileChooserPanelController(this,allCopyItemList);
            LogFormPanel.Controls.Add(TileChooserPanelController.TileChooserPanel);
            if(TileChooserPanelController.TileControllerList != null && TileChooserPanelController.TileControllerList.Count > 0) {
                TileChooserPanelController.SelectTile(TileChooserPanelController.TileControllerList[TileChooserPanelController.TileControllerList.Count - 1]);
                Update();
            }
            
            Link(new List<MpIView>() { LogFormPanel });
        }

        public override void Update() {
            //log form rect
            Rectangle lfr = ((MpLogFormController)Parent).LogForm.Bounds;

            int h = (int)((float)lfr.Height * Properties.Settings.Default.LogScreenHeightRatio);

            LogFormPanel.SetBounds(0,lfr.Height - h,lfr.Width,h);

            LogMenuPanelController.Update();
            TreeViewPanelController.Update();
            TileChooserPanelController.Update();

            LogFormPanel.Invalidate();
        }
        private void SearchTextBox_TextChanged(object sender,EventArgs e) {
            string searchText = LogMenuPanelController.LogSubMenuPanelController.LogMenuSearchTextBoxController.SearchTextBox.Text;
            TileChooserPanelController.FilterTiles(searchText);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpLogFormPanelController : MpPanelController {
        public MpTileChooserPanelController TileChooserPanelController { get; set; }
        public MpLogMenuPanelController LogMenuPanelController { get; set; }
        public MpTreeViewPanelController TreeViewPanelController { get; set; }
        
        public Panel LogFormPanel { get; set; }
        public int CustomHeight { get; set; } = -1;
        public int MinimumHeight { get; set; } = 50;


        public MpLogFormPanelController(MpController parentController) : base(parentController) {
            LogFormPanel = new Panel() {
                AutoSize = false,                
                BorderStyle = BorderStyle.None,
                MinimumSize = new Size(MpScreenManager.Instance.GetScreenBoundsWithMouse().Width,MinimumHeight),
                Bounds = GetBounds(),
                BackColor = Properties.Settings.Default.LogPanelBgColor,
                Margin = Padding.Empty,
                Padding = Padding.Empty                
            };
            LogFormPanel.DoubleBuffered(true);
            LogMenuPanelController = new MpLogMenuPanelController(this);
            LogMenuPanelController.LogSubMenuPanelController.LogMenuSearchTextBoxController.SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            LogFormPanel.Controls.Add(LogMenuPanelController.LogMenuPanel);
            
            TreeViewPanelController = new MpTreeViewPanelController(this);
            LogFormPanel.Controls.Add(TreeViewPanelController.TreeViewPanel);
            
            TileChooserPanelController = new MpTileChooserPanelController(this);
            LogFormPanel.Controls.Add(TileChooserPanelController.TileChooserPanel);
        }
        public override Rectangle GetBounds() {
            Rectangle lfr = ((MpLogFormController)Parent).LogForm.Bounds;

            int h = CustomHeight > MinimumHeight ? CustomHeight : (int)((float)lfr.Height * Properties.Settings.Default.LogScreenHeightRatio);

            return new Rectangle(0, lfr.Height - h, lfr.Width, h);
        }
        public override void Update() {
            //log form rect
            LogFormPanel.Bounds = GetBounds();

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

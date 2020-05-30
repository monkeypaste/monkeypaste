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
        
        public Panel LogFormPanel { get; set; }

        public int CustomHeight { get; set; } = -1;
        public int MinimumHeight { get; set; } = 50;

        public MpLogFormPanelController(MpController parentController) : base(parentController) {
            LogFormPanel = new Panel() {
                AutoSize = false,                
                BorderStyle = BorderStyle.None,
                MinimumSize = new Size(MpSingleton.Instance.ScreenManager.GetScreenWorkingAreaWithMouse().Width,MinimumHeight),
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
            var lfc = ((MpLogFormController)Find(typeof(MpLogFormController)));
            int h = CustomHeight > MinimumHeight ? CustomHeight : (int)((float)lfc.GetBounds().Height * Properties.Settings.Default.LogScreenHeightRatio);
            //taskbar height (onkly needed for initial load for some reason)
            int tbh = h == CustomHeight ? 0 : MpSingleton.Instance.ScreenManager.GetScreenBoundsWithMouse().Height - MpSingleton.Instance.ScreenManager.GetScreenWorkingAreaWithMouse().Height;
            return new Rectangle(0, lfc.GetBounds().Height - h - tbh , lfc.GetBounds().Width, h);
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTreeViewPanelController : MpController {
        public MpTreeViewController TreeViewController { get; set; }

        public MpTreeViewPanel TreeViewPanel { get; set; }

        public bool IsExpanded {
            get {
                return _isExpanded;
            }
        }
        private bool _isExpanded = false;

        public MpTreeViewPanelController(MpController parentController,List<MpCopyItem> copyItemList) : base(parentController) {
            TreeViewPanel = new MpTreeViewPanel() {
                AutoSize = false,
                BackColor = Color.Orange,
                BorderStyle = BorderStyle.None,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            TreeViewPanel.Click += TreeViewPanel_Click;
            TreeViewController = new MpTreeViewController(this,copyItemList);
            TreeViewPanel.Controls.Add(TreeViewController.TreeView);
        }

        private void TreeViewPanel_Click(object sender,EventArgs e) {
            _isExpanded = !_isExpanded;
            ((MpLogFormPanelController)Find("MpLogFormPanelController")).Update();
        }

        public override void Update() {
            //tree view panel width
            int tvpw = (int)((float)((MpLogFormPanelController)Find("MpLogFormPanelController")).LogFormPanel.Width * Properties.Settings.Default.TreeViewWidthRatio);
            if(!_isExpanded) {
                tvpw = (int)((float)tvpw * Properties.Settings.Default.TreeViewCollapsedWidthRatio);
            }
            //tree view panel height
            int tvph = ((MpLogFormPanelController)Parent).LogFormPanel.Height;
            //tree view panel y
            int tvpy = ((MpLogFormPanelController)Parent).LogMenuPanelController.LogMenuPanel.Bottom;

            TreeViewPanel.SetBounds(0,tvpy,tvpw,tvph);

            TreeViewController.Update();

            TreeViewPanel.Invalidate();
        }
    }
}

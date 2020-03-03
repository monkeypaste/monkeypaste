using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTreeViewController : MpController {
        public List<MpTreeNodeController> TreeNodeControllerList { get; set; } = new List<MpTreeNodeController>();
        public MpTreeView TreeView { get; set; }

        private MpTreeNodeController _rootTreeNodeController;

        public MpTreeViewController(MpController parentController,List<MpCopyItem> copyItemList) : base(parentController) {
            TreeView = new MpTreeView() {
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                DrawMode = TreeViewDrawMode.Normal,
                Scrollable = true,
                ShowPlusMinus = true
            };

            _rootTreeNodeController = new MpTreeNodeController(this,"TestUser");
            TreeView.Nodes.Add(_rootTreeNodeController.TreeNode);

            if(copyItemList != null) {
                foreach(MpCopyItem ci in copyItemList) {
                    if(ci.App != null) {
                        AddTreeNode(_rootTreeNodeController,Path.GetFileName(ci.App.SourcePath));
                    } else {
                        Console.WriteLine("MpTreeViewController error: cannot load copyitem without an MpApp");
                    }
                }
            }
        }
        public override void Update() {
            //tree view panel rect
            Rectangle tvpr = ((MpTreeViewPanelController)Find("MpTreeViewPanelController")).TreeViewPanel.Bounds;
            //tree view panel collapsed width
            int tvpcw = (int)((float)tvpr.Width * Properties.Settings.Default.TreeViewCollapsedWidthRatio);

            if(((MpTreeViewPanelController)Find("MpTreeViewPanelController")).IsExpanded) {
                TreeView.Visible = true;
                TreeView.SetBounds(tvpcw,0,tvpr.Width-tvpcw,tvpr.Height);
            } else {
                TreeView.Visible = false;
            }
            TreeView.Invalidate();
        }
        public void AddTreeNode(MpTreeNodeController parentNodeController,string nodeName) {
            //first make sure node doesn't already exist
            foreach(TreeNode tn in _rootTreeNodeController.TreeNode.Nodes) {
                if(tn.Text == nodeName) {
                    Console.WriteLine("MpTreeViewController: Skipping adding duplicate app, " + nodeName);
                    return;
                }
            }
            TreeView.BeginUpdate();
            
            MpTreeNodeController newTreeNode = new MpTreeNodeController(parentNodeController,nodeName);
            _rootTreeNodeController.TreeNode.Nodes.Add(newTreeNode.TreeNode);

            TreeView.EndUpdate();
        }
    }
} 
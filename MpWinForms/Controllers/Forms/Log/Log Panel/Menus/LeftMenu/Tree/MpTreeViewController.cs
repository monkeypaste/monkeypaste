using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTreeItemController : MpController {
        public TreeNode TreeNode { get; set; }

        public MpTreeItemController(MpController parentController, string nodeName) : base(parentController) {
            TreeNode = new TreeNode()
            {
                Text = nodeName
            };
        }

           public override void Update() {

        }
    }
    public class MpTreeViewController : MpController {
        public List<MpTreeItemController> TreeNodeControllerList { get; set; } = new List<MpTreeItemController>();
        public MpTreeView TreeView { get; set; }

        private MpTreeItemController _rootTreeNodeController;

        public MpTreeViewController(MpController parentController) : base(parentController) {
            TreeView = new MpTreeView() {
                BackColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                DrawMode = TreeViewDrawMode.Normal,
                Scrollable = true,
                ShowPlusMinus = true
            };

            _rootTreeNodeController = new MpTreeItemController(this,"TestUser");
            TreeView.Nodes.Add(_rootTreeNodeController.TreeNode);

            var copyItemList = MpApplication.Instance.DataModel.CopyItemList;
            if(copyItemList != null) {
                foreach(MpCopyItem ci in copyItemList) {
                    if(ci.App != null) {
                        AddTreeNode(_rootTreeNodeController,Path.GetFileName(ci.App.AppPath));
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
        public void AddTreeNode(MpTreeItemController parentNodeController,string nodeName) {
            //first make sure node doesn't already exist
            foreach(TreeNode tn in _rootTreeNodeController.TreeNode.Nodes) {
                if(tn.Text == nodeName) {
                    Console.WriteLine("MpTreeViewController: Skipping adding duplicate app, " + nodeName);
                    return;
                }
            }
            TreeView.BeginUpdate();
            
            MpTreeItemController newTreeNode = new MpTreeItemController(parentNodeController,nodeName);
            _rootTreeNodeController.TreeNode.Nodes.Add(newTreeNode.TreeNode);

            TreeView.EndUpdate();
        }
    }
} 
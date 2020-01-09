using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTreeNodeController:MpController {
        public MpTreeNode TreeNode { get; set; }

        public MpTreeNodeController(MpController parentController,string nodeName) : base(parentController) {
            TreeNode = new MpTreeNode() {
                Text = nodeName
            };
        }

        public override void Update() {
            
        }
    }
}

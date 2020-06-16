using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWinFormsApp {
    public class MpTreeNode : MpNode {
        public new MpTreeNode Parent { get; set; } = null;
        public new MpLinkedList ChildList { get; set; }
        //root next prev first child
        public MpTreeNode(MpTreeNode r = null,MpTreeNode n = null,MpTreeNode p = null,MpLinkedList cl = null) {
            Parent = r;
            Next = n;
            Prev = p;
            ChildList = cl;
        }
        public void AddChild(MpTreeNode newChild) {
            ChildList.AddNode(newChild);
        }
        
    }
}

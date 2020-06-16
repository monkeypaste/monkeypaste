using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpLinkedList {
        public MpNode FirstNode { get; set; } = null;
        public MpNode LastNode { get; set; } = null;

        public MpLinkedList(MpNode fn = null,MpNode ln = null) {
            FirstNode = fn;
            LastNode = ln;
        }
        public void AddNode(MpNode newNode) {
            if(FirstNode == null) {
                FirstNode = LastNode = newNode;
                return;
            }
            LastNode.Next = newNode;
            LastNode.Next.Prev = LastNode;
        }
        public int Count() {
            int count = 0;
            for(MpNode cn = FirstNode; cn != null; cn = cn.Next) {
                count++;
            }
            return count;
        }
    }
}

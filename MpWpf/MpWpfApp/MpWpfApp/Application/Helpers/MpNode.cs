using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpNode  {
        public MpNode Next { get; set; } = null;
        public MpNode Prev { get; set; } = null;

        public MpNode(MpNode n = null,MpNode p = null) {
            Next = n;
            Prev = p;
        }
    }
}

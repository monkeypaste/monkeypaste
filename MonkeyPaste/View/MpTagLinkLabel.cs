using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTagLinkLabel : LinkLabel,MpIView {
        public MpTagLinkLabel(int tagId) : base() {
            this.DoubleBuffered = true;
            ViewType = this.GetType().ToString();
            ViewName = ViewType+"_"+tagId;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;
        }

        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }
    }
}

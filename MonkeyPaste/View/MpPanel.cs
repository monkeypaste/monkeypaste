using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpPanel : BevelPanel.AdvancedPanel,MpIView {
        public MpPanel() : base() {
            this.DoubleBuffered = true;

            ViewType = this.GetType().ToString();
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewName = ViewType+"_"+ViewId;
            ViewData = this;
        }
        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }
    }
}

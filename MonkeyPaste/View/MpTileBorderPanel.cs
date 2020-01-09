using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTileBorderPanel:MpRoundedPanel, MpIView {
        public MpTileBorderPanel(int panelId) {
            this.DoubleBuffered = true;
            ViewType = this.GetType().ToString();
            ViewName = ViewType + panelId;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;
        }

    public string ViewType { get; set; }
    public string ViewName { get; set; }
    public int ViewId { get; set; }
    public object ViewData { get; set; }
}
}

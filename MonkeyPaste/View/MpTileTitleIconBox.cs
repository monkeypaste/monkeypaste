using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileTitleIconBox : PictureBox, MpIView {

        public MpTileTitleIconBox(int tileId,int panelId) : base() {
            this.DoubleBuffered = true;
            ViewType = this.GetType().ToString();
            ViewName = ViewType + panelId + "_" + tileId;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;
        }

        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }
    }
}

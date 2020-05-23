using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpButton : PictureBox,MpIView {
        public Image DefaultImage { get; set; }
        public Image OverImage { get; set; }
        public Image DownImage { get; set; }

        public MpButton(int tokenId = 0) : base() {
            this.DoubleBuffered = true;
            ViewType = this.GetType().ToString();
            ViewName = ViewType + tokenId;
            ViewId = MpSingletonController.Instance.Rand.Next(1, int.MaxValue);
            ViewData = this;
        }

        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }
    }
}

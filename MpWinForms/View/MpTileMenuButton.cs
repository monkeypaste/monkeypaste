using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpTileMenuButton : Button, MpIView {
        public void ButtonLeave() {
            this.OnMouseLeave(new EventArgs());
        }
        public void ButtonOver() {
            this.OnMouseHover(new EventArgs());
        }
        public void ButtonDown() {
            this.OnMouseDown(new MouseEventArgs(MouseButtons.Left,0,1,1,0));
        }
        public void ButtonUp() {
            this.OnMouseUp(new MouseEventArgs(MouseButtons.Left,0,1,1,0));
        }
        public MpTileMenuButton(int buttonId,int tileId,int panelId) : base() {
            this.DoubleBuffered = true;
            ButtonId = buttonId;
            ViewType = this.GetType().ToString();
            ViewName = ViewType + panelId + "_" + tileId+"_"+ButtonId;
            ViewId = MpSingletonController.Instance.Rand.Next(1,int.MaxValue);
            ViewData = this;
        }
        public int ButtonId { get; set; }
        public string ViewType { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public object ViewData { get; set; }
    }
}

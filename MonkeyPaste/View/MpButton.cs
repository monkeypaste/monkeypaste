using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpButton : Button {
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
    }
}

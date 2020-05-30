using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpButtonPanelController : MpController {
        public MpButton ButtonPanel { get; set; }

        public delegate void ButtonClick(object sender, EventArgs e);
        public event ButtonClick ButtonClickEvent;

        public delegate void ButtonOver(object sender, EventArgs e);
        public event ButtonOver ButtonOverEvent;

        public delegate void ButtonLeave(object sender, EventArgs e);
        public event ButtonLeave ButtonLeaveEvent;

        public MpButtonPanelController(MpController p) : base(p) {

        }

        public override Rectangle GetBounds() {
            throw new NotImplementedException();
        }

        public override void Update() {
            throw new NotImplementedException();
        }
    }
}

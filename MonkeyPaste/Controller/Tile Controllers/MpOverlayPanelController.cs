using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpOverlayPanelController : MpController {
        public MpOverlayPanelController(MpController parentController) : base(parentController) {

        }

        public override void UpdateBounds() {
            throw new NotImplementedException();
        }

        protected override void View_KeyPress(object sender,KeyPressEventArgs e) {
            base.View_KeyPress(sender,e);
        }
    }
}

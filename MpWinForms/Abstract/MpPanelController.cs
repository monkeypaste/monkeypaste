using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpPanelController : MpController {
        public MpPanelController(MpController p) : base(p) {

        }
        public abstract Rectangle GetBounds();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpTilePanel : BeveledPanel.AdvancedPanel {
        private bool _visible = true;
        public new bool Visible {
            get {
                return _visible;
            }
            set {
                _visible = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpTileBaseViewModel : MpViewModelBase {
        #region Properties
        public bool IsSwiping { get; set; } = false;
        public bool IsSwiped { get; set; } = false;
        #endregion
    }
}

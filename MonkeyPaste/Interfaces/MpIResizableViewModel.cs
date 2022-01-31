using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIResizableViewModel {
        bool IsResizing { get; set; }
        bool CanResize { get; set; }
    }
}

using System;
using System.Text;

namespace MonkeyPaste {
    public interface MpIResizableViewModel {
        bool IsResizing { get; set; }
        bool CanResize { get; set; }
    }
}

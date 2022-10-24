using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public interface MpIDndWindowPointerLocator {
        MpPoint DragPointerPosition { get; set; }
    }

    public interface MpIDndUserCancelNotifier {
        event EventHandler OnGlobalEscKeyPressed;
    }
}

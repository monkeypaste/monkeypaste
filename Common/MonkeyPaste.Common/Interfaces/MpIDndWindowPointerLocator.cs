using System;

namespace MonkeyPaste.Common {
    public interface MpIDndWindowPointerLocator {
        MpPoint DragPointerPosition { get; set; }
    }

    public interface MpIDndUserCancelNotifier {
        event EventHandler OnGlobalEscKeyPressed;
    }
}

using System;

namespace MonkeyPaste {
    public interface MpIErrorHandler {
        void HandleError(Exception ex);
    }
}

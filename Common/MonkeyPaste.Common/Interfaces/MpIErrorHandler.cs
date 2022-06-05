using System;

namespace MonkeyPaste.Common {
    public interface MpIErrorHandler {
        void HandleError(Exception ex);
    }
}

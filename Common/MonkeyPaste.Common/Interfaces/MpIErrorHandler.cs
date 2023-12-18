using System;
using System.Runtime.CompilerServices;

namespace MonkeyPaste.Common {
    public interface MpIErrorHandler {
        void HandleError(Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0);
    }
}

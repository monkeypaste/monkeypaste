using System;
using System.Runtime.CompilerServices;

namespace MonkeyPaste.Common {
    public class MpDefaultErrorHandler : MpIErrorHandler {
        private static MpDefaultErrorHandler _instance;
        public static MpDefaultErrorHandler Instance => _instance ?? (_instance = new MpDefaultErrorHandler());
        public void HandleError(Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            MpConsole.WriteTraceLine(ex.ToString(), null, MpLogLevel.Error, callerName, callerFilePath, lineNum);
        }
    }
}

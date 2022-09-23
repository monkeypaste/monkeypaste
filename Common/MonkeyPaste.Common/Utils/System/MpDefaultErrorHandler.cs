using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpDefaultErrorHandler : MpIErrorHandler {
        private static MpDefaultErrorHandler _instance;
        public static MpDefaultErrorHandler Instance => _instance ?? (_instance = new MpDefaultErrorHandler());
        public void HandleError(Exception ex) {
            MpConsole.WriteTraceLine(ex);
        }
    }
}

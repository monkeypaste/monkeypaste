using System;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static void Break(object args = null, bool silent = false, bool asException = false) {
            if (args != null) {
                MpConsole.WriteLine(args.ToString());
            }
            if (silent) {
                return;
            }
            if (asException) {
                throw new Exception(args.ToString());
            }
#if DEBUG
            if (Debugger.IsAttached) {

                Debugger.Break();
            }
#endif
        }
        public static void Assert(bool test, string msg, bool silent = false, bool failAsException = false) {
            if (test) {
                return;
            }
            Break(msg, silent, failAsException);
        }
    }
}

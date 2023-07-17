using System;
using System.Diagnostics;
using System.Threading;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static void Break(object args = null, bool silent = false) {
            if (args != null) {
                MpConsole.WriteLine(args.ToString());
            }
#if DEBUG
            if (Debugger.IsAttached) {

                if (silent) {
                    return;
                }
                Debugger.Break();
            }
            //else {
            //    throw new Exception(args?.ToString());
            //}
#endif
        }
        public static void Assert(bool test, string msg, bool silent = false) {
            if (test) {
                return;
            }
            Break(msg, silent);
        }
    }
}

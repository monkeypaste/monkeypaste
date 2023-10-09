using System;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif
        public static void Break(object args = null, bool silent = false, bool asException = false, MpLogLevel level = MpLogLevel.Debug) {
            if (args != null) {
                MpConsole.WriteLine(
                    line: args.ToString(),
                    level: level);
            }
            if (silent) {
                return;
            }
            if (asException) {
                throw new Exception(args.ToString());
            }
#if DEBUG
            if (Debugger.IsAttached) {
                MpCommonTools.Services.DebugBreakHelper.HandlePreBreak();
                Debugger.Break();
                MpCommonTools.Services.DebugBreakHelper.HandlePostBreak();
            }
#endif
        }
        public static void Assert(bool test, string msg, bool silent = false, bool failAsException = false, MpLogLevel level = MpLogLevel.Debug) {
            if (test) {
                return;
            }
            Break(
                args: msg,
                silent: silent,
                asException: failAsException,
                level: level);
        }
    }
}
